namespace BrightData.LinearAlegbra2
{
    public class Tensor4D2<CU> : TensorBase2<ITensor4D, CU>, ITensor4D
        where CU: ComputationUnit
    {
        public Tensor4D2(ITensorSegment2 data, uint count, uint depth, uint rows, uint columns, CU computationUnit) : base(data, computationUnit)
        {
            Count = count;
            Depth = depth;
            RowCount = rows;
            ColumnCount = columns;
            MatrixSize = RowCount * ColumnCount;
            TensorSize = MatrixSize * Depth;
            Size = TensorSize * Count;
        }

        public override ITensor4D Create(ITensorSegment2 segment) => new Tensor4D2<CU>(segment, Count, Depth, RowCount, ColumnCount, _computationUnit);

        public uint Count { get; private set; }
        public uint Depth { get; private set; }
        public uint RowCount { get; private set; }
        public uint ColumnCount { get; private set; }
        public uint MatrixSize { get; private set; }
        public uint TensorSize { get; private set; }
        public override uint Size { get; protected set; }
        public override uint[] Shape
        {
            get => new[] { Count, Depth, RowCount, ColumnCount };
            protected set
            {
                ColumnCount = value[0];
                RowCount = value[1];
                Depth = value[2];
                Count = value[3];
                MatrixSize = RowCount * ColumnCount;
                TensorSize = MatrixSize * Depth;
                Size = TensorSize * Count;
            }
        }

        public float this[int count, int depth, int rowY, int columnX]
        {
            get => Segment[count * TensorSize + depth * MatrixSize + rowY * ColumnCount + columnX];
            set => Segment[count * TensorSize + depth * MatrixSize + rowY * ColumnCount + columnX] = value;
        }
        public float this[uint count, uint depth, uint rowY, uint columnX]
        {
            get => Segment[count * TensorSize + depth * MatrixSize + rowY * ColumnCount + columnX];
            set => Segment[count * TensorSize + depth * MatrixSize + rowY * ColumnCount + columnX] = value;
        }
        public float this[long count, long depth, long rowY, long columnX]
        {
            get => Segment[count * TensorSize + depth * MatrixSize + rowY * ColumnCount + columnX];
            set => Segment[count * TensorSize + depth * MatrixSize + rowY * ColumnCount + columnX] = value;
        }
        public float this[ulong count, ulong depth, ulong rowY, ulong columnX]
        {
            get => Segment[count * TensorSize + depth * MatrixSize + rowY * ColumnCount + columnX];
            set => Segment[count * TensorSize + depth * MatrixSize + rowY * ColumnCount + columnX] = value;
        }

        public ITensor3D Tensor(uint index) => _computationUnit.GetTensor(this, index);
        public ITensor4D AddPadding(uint padding) => _computationUnit.AddPadding(this, padding);
        public ITensor4D RemovePadding(uint padding) => _computationUnit.RemovePadding(this, padding);
        public (ITensor4D Result, ITensor4D? Indices) MaxPool(uint filterWidth, uint filterHeight, uint xStride, uint yStride, bool saveIndices) => _computationUnit.MaxPool(this, filterWidth, filterHeight, xStride, yStride, saveIndices);
        public ITensor4D ReverseMaxPool(ITensor4D indices, uint outputRows, uint outputColumns, uint filterWidth, uint filterHeight, uint xStride, uint yStride) => _computationUnit.ReverseMaxPool(this, indices, outputRows, outputColumns, filterWidth, filterHeight, xStride, yStride);
        public ITensor3D Im2Col(uint filterWidth, uint filterHeight, uint xStride, uint yStride) => _computationUnit.Im2Col(this, filterWidth, filterHeight, xStride, yStride);
        public ITensor4D ReverseIm2Col(IMatrix filter, uint outputRows, uint outputColumns, uint outputDepth, uint filterWidth, uint filterHeight, uint xStride, uint yStride) => _computationUnit.ReverseIm2Col(this, filter, outputRows, outputColumns, outputDepth, filterWidth, filterHeight, xStride, yStride);

        public override string ToString() => $"Tensor4D (Count: {Count}, Depth: {Depth}, Rows: {RowCount}, Columns: {ColumnCount})";
    }

    public class Tensor4D2 : Tensor4D2<ComputationUnit>
    {
        public Tensor4D2(ITensorSegment2 data, uint count, uint depth, uint rows, uint columns, ComputationUnit computationUnit) : base(data, count, depth, rows, columns, computationUnit)
        {
        }
    }
}
