using System.Collections.Generic;
using System.IO;
using System.Linq;
using BrightData.Memory;

namespace BrightData.LinearAlgebra
{
    /// <summary>
    /// 3D tensor type
    /// </summary>
    /// <typeparam name="T">Data type within the tensor</typeparam>
    public class Tensor3D<T> : TensorBase<T, Tensor3D<T>>
        where T: struct
    {
        internal Tensor3D(ITensorSegment<T> segment, uint depth, uint rows, uint columns) : base(segment, new[] {depth, rows, columns}) { }
        internal Tensor3D(IBrightDataContext context, BinaryReader reader) : base(context, reader) { }

        /// <summary>
        /// Number of matrices
        /// </summary>
        public uint Depth => Shape[0];

        /// <summary>
        /// Number of rows within each matrix
        /// </summary>
        public uint RowCount => Shape[1];

        /// <summary>
        /// Number of columns within each matrix
        /// </summary>
        public uint ColumnCount => Shape[2];

        /// <summary>
        /// Size of each matrix
        /// </summary>
        public uint MatrixSize => RowCount * ColumnCount;
        //public new uint Size => Depth * MatrixSize;

        /// <summary>
        /// Returns the value at the specified index
        /// </summary>
        /// <param name="depth">Index of the matrix</param>
        /// <param name="rowY">Row within the matrix</param>
        /// <param name="columnX">Column within the matrix</param>
        /// <returns></returns>
        public T this[int depth, int rowY, int columnX]
        {
            get => _segment[depth * MatrixSize + rowY * ColumnCount + columnX];
            set => _segment[depth * MatrixSize + rowY * ColumnCount + columnX] = value;
        }

        /// <summary>
        /// Returns the value at the specified index
        /// </summary>
        /// <param name="depth">Index of the matrix</param>
        /// <param name="rowY">Row within the matrix</param>
        /// <param name="columnX">Column within the matrix</param>
        /// <returns></returns>
        public T this[uint depth, uint rowY, uint columnX]
        {
            get => _segment[depth * MatrixSize + rowY * ColumnCount + columnX];
            set => _segment[depth * MatrixSize + rowY * ColumnCount + columnX] = value;
        }

        /// <summary>
        /// Returns a matrix at the specified index
        /// </summary>
        /// <param name="index">Matrix index</param>
        /// <returns></returns>
        public Matrix<T> Matrix(uint index)
        {
            var segment = new TensorSegmentWrapper<T>(_segment, index * MatrixSize, 1, MatrixSize);
            return new Matrix<T>(segment, RowCount, ColumnCount);
        }

        /// <summary>
        /// All matrices
        /// </summary>
        public IEnumerable<Matrix<T>> Matrices => Depth.AsRange().Select(Matrix);

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Tensor3D (Depth: {Depth}, Rows: {RowCount}, Columns: {ColumnCount})";
        }

        /// <inheritdoc />
        protected override Tensor3D<T> Create(ITensorSegment<T> segment)
        {
            return new Tensor3D<T>(segment, Depth, RowCount, ColumnCount);
        }

        /// <summary>
        /// Converts the segment to a column major vector (default is row major)
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
