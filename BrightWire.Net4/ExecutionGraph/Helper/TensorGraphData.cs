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
        readonly int? _rowId;

        public TensorGraphData(I3DTensor tensor, int? rowId = null)
        {
            _tensor = tensor;
            _rowId = rowId;
        }

        public GraphDataType DataType => GraphDataType.Tensor;
        public int? RowId => _rowId;

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
