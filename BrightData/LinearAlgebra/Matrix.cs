﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BrightData.Memory;

namespace BrightData.LinearAlgebra
{
    /// <summary>
    /// Matrix type
    /// </summary>
    /// <typeparam name="T">Data type within the matrix</typeparam>
    public class Matrix<T> : TensorBase<T, Matrix<T>>
        where T: struct
    {
        internal Matrix(ITensorSegment<T> segment, uint rows, uint columns) : base(segment, new[] { rows, columns }) { }
        internal Matrix(IBrightDataContext context, BinaryReader reader) : base(context, reader) { }

        /// <summary>
        /// Number of rows
        /// </summary>
        public uint RowCount => Shape[0];

        /// <summary>
        /// Number of columns
        /// </summary>
        public uint ColumnCount => Shape[1];

        /// <summary>
        /// Returns a row as a vector
        /// </summary>
        /// <param name="index">Row index</param>
        /// <returns></returns>
        public Vector<T> Row(uint index) => new(new TensorSegmentWrapper<T>(_segment, index * ColumnCount, 1, ColumnCount));

        /// <summary>
        /// Returns a column as a vector
        /// </summary>
        /// <param name="index">Column index</param>
        /// <returns></returns>
        public Vector<T> Column(uint index) => new(new TensorSegmentWrapper<T>(_segment, index, ColumnCount, RowCount));

        /// <summary>
        /// All rows
        /// </summary>
        public IEnumerable<Vector<T>> Rows => RowCount.AsRange().Select(Row);

        /// <summary>
        /// All columns
        /// </summary>
        public IEnumerable<Vector<T>> Columns => ColumnCount.AsRange().Select(Column);

        /// <summary>
        /// Returns the value at the specified index
        /// </summary>
        /// <param name="rowY">Row index</param>
        /// <param name="columnX">Column index</param>
        /// <returns></returns>
        public T this[int rowY, int columnX]
        {
            get => _segment[rowY * ColumnCount + columnX];
            set => _segment[rowY * ColumnCount + columnX] = value;
        }

        /// <summary>
        /// Returns the value at the specified index
        /// </summary>
        /// <param name="rowY">Row index</param>
        /// <param name="columnX">Column index</param>
        /// <returns></returns>
        public T this[uint rowY, uint columnX]
        {
            get => _segment[rowY * ColumnCount + columnX];
            set => _segment[rowY * ColumnCount + columnX] = value;
        }

        /// <inheritdoc />
        public override string ToString() => String.Format($"Matrix (Rows: {RowCount}, Columns: {ColumnCount})");

        /// <summary>
        /// Converts to a column major array format (default is row major)
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Transpose the matrix
        /// </summary>
        /// <returns></returns>
        public Matrix<T> Transpose()
        {
            var ret = new Matrix<T>(Context.CreateSegment<T>(Size), ColumnCount, RowCount);
            Parallel.For(0, ret.Size, ind => {
                var j = (uint)(ind / ColumnCount);
                var i = (uint)(ind % ColumnCount);
                ret[i, j] = this[j, i];
            });
            return ret;
        }

        /// <summary>
        /// Multiply the matrix with another matrix
        /// </summary>
        /// <param name="other">Other matrix to multiply</param>
        /// <returns></returns>
        public Matrix<T> Multiply(Matrix<T> other)
        {
            var ret = new Matrix<T>(Context.CreateSegment<T>(RowCount * other.ColumnCount), RowCount, other.ColumnCount);
            Parallel.For(0, ret.Size, ind => {
            //for (uint ind = 0; ind < ret.Size; ind++) {
                var i = (uint) (ind % RowCount);
                var j = (uint) (ind / RowCount);
                var val = Row(i).DotProduct(other.Column(j));
                ret[i, j] = val;
            //}
            });
            return ret;
        }

        /// <summary>
        /// Returns the diagonal of the matrix as a vector
        /// </summary>
        /// <returns></returns>
        public Vector<T> GetDiagonal()
        {
            if(RowCount != ColumnCount)
                throw new Exception("Diagonal can only be found from square matrices");
            return Context.CreateVector(RowCount, i => this[i, i]);
        }

        /// <summary>
        /// Returns the sum of each row as a vector
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Vector<T> RowSums() => Context.CreateVector(Rows.Select(r => r.Sum()).ToArray());

        /// <summary>
        /// Returns the sum of each column as a vector
        /// </summary>
        /// <returns></returns>
        public Vector<T> ColumnSums() => Context.CreateVector(Columns.Select(r => r.Sum()).ToArray());

        /// <summary>
        /// Multiplies the matrix with a vector
        /// </summary>
        /// <param name="vector">Vector to multiply</param>
        /// <returns></returns>
        public Matrix<T> Multiply(Vector<T> vector) => Multiply(vector.Reshape(null, 1));

        /// <inheritdoc />
        protected override Matrix<T> Create(ITensorSegment<T> segment) => new(segment, RowCount, ColumnCount);

        /// <summary>
        /// Applies a mapping function to each value within the matrix - creating a new matrix as a result
        /// </summary>
        /// <param name="mutator">Mapping function</param>
        /// <returns></returns>
        public Matrix<T> Map(Func<T, T> mutator)
        {
            var ret = MapParallel((_, v) => mutator(v));
            return new Matrix<T>(ret, RowCount, ColumnCount);
        }

        /// <summary>
        /// Applies a mapping function to each value within the matrix
        /// </summary>
        /// <param name="mutator">Mapping function</param>
        public void MapInPlace(Func<T, T> mutator)
        {
            using var ret = MapParallel((_, v) => mutator(v));
            ret.CopyTo(_segment);
        }

        /// <summary>
        /// Applies an indexed mapping function to each value within the matrix - creating a new matrix as a result
        /// </summary>
        /// <param name="mutator">Mapping function</param>
        /// <returns></returns>
        public Matrix<T> MapIndexed(Func<uint, uint, T, T> mutator)
        {
            var ret = MapParallel((ind, val) => {
                var i = ind / ColumnCount;
                var j = ind % ColumnCount;
                return mutator(i, j, val);
            });
            return new Matrix<T>(ret, RowCount, ColumnCount);
        }

        /// <summary>
        /// Applies an indexed mapping function to each value within the matrix
        /// </summary>
        /// <param name="mutator">Mapping function</param>
        public void MapIndexedInPlace(Func<uint, uint, T, T> mutator)
        {
            using var ret = MapParallel((ind, val) => {
                var i = ind / ColumnCount;
                var j = ind % ColumnCount;
                return mutator(i, j, val);
            });
            ret.CopyTo(_segment);
        }
    }
}
