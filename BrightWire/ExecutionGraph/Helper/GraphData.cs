using BrightData;
using System.Diagnostics;
using System.Linq;

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

    class SingleGraphData : IGraphData
    {
        readonly float _value;

        public uint Rows { get; } = 1;
        public uint Columns { get; } = 1;
        public uint Depth { get; } = 1;
        public uint Count { get; } = 1;

        public SingleGraphData(float data) => _value = data;

        public IMatrix GetMatrix()
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

        public float this[uint index] => _value;
        public bool HasValue { get; } = true;
        public override string ToString() => $"Single graph data ({_value})";
    }

    /// <summary>
    /// Graph data adaptor for matrices
    /// </summary>
    internal class MatrixGraphData : IGraphData
    {
        readonly IMatrix _matrix;

        public MatrixGraphData(IMatrix matrix)
        {
            _matrix = matrix;
        }

        public uint Rows => _matrix.RowCount;
        public uint Columns => _matrix.ColumnCount;
        public uint Depth => 1;
        public uint Count => 1;

        public IMatrix GetMatrix() => _matrix;
        public ITensor4D? Get4DTensor() => null;
        public IGraphData ReplaceWith(IMatrix matrix) => new MatrixGraphData(matrix);

        public float this[uint index] => _matrix.Segment[index];
        public bool HasValue { get; } = true;

        public override string ToString() => $"Matrix graph data: {_matrix}";
    }

    /// <summary>
    /// Graph data adaptor for 3D tensors
    /// </summary>
    internal class Tensor3DGraphData : IGraphData
    {
        readonly IMatrix _matrix;

	    public Tensor3DGraphData(ITensor3D tensor):
            this(tensor.ReshapeAsMatrix(), tensor.RowCount, tensor.ColumnCount)
        {
        }
        public Tensor3DGraphData(IMatrix matrix, uint rows, uint columns)
        {
            _matrix = matrix;
            Rows = rows;
            Columns = columns;
        }

        public uint Rows { get; }
	    public uint Columns { get; }
	    public uint Depth => _matrix.ColumnCount;
        public uint Count => 1;
	    public IMatrix GetMatrix() => _matrix;
        public IGraphData ReplaceWith(IMatrix matrix) => new Tensor3DGraphData(matrix, Rows, Columns);
        public IGraphData ReplaceWith(IGraphContext context, IMatrix[] matrixList)
        {
            Debug.Assert(matrixList.Length == Depth);
            var tensor = context.GetLinearAlgebraProvider().CreateTensor3D(matrixList);
            return new Tensor3DGraphData(tensor);
        }
        public ITensor4D? Get4DTensor()
        {
            return null;
        }
        public bool HasValue { get; } = true;
        public override string ToString() => $"Tensor 3D graph data (rows:{Rows}, columns:{Columns}, depth:{Depth}): {_matrix}";
        public float this[uint index] => _matrix.Segment[index];
    }

    /// <summary>
    /// Graph data adaptor for 4D tensors
    /// </summary>
    internal class Tensor4DGraphData : IGraphData
    {
        readonly IMatrix _matrix;

	    public Tensor4DGraphData(ITensor4D tensor) :
            this(tensor.ReshapeAsMatrix(), tensor.RowCount, tensor.ColumnCount, tensor.Depth)
        {
        }
        public Tensor4DGraphData(IMatrix matrix, uint rows, uint columns, uint depth)
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
        public IMatrix GetMatrix() => _matrix;
        public IGraphData ReplaceWith(IMatrix matrix) => new Tensor4DGraphData(matrix, Rows, Columns, Depth);
        public ITensor4D Get4DTensor()
        {
            return _matrix.ReshapeAs4DTensor(Rows, Columns, Depth);
        }
        public bool HasValue { get; } = true;
        public override string ToString() => $"Tensor 4D graph data (rows:{Rows}, columns:{Columns}, depth:{Depth}, count:{Count}): {_matrix}";
        public float this[uint index] => _matrix.Segment[index];

    }

    static class GraphHelperMethods
    {
        public static IMatrix ReshapeAsMatrix(this ITensor3D tensor)
        {
            return tensor.Reshape(tensor.RowCount * tensor.ColumnCount, tensor.Depth);
        }

        public static IMatrix ReshapeAsMatrix(this ITensor4D tensor)
        {
            return tensor.Reshape(tensor.RowCount * tensor.ColumnCount * tensor.Depth, tensor.Count);
        }

        //public static ITensor3D ReshapeAs3DTensor(this IMatrix inputMatrix, uint rows, uint columns, uint depth)
        //{
        //    Debug.Assert(rows * columns * depth == inputMatrix.ColumnCount);
        //    if (depth > 1) {
        //        var slice = inputMatrix.Split(depth);
        //        var matrixList = slice.Select(part => part.Reshape(rows, columns)).ToArray();
        //        return inputMatrix.LinearAlgebraProvider.CreateTensor3D(true, matrixList);
        //    }
        //    var matrix = ReshapeAsMatrix(rows, columns).Data;
        //    return new Float3DTensor(Data.Context.CreateTensor3D(matrix));
        //}

        public static ITensor4D ReshapeAs4DTensor(this IMatrix matrix, uint rows, uint columns, uint depth)
        {
            var lap = matrix.LinearAlgebraProvider;
            var list = new ITensor3D[matrix.ColumnCount];
            for (uint i = 0; i < matrix.ColumnCount; i++)
                list[i] = lap.CreateTensor3DAndThenDisposeInput(matrix.GetColumn(i).Segment.Split(depth).Select(v => v.ToMatrix(lap, rows, columns)).ToArray());
            return lap.CreateTensor4DAndThenDisposeInput(list);
        }
    }
}
