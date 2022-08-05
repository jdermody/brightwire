using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using BrightData.LinearAlgebra.ReadOnly;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra
{
    public class BrightMatrix<LAP> : BrightTensorBase<IMatrix, LAP>, IMatrix, IMatrixSegments
        where LAP: LinearAlgebraProvider
    {
        public BrightMatrix(ITensorSegment data, uint rows, uint columns, LAP lap) : base(data, lap)
        {
            RowCount = rows;
            ColumnCount = columns;
            TotalSize = rows * columns;
        }

        public uint RowCount { get; private set; }
        public uint ColumnCount { get; private set; }
        public sealed override uint TotalSize { get; protected set; }
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

        public float this[int rowY, int columnX]
        {
            get => Segment[columnX * RowCount + rowY];
            set => Segment[columnX * RowCount + rowY] = value;
        }
        public float this[uint rowY, uint columnX]
        {
            get => Segment[columnX * RowCount + rowY];
            set => Segment[columnX * RowCount + rowY] = value;
        }
        public float this[long rowY, long columnX]
        {
            get => Segment[columnX * RowCount + rowY];
            set => Segment[columnX * RowCount + rowY] = value;
        }
        public float this[ulong rowY, ulong columnX]
        {
            get => Segment[columnX * RowCount + rowY];
            set => Segment[columnX * RowCount + rowY] = value;
        }

        public TensorSegmentWrapper Row(uint index, ITensorSegment? segment = null)
        {
            if(index > RowCount)
                throw new ArgumentOutOfRangeException(nameof(index), $"Number of rows is {RowCount} but index {index} was requested");
            return new(segment ?? Segment, index, RowCount, ColumnCount);
        }

        public TensorSegmentWrapper Column(uint index, ITensorSegment? segment = null)
        {
            if(index > ColumnCount)
                throw new ArgumentOutOfRangeException(nameof(index), $"Number of columns is {ColumnCount} but index {index} was requested");
            return new(segment ?? Segment, index * RowCount, 1, RowCount);
        }

        public unsafe ReadOnlySpan<float> GetRowSpan(uint rowIndex, ref SpanOwner<float> temp)
        {
            temp = SpanOwner<float>.Allocate((int)TotalSize);
            var span = temp.Span;
            fixed (float* ptr = &MemoryMarshal.GetReference(span)) {
                Segment.CopyTo(ptr, (int)rowIndex * (int)RowCount, (int)RowCount, (int)ColumnCount);
            }
            return span;
        }

        public ReadOnlySpan<float> GetColumnSpan(uint columnIndex)
        {
            var ret = Segment.GetSpan();
            return ret.Slice((int)(columnIndex * RowCount), (int)RowCount);
        }

        public override IMatrix Create(ITensorSegment segment) => _lap.CreateMatrix(RowCount, ColumnCount, segment);
        IMatrix IReadOnlyMatrix.Create(LinearAlgebraProvider lap) => lap.CreateMatrix(RowCount, ColumnCount, (i, j) => this[i, j]);
        public virtual IReadOnlyVector GetRow(uint rowIndex) => new ReadOnlyVectorWrapper(Row(rowIndex));
        public virtual IReadOnlyVector GetColumn(uint columnIndex) => new ReadOnlyVectorWrapper(Column(columnIndex));

        public virtual IReadOnlyVector[] AllRows(bool makeCopy)
        {
            var ret = new IReadOnlyVector[RowCount];
            if (makeCopy) {
                var segment = new ArrayBasedTensorSegment(Segment.ToNewArray());
                for (uint i = 0; i < RowCount; i++)
                    ret[i] = Row(i, segment).ToVectorInfo();
            }
            else {
                for (uint i = 0; i < RowCount; i++)
                    ret[i] = GetRow(i);
            }
            return ret;
        }
        public virtual IReadOnlyVector[] AllColumns(bool makeCopy)
        {
            var ret = new IReadOnlyVector[ColumnCount];
            if (makeCopy) {
                var segment = new ArrayBasedTensorSegment(Segment.ToNewArray());
                for (uint i = 0; i < ColumnCount; i++)
                    ret[i] = Column(i, segment).ToVectorInfo();
            }
            else {
                for (uint i = 0; i < ColumnCount; i++)
                    ret[i] = GetColumn(i);
            }

            return ret;
        }

        public virtual IVector GetRowVector(uint index) => _lap.CreateVector(Row(index));
        public virtual IVector GetColumnVector(uint index) => _lap.CreateVector(Column(index));

        public IMatrix Transpose() => _lap.Transpose(this);
        public IMatrix Multiply(IMatrix other) => _lap.Multiply(this, other);
        public IVector GetDiagonal() => _lap.GetDiagonal(this);
        public IVector RowSums() => _lap.RowSums(this);
        public IVector ColumnSums() => _lap.ColumnSums(this);

        public IVector Multiply(IVector vector) => _lap.Multiply(this, vector);
        public IMatrix TransposeAndMultiply(IMatrix other) => _lap.TransposeSecondAndMultiply(this, other);
        public IMatrix TransposeThisAndMultiply(IMatrix other) => _lap.TransposeFirstAndMultiply(this, other);

        public IMatrix MapIndexed(Func<uint, uint, float, float> mutator)
        {
            var ret = _lap.MapParallel(Segment, (ind, val) => {
                var i = ind % RowCount;
                var j = ind / RowCount;
                return mutator(i, j, val);
            });
            return _lap.CreateMatrix(RowCount, ColumnCount, ret);
        }

        public void MapIndexedInPlace(Func<uint, uint, float, float> mutator)
        {
            var ret = _lap.MapParallel(Segment, (ind, val) => {
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

        public (IMatrix Left, IMatrix Right) SplitAtColumn(uint columnIndex) => _lap.SplitAtColumn(this, columnIndex);
        public (IMatrix Top, IMatrix Bottom) SplitAtRow(uint rowIndex) => _lap.SplitAtRow(this, rowIndex);
        public IMatrix ConcatColumns(IMatrix bottom) => _lap.ConcatColumns(this, bottom);
        public IMatrix ConcatRows(IMatrix right) => _lap.ConcatRows(this, right);
        public (IMatrix U, IVector S, IMatrix VT) Svd() => _lap.Svd(this);
        public IMatrix GetNewMatrixFromRows(IEnumerable<uint> rowIndices) => _lap.GetNewMatrixFromRows(this, rowIndices);
        public IMatrix GetNewMatrixFromColumns(IEnumerable<uint> columnIndices) => _lap.GetNewMatrixFromColumns(this, columnIndices);
        public void AddToEachRow(ITensorSegment segment) => _lap.AddToEachRow(this, segment);
        public void AddToEachColumn(ITensorSegment segment) => _lap.AddToEachColumn(this, segment);
        public void MultiplyEachRowWith(ITensorSegment segment) => _lap.MultiplyEachRowWith(this, segment);
        public void MultiplyEachColumnWith(ITensorSegment segment) => _lap.MultiplyEachColumnWith(this, segment);

        public ITensorSegment[] SoftmaxPerRow()
        {
            var segments = SpanOwner<ITensorSegment>.Allocate((int)RowCount);
            var ptr = segments.Span;
            for (var i = 0; i < RowCount; i++)
                ptr[i] = Row((uint)i);
            return _lap.MultiSoftmax(segments.DangerousGetArray());
        }
        public ITensorSegment[] SoftmaxDerivativePerRow(ITensorSegment[] rows) => _lap.SoftmaxDerivativePerRow(this, rows);

        //public ITensorSegment[] SoftmaxDerivativePerRow(IVector[] rows)
        //{
        //    var ret = new ITensorSegment[RowCount];
        //    var derivative = _lap.MultiSoftmaxDerivative(new ArraySegment<ITensorSegment>(rows.Select(x => x.Segment).ToArray()));
        //    for (uint i = 0, len = RowCount; i < len; i++) {
        //        var row = Row(i);
        //        using var softmaxDerivative = derivative[i];
        //        var segments = (IMatrixSegments)softmaxDerivative;
        //        ret[i] = _lap.CreateSegment(row.Size, true);
        //        for (uint j = 0; j < row.Size; j++) {
        //            var otherRow = segments.Row(j);
        //            var val = row.DotProduct(otherRow);
        //            ret[i][j] = val;
        //        }
        //    }

        //    return ret;
        //}

        /// <inheritdoc />
        public override string ToString()
        {
            var preview = String.Join("|", Segment.Values.Take(Consts.PreviewSize));
            if (TotalSize > Consts.PreviewSize)
                preview += "|...";
            return $"Matrix (Rows: {RowCount}, Columns: {ColumnCount}) {preview}";
        }
    }

    public class BrightMatrix : BrightMatrix<LinearAlgebraProvider>
    {
        public BrightMatrix(ITensorSegment data, uint rows, uint columns, LinearAlgebraProvider computationUnit) : base(data, rows, columns, computationUnit)
        {
        }
    }
}
