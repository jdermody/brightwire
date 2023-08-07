using BrightData.LinearAlgebra.ReadOnly;
using BrightData.LinearAlgebra.Segments;
using System.Linq;

namespace BrightData.LinearAlgebra
{
    /// <summary>
    /// Row major 4D tensor
    /// </summary>
    /// <typeparam name="LAP"></typeparam>
    public class BrightTensor4D<LAP> : BrightTensorBase<ITensor4D, LAP>, ITensor4D, IReadOnlyTensor4D
        where LAP: LinearAlgebraProvider
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Tensor segment</param>
        /// <param name="count">Number of 3D tensors</param>
        /// <param name="depth">Number of matrices in each 3D tensor</param>
        /// <param name="rows">Number of rows in each matrix</param>
        /// <param name="columns">Number of columns in each matrix</param>
        /// <param name="lap">Linear algebra provider</param>
        public BrightTensor4D(ITensorSegment data, uint count, uint depth, uint rows, uint columns, LAP lap) : base(data, lap)
        {
            Count = count;
            Depth = depth;
            RowCount = rows;
            ColumnCount = columns;
            MatrixSize = RowCount * ColumnCount;
            TensorSize = MatrixSize * Depth;
            TotalSize = TensorSize * Count;
        }
        
        /// <inheritdoc />
        public override ITensor4D Create(ITensorSegment segment) => new BrightTensor4D<LAP>(segment, Count, Depth, RowCount, ColumnCount, Lap);

        /// <inheritdoc />
        public ITensor4D Clone(LinearAlgebraProvider? lap) => (lap ?? LinearAlgebraProvider).CreateTensor4DAndThenDisposeInput(Count.AsRange().Select(GetTensor).ToArray());

        /// <inheritdoc />
        public uint Count { get; private set; }

        /// <inheritdoc />
        public uint Depth { get; private set; }

        /// <inheritdoc />
        public uint RowCount { get; private set; }

        /// <inheritdoc />
        public uint ColumnCount { get; private set; }

        /// <inheritdoc />
        public uint MatrixSize { get; private set; }

        /// <inheritdoc />
        public uint TensorSize { get; private set; }

        /// <inheritdoc />
        public sealed override uint TotalSize { get; protected set; }

        /// <inheritdoc cref="ITensor4D" />
        public bool IsReadOnly => false;

        /// <inheritdoc />
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

        /// <summary>
        /// Returns a value from the 4D tensor
        /// </summary>
        /// <param name="count">3D tensor index</param>
        /// <param name="depth">Matrix index</param>
        /// <param name="rowY">Row index</param>
        /// <param name="columnX">Column index</param>
        public float this[int count, int depth, int rowY, int columnX]
        {
            get => Segment[count * TensorSize + depth * MatrixSize + columnX * RowCount + rowY];
            set => Segment[count * TensorSize + depth * MatrixSize + columnX * RowCount + rowY] = value;
        }

        /// <summary>
        /// Returns a value from the 4D tensor
        /// </summary>
        /// <param name="count">3D tensor index</param>
        /// <param name="depth">Matrix index</param>
        /// <param name="rowY">Row index</param>
        /// <param name="columnX">Column index</param>
        public float this[uint count, uint depth, uint rowY, uint columnX]
        {
            get => Segment[count * TensorSize + depth * MatrixSize + columnX * RowCount + rowY];
            set => Segment[count * TensorSize + depth * MatrixSize + columnX * RowCount + rowY] = value;
        }

        IReadOnlyTensor3D IReadOnlyTensor4D.GetTensor3D(uint index) => GetTensorAsReadOnly(index);
        IReadOnlyTensor3D[] IReadOnlyTensor4D.AllTensors() => AllTensorsAsReadOnly();

        /// <summary>
        /// Returns a value from the 4D tensor
        /// </summary>
        /// <param name="count">3D tensor index</param>
        /// <param name="depth">Matrix index</param>
        /// <param name="rowY">Row index</param>
        /// <param name="columnX">Column index</param>
        public float this[long count, long depth, long rowY, long columnX]
        {
            get => Segment[count * TensorSize + depth * MatrixSize + columnX * RowCount + rowY];
            set => Segment[count * TensorSize + depth * MatrixSize + columnX * RowCount + rowY] = value;
        }

        /// <summary>
        /// Returns a value from the 4D tensor
        /// </summary>
        /// <param name="count">3D tensor index</param>
        /// <param name="depth">Matrix index</param>
        /// <param name="rowY">Row index</param>
        /// <param name="columnX">Column index</param>
        public float this[ulong count, ulong depth, ulong rowY, ulong columnX]
        {
            get => Segment[count * TensorSize + depth * MatrixSize + columnX * RowCount + rowY];
            set => Segment[count * TensorSize + depth * MatrixSize + columnX * RowCount + rowY] = value;
        }

        /// <inheritdoc />
        public ITensor3D GetTensor(uint index) => Lap.GetTensor(this, index);

        /// <inheritdoc />
        public ITensor4D AddPadding(uint padding) => Lap.AddPadding(this, padding);

        /// <inheritdoc />
        public ITensor4D RemovePadding(uint padding) => Lap.RemovePadding(this, padding);

        /// <inheritdoc />
        public (ITensor4D Result, ITensor4D? Indices) MaxPool(uint filterWidth, uint filterHeight, uint xStride, uint yStride, bool saveIndices) => Lap.MaxPool(this, filterWidth, filterHeight, xStride, yStride, saveIndices);

        /// <inheritdoc />
        public ITensor4D ReverseMaxPool(ITensor4D indices, uint outputRows, uint outputColumns, uint filterWidth, uint filterHeight, uint xStride, uint yStride) => Lap.ReverseMaxPool(this, indices, outputRows, outputColumns, filterWidth, filterHeight, xStride, yStride);

        /// <inheritdoc />
        public ITensor3D Im2Col(uint filterWidth, uint filterHeight, uint xStride, uint yStride) => Lap.Im2Col(this, filterWidth, filterHeight, xStride, yStride);

        /// <inheritdoc />
        public ITensor4D ReverseIm2Col(IMatrix filter, uint outputRows, uint outputColumns, uint outputDepth, uint filterWidth, uint filterHeight, uint xStride, uint yStride) => Lap.ReverseIm2Col(this, filter, outputRows, outputColumns, outputDepth, filterWidth, filterHeight, xStride, yStride);

        /// <inheritdoc />
        public IVector ColumnSums() => Lap.ColumnSums(this);

        /// <inheritdoc />
        public IVector RowSums() => Lap.ColumnSums(this);

        /// <inheritdoc />
        public IReadOnlyTensor3D GetTensorAsReadOnly(uint index) => new ReadOnlyTensor3DWrapper(Tensor(index), Depth, RowCount, ColumnCount);
        TensorSegmentWrapper Tensor(uint index) => new(Segment, index * TensorSize, 1, TensorSize);

        /// <inheritdoc />
        public IReadOnlyTensor3D[] AllTensorsAsReadOnly()
        {
            var ret = new IReadOnlyTensor3D[Count];
            for (uint i = 0; i < Depth; i++)
                ret[i] = GetTensorAsReadOnly(i);
            return ret;
        }

        /// <inheritdoc />
        public override string ToString() => $"Tensor4D (Count: {Count}, Depth: {Depth}, Rows: {RowCount}, Columns: {ColumnCount})";


        /// <inheritdoc />
        public ITensor4D Create(LinearAlgebraProvider lap) => lap.CreateTensor4D(Count, Depth, RowCount, ColumnCount, Segment);
    }

    /// <summary>
    /// 4D tensor
    /// </summary>
    public class BrightTensor4D : BrightTensor4D<LinearAlgebraProvider>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Tensor segment</param>
        /// <param name="count">Number of 3D tensors</param>
        /// <param name="depth">Number of matrices in each 3D tensor</param>
        /// <param name="rows">Number of rows in each matrix</param>
        /// <param name="columns">Number of columns in each matrix</param>
        /// <param name="lap">Linear algebra provider</param>
        public BrightTensor4D(ITensorSegment data, uint count, uint depth, uint rows, uint columns, LinearAlgebraProvider lap) : base(data, count, depth, rows, columns, lap)
        {
        }
    }
}
