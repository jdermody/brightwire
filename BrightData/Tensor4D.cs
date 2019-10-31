using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BrightData.Helper;

namespace BrightData
{
    public class Tensor4D<T> : TensorBase<T, Tensor4D<T>>
    {
        public Tensor4D(IBrightDataContext context, ITensorSegment<T> data, uint count, uint depth, uint rows, uint columns) : base(context, data, new[] { count, depth, rows, columns }) { }
        public Tensor4D(IBrightDataContext context, BinaryReader reader) : base(context, reader) { }

        public uint Count => Shape[0];
        public uint Depth => Shape[1];
        public uint RowCount => Shape[2];
        public uint ColumnCount => Shape[3];
        public uint MatrixSize => RowCount * ColumnCount;
        public uint TensorSize => Depth * MatrixSize;
        public uint Size => Count * TensorSize;

        public Tensor3D<T> Tensor(uint index)
        {
            var segment = new TensorSegmentWrapper<T>(_data, index * TensorSize, 1, TensorSize);
            return new Tensor3D<T>(Context, segment, Depth, RowCount, ColumnCount);
        }

        public override string ToString() => $" Tensor4D (Count: {Count}, Depth: {Depth}, Rows: {RowCount}, Columns: {ColumnCount})";

        protected override Tensor4D<T> Create(ITensorSegment<T> segment)
        {
            return new Tensor4D<T>(Context, segment, Count, Depth, RowCount, ColumnCount);
        }
    }
}
