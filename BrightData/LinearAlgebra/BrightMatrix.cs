using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using BrightData.LinearAlgebra.ReadOnly;
using BrightData.LinearAlgebra.Segments;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra
{
    /// <summary>
    /// Row major matrix type
    /// </summary>
    /// <typeparam name="LAP"></typeparam>
    public class BrightMatrix<LAP> : BrightTensorBase<IMatrix, LAP>, IMatrix
        where LAP: LinearAlgebraProvider
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Tensor segment</param>
        /// <param name="rows">Number of rows</param>
        /// <param name="columns">Number of columns</param>
        /// <param name="lap">Linear algebra provider</param>
        public BrightMatrix(INumericSegment<float> data, uint rows, uint columns, LAP lap) : base(data, lap)
        {
            RowCount = rows;
            ColumnCount = columns;
            TotalSize = rows * columns;
        }

        /// <inheritdoc />
        public uint RowCount { get; private set; }

        /// <inheritdoc />
        public uint ColumnCount { get; private set; }

        /// <inheritdoc />
        public sealed override uint TotalSize { get; protected set; }

        /// <inheritdoc />
        public sealed override uint[] Shape
        {
            get => new[] { ColumnCount, RowCount };
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
        public INumericSegment<float> Row(uint index)
        {
            if(index > RowCount)
                throw new ArgumentOutOfRangeException(nameof(index), $"Number of rows is {RowCount} but index {index} was requested");
            return new TensorSegmentWrapper(Segment, index, RowCount, ColumnCount);
        }

        /// <inheritdoc />
        public INumericSegment<float> Column(uint index)
        {
            if(index > ColumnCount)
                throw new ArgumentOutOfRangeException(nameof(index), $"Number of columns is {ColumnCount} but index {index} was requested");
            return new TensorSegmentWrapper(Segment, index * RowCount, 1, RowCount);
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
        public virtual IReadOnlyVector GetRowAsReadOnly(uint rowIndex) => new ReadOnlyVectorWrapper(Row(rowIndex));

        /// <inheritdoc />
        public virtual IReadOnlyVector GetColumnAsReadOnly(uint columnIndex) => new ReadOnlyVectorWrapper(Column(columnIndex));

        /// <inheritdoc />
        public virtual IReadOnlyVector[] AllRowsAsReadOnly(bool makeCopy)
        {
            var ret = new IReadOnlyVector[RowCount];
            if (makeCopy) {
                var segment = new ArrayBasedTensorSegment(Segment.ToNewArray());
                for (uint i = 0; i < RowCount; i++)
                    ret[i] = new TensorSegmentWrapper(segment, i, RowCount, ColumnCount).ToReadOnlyVector();
            }
            else {
                for (uint i = 0; i < RowCount; i++)
                    ret[i] = GetRowAsReadOnly(i);
            }
            return ret;
        }

        /// <inheritdoc />
        public virtual IReadOnlyVector[] AllColumnsAsReadOnly(bool makeCopy)
        {
            var ret = new IReadOnlyVector[ColumnCount];
            if (makeCopy) {
                var segment = new ArrayBasedTensorSegment(Segment.ToNewArray());
                for (uint i = 0; i < ColumnCount; i++)
                    ret[i] = new TensorSegmentWrapper(segment, i * RowCount, 1, RowCount).ToReadOnlyVector();
            }
            else {
                for (uint i = 0; i < ColumnCount; i++)
                    ret[i] = GetColumnAsReadOnly(i);
            }

            return ret;
        }

        /// <inheritdoc />
        public virtual IVector GetRowVector(uint index) => Lap.CreateVector(Row(index));

        /// <inheritdoc />
        public virtual IVector GetColumnVector(uint index) => Lap.CreateVector(Column(index));

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
                ptr[i] = Row((uint)i);
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

        IReadOnlyVector IReadOnlyMatrix.GetRow(uint rowIndex) => GetRowAsReadOnly(rowIndex);
        IReadOnlyVector IReadOnlyMatrix.GetColumn(uint columnIndex) => GetColumnAsReadOnly(columnIndex);
        IReadOnlyVector[] IReadOnlyMatrix.AllRows() => AllRowsAsReadOnly(false);
        IReadOnlyVector[] IReadOnlyMatrix.AllColumns() => AllColumnsAsReadOnly(false);
    }

    /// <summary>
    /// Matrix type
    /// </summary>
    public class BrightMatrix : BrightMatrix<LinearAlgebraProvider>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Tensor segment</param>
        /// <param name="rows">Number of rows</param>
        /// <param name="columns">Number of columns</param>
        /// <param name="lap">Linear algebra provider</param>
        public BrightMatrix(INumericSegment<float> data, uint rows, uint columns, LinearAlgebraProvider lap) : base(data, rows, columns, lap)
        {
        }
    }
}
