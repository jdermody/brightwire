using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using BrightData.Helper;

namespace BrightData
{
    public class Matrix<T> : TensorBase<T, Matrix<T>>
        where T: struct
    {
        public Matrix(IBrightDataContext context, ITensorSegment<T> data, uint rows, uint columns) : base(context, data, new[] { rows, columns }) { }
        public Matrix(IBrightDataContext context, BinaryReader reader) : base(context, reader) { }

        public uint RowCount => Shape[0];
        public uint ColumnCount => Shape[1];
        public uint Size => RowCount * ColumnCount;

        public ITensorSegment<T> Row(uint index) => new TensorSegmentWrapper<T>(_data, index * ColumnCount, 1, ColumnCount);
        public ITensorSegment<T> Column(uint index) => new TensorSegmentWrapper<T>(_data, index, ColumnCount, RowCount);

        public T this[int y, int x]
        {
            get => _data[y * ColumnCount + x];
            set => _data[y * ColumnCount + x] = value;
        }
        public T this[uint y, uint x]
        {
            get => _data[y * ColumnCount + x];
            set => _data[y * ColumnCount + x] = value;
        }

        public override string ToString() => String.Format($"Matrix (Rows: {RowCount}, Columns: {ColumnCount})");

        public T[] ToColumnMajor()
        {
            var ret = new T[Size];
            for (uint i = 0; i < ColumnCount; i++) {
                var column = Column(i);
                var offset = i * RowCount;
                for (uint j = 0; j < RowCount; j++)
                    ret[offset + j] = column[j];
            }
            return ret;
        }

        public Matrix<T> Multiply(Matrix<T> matrix)
        {
            if (ColumnCount != matrix.RowCount)
                throw new ArgumentException("Target rows do not align with source columns");

            // naive implementation
            var computation = Computation;
            var resultSize = RowCount * matrix.ColumnCount;
            var ret = new Matrix<T>(Context, Context.TensorPool.Get<T>(resultSize).GetSegment(), RowCount, matrix.ColumnCount);
            Parallel.For(0, ret.Size, ind => {
                var j = (uint)(ind / RowCount);
                var i = (uint)(ind % RowCount);
                ret[j, i] = computation.SumIndexedProducts(ColumnCount, k => this[j, k], k => matrix[k, i]);
            });
            return ret;
        }

        public Matrix<T> Transpose()
        {
            var ret = new Matrix<T>(Context, Context.TensorPool.Get<T>(Size).GetSegment(), ColumnCount, RowCount);
            Parallel.For(0, ret.Size, ind => {
                var j = (uint)(ind / ColumnCount);
                var i = (uint)(ind % ColumnCount);
                ret[i, j] = this[j, i];
            });
            return ret;
        }

        protected override Matrix<T> Create(ITensorSegment<T> segment)
        {
            return new Matrix<T>(Context, segment, RowCount, ColumnCount);
        }
    }
}
