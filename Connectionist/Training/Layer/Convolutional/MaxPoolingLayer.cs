using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BrightWire.Models;

namespace BrightWire.Connectionist.Training.Layer.Convolutional
{
    internal class MaxPoolingLayer : IConvolutionalLayer
    {
        class Backpropagation : IConvolutionalLayerBackpropagation
        {
            readonly ILinearAlgebraProvider _lap;
            readonly List<Dictionary<Tuple<int, int>, Tuple<int, int>>> _indexPosList;
            readonly int _columns, _rows, _newColumns, _newRows;

            public Backpropagation(ILinearAlgebraProvider lap, List<Dictionary<Tuple<int, int>, Tuple<int, int>>> indexPosList, int columns, int rows, int newColumns, int newRows)
            {
                _lap = lap;
                _indexPosList = indexPosList;
                _columns = columns;
                _rows = rows;
                _newColumns = newColumns;
                _newRows = newRows;
            }

            public int ColumnCount
            {
                get
                {
                    return _indexPosList.Count;
                }
            }

            public int RowCount
            {
                get
                {
                    return _newRows * _newColumns;
                }
            }

            public IMatrix Execute(IMatrix error, ITrainingContext context, bool calculateOutput, INeuralNetworkUpdateAccumulator updateAccumulator)
            {
                var matrix = error.ConvertInPlaceToVector().Split(_indexPosList.Count).Select(v => v.AsIndexable().ToArray()).ToList();

                Tuple<int, int> newIndex;
                var ret = _lap.Create(_rows * _columns, _indexPosList.Count, (i, j) => {
                    var table = _indexPosList[j];
                    var x = i / _rows;
                    var y = i % _rows;
                    if (table.TryGetValue(Tuple.Create(x, y), out newIndex)) {
                        var newIndex2 = newIndex.Item1 * _newRows + newIndex.Item2;
                        return matrix[j][newIndex2];
                    }
                    return 0f;
                });
                return ret;
            }
        }
        readonly ILinearAlgebraProvider _lap;
        readonly int _filterWidth, _filterHeight, _stride;

        public ConvolutionalNetwork.Layer Layer
        {
            get
            {
                return new ConvolutionalNetwork.Layer {
                    Type = ConvolutionalNetwork.ConvolutionalLayerType.MaxPooling,
                    FilterHeight = _filterHeight,
                    FilterWidth = _filterWidth,
                    Stride = _stride
                };
            }
        }

        public MaxPoolingLayer(ILinearAlgebraProvider lap, int filterWidth, int filterHeight, int stride)
        {
            _lap = lap;
            _filterWidth = filterWidth;
            _filterHeight = filterHeight;
            _stride = stride;
        }

        public void Dispose()
        {
            // nop
        }

        public I3DTensor ExecuteToTensor(I3DTensor tensor, Stack<IConvolutionalLayerBackpropagation> backpropagation)
        {
            var indexPosList = new List<Dictionary<Tuple<int, int>, Tuple<int, int>>>();
            var ret = tensor.MaxPool(_filterWidth, _filterHeight, _stride, indexPosList);
            if (backpropagation != null)
                backpropagation.Push(new Backpropagation(_lap, indexPosList, tensor.ColumnCount, tensor.RowCount, ret.ColumnCount, ret.RowCount));
            return ret;
        }

        public IVector ExecuteToVector(I3DTensor tensor, Stack<IConvolutionalLayerBackpropagation> backpropagation)
        {
            var ret = ExecuteToTensor(tensor, backpropagation);
            return ret.ConvertInPlaceToVector();
        }
    }
}
