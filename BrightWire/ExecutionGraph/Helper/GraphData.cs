using System.Diagnostics;
using BrightData;

namespace BrightWire.ExecutionGraph.Helper
{
    public static class GraphData
    {
        public static readonly IGraphData Null = new NullGraphData();
    }

    class NullGraphData : IGraphData
    {
        internal NullGraphData()
        {
        }

        public uint Rows { get; } = 0;
        public uint Columns { get; } = 0;
        public uint Depth { get; } = 0;
        public uint Count { get; } = 0;
        public IFloatMatrix GetMatrix()
        {
            throw new System.NotImplementedException();
        }

        public I4DFloatTensor Get4DTensor()
        {
            throw new System.NotImplementedException();
        }

        public IGraphData ReplaceWith(IFloatMatrix matrix)
        {
            throw new System.NotImplementedException();
        }

        public IFloatMatrix[] GetSubMatrices()
        {
            throw new System.NotImplementedException();
        }

        public bool HasValue { get; } = false;

        public override string ToString() => "Null graph data";
    }

    /// <summary>
    /// Graph data adaptor for matrices
    /// </summary>
    internal class MatrixGraphData : IGraphData
    {
        readonly IFloatMatrix _matrix;

        public MatrixGraphData(IFloatMatrix matrix)
        {
            _matrix = matrix;
        }

        public uint Rows => _matrix.RowCount;
        public uint Columns => _matrix.ColumnCount;
        public uint Depth => 1;
        public uint Count => 1;

        public IFloatMatrix GetMatrix() => _matrix;
        public I4DFloatTensor? Get4DTensor() => null;
        public IGraphData ReplaceWith(IFloatMatrix matrix) => new MatrixGraphData(matrix);
        public IFloatMatrix[] GetSubMatrices()
        {
            return new[] {
                _matrix
            };
        }
        public bool HasValue { get; } = true;

        public override string ToString() => $"Matrix graph data (rows:{Rows}, columns:{Columns})";
    }

    /// <summary>
    /// Graph data adaptor for 3D tensors
    /// </summary>
    internal class Tensor3DGraphData : IGraphData
    {
        readonly IFloatMatrix _matrix;

	    public Tensor3DGraphData(I3DFloatTensor tensor):
            this(tensor.ReshapeAsMatrix(), tensor.RowCount, tensor.ColumnCount)
        {
        }
        public Tensor3DGraphData(IFloatMatrix matrix, uint rows, uint columns)
        {
            _matrix = matrix;
            Rows = rows;
            Columns = columns;
        }

        public uint Rows { get; }
	    public uint Columns { get; }
	    public uint Depth => _matrix.ColumnCount;
        public uint Count => 1;
	    public IFloatMatrix GetMatrix() => _matrix;
        public IGraphData ReplaceWith(IFloatMatrix matrix) => new Tensor3DGraphData(matrix, Rows, Columns);
        public IGraphData ReplaceWith(IGraphSequenceContext context, IFloatMatrix[] matrixList)
        {
            Debug.Assert(matrixList.Length == Depth);
            var tensor = context.LinearAlgebraProvider.Create3DTensor(matrixList);
            return new Tensor3DGraphData(tensor);
        }
        public IFloatMatrix[] GetSubMatrices()
        {
            var ret = new IFloatMatrix[Depth];
            for (uint i = 0; i < Depth; i++)
                ret[i] = _matrix.Column(i).ReshapeAsMatrix(Rows, Columns);
            return ret;
        }
        public I4DFloatTensor? Get4DTensor()
        {
            return null;
        }
        public bool HasValue { get; } = true;
        public override string ToString() => $"Tensor 3D graph data (rows:{Rows}, columns:{Columns}, depth:{Depth})";
    }

    /// <summary>
    /// Graph data adaptor for 4D tensors
    /// </summary>
    internal class Tensor4DGraphData : IGraphData
    {
        readonly IFloatMatrix _matrix;

	    public Tensor4DGraphData(I4DFloatTensor tensor) :
            this(tensor.ReshapeAsMatrix(), tensor.RowCount, tensor.ColumnCount, tensor.Depth)
        {
        }
        public Tensor4DGraphData(IFloatMatrix matrix, uint rows, uint columns, uint depth)
        {
            _matrix = matrix;
            Rows = rows;
            Columns = columns;
            Depth = depth;
        }

        public uint Rows { get; }
	    public uint Columns { get; }
	    public uint Depth { get; }
	    public uint Count => _matrix.ColumnCount;
        public IFloatMatrix GetMatrix() => _matrix;
        public IGraphData ReplaceWith(IFloatMatrix matrix) => new Tensor4DGraphData(matrix, Rows, Columns, Depth);
        public IFloatMatrix[] GetSubMatrices()
        {
            var index = 0;
            var ret = new IFloatMatrix[Count * Depth];
            for (uint j = 0; j < Count; j++) {
                var matrix = _matrix.Column(j);
                using var tensor = matrix.ReshapeAs3DTensor(Rows, Columns, Depth);
                for (uint i = 0; i < Depth; i++)
                    ret[index++] = tensor.GetMatrixAt(i);
            }
            return ret;
        }
        public I4DFloatTensor Get4DTensor()
        {
            return _matrix.ReshapeAs4DTensor(Rows, Columns, Depth);
        }
        public bool HasValue { get; } = true;
        public override string ToString() => $"Tensor 4D graph data (rows:{Rows}, columns:{Columns}, depth:{Depth}, count:{Count})";
    }
}
