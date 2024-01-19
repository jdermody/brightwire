using BrightData;

namespace BrightWire.ExecutionGraph.Helper
{
    /// <summary>
    /// Graph data constants
    /// </summary>
    public static class GraphData
    {
        /// <summary>
        /// Null graph data
        /// </summary>
        public static readonly IGraphData Null = new NullGraphData();
    }

    class NullGraphData : IGraphData
    {
        public uint Rows { get; } = 0;
        public uint Columns { get; } = 0;
        public uint Depth { get; } = 0;
        public uint Count { get; } = 0;
        public IMatrix GetMatrix()
        {
            throw new System.NotImplementedException();
        }

        public ITensor3D Get3DTensor()
        {
            throw new System.NotImplementedException();
        }

        public ITensor4D Get4DTensor()
        {
            throw new System.NotImplementedException();
        }

        public IGraphData ReplaceWith(IMatrix matrix)
        {
            throw new System.NotImplementedException();
        }

        public float this[uint index] => throw new System.NotImplementedException();

        public bool HasValue { get; } = false;

        public override string ToString() => "Null graph data";
    }

    class SingleGraphData(float data) : IGraphData
    {
        public uint Rows { get; } = 1;
        public uint Columns { get; } = 1;
        public uint Depth { get; } = 1;
        public uint Count { get; } = 1;

        public IMatrix GetMatrix()
        {
            throw new System.NotImplementedException();
        }

        public ITensor3D Get3DTensor()
        {
            throw new System.NotImplementedException();
        }

        public ITensor4D Get4DTensor()
        {
            throw new System.NotImplementedException();
        }

        public IGraphData ReplaceWith(IMatrix matrix)
        {
            throw new System.NotImplementedException();
        }

        public float this[uint index] => data;
        public bool HasValue { get; } = true;
        public override string ToString() => $"Single graph data ({data})";
    }

    /// <summary>
    /// Graph data adaptor for matrices
    /// </summary>
    internal class MatrixGraphData(IMatrix matrix) : IGraphData
    {
        public uint Rows => matrix.RowCount;
        public uint Columns => matrix.ColumnCount;
        public uint Depth => 1;
        public uint Count => 1;

        public IMatrix GetMatrix() => matrix;
        public ITensor3D? Get3DTensor() => null;
        public ITensor4D? Get4DTensor() => null;
        public IGraphData ReplaceWith(IMatrix other) => new MatrixGraphData(other);

        public float this[uint index] => matrix.Segment[index];
        public bool HasValue { get; } = true;

        public override string ToString() => $"Matrix graph data: {matrix}";
    }

    /// <summary>
    /// Graph data adaptor for 3D tensors
    /// </summary>
    internal class Tensor3DGraphData(IMatrix matrix, uint rows, uint columns) : IGraphData
    {
        public Tensor3DGraphData(ITensor3D tensor):
            this(tensor.ReshapeAsMatrix(), tensor.RowCount, tensor.ColumnCount)
        {
        }

        public uint Rows { get; } = rows;
        public uint Columns { get; } = columns;
        public uint Depth => matrix.ColumnCount;
        public uint Count => 1;
	    public IMatrix GetMatrix() => matrix;
        public ITensor3D Get3DTensor() => matrix.Reshape(matrix.ColumnCount, Rows, Columns);
        public IGraphData ReplaceWith(IMatrix other) => new Tensor3DGraphData(other, Rows, Columns);
        public ITensor4D? Get4DTensor()
        {
            return null;
        }
        public bool HasValue { get; } = true;
        public override string ToString() => $"Tensor 3D graph data (rows:{Rows}, columns:{Columns}, depth:{Depth}): {matrix}";
        public float this[uint index] => matrix.Segment[index];
    }

    /// <summary>
    /// Graph data adapter for 4D tensors
    /// </summary>
    internal class Tensor4DGraphData(IMatrix matrix, uint rows, uint columns, uint depth)
        : IGraphData
    {
        public Tensor4DGraphData(ITensor4D tensor) :
            this(tensor.ReshapeAsMatrix(), tensor.RowCount, tensor.ColumnCount, tensor.Depth)
        {
        }

        public uint Rows { get; } = rows;
        public uint Columns { get; } = columns;
        public uint Depth { get; } = depth;
        public uint Count => matrix.ColumnCount;
        public IMatrix GetMatrix() => matrix;
        public IGraphData ReplaceWith(IMatrix other) => new Tensor4DGraphData(other, Rows, Columns, Depth);
        public ITensor3D Get3DTensor() => matrix.Reshape(null, Rows, Columns);
        public ITensor4D Get4DTensor() => matrix.Reshape(null, Depth, Rows, Columns);
        public bool HasValue { get; } = true;
        public override string ToString() => $"Tensor 4D graph data (rows:{Rows}, columns:{Columns}, depth:{Depth}, count:{Count}): {matrix}";
        public float this[uint index] => matrix.Segment[index];

    }

    /// <summary>
    /// Graph extension methods
    /// </summary>
    public static class GraphHelperMethods
    {
        /// <summary>
        /// Reshapes the 3D tensor as a matrix
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public static IMatrix ReshapeAsMatrix(this ITensor3D tensor) => tensor.Reshape(null, tensor.Depth);

        /// <summary>
        /// Reshapes the 4D tensor as a matrix
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public static IMatrix ReshapeAsMatrix(this ITensor4D tensor) => tensor.Reshape(null, tensor.Count);
    }
}
