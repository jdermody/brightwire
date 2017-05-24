using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Helper
{
    class MatrixGraphData : IGraphData
    {
        readonly IMatrix _matrix;
        readonly int? _rowId;

        public MatrixGraphData(IMatrix matrix, int? rowId = null)
        {
            _matrix = matrix;
            _rowId = rowId;
        }

        public GraphDataType DataType => GraphDataType.Matrix;
        public int? RowId => _rowId;

        public IMatrix GetMatrix()
        {
            return _matrix;
        }

        public I3DTensor GetTensor()
        {
            throw new NotImplementedException();
        }
    }
}
