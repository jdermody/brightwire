using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using BrightData.Helper;
using BrightData.LinearAlgebra;
using BrightData.LinearAlgebra.ReadOnly;

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
        public static IReadOnlyVector CreateVectorInfo(this BrightDataContext _, uint size, Func<uint, float>? initializer) => initializer is not null
            ? new ReadOnlyVector(size, initializer)
            : new ReadOnlyVector(size)
        ;
        public static IReadOnlyVector CreateVectorInfo(this BrightDataContext context, int size, Func<uint, float>? initializer) => CreateVectorInfo(context, (uint)size, initializer);

        /// <summary>
        /// Creates a vector
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="size">Size of vector</param>
        /// <param name="initialValue">Initial value of each element</param>
        /// <returns></returns>
        public static IReadOnlyVector CreateVectorInfo(this BrightDataContext _, uint size, float initialValue = 0f) => initialValue == 0f
            ? new ReadOnlyVector(size)
            : new ReadOnlyVector(size, _ => initialValue)
        ;
        public static IReadOnlyVector CreateVectorInfo(this BrightDataContext context, int size, float initialValue = 0f) => CreateVectorInfo(context, (uint)size, initialValue);

        /// <summary>
        /// Creates a vector
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="initialData">Initial data</param>
        /// <returns></returns>
        public static IReadOnlyVector CreateVectorInfo(this BrightDataContext _, params float[] initialData) => new ReadOnlyVector(initialData);

        /// <summary>
        /// Creates a vector from a binary reader
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static IReadOnlyVector CreateVectorInfo(this BrightDataContext context, BinaryReader reader)
        {
            var ret = GenericActivator.CreateUninitialized<ICanInitializeFromBinaryReader>(typeof(ReadOnlyVector));
            ret.Initialize(context, reader);
            return (IReadOnlyVector)ret;
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
        public static IReadOnlyMatrix CreateMatrixInfo(this BrightDataContext _, uint rows, uint columns, Func<uint, uint, float>? initializer = null) => initializer is not null
            ? new ReadOnlyMatrix(rows, columns, initializer)
            : new ReadOnlyMatrix(rows, columns)
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
        public static IReadOnlyMatrix CreateMatrixInfo(this BrightDataContext _, uint rows, uint columns, float initialValue = 0f) => initialValue == 0f
            ? new ReadOnlyMatrix(rows, columns)
            : new ReadOnlyMatrix(rows, columns, (_, _) => initialValue)
        ;

        /// <summary>
        /// Creates a matrix from a binary reader
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static IReadOnlyMatrix CreateMatrixInfo(this BrightDataContext context, BinaryReader reader)
        {
            var ret = GenericActivator.CreateUninitialized<ICanInitializeFromBinaryReader>(typeof(ReadOnlyMatrix));
            ret.Initialize(context, reader);
            return (IReadOnlyMatrix)ret;
        }

        /// <summary>
        /// Creates a matrix from vectors (each will become a row)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        public static IReadOnlyMatrix CreateMatrixInfoFromRows(this BrightDataContext _, params IReadOnlyVector[] rows)
        {
            var columns = rows[0].Size;
            var ret = new ReadOnlyMatrix((uint)rows.Length, columns);
            for (var i = 0; i < rows.Length; i++) {
                var source = rows[i];
                var target = ret.GetRow((uint)i);
                source.Segment.CopyTo(target.Segment);
            } 
            return ret;
        }

        /// <summary>
        /// Creates a matrix from rows (each will become a row)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        public static IReadOnlyMatrix CreateMatrixInfoFromRows(this BrightDataContext _, params float[][] rows)
        {
            var columns = (uint)rows[0].Length;
            var ret = new ReadOnlyMatrix((uint)rows.Length, columns);
            for (var i = 0; i < rows.Length; i++) {
                var source = rows[i];
                var target = ret.GetRow((uint)i);
                target.Segment.CopyFrom(source);
            } 
            return ret;
        }

        /// <summary>
        /// Creates a matrix from vectors (each will become a column)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static IReadOnlyMatrix CreateMatrixInfoFromColumns(this BrightDataContext _, params IReadOnlyVector[] columns)
        {
            var rows = columns[0].Size;
            var ret = new ReadOnlyMatrix(rows, (uint)columns.Length);
            for (var i = 0; i < columns.Length; i++) {
                var source = columns[i];
                var target = ret.GetRow((uint)i);
                source.Segment.CopyTo(target.Segment);
            } 
            return ret;
        }

        /// <summary>
        /// Creates a matrix from vectors (each will become a column)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static IReadOnlyMatrix CreateMatrixInfoFromColumns(this BrightDataContext _, params float[][] columns)
        {
            var rows = (uint)columns[0].Length;
            var ret = new ReadOnlyMatrix(rows, (uint)columns.Length);
            for (var i = 0; i < columns.Length; i++) {
                var source = columns[i];
                var target = ret.GetRow((uint)i);
                target.Segment.CopyFrom(source);
            } 
            return ret;
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
        public static IReadOnlyTensor3D CreateTensor3D(this BrightDataContext context, uint depth, uint rows, uint columns) => context.LinearAlgebraProvider.CreateTensor3D(depth, rows, columns);

        /// <summary>
        /// Creates a 3D tensor from matrices
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="slices"></param>
        /// <returns></returns>
        public static IReadOnlyTensor3D CreateTensor3D(this BrightDataContext _, params IReadOnlyMatrix[] matrices) => new ReadOnlyTensor3D(matrices);

        /// <summary>
        /// Create a 3D tensor from a binary reader
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static IReadOnlyTensor3D CreateTensor3DInfo(this BrightDataContext context, BinaryReader reader)
        {
            var ret = GenericActivator.CreateUninitialized<ICanInitializeFromBinaryReader>(typeof(ReadOnlyTensor3D));
            ret.Initialize(context, reader);
            return (IReadOnlyTensor3D)ret;
        }

        /// <summary>
        /// Creates a 4D tensor from a binary reader
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static IReadOnlyTensor4D CreateTensor4DInfo(this BrightDataContext context, BinaryReader reader)
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
        /// <typeparam name="T"></typeparam>
        public static void InitializeRandomly<T>(this ITensor tensor) where T : struct
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
        public static void Initialize(this ITensor tensor, float value)
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
        public static void Initialize(this ITensor tensor, Func<uint, float> initializer)
        {
            var segment = tensor.Segment;
            for (uint i = 0, len = segment.Size; i < len; i++)
                segment[i] = initializer(i);
        }

        /// <summary>
        /// Converts the tensor segment to a sparse format (only non zero entries are preserved)
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public static WeightedIndexList ToSparse(this ITensorSegment segment)
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
        public static IReadOnlyVector ReadVectorFrom(this BrightDataContext context, BinaryReader reader)
        {
            var lap = context.LinearAlgebraProvider;
            if (context.Get(Consts.LegacyFloatSerialisationInput, false))
            {
                var len = reader.ReadInt32();
                var ret = new float[len];
                for (var i = 0; i < len; i++)
                    ret[i] = reader.ReadSingle();
                return lap.CreateVector(ret);
            }
            return context.CreateVectorInfo(reader);
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
            var temp = context.CreateVectorInfo(reader);
            return temp.Segment.ToNewArray();
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
                    ret[i] = context.ReadVectorFrom(reader);
                return context.CreateMatrixInfoFromRows(ret);
            }
            return context.CreateMatrixInfo(reader);
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
            ITensorSegment? ret = null;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static float[] GetLocalOrNewArray(this ITensorSegment segment) => segment.GetArrayForLocalUseOnly() ?? segment.ToNewArray();
    }
}
