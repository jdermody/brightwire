using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BrightData.Helper;
using BrightData.LinearAlegbra2;
using BrightData.LinearAlgebra;
using BrightData.LinearAlgebra.Memory;
using BrightData.Serialisation;
using Microsoft.Toolkit.HighPerformance.Buffers;

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
        public static IVector CreateVector(this BrightDataContext context, uint size, Func<uint, float>? initializer) => initializer is not null
            ? context.LinearAlgebraProvider2.CreateVector(size, initializer)
            : context.LinearAlgebraProvider2.CreateVector(size)
        ;

        /// <summary>
        /// Creates a vector
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="size">Size of vector</param>
        /// <param name="initialValue">Initial value of each element</param>
        /// <returns></returns>
        public static IVector CreateVector(this BrightDataContext context, uint size, float initialValue = 0f) => initialValue == 0f
            ? context.LinearAlgebraProvider2.CreateVector(size)
            : context.LinearAlgebraProvider2.CreateVector(size, _ => initialValue)
        ;

        /// <summary>
        /// Creates a vector
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="initialData">Initial data</param>
        /// <returns></returns>
        public static IVector CreateVector(this BrightDataContext context, params float[] initialData) => context.LinearAlgebraProvider2.CreateVector(initialData);

        /// <summary>
        /// Creates a vector from a binary reader
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static IVector CreateVector(this BrightDataContext context, BinaryReader reader)
        {
            var ret = GenericActivator.CreateUninitialized<ICanInitializeFromBinaryReader>(context.LinearAlgebraProvider2.VectorType);
            ret.Initialize(context, reader);
            return (IVector)ret;
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
        public static IMatrix CreateMatrix(this BrightDataContext context, uint rows, uint columns, Func<uint, uint, float>? initializer = null) => initializer is not null
            ? context.LinearAlgebraProvider2.CreateMatrix(rows, columns, initializer)
            : context.LinearAlgebraProvider2.CreateMatrix(rows, columns)
        ;

        /// <summary>
        /// Creates a matrix
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="rows">Number of rows</param>
        /// <param name="columns">Number of columns</param>
        /// <param name="initialValue">Initial value of each element</param>
        /// <returns></returns>
        public static IMatrix CreateMatrix(this BrightDataContext context, uint rows, uint columns, float initialValue = 0f) => initialValue == 0f
            ? context.LinearAlgebraProvider2.CreateMatrix(rows, columns)
            : context.LinearAlgebraProvider2.CreateMatrix(rows, columns, (_, _) => initialValue)
        ;

        /// <summary>
        /// Creates a matrix from a segment
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rows">Number of rows</param>
        /// <param name="columns">Number of columns</param>
        /// <param name="segment">Segment</param>
        /// <returns></returns>
        public static IMatrix CreateMatrix(this BrightDataContext context, uint rows, uint columns, ITensorSegment2 segment) => context.LinearAlgebraProvider2.CreateMatrix(rows, columns, segment);

        /// <summary>
        /// Creates a matrix from a binary reader
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static IMatrix CreateMatrix(this BrightDataContext context, BinaryReader reader)
        {
            var ret = GenericActivator.CreateUninitialized<ICanInitializeFromBinaryReader>(context.LinearAlgebraProvider2.MatrixType);
            ret.Initialize(context, reader);
            return (IMatrix)ret;
        }

        /// <summary>
        /// Creates a matrix from vectors (each will become a row)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        public static IMatrix CreateMatrixFromRows(this BrightDataContext context, params IVector[] rows) => context.LinearAlgebraProvider2.CreateMatrixFromRows(rows);

        /// <summary>
        /// Creates a matrix from rows (each will become a row)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        public static IMatrix CreateMatrixFromRows(this BrightDataContext context, params float[][] rows) => context.LinearAlgebraProvider2.CreateMatrixFromRowsAndThenDisposeInput(
            rows.Select(x => context.LinearAlgebraProvider2.CreateVector(x)).ToArray()
        );

        /// <summary>
        /// Creates a matrix from vectors (each will become a column)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static IMatrix CreateMatrixFromColumns(this BrightDataContext context, params IVector[] columns) => context.LinearAlgebraProvider2.CreateMatrixFromColumns(columns);

        /// <summary>
        /// Creates a matrix from vectors (each will become a column)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static IMatrix CreateMatrixFromColumns(this BrightDataContext context, params float[][] columns) => context.LinearAlgebraProvider2.CreateMatrixFromColumnsAndThenDisposeInput(
            columns.Select(x => context.LinearAlgebraProvider2.CreateVector(x)).ToArray()
        );

        /// <summary>
        /// Creates a 3D tensor
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="depth">Number of matrices</param>
        /// <param name="rows">Number of rows in each matrix</param>
        /// <param name="columns">Number of columns in each matrix</param>
        /// <returns></returns>
        public static ITensor3D CreateTensor3D(this BrightDataContext context, uint depth, uint rows, uint columns) => context.LinearAlgebraProvider2.CreateTensor3D(depth, rows, columns);

        /// <summary>
        /// Creates a 3D tensor from matrices
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="slices"></param>
        /// <returns></returns>
        public static ITensor3D CreateTensor3D(this BrightDataContext context, params IMatrix[] slices) => context.LinearAlgebraProvider2.CreateTensor3D(slices);

        /// <summary>
        /// Create a 3D tensor from a binary reader
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static ITensor3D CreateTensor3D(this BrightDataContext context, BinaryReader reader)
        {
            var ret = GenericActivator.CreateUninitialized<ICanInitializeFromBinaryReader>(context.LinearAlgebraProvider2.Tensor3DType);
            ret.Initialize(context, reader);
            return (ITensor3D)ret;
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
        public static ITensor4D CreateTensor4D(this BrightDataContext context, uint count, uint depth, uint rows, uint columns) => context.LinearAlgebraProvider2.CreateTensor4D(count, depth, rows, columns);

        /// <summary>
        /// Creates a 4D tensor from a binary reader
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static ITensor4D CreateTensor4D(this BrightDataContext context, BinaryReader reader)
        {
            var ret = GenericActivator.CreateUninitialized<ICanInitializeFromBinaryReader>(context.LinearAlgebraProvider2.Tensor4DType);
            ret.Initialize(context, reader);
            return (ITensor4D)ret;
        }

        /// <summary>
        /// Creates a 4D tensor from 3D tensors
        /// </summary>
        /// <param name="context"></param>
        /// <param name="tensors">3D tensors that form the 4D tensor</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static ITensor4D CreateTensor4D(this BrightDataContext context, params ITensor3D[] tensors) => context.LinearAlgebraProvider2.CreateTensor4D(tensors);

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
        /// <typeparam name="T"></typeparam>
        public static void InitializeRandomly<T>(this ITensor2 tensor) where T : struct
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
        /// <typeparam name="T"></typeparam>
        public static void Initialize(this ITensor2 tensor, float value)
        {
            var segment = tensor.Segment;
            for (uint i = 0, len = segment.Size; i < len; i++)
                segment[i] = value;
        }

        /// <summary>
        /// Initialize a tensor using a callback
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tensor"></param>
        /// <param name="initializer">Callback for each element</param>
        public static void Initialize(this ITensor2 tensor, Func<uint, float> initializer)
        {
            var segment = tensor.Segment;
            for (uint i = 0, len = segment.Size; i < len; i++)
                segment[i] = initializer(i);
        }

        /// <summary>
        /// Mutates a vector via a callback
        /// </summary>
        /// <param name="vector">Vector to mutate</param>
        /// <param name="mutator">Callback that can mutate each value of the vector</param>
        /// <returns>New vector</returns>
        //public static IVector Mutate(this IVector vector, Func<float, float> mutator)
        //{
        //    var lap = vector.Context.LinearAlgebraProvider2;
        //    var segment = lap.CreateSegment(vector.Size);
        //    segment.Initialize(i => mutator(vector[i]));
        //    return new Vector<float>(segment);
        //}

        /// <summary>
        /// Mutates a vector by combining it with another vector
        /// </summary>
        /// <param name="vector">Vector to mutate</param>
        /// <param name="other">Other vector</param>
        /// <param name="mutator">Callback that can mutate each value of the vector</param>
        /// <returns></returns>
        //public static Vector<float> MutateWith(this Vector<float> vector, Vector<float> other, Func<float, float, float> mutator)
        //{
        //    var context = vector.Context;
        //    var segment = context.CreateSegment<float>(vector.Size);
        //    segment.Initialize(i => mutator(vector[i], other[i]));
        //    return new Vector<float>(segment);
        //}

        /// <summary>
        /// Converts the tensor segment to a sparse format (only non zero entries are preserved)
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public static WeightedIndexList ToSparse(this ITensorSegment2 segment, BrightDataContext context)
        {
            return WeightedIndexList.Create(segment.Values
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
        public static IVector ReadVectorFrom(this BrightDataContext context, BinaryReader reader)
        {
            var lap = context.LinearAlgebraProvider2;
            if (context.Get(Consts.LegacyFloatSerialisationInput, false))
            {
                var len = reader.ReadInt32();
                var ret = new float[len];
                for (var i = 0; i < len; i++)
                    ret[i] = reader.ReadSingle();
                return lap.CreateVector(ret);
            }
            return context.CreateVector(reader);
        }

        public static float[] ReadVectorAndThenGetArrayFrom(this BrightDataContext context, BinaryReader reader)
        {
            if (context.Get(Consts.LegacyFloatSerialisationInput, false))
            {
                var len = reader.ReadInt32();
                var ret = new float[len];
                for (var i = 0; i < len; i++)
                    ret[i] = reader.ReadSingle();
                return ret;
            }
            // TODO: refactor this to avoid creating the vector
            using var temp = context.CreateVector(reader);
            return temp.Segment.ToNewArray();
        }

        /// <summary>
        /// Reads a matrix from a binary reader
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static IMatrix ReadMatrixFrom(this BrightDataContext context, BinaryReader reader)
        {
            var lap = context.LinearAlgebraProvider2;
            if (context.Get(Consts.LegacyFloatSerialisationInput, false))
            {
                var len = reader.ReadInt32();
                var ret = new IVector[len];
                for (var i = 0; i < len; i++)
                    ret[i] = context.ReadVectorFrom(reader);
                return lap.CreateMatrixFromRowsAndThenDisposeInput(ret);
            }
            return context.Create<IMatrix>(reader);
        }

        /// <summary>
        /// Find the minimum value and index in a vector
        /// </summary>
        /// <param name="vector">Vector to analyse</param>
        /// <returns>Tuple containing the minimum value and its index</returns>
        public static (float Value, uint Index) Minimum(this float[] vector)
        {
            var ret = uint.MaxValue;
            var lowestValue = float.MaxValue;

            for (uint i = 0, len = (uint)vector.Length; i < len; i++) {
                var val = vector[i];
                if (val < lowestValue) {
                    lowestValue = val;
                    ret = i;
                }
            }

            return (lowestValue, ret);
        }

        /// <summary>
        /// Returns the index of the minimum value within a vector
        /// </summary>
        /// <param name="vector">Vector to analyse</param>
        /// <returns></returns>
        public static uint MinimumIndex(this float[] vector) => Minimum(vector).Index;

        /// <summary>
        /// Returns the minimum value
        /// </summary>
        /// <param name="vector">Vector to analyse</param>
        /// <returns></returns>
        public static float MinimumValue(this float[] vector) => Minimum(vector).Value;

        /// <summary>
        /// Returns the maximum value and index within a vector
        /// </summary>
        /// <param name="vector">Vector to analyse</param>
        /// <returns>Tuple containing the maximum value and its index</returns>
        public static (float Value, uint Index) Maximum(this float[] vector)
        {
            var ret = uint.MaxValue;
            var highestValue = float.MinValue;

            for (uint i = 0, len = (uint)vector.Length; i < len; i++) {
                var val = vector[i];
                if (val > highestValue) {
                    highestValue = val;
                    ret = i;
                }
            }

            return (highestValue, ret);
        }

        /// <summary>
        /// Returns the maximum value within a vector
        /// </summary>
        /// <param name="vector">Vector to analyse</param>
        /// <returns></returns>
        public static uint MaximumIndex(this float[] vector) => Maximum(vector).Index;

        /// <summary>
        /// Returns the index of the maximum value within a vector
        /// </summary>
        /// <param name="vector">Vector to analyse</param>
        /// <returns></returns>
        public static float MaximumValue(this float[] vector) => Maximum(vector).Value;

        /// <summary>
        /// Calculates the softmax of a vector
        /// https://en.wikipedia.org/wiki/Softmax_function
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static float[] Softmax(this float[] vector)
        {
            var max = MaximumValue(vector);

            var softmax = vector.Select(v => MathF.Exp(v - max)).ToArray();
            var sum = softmax.Sum();
            if (!sum.Equals(0))
                softmax = softmax.Select(v => v / sum).ToArray();
            return softmax;
        }

        /// <summary>
        /// Reduce dimensions of the matrix with a singular value decomposition
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="dimensions">Number of dimensions to reduce to</param>
        /// <returns></returns>
        public static IMatrix ReduceDimensions(this IMatrix matrix, uint dimensions)
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
        /// Calculates an average matrix from a collection of matrices
        /// </summary>
        /// <param name="matrices">Matrices to average</param>
        /// <param name="dispose">True to dispose each of the input matrices</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static IMatrix Average(this IEnumerable<IMatrix> matrices, bool dispose) => Average<IMatrix>(matrices, dispose);

        /// <summary>
        /// Calculates an average vector from a collection of vectors
        /// </summary>
        /// <param name="vectors">Vectors to average</param>
        /// <param name="dispose">True to dispose each of the input vectors</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static IVector Average(this IEnumerable<IVector> vectors, bool dispose) => Average<IVector>(vectors, dispose);

        static T Average<T>(this IEnumerable<T> tensors, bool dispose)
            where T: ITensor2
        {
            if(tensors == null)
                throw new ArgumentException("Null enumerable", nameof(tensors));
            ITensorSegment2? ret = null;
            var count = 0;
            LinearAlgebraProvider? lap = null;
            uint[]? shape = null;
            foreach(var item in tensors) {
                if (ret is null) {
                    ret = (lap ??= item.LinearAlgebraProvider).CreateSegment(item.TotalSize);
                    shape = item.Shape;
                }
                else
                    ret.AddInPlace(item.Segment);
                ++count;
                if(dispose)
                    item.Dispose();
            }

            if (ret is null || lap is null || shape is null)
                throw new ArgumentException("Empty enumerable", nameof(tensors));
            ret.Multiply(1f / count);
            return (T)lap.CreateTensor(shape, ret);
        }
    }
}
