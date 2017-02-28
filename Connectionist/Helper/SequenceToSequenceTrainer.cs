using BrightWire.Models.Input;
using System;
using System.Collections.Generic;
using System.Text;
using BrightWire.Models.Output;
using System.Linq;
using BrightWire.Models.ExecutionResults;
using BrightWire.Connectionist.Training.Helper;

namespace BrightWire.Connectionist.Helper
{
    public class SequenceToSequenceTrainer
    {
        public class TrainingSet
        {
            readonly SequenceInfo[] _sequenceLength;
            readonly Dictionary<int, List<Tuple<VectorSequence, int>>> _inputData;
            readonly int _inputSize, _outputSize;

            public TrainingSet(IEnumerable<Tuple<VectorSequence, int>> data)
            {
                _inputData = data
                    .GroupBy(s => s.Item1.Sequence.Length)
                    .OrderBy(g => g.Key)
                    .ToDictionary(g => g.Key, g => g.ToList())
                ;
                _sequenceLength = _inputData.Select(s => new SequenceInfo(s.Key, s.Value.Count)).ToArray();

                // find the dimensions of the input
                var firstItem = data.First().Item1.Sequence.First();
                _inputSize = firstItem.Data.Length;
            }

            public SequenceInfo[] SequenceLength { get { return _sequenceLength; } }
            public IReadOnlyDictionary<int, List<Tuple<VectorSequence, int>>> InputData { get { return _inputData; } }
            public int InputSize { get { return _inputSize; } }

            public ISequentialMiniBatch GetTrainingData(ILinearAlgebraProvider lap, int sequenceLength, IReadOnlyList<int> rows)
            {
                var input = new IMatrix[sequenceLength];
                var output = new IMatrix[sequenceLength];
                var dataGroup = _inputData[sequenceLength];
                for (var k = 0; k < sequenceLength; k++) {
                    input[k] = lap.Create(rows.Count, _inputSize, (x, y) => dataGroup[rows[x]].Item1.Sequence[k].Data[y]);
                }
                return new SequentialMiniBatch(input, null, rows.Select(r => dataGroup[r].Item2).ToArray());
            }
        }

        readonly ILinearAlgebraProvider _lap;
        readonly IReadOnlyList<SeqeuenceToSequenceTrainingExample> _data;
        readonly TrainingSet _encoderData, _decoderData;
        readonly IReadOnlyList<INeuralNetworkRecurrentLayer> _encoder;
        readonly IReadOnlyList<INeuralNetworkRecurrentLayer> _decoder;
        readonly INeuralNetworkLayerTrainer _classifier;
        readonly bool _stochastic, _collectTrainingError;
        
        public SequenceToSequenceTrainer(
            ILinearAlgebraProvider lap, 
            IReadOnlyList<SeqeuenceToSequenceTrainingExample> data,
            IReadOnlyList<INeuralNetworkRecurrentLayer> encoder,
            IReadOnlyList<INeuralNetworkRecurrentLayer> decoder,
            INeuralNetworkLayerTrainer classifier,
            bool stochastic = true,
            bool collectTrainingError = true
        )
        {
            _lap = lap;
            _data = data;
            _stochastic = stochastic;
            _encoder = encoder;
            _decoder = decoder;
            _collectTrainingError = collectTrainingError;
            _classifier = classifier;

            _encoderData = new TrainingSet(data.Select((a, ind) => Tuple.Create(a.Input, ind)));
            _decoderData = new TrainingSet(data.Select((a, ind) => Tuple.Create(a.ExpectedOutput, ind)));
        }

        public void Train(float[] encoderMemory, float[] decoderMemory, int numEpochs, IRecurrentTrainingContext context)
        {
            var trainingContext = context.TrainingContext;
            for (int i = 0; i < numEpochs && context.TrainingContext.ShouldContinue; i++) {
                trainingContext.StartEpoch(_data.Count);
                var batchErrorList = new List<double>();

                foreach (var miniBatch in _GetMiniBatches(_stochastic, trainingContext.MiniBatchSize)) {
                    _TrainOnMiniBatch(miniBatch, encoderMemory, decoderMemory, context, curr => {
                        if (_collectTrainingError) // get a measure of the training error
                            batchErrorList.Add(curr.AsIndexable().Values.Select(v => Math.Pow(v, 2)).Average() / 2);
                    }, null);
                    miniBatch.Dispose();
                }
                trainingContext.EndRecurrentEpoch(_collectTrainingError ? batchErrorList.Average() : 0, context);
            }
        }

        void _TrainOnMiniBatch(ISequentialMiniBatch miniBatch, float[] encoderMemory, float[] decoderMemory, IRecurrentTrainingContext context, Action<IMatrix> beforeBackProp, Action<IMatrix> afterBackProp)
        {
            var trainingContext = context.TrainingContext;
            var errorMetric = trainingContext.ErrorMetric;
            var outputTable = _decoderData.InputData[miniBatch.SequenceLength+2];

            // encode the input
            _lap.PushLayer();
            var sequenceLength = miniBatch.SequenceLength;
            var encoderUpdateStack = new Stack<Tuple<Stack<INeuralNetworkRecurrentBackpropagation>, IMatrix, IMatrix>>();
            context.ExecuteForward(miniBatch, encoderMemory, (k, fc) => {
                var layerStack = new Stack<INeuralNetworkRecurrentBackpropagation>();
                foreach (var action in _encoder)
                    layerStack.Push(action.Execute(fc, true));
                encoderUpdateStack.Push(Tuple.Create(layerStack, fc[0], fc[1]));
            });
            var encoderOutput = encoderUpdateStack.Peek().Item2;
            var encoderOutputMemory = encoderUpdateStack.Peek().Item3;

            // initialise the decoder context
            var decoderContext = new List<IMatrix>();
            decoderContext.Add(encoderOutput);
            decoderContext.Add(context.CreateMemory(decoderMemory, trainingContext.MiniBatchSize));
            //decoderContext.Add(encoderOutputMemory);

            // decoder feed forward
            var decoderUpdateStack = new Stack<Tuple<Stack<INeuralNetworkRecurrentBackpropagation>, IMatrix, IMatrix>>();
            for (var i = 0; i < miniBatch.SequenceLength; i++) {
                var layerUpdate = new Stack<INeuralNetworkRecurrentBackpropagation>();
                foreach (var layer in _decoder)
                    layerUpdate.Push(layer.Execute(decoderContext, true));

                // classifier feed forward
                var output = _classifier.FeedForward(decoderContext[0], true);
                var expectedOutput = miniBatch.CurrentRows.Select(r => outputTable[r]).Select(d => d.Item1.Sequence[i].Data).ToList();
                var batchExpectation = _lap.Create(expectedOutput.Count, expectedOutput[0].Length, (j, k) => expectedOutput[j][k]);
                decoderUpdateStack.Push(Tuple.Create(layerUpdate, batchExpectation, output));
            }

            // decoder back propagation
            using (var updateAccumulator = new UpdateAccumulator(trainingContext)) {
                IMatrix curr = null;
                while (decoderUpdateStack.Any()) {
                    var update = decoderUpdateStack.Pop();
                    var isT0 = !decoderUpdateStack.Any();
                    var actionStack = update.Item1;

                    // calculate error
                    var expectedOutput = update.Item2;
                    if (expectedOutput != null)
                        curr = trainingContext.ErrorMetric.CalculateDelta(update.Item3, expectedOutput);
                    beforeBackProp?.Invoke(curr);

                    // backpropagate
                    while (actionStack.Any()) {
                        var backpropagationAction = actionStack.Pop();
                        curr = backpropagationAction.Execute(curr, trainingContext, true, updateAccumulator);
                    }
                    afterBackProp?.Invoke(curr);
                }

                if (curr != null) {
                    // adjust the decoder's initial memory against the error signal
                    using (var columnSums = curr.ColumnSums()) {
                        var initialDelta = columnSums.AsIndexable();
                        for (var j = 0; j < decoderMemory.Length; j++)
                            decoderMemory[j] += initialDelta[j] * trainingContext.TrainingRate;
                    }
                }

                // backpropagate the encoder
                while (encoderUpdateStack.Any()) {
                    var update = encoderUpdateStack.Pop();
                    var isT0 = !encoderUpdateStack.Any();
                    var actionStack = update.Item1;
                    var encoderOutput2 = update.Item2;

                    //var lastUpdate = updateAccumulator.Updates.Last();
                    //curr = curr.TransposeAndMultiply(lastUpdate.Item3);

                    // backpropagate
                    while (actionStack.Any()) {
                        var backpropagationAction = actionStack.Pop();
                        curr = backpropagationAction.Execute(curr, trainingContext, true, updateAccumulator);
                    }
                }

                // adjust the encoder's initial memory against the error signal
                using (var columnSums = curr.ColumnSums()) {
                    var initialDelta = columnSums.AsIndexable();
                    for (var j = 0; j < encoderMemory.Length; j++)
                        encoderMemory[j] += initialDelta[j] * trainingContext.TrainingRate;
                }
            }

            // cleanup
            trainingContext.EndBatch();
            _lap.PopLayer();
        }

        protected IEnumerable<ISequentialMiniBatch> _GetMiniBatches(bool shuffle, int batchSize)
        {
            var sequences = shuffle ? _encoderData.SequenceLength.Shuffle() : _encoderData.SequenceLength;
            foreach (var item in sequences) {
                var range = Enumerable.Range(0, item.SampleCount);
                var items = shuffle ? range.Shuffle().ToList() : range.ToList();
                for (var i = 0; i < items.Count; i += batchSize) {
                    var batch = items.Skip(i).Take(batchSize).ToList();
                    yield return _encoderData.GetTrainingData(_lap, item.SequenceLength, batch);
                }
            }
        }
    }
}
