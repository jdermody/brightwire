using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Helper
{
    class TensorGraphData : IGraphData
    {
        readonly I3DTensor _tensor;

        public TensorGraphData(I3DTensor tensor)
        {
            _tensor = tensor;
        }

        public GraphDataType DataType => GraphDataType.Tensor;

        public IMatrix GetMatrix()
        {
            throw new NotImplementedException();
        }

        public I3DTensor GetTensor()
        {
            return _tensor;
        }
    }
}
