using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BrightWire.Connectionist.Training.Layer.Convolutional;
using BrightWire.Helper;
using BrightWire.Models;

namespace BrightWire.Connectionist.Helper
{
    public class ConvolutionalTrainingDataProvider : ITrainingDataProvider, ICanBackpropagate
    {
        readonly ILinearAlgebraProvider _lap;
        readonly IReadOnlyList<Tuple<I3DTensor, float[]>> _data;
        readonly int _inputSize, _outputSize;
        readonly IReadOnlyList<IConvolutionalLayer> _layer;
        readonly List<Stack<IConvolutionalLayerBackpropagation>> _backpropagation = new List<Stack<IConvolutionalLayerBackpropagation>>();
        readonly ConvolutionDescriptor _descriptor;
        readonly bool _isTraining;

        public ConvolutionalTrainingDataProvider(ILinearAlgebraProvider lap, ConvolutionDescriptor descriptor, IReadOnlyList<Tuple<I3DTensor, float[]>> data, IReadOnlyList<IConvolutionalLayer> layer, bool isTraining)
        {
            _lap = lap;
            _data = data;
            _descriptor = descriptor;
            _layer = layer;
            _isTraining = isTraining;
            var first = data.First();
            var firstTensor = first.Item1;
            var extent = descriptor.CalculateExtent(firstTensor.ColumnCount, firstTensor.RowCount);

            _inputSize = extent.Item1 * extent.Item2 * firstTensor.Depth * descriptor.FilterDepth;
            _outputSize = first.Item2.Length;
        }

        public int Count
        {
            get
            {
                return _data.Count;
            }
        }

        public int InputSize
        {
            get
            {
                return _inputSize;
            }
        }

        public int OutputSize
        {
            get
            {
                return _outputSize;
            }
        }


        public IMiniBatch GetTrainingData(IReadOnlyList<int> rows)
        {
            _backpropagation.Clear();

            var rowList = new List<IVector>();
            var outputList = new List<float[]>();
            foreach (var item in rows) {
                var data = _data[item];
                var tensor = data.Item1;
                var backpropagation = _isTraining ? new Stack<IConvolutionalLayerBackpropagation>() : null;
                for(int i = 0, len = _layer.Count; i < len-1; i++)
                    tensor = _layer[i].ExecuteToTensor(tensor, backpropagation);
                rowList.Add(_layer.Last().ExecuteToVector(tensor, backpropagation));
                if(backpropagation != null)
                    _backpropagation.Add(backpropagation);
                outputList.Add(data.Item2);
            }

            var input = _lap.Create(rowList);
            foreach (var item in rowList)
                item.Dispose();

            var output = _lap.Create(rows.Count, _outputSize, (x, y) => outputList[x][y]);
            return new MiniBatch(input, output);
        }

        public void StartEpoch()
        {
            _backpropagation.Clear();
        }

        public IMatrix Backpropagate(IMatrix errorSignal, ITrainingContext context, bool calculateOutput, INeuralNetworkUpdateAccumulator updates)
        {
            if (_isTraining) {
                for (var i = 0; i < errorSignal.RowCount; i++) {
                    using (var row = errorSignal.Row(i)) {
                        var backpropagationStack = _backpropagation[i];
                        var isFirst = true;
                        IMatrix lastError = null;
                        while (backpropagationStack.Any()) {
                            var backpropagation = backpropagationStack.Pop();
                            if (isFirst) {
                                using (var rowError = row.ConvertInPlaceToMatrix(backpropagation.RowCount, backpropagation.ColumnCount))
                                    lastError = backpropagation.Execute(rowError, context, backpropagationStack.Any(), updates);
                                isFirst = false;
                            }
                            else {
                                var nextError = backpropagation.Execute(lastError, context, backpropagationStack.Any(), updates);
                                lastError.Dispose();
                                lastError = nextError;
                            }
                        }
                    }
                }
            }
            return null;
        }

        public ConvolutionalNetwork GetCurrentNetwork(INeuralNetworkTrainer trainer)
        {
            return new ConvolutionalNetwork {
                ConvolutionalLayer = _layer.Select(l => l.Layer).ToArray(),
                FeedForward = new FeedForwardNetwork {
                    Layer = trainer.Layer.Select(l => l.LayerUpdater.Layer.LayerInfo).ToArray()
                }
            };
        }
    }
}
