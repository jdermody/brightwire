using System;
using System.Linq;
using BrightData.LinearAlgebra.ReadOnly;
using BrightData.LinearAlgebra.Segments;

namespace BrightData.LinearAlgebra
{
    /// <summary>
    /// Row major 3D tensor
    /// </summary>
    /// <typeparam name="LAP"></typeparam>
    public class BrightTensor3D<LAP> : BrightTensorBase<ITensor3D, LAP>, ITensor3D, IReadOnlyTensor3D
        where LAP: LinearAlgebraProvider
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Tensor segment</param>
        /// <param name="depth">Number of matrices</param>
        /// <param name="rows">Rows in each matrix</param>
        /// <param name="columns">Columns in each matrix</param>
        /// <param name="lap">Linear algebra provider</param>
        /// <exception cref="ArgumentException"></exception>
        public BrightTensor3D(INumericSegment<float> data, uint depth, uint rows, uint columns, LAP lap) : base(data, lap)
        {
            Depth = depth;
            RowCount = rows;
            ColumnCount = columns;
            MatrixSize = rows * columns;
            TotalSize = MatrixSize * depth;
#if  DEBUG
            if (data.Size != TotalSize)
                throw new ArgumentException("Sizes do not align");
#endif
        }

        /// <inheritdoc />
        public override ITensor3D Create(INumericSegment<float> segment) => new BrightTensor3D<LAP>(segment, Depth, RowCount, ColumnCount, Lap);

        /// <inheritdoc />
        public ITensor3D Clone(LinearAlgebraProvider? lap) => (lap ?? LinearAlgebraProvider).CreateTensor3DAndThenDisposeInput(Depth.AsRange().Select(GetMatrix).ToArray());

        /// <inheritdoc />
        public IReadOnlyMatrix GetMatrixAsReadOnly(uint index) => new ReadOnlyMatrixWrapper(Matrix(index), RowCount, ColumnCount);
        TensorSegmentWrapper Matrix(uint index) => new(Segment, index * MatrixSize, 1, MatrixSize);

        /// <inheritdoc />
        public uint Depth { get; private set; }

        /// <inheritdoc />
        public uint RowCount { get; private set; }

        /// <inheritdoc />
        public uint ColumnCount { get; private set; }

        /// <inheritdoc />
        public uint MatrixSize { get; private set; }

        /// <inheritdoc />
        public sealed override uint TotalSize { get; protected set; }

        /// <inheritdoc />
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

        /// <summary>
        /// Returns a value from the tensor
        /// </summary>
        /// <param name="depth">Matrix index</param>
        /// <param name="rowY">Row index</param>
        /// <param name="columnX">Column index</param>
        public float this[int depth, int rowY, int columnX]
        {
            get => Segment[depth * MatrixSize + columnX * RowCount + rowY];
            set => Segment[depth * MatrixSize + columnX * RowCount + rowY] = value;
        }

        /// <summary>
        /// Returns a value from the tensor
        /// </summary>
        /// <param name="depth">Matrix index</param>
        /// <param name="rowY">Row index</param>
        /// <param name="columnX">Column index</param>
        public float this[uint depth, uint rowY, uint columnX]
        {
            get => Segment[depth * MatrixSize + columnX * RowCount + rowY];
            set => Segment[depth * MatrixSize + columnX * RowCount + rowY] = value;
        }

        IReadOnlyMatrix IReadOnlyTensor3D.GetMatrix(uint index) => GetMatrixAsReadOnly(index);
        IReadOnlyMatrix[] IReadOnlyTensor3D.AllMatrices() => AllMatricesAsReadOnly();

        /// <summary>
        /// Returns a value from the tensor
        /// </summary>
        /// <param name="depth">Matrix index</param>
        /// <param name="rowY">Row index</param>
        /// <param name="columnX">Column index</param>
        public float this[long depth, long rowY, long columnX]
        {
            get => Segment[depth * MatrixSize + columnX * RowCount + rowY];
            set => Segment[depth * MatrixSize + columnX * RowCount + rowY] = value;
        }

        /// <summary>
        /// Returns a value from the tensor
        /// </summary>
        /// <param name="depth">Matrix index</param>
        /// <param name="rowY">Row index</param>
        /// <param name="columnX">Column index</param>
        public float this[ulong depth, ulong rowY, ulong columnX]
        {
            get => Segment[depth * MatrixSize + columnX * RowCount + rowY];
            set => Segment[depth * MatrixSize + columnX * RowCount + rowY] = value;
        }

        /// <inheritdoc />
        public IReadOnlyMatrix[] AllMatricesAsReadOnly()
        {
            var ret = new IReadOnlyMatrix[Depth];
            for (uint i = 0; i < Depth; i++)
                ret[i] = GetMatrixAsReadOnly(i);
            return ret;
        }

        /// <inheritdoc />
        public IMatrix GetMatrix(uint index) => Lap.GetMatrix(this, index);

        /// <inheritdoc />
        public ITensor3D AddPadding(uint padding) => Lap.AddPadding(this, padding);

        /// <inheritdoc />
        public ITensor3D RemovePadding(uint padding) => Lap.RemovePadding(this, padding);

        /// <inheritdoc />
        public IMatrix Im2Col(uint filterWidth, uint filterHeight, uint xStride, uint yStride) => Lap.Im2Col(this, filterWidth, filterHeight, xStride, yStride);

        /// <inheritdoc />
        public (ITensor3D Result, ITensor3D? Indices) MaxPool(uint filterWidth, uint filterHeight, uint xStride, uint yStride, bool saveIndices) => Lap.MaxPool(this, filterWidth, filterHeight, xStride, yStride, saveIndices);

        /// <inheritdoc />
        public ITensor3D ReverseMaxPool(ITensor3D indices, uint outputRows, uint outputColumns, uint filterWidth, uint filterHeight, uint xStride, uint yStride) => Lap.ReverseMaxPool(this, indices, outputRows, outputColumns, filterWidth, filterHeight, xStride, yStride);

        /// <inheritdoc />
        public ITensor3D ReverseIm2Col(IMatrix filter, uint outputRows, uint outputColumns, uint outputDepth, uint filterWidth, uint filterHeight, uint xStride, uint yStride) => Lap.ReverseIm2Col(this, filter, outputRows, outputColumns, outputDepth, filterWidth, filterHeight, xStride, yStride);

        /// <inheritdoc />
        public IMatrix AddAllMatrices() => Lap.AddMatrices(this);

        /// <inheritdoc />
        public ITensor3D MultiplyEachMatrixBy(IMatrix matrix) => Lap.Multiply(this, matrix);

        /// <inheritdoc />
        public ITensor3D TransposeAndMultiplyEachMatrixBy(IMatrix matrix) => Lap.TransposeFirstAndMultiply(this, matrix);

        /// <inheritdoc />
        public void AddToEachRow(IVector vector) => Lap.AddToEachRow(this, vector);

        /// <inheritdoc />
        public void AddToEachColumn(IVector vector) => Lap.AddToEachColumn(this, vector);

        /// <inheritdoc />
        public ITensor3D Multiply(ITensor4D other) => Lap.Multiply(this, other);

        /// <inheritdoc />
        public ITensor3D TransposeAndMultiply(ITensor4D other) => Lap.TransposeSecondAndMultiply(this, other);

        /// <inheritdoc />
        public ITensor3D TransposeThisAndMultiply(ITensor4D other) => Lap.TransposeFirstAndMultiply(this, other);

        /// <inheritdoc />
        public override string ToString() => $"Tensor3D (Depth: {Depth}, Rows: {RowCount}, Columns: {ColumnCount})";

        /// <inheritdoc />
        public ITensor3D Create(LinearAlgebraProvider lap) => lap.CreateTensor3D(Depth, RowCount, ColumnCount, (IReadOnlyNumericSegment<float>)Segment);
    }

    /// <summary>
    /// 3D tensor 
    /// </summary>
    public class BrightTensor3D : BrightTensor3D<LinearAlgebraProvider>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Tensor segment</param>
        /// <param name="depth">Number of matrices</param>
        /// <param name="rows">Rows in each matrix</param>
        /// <param name="columns">Columns in each matrix</param>
        /// <param name="lap">Linear algebra provider</param>
        public BrightTensor3D(INumericSegment<float> data, uint depth, uint rows, uint columns, LinearAlgebraProvider lap) : base(data, depth, rows, columns, lap)
        {
        }
    }
}
