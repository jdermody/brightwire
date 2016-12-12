using BrightWire.Models.Convolutional;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BrightWire.Connectionist.Training.Layer.Convolutional;
using BrightWire.Helper;

namespace BrightWire.Connectionist.Helper
{
    public class ConvolutionalTrainingDataProvider : ITrainingDataProvider, ICanBackpropagate
    {
        readonly ILinearAlgebraProvider _lap;
        readonly IReadOnlyList<Volume> _data;
        readonly List<IMatrix> _matrixList;
        readonly int _inputSize, _outputSize;
        readonly IReadOnlyList<IConvolutionalLayer> _layer;
        readonly List<Stack<IConvolutionalLayerBackpropagation>> _backpropagation = new List<Stack<IConvolutionalLayerBackpropagation>>();
        readonly ConvolutionDescriptor _descriptor;
        readonly bool _isTraining;

        public ConvolutionalTrainingDataProvider(ILinearAlgebraProvider lap, ConvolutionDescriptor descriptor, IReadOnlyList<Volume> data, IReadOnlyList<IConvolutionalLayer> layer, bool isTraining)
        {
            _lap = lap;
            _data = data;
            _descriptor = descriptor;
            _matrixList = data
                .Select(d => descriptor.Padding > 0 ? d.AddPadding(descriptor.Padding) : d)
                .Select(d => d.Im2Col(lap, descriptor))
                .ToList()
            ;
            _layer = layer;
            _isTraining = isTraining;
            var first = _matrixList.First();
            _inputSize = first.RowCount * descriptor.FilterDepth;
            _outputSize = _data.First().ExpectedOutput.Length;
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

        IIndexableVector _ExecuteLayer(IMatrix input, int layerIndex, Stack<IConvolutionalLayerBackpropagation> backpropagation)
        {
            var layer = _layer[layerIndex];
            var output = layer.Execute(input, backpropagation);

            if(layerIndex < _layer.Count - 1) {
                var convolutionData = new Volume {
                    Layers = new Volume.Layer[output.ColumnCount]
                };
                for (int i = 0, len = output.ColumnCount; i < len; i++) {
                    var column = output.Column(i);
                    convolutionData.Layers[i] = new Volume.Layer {
                        Data = column.AsIndexable().ToArray(),
                        Height = (int)Math.Sqrt(column.Count),
                        Width = (int)Math.Sqrt(column.Count)
                    };
                }
                var output2 = convolutionData.Im2Col(_lap, _descriptor);
                return _ExecuteLayer(output2, layerIndex + 1, backpropagation);
            }
            else
                return output.ConvertInPlaceToVector().AsIndexable();
        }

        public IMiniBatch GetTrainingData(IReadOnlyList<int> rows)
        {
            var rowList = new List<IIndexableVector>();
            foreach (var item in rows) {
                var matrix = _matrixList[item];
                var backpropagation = _isTraining ? new Stack<IConvolutionalLayerBackpropagation>() : null;
                rowList.Add(_ExecuteLayer(matrix, 0, backpropagation));
                if(backpropagation != null)
                    _backpropagation.Add(backpropagation);
            }

            var input = _lap.Create(rowList);
            var output = _lap.Create(rows.Count, _outputSize, (x, y) => _data[rows[x]].ExpectedOutput[y]);
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
                    var row = errorSignal.Row(i).AsIndexable();
                    //row = row.Rotate180(row.Count / _descriptor.FilterDepth);
                    var backpropagationStack = _backpropagation[i];
                    while (backpropagationStack.Any()) {
                        var backpropagation = backpropagationStack.Pop();
                        var rowSignal = row.ConvertInPlaceToMatrix(backpropagation.RowCount, backpropagation.ColumnCount);
                        rowSignal = backpropagation.Execute(rowSignal, context, backpropagationStack.Any(), updates)?.AsIndexable();
                    }
                }
            }
            return errorSignal;
        }
    }
}
