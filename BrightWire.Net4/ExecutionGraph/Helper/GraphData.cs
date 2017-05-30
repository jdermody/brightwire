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

        public int? RowId => _rowId;
        public int Rows => _matrix.RowCount;
        public int Columns => _matrix.ColumnCount;
        public int Depth => 1;
        public int Count => 1;

        public IMatrix GetMatrix()
        {
            return _matrix;
        }
        public IGraphData ReplaceWith(IMatrix matrix)
        {
            return new MatrixGraphData(matrix);
        }
    }

    class Tensor3DGraphData : IGraphData
    {
        readonly IMatrix _matrix;
        readonly int _rows, _columns;
        readonly int? _rowId;

        public Tensor3DGraphData(I3DTensor tensor):
            this(tensor.ConvertToMatrix(), tensor.RowCount, tensor.ColumnCount)
        {
        }
        public Tensor3DGraphData(IMatrix matrix, int rows, int columns, int? rowId = null)
        {
            _matrix = matrix;
            _rowId = rowId;
            _rows = rows;
            _columns = columns;
        }

        public int Rows => _rows;
        public int Columns => _columns;
        public int Depth => _matrix.ColumnCount;
        public int Count => 1;
        public int? RowId => _rowId;
        public IMatrix GetMatrix()
        {
            return _matrix;
        }
        public IGraphData ReplaceWith(IMatrix matrix)
        {
            return new Tensor3DGraphData(matrix, _rows, _columns);
        }
    }

    class Tensor4DGraphData : IGraphData
    {
        readonly IMatrix _matrix;
        readonly int _rows, _columns, _depth;
        readonly int? _rowId;

        public Tensor4DGraphData(I4DTensor tensor) :
            this(tensor.ConvertToMatrix(), tensor.RowCount, tensor.ColumnCount, tensor.Depth)
        {
        }
        public Tensor4DGraphData(IMatrix matrix, int rows, int columns, int depth, int? rowId = null)
        {
            _matrix = matrix;
            _rowId = rowId;
            _rows = rows;
            _columns = columns;
            _depth = depth;
        }

        public int Rows => _rows;
        public int Columns => _columns;
        public int Depth => _depth;
        public int Count => _matrix.ColumnCount;
        public int? RowId => _rowId;
        public IMatrix GetMatrix()
        {
            return _matrix;
        }
        public IGraphData ReplaceWith(IMatrix matrix)
        {
            return new Tensor4DGraphData(matrix, _rows, _columns, _depth);
        }
    }
}
