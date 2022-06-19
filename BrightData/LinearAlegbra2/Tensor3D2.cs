using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlegbra2
{
    public class Tensor3D2<CU> : TensorBase2<ITensor3D, CU>, ITensor3D
        where CU: LinearAlgebraProvider
    {
        public Tensor3D2(ITensorSegment2 data, uint depth, uint rowCount, uint columnCount, CU computationUnit) : base(data, computationUnit)
        {
            Depth = depth;
            RowCount = rowCount;
            ColumnCount = columnCount;
            MatrixSize = rowCount * columnCount;
            Size = MatrixSize * depth;
        }

        public override ITensor3D Create(ITensorSegment2 segment) => new Tensor3D2<CU>(segment, Depth, RowCount, ColumnCount, _lap);

        public uint Depth { get; private set; }
        public uint RowCount { get; private set; }
        public uint ColumnCount { get; private set; }
        public uint MatrixSize { get; private set; }
        public override uint Size { get; protected set; }
        public override uint[] Shape
        {
            get => new[] { Depth, RowCount, ColumnCount };
            protected set
            {
                ColumnCount = value[0];
                RowCount = value[1];
                Depth = value[2];
                MatrixSize = RowCount * ColumnCount;
                Size = MatrixSize * Depth;
            }
        }

        public float this[int depth, int rowY, int columnX]
        {
            get => Segment[depth * MatrixSize + rowY * ColumnCount + columnX];
            set => Segment[depth * MatrixSize + rowY * ColumnCount + columnX] = value;
        }
        public float this[uint depth, uint rowY, uint columnX]
        {
            get => Segment[depth * MatrixSize + rowY * ColumnCount + columnX];
            set => Segment[depth * MatrixSize + rowY * ColumnCount + columnX] = value;
        }
        public float this[long depth, long rowY, long columnX]
        {
            get => Segment[depth * MatrixSize + rowY * ColumnCount + columnX];
            set => Segment[depth * MatrixSize + rowY * ColumnCount + columnX] = value;
        }
        public float this[ulong depth, ulong rowY, ulong columnX]
        {
            get => Segment[depth * MatrixSize + rowY * ColumnCount + columnX];
            set => Segment[depth * MatrixSize + rowY * ColumnCount + columnX] = value;
        }

        /// <summary>
        /// Converts the segment to a column major vector (default is row major)
        /// </summary>
        public MemoryOwner<float> ToNewColumnMajor()
        {
            var ret = MemoryOwner<float>.Allocate((int)Size);
            var ptr = ret.Span;
            var blockSize = Size / Depth;
            var k = 0;

            for(uint z = 0; z < Size; z++) {
                using var matrix = Matrix(z);
                var i = 0;
                var rowCount = matrix.RowCount;
                var rows = matrix.Rows();
                foreach (var row in rows) {
                    var j = 0;
                    foreach (var item in row.Values) {
                        var index = (j * rowCount + i) + (k * blockSize);
                        ptr[(int)index] = item;
                        ++j;
                    }
                    ++i;
                }
                ++k;
            }

            return ret;
        }

        public IMatrix Matrix(uint index) => _lap.GetMatrix(this, index);
        public ITensor3D AddPadding(uint padding) => _lap.AddPadding(this, padding);
        public ITensor3D RemovePadding(uint padding) => _lap.RemovePadding(this, padding);
        public IMatrix Im2Col(uint filterWidth, uint filterHeight, uint xStride, uint yStride) => _lap.Im2Col(this, filterWidth, filterHeight, xStride, yStride);
        public (ITensor3D Result, ITensor3D? Indices) MaxPool(uint filterWidth, uint filterHeight, uint xStride, uint yStride, bool saveIndices) => _lap.MaxPool(this, filterWidth, filterHeight, xStride, yStride, saveIndices);
        public ITensor3D ReverseMaxPool(ITensor3D indices, uint outputRows, uint outputColumns, uint filterWidth, uint filterHeight, uint xStride, uint yStride) => _lap.ReverseMaxPool(this, indices, outputRows, outputColumns, filterWidth, filterHeight, xStride, yStride);
        public ITensor3D ReverseIm2Col(IMatrix filter, uint outputRows, uint outputColumns, uint outputDepth, uint filterWidth, uint filterHeight, uint xStride, uint yStride) => _lap.ReverseIm2Col(this, filter, outputRows, outputColumns, outputDepth, filterWidth, filterHeight, xStride, yStride);
        public IMatrix CombineDepthSlices() => _lap.CombineDepthSlices(this);
        public ITensor3D Multiply(IMatrix matrix) => _lap.Multiply(this, matrix);
        public void AddToEachRow(IVector vector) => _lap.AddToEachRow(this, vector);
        public ITensor3D TransposeThisAndMultiply(ITensor4D other) => _lap.TransposeFirstAndMultiply(this, other);
        public override string ToString() => $"Tensor3D (Depth: {Depth}, Rows: {RowCount}, Columns: {ColumnCount})";
    }

    public class Tensor3D2 : Tensor3D2<LinearAlgebraProvider>
    {
        public Tensor3D2(ITensorSegment2 data, uint depth, uint rows, uint columns, LinearAlgebraProvider computationUnit) : base(data, depth, rows, columns, computationUnit)
        {
        }
    }

    public class ArrayBasedTensor3D : Tensor3D2<ArrayBasedLinearAlgebraProvider>
    {
        public ArrayBasedTensor3D(ITensorSegment2 data, uint depth, uint rows, uint columns, ArrayBasedLinearAlgebraProvider computationUnit) : base(data, depth, rows, columns, computationUnit)
        {
        }
    }
}
