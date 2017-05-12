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

        public MatrixGraphData(IMatrix matrix) { _matrix = matrix; }

        public GraphDataType DataType => GraphDataType.Matrix;

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
