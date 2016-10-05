using BrightWire.Connectionist.Training.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightWire.Models;
using BrightWire.Models.ExecutionResults;

namespace BrightWire.Connectionist.Training.Batch
{
    internal class RecurrentBatchTrainer : RecurrentBatchTrainerBase, INeuralNetworkRecurrentBatchTrainer
    {
        readonly IReadOnlyList<INeuralNetworkRecurrentLayer> _layer;
        readonly bool _collectTrainingError;

        public RecurrentBatchTrainer(ILinearAlgebraProvider lap, IReadOnlyList<INeuralNetworkRecurrentLayer> layer, bool stochastic, bool collectTrainingError) : base(lap, stochastic)
        {
            _layer = layer;
            _collectTrainingError = collectTrainingError;
        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
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
                    var ret = fc[0].AsIndexable().Rows.Zip(miniBatch.GetExpectedOutput(fc, k).AsIndexable().Rows, (a, e) => Tuple.Create(a, e));
                    temp.AddRange(ret.Zip(memoryOutput, (t, d) => new RecurrentExecutionResults(t.Item1, t.Item2, d)));
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

        public void TrainOnMiniBatch(ISequentialMiniBatch miniBatch, float[] memory, IRecurrentTrainingContext context, Action<IMatrix> beforeBackProp, Action<IMatrix> afterBackProp)
        {
            var trainingContext = context.TrainingContext;

            _lap.PushLayer();
            var sequenceLength = miniBatch.SequenceLength;
            var updateStack = new Stack<Tuple<Stack<INeuralNetworkRecurrentBackpropagation>, IMatrix, IMatrix, ISequentialMiniBatch, int>>();
            context.ExecuteForward(miniBatch, memory, (k, fc) => {
                var layerStack = new Stack<INeuralNetworkRecurrentBackpropagation>();
                foreach (var action in _layer)
                    layerStack.Push(action.Execute(fc, true));
                updateStack.Push(Tuple.Create(layerStack, miniBatch.GetExpectedOutput(fc, k), fc[0], miniBatch, k));
            });

            // backpropagate, accumulating errors across the sequence
            using (var updateAccumulator = new UpdateAccumulator(trainingContext)) {
                IMatrix curr = null;
                while (updateStack.Any()) {
                    var update = updateStack.Pop();
                    var isT0 = !updateStack.Any();
                    var actionStack = update.Item1;

                    // calculate error
                    var expectedOutput = update.Item2;
                    if (expectedOutput != null)
                        curr = trainingContext.ErrorMetric.CalculateDelta(update.Item3, expectedOutput);

                    // backpropagate
                    beforeBackProp?.Invoke(curr);
                    while (actionStack.Any()) {
                        var backpropagationAction = actionStack.Pop();
                        var shouldCalculateOutput = actionStack.Any() || isT0;
                        curr = backpropagationAction.Execute(curr, trainingContext, true, updateAccumulator);
                    }
                    afterBackProp?.Invoke(curr);

                    // apply any filters
                    foreach (var filter in _filter)
                        filter.AfterBackPropagation(update.Item4, update.Item5, curr);
                }

                // adjust the initial memory against the error signal
                if (curr != null) {
                    using (var columnSums = curr.ColumnSums()) {
                        var initialDelta = columnSums.AsIndexable();
                        for (var j = 0; j < memory.Length; j++)
                            memory[j] += initialDelta[j] * trainingContext.TrainingRate;
                    }
                }
            }

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
