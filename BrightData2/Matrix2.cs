using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData;

namespace BrightData2
{
    public class Matrix2 : TensorBase2<IMatrix, ComputationUnit>, IMatrix
    {
        public Matrix2(ITensorSegment2 data, uint rows, uint columns, ComputationUnit computationUnit) : base(data, computationUnit)
        {
            RowCount = rows;
            ColumnCount = columns;
        }

        public uint RowCount { get; }
        public uint ColumnCount { get; }
        public uint Size => RowCount * ColumnCount;

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

        public override IMatrix Create(ITensorSegment2 segment) => new Matrix2(segment, RowCount, ColumnCount, _computationUnit);

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

        public virtual IMatrix Transpose()
        {
            var ret = _computationUnit.CreateMatrix(ColumnCount, RowCount);
            Parallel.For(0, ret.Size, ind => {
                var j = (uint)(ind / ColumnCount);
                var i = (uint)(ind % ColumnCount);
                ret[i, j] = this[j, i];
            });
            return ret;
        }

        public virtual IMatrix Multiply(IMatrix other)
        {
            var ret = _computationUnit.CreateMatrix(RowCount, other.ColumnCount);
            var columns = other.Columns();
            var rows = Rows();
            Parallel.For(0, ret.Size, ind => {
                var i = (uint) (ind % RowCount);
                var j = (uint) (ind / RowCount);
                var column = columns[j];
                var row = rows[i];
                var val = row.DotProduct(column);
                ret[i, j] = val;
            });

            // don't need to dispose the wrappers
            return ret;
        }

        public override string ToString() => String.Format($"Matrix (Rows: {RowCount}, Columns: {ColumnCount})");
    }
}
