using BrightWire.Connectionist.Training.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightWire.Models;
using BrightWire.Models.ExecutionResults;
using BrightWire.Models.Output;

namespace BrightWire.Connectionist.Training.Batch
{
    internal class RecurrentBatchTrainer : RecurrentBatchTrainerBase, INeuralNetworkRecurrentBatchTrainer
    {
        readonly ILinearAlgebraProvider _lap;
        readonly IReadOnlyList<INeuralNetworkRecurrentLayer> _layer;
        readonly bool _collectTrainingError;

        public RecurrentBatchTrainer(ILinearAlgebraProvider lap, IReadOnlyList<INeuralNetworkRecurrentLayer> layer, bool stochastic, bool collectTrainingError) : base(lap, stochastic)
        {
            _lap = lap;
            _layer = layer;
            _collectTrainingError = collectTrainingError;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing) {
                foreach (var item in _layer)
                    item.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IReadOnlyList<INeuralNetworkRecurrentLayer> Layer { get { return _layer; } }

        public RecurrentNetwork NetworkInfo
        {
            get
            {
                return new RecurrentNetwork {
                    Layer = _layer.Select(l => l.LayerInfo).ToArray()
                };
            }

            set
            {
                for (int i = 0, len = value.Layer.Length, len2 = _layer.Count; i < len && i < len2; i++)
                    _layer[i].LayerInfo = value.Layer[i];
            }
        }

        public float CalculateCost(ISequentialTrainingDataProvider data, float[] memory, IRecurrentTrainingContext context)
        {
            return Execute(data, memory, context).SelectMany(r => r).Select(r => context.TrainingContext.ErrorMetric.Compute(r.Output, r.ExpectedOutput)).Average();
        }

        public IReadOnlyList<IRecurrentExecutionResults[]> Execute(ISequentialTrainingDataProvider trainingData, float[] memory, IRecurrentTrainingContext context)
        {
            List<IRecurrentExecutionResults> temp;
            var sequenceOutput = new Dictionary<int, List<IRecurrentExecutionResults>>();
            var batchSize = context.TrainingContext.MiniBatchSize;

            foreach (var miniBatch in _GetMiniBatches(trainingData, false, batchSize)) {
                _lap.PushLayer();
                context.ExecuteForward(miniBatch, memory, (k, fc) => {
                    foreach (var action in _layer) {
                        action.Execute(fc, false);
                    }
                    var memoryOutput = fc[1].AsIndexable().Rows.ToList();

                    // store the output
                    if (!sequenceOutput.TryGetValue(k, out temp))
                        sequenceOutput.Add(k, temp = new List<IRecurrentExecutionResults>());
                    var expectedOutput = miniBatch.GetExpectedOutput(fc, k);
                    if (expectedOutput != null) {
                        var ret = fc[0].AsIndexable().Rows.Zip(expectedOutput.AsIndexable().Rows, (a, e) => Tuple.Create(a, e));
                        temp.AddRange(ret.Zip(memoryOutput, (t, d) => new RecurrentExecutionResults(t.Item1, t.Item2, d)));
                    }else
                        temp.AddRange(fc[0].AsIndexable().Rows.Zip(memoryOutput, (t, d) => new RecurrentExecutionResults(t, null, d)));
                });

                // cleanup
                context.TrainingContext.EndBatch();
                _lap.PopLayer();
                miniBatch.Dispose();
            }
            return sequenceOutput.OrderBy(kv => kv.Key).Select(kv => kv.Value.ToArray()).ToList();
        }

        //public RecurrentExecutionResults[] ExecuteSingle(Tuple<float[], float[]>[] data, float[] memory, IRecurrentTrainingContext context, int dataIndex)
        //{
        //    var ret = new List<RecurrentExecutionResults>();
        //    context.ExecuteForwardSingle(data, memory, dataIndex, (k, fc) => {
        //        foreach (var action in _layer)
        //            action.Execute(fc, false);
        //        var memoryOutput = fc[1].AsIndexable().Rows.First();

        //        var output = fc[0].Row(0).AsIndexable();
        //        ret.Add(new RecurrentExecutionResults(output, _lap.Create(data[k].Item2).AsIndexable(), memoryOutput));
        //    });
        //    return ret.ToArray();
        //}

        public IRecurrentExecutionResults ExecuteSingleStep(float[] input, float[] memory)
        {
            var context = new List<IMatrix>();
            context.Add(null);
            context.Add(_lap.Create(memory).ToRowMatrix());
            context[0] = _lap.Create(input).ToRowMatrix();

            foreach (var action in _layer)
                action.Execute(context, false);
            var memoryOutput = context[1].AsIndexable().Rows.First();

            var output = context[0].Row(0).AsIndexable();
            return new RecurrentExecutionResults(output, null, memoryOutput);
        }

        public Stack<SequenceBackpropagationData> FeedForward(
            ISequentialMiniBatch miniBatch,
            float[] memory,
            IRecurrentTrainingContext context
        )
        {
            var updateStack = new Stack<SequenceBackpropagationData>();
            context.ExecuteForward(miniBatch, memory, (k, fc) => {
                var backProp = new SequenceBackpropagationData(miniBatch, k);
                foreach (var action in _layer)
                    backProp.Add(action.Execute(fc, true));
                backProp.Output = fc[0];
                backProp.ExpectedOutput = miniBatch.GetExpectedOutput(fc, k);
                updateStack.Push(backProp);
            });
            return updateStack;
        }

        public void Backpropagate(
            IRecurrentTrainingContext context,
            float[] memory,
            Stack<SequenceBackpropagationData> updateStack,
            Action<IMatrix> beforeBackProp, 
            Action<IMatrix> afterBackProp
        ){
            var trainingContext = context.TrainingContext;

            // backpropagate, accumulating errors across the sequence
            using (var updateAccumulator = new UpdateAccumulator(trainingContext)) {
                IMatrix curr = null;
                while (updateStack.Any()) {
                    var update = updateStack.Pop();
                    var isT0 = !updateStack.Any();
                    var prevHasNoSignal = !isT0 && updateStack.Peek().ExpectedOutput == null;
                    var actionStack = update.LayerBackProp;

                    // calculate error
                    var expectedOutput = update.ExpectedOutput;
                    if (expectedOutput != null) {
                        curr?.Dispose();
                        curr = trainingContext.ErrorMetric.CalculateDelta(update.Output, expectedOutput);
                    }
                    //else if(curr != null)
                    //    curr = curr.Multiply(actionStack.Peek().Weight);

                    // backpropagate
                    beforeBackProp?.Invoke(curr);
                    while (actionStack.Any()) {
                        var backpropagationAction = actionStack.Pop();
                        var shouldCalculateOutput = actionStack.Any() || isT0 || prevHasNoSignal;
                        curr = backpropagationAction.Execute(curr, trainingContext, shouldCalculateOutput, updateAccumulator);
                    }
                    afterBackProp?.Invoke(curr);

                    // apply any filters
                    foreach (var filter in _filter)
                        filter.AfterBackPropagation(update.MiniBatch, update.SequenceIndex, curr);

                    // adjust the initial memory against the error signal
                    if (isT0 && curr != null) {
                        using (var columnSums = curr.ColumnSums()) {
                            var initialDelta = columnSums.AsIndexable();
                            for (var j = 0; j < memory.Length; j++)
                                memory[j] += initialDelta[j] * trainingContext.TrainingRate;
                        }
                    }
                }
            }
        }

        public void TrainOnMiniBatch(ISequentialMiniBatch miniBatch, float[] memory, IRecurrentTrainingContext context, Action<IMatrix> beforeBackProp, Action<IMatrix> afterBackProp)
        {
            var trainingContext = context.TrainingContext;

            _lap.PushLayer();
            var updateStack = FeedForward(miniBatch, memory, context);
            Backpropagate(context, memory, updateStack, beforeBackProp, afterBackProp);

            // cleanup
            trainingContext.EndBatch();
            _lap.PopLayer();
        }

        public float[] Train(ISequentialTrainingDataProvider trainingData, float[] memory, int numEpochs, IRecurrentTrainingContext context)
        {
            var trainingContext = context.TrainingContext;
            for (int i = 0; i < numEpochs && context.TrainingContext.ShouldContinue; i++) {
                trainingContext.StartEpoch(trainingData.Count);
                var batchErrorList = new List<double>();
                    
                foreach (var miniBatch in _GetMiniBatches(trainingData, _stochastic, trainingContext.MiniBatchSize)) {
                    TrainOnMiniBatch(miniBatch, memory, context, curr => {
                        if (_collectTrainingError) // get a measure of the training error
                            batchErrorList.Add(curr.AsIndexable().Values.Select(v => Math.Pow(v, 2)).Average() / 2);
                    }, null);
                    miniBatch.Dispose();
                }
                trainingContext.EndRecurrentEpoch(_collectTrainingError ? batchErrorList.Average() : 0, context);
            }
            return memory;
        }
    }
}
