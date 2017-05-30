using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public IGraphData ReplaceWith(IContext context, IReadOnlyList<IMatrix> matrixList)
        {
            Debug.Assert(matrixList.Count == 1);
            return new MatrixGraphData(matrixList.First());
        }
        public IReadOnlyList<IMatrix> AllMatrices => new[] { _matrix };
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
        public IGraphData ReplaceWith(IContext context, IReadOnlyList<IMatrix> matrixList)
        {
            Debug.Assert(matrixList.Count == Depth);
            var tensor = context.LinearAlgebraProvider.Create3DTensor(matrixList);
            return new Tensor3DGraphData(tensor);
        }
        public IReadOnlyList<IMatrix> AllMatrices => _matrix.ConvertTo3DTensor(_rows, _columns).SubMatrices;
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
        public IGraphData ReplaceWith(IContext context, IReadOnlyList<IMatrix> matrixList)
        {
            Debug.Assert(matrixList.Count == Count * Depth);
            var groupedMatrixList = Enumerable.Range(0, Count).Select(z => new IMatrix[Depth]).ToArray();
            var curr = new List<IMatrix>();
            for(var i = 0; i < matrixList.Count; i++) {
                var z = i / Count;
                var k = i % Count;
                groupedMatrixList[z][k] = matrixList[i];
            }
            var lap = context.LinearAlgebraProvider;
            var tensorList = groupedMatrixList.Select(gm => lap.Create3DTensor(gm)).ToList();
            var tensor = context.LinearAlgebraProvider.Create4DTensor(tensorList);
            return new Tensor4DGraphData(tensor);
        }
        public IReadOnlyList<IMatrix> AllMatrices => _matrix.ConvertTo4DTensor(_rows, _columns, _depth).SubMatrices;
    }
}
