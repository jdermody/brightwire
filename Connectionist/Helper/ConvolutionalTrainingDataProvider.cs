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
        readonly IReadOnlyList<ConvolutionalData> _data;
        readonly List<IMatrix> _matrixList;
        readonly int _inputSize, _outputSize;
        readonly List<IConvolutionalLayer> _layer = new List<IConvolutionalLayer>();
        readonly List<Stack<IConvolutionalLayerBackpropagation>> _backpropagation = new List<Stack<IConvolutionalLayerBackpropagation>>();
        readonly ConvolutionDescriptor _descriptor;

        public ConvolutionalTrainingDataProvider(ILinearAlgebraProvider lap, int padding, ConvolutionDescriptor descriptor, IReadOnlyList<ConvolutionalData> data)
        {
            _lap = lap;
            _data = data;
            _descriptor = descriptor;
            _matrixList = data
                .Select(d => padding > 0 ? d.AddPadding(padding) : d)
                .Select(d => d.Im2Col(lap, descriptor))
                .ToList()
            ;
            var first = _matrixList.First();
            _inputSize = first.RowCount * descriptor.FilterDepth;
            _outputSize = _data.First().ExpectedOutput.Length;

            // create the convolutional layer
            _layer.Add(new ConvolutionalLayer(lap.NN, first.ColumnCount, descriptor));
            _layer.Add(new MaxPoolingLayer(2, 2));
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
            var rowList = new List<IIndexableVector>();
            foreach (var item in rows) {
                var matrix = _matrixList[item];
                var backpropagation = new Stack<IConvolutionalLayerBackpropagation>();
                foreach (var layer in _layer)
                    matrix = layer.Execute(matrix, backpropagation);
                _backpropagation.Add(backpropagation);
                rowList.Add(matrix.AsIndexable().ConvertInPlaceToVector());
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
            for(var i = 0; i < errorSignal.RowCount; i++) {
                var row = errorSignal.Row(i).AsIndexable();
                //row = row.Rotate180(row.Count / _descriptor.FilterDepth);
                var backpropagationStack = _backpropagation[i];
                while (backpropagationStack.Any()) {
                    var backpropagation = backpropagationStack.Pop();
                    var rowSignal = row.ConvertInPlaceToMatrix(backpropagation.RowCount, backpropagation.ColumnCount);
                    rowSignal = backpropagation.Execute(rowSignal, context, backpropagationStack.Any(), updates)?.AsIndexable();
                }
            }
            return errorSignal;
        }
    }
}
