﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BrightData.Helper;
using BrightData.LinearAlgebra;
using BrightData.LinearAlgebra.CostFunctions;
using BrightData.LinearAlgebra.ReadOnly;
using BrightData.LinearAlgebra.Segments;
using BrightData.Types;

namespace BrightData
{
    public static partial class ExtensionMethods
    {
        /// <summary>
        /// Creates a read only vector
        /// </summary>
        /// <param name="_"></param>
        /// <param name="size">Size of vector</param>
        /// <param name="initializer">Callback to initialize each value (optional)</param>
        /// <returns></returns>
        public static ReadOnlyVector<float> CreateReadOnlyVector(this BrightDataContext _, uint size, Func<uint, float>? initializer) => initializer is not null
            ? new ReadOnlyVector<float>(size, initializer)
            : new ReadOnlyVector<float>(size)
        ;


        /// <summary>
        /// Creates a read only vector
        /// </summary>
        /// <param name="context"></param>
        /// <param name="size">Size of vector</param>
        /// <param name="initializer">Callback to initialize each value (optional)</param>
        /// <returns></returns>
        public static ReadOnlyVector<float> CreateReadOnlyVector(this BrightDataContext context, int size, Func<uint, float>? initializer) => CreateReadOnlyVector(context, (uint)size, initializer);

        /// <summary>
        /// Creates a read only vector
        /// </summary>
        /// <param name="_"></param>
        /// <param name="size">Size of vector</param>
        /// <param name="initialValue">Initial value of each element</param>
        /// <returns></returns>
        public static ReadOnlyVector<float> CreateReadOnlyVector(this BrightDataContext _, uint size, float initialValue = 0f) => initialValue == 0f
            ? new ReadOnlyVector<float>(size)
            : new ReadOnlyVector<float>(size, _ => initialValue)
        ;

        /// <summary>
        /// Creates a vector
        /// </summary>
        /// <param name="context"></param>
        /// <param name="size">Size of vector</param>
        /// <param name="initialValue">Initial value of each element</param>
        /// <returns></returns>
        public static ReadOnlyVector<float> CreateReadOnlyVector(this BrightDataContext context, int size, float initialValue = 0f) => CreateReadOnlyVector(context, (uint)size, initialValue);

        /// <summary>
        /// Creates a read only vector
        /// </summary>
        /// <param name="_"></param>
        /// <param name="initialData">Initial data</param>
        /// <returns></returns>
        public static ReadOnlyVector<float> CreateReadOnlyVector(this BrightDataContext _, params float[] initialData) => new(initialData);

        /// <summary>
        /// Creates a read only vector
        /// </summary>
        /// <param name="_"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ReadOnlyVector<float> CreateReadOnlyVector(this BrightDataContext _, Span<float> data) => new(data.ToArray());

        /// <summary>
        /// Creates a read only vector
        /// </summary>
        /// <param name="_"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ReadOnlyVector<float> CreateReadOnlyVector(this BrightDataContext _, ReadOnlySpan<float> data) => new(data.ToArray());

        /// <summary>
        /// Creates a vector from a binary reader
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static ReadOnlyVector<float> CreateReadOnlyVector(this BrightDataContext context, BinaryReader reader)
        {
            var ret = GenericActivator.CreateUninitialized<ReadOnlyVector<float>>();
            ret.Initialize(context, reader);
            return ret;
        }


        /// <summary>
        /// Creates a matrix
        /// </summary>
        /// <param name="_"></param>
        /// <param name="rows">Number of rows</param>
        /// <param name="columns">Number of columns</param>
        /// <param name="initializer">Callback to initialize each value (optional)</param>
        /// <returns></returns>
        public static ReadOnlyMatrix<float> CreateReadOnlyMatrix(this BrightDataContext _, uint rows, uint columns, Func<uint, uint, float>? initializer = null) => initializer is not null
            ? new ReadOnlyMatrix<float>(rows, columns, initializer)
            : new ReadOnlyMatrix<float>(rows, columns)
        ;

        /// <summary>
        /// Creates a matrix
        /// </summary>
        /// <param name="_"></param>
        /// <param name="rows">Number of rows</param>
        /// <param name="columns">Number of columns</param>
        /// <param name="initialValue">Initial value of each element</param>
        /// <returns></returns>
        public static ReadOnlyMatrix<float> CreateReadOnlyMatrix(this BrightDataContext _, uint rows, uint columns, float initialValue = 0f) => initialValue == 0f
            ? new ReadOnlyMatrix<float>(rows, columns)
            : new ReadOnlyMatrix<float>(rows, columns, (_, _) => initialValue)
        ;

        /// <summary>
        /// Creates a matrix from a binary reader
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static ReadOnlyMatrix<float> CreateReadOnlyMatrix(this BrightDataContext context, BinaryReader reader)
        {
            var ret = GenericActivator.CreateUninitialized<ReadOnlyMatrix<float>>();
            ret.Initialize(context, reader);
            return ret;
        }

        /// <summary>
        /// Creates a matrix from vectors (each will become a row)
        /// </summary>
        /// <param name="_"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        public static ReadOnlyMatrix<float> CreateReadOnlyMatrixFromRows(this BrightDataContext _, params IReadOnlyVector<float>[] rows)
        {
            var columns = rows[0].Size;
            return new ReadOnlyMatrix<float>((uint)rows.Length, columns, (i, j) => rows[i][j]);
        }

        /// <summary>
        /// Creates a matrix from vectors (each will become a row)
        /// </summary>
        /// <param name="_"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        public static ReadOnlyMatrix<float> CreateReadOnlyMatrixFromRows(this BrightDataContext _, params ReadOnlyVector<float>[] rows)
        {
            var columns = rows[0].Size;
            return new ReadOnlyMatrix<float>((uint)rows.Length, columns, (i, j) => rows[i][j]);
        }

        /// <summary>
        /// Creates a matrix from rows (each will become a row)
        /// </summary>
        /// <param name="_"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        public static ReadOnlyMatrix<float> CreateReadOnlyMatrixFromRows(this BrightDataContext _, params float[][] rows)
        {
            var columns = (uint)rows[0].Length;
            return new ReadOnlyMatrix<float>((uint)rows.Length, columns, (i, j) => rows[i][j]);
        }

        /// <summary>
        /// Creates a matrix from vectors (each will become a column)
        /// </summary>
        /// <param name="_"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static ReadOnlyMatrix<float> CreateReadOnlyMatrixFromColumns(this BrightDataContext _, params IReadOnlyVector<float>[] columns)
        {
            var rows = columns[0].Size;
            return new ReadOnlyMatrix<float>(rows, (uint)columns.Length, (i, j) => columns[j][i]);
        }

        /// <summary>
        /// Creates a matrix from vectors (each will become a column)
        /// </summary>
        /// <param name="_"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static ReadOnlyMatrix<float> CreateReadOnlyMatrixFromColumns(this BrightDataContext _, params float[][] columns)
        {
            var rows = (uint)columns[0].Length;
            return new ReadOnlyMatrix<float>(rows, (uint)columns.Length, (i, j) => columns[j][i]);
        }

        /// <summary>
        /// Creates a 3D tensor from matrices
        /// </summary>
        /// <param name="_"></param>
        /// <param name="matrices"></param>
        /// <returns></returns>
        public static ReadOnlyTensor3D<float> CreateReadOnlyTensor3D(this BrightDataContext _, params IReadOnlyMatrix<float>[] matrices) => new(matrices);

        /// <summary>
        /// Create a 3D tensor from a binary reader
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static ReadOnlyTensor3D<float> CreateReadOnlyTensor3D(this BrightDataContext context, BinaryReader reader)
        {
            var ret = GenericActivator.CreateUninitialized<ReadOnlyTensor3D<float>>();
            ret.Initialize(context, reader);
            return ret;
        }

        /// <summary>
        /// Creates a 4D tensor from matrices
        /// </summary>
        /// <param name="_"></param>
        /// <param name="tensors"></param>
        /// <returns></returns>
        public static ReadOnlyTensor4D<float> CreateReadOnlyTensor4D(this BrightDataContext _, params IReadOnlyTensor3D<float>[] tensors) => new(tensors);

        /// <summary>
        /// Creates a 4D tensor from a binary reader
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static ReadOnlyTensor4D<float> CreateReadOnlyTensor4D(this BrightDataContext context, BinaryReader reader)
        {
            var ret = GenericActivator.CreateUninitialized<ReadOnlyTensor4D<float>>();
            ret.Initialize(context, reader);
            return ret;
        }

        /// <summary>
        /// Creates an identity matrix (each diagonal element is 1, each other element is 0)
        /// </summary>
        /// <param name="lap"></param>
        /// <param name="size">Width and height of the new matrix</param>
        /// <returns></returns>
        public static IMatrix<float> CreateIdentityMatrix(this LinearAlgebraProvider<float> lap, uint size)
        {
            return lap.CreateMatrix(size, size, (x, y) => x == y ? 1f : 0f);
        }

        /// <summary>
        /// Creates a diagonal matrix
        /// </summary>
        /// <param name="lap"></param>
        /// <param name="values">Diagonal values</param>
        /// <returns></returns>
        public static IMatrix<float> CreateDiagonalMatrix(this LinearAlgebraProvider<float> lap, params float[] values)
        {
            return lap.CreateMatrix((uint)values.Length, (uint)values.Length, (x, y) => x == y ? values[x] : 0f);
        }

        /// <summary>
        /// Randomly initialize a tensor
        /// </summary>
        /// <param name="tensor"></param>
        public static void InitializeRandomly(this ITensor<float> tensor)
        {
            var segment = tensor.Segment;
            var context = tensor.Context;
            for (uint i = 0, len = segment.Size; i < len; i++)
                segment[i] = context.NextRandomFloat();
        }

        /// <summary>
        /// Initialize a tensor to a single value
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="value">Value to initialize each element of the tensor</param>
        public static void Initialize(this ITensor<float> tensor, float value)
        {
            var segment = tensor.Segment;
            for (uint i = 0, len = segment.Size; i < len; i++)
                segment[i] = value;
        }

        /// <summary>
        /// Initialize a tensor using a callback
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="initializer">Callback for each element</param>
        public static void Initialize(this ITensor<float> tensor, Func<uint, float> initializer)
        {
            var segment = tensor.Segment;
            for (uint i = 0, len = segment.Size; i < len; i++)
                segment[i] = initializer(i);
        }

        /// <summary>
        /// Reads a vector from a binary reader
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static ReadOnlyVector<float> LoadReadOnlyVectorFrom(this BrightDataContext context, BinaryReader reader)
        {
            if (context.Get(Consts.LegacyFloatSerialisationInput, false))
            {
                var len = reader.ReadInt32();
                var ret = new float[len];
                for (var i = 0; i < len; i++)
                    ret[i] = reader.ReadSingle();
                return context.CreateReadOnlyVector(ret);
            }
            return context.CreateReadOnlyVector(reader);
        }

        /// <summary>
        /// Reacts a float array from a binary reader
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static float[] LoadReadOnlyVectorAndThenGetArrayFrom(this BrightDataContext context, BinaryReader reader)
        {
            if (context.Get(Consts.LegacyFloatSerialisationInput, false))
            {
                var len = reader.ReadInt32();
                var ret = new float[len];
                for (var i = 0; i < len; i++)
                    ret[i] = reader.ReadSingle();
                return ret;
            }
            // TODO: refactor this to avoid creating the vector?
            var temp = context.CreateReadOnlyVector(reader);
            return temp.ToArray();
        }

        /// <summary>
        /// Reads a matrix from a binary reader
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static ReadOnlyMatrix<float> ReadMatrixFrom(this BrightDataContext context, BinaryReader reader)
        {
            if (context.Get(Consts.LegacyFloatSerialisationInput, false))
            {
                var len = reader.ReadInt32();
                var ret = new IReadOnlyVector<float>[len];
                for (var i = 0; i < len; i++)
                    ret[i] = context.LoadReadOnlyVectorFrom(reader);
                return context.CreateReadOnlyMatrixFromRows(ret);
            }
            return context.CreateReadOnlyMatrix(reader);
        }

        /// <summary>
        /// Reduce dimensions of the matrix with a singular value decomposition
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="dimensions">Number of dimensions to reduce to</param>
        /// <returns></returns>
        public static IMatrix<float> ReduceDimensionsWithSvd(this IMatrix<float> matrix, uint dimensions)
        {
            using var matrixT = matrix.Transpose();
            var (u, vector, vt) = matrixT.Svd();

            try {
                using var s = matrix.LinearAlgebraProvider.CreateDiagonalMatrix([..vector.Segment.Values.Take((int)dimensions)]);
                using var v2 = vt.GetNewMatrixFromRows(dimensions.AsRange());
                return s.Multiply(v2);
            }
            finally {
                u.Dispose();
                vector.Dispose();
                vt.Dispose();
            }
        }

        /// <summary>
        /// Calculates an average from a collection of tensors
        /// </summary>
        /// <param name="tensors">Tensors to average</param>
        /// <param name="dispose">True to dispose each of the input vectors</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static T Average<T>(this IEnumerable<T> tensors, bool dispose)
            where T: ITensor<float>
        {
            if(tensors == null)
                throw new ArgumentException("Null enumerable", nameof(tensors));
            INumericSegment<float>? ret = null;
            try {
                var count = 0;
                LinearAlgebraProvider<float>? lap = null;
                uint[]? shape = null;
                foreach (var item in tensors) {
                    if (ret is null) {
                        ret = (lap ??= item.LinearAlgebraProvider).CreateSegment(item.TotalSize, false);
                        item.Segment.CopyTo(ret);
                        shape = item.Shape;
                    }
                    else
                        ret.ApplySpans(true, item.Segment, (x, y) => x.AddInPlace(y));

                    ++count;
                    if (dispose)
                        item.Dispose();
                }

                if (ret is null || lap is null || shape is null)
                    throw new ArgumentException("Empty enumerable", nameof(tensors));
                ret.ApplySpan(true, x => x.MultiplyInPlace(1f / count));
                return (T)lap.CreateTensor(shape, ret);
            }
            catch {
                ret?.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Applies a distance metric to two vectors and returns the distance between them
        /// </summary>
        /// <param name="vector">First vector</param>
        /// <param name="other">Second vector</param>
        /// <param name="distance">Distance metric</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static T FindDistance<T>(this IVector<T> vector, IVector<T> other, DistanceMetric distance) where T: unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T> => distance switch {
            DistanceMetric.Cosine => vector.CosineDistance(other),
            DistanceMetric.Euclidean => vector.EuclideanDistance(other),
            DistanceMetric.Manhattan => vector.ManhattanDistance(other),
            DistanceMetric.MeanSquared => vector.MeanSquaredDistance(other),
            DistanceMetric.SquaredEuclidean => vector.SquaredEuclideanDistance(other),
            _ => throw new NotImplementedException(distance.ToString())
        };

        /// <summary>
        /// Applies a distance metric to this and a list of other vectors
        /// </summary>
        /// <param name="compareTo">This vector</param>
        /// <param name="vectors">List of other vectors</param>
        /// <param name="distanceMetric">Distance metric</param>
        /// <returns>A vector in which each value is the distance between this and the corresponding other vector</returns>
        public static IVector<T> FindDistances<T>(this IVector<T> compareTo, IReadOnlyList<IVector<T>> vectors, DistanceMetric distanceMetric) where T: unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
        {
            var size = (uint)vectors.Count;
            var lap = compareTo.LinearAlgebraProvider;
            var ret = lap.CreateVector(size, false);
            if (size >= Consts.MinimumSizeForParallel) {
                Parallel.For(0, size, ind => {
                    lap.BindThread();
                    ret[ind] = FindDistance(compareTo, vectors[(int)ind], distanceMetric);
                });
            }
            else {
                for (var i = 0; i < size; i++)
                    ret[i] = FindDistance(compareTo, vectors[i], distanceMetric);
            }

            return ret;
        }

        /// <summary>
        /// Converts the vector a weighted index list (sparse vector)
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static WeightedIndexList ToSparse(this IVector<float> vector)
        {
            return WeightedIndexList.Create(vector.Segment.Values
                .Select((v, i) => new WeightedIndexList.Item((uint)i, v))
                .Where(d => Math<float>.IsNotZero(d.Weight))
            );
        }

        /// <summary>
        /// Copies all values from this tensor to another tensor
        /// </summary>
        /// <param name="tensor">This tensor</param>
        /// <param name="other">Other tensor</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this ITensor<T> tensor, ITensor<T> other) where T: unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T> => 
            tensor.Segment.CopyTo(other.Segment);

        /// <summary>
        /// Sets the context to use the default linear algebra provider
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static LinearAlgebraProvider<float> UseDefaultLinearAlgebraProvider(this BrightDataContext context)
        {
            var ret = new LinearAlgebraProvider<float>(context);
            ((ISetLinearAlgebraProvider<float>)context).LinearAlgebraProvider = ret;
            return ret;
        }

        internal static uint[] ResolveShape(this uint total, params uint?[] shape)
        {
            uint nonNullTotal = 1;
            var hasFoundNull = false;
            foreach (var item in shape) {
                if (item.HasValue)
                    nonNullTotal *= item.Value;
                else if (!hasFoundNull)
                    hasFoundNull = true;
                else
                    throw new ArgumentException("Only one parameter can be null");
            }

            if (hasFoundNull && nonNullTotal == 0)
                throw new ArgumentException("Cannot resolve null parameter");

            if (!hasFoundNull && nonNullTotal != total)
                throw new ArgumentException($"Invalid shape arguments: {String.Join("x", shape)} == {nonNullTotal:N0} but expected to be {total:N0}");

            return [.. shape.Select(v => v ?? total / nonNullTotal)];
        }

        /// <summary>
        /// Returns all rows as read only vectors
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="makeCopy">True to make a copy of each row</param>
        /// <returns></returns>
        public static IReadOnlyVector<T>[] AllRowsAsReadOnly<T>(this IReadOnlyMatrix<T> matrix, bool makeCopy) where T: unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
        {
            var ret = new IReadOnlyVector<T>[matrix.RowCount];
            if (makeCopy) {
                var segment = new ReadOnlyTensorSegment<T>(matrix.ToArray());
                for (uint i = 0; i < matrix.RowCount; i++)
                    ret[i] = new ReadOnlyTensorSegmentWrapper<T>(segment, i, matrix.RowCount, matrix.ColumnCount).ToReadOnlyVector();
            }
            else {
                for (uint i = 0; i < matrix.RowCount; i++)
                    ret[i] = matrix.GetReadOnlyRow(i).ToReadOnlyVector();
            }
            return ret;
        }

        /// <summary>
        /// Returns all columns as read only vectors
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="makeCopy">True to make a copy of each column</param>
        /// <returns></returns>
        public static IReadOnlyVector<T>[] AllColumnsAsReadOnly<T>(this IReadOnlyMatrix<T> matrix, bool makeCopy) 
            where T: unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
        {
            var ret = new IReadOnlyVector<T>[matrix.ColumnCount];
            if (makeCopy) {
                var segment = new MutableTensorSegment<T>(matrix.ToArray());
                for (uint i = 0; i < matrix.ColumnCount; i++)
                    ret[i] = new MutableTensorSegmentWrapper<T>(segment, i * matrix.RowCount, 1, matrix.RowCount).ToReadOnlyVector();
            }
            else {
                for (uint i = 0; i < matrix.ColumnCount; i++)
                    ret[i] = matrix.GetReadOnlyColumn(i).ToReadOnlyVector();
            }

            return ret;
        }

        /// <summary>
        /// Returns a row as a vector
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="rowY">Row index</param>
        /// <returns></returns>
        public static IVector<T> GetRowVector<T>(this IMatrix<T> matrix, uint rowY)
            where T: unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
        {
            return matrix.GetRow(rowY).ToVector(matrix.LinearAlgebraProvider);
        }

        /// <summary>
        /// Returns a column as a vector
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="columnX">Column index</param>
        /// <returns></returns>
        public static IVector<T> GetColumnVector<T>(this IMatrix<T> matrix, uint columnX)
            where T: unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
        {
            return matrix.GetColumn(columnX).ToVector(matrix.LinearAlgebraProvider);
        }

        /// <summary>
        /// Creates MSE cost function
        /// </summary>
        /// <param name="lap"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ICostFunction<T> CreateMeanSquaredErrorCostFunction<T>(this LinearAlgebraProvider<T> lap)
            where T: unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
        {
            return new MeanSquaredErrorCostFunction<T>(lap);
        }

        /// <summary>
        /// Creates a cross entropy (logistic loss) cost function
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lap"></param>
        /// <returns></returns>
        public static ICostFunction<T> CreateCrossEntropyCostFunction<T>(this LinearAlgebraProvider<T> lap)
            where T: unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
        {
            return new CrossEntropyCostFunction<T>(lap);
        }

        /// <summary>
        /// Converts the vector to a sparse format (only non-zero entries are preserved)
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WeightedIndexList ToSparse(this IReadOnlyVector<float> vector) => vector.ReadOnlySegment.ToSparse();
    }
}
