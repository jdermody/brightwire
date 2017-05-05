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

        public GraphDataType CurrentType => GraphDataType.Matrix;
        public IMatrix GetAsMatrix()
        {
            return _matrix;
        }
    }
}
