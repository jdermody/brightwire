using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BrightWire
{
    public static partial class ExtensionMethods
    {
        /// <summary>
        /// Create a vector
        /// </summary>
        /// <param name="lap"></param>
        /// <param name="data">Indexable vector to copy</param>
        /// <returns></returns>
        public static IVector CreateVector(this ILinearAlgebraProvider lap, IIndexableVector data)
        {
            return lap.CreateVector(data.Count, i => data[i]);
        }

        /// <summary>
        /// Create a vector
        /// </summary>
        /// <param name="lap"></param>
        /// <param name="data">Vector to copy</param>
        /// <returns></returns>
        public static IVector CreateVector(this ILinearAlgebraProvider lap, FloatVector data)
        {
            var array = data.Data;
            return lap.CreateVector(array.Length, i => array[i]);
        }

        /// <summary>
        /// Create a vector
        /// </summary>
        /// <param name="lap"></param>
        /// <param name="data">List of values</param>
        /// <returns></returns>
        public static IVector CreateVector(this ILinearAlgebraProvider lap, IReadOnlyList<float> data)
        {
            return lap.CreateVector(data.Count, i => data[i]);
        }

        /// <summary>
        /// Create a vector
        /// </summary>
        /// <param name="lap"></param>
        /// <param name="length">Vector size</param>
        /// <param name="value">Constant value</param>
        /// <returns></returns>
        public static IVector CreateVector(this ILinearAlgebraProvider lap, int length, float value = 0f)
        {
            return lap.CreateVector(length, i => value);
        }

        /// <summary>
        /// Create a matrix
        /// </summary>
        /// <param name="lap"></param>
        /// <param name="matrix">Matrix to copy</param>
        /// <returns></returns>
        public static IMatrix CreateMatrix(this ILinearAlgebraProvider lap, FloatMatrix matrix)
        {
            return lap.CreateMatrix(matrix.RowCount, matrix.ColumnCount, (i, j) => matrix.Row[i].Data[j]);
        }

        /// <summary>
        /// Create a matrix
        /// </summary>
        /// <param name="lap"></param>
        /// <param name="rowList">List of vectors (each vector becomes a row in the new matrix)</param>
        /// <returns></returns>
        public static IMatrix CreateMatrix(this ILinearAlgebraProvider lap, IReadOnlyList<FloatVector> rowList)
        {
            int rows = rowList.Count;
            var size = rowList[0].Size;
            return lap.CreateMatrix(rows, size, (x, y) => rowList[x].Data[y]);
        }

        /// <summary>
        /// Create a matrix
        /// </summary>
        /// <param name="lap"></param>
        /// <param name="rowList">List of indexable vectors (each vector becomes a row in the new matrix)</param>
        /// <returns></returns>
        public static IMatrix CreateMatrix(this ILinearAlgebraProvider lap, IReadOnlyList<IIndexableVector> rowList)
        {
            int rows = rowList.Count;
            var size = rowList[0].Count;
            return lap.CreateMatrix(rows, size, (x, y) => rowList[x][y]);
        }

        /// <summary>
        /// Create a matrix
        /// </summary>
        /// <param name="lap"></param>
        /// <param name="rows">Matrix rows</param>
        /// <param name="columns">Matrix columns</param>
        /// <param name="value">Constant value</param>
        /// <returns></returns>
        public static IMatrix CreateMatrix(this ILinearAlgebraProvider lap, int rows, int columns, float value)
        {
            return lap.CreateMatrix(rows, columns, (i, j) => value);
        }

        /// <summary>
        /// Create a matrix
        /// </summary>
        /// <param name="lap"></param>
        /// <param name="matrix">Indexable matrix to copy</param>
        /// <returns></returns>
        public static IMatrix CreateMatrix(this ILinearAlgebraProvider lap, IIndexableMatrix matrix)
        {
            return lap.CreateMatrix(matrix.RowCount, matrix.ColumnCount, (i, j) => matrix[i, j]);
        }

        /// <summary>
        /// Create an identity matrix
        /// </summary>
        /// <param name="lap"></param>
        /// <param name="size">Width and height of the new matrix</param>
        /// <returns></returns>
        public static IMatrix CreateIdentityMatrix(this ILinearAlgebraProvider lap, int size)
        {
            return lap.CreateMatrix(size, size, (x, y) => x == y ? 1f : 0f);
        }

        /// <summary>
        /// Create a diagonal matrix
        /// </summary>
        /// <param name="lap"></param>
        /// <param name="values">List of diagonal values</param>
        /// <returns></returns>
        public static IMatrix CreateDiagonalMatrix(this ILinearAlgebraProvider lap, IReadOnlyList<float> values)
        {
            return lap.CreateMatrix(values.Count, values.Count, (x, y) => x == y ? values[x] : 0f);
        }

        /// <summary>
        /// Create a 3D tensor
        /// </summary>
        /// <param name="lap"></param>
        /// <param name="tensor">An indexable 3D tensor to use as a source</param>
        public static I3DTensor Create3DTensor(this ILinearAlgebraProvider lap, IIndexable3DTensor tensor)
        {
            return lap.Create3DTensor(tensor.Matrix.Select(m => CreateMatrix(lap, m)).ToList());
        }

        /// <summary>
        /// Create a 3D tensor
        /// </summary>
        /// <param name="lap"></param>
        /// <param name="tensor">The serialised representation of the 3D tensor</param>
        /// <returns></returns>
        public static I3DTensor Create3DTensor(this ILinearAlgebraProvider lap, FloatTensor tensor)
        {
            return lap.Create3DTensor(tensor.Matrix.Select(m => CreateMatrix(lap, m)).ToList());
        }

        /// <summary>
        /// Calculates the distance between two matrices
        /// </summary>
        /// <param name="distance"></param>
        /// <param name="matrix1"></param>
        /// <param name="matrix2"></param>
        /// <returns></returns>
        public static IVector Calculate(this DistanceMetric distance, IMatrix matrix1, IMatrix matrix2)
        {
            switch (distance) {
                case DistanceMetric.Euclidean:
                    using (var diff = matrix1.Subtract(matrix2))
                    using (var diffSquared = diff.PointwiseMultiply(diff))
                    using (var rowSums = diffSquared.RowSums()) {
                        return rowSums.Sqrt();
                    }
                case DistanceMetric.SquaredEuclidean:
                    using (var diff = matrix1.Subtract(matrix2))
                    using (var diffSquared = diff.PointwiseMultiply(diff)) {
                        return diffSquared.RowSums();
                    }
                case DistanceMetric.Cosine:
                case DistanceMetric.Manhattan:
                case DistanceMetric.MeanSquared:
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Calculates the distance between two vectors
        /// </summary>
        /// <param name="distance"></param>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        public static float Calculate(this DistanceMetric distance, IVector vector1, IVector vector2)
        {
            switch (distance) {
                case DistanceMetric.Cosine:
                    return vector1.CosineDistance(vector2);
                case DistanceMetric.Euclidean:
                    return vector1.EuclideanDistance(vector2);
                case DistanceMetric.Manhattan:
                    return vector1.ManhattanDistance(vector2);
                case DistanceMetric.SquaredEuclidean:
                    return vector1.SquaredEuclidean(vector2);
                default:
                    return vector1.MeanSquaredDistance(vector2);
            }
        }
    }
}
