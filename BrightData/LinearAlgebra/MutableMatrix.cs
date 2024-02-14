using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using BrightData.LinearAlgebra.Segments;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra
{
    /// <summary>
    /// Row major matrix type
    /// </summary>
    /// <typeparam name="LAP"></typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="data">Tensor segment</param>
    /// <param name="rows">Number of rows</param>
    /// <param name="columns">Number of columns</param>
    /// <param name="lap">Linear algebra provider</param>
    public class MutableMatrix<LAP>(INumericSegment<float> data, uint rows, uint columns, LAP lap) : MutableTensorBase<IMatrix, LAP>(data, lap), IMatrix
        where LAP: LinearAlgebraProvider
    {
        /// <inheritdoc />
        public uint RowCount { get; private set; } = rows;

        /// <inheritdoc />
        public uint ColumnCount { get; private set; } = columns;

        /// <inheritdoc />
        public sealed override uint TotalSize { get; protected set; } = rows * columns;

        /// <inheritdoc />
        public sealed override uint[] Shape
        {
            get => [ColumnCount, RowCount];
            protected set
            {
                ColumnCount = value[0];
                RowCount = value[1];
                TotalSize = RowCount * ColumnCount;
            }
        }

        /// <inheritdoc cref="IMatrix" />
        public float this[int rowY, int columnX]
        {
            get => Segment[columnX * RowCount + rowY];
            set => Segment[columnX * RowCount + rowY] = value;
        }

        /// <inheritdoc cref="IMatrix" />
        public float this[uint rowY, uint columnX]
        {
            get => Segment[columnX * RowCount + rowY];
            set => Segment[columnX * RowCount + rowY] = value;
        }

        /// <inheritdoc />
        public float this[long rowY, long columnX]
        {
            get => Segment[columnX * RowCount + rowY];
            set => Segment[columnX * RowCount + rowY] = value;
        }

        /// <inheritdoc />
        public float this[ulong rowY, ulong columnX]
        {
            get => Segment[columnX * RowCount + rowY];
            set => Segment[columnX * RowCount + rowY] = value;
        }

        /// <inheritdoc />
        public INumericSegment<float> GetRow(uint index)
        {
            if(index > RowCount)
                throw new ArgumentOutOfRangeException(nameof(index), $"Number of rows is {RowCount} but index {index} was requested");
            return new MutableTensorSegmentWrapper(Segment, index, RowCount, ColumnCount);
        }

        /// <inheritdoc />
        public IReadOnlyNumericSegment<float> GetReadOnlyRow(uint index)
        {
            if(index > RowCount)
                throw new ArgumentOutOfRangeException(nameof(index), $"Number of rows is {RowCount} but index {index} was requested");
            return new ReadOnlyTensorSegmentWrapper(Segment, index, RowCount, ColumnCount);
        }

        /// <inheritdoc />
        public INumericSegment<float> GetColumn(uint index)
        {
            if(index > ColumnCount)
                throw new ArgumentOutOfRangeException(nameof(index), $"Number of columns is {ColumnCount} but index {index} was requested");
            return new MutableTensorSegmentWrapper(Segment, index * RowCount, 1, RowCount);
        }

        /// <inheritdoc />
        public IReadOnlyNumericSegment<float> GetReadOnlyColumn(uint index)
        {
            if(index > ColumnCount)
                throw new ArgumentOutOfRangeException(nameof(index), $"Number of columns is {ColumnCount} but index {index} was requested");
            return new ReadOnlyTensorSegmentWrapper(Segment, index * RowCount, 1, RowCount);
        }

        /// <inheritdoc />
        public unsafe ReadOnlySpan<float> GetRowSpan(uint rowIndex, ref SpanOwner<float> temp)
        {
            temp = SpanOwner<float>.Allocate((int)TotalSize);
            var span = temp.Span;
            fixed (float* ptr = &MemoryMarshal.GetReference(span)) {
                Segment.CopyTo(ptr, (int)rowIndex * (int)RowCount, (int)RowCount, (int)ColumnCount);
            }
            return span;
        }

        /// <inheritdoc />
        public ReadOnlySpan<float> GetColumnSpan(uint columnIndex)
        {
            var ret = Segment.Contiguous!.ReadOnlySpan;
            return ret.Slice((int)(columnIndex * RowCount), (int)RowCount);
        }

        /// <inheritdoc />
        public override IMatrix Create(INumericSegment<float> segment) => Lap.CreateMatrix(RowCount, ColumnCount, segment);

        /// <inheritdoc />
        public virtual IVector GetRowVector(uint index) => Lap.CreateVector(GetRow(index));

        /// <inheritdoc />
        public virtual IVector GetColumnVector(uint index) => Lap.CreateVector(GetColumn(index));

        /// <inheritdoc />
        public IMatrix Transpose() => Lap.Transpose(this);

        /// <inheritdoc />
        public IMatrix Multiply(IMatrix other) => Lap.Multiply(this, other);

        /// <inheritdoc />
        public IVector GetDiagonal() => Lap.GetDiagonal(this);

        /// <inheritdoc />
        public IVector RowSums() => Lap.RowSums(this);

        /// <inheritdoc />
        public IVector ColumnSums() => Lap.ColumnSums(this);

        /// <inheritdoc />
        public IVector Multiply(IVector vector) => Lap.Multiply(this, vector);

        /// <inheritdoc />
        public IMatrix TransposeAndMultiply(IMatrix other) => Lap.TransposeSecondAndMultiply(this, other);

        /// <inheritdoc />
        public IMatrix TransposeThisAndMultiply(IMatrix other) => Lap.TransposeFirstAndMultiply(this, other);

        /// <inheritdoc />
        public IMatrix MapIndexed(Func<uint, uint, float, float> mutator)
        {
            var ret = Lap.MapParallel(Segment, (ind, val) => {
                var i = ind % RowCount;
                var j = ind / RowCount;
                return mutator(i, j, val);
            });
            return Lap.CreateMatrix(RowCount, ColumnCount, ret);
        }

        /// <inheritdoc />
        public void MapIndexedInPlace(Func<uint, uint, float, float> mutator)
        {
            var ret = Lap.MapParallel(Segment, (ind, val) => {
                var i = ind % RowCount;
                var j = ind / RowCount;
                return mutator(i, j, val);
            });
            try {
                ret.CopyTo(Segment);
            }
            finally {
                ret.Release();
            }
        }

        /// <inheritdoc />
        public (IMatrix Left, IMatrix Right) SplitAtColumn(uint columnIndex) => Lap.SplitAtColumn(this, columnIndex);

        /// <inheritdoc />
        public (IMatrix Top, IMatrix Bottom) SplitAtRow(uint rowIndex) => Lap.SplitAtRow(this, rowIndex);

        /// <inheritdoc />
        public IMatrix ConcatBelow(IMatrix bottom) => Lap.ConcatColumns(this, bottom);

        /// <inheritdoc />
        public IMatrix ConcatRight(IMatrix right) => Lap.ConcatRows(this, right);

        /// <inheritdoc />
        public (IMatrix U, IVector S, IMatrix VT) Svd() => Lap.Svd(this);

        /// <inheritdoc />
        public IMatrix GetNewMatrixFromRows(IEnumerable<uint> rowIndices) => Lap.GetNewMatrixFromRows(this, rowIndices);

        /// <inheritdoc />
        public IMatrix GetNewMatrixFromColumns(IEnumerable<uint> columnIndices) => Lap.GetNewMatrixFromColumns(this, columnIndices);

        /// <inheritdoc />
        public void AddToEachRow(INumericSegment<float> segment) => Lap.AddToEachRow(this, segment);

        /// <inheritdoc />
        public void AddToEachColumn(INumericSegment<float> segment) => Lap.AddToEachColumn(this, segment);

        /// <inheritdoc />
        public void MultiplyEachRowWith(INumericSegment<float> segment) => Lap.MultiplyEachRowWith(this, segment);

        /// <inheritdoc />
        public void MultiplyEachColumnWith(INumericSegment<float> segment) => Lap.MultiplyEachColumnWith(this, segment);

        /// <inheritdoc />
        public INumericSegment<float>[] SoftmaxPerRow()
        {
            using var segments = SpanOwner<INumericSegment<float>>.Allocate((int)RowCount);
            var ptr = segments.Span;
            for (var i = 0; i < RowCount; i++)
                ptr[i] = GetRow((uint)i);
            return Lap.MultiSoftmax(segments.DangerousGetArray());
        }

        /// <inheritdoc />
        public INumericSegment<float>[] SoftmaxDerivativePerRow(INumericSegment<float>[] rows) => Lap.SoftmaxDerivativePerRow(this, rows);


        /// <inheritdoc />
        public override string ToString()
        {
            var preview = String.Join("|", Segment.Values.Take(Consts.DefaultPreviewSize));
            if (TotalSize > Consts.DefaultPreviewSize)
                preview += "|...";
            return $"Matrix (Rows: {RowCount}, Columns: {ColumnCount}) {preview}";
        }

        /// <inheritdoc />
        public IMatrix Create(LinearAlgebraProvider lap) => lap.CreateMatrix((IReadOnlyMatrix)this);
    }

    /// <summary>
    /// Matrix type
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="data">Tensor segment</param>
    /// <param name="rows">Number of rows</param>
    /// <param name="columns">Number of columns</param>
    /// <param name="lap">Linear algebra provider</param>
    public class BrightMatrix(INumericSegment<float> data, uint rows, uint columns, LinearAlgebraProvider lap) : MutableMatrix<LinearAlgebraProvider>(data, rows, columns, lap);
}
