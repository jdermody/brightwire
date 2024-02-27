﻿using BrightData.LinearAlgebra.ReadOnly;
using BrightData.LinearAlgebra.Segments;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra
{
    /// <summary>
    /// Row major 4D tensor
    /// </summary>
    /// <typeparam name="LAP"></typeparam>
    public class MutableTensor4D<LAP> : MutableTensorBase<ITensor4D, LAP>, ITensor4D
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
        public MutableTensor4D(INumericSegment<float> data, uint count, uint depth, uint rows, uint columns, LAP lap) : base(data, lap)
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
        public override ITensor4D Create(INumericSegment<float> segment) => new MutableTensor4D<LAP>(segment, Count, Depth, RowCount, ColumnCount, Lap);

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

        /// <inheritdoc />
        public sealed override uint[] Shape
        {
            get => [ColumnCount, RowCount, Depth, Count];
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

        IReadOnlyTensor3D IReadOnlyTensor4D.GetTensor(uint index) => GetTensorAsReadOnly(index);

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
        public virtual ITensor3D GetTensor(uint index)
        {
            var segment = new MutableTensorSegmentWrapper<float>(Segment, index * TensorSize, 1, TensorSize);
            return Lap.CreateTensor3D(Depth, RowCount, ColumnCount, segment);
        }

        /// <inheritdoc />
        public virtual ITensor4D AddPadding(uint padding)
        {
            using var temp = SpanOwner<ITensor3D>.Allocate((int)Count, AllocationMode.Default);
            var ptr = temp.Span;

            for (uint i = 0; i < Count; i++) {
                using var subTensor = GetTensor(i);
                ptr[(int)i] = subTensor.AddPadding(padding);
            }

            return Lap.CreateTensor4DAndThenDisposeInput(ptr);
        }

        /// <inheritdoc />
        public virtual ITensor4D RemovePadding(uint padding)
        {
            using var temp = SpanOwner<ITensor3D>.Allocate((int)Count, AllocationMode.Default);
            var ptr = temp.Span;

            for (uint i = 0; i < Count; i++) {
                using var subTensor = GetTensor(i);
                ptr[(int)i] = subTensor.RemovePadding(padding);
            }

            return Lap.CreateTensor4DAndThenDisposeInput(ptr);
        }

        /// <inheritdoc />
        public virtual (ITensor4D Result, ITensor4D? Indices) MaxPool(uint filterWidth, uint filterHeight, uint xStride, uint yStride, bool saveIndices)
        {
            var indexList = saveIndices
                ? new ITensor3D[Count]
                : null;
            using var temp = SpanOwner<ITensor3D>.Allocate((int)Count, AllocationMode.Default);
            var ptr = temp.Span;

            for (uint i = 0; i < Count; i++) {
                using var subTensor = GetTensor(i);
                var (result, indices) = subTensor.MaxPool(filterWidth, filterHeight, xStride, yStride, saveIndices);
                ptr[(int)i] = result;
                if (indexList != null && indices != null)
                    indexList[i] = indices;
            }

            return (Lap.CreateTensor4DAndThenDisposeInput(ptr), indexList != null ? Lap.CreateTensor4DAndThenDisposeInput(indexList) : null);
        }

        /// <inheritdoc />
        public virtual ITensor4D ReverseMaxPool(ITensor4D indices, uint outputRows, uint outputColumns, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
            using var temp = SpanOwner<ITensor3D>.Allocate((int)Count, AllocationMode.Default);
            var ptr = temp.Span;

            for (uint i = 0; i < Count; i++) {
                using var subTensor = GetTensor(i);
                using var indexTensor = indices.GetTensor(i);
                var result = subTensor.ReverseMaxPool(indexTensor, outputRows, outputColumns, filterWidth, filterHeight, xStride, yStride);
                ptr[(int)i] = result;
            }

            return Lap.CreateTensor4DAndThenDisposeInput(ptr);
        }

        /// <inheritdoc />
        public virtual ITensor3D Im2Col(uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
            using var temp = SpanOwner<IMatrix>.Allocate((int)Count, AllocationMode.Default);
            var ptr = temp.Span;

            for (uint i = 0; i < Count; i++) {
                using var subTensor = GetTensor(i);
                ptr[(int)i] = subTensor.Im2Col(filterWidth, filterHeight, xStride, yStride);
            }

            return Lap.CreateTensor3DAndThenDisposeInput(ptr);
        }

        /// <inheritdoc />
        public virtual ITensor4D ReverseIm2Col(IMatrix filter, uint outputRows, uint outputColumns, uint outputDepth, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
            using var temp = SpanOwner<ITensor3D>.Allocate((int)Count, AllocationMode.Default);
            var ptr = temp.Span;

            for (uint i = 0; i < Count; i++) {
                using var subTensor = GetTensor(i);
                ptr[(int)i] = subTensor.ReverseIm2Col(filter, outputRows, outputColumns, outputDepth, filterWidth, filterHeight, xStride, yStride);
            }

            return Lap.CreateTensor4DAndThenDisposeInput(ptr);
        }

        /// <inheritdoc />
        public virtual IVector ColumnSums()
        {
            IVector? ret = null;
            for (uint i = 0, count = Count; i < count; i++) {
                using var subTensor = GetTensor(i);
                using var tensorAsMatrix = subTensor.Reshape(subTensor.RowCount * subTensor.ColumnCount, subTensor.Depth);
                var columnSums = tensorAsMatrix.ColumnSums();
                if (ret == null)
                    ret = columnSums;
                else {
                    ret.AddInPlace(columnSums);
                    columnSums.Dispose();
                }
            }
            return ret ?? Lap.CreateVector(Depth, true);
        }

        /// <summary>
        /// Returns a read only tensor
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public IReadOnlyTensor3D GetTensorAsReadOnly(uint index) => new ReadOnlyTensor3D(Tensor(index), Depth, RowCount, ColumnCount);
        MutableTensorSegmentWrapper<float> Tensor(uint index) => new(Segment, index * TensorSize, 1, TensorSize);

        /// <inheritdoc />
        public override string ToString() => $"Tensor4D (Count: {Count}, Depth: {Depth}, Rows: {RowCount}, Columns: {ColumnCount})";


        /// <inheritdoc />
        public ITensor4D Create(LinearAlgebraProvider lap) => lap.CreateTensor4D(Count, Depth, RowCount, ColumnCount, Segment);
    }

    /// <summary>
    /// 4D tensor
    /// </summary>
    public class BrightTensor4D : MutableTensor4D<LinearAlgebraProvider>
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
        public BrightTensor4D(INumericSegment<float> data, uint count, uint depth, uint rows, uint columns, LinearAlgebraProvider lap) : base(data, count, depth, rows, columns, lap)
        {
        }
    }
}
