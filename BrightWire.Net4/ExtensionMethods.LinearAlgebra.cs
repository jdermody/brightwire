using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire
{
    public static partial class ExtensionMethods
    {
        public static IVector CreateVector(this ILinearAlgebraProvider lap, IIndexableVector data)
        {
            return lap.CreateVector(data.Count, i => data[i]);
        }

        public static IVector CreateVector(this ILinearAlgebraProvider lap, FloatVector data)
        {
            var array = data.Data;
            return lap.CreateVector(array.Length, i => array[i]);
        }

        public static IVector CreateVector(this ILinearAlgebraProvider lap, IReadOnlyList<float> data)
        {
            return lap.CreateVector(data.Count, i => data[i]);
        }

        public static IVector CreateVector(this ILinearAlgebraProvider lap, int length, float value = 0f)
        {
            return lap.CreateVector(length, i => value);
        }

        public static IMatrix CreateMatrix(this ILinearAlgebraProvider lap, FloatMatrix matrix)
        {
            return lap.CreateMatrix(matrix.RowCount, matrix.ColumnCount, (i, j) => matrix.Row[i].Data[j]);
        }

        public static IMatrix CreateMatrix(this ILinearAlgebraProvider lap, IReadOnlyList<FloatVector> rowList)
        {
            int rows = rowList.Count;
            var size = rowList[0].Size;
            return lap.CreateMatrix(rows, size, (x, y) => rowList[x].Data[y]);
        }

        public static IMatrix CreateMatrix(this ILinearAlgebraProvider lap, IReadOnlyList<IIndexableVector> rowList)
        {
            int rows = rowList.Count;
            var size = rowList[0].Count;
            return lap.CreateMatrix(rows, size, (x, y) => rowList[x][y]);
        }

        public static IMatrix CreateMatrix(this ILinearAlgebraProvider lap, int rows, int columns, float value = 0f)
        {
            return lap.CreateMatrix(rows, columns, (i, j) => value);
        }

        public static IMatrix CreateMatrix(this ILinearAlgebraProvider lap, IIndexableMatrix matrix)
        {
            return lap.CreateMatrix(matrix.RowCount, matrix.ColumnCount, (i, j) => matrix[i, j]);
        }

        public static IMatrix CreateIdentityMatrix(this ILinearAlgebraProvider lap, int size)
        {
            return lap.CreateMatrix(size, size, (x, y) => x == y ? 1f : 0f);
        }

        public static IMatrix CreateDiagonalMatrix(this ILinearAlgebraProvider lap, IReadOnlyList<float> values)
        {
            return lap.CreateMatrix(values.Count, values.Count, (x, y) => x == y ? values[x] : 0f);
        }

        /// <summary>
        /// Creates a 3D tensor
        /// </summary>
        /// <param name="tensor">An indexable 3D tensor to use as a source</param>
        public static I3DTensor CreateTensor(this ILinearAlgebraProvider lap, IIndexable3DTensor tensor)
        {
            return lap.CreateTensor(tensor.Matrix.Select(m => CreateMatrix(lap, m)).ToList());
        }

        /// <summary>
        /// Creates a 3D tensor
        /// </summary>
        /// <param name="tensor">The serialised representation of the 3D tensor</param>
        /// <returns></returns>
        public static I3DTensor CreateTensor(this ILinearAlgebraProvider lap, FloatTensor tensor)
        {
            return lap.CreateTensor(tensor.Matrix.Select(m => CreateMatrix(lap, m)).ToList());
        }

        public static I3DTensor ConvertToTensor(this IVector vector, ILinearAlgebraProvider lap, int rows, int columns, int depth)
        {
            if (depth > 1) {
                var matrixList = new List<IMatrix>();
                var slice = vector.Split(depth);
                foreach (var part in slice)
                    matrixList.Add(part.ConvertInPlaceToMatrix(rows, columns));
                var ret = lap.CreateTensor(matrixList);
                return ret;
            } else {
                var matrix = vector.ConvertInPlaceToMatrix(rows, columns);
                return lap.CreateTensor(new[] { matrix });
            }
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
