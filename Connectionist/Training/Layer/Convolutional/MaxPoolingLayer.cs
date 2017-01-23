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
                var matrixList = error.AsIndexable().Columns.Select(v => v.ToArray()).ToList();

                var newMatrixList = new List<IMatrix>();
                Tuple<int, int> newIndex;
                for (var i = 0; i < matrixList.Count; i++) {
                    var matrix = matrixList[i];
                    var table = _indexPosList[i];

                    newMatrixList.Add(_lap.Create(_rows, _columns, (x, y) => {
                        if (table.TryGetValue(Tuple.Create(x, y), out newIndex)) {
                            var newIndex2 = newIndex.Item1 * _newRows + newIndex.Item2;
                            return matrix[newIndex2];
                        }
                        return 0f;
                    }));
                }
                using (var tensor = _lap.CreateTensor(newMatrixList)) {
                    var ret = tensor.ConvertToMatrix();
                    foreach (var item in newMatrixList)
                        item.Dispose();
                    return ret;
                }
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
            using (var ret = ExecuteToTensor(tensor, backpropagation))
                return ret.ConvertToVector();
        }
    }
}
