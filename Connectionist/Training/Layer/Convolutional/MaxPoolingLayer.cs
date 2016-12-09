using System;
using System.Collections.Generic;
using System.Text;

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

        public MaxPoolingLayer(int filterSize, int stride)
        {
            _filterSize = filterSize;
            _stride = stride;
        }

        public void Dispose()
        {
            // nop
        }

        public IMatrix Execute(IMatrix matrix, Stack<IConvolutionalLayerBackpropagation> backpropagation)
        {
            var matrix2 = matrix.AsIndexable();
            int xOffset = 0, yOffset = 0, width = matrix.RowCount, height = matrix.ColumnCount;

            //matrix2.ConvertInPlaceToVector()

            //for(var i = 0)
            return null;
        }
    }
}
