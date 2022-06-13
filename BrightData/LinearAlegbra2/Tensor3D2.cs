using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlegbra2
{
    public class Tensor3D2<CU> : TensorBase2<ITensor3D, CU>, ITensor3D
        where CU: ComputationUnit
    {
        public Tensor3D2(ITensorSegment2 data, uint depth, uint rowCount, uint columnCount, CU computationUnit) : base(data, computationUnit)
        {
            Depth = depth;
            RowCount = rowCount;
            ColumnCount = columnCount;
            MatrixSize = rowCount * columnCount;
            Size = MatrixSize * depth;
        }

        public override ITensor3D Create(ITensorSegment2 segment) => new Tensor3D2<CU>(segment, Depth, RowCount, ColumnCount, _computationUnit);

        public uint Depth { get; }
        public uint RowCount { get; }
        public uint ColumnCount { get; }
        public uint MatrixSize { get; }
        public override uint Size { get; }

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

        public IMatrix Matrix(uint index) => _computationUnit.GetMatrix(this, index);
        public ITensor3D AddPadding(uint padding) => _computationUnit.AddPadding(this, padding);
        public ITensor3D RemovePadding(uint padding) => _computationUnit.RemovePadding(this, padding);
        public IMatrix Im2Col(uint filterWidth, uint filterHeight, uint xStride, uint yStride) => _computationUnit.Im2Col(this, filterWidth, filterHeight, xStride, yStride);
        public (ITensor3D Result, ITensor3D? Indices) MaxPool(uint filterWidth, uint filterHeight, uint xStride, uint yStride, bool saveIndices) => _computationUnit.MaxPool(this, filterWidth, filterHeight, xStride, yStride, saveIndices);
        public ITensor3D ReverseMaxPool(ITensor3D indices, uint outputRows, uint outputColumns, uint filterWidth, uint filterHeight, uint xStride, uint yStride) => _computationUnit.ReverseMaxPool(this, indices, outputRows, outputColumns, filterWidth, filterHeight, xStride, yStride);
        public ITensor3D ReverseIm2Col(IMatrix filter, uint outputRows, uint outputColumns, uint outputDepth, uint filterWidth, uint filterHeight, uint xStride, uint yStride) => _computationUnit.ReverseIm2Col(this, filter, outputRows, outputColumns, outputDepth, filterWidth, filterHeight, xStride, yStride);
        public IMatrix CombineDepthSlices() => _computationUnit.CombineDepthSlices(this);
        public ITensor3D Multiply(IMatrix matrix) => _computationUnit.Multiply(this, matrix);
        public void AddToEachRow(IVector vector) => _computationUnit.AddToEachRow(this, vector);
        public ITensor3D TransposeThisAndMultiply(ITensor4D other) => _computationUnit.TransposeFirstAndMultiply(this, other);
        public override string ToString() => $"Tensor3D (Depth: {Depth}, Rows: {RowCount}, Columns: {ColumnCount})";
    }

    public class Tensor3D2 : Tensor3D2<ComputationUnit>
    {
        public Tensor3D2(ITensorSegment2 data, uint depth, uint rows, uint columns, ComputationUnit computationUnit) : base(data, depth, rows, columns, computationUnit)
        {
        }
    }
}
