using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BrightWire.Connectionist.Training.Helper
{
    internal class RecurrentContext : IRecurrentTrainingContext
    {
        readonly ILinearAlgebraProvider _lap;
        readonly ITrainingContext _trainingContext;
        readonly List<INeuralNetworkRecurrentTrainerFilter> _filter = new List<INeuralNetworkRecurrentTrainerFilter>();

        public RecurrentContext(ILinearAlgebraProvider lap, ITrainingContext trainingContext)
        {
            _lap = lap;
            _trainingContext = trainingContext;
        }

        public ITrainingContext TrainingContext { get { return _trainingContext; } }

        public void AddFilter(INeuralNetworkRecurrentTrainerFilter filter)
        {
            _filter.Add(filter);
        }

        IMatrix _CreateMemory(float[] data, int columns)
        {
            return _lap.Create(columns, data.Length, (x, y) => data[y]);
        }

        public void ExecuteForward(ISequentialMiniBatch miniBatch, float[] memory, Action<int, List<IMatrix>> onT)
        {
            var sequenceLength = miniBatch.SequenceLength;
            var context = new List<IMatrix>();

            // initialise context with initial memory and space for the input at position 0
            context.Add(null);
            context.Add(_CreateMemory(memory, miniBatch.BatchSize));

            for (var k = 0; k < sequenceLength; k++) {
                // push the sequence into context
                context[0] = miniBatch.Input[k];

                // apply any filters
                foreach (var filter in _filter)
                    filter.BeforeFeedForward(miniBatch, k, context);

                onT(k, context);
            }
        }

        //public void ExecuteForwardSingle(Tuple<float[], float[]>[] data, float[] memory, int dataIndex, Action<int, List<IMatrix>> onT)
        //{
        //    var context = new List<IMatrix>();
        //    context.Add(null);
        //    context.Add(_lap.Create(memory).ToRowMatrix());

        //    var dataIndexArray = new[] { dataIndex };
        //    for (var k = 0; k < data.Length; k++) {
        //        // push the sequence into context
        //        context[0] = _lap.Create(data[k].Item1).ToRowMatrix();

        //        // apply any filters
        //        foreach (var filter in _filter)
        //            filter.BeforeFeedForward(dataIndexArray, k, data.Length, context);

        //        onT(k, context);
        //    }
        //}

        void _Add(Dictionary<Tuple<int, bool>, List<INeuralNetworkRecurrentBackpropagation>> table, Tuple<int, bool> key, INeuralNetworkRecurrentBackpropagation bp)
        {
            List<INeuralNetworkRecurrentBackpropagation> temp;
            if (!table.TryGetValue(key, out temp))
                table.Add(key, temp = new List<INeuralNetworkRecurrentBackpropagation>());
            temp.Add(bp);
        }

        public void ExecuteBidirectional(
            ISequentialMiniBatch miniBatch, 
            IReadOnlyList<INeuralNetworkBidirectionalLayer> layers, 
            float[] memoryForward,
            float[] memoryBackward,
            int padding,
            Stack<Tuple<Stack<Tuple<INeuralNetworkRecurrentBackpropagation, INeuralNetworkRecurrentBackpropagation>>, IMatrix, IMatrix, ISequentialMiniBatch, int>> updateStack,
            Action<List<IIndexableVector[]>, List<IMatrix>> onFinished
        ){
            IMatrix temp;
            var layerStackTable = new Dictionary<Tuple<int, bool>, List<INeuralNetworkRecurrentBackpropagation>>();
            var memoryOutput = new Dictionary<Tuple<int, bool>, IMatrix>();
            var garbage = new List<IMatrix>();

            // init
            var sequenceLength = miniBatch.SequenceLength;
            var batchSize = miniBatch.BatchSize;
            var shouldBackpropagate = updateStack != null;
            var lastRecurrentLayer = layers.Where(l => l.Forward.IsRecurrent).Last();

            // create the forward context
            var forwardContext = new List<IMatrix>();
            forwardContext.Add(null);
            forwardContext.Add(_CreateMemory(memoryForward, batchSize));

            // create the backward context
            var backwardContext = new List<IMatrix>();
            backwardContext.Add(null);
            backwardContext.Add(_CreateMemory(memoryBackward, batchSize));

            // load the first inputs
            var input = new Dictionary<Tuple<int, bool>, IMatrix>();
            for (var k = 0; k < sequenceLength; k++) {
                input.Add(Tuple.Create(k, true), miniBatch.Input[k]);
                input.Add(Tuple.Create(k, false), miniBatch.Input[k]);
            }

            // execute the bidirectional layers
            foreach (var layer in layers.Where(l => l.Backward != null && l.Forward != null)) {
                var layerOutput = new Dictionary<Tuple<int, bool>, IMatrix>();
                for (var k = 0; k < sequenceLength; k++) {
                    var bK = sequenceLength - k - 1;
                    var forwardKey = Tuple.Create(k, true);
                    var backwardKey = Tuple.Create(bK, false);

                    // load the input into the context
                    forwardContext[0] = input[forwardKey];
                    backwardContext[0] = input[backwardKey];

                    // restore the memory from the previous layer at this position
                    if (memoryOutput.TryGetValue(forwardKey, out temp))
                        forwardContext[1] = temp;
                    if (memoryOutput.TryGetValue(backwardKey, out temp))
                        backwardContext[1] = temp;

                    // apply any filters
                    foreach (var filter in _filter) {
                        filter.BeforeFeedForward(miniBatch, k, forwardContext);
                        filter.BeforeFeedForward(miniBatch, bK, backwardContext);
                    }

                    // execute the layers
                    var forward = layer.Forward.Execute(forwardContext, shouldBackpropagate);
                    var backward = layer.Backward.Execute(backwardContext, shouldBackpropagate);
                    if (shouldBackpropagate) {
                        _Add(layerStackTable, forwardKey, forward);
                        _Add(layerStackTable, backwardKey, backward);
                    }

                    // store memory and outputs
                    layerOutput[forwardKey] = forwardContext[0];
                    layerOutput[backwardKey] = backwardContext[0];
                    memoryOutput[forwardKey] = forwardContext[1];
                    memoryOutput[backwardKey] = backwardContext[1];
                }

                // create the next inputs
                input = layerOutput;
            }

            // merge the bidirectional layer output
            var input2 = new List<IMatrix>();
            var memoryOutput2 = new Dictionary<int, IMatrix>();
            for (var k = 0; k < sequenceLength; k++) {
                var forwardKey = Tuple.Create(k, true);
                if (k + padding < sequenceLength) {
                    var backwardKey = Tuple.Create(k + padding, false);

                    var forwardOutput = input[forwardKey];
                    var backwardOutput = input[backwardKey];
                    input2.Add(forwardOutput.ConcatRows(backwardOutput));

                    var forwardMemory = memoryOutput[forwardKey];
                    var backwardMemory = memoryOutput[backwardKey];
                    garbage.Add(memoryOutput2[k] = forwardMemory.ConcatRows(backwardMemory));
                }else {
                    var forwardOutput = input[forwardKey];
                    var forwardMemory = memoryOutput[forwardKey];
                    input2.Add(forwardOutput.ConcatRows(_lap.Create(forwardOutput.RowCount, forwardOutput.ColumnCount, 0f)));
                    garbage.Add(memoryOutput2[k] = forwardMemory.ConcatRows(_lap.Create(forwardMemory.RowCount, forwardMemory.ColumnCount, 0f)));
                }
            }

            // execute the unidirectional layers
            foreach (var layer in layers.Where(l => l.Backward == null)) {
                var nextInput = new List<IMatrix>();
                for (var k = 0; k < sequenceLength; k++) {
                    forwardContext[0] = input2[k];
                    if (memoryOutput2.TryGetValue(k, out temp))
                        forwardContext[1] = temp;

                    // apply any filters
                    foreach (var filter in _filter)
                        filter.BeforeFeedForward(miniBatch, k, forwardContext);

                    var forward = layer.Forward.Execute(forwardContext, shouldBackpropagate);
                    if (shouldBackpropagate)
                        _Add(layerStackTable, Tuple.Create(k, true), forward);
                    garbage.Add(memoryOutput2[k] = forwardContext[1]);
                    nextInput.Add(forwardContext[0]);
                }
                input2 = nextInput;
            }

            // load the update stack
            if (updateStack != null) {
                for (var k = sequenceLength - 1; k >= 0; k--) {
                    var forward = layerStackTable[Tuple.Create(k, true)];
                    var backward = layerStackTable[Tuple.Create(k, false)];
                    var stack = new Stack<Tuple<INeuralNetworkRecurrentBackpropagation, INeuralNetworkRecurrentBackpropagation>>();
                    for(var i = 0; i < Math.Max(forward.Count, backward.Count); i++)
                        stack.Push(Tuple.Create(i < forward.Count ? forward[i] : null, i < backward.Count ? backward[i] : null));
                    updateStack.Push(Tuple.Create(stack, miniBatch.GetExpectedOutput(input2, k), input2[k], miniBatch, k));
                }
            }

            // notify finished
            onFinished?.Invoke(memoryOutput2.OrderBy(k => k.Key).Select(kv => kv.Value.AsIndexable().Rows.ToArray()).ToList(), input2);

            // cleanup
            foreach (var item in garbage)
                item.Dispose();
        }
    }
}
