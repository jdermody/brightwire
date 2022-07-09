using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BrightData.LinearAlegbra2.TensorInfo;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlegbra2
{
    public class Matrix2<LAP> : TensorBase2<IMatrix, LAP>, IMatrix, IMatrixSegments
        where LAP: LinearAlgebraProvider
    {
        public Matrix2(ITensorSegment2 data, uint rows, uint columns, LAP lap) : base(data, lap)
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
        public TensorSegmentWrapper2 Row(uint index) => new(Segment, index, RowCount, ColumnCount);
        public TensorSegmentWrapper2 Column(uint index) => new(Segment, index * RowCount, 1, RowCount);
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

        public override IMatrix Create(ITensorSegment2 segment) => new Matrix2<LAP>(segment, RowCount, ColumnCount, _lap);
        IMatrix IMatrixInfo.Create(LinearAlgebraProvider lap) => lap.CreateMatrix(RowCount, ColumnCount, (i, j) => this[i, j]);
        public IVectorInfo GetRow(uint rowIndex) => new VectorInfoWrapper(Row(rowIndex));
        public IVectorInfo GetColumn(uint columnIndex) => new VectorInfoWrapper(Column(columnIndex));

        public IVectorInfo[] AllRows()
        {
            var ret = new IVectorInfo[RowCount];
            for (uint i = 0; i < RowCount; i++)
                ret[i] = GetRow(i);
            return ret;
        }
        public IVectorInfo[] AllColumns()
        {
            var ret = new IVectorInfo[ColumnCount];
            for (uint i = 0; i < ColumnCount; i++)
                ret[i] = GetColumn(i);
            return ret;
        }

        //public IVector[] RowVectors()
        //{
        //    var ret = new IVector[RowCount];
        //    for (uint i = 0; i < RowCount; i++)
        //        ret[i] = LinearAlgebraProvider.CreateVector(Row(i));
        //    return ret;
        //}
        //public IVector[] ColumnVectors()
        //{
        //    var ret = new IVector[ColumnCount];
        //    for (uint i = 0; i < ColumnCount; i++)
        //        ret[i] = LinearAlgebraProvider.CreateVector(Column(i));
        //    return ret;
        //}

        public MemoryOwner<float> ToRowMajor()
        {
            var ret = MemoryOwner<float>.Allocate((int)TotalSize);
            var array = ret.DangerousGetArray();
            Parallel.For(0, RowCount, ind => {
                var i = (uint) ind;
                using var row = Row(i);
                var offset = i * ColumnCount;
                for (uint j = 0; j < ColumnCount; j++)
                    array[(int)(offset + j)] = row[j];
            });
            return ret;
        }

        public IMatrix Transpose() => _lap.Transpose(this);
        public IMatrix Multiply(IMatrix other) => _lap.Multiply(this, other);
        public IVector GetDiagonal() => _lap.GetDiagonal(this);
        public IVector RowSums() => _lap.RowSums(this);
        public IVector ColumnSums() => _lap.ColumnSums(this);
        public IMatrix Multiply(IVector vector) => Multiply(vector.Reshape(null, 1));
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
        public void AddToEachRow(ITensorSegment2 segment) => _lap.AddToEachRow(this, segment);
        public void AddToEachColumn(ITensorSegment2 segment) => _lap.AddToEachColumn(this, segment);

        /// <inheritdoc />
        public override string ToString()
        {
            var preview = String.Join("|", Segment.Values.Take(8));
            if (TotalSize > 8)
                preview += "|...";
            return $"Matrix (Rows: {RowCount}, Columns: {ColumnCount}) {preview}";
        }
    }

    public class Matrix2 : Matrix2<LinearAlgebraProvider>
    {
        public Matrix2(ITensorSegment2 data, uint rows, uint columns, LinearAlgebraProvider computationUnit) : base(data, rows, columns, computationUnit)
        {
        }
    }

    public class ArrayBasedMatrix : Matrix2<ArrayBasedLinearAlgebraProvider>
    {
        public ArrayBasedMatrix(ITensorSegment2 data, uint rows, uint columns, ArrayBasedLinearAlgebraProvider computationUnit) : base(data, rows, columns, computationUnit)
        {
        }
    }
}
