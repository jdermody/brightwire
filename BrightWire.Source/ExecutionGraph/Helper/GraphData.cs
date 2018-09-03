using System.Collections.Generic;
using System.Diagnostics;

namespace BrightWire.ExecutionGraph.Helper
{
    /// <summary>
    /// Graph data adaptor for matrices
    /// </summary>
    class MatrixGraphData : IGraphData
    {
        readonly IMatrix _matrix;

        public MatrixGraphData(IMatrix matrix)
        {
            _matrix = matrix;
        }

        public int Rows => _matrix.RowCount;
        public int Columns => _matrix.ColumnCount;
        public int Depth => 1;
        public int Count => 1;

        public IMatrix GetMatrix() => _matrix;
        public I4DTensor Get4DTensor() => null;
        public IGraphData ReplaceWith(IMatrix matrix) => new MatrixGraphData(matrix);
        public IReadOnlyList<IMatrix> GetSubMatrices()
        {
            return new[] {
                _matrix
            };
        }
    }

    /// <summary>
    /// Graph data adaptor for 3D tensors
    /// </summary>
    class Tensor3DGraphData : IGraphData
    {
        readonly IMatrix _matrix;

	    public Tensor3DGraphData(I3DTensor tensor):
            this(tensor.AsMatrix(), tensor.RowCount, tensor.ColumnCount)
        {
        }
        public Tensor3DGraphData(IMatrix matrix, int rows, int columns)
        {
            _matrix = matrix;
            Rows = rows;
            Columns = columns;
        }

        public int Rows { get; }
	    public int Columns { get; }
	    public int Depth => _matrix.ColumnCount;
        public int Count => 1;
	    public IMatrix GetMatrix() => _matrix;
        public IGraphData ReplaceWith(IMatrix matrix) => new Tensor3DGraphData(matrix, Rows, Columns);
        public IGraphData ReplaceWith(IContext context, IReadOnlyList<IMatrix> matrixList)
        {
            Debug.Assert(matrixList.Count == Depth);
            var tensor = context.LinearAlgebraProvider.Create3DTensor(matrixList);
            return new Tensor3DGraphData(tensor);
        }
        public IReadOnlyList<IMatrix> GetSubMatrices()
        {
            var ret = new IMatrix[Depth];
            for (var i = 0; i < Depth; i++)
                ret[i] = _matrix.Column(i).AsMatrix(Rows, Columns);
            return ret;
        }
        public I4DTensor Get4DTensor()
        {
            return null;
        }
    }

    /// <summary>
    /// Graph data adaptor for 4D tensors
    /// </summary>
    class Tensor4DGraphData : IGraphData
    {
        readonly IMatrix _matrix;

	    public Tensor4DGraphData(I4DTensor tensor) :
            this(tensor.AsMatrix(), tensor.RowCount, tensor.ColumnCount, tensor.Depth)
        {
        }
        public Tensor4DGraphData(IMatrix matrix, int rows, int columns, int depth)
        {
            _matrix = matrix;
            Rows = rows;
            Columns = columns;
            Depth = depth;
        }

        public int Rows { get; }
	    public int Columns { get; }
	    public int Depth { get; }
	    public int Count => _matrix.ColumnCount;
        public IMatrix GetMatrix() => _matrix;
        public IGraphData ReplaceWith(IMatrix matrix) => new Tensor4DGraphData(matrix, Rows, Columns, Depth);
        public IReadOnlyList<IMatrix> GetSubMatrices()
        {
            var ret = new List<IMatrix>();
            for (var j = 0; j < Count; j++) {
                var matrix = _matrix.Column(j);
                using (var tensor = matrix.As3DTensor(Rows, Columns, Depth)) {
                    for (var i = 0; i < Depth; i++)
                        ret.Add(tensor.GetMatrixAt(i));
                }
            }
            return ret;
        }
        public I4DTensor Get4DTensor()
        {
            return _matrix.As4DTensor(Rows, Columns, Depth);
        }
    }
}
