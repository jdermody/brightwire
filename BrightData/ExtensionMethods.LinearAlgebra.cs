using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;
using BrightData.Helper;
using BrightData.LinearAlgebra;
using BrightData.LinearAlgebra.ReadOnly;
using BrightData.LinearAlgebra.Segments;
using CommunityToolkit.HighPerformance.Buffers;
using CommunityToolkit.HighPerformance.Helpers;

namespace BrightData
{
    public static partial class ExtensionMethods
    {
        /// <summary>
        /// Creates a vector
        /// </summary>
        /// <param name="_"></param>
        /// <param name="size">Size of vector</param>
        /// <param name="initializer">Callback to initialize each value (optional)</param>
        /// <returns></returns>
        public static IReadOnlyVector CreateReadOnlyVector(this BrightDataContext _, uint size, Func<uint, float>? initializer) => initializer is not null
            ? new ReadOnlyVector(size, initializer)
            : new ReadOnlyVector(size)
        ;


        /// <summary>
        /// Creates a vector
        /// </summary>
        /// <param name="context"></param>
        /// <param name="size">Size of vector</param>
        /// <param name="initializer">Callback to initialize each value (optional)</param>
        /// <returns></returns>
        public static IReadOnlyVector CreateReadOnlyVector(this BrightDataContext context, int size, Func<uint, float>? initializer) => CreateReadOnlyVector(context, (uint)size, initializer);

        /// <summary>
        /// Creates a vector
        /// </summary>
        /// <param name="_"></param>
        /// <param name="size">Size of vector</param>
        /// <param name="initialValue">Initial value of each element</param>
        /// <returns></returns>
        public static IReadOnlyVector CreateReadOnlyVector(this BrightDataContext _, uint size, float initialValue = 0f) => initialValue == 0f
            ? new ReadOnlyVector(size)
            : new ReadOnlyVector(size, _ => initialValue)
        ;

        /// <summary>
        /// Creates a vector
        /// </summary>
        /// <param name="context"></param>
        /// <param name="size">Size of vector</param>
        /// <param name="initialValue">Initial value of each element</param>
        /// <returns></returns>
        public static IReadOnlyVector CreateReadOnlyVector(this BrightDataContext context, int size, float initialValue = 0f) => CreateReadOnlyVector(context, (uint)size, initialValue);

        /// <summary>
        /// Creates a vector
        /// </summary>
        /// <param name="_"></param>
        /// <param name="initialData">Initial data</param>
        /// <returns></returns>
        public static IReadOnlyVector CreateReadOnlyVector(this BrightDataContext _, params float[] initialData) => new ReadOnlyVector(initialData);

        /// <summary>
        /// Creates a vector from a binary reader
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static IReadOnlyVector CreateReadOnlyVector(this BrightDataContext context, BinaryReader reader)
        {
            var ret = GenericActivator.CreateUninitialized<ICanInitializeFromBinaryReader>(typeof(ReadOnlyVector));
            ret.Initialize(context, reader);
            return (IReadOnlyVector)ret;
        }


        /// <summary>
        /// Creates a matrix
        /// </summary>
        /// <param name="_"></param>
        /// <param name="rows">Number of rows</param>
        /// <param name="columns">Number of columns</param>
        /// <param name="initializer">Callback to initialize each value (optional)</param>
        /// <returns></returns>
        public static IReadOnlyMatrix CreateReadOnlyMatrix(this BrightDataContext _, uint rows, uint columns, Func<uint, uint, float>? initializer = null) => initializer is not null
            ? new ReadOnlyMatrix(rows, columns, initializer)
            : new ReadOnlyMatrix(rows, columns)
        ;

        /// <summary>
        /// Creates a matrix
        /// </summary>
        /// <param name="_"></param>
        /// <param name="rows">Number of rows</param>
        /// <param name="columns">Number of columns</param>
        /// <param name="initialValue">Initial value of each element</param>
        /// <returns></returns>
        public static IReadOnlyMatrix CreateReadOnlyMatrix(this BrightDataContext _, uint rows, uint columns, float initialValue = 0f) => initialValue == 0f
            ? new ReadOnlyMatrix(rows, columns)
            : new ReadOnlyMatrix(rows, columns, (_, _) => initialValue)
        ;

        /// <summary>
        /// Creates a matrix from a binary reader
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static IReadOnlyMatrix CreateReadOnlyMatrix(this BrightDataContext context, BinaryReader reader)
        {
            var ret = GenericActivator.CreateUninitialized<ICanInitializeFromBinaryReader>(typeof(ReadOnlyMatrix));
            ret.Initialize(context, reader);
            return (IReadOnlyMatrix)ret;
        }

        /// <summary>
        /// Creates a matrix from vectors (each will become a row)
        /// </summary>
        /// <param name="_"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        public static IReadOnlyMatrix CreateReadOnlyMatrixFromRows(this BrightDataContext _, params IReadOnlyVector[] rows)
        {
            var columns = rows[0].Size;
            var ret = new ReadOnlyMatrix(new float[rows.Length * columns], (uint)rows.Length, columns);
            for (var i = 0; i < rows.Length; i++) {
                var source = rows[i];
                var target = ret.Row((uint)i);
                source.ReadOnlySegment.CopyTo(target);
            } 
            return ret;
        }

        /// <summary>
        /// Creates a matrix from rows (each will become a row)
        /// </summary>
        /// <param name="_"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        public static IReadOnlyMatrix CreateReadOnlyMatrixFromRows(this BrightDataContext _, params float[][] rows)
        {
            var columns = (uint)rows[0].Length;
            var ret = new ReadOnlyMatrix((uint)rows.Length, columns);
            for (var i = 0; i < rows.Length; i++) {
                var source = rows[i];
                var target = ret.Row((uint)i);
                target.CopyFrom(source.AsSpan(), 0);
            } 
            return ret;
        }

        /// <summary>
        /// Creates a matrix from vectors (each will become a column)
        /// </summary>
        /// <param name="_"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static IReadOnlyMatrix CreateReadOnlyMatrixFromColumns(this BrightDataContext _, params IReadOnlyVector[] columns)
        {
            var rows = columns[0].Size;
            var ret = new ReadOnlyMatrix(rows, (uint)columns.Length);
            for (var i = 0; i < columns.Length; i++) {
                var source = columns[i];
                var target = ret.Column((uint)i);
                source.ReadOnlySegment.CopyTo(target);
            } 
            return ret;
        }

        /// <summary>
        /// Creates a matrix from vectors (each will become a column)
        /// </summary>
        /// <param name="_"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static IReadOnlyMatrix CreateReadOnlyMatrixFromColumns(this BrightDataContext _, params float[][] columns)
        {
            var rows = (uint)columns[0].Length;
            var ret = new ReadOnlyMatrix(rows, (uint)columns.Length);
            for (var i = 0; i < columns.Length; i++) {
                var source = columns[i];
                var target = ret.Column((uint)i);
                target.CopyFrom(source.AsSpan(), 0);
            } 
            return ret;
        }

        /// <summary>
        /// Creates a 3D tensor from matrices
        /// </summary>
        /// <param name="_"></param>
        /// <param name="matrices"></param>
        /// <returns></returns>
        public static IReadOnlyTensor3D CreateReadOnlyTensor3D(this BrightDataContext _, params IReadOnlyMatrix[] matrices) => new ReadOnlyTensor3D(matrices);

        /// <summary>
        /// Create a 3D tensor from a binary reader
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static IReadOnlyTensor3D CreateReadOnlyTensor3D(this BrightDataContext context, BinaryReader reader)
        {
            var ret = GenericActivator.CreateUninitialized<ICanInitializeFromBinaryReader>(typeof(ReadOnlyTensor3D));
            ret.Initialize(context, reader);
            return (IReadOnlyTensor3D)ret;
        }

        /// <summary>
        /// Creates a 4D tensor from matrices
        /// </summary>
        /// <param name="_"></param>
        /// <param name="tensors"></param>
        /// <returns></returns>
        public static IReadOnlyTensor4D CreateReadOnlyTensor4D(this BrightDataContext _, params IReadOnlyTensor3D[] tensors) => new ReadOnlyTensor4D(tensors);

        /// <summary>
        /// Creates a 4D tensor from a binary reader
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static IReadOnlyTensor4D CreateReadOnlyTensor4D(this BrightDataContext context, BinaryReader reader)
        {
            var ret = GenericActivator.CreateUninitialized<ICanInitializeFromBinaryReader>(typeof(ReadOnlyTensor4D));
            ret.Initialize(context, reader);
            return (IReadOnlyTensor4D)ret;
        }

        /// <summary>
        /// Creates an identity matrix (each diagonal element is 1, each other element is 0)
        /// </summary>
        /// <param name="lap"></param>
        /// <param name="size">Width and height of the new matrix</param>
        /// <returns></returns>
        public static IMatrix CreateIdentityMatrix(this LinearAlgebraProvider lap, uint size)
        {
            return lap.CreateMatrix(size, size, (x, y) => x == y ? 1f : 0f);
        }

        /// <summary>
        /// Creates a diagonal matrix
        /// </summary>
        /// <param name="lap"></param>
        /// <param name="values">Diagonal values</param>
        /// <returns></returns>
        public static IMatrix CreateDiagonalMatrix(this LinearAlgebraProvider lap, params float[] values)
        {
            return lap.CreateMatrix((uint)values.Length, (uint)values.Length, (x, y) => x == y ? values[x] : 0f);
        }

        /// <summary>
        /// Randomly initialize a tensor
        /// </summary>
        /// <param name="tensor"></param>
        public static void InitializeRandomly(this ITensor tensor)
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
        public static void Initialize(this ITensor tensor, float value)
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
        public static void Initialize(this ITensor tensor, Func<uint, float> initializer)
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
        public static IReadOnlyVector LoadReadOnlyVectorFrom(this BrightDataContext context, BinaryReader reader)
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
        public static IReadOnlyMatrix ReadMatrixFrom(this BrightDataContext context, BinaryReader reader)
        {
            if (context.Get(Consts.LegacyFloatSerialisationInput, false))
            {
                var len = reader.ReadInt32();
                var ret = new IReadOnlyVector[len];
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
        public static IMatrix ReduceDimensionsWithSvd(this IMatrix matrix, uint dimensions)
        {
            using var matrixT = matrix.Transpose();
            var (u, vector, vt) = matrixT.Svd();

            try {
                using var s = matrix.LinearAlgebraProvider.CreateDiagonalMatrix(vector.Segment.Values.Take((int)dimensions).ToArray());
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
            where T: ITensor
        {
            if(tensors == null)
                throw new ArgumentException("Null enumerable", nameof(tensors));
            INumericSegment<float>? ret = null;
            try {
                var count = 0;
                LinearAlgebraProvider? lap = null;
                uint[]? shape = null;
                foreach (var item in tensors) {
                    if (ret is null) {
                        ret = (lap ??= item.LinearAlgebraProvider).CreateSegment(item.TotalSize, false);
                        item.Segment.CopyTo(ret);
                        shape = item.Shape;
                    }
                    else
                        ret.GetSpans(item.Segment, (x, y) => x.AddInPlace(y));

                    ++count;
                    if (dispose)
                        item.Dispose();
                }

                if (ret is null || lap is null || shape is null)
                    throw new ArgumentException("Empty enumerable", nameof(tensors));
                ret.GetSpan(x => x.MultiplyInPlace(1f / count));
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
        public static float FindDistance(this IVector vector, IVector other, DistanceMetric distance) => distance switch {
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
        public static IVector FindDistances(this IVector compareTo, IReadOnlyList<IVector> vectors, DistanceMetric distanceMetric)
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
        public static WeightedIndexList ToSparse(this IVector vector)
        {
            return WeightedIndexList.Create(vector.Segment.Values
                .Select((v, i) => new WeightedIndexList.Item((uint)i, v))
                .Where(d => FloatMath.IsNotZero(d.Weight))
            );
        }

        /// <summary>
        /// Copies all values from this tensor to another tensor
        /// </summary>
        /// <param name="tensor">This tensor</param>
        /// <param name="other">Other tensor</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo(this ITensor tensor, ITensor other) => tensor.Segment.CopyTo(other.Segment);

        /// <summary>
        /// Sets the context to use the default linear algebra provider
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static LinearAlgebraProvider UseDefaultLinearAlgebraProvider(this BrightDataContext context)
        {
            var ret = new LinearAlgebraProvider(context);
            ((ISetLinearAlgebraProvider)context).LinearAlgebraProvider = ret;
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

            return shape.Select(v => v ?? total / nonNullTotal).ToArray();
        }
    }
}
