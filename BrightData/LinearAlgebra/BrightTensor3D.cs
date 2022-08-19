using System;
using System.Linq;
using BrightData.LinearAlgebra.ReadOnly;

namespace BrightData.LinearAlgebra
{
    public class BrightTensor3D<LAP> : BrightTensorBase<ITensor3D, LAP>, ITensor3D
        where LAP: LinearAlgebraProvider
    {
        public BrightTensor3D(ITensorSegment data, uint depth, uint rowCount, uint columnCount, LAP lap) : base(data, lap)
        {
            Depth = depth;
            RowCount = rowCount;
            ColumnCount = columnCount;
            MatrixSize = rowCount * columnCount;
            TotalSize = MatrixSize * depth;
#if  DEBUG
            if (data.Size != TotalSize)
                throw new ArgumentException("Sizes do not align");
#endif
        }

        public override ITensor3D Create(ITensorSegment segment) => new BrightTensor3D<LAP>(segment, Depth, RowCount, ColumnCount, _lap);
        ITensor3D IReadOnlyTensor3D.Create(LinearAlgebraProvider lap) => lap.CreateTensor3DAndThenDisposeInput(Depth.AsRange().Select(GetMatrix).ToArray());
        IReadOnlyMatrix IReadOnlyTensor3D.GetReadOnlyMatrix(uint index) => new ReadOnlyMatrixWrapper(Matrix(index), RowCount, ColumnCount);
        TensorSegmentWrapper Matrix(uint index) => new(Segment, index * MatrixSize, 1, MatrixSize);
        public uint Depth { get; private set; }
        public uint RowCount { get; private set; }
        public uint ColumnCount { get; private set; }
        public uint MatrixSize { get; private set; }
        public sealed override uint TotalSize { get; protected set; }
        public sealed override uint[] Shape
        {
            get => new[] { ColumnCount, RowCount, Depth };
            protected set
            {
                ColumnCount = value[0];
                RowCount = value[1];
                Depth = value[2];
                MatrixSize = RowCount * ColumnCount;
                TotalSize = MatrixSize * Depth;
            }
        }

        public float this[int depth, int rowY, int columnX]
        {
            get => Segment[depth * MatrixSize + columnX * RowCount + rowY];
            set => Segment[depth * MatrixSize + columnX * RowCount + rowY] = value;
        }
        public float this[uint depth, uint rowY, uint columnX]
        {
            get => Segment[depth * MatrixSize + columnX * RowCount + rowY];
            set => Segment[depth * MatrixSize + columnX * RowCount + rowY] = value;
        }
        public float this[long depth, long rowY, long columnX]
        {
            get => Segment[depth * MatrixSize + columnX * RowCount + rowY];
            set => Segment[depth * MatrixSize + columnX * RowCount + rowY] = value;
        }
        public float this[ulong depth, ulong rowY, ulong columnX]
        {
            get => Segment[depth * MatrixSize + columnX * RowCount + rowY];
            set => Segment[depth * MatrixSize + columnX * RowCount + rowY] = value;
        }
        public IReadOnlyMatrix[] AllMatrices()
        {
            var ret = new IReadOnlyMatrix[Depth];
            var _this = (IReadOnlyTensor3D)this;
            for (uint i = 0; i < Depth; i++)
                ret[i] = _this.GetReadOnlyMatrix(i);
            return ret;
        }
        public IMatrix GetMatrix(uint index) => _lap.GetMatrix(this, index);
        public ITensor3D AddPadding(uint padding) => _lap.AddPadding(this, padding);
        public ITensor3D RemovePadding(uint padding) => _lap.RemovePadding(this, padding);
        public IMatrix Im2Col(uint filterWidth, uint filterHeight, uint xStride, uint yStride) => _lap.Im2Col(this, filterWidth, filterHeight, xStride, yStride);
        public (ITensor3D Result, ITensor3D? Indices) MaxPool(uint filterWidth, uint filterHeight, uint xStride, uint yStride, bool saveIndices) => _lap.MaxPool(this, filterWidth, filterHeight, xStride, yStride, saveIndices);
        public ITensor3D ReverseMaxPool(ITensor3D indices, uint outputRows, uint outputColumns, uint filterWidth, uint filterHeight, uint xStride, uint yStride) => _lap.ReverseMaxPool(this, indices, outputRows, outputColumns, filterWidth, filterHeight, xStride, yStride);
        public ITensor3D ReverseIm2Col(IMatrix filter, uint outputRows, uint outputColumns, uint outputDepth, uint filterWidth, uint filterHeight, uint xStride, uint yStride) => _lap.ReverseIm2Col(this, filter, outputRows, outputColumns, outputDepth, filterWidth, filterHeight, xStride, yStride);
        public IMatrix AddAllMatrices() => _lap.AddMatrices(this);
        public ITensor3D MultiplyEachMatrixBy(IMatrix matrix) => _lap.Multiply(this, matrix);
        public ITensor3D TransposeAndMultiplyEachMatrixBy(IMatrix matrix) => _lap.TransposeFirstAndMultiply(this, matrix);
        public void AddToEachRow(IVector vector) => _lap.AddToEachRow(this, vector);
        public void AddToEachColumn(IVector vector) => _lap.AddToEachColumn(this, vector);
        public ITensor3D Multiply(ITensor4D other) => _lap.Multiply(this, other);
        public ITensor3D TransposeAndMultiply(ITensor4D other) => _lap.TransposeSecondAndMultiply(this, other);

        public ITensor3D TransposeThisAndMultiply(ITensor4D other) => _lap.TransposeFirstAndMultiply(this, other);
        public override string ToString() => $"Tensor3D (Depth: {Depth}, Rows: {RowCount}, Columns: {ColumnCount})";
    }

    public class BrightTensor3D : BrightTensor3D<LinearAlgebraProvider>
    {
        public BrightTensor3D(ITensorSegment data, uint depth, uint rows, uint columns, LinearAlgebraProvider computationUnit) : base(data, depth, rows, columns, computationUnit)
        {
        }
    }
}
