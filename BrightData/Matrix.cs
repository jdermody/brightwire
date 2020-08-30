using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData.Helper;
using BrightData.Memory;

namespace BrightData
{
    public class Matrix<T> : TensorBase<T, Matrix<T>>
        where T: struct
    {
        public Matrix(ITensorSegment<T> data, uint rows, uint columns) : base(data, new[] { rows, columns }) { }
        public Matrix(IBrightDataContext context, BinaryReader reader) : base(context, reader) { }

        public uint RowCount => Shape[0];
        public uint ColumnCount => Shape[1];
        public new uint Size => RowCount * ColumnCount;

        public Vector<T> Row(uint index) => new Vector<T>(new TensorSegmentWrapper<T>(_data, index * ColumnCount, 1, ColumnCount));
        public Vector<T> Column(uint index) => new Vector<T>(new TensorSegmentWrapper<T>(_data, index, ColumnCount, RowCount));

        public IEnumerable<Vector<T>> Rows => RowCount.AsRange().Select(i => Row(i));
        public IEnumerable<Vector<T>> Columns => ColumnCount.AsRange().Select(i => Column(i));

        public T this[int rowY, int columnX]
        {
            get => _data[rowY * ColumnCount + columnX];
            set => _data[rowY * ColumnCount + columnX] = value;
        }
        public T this[uint rowY, uint columnX]
        {
            get => _data[rowY * ColumnCount + columnX];
            set => _data[rowY * ColumnCount + columnX] = value;
        }

        public override string ToString() => String.Format($"Matrix (Rows: {RowCount}, Columns: {ColumnCount})");

        public T[] ToColumnMajor()
        {
            var ret = new T[Size];
            Parallel.For(0, ColumnCount, ind => {
                var i = (uint) ind;
                var column = Column(i);
                var offset = i * RowCount;
                for (uint j = 0; j < RowCount; j++)
                    ret[offset + j] = column[j];
            });
            return ret;
        }

        public Matrix<T> Transpose()
        {
            var ret = new Matrix<T>(Context.TensorPool.Get<T>(Size).GetSegment(), ColumnCount, RowCount);
            Parallel.For(0, ret.Size, ind => {
                var j = (uint)(ind / ColumnCount);
                var i = (uint)(ind % ColumnCount);
                ret[i, j] = this[j, i];
            });
            return ret;
        }

        protected override Matrix<T> Create(ITensorSegment<T> segment)
        {
            return new Matrix<T>(segment, RowCount, ColumnCount);
        }
    }
}
