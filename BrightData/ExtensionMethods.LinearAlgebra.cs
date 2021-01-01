﻿using System;
using System.Collections.Generic;
using System.Linq;
using BrightData.FloatTensors;

namespace BrightData
{
    public static partial class ExtensionMethods
    {
        /// <summary>
        /// Creates a vector based on an enumerable of floats
        /// </summary>
        /// <param name="lap"></param>
        /// <param name="data">The initial values in the vector</param>
        /// <returns></returns>
        public static IFloatVector CreateVector(this ILinearAlgebraProvider lap, IEnumerable<float> data) => CreateVector(lap, data.ToArray());

        /// <summary>
		/// Create a vector
		/// </summary>
		/// <param name="lap"></param>
		/// <param name="data">Indexable vector to copy</param>
		/// <returns></returns>
		public static IFloatVector CreateVector(this ILinearAlgebraProvider lap, IIndexableFloatVector data)
        {
            return lap.CreateVector(data.Count, i => data[i]);
        }

        public static IFloatVector CreateVector(this ILinearAlgebraProvider lap, IFloatVector data) => CreateVector(lap, data.Data);

        /// <summary>
        /// Create a vector
        /// </summary>
        /// <param name="lap"></param>
        /// <param name="data">Vector to copy</param>
        /// <returns></returns>
        public static IFloatVector CreateVector(this ILinearAlgebraProvider lap, Vector<float> data)
        {
            //var array = data.Segment;
            //return lap.CreateVector(array.Length, i => array[i]);

	        var ret = lap.CreateVector(data.Size);
	        ret.Data = data;
	        return ret;
        }

        /// <summary>
        /// Create a vector
        /// </summary>
        /// <param name="lap"></param>
        /// <param name="data">List of values</param>
        /// <returns></returns>
        public static IFloatVector CreateVector(this ILinearAlgebraProvider lap, params float[] data)
        {
            return lap.CreateVector((uint)data.Length, i => data[i]);
        }

        /// <summary>
        /// Create a vector
        /// </summary>
        /// <param name="lap"></param>
        /// <param name="length">Vector size</param>
        /// <param name="value">Constant value</param>
        /// <returns></returns>
        public static IFloatVector CreateVector(this ILinearAlgebraProvider lap, uint length, float value = 0f)
        {
            return lap.CreateVector(length, i => value);
        }

		/// <summary>
		/// Creates a matrix with every value initialised to zero
		/// </summary>
		/// <param name="lap"></param>
		/// <param name="rows">Number of rows</param>
		/// <param name="columns">Numer of columns</param>
	    public static IFloatMatrix CreateZeroMatrix(this ILinearAlgebraProvider lap, uint rows, uint columns) => lap.CreateMatrix(rows, columns, true);

		/// <summary>
		/// Create a matrix
		/// </summary>
		/// <param name="lap"></param>
		/// <param name="matrix">Matrix to copy</param>
		/// <returns></returns>
		public static IFloatMatrix CreateMatrix(this ILinearAlgebraProvider lap, Matrix<float> matrix)
		{
			var ret = lap.CreateMatrix(matrix.RowCount, matrix.ColumnCount);
			ret.Data = matrix;
			return ret;
		}

        /// <summary>
        /// Create a matrix from a list of row vectors
        /// </summary>
        /// <param name="lap"></param>
        /// <param name="rowList">List of vectors (each vector becomes a row in the new matrix)</param>
        /// <returns></returns>
        public static IFloatMatrix CreateMatrixFromRows(this ILinearAlgebraProvider lap, Vector<float>[] rowList)
        {
            var rows = (uint)rowList.Length;
            var columns = rowList[0].Size;
            return lap.CreateMatrix(rows, columns, (x, y) => rowList[x].Segment[y]);
        }

        /// <summary>
        /// Create a matrix from a list of row vectors
        /// </summary>
        /// <param name="lap"></param>
        /// <param name="rowList">List of indexable vectors (each vector becomes a row in the new matrix)</param>
        /// <returns></returns>
        public static IFloatMatrix CreateMatrixFromRows(this ILinearAlgebraProvider lap, IIndexableFloatVector[] rowList)
        {
            var rows = (uint)rowList.Length;
            var columns = rowList[0].Count;
            return lap.CreateMatrix(rows, columns, (x, y) => rowList[x][y]);
        }

	    /// <summary>
	    /// Create a matrix from a list of column vectors
	    /// </summary>
	    /// <param name="lap"></param>
	    /// <param name="columnList">List of vectors (each vector becomes a column in the new matrix)</param>
	    /// <returns></returns>
	    public static IFloatMatrix CreateMatrixFromColumns(this ILinearAlgebraProvider lap, Vector<float>[] columnList)
	    {
		    var columns = (uint)columnList.Length;
		    var rows = columnList[0].Size;
		    return lap.CreateMatrix(rows, columns, (x, y) => columnList[y].Segment[x]);
	    }

	    /// <summary>
	    /// Create a matrix from a list of column vectors
	    /// </summary>
	    /// <param name="lap"></param>
	    /// <param name="columnList">List of indexable vectors (each vector becomes a column in the new matrix)</param>
	    /// <returns></returns>
	    public static IFloatMatrix CreateMatrixFromColumns(this ILinearAlgebraProvider lap, IIndexableFloatVector[] columnList)
	    {
		    var columns = (uint)columnList.Length;
		    var rows = columnList[0].Count;
		    return lap.CreateMatrix(rows, columns, (x, y) => columnList[y][x]);
	    }

        /// <summary>
        /// Create a matrix
        /// </summary>
        /// <param name="lap"></param>
        /// <param name="rows">Matrix rows</param>
        /// <param name="columns">Matrix columns</param>
        /// <param name="value">Constant value</param>
        /// <returns></returns>
        public static IFloatMatrix CreateMatrix(this ILinearAlgebraProvider lap, uint rows, uint columns, float value)
        {
            return lap.CreateMatrix(rows, columns, (i, j) => value);
        }

        /// <summary>
        /// Create a matrix
        /// </summary>
        /// <param name="lap"></param>
        /// <param name="matrix">Indexable matrix to copy</param>
        /// <returns></returns>
        public static IFloatMatrix CreateMatrix(this ILinearAlgebraProvider lap, IIndexableFloatMatrix matrix)
        {
            return lap.CreateMatrix(matrix.RowCount, matrix.ColumnCount, (i, j) => matrix[i, j]);
        }

        /// <summary>
        /// Create an identity matrix
        /// </summary>
        /// <param name="lap"></param>
        /// <param name="size">Width and height of the new matrix</param>
        /// <returns></returns>
        public static IFloatMatrix CreateIdentityMatrix(this ILinearAlgebraProvider lap, uint size)
        {
            return lap.CreateMatrix(size, size, (x, y) => x == y ? 1f : 0f);
        }

        /// <summary>
        /// Create a diagonal matrix
        /// </summary>
        /// <param name="lap"></param>
        /// <param name="values">List of diagonal values</param>
        /// <returns></returns>
        public static IFloatMatrix CreateDiagonalMatrix(this ILinearAlgebraProvider lap, params float[] values)
        {
            return lap.CreateMatrix((uint)values.Length, (uint)values.Length, (x, y) => x == y ? values[x] : 0f);
        }

        /// <summary>
        /// Create a 3D tensor
        /// </summary>
        /// <param name="lap"></param>
        /// <param name="tensor">The serialised representation of the 3D tensor</param>
        /// <returns></returns>
        public static I3DFloatTensor Create3DTensor(this ILinearAlgebraProvider lap, Tensor3D<float> tensor)
        {
	        var ret = lap.Create3DTensor(tensor.RowCount, tensor.ColumnCount, tensor.Depth);
	        ret.Data = tensor;
	        return ret;
	        //return lap.Create3DTensor(tensor.Matrix.Select(m => CreateMatrix(lap, m)).ToList());
        }

	    /// <summary>
	    /// Creates a 3D tensor from a list of matrices
	    /// </summary>
	    /// <param name="lap"></param>
	    /// <param name="matrices">List of matrices</param>
	    /// <returns></returns>
	    public static I3DFloatTensor Create3DTensor(this ILinearAlgebraProvider lap, Matrix<float>[] matrices)
	    {
		    var first = matrices[0];
		    var ret = lap.Create3DTensor(first.RowCount, first.ColumnCount, (uint)matrices.Length);
		    ret.Data = Float3DTensor.Create(lap.Context, matrices);
		    return ret;
	    }

        /// <summary>
        /// Calculates the distance between two matrices
        /// </summary>
        /// <param name="distance"></param>
        /// <param name="matrix1"></param>
        /// <param name="matrix2"></param>
        /// <returns></returns>
        public static IFloatVector Calculate(this DistanceMetric distance, IFloatMatrix matrix1, IFloatMatrix matrix2)
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
        public static float Calculate(this DistanceMetric distance, IFloatVector vector1, IFloatVector vector2)
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

        public static IFloatMatrix CreateMatrixFromRows(this ILinearAlgebraProvider lap, IEnumerable<Vector<float>> rows) => 
            lap.CreateMatrixFromRows(rows.ToArray());
        public static IFloatMatrix CreateMatrixFromRows(this ILinearAlgebraProvider lap, IEnumerable<IFloatVector> rows) =>
            lap.CreateMatrixFromRows(rows.ToArray());

        public static IFloatMatrix CreateMatrixFromColumns(this ILinearAlgebraProvider lap, IEnumerable<Vector<float>> columns) =>
            lap.CreateMatrixFromColumns(columns.ToArray());
        public static IFloatMatrix CreateMatrixFromColumns(this ILinearAlgebraProvider lap, IEnumerable<IFloatVector> columns) =>
            lap.CreateMatrixFromColumns(columns.ToArray());

        //public static IFloatMatrix Create3DTensor(this ILinearAlgebraProvider lap, IEnumerable<IFloatMatrix> columns) =>
        //    lap.CreateMatrixFromColumns(columns.ToArray());
    }
}