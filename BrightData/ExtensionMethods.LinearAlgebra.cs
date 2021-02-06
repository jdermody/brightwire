using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BrightData.Helper;
using BrightData.LinearAlgebra;
using BrightData.Memory;

namespace BrightData
{
    public static partial class ExtensionMethods
    {
        /// <summary>
        /// Creates a vector
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="size">Size of vector</param>
        /// <param name="initializer">Callback to initialize each value (optional)</param>
        /// <returns></returns>
        public static Vector<T> CreateVector<T>(this IBrightDataContext context, uint size, Func<uint, T>? initializer) where T : struct
        {
            var segment = context.CreateSegment<T>(size);
            if (initializer != null)
                segment.InitializeFrom(initializer);
            return new Vector<T>(segment);
        }

        /// <summary>
        /// Creates a vector
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="size">Size of vector</param>
        /// <param name="initialValue">Initial value of each element</param>
        /// <returns></returns>
        public static Vector<T> CreateVector<T>(this IBrightDataContext context, uint size, T initialValue = default) where T : struct
        {
            var segment = context.CreateSegment<T>(size);
            segment.InitializeTo(initialValue);
            return new Vector<T>(segment);
        }

        /// <summary>
        /// Creates a vector
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="initialData">Initial data</param>
        /// <returns></returns>
        public static Vector<T> CreateVector<T>(this IBrightDataContext context, params T[] initialData) where T : struct
        {
            var segment = context.CreateSegment<T>((uint)initialData.Length);
            if (initialData.Any())
                segment.Initialize(initialData);
            return new Vector<T>(segment);
        }

        /// <summary>
        /// Creates a vector from a binary reader
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static Vector<T> CreateVector<T>(this IBrightDataContext context, BinaryReader reader) where T : struct
        {
            return new Vector<T>(context, reader);
        }


        /// <summary>
        /// Creates a matrix
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="rows">Number of rows</param>
        /// <param name="columns">Number of columns</param>
        /// <param name="initializer">Callback to initialize each value (optional)</param>
        /// <returns></returns>
        public static Matrix<T> CreateMatrix<T>(this IBrightDataContext context, uint rows, uint columns, Func<uint, uint, T>? initializer = null) where T : struct
        {
            var segment = context.CreateSegment<T>(rows * columns);
            if (initializer != null)
                segment.InitializeFrom(i => initializer(i / columns, i % columns));
            return new Matrix<T>(segment, rows, columns);
        }

        /// <summary>
        /// Creates a matrix
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="rows">Number of rows</param>
        /// <param name="columns">Number of columns</param>
        /// <param name="initialValue">Initial value of each element</param>
        /// <returns></returns>
        public static Matrix<T> CreateMatrix<T>(this IBrightDataContext context, uint rows, uint columns, T initialValue) where T : struct
        {
            var segment = context.CreateSegment<T>(rows * columns);
            segment.InitializeTo(initialValue);
            return new Matrix<T>(segment, rows, columns);
        }

        /// <summary>
        /// Creates a matrix from a binary reader
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static Matrix<T> CreateMatrix<T>(this IBrightDataContext context, BinaryReader reader) where T : struct
        {
            return new Matrix<T>(context, reader);
        }

        /// <summary>
        /// Creates a matrix from vectors (each will become a row)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        public static Matrix<T> CreateMatrixFromRows<T>(this IBrightDataContext context, params Vector<T>[] rows) where T : struct
        {
            var columns = rows.First().Size;
            return CreateMatrix(context, (uint)rows.Length, columns, (j, i) => rows[j][i]);
        }

        /// <summary>
        /// Creates a matrix from rows (each will become a row)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        public static Matrix<T> CreateMatrixFromRows<T>(this IBrightDataContext context, params T[][] rows) where T : struct
        {
            var columns = (uint)rows.First().Length;
            return CreateMatrix(context, (uint)rows.Length, columns, (j, i) => rows[j][i]);
        }

        /// <summary>
        /// Creates a matrix from vectors (each will become a column)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static Matrix<T> CreateMatrixFromColumns<T>(this IBrightDataContext context, params Vector<T>[] columns) where T : struct
        {
            var rows = columns.First().Size;
            return CreateMatrix(context, rows, (uint)columns.Length, (j, i) => columns[i][j]);
        }

        /// <summary>
        /// Creates a matrix from vectors (each will become a column)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static Matrix<T> CreateMatrixFromColumns<T>(this IBrightDataContext context, params T[][] columns) where T : struct
        {
            var rows = (uint)columns.First().Length;
            return CreateMatrix(context, rows, (uint)columns.Length, (j, i) => columns[i][j]);
        }

        /// <summary>
        /// Creates a 3D tensor
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="depth">Number of matrices</param>
        /// <param name="rows">Number of rows in each matrix</param>
        /// <param name="columns">Number of columns in each matrix</param>
        /// <returns></returns>
        public static Tensor3D<T> CreateTensor3D<T>(this IBrightDataContext context, uint depth, uint rows, uint columns) where T : struct
        {
            var segment = context.CreateSegment<T>(depth * rows * columns);
            return new Tensor3D<T>(segment, depth, rows, columns);
        }

        /// <summary>
        /// Creates a 3D tensor from matrices
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="slices"></param>
        /// <returns></returns>
        public static Tensor3D<T> CreateTensor3D<T>(this IBrightDataContext context, params Matrix<T>[] slices) where T : struct
        {
            var first = slices.First();
            var depth = (uint)slices.Length;
            var rows = first.RowCount;
            var columns = first.ColumnCount;

            var data = context.CreateSegment<T>(depth * rows * columns);
            var ret = new Tensor3D<T>(data, depth, rows, columns);
            var allSame = ret.Matrices.Zip(slices, (t, s) => {
                if (s.RowCount == t.RowCount && s.ColumnCount == t.ColumnCount)
                {
                    s.Segment.CopyTo(t.Segment);
                    return true;
                }
                return false;
            }).All(v => v);
            if (!allSame)
                throw new ArgumentException("Input matrices had different sizes");
            return ret;
        }

        /// <summary>
        /// Create a 3D tensor from a binary reader
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static Tensor3D<T> CreateTensor3D<T>(this IBrightDataContext context, BinaryReader reader) where T : struct
        {
            return new Tensor3D<T>(context, reader);
        }

        /// <summary>
        /// Creates a 4D tensor
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="count">Number of 3D tensors</param>
        /// <param name="depth">Number of matrices in each 3D tensor</param>
        /// <param name="rows">Number of rows in each matrix</param>
        /// <param name="columns">Number of columns in each matrix</param>
        /// <returns></returns>
        public static Tensor4D<T> CreateTensor4D<T>(this IBrightDataContext context, uint count, uint depth, uint rows, uint columns) where T : struct
        {
            var segment = context.CreateSegment<T>(count * depth * rows * columns);
            return new Tensor4D<T>(segment, count, depth, rows, columns);
        }

        /// <summary>
        /// Creates a 4D tensor from a binary reader
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static Tensor4D<T> CreateTensor4D<T>(this IBrightDataContext context, BinaryReader reader) where T : struct
        {
            return new Tensor4D<T>(context, reader);
        }

        /// <summary>
        /// Returns the number of columns in this tensor
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public static uint GetColumnCount<T>(this ITensor<T> tensor) where T : struct
        {
            return tensor.Shape.Length > 1 ? tensor.Shape[^1] : 0;
        }

        /// <summary>
        /// Returns the number of rows in this tensor
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public static uint GetRowCount<T>(this ITensor<T> tensor) where T : struct
        {
            return tensor.Shape.Length > 1 ? tensor.Shape[^2] : 0;
        }

        /// <summary>
        /// Returns the number of matrices in this tensor
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public static uint GetDepth<T>(this ITensor<T> tensor) where T : struct
        {
            return tensor.Shape.Length > 2 ? tensor.Shape[^3] : 0;
        }

        /// <summary>
        /// Returns the number of 3D tensors in this tensor
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public static uint GetCount<T>(this ITensor<T> tensor) where T : struct
        {
            return tensor.Shape.Length > 3 ? tensor.Shape[^4] : 0;
        }

        /// <summary>
        /// Creates a vector from an enumerable of floats
        /// </summary>
        /// <param name="lap"></param>
        /// <param name="data">The initial values in the vector</param>
        /// <returns></returns>
        public static IFloatVector CreateVector(this ILinearAlgebraProvider lap, IEnumerable<float> data) => CreateVector(lap, data.ToArray());

        /// <summary>
		/// Creates a vector
		/// </summary>
		/// <param name="lap"></param>
		/// <param name="data">Indexable vector to copy</param>
		/// <returns></returns>
		public static IFloatVector CreateVector(this ILinearAlgebraProvider lap, IIndexableFloatVector data)
        {
            return lap.CreateVector(data.Count, i => data[i]);
        }

        /// <summary>
        /// Creates a vector
        /// </summary>
        /// <param name="lap"></param>
        /// <param name="data">Vector to copy</param>
        /// <returns></returns>
        public static IFloatVector CreateVector(this ILinearAlgebraProvider lap, IFloatVector data) => CreateVector(lap, data.Data);

        /// <summary>
        /// Creates a vector
        /// </summary>
        /// <param name="lap"></param>
        /// <param name="data">Vector to copy</param>
        /// <returns></returns>
        public static IFloatVector CreateVector(this ILinearAlgebraProvider lap, Vector<float> data)
        {
            var ret = lap.CreateVector(data.Size);
	        ret.Data = data;
	        return ret;
        }

        /// <summary>
        /// Creates a vector
        /// </summary>
        /// <param name="lap"></param>
        /// <param name="data">List of values</param>
        /// <returns></returns>
        public static IFloatVector CreateVector(this ILinearAlgebraProvider lap, params float[] data)
        {
            return lap.CreateVector((uint)data.Length, i => data[i]);
        }

        /// <summary>
        /// Creates a vector
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
		/// Creates a matrix with every element initialized to zero
		/// </summary>
		/// <param name="lap"></param>
		/// <param name="rows">Number of rows</param>
		/// <param name="columns">Number of columns</param>
	    public static IFloatMatrix CreateZeroMatrix(this ILinearAlgebraProvider lap, uint rows, uint columns) => lap.CreateMatrix(rows, columns, true);

		/// <summary>
		/// Creates a matrix from an existing matrix
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
        /// Creates a matrix from row vectors
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
        /// Creates a matrix from row vectors
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
	    /// Creates a matrix from column vectors
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
	    /// Creates a matrix column vectors
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
        /// Creates a matrix
        /// </summary>
        /// <param name="lap"></param>
        /// <param name="rows">Number of rows</param>
        /// <param name="columns">Number of columns</param>
        /// <param name="value">Constant value to initialize each element</param>
        /// <returns></returns>
        public static IFloatMatrix CreateMatrix(this ILinearAlgebraProvider lap, uint rows, uint columns, float value)
        {
            return lap.CreateMatrix(rows, columns, (i, j) => value);
        }

        /// <summary>
        /// Creates a matrix
        /// </summary>
        /// <param name="lap"></param>
        /// <param name="matrix">Indexable matrix to copy</param>
        /// <returns></returns>
        public static IFloatMatrix CreateMatrix(this ILinearAlgebraProvider lap, IIndexableFloatMatrix matrix)
        {
            return lap.CreateMatrix(matrix.RowCount, matrix.ColumnCount, (i, j) => matrix[i, j]);
        }

        /// <summary>
        /// Creates an identity matrix (each diagonal element is 1, each other element is 0)
        /// </summary>
        /// <param name="lap"></param>
        /// <param name="size">Width and height of the new matrix</param>
        /// <returns></returns>
        public static IFloatMatrix CreateIdentityMatrix(this ILinearAlgebraProvider lap, uint size)
        {
            return lap.CreateMatrix(size, size, (x, y) => x == y ? 1f : 0f);
        }

        /// <summary>
        /// Creates a diagonal matrix
        /// </summary>
        /// <param name="lap"></param>
        /// <param name="values">Diagonal values</param>
        /// <returns></returns>
        public static IFloatMatrix CreateDiagonalMatrix(this ILinearAlgebraProvider lap, params float[] values)
        {
            return lap.CreateMatrix((uint)values.Length, (uint)values.Length, (x, y) => x == y ? values[x] : 0f);
        }

        /// <summary>
        /// Creates a 3D tensor
        /// </summary>
        /// <param name="lap"></param>
        /// <param name="tensor">The tensor to copy from</param>
        /// <returns></returns>
        public static I3DFloatTensor Create3DTensor(this ILinearAlgebraProvider lap, Tensor3D<float> tensor)
        {
	        var ret = lap.Create3DTensor(tensor.RowCount, tensor.ColumnCount, tensor.Depth);
	        ret.Data = tensor;
	        return ret;
        }

	    /// <summary>
	    /// Creates a 3D tensor from matrices
	    /// </summary>
	    /// <param name="lap"></param>
	    /// <param name="matrices">Matrices to copy fropm</param>
	    /// <returns></returns>
	    public static I3DFloatTensor Create3DTensor(this ILinearAlgebraProvider lap, Matrix<float>[] matrices)
	    {
		    var first = matrices[0];
		    var ret = lap.Create3DTensor(first.RowCount, first.ColumnCount, (uint)matrices.Length);
		    ret.Data = lap.Context.CreateTensor3D(matrices);
		    return ret;
	    }

        /// <summary>
        /// Calculates the distance between two matrices
        /// </summary>
        /// <param name="distance">Distance metric (either euclidean or square euclidean)</param>
        /// <param name="matrix1">First matrix</param>
        /// <param name="matrix2">Second matrix</param>
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
        /// <param name="distance">Distance metric</param>
        /// <param name="vector1">First vector</param>
        /// <param name="vector2">Second vector</param>
        public static float Calculate(this DistanceMetric distance, IFloatVector vector1, IFloatVector vector2)
        {
            return distance switch {
                DistanceMetric.Cosine => vector1.CosineDistance(vector2),
                DistanceMetric.Euclidean => vector1.EuclideanDistance(vector2),
                DistanceMetric.Manhattan => vector1.ManhattanDistance(vector2),
                DistanceMetric.SquaredEuclidean => vector1.SquaredEuclidean(vector2),
                _ => vector1.MeanSquaredDistance(vector2)
            };
        }

        /// <summary>
        /// Creates a matrix from row vectors
        /// </summary>
        /// <param name="lap"></param>
        /// <param name="rows">Row vectors</param>
        /// <returns></returns>
        public static IFloatMatrix CreateMatrixFromRows(this ILinearAlgebraProvider lap, IEnumerable<Vector<float>> rows) => 
            lap.CreateMatrixFromRows(rows.ToArray());

        /// <summary>
        /// Creates a matrix from row vectors
        /// </summary>
        /// <param name="lap"></param>
        /// <param name="rows">Row vectors</param>
        /// <returns></returns>
        public static IFloatMatrix CreateMatrixFromRows(this ILinearAlgebraProvider lap, IEnumerable<IFloatVector> rows) =>
            lap.CreateMatrixFromRows(rows.ToArray());

        /// <summary>
        /// Creates a matrix from column vectors
        /// </summary>
        /// <param name="lap"></param>
        /// <param name="columns">Column vectors</param>
        /// <returns></returns>
        public static IFloatMatrix CreateMatrixFromColumns(this ILinearAlgebraProvider lap, IEnumerable<Vector<float>> columns) =>
            lap.CreateMatrixFromColumns(columns.ToArray());

        /// <summary>
        /// Creates a matrix from column vectors
        /// </summary>
        /// <param name="lap"></param>
        /// <param name="columns">Column vectors</param>
        /// <returns></returns>
        public static IFloatMatrix CreateMatrixFromColumns(this ILinearAlgebraProvider lap, IEnumerable<IFloatVector> columns) =>
            lap.CreateMatrixFromColumns(columns.ToArray());

        /// <summary>
        /// Converts vectors to float vectors
        /// </summary>
        /// <param name="vectors"></param>
        /// <returns></returns>
        public static IEnumerable<IFloatVector> AsFloatVectors(this IEnumerable<Vector<float>> vectors)
        {
            ILinearAlgebraProvider? lap = null;
            foreach (var vector in vectors)
            {
                lap ??= vector.Context.LinearAlgebraProvider;
                yield return lap.CreateVector(vector);
            }
        }

        /// <summary>
        /// Sets each element in a tensor
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="getValue"></param>
        /// <typeparam name="T"></typeparam>
        public static void Set<T>(this ITensorSegment<T> vector, Func<uint, T> getValue)
            where T : struct
        {
            for (uint i = 0, len = vector.Size; i < len; i++)
                vector[i] = getValue(i);
        }

        /// <summary>
        /// Creates a tensor segment from an existing array
        /// </summary>
        /// <param name="context"></param>
        /// <param name="block">Array to copy values from</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ITensorSegment<T> CreateSegment<T>(this IBrightDataContext context, T[] block) where T : struct => new TensorSegment<T>(context, block);

        /// <summary>
        /// Creates a tensor segment
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="size">Size of new segment</param>
        /// <returns></returns>
        public static ITensorSegment<T> CreateSegment<T>(this IBrightDataContext context, uint size) where T : struct => new TensorSegment<T>(context, context.TensorPool.Get<T>(size));

        /// <summary>
        /// Randomly initialize a tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <typeparam name="T"></typeparam>
        public static void InitializeRandomly<T>(this ITensor<T> tensor) where T : struct
        {
            var computation = tensor.Computation;
            tensor.Segment.InitializeFrom(i => computation.NextRandom());
        }

        /// <summary>
        /// Initialize a tensor to a single value
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="value">Value to initialize each element of the tensor</param>
        /// <typeparam name="T"></typeparam>
        public static void Initialize<T>(this ITensor<T> tensor, T value) where T : struct
        {
            tensor.Segment.InitializeTo(value);
        }

        /// <summary>
        /// Initialize a tensor using a callback
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tensor"></param>
        /// <param name="initializer">Callback for each element</param>
        public static void Initialize<T>(this ITensor<T> tensor, Func<uint, T> initializer) where T : struct
        {
            tensor.Segment.InitializeFrom(initializer);
        }

        /// <summary>
        /// Calculates the cosine distance between two vectors
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static float CosineDistance(this float[] vector, float[] other)
        {
            return Distance.CosineDistance.Calculate(vector, other);
        }

        /// <summary>
        /// Calculates euclidean distance between two vectors
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static float EuclideanDistance(this float[] vector, float[] other)
        {
            return Distance.EuclideanDistance.Calculate(vector, other);
        }

        /// <summary>
        /// Calculates manhattan distance between two vectors
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static float ManhattanDistance(this float[] vector, float[] other)
        {
            return Distance.ManhattanDistance.Calculate(vector, other);
        }

        /// <summary>
        /// Mutates a vector via a callback
        /// </summary>
        /// <param name="vector">Vector to mutate</param>
        /// <param name="mutator">Callback that can mutate each value of the vector</param>
        /// <returns>New vector</returns>
        public static Vector<float> Mutate(this Vector<float> vector, Func<float, float> mutator)
        {
            var context = vector.Context;
            var segment = context.CreateSegment<float>(vector.Size);
            segment.InitializeFrom(i => mutator(vector[i]));
            return new Vector<float>(segment);
        }

        /// <summary>
        /// Mutates a vector by combining it with another vector
        /// </summary>
        /// <param name="vector">Vector to mutate</param>
        /// <param name="other">Other vector</param>
        /// <param name="mutator">Callback that can mutate each value of the vector</param>
        /// <returns></returns>
        public static Vector<float> MutateWith(this Vector<float> vector, Vector<float> other, Func<float, float, float> mutator)
        {
            var context = vector.Context;
            var segment = context.CreateSegment<float>(vector.Size);
            segment.InitializeFrom(i => mutator(vector[i], other[i]));
            return new Vector<float>(segment);
        }

        /// <summary>
        /// Converts the tensor segment to a sparse format (only non zero entries are preserved)
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public static WeightedIndexList ToSparse(this ITensorSegment<float> segment)
        {
            return WeightedIndexList.Create(segment.Context, segment.Values
                .Select((v, i) => new WeightedIndexList.Item((uint)i, v))
                .Where(d => FloatMath.IsNotZero(d.Weight))
            );
        }

        /// <summary>
        /// Reads a vector from a binary reader
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static Vector<float> ReadVectorFrom(this IBrightDataContext context, BinaryReader reader)
        {
            if (context.Get(Consts.LegacyFloatSerialisationInput, false))
            {
                var len = reader.ReadInt32();
                var ret = new float[len];
                for (var i = 0; i < len; i++)
                    ret[i] = reader.ReadSingle();
                return context.CreateVector(ret);
            }
            return new Vector<float>(context, reader);
        }

        /// <summary>
        /// Reads a matrix from a binary reader
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static Matrix<float> ReadMatrixFrom(this IBrightDataContext context, BinaryReader reader)
        {
            if (context.Get(Consts.LegacyFloatSerialisationInput, false))
            {
                var len = reader.ReadInt32();
                var ret = new Vector<float>[len];
                for (var i = 0; i < len; i++)
                    ret[i] = context.ReadVectorFrom(reader);
                return context.CreateMatrixFromRows(ret);
            }
            return new Matrix<float>(context, reader);
        }
    }
}
