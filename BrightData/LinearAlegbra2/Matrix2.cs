﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlegbra2
{
    public class Matrix2<CU> : TensorBase2<IMatrix, CU>, IMatrix
        where CU: ComputationUnit
    {
        public Matrix2(ITensorSegment2 data, uint rows, uint columns, CU computationUnit) : base(data, computationUnit)
        {
            RowCount = rows;
            ColumnCount = columns;
            Size = rows * columns;
        }

        public uint RowCount { get; private set; }
        public uint ColumnCount { get; private set; }
        public override uint Size { get; protected set; }
        public override uint[] Shape
        {
            get => new[] { RowCount, ColumnCount };
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

        public override IMatrix Create(ITensorSegment2 segment) => new Matrix2<CU>(segment, RowCount, ColumnCount, _computationUnit);

        public IDisposableTensorSegmentWrapper Row(uint index) => new TensorSegmentWrapper2(Segment, index * ColumnCount, 1, ColumnCount);
        public IDisposableTensorSegmentWrapper Column(uint index) => new TensorSegmentWrapper2(Segment, index, ColumnCount, RowCount);

        public IDisposableTensorSegmentWrapper[] Rows()
        {
            var ret = new IDisposableTensorSegmentWrapper[RowCount];
            for (uint i = 0; i < RowCount; i++)
                ret[i] = Row(i);
            return ret;
        }
        public IDisposableTensorSegmentWrapper[] Columns()
        {
            var ret = new IDisposableTensorSegmentWrapper[ColumnCount];
            for (uint i = 0; i < ColumnCount; i++)
                ret[i] = Column(i);
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

        public IMatrix Transpose() => _computationUnit.Transpose(this);
        public IMatrix Multiply(IMatrix other) => _computationUnit.Multiply(this, other);
        public IVector GetDiagonal() => _computationUnit.GetDiagonal(this);
        public IVector RowSums() => _computationUnit.RowSums(this);
        public IVector ColumnSums() => _computationUnit.ColumnSums(this);
        public IMatrix Multiply(IVector vector) => Multiply(vector.Reshape(null, 1));
        public IMatrix TransposeAndMultiply(IMatrix other) => _computationUnit.TransposeSecondAndMultiply(this, other);
        public IMatrix TransposeThisAndMultiply(IMatrix other) => _computationUnit.TransposeFirstAndMultiply(this, other);

        public IMatrix MapIndexed(Func<uint, uint, float, float> mutator)
        {
            var ret = _computationUnit.MapParallel(Segment, (ind, val) => {
                var i = ind / ColumnCount;
                var j = ind % ColumnCount;
                return mutator(i, j, val);
            });
            return _computationUnit.CreateMatrix(ret, RowCount, ColumnCount);
        }

        public void MapIndexedInPlace(Func<uint, uint, float, float> mutator)
        {
            var ret = _computationUnit.MapParallel(Segment, (ind, val) => {
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

        public (IMatrix Left, IMatrix Right) SplitAtColumn(uint columnIndex) => _computationUnit.SplitAtColumn(this, columnIndex);
        public (IMatrix Top, IMatrix Bottom) SplitAtRow(uint rowIndex) => _computationUnit.SplitAtRow(this, rowIndex);
        public IMatrix ConcatColumns(IMatrix bottom) => _computationUnit.ConcatColumns(this, bottom);
        public IMatrix ConcatRows(IMatrix right) => _computationUnit.ConcatRows(this, right);
        public (IMatrix U, IVector S, IMatrix VT) Svd() => _computationUnit.Svd(this);

        /// <inheritdoc />
        public override string ToString()
        {
            var preview = Size <= 8 ? string.Join('|', Segment.Values) : "";
            return $"Matrix (Rows: {RowCount}, Columns: {ColumnCount}) {preview}";
        }
    }

    public class Matrix2 : Matrix2<ComputationUnit>
    {
        public Matrix2(ITensorSegment2 data, uint rows, uint columns, ComputationUnit computationUnit) : base(data, rows, columns, computationUnit)
        {
        }
    }
}
