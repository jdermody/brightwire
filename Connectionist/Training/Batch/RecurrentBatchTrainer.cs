using BrightWire.Connectionist.Training.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightWire.Models;

namespace BrightWire.Connectionist.Training.Batch
{
    class RecurrentBatchTrainer : RecurrentBatchTrainerBase, INeuralNetworkRecurrentBatchTrainer
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

        public float CalculateCost(IReadOnlyList<Tuple<float[], float[]>[]> data, float[] memory, ICostFunction costFunction, IRecurrentTrainingContext context)
        {
            return Execute(data, memory, context).SelectMany(r => r).Select(r => costFunction.Calculate(r.Output, r.Target)).Average();
        }

        public IReadOnlyList<RecurrentExecutionResults[]> Execute(IReadOnlyList<Tuple<float[], float[]>[]> trainingData, float[] memory, IRecurrentTrainingContext context)
        {
            List<RecurrentExecutionResults> temp;
            var sequenceOutput = new Dictionary<int, List<RecurrentExecutionResults>>();
            var batchSize = context.TrainingContext.MiniBatchSize;

            foreach (var miniBatch in _GetMiniBatches(trainingData, false, batchSize)) {
                var garbage = new List<IMatrix>();
                context.ExecuteForward(miniBatch, memory, (k, fc) => {
                    foreach (var action in _layer) {
                        action.Execute(fc, false);
                    }
                    var memoryOutput = fc[1].AsIndexable().Rows.ToList();
                    garbage.AddRange(fc);

                    // store the output
                    if (!sequenceOutput.TryGetValue(k, out temp))
                        sequenceOutput.Add(k, temp = new List<RecurrentExecutionResults>());
                    var ret = fc[0].AsIndexable().Rows.Zip(miniBatch.Item2[k].AsIndexable().Rows, (a, e) => Tuple.Create(a, e));
                    temp.AddRange(ret.Zip(memoryOutput, (t, d) => new RecurrentExecutionResults(t.Item1, t.Item2, d)));
                });

                // cleanup
                foreach (var item in garbage)
                    item.Dispose();
                foreach (var item in miniBatch.Item1)
                    item.Dispose();
                foreach (var item in miniBatch.Item2)
                    item.Dispose();
                context.TrainingContext.EndBatch();
            }
            return sequenceOutput.OrderBy(kv => kv.Key).Select(kv => kv.Value.ToArray()).ToList();
        }

        public RecurrentExecutionResults[] ExecuteSingle(Tuple<float[], float[]>[] data, float[] memory, IRecurrentTrainingContext context, int dataIndex)
        {
            var ret = new List<RecurrentExecutionResults>();
            context.ExecuteForwardSingle(data, memory, dataIndex, (k, fc) => {
                foreach (var action in _layer)
                    action.Execute(fc, false);
                var memoryOutput = fc[1].AsIndexable().Rows.First();

                var output = fc[0].Row(0).AsIndexable();
                ret.Add(new RecurrentExecutionResults(output, _lap.Create(data[k].Item2).AsIndexable(), memoryOutput));
            });
            return ret.ToArray();
        }

        public RecurrentExecutionResults ExecuteSingleStep(float[] input, float[] memory)
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

        public float[] Train(IReadOnlyList<Tuple<float[], float[]>[]> trainingData, float[] memory, int numEpochs, IRecurrentTrainingContext context)
        {
            var trainingContext = context.TrainingContext;
            for (int i = 0; i < numEpochs; i++) {
                trainingContext.StartEpoch(trainingData.Count);
                var batchErrorList = new List<double>();
                    
                foreach (var miniBatch in _GetMiniBatches(trainingData, _stochastic, trainingContext.MiniBatchSize)) {
                    var sequenceLength = miniBatch.Item1.Length;
                    var updateStack = new Stack<Tuple<Stack<INeuralNetworkRecurrentBackpropagation>, IMatrix, IMatrix, int[], int>>();
                    var garbage = new List<IMatrix>();
                    context.ExecuteForward(miniBatch, memory, (k, fc) => {
                        var layerStack = new Stack<INeuralNetworkRecurrentBackpropagation>();
                        foreach (var action in _layer) {
                            layerStack.Push(action.Execute(fc, true));
                            trainingContext.AddToGarbage(fc[0]);
                        }
                        updateStack.Push(Tuple.Create(layerStack, miniBatch.Item2[k], fc[0], miniBatch.Item3, k));
                        garbage.Add(fc[1]);
                    });

                    // backpropagate, accumulating errors across the sequence
                    using (var updateAccumulator = new UpdateAccumulator(trainingContext)) {
                        while (updateStack.Any()) {
                            var update = updateStack.Pop();
                            var isT0 = !updateStack.Any();
                            var actionStack = update.Item1;
                            garbage.Add(update.Item3);
                            IMatrix curr;

                            // calculate error
                            var expectedOutput = update.Item2;
                            garbage.Add(curr = expectedOutput.Subtract(update.Item3));

                            // get a measure of the training error
                            if(_collectTrainingError)
                                batchErrorList.Add(curr.AsIndexable().Values.Select(v => Math.Pow(v, 2)).Average() / 2);

                            // backpropagate
                            while (actionStack.Any()) {
                                var backpropagationAction = actionStack.Pop();
                                garbage.Add(curr = backpropagationAction.Execute(curr, trainingContext, actionStack.Any() || isT0, updateAccumulator));
                            }

                            // apply any filters
                            foreach (var filter in _filter)
                                filter.AfterBackPropagation(update.Item4, update.Item5, sequenceLength, curr);

                            // adjust the initial memory against the error signal
                            if (isT0) {
                                using (var columnSums = curr.ColumnSums()) {
                                    var initialDelta = columnSums.AsIndexable();
                                    for (var j = 0; j < memory.Length; j++)
                                        memory[j] += initialDelta[j] * trainingContext.TrainingRate;
                                }
                            }
                        }
                    }

                    // cleanup
                    foreach (var item in garbage)
                        item.Dispose();
                    foreach (var item in miniBatch.Item1)
                        item.Dispose();
                    foreach (var item in miniBatch.Item2)
                        item.Dispose();
                    trainingContext.EndBatch();
                }
                trainingContext.EndRecurrentEpoch(_collectTrainingError ? batchErrorList.Average() : 0, context);
            }
            return memory;
        }
    }
}
