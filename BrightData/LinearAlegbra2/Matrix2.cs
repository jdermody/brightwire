using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlegbra2
{
    public class Matrix2<CU> : TensorBase2<IMatrix, CU>, IMatrix
        where CU: LinearAlgebraProvider
    {
        public Matrix2(ITensorSegment2 data, uint rows, uint columns, CU computationUnit) : base(data, computationUnit)
        {
            RowCount = rows;
            ColumnCount = columns;
            Size = rows * columns;
        }

        public uint RowCount { get; private set; }
        public uint ColumnCount { get; private set; }
        public sealed override uint Size { get; protected set; }
        public override uint[] Shape
        {
            get => new[] { ColumnCount, RowCount };
            protected set
            {
                ColumnCount = value[0];
                RowCount = value[1];
                Size = RowCount * ColumnCount;
            }
        }

        public float this[int rowY, int columnX]
        {
            get => Segment[rowY * ColumnCount + columnX];
            set => Segment[rowY * ColumnCount + columnX] = value;
        }
        public float this[uint rowY, uint columnX]
        {
            get => Segment[rowY * ColumnCount + columnX];
            set => Segment[rowY * ColumnCount + columnX] = value;
        }
        public float this[long rowY, long columnX]
        {
            get => Segment[rowY * ColumnCount + columnX];
            set => Segment[rowY * ColumnCount + columnX] = value;
        }
        public float this[ulong rowY, ulong columnX]
        {
            get => Segment[rowY * ColumnCount + columnX];
            set => Segment[rowY * ColumnCount + columnX] = value;
        }

        public override IMatrix Create(ITensorSegment2 segment) => new Matrix2<CU>(segment, RowCount, ColumnCount, _lap);

        public ITensorSegment2 Row(uint index) => new TensorSegmentWrapper2(Segment, index * ColumnCount, 1, ColumnCount);
        public ITensorSegment2 Column(uint index) => new TensorSegmentWrapper2(Segment, index, ColumnCount, RowCount);

        public unsafe ReadOnlySpan<float> GetColumnSpan(uint columnIndex, ref SpanOwner<float> temp, out bool wasTempUsed)
        {
            temp = SpanOwner<float>.Allocate((int)Size);
            var span = temp.Span;
            fixed (float* ptr = &MemoryMarshal.GetReference(span)) {
                Segment.CopyTo(ptr, (int)columnIndex, (int)ColumnCount, (int)RowCount);
            }
            wasTempUsed = true;
            return span;
        }
        public ReadOnlySpan<float> GetRowSpan(uint rowIndex)
        {
            var ret = Segment.GetSpan();
            return ret.Slice((int)(rowIndex * ColumnCount), (int)ColumnCount);
        }

        public ITensorSegment2[] Rows()
        {
            var ret = new ITensorSegment2[RowCount];
            for (uint i = 0; i < RowCount; i++)
                ret[i] = Row(i);
            return ret;
        }
        public ITensorSegment2[] Columns()
        {
            var ret = new ITensorSegment2[ColumnCount];
            for (uint i = 0; i < ColumnCount; i++)
                ret[i] = Column(i);
            return ret;
        }

        public IVector[] RowVectors()
        {
            var ret = new IVector[RowCount];
            for (uint i = 0; i < RowCount; i++)
                ret[i] = LinearAlgebraProvider.CreateVector(Row(i));
            return ret;
        }
        public IVector[] ColumnVectors()
        {
            var ret = new IVector[ColumnCount];
            for (uint i = 0; i < ColumnCount; i++)
                ret[i] = LinearAlgebraProvider.CreateVector(Column(i));
            return ret;
        }

        public MemoryOwner<float> ToNewColumnMajor()
        {
            var ret = MemoryOwner<float>.Allocate((int)Size);
            var array = ret.DangerousGetArray();
            Parallel.For(0, ColumnCount, ind => {
                var i = (uint) ind;
                var column = Column(i);
                var offset = i * RowCount;
                for (uint j = 0; j < RowCount; j++)
                    array[(int)(offset + j)] = column[j];
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
                var i = ind / ColumnCount;
                var j = ind % ColumnCount;
                return mutator(i, j, val);
            });
            return _lap.CreateMatrix(RowCount, ColumnCount, ret);
        }

        public void MapIndexedInPlace(Func<uint, uint, float, float> mutator)
        {
            var ret = _lap.MapParallel(Segment, (ind, val) => {
                var i = ind / ColumnCount;
                var j = ind % ColumnCount;
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
            if (Size > 8)
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
