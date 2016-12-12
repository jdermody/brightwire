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
            public int ColumnCount
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public int RowCount
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public IMatrix Execute(IMatrix error, ITrainingContext context, bool calculateOutput, INeuralNetworkUpdateAccumulator updateAccumulator)
            {
                throw new NotImplementedException();
            }
        }
        readonly int _filterSize, _stride;
        readonly ConvolutionDescriptor _descriptor;

        public int OutputSize
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public MaxPoolingLayer(ConvolutionDescriptor descriptor, int filterSize, int stride)
        {
            _filterSize = filterSize;
            _stride = stride;
            _descriptor = descriptor;
        }

        public void Dispose()
        {
            // nop
        }

        public IMatrix Execute(IMatrix matrix, Stack<IConvolutionalLayerBackpropagation> backpropagation)
        {
            //var matrix2 = matrix.AsIndexable();
            //var parts = matrix2.ConvertInPlaceToVector().Split(_descriptor.FilterDepth);
            //var matrixParts = parts.Select(d => d.ConvertInPlaceToMatrix(_descriptor.FieldSize, _descriptor.FieldSize)).ToList();

            //var ret = new List<float>();
            //var retPos = new List<Tuple<int, int>>();
            //foreach (var item in matrixParts) {
            //    int xOffset = 0, yOffset = 0, width = matrix.RowCount, height = matrix.ColumnCount;
            //    while (yOffset <= height - _filterSize) {
            //        float max = float.MinValue;
            //        Tuple<int, int> bestPos = null;
            //        for (var j = 0; j < _filterSize; j++) {
            //            for (var i = 0; i < _filterSize; i++) {
            //                var value = item[i, j];
            //                if(value > max) {
            //                    max = value;
            //                    bestPos = Tuple.Create(i, j);
            //                }
            //            }
            //        }
            //        ret.Add(max);
            //        retPos.Add(bestPos);

            //        // move the window
            //        xOffset += _stride;
            //        if (xOffset > width - _filterSize) {
            //            xOffset = 0;
            //            yOffset += _stride;
            //        }
            //    }
            //}
            return matrix;
        }
    }
}
