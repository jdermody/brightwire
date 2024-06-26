﻿using System.Numerics;
using System.Runtime.CompilerServices;
using BrightData.Helper;
using BrightData.LinearAlgebra.Segments;
using BrightData.Types;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData
{
    public partial class ExtensionMethods
    {
        /// <summary>
        /// Converts the vector to a sparse format (only non-zero entries are preserved)
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static WeightedIndexList ToSparse(this IReadOnlyVector<float> vector) => vector.ReadOnlySegment.ToSparse();

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        static INumericSegment<float> Apply(IReadOnlyVector vector, IReadOnlyVector other, TransformReadOnlySpans<float, MemoryOwner<float>> mutator)
//        {
//            var result = vector.ReadOnlySegment.ApplyReadOnlySpans(other.ReadOnlySegment, mutator);
//            return new ArrayPoolTensorSegment<float>(result);
//        }
//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        static float Apply(IReadOnlyVector vector, IReadOnlyVector other, TransformReadOnlySpans<float, float> mutator)
//        {
//            return vector.ReadOnlySegment.ApplyReadOnlySpans(other.ReadOnlySegment, mutator);
//        }
//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        static INumericSegment<float> Apply(IReadOnlyVector vector, TransformReadOnlySpan<float, MemoryOwner<float>> mutator)
//        {
//            var result = vector.ReadOnlySegment.ApplyReadOnlySpan(mutator);
//            return new ArrayPoolTensorSegment<float>(result);
//        }
//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        static float Apply(IReadOnlyVector vector, TransformReadOnlySpan<float, float> mutator)
//        {
//            return vector.ReadOnlySegment.ApplyReadOnlySpan(mutator);
//        }

//#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
//        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static INumericSegment<float> Add(this IReadOnlyVector vector, IReadOnlyVector other) => Apply(vector, other, (x, y) => x.Add(y));
//        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static INumericSegment<float> Add(this IReadOnlyVector vector, IReadOnlyVector other, float coefficient1, float coefficient2) => Apply(vector, other, (x, y) => x.Add(y, coefficient1, coefficient2));
//        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static INumericSegment<float> Add(this IReadOnlyVector vector, float scalar) => Apply(vector, x => x.Add(scalar));
//        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static INumericSegment<float> Multiply(this IReadOnlyVector vector, float scalar) => Apply(vector, x => x.Multiply(scalar));
//        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static INumericSegment<float> Subtract(this IReadOnlyVector vector, IReadOnlyVector other) => Apply(vector, other, (x, y) => x.Subtract(y));
//        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static INumericSegment<float> Subtract(this IReadOnlyVector vector, IReadOnlyVector other, float coefficient1, float coefficient2) => Apply(vector, other, (x, y) => x.Subtract(y, coefficient1, coefficient2));
//        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static INumericSegment<float> PointwiseMultiply(this IReadOnlyVector vector, IReadOnlyVector other) => Apply(vector, other, (x, y) => x.PointwiseMultiply(y));
//        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static INumericSegment<float> PointwiseDivide(this IReadOnlyVector vector, IReadOnlyVector other) => Apply(vector, other, (x, y) => x.PointwiseDivide(y));
//        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float DotProduct(this IReadOnlyVector vector, IReadOnlyVector other) => Apply(vector, other, (x, y) => x.DotProduct(y));
//        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static INumericSegment<float> Sqrt(this IReadOnlyVector vector, float adjustment = FloatMath.AlmostZero) => Apply(vector, x => x.Sqrt(adjustment));
//        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float Average(this IReadOnlyVector vector) => Apply(vector, x => x.Average());
//        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float L1Norm(this IReadOnlyVector vector) => Apply(vector, x => x.L1Norm());
//        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float L2Norm(this IReadOnlyVector vector) => Apply(vector, x => x.L2Norm());
//        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static INumericSegment<float> Reverse(this IReadOnlyVector vector) => Apply(vector, x => x.Reverse());
//        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static INumericSegment<float> Abs(this IReadOnlyVector vector) => Apply(vector, x => x.Abs());
//        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static INumericSegment<float> Log(this IReadOnlyVector vector) => Apply(vector, x => x.Log());
//        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static INumericSegment<float> Exp(this IReadOnlyVector vector) => Apply(vector, x => x.Exp());
//        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static INumericSegment<float> Pow(this IReadOnlyVector vector, float power) => Apply(vector, x => x.Pow(power));
//        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static INumericSegment<float> Squared(this IReadOnlyVector vector) => Apply(vector, x => x.Squared());
//        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float StdDev(this IReadOnlyVector vector, float? mean) => Apply(vector, x => x.StdDev(mean));
//        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static INumericSegment<float> Sigmoid(this IReadOnlyVector vector) => Apply(vector, x => x.Sigmoid());
//        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static INumericSegment<float> SigmoidDerivative(this IReadOnlyVector vector) => Apply(vector, x => x.SigmoidDerivative());
//        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static INumericSegment<float> Tanh(this IReadOnlyVector vector) => Apply(vector, x => x.Tanh());
//        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static INumericSegment<float> TanhDerivative(this IReadOnlyVector vector) => Apply(vector, x => x.TanhDerivative());
//        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static INumericSegment<float> Relu(this IReadOnlyVector vector) => Apply(vector, x => x.Relu());
//        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static INumericSegment<float> ReluDerivative(this IReadOnlyVector vector) => Apply(vector, x => x.ReluDerivative());
//        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static INumericSegment<float> LeakyRelu(this IReadOnlyVector vector) => Apply(vector, x => x.LeakyRelu());
//        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static INumericSegment<float> LeakyReluDerivative(this IReadOnlyVector vector) => Apply(vector, x => x.LeakyReluDerivative());
//        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static INumericSegment<float> Softmax(this IReadOnlyVector vector) => Apply(vector, x => x.Softmax());
//        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static INumericSegment<float> SoftmaxDerivative(this IReadOnlyVector vector, int rowCount) => Apply(vector, x => x.SoftmaxDerivative(rowCount));
//        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static INumericSegment<float> CherryPickIndices(this IReadOnlyVector vector, params uint[] indices) => Apply(vector, x => x.CherryPickIndices(indices));
//#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
