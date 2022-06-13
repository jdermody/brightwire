﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData;

namespace BrightData2
{
    public class Matrix2<CU> : TensorBase2<IMatrix, CU>, IMatrix
        where CU: ComputationUnit
    {
        public Matrix2(ITensorSegment2 data, uint rows, uint columns, CU computationUnit) : base(data, computationUnit)
        {
            RowCount = rows;
            ColumnCount = columns;
        }

        public uint RowCount { get; }
        public uint ColumnCount { get; }
        public override uint Size => RowCount * ColumnCount;

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

        public float[] ToNewColumnMajorArray()
        {
            var ret = new float[Size];
            Parallel.For(0, ColumnCount, ind => {
                var i = (uint) ind;
                var column = Column(i);
                var offset = i * RowCount;
                for (uint j = 0; j < RowCount; j++)
                    ret[offset + j] = column[j];
            });
            return ret;
        }

        public IMatrix Transpose() => _computationUnit.Transpose(this);
        public IMatrix Multiply(IMatrix other) => _computationUnit.Multiply(this, other);
        public IVector GetDiagonal() => _computationUnit.GetDiagonal(this);
        public IVector RowSums() => _computationUnit.RowSums(this);
        public IVector ColumnSums() => _computationUnit.ColumnSums(this);
        public IMatrix Multiply(IVector vector) => Multiply(vector.Reshape(null, 1));

        public IMatrix MapIndexed(Func<uint, uint, float, float> mutator)
        {
            var ret = MapParallel((ind, val) => {
                var i = ind / ColumnCount;
                var j = ind % ColumnCount;
                return mutator(i, j, val);
            });
            return _computationUnit.CreateMatrix(ret, RowCount, ColumnCount);
        }

        public void MapIndexedInPlace(Func<uint, uint, float, float> mutator)
        {
            var ret = MapParallel((ind, val) => {
                var i = ind / ColumnCount;
                var j = ind % ColumnCount;
                return mutator(i, j, val);
            });
            try {
                Segment.CopyFrom(ret.GetSpan());
            }
            finally {
                ret.Release();
            }
        }

        public override string ToString() => String.Format($"Matrix (Rows: {RowCount}, Columns: {ColumnCount})");
    }

    public class Matrix2 : Matrix2<ComputationUnit>
    {
        public Matrix2(ITensorSegment2 data, uint rows, uint columns, ComputationUnit computationUnit) : base(data, rows, columns, computationUnit)
        {
        }
    }
}
