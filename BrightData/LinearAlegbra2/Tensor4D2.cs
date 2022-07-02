namespace BrightData.LinearAlegbra2
{
    public class Tensor4D2<LAP> : TensorBase2<ITensor4D, LAP>, ITensor4D
        where LAP: LinearAlgebraProvider
    {
        public Tensor4D2(ITensorSegment2 data, uint count, uint depth, uint rows, uint columns, LAP lap) : base(data, lap)
        {
            Count = count;
            Depth = depth;
            RowCount = rows;
            ColumnCount = columns;
            MatrixSize = RowCount * ColumnCount;
            TensorSize = MatrixSize * Depth;
            TotalSize = TensorSize * Count;
        }

        public override ITensor4D Create(ITensorSegment2 segment) => new Tensor4D2<LAP>(segment, Count, Depth, RowCount, ColumnCount, _lap);

        public uint Count { get; private set; }
        public uint Depth { get; private set; }
        public uint RowCount { get; private set; }
        public uint ColumnCount { get; private set; }
        public uint MatrixSize { get; private set; }
        public uint TensorSize { get; private set; }
        public sealed override uint TotalSize { get; protected set; }
        public sealed override uint[] Shape
        {
            get => new[] { ColumnCount, RowCount, Depth, Count };
            protected set
            {
                ColumnCount = value[0];
                RowCount = value[1];
                Depth = value[2];
                Count = value[3];
                MatrixSize = RowCount * ColumnCount;
                TensorSize = MatrixSize * Depth;
                TotalSize = TensorSize * Count;
            }
        }

        public float this[int count, int depth, int rowY, int columnX]
        {
            get => Segment[count * TotalSize + depth * MatrixSize + rowY * ColumnCount + columnX];
            set => Segment[count * TotalSize + depth * MatrixSize + rowY * ColumnCount + columnX] = value;
        }
        public float this[uint count, uint depth, uint rowY, uint columnX]
        {
            get => Segment[count * TotalSize + depth * MatrixSize + rowY * ColumnCount + columnX];
            set => Segment[count * TotalSize + depth * MatrixSize + rowY * ColumnCount + columnX] = value;
        }
        public float this[long count, long depth, long rowY, long columnX]
        {
            get => Segment[count * TotalSize + depth * MatrixSize + rowY * ColumnCount + columnX];
            set => Segment[count * TotalSize + depth * MatrixSize + rowY * ColumnCount + columnX] = value;
        }
        public float this[ulong count, ulong depth, ulong rowY, ulong columnX]
        {
            get => Segment[count * TotalSize + depth * MatrixSize + rowY * ColumnCount + columnX];
            set => Segment[count * TotalSize + depth * MatrixSize + rowY * ColumnCount + columnX] = value;
        }
        public ITensor4D Create(LinearAlgebraProvider lap)
        {
            throw new System.NotImplementedException();
        }
        public ITensor3D Tensor(uint index) => _lap.GetTensor(this, index);
        public ITensor4D AddPadding(uint padding) => _lap.AddPadding(this, padding);
        public ITensor4D RemovePadding(uint padding) => _lap.RemovePadding(this, padding);
        public (ITensor4D Result, ITensor4D? Indices) MaxPool(uint filterWidth, uint filterHeight, uint xStride, uint yStride, bool saveIndices) => _lap.MaxPool(this, filterWidth, filterHeight, xStride, yStride, saveIndices);
        public ITensor4D ReverseMaxPool(ITensor4D indices, uint outputRows, uint outputColumns, uint filterWidth, uint filterHeight, uint xStride, uint yStride) => _lap.ReverseMaxPool(this, indices, outputRows, outputColumns, filterWidth, filterHeight, xStride, yStride);
        public ITensor3D Im2Col(uint filterWidth, uint filterHeight, uint xStride, uint yStride) => _lap.Im2Col(this, filterWidth, filterHeight, xStride, yStride);
        public ITensor4D ReverseIm2Col(IMatrix filter, uint outputRows, uint outputColumns, uint outputDepth, uint filterWidth, uint filterHeight, uint xStride, uint yStride) => _lap.ReverseIm2Col(this, filter, outputRows, outputColumns, outputDepth, filterWidth, filterHeight, xStride, yStride);
        public IVector ColumnSums() => _lap.ColumnSums(this);

        public override string ToString() => $"Tensor4D (Count: {Count}, Depth: {Depth}, Rows: {RowCount}, Columns: {ColumnCount})";
    }

    public class Tensor4D2 : Tensor4D2<LinearAlgebraProvider>
    {
        public Tensor4D2(ITensorSegment2 data, uint count, uint depth, uint rows, uint columns, LinearAlgebraProvider computationUnit) : base(data, count, depth, rows, columns, computationUnit)
        {
        }
    }

    public class ArrayBasedTensor4D : Tensor4D2<ArrayBasedLinearAlgebraProvider>
    {
        public ArrayBasedTensor4D(ITensorSegment2 data, uint count, uint depth, uint rows, uint columns, ArrayBasedLinearAlgebraProvider computationUnit) : base(data, count, depth, rows, columns, computationUnit)
        {
        }
    }
}
