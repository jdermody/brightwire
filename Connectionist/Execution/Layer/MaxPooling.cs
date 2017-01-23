using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Connectionist.Execution.Layer
{
    internal class MaxPooling : IConvolutionalLayerExecution
    {
        readonly int _filterWidth, _filterHeight, _stride;

        public MaxPooling(int filterWidth, int filterHeight, int stride)
        {
            _filterWidth = filterWidth;
            _filterHeight = filterHeight;
            _stride = stride;
        }

        public void Dispose()
        {
            // nop
        }

        public IVector ExecuteToVector(I3DTensor tensor)
        {
            using (var ret = tensor.MaxPool(_filterWidth, _filterHeight, _stride, null))
                return ret.ConvertToVector();
        }

        public I3DTensor ExecuteToTensor(I3DTensor tensor)
        {
            return tensor.MaxPool(_filterWidth, _filterHeight, _stride, null);
        }
    }
}
