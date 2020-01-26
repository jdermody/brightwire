using System.IO;
using BrightData.Memory;

namespace BrightData
{
    public class Tensor3D<T> : TensorBase<T, Tensor3D<T>>
        where T: struct
    {
        public Tensor3D(IBrightDataContext context, ITensorSegment<T> data, uint depth, uint rows, uint columns) : base(context, data, new[] {depth, rows, columns}) { }
        public Tensor3D(IBrightDataContext context, BinaryReader reader) : base(context, reader) { }

        public uint Depth => Shape[0];
        public uint RowCount => Shape[1];
        public uint ColumnCount => Shape[2];
        public uint MatrixSize => RowCount * ColumnCount;
        public new uint Size => Depth * MatrixSize;

        public T this[int depth, int rowY, int columnX]
        {
            get => _data[depth * MatrixSize + rowY * ColumnCount + columnX];
            set => _data[depth * MatrixSize + rowY * ColumnCount + columnX] = value;
        }
        public T this[uint depth, uint rowY, uint columnX]
        {
            get => _data[depth * MatrixSize + rowY * ColumnCount + columnX];
            set => _data[depth * MatrixSize + rowY * ColumnCount + columnX] = value;
        }

        public Matrix<T> Matrix(uint index)
        {
            var segment = new TensorSegmentWrapper<T>(_data, index * MatrixSize, 1, MatrixSize);
            return new Matrix<T>(Context, segment, RowCount, ColumnCount);
        }

        public override string ToString()
        {
            return $"Tensor3D (Depth: {Depth}, Rows: {RowCount}, Columns: {ColumnCount})";
        }

        protected override Tensor3D<T> Create(ITensorSegment<T> segment)
        {
            return new Tensor3D<T>(Context, segment, Depth, RowCount, ColumnCount);
        }
    }
}
