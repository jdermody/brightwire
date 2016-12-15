using BrightWire.Models.Convolutional;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace BrightWire.Connectionist.Training.Layer.Convolutional
{
    public class MaxPoolingLayer : IConvolutionalLayer
    {
        class Backpropagation : IConvolutionalLayerBackpropagation
        {
            readonly ConvolutionDescriptor _descriptor;
            readonly IReadOnlyList<int[]> _indexList;
            readonly int _size, _filterSize, _stride;
            readonly ILinearAlgebraProvider _lap;

            public Backpropagation(ILinearAlgebraProvider lap, ConvolutionDescriptor descriptor, IReadOnlyList<int[]> indexList, int size, int filterSize, int stride)
            {
                _lap = lap;
                _descriptor = descriptor;
                _indexList = indexList;
                _size = size;
                _filterSize = filterSize;
                _stride = stride;
            }

            public int ColumnCount
            {
                get
                {
                    return _size;
                }
            }

            public ConvolutionDescriptor Descriptor
            {
                get
                {
                    return _descriptor;
                }
            }

            public int RowCount
            {
                get
                {
                    return _size;
                }
            }

            public IMatrix Execute(IMatrix error, ITrainingContext context, bool calculateOutput, INeuralNetworkUpdateAccumulator updateAccumulator)
            {
                var filterIndex = 0;
                var filters = error.ConvertInPlaceToVector().Split(_descriptor.FilterDepth);
                var sparseDictionary = Enumerable.Range(0, _descriptor.FilterDepth).Select(i => new Dictionary<Tuple<int, int>, float>()).ToList();

                foreach (var item in filters) {
                    var itemIndex = 0;
                    int xOffset = 0, yOffset = 0;
                    foreach(var value in item.AsIndexable().Values) {
                        var maxIndex = _indexList[itemIndex][filterIndex];
                        var yIndex = maxIndex / _filterSize;
                        var xIndex = maxIndex % _filterSize;
                        sparseDictionary[filterIndex].Add(Tuple.Create(xOffset + xIndex, yOffset + yIndex), value);
                        xOffset += _filterSize;
                        if(xOffset >= _size) {
                            yOffset += _filterSize;
                            xOffset = 0;
                        }
                        ++itemIndex;
                    }
                    ++filterIndex;
                }

                // 4 columns, 784 rows
                return _lap.Create(_size * _size, _descriptor.FilterDepth, (i, j) => {
                    var y = i / _size;
                    var x = i % _size;
                    float val;
                    if (sparseDictionary[j].TryGetValue(Tuple.Create(x, y), out val))
                        return val;
                    return 0f;
                });
            }
        }
        readonly int _filterSize, _stride, _inputWidth;
        readonly ConvolutionDescriptor _descriptor;
        readonly ILinearAlgebraProvider _lap;

        public MaxPoolingLayer(ILinearAlgebraProvider lap, ConvolutionDescriptor descriptor, int filterSize, int stride, int inputWidth)
        {
            _lap = lap;
            _filterSize = filterSize;
            _stride = stride;
            _descriptor = descriptor;
            _inputWidth = inputWidth;
        }

        public void Dispose()
        {
            // nop
        }

        public I3DTensor ExecuteToTensor(I3DTensor tensor, Stack<IConvolutionalLayerBackpropagation> backpropagation)
        {
            var matrix = tensor.Im2Col(_filterSize, _filterSize, _stride);
            var size = (int)Math.Sqrt(matrix.RowCount);
            var output = Enumerable.Range(0, tensor.Depth).Select(i => new List<float>()).ToArray();
            var indexList = new List<int[]>();

            for(int i = 0, len = matrix.RowCount; i < len; i++) {
                var row = matrix.Row(i);
                var parts = row.Split(_descriptor.FilterDepth);
                var maxIndex = parts.Select(v => v.MaximumIndex()).ToArray();
                for(var j = 0; j < tensor.Depth; j++) {
                    var index = maxIndex[j];
                    var slice = parts[j].AsIndexable();
                    output[j].Add(slice[index]);
                }
                indexList.Add(maxIndex);
            }
            var matrixList = new List<IMatrix>();
            foreach(var slice in output) {
                var rowList = new List<float[]>();
                for(var i = 0; i < size; i++)
                    rowList.Add(slice.Skip(i * size).Take(size).ToArray());
                //matrixList.Add(_lap.Create(rowList.Count, size, (i, j) => rowList[j][i]));
                matrixList.Add(_lap.Create(rowList.Count, size, (i, j) => rowList[i][j]));
            }
            if (backpropagation != null)
                backpropagation.Push(new Backpropagation(_lap, _descriptor, indexList, _inputWidth, _filterSize, _stride));
            return _lap.CreateTensor(matrixList);
        }

        public IVector ExecuteToVector(I3DTensor tensor, Stack<IConvolutionalLayerBackpropagation> backpropagation)
        {
            var ret = ExecuteToTensor(tensor, backpropagation);
            return ret.ConvertInPlaceToVector();
        }
    }
}
