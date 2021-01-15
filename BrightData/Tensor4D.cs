using System.Collections.Generic;
using System.IO;
using System.Linq;
using BrightData.Memory;

namespace BrightData
{
    /// <summary>
    /// 4D tensor type
    /// </summary>
    /// <typeparam name="T">Data type within the tensor</typeparam>
    public class Tensor4D<T> : TensorBase<T, Tensor4D<T>>
        where T: struct
    {
        internal Tensor4D(ITensorSegment<T> segment, uint count, uint depth, uint rows, uint columns) : base(segment, new[] { count, depth, rows, columns }) { }
        internal Tensor4D(IBrightDataContext context, BinaryReader reader) : base(context, reader) { }

        /// <summary>
        /// Number of 3D tensors
        /// </summary>
        public uint Count => Shape[0];

        /// <summary>
        /// Number of matrices within each 3D tensor
        /// </summary>
        public uint Depth => Shape[1];

        /// <summary>
        /// Number of rows within each matrix
        /// </summary>
        public uint RowCount => Shape[2];

        /// <summary>
        /// Number of columns within each matrix
        /// </summary>
        public uint ColumnCount => Shape[3];

        /// <summary>
        /// Size of each matrix
        /// </summary>
        public uint MatrixSize => RowCount * ColumnCount;

        /// <summary>
        /// Size of each 3D tensor
        /// </summary>
        public uint TensorSize => Depth * MatrixSize;
        //public new uint Size => Count * TensorSize;

        /// <summary>
        /// Returns the value at the specified index
        /// </summary>
        /// <param name="count">3D tensor index</param>
        /// <param name="depth">Matrix depth within the 3D tensor</param>
        /// <param name="rowY">Row within the matrix</param>
        /// <param name="columnX">Column within the matrix</param>
        public T this[int count, int depth, int rowY, int columnX]
        {
            get => _segment[count * TensorSize + depth * MatrixSize + rowY * ColumnCount + columnX];
            set => _segment[count * TensorSize + depth * MatrixSize + rowY * ColumnCount + columnX] = value;
        }

        /// <summary>
        /// Returns the value at the specified index
        /// </summary>
        /// <param name="count">3D tensor index</param>
        /// <param name="depth">Matrix depth within the 3D tensor</param>
        /// <param name="rowY">Row within the matrix</param>
        /// <param name="columnX">Column within the matrix</param>
        /// <returns></returns>
        public T this[uint count, uint depth, uint rowY, uint columnX]
        {
            get => _segment[count * TensorSize + depth * MatrixSize + rowY * ColumnCount + columnX];
            set => _segment[count * TensorSize + depth * MatrixSize + rowY * ColumnCount + columnX] = value;
        }

        /// <summary>
        /// Returns a nested 3D tensor
        /// </summary>
        /// <param name="index">Tensor index</param>
        public Tensor3D<T> Tensor(uint index)
        {
            var segment = new TensorSegmentWrapper<T>(_segment, index * TensorSize, 1, TensorSize);
            return new Tensor3D<T>(segment, Depth, RowCount, ColumnCount);
        }

        /// <summary>
        /// All 3D tensors
        /// </summary>
        public IEnumerable<Tensor3D<T>> Tensors => Depth.AsRange().Select(Tensor);

        /// <inheritdoc />
        public override string ToString() => $" Tensor4D (Count: {Count}, Depth: {Depth}, Rows: {RowCount}, Columns: {ColumnCount})";

        /// <inheritdoc />
        protected override Tensor4D<T> Create(ITensorSegment<T> segment)
        {
            return new Tensor4D<T>(segment, Count, Depth, RowCount, ColumnCount);
        }
    }
}
