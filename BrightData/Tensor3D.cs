using System.Collections.Generic;
using System.IO;
using System.Linq;
using BrightData.Memory;

namespace BrightData
{
    public class Tensor3D<T> : TensorBase<T, Tensor3D<T>>
        where T: struct
    {
        public Tensor3D(ITensorSegment<T> segment, uint depth, uint rows, uint columns) : base(segment, new[] {depth, rows, columns}) { }
        public Tensor3D(IBrightDataContext context, BinaryReader reader) : base(context, reader) { }

        public uint Depth => Shape[0];
        public uint RowCount => Shape[1];
        public uint ColumnCount => Shape[2];
        public uint MatrixSize => RowCount * ColumnCount;
        public new uint Size => Depth * MatrixSize;

        public T this[int depth, int rowY, int columnX]
        {
            get => _segment[depth * MatrixSize + rowY * ColumnCount + columnX];
            set => _segment[depth * MatrixSize + rowY * ColumnCount + columnX] = value;
        }
        public T this[uint depth, uint rowY, uint columnX]
        {
            get => _segment[depth * MatrixSize + rowY * ColumnCount + columnX];
            set => _segment[depth * MatrixSize + rowY * ColumnCount + columnX] = value;
        }

        public Matrix<T> Matrix(uint index)
        {
            var segment = new TensorSegmentWrapper<T>(_segment, index * MatrixSize, 1, MatrixSize);
            return new Matrix<T>(segment, RowCount, ColumnCount);
        }

        public IEnumerable<Matrix<T>> Matrices => Depth.AsRange().Select(Matrix);

        public override string ToString()
        {
            return $"Tensor3D (Depth: {Depth}, Rows: {RowCount}, Columns: {ColumnCount})";
        }

        protected override Tensor3D<T> Create(ITensorSegment<T> segment)
        {
            return new Tensor3D<T>(segment, Depth, RowCount, ColumnCount);
        }

        /// <summary>
        /// Converts the segment to a column major vector
        /// </summary>
        public T[] GetAsRaw()
        {
            var data = new T[Size];
            var blockSize = Size / Depth;
            int k = 0;
            foreach(var matrix in Matrices) {
                int i = 0;
                var rowCount = matrix.RowCount;
                foreach(var row in matrix.Rows) {
                    int j = 0;
                    foreach(var item in row.Segment.Values) {
                        data[(j * rowCount + i) + (k * blockSize)] = item;
                        ++j;
                    }
                    ++i;
                }
                ++k;
            }
            return data;
        }
    }
}
