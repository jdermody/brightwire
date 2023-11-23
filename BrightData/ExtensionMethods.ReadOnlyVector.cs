using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BrightData.LinearAlgebra.Segments;
using BrightData.Types;
using CommunityToolkit.HighPerformance.Buffers;
using static BrightData.ExtensionMethods;

namespace BrightData
{
    public partial class ExtensionMethods
    {
        /// <summary>
        /// Converts the vector to a sparse format (only non zero entries are preserved)
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static WeightedIndexList ToSparse(this IReadOnlyVector vector) => vector.ReadOnlySegment.ToSparse();

        /// <summary>
        /// Returns the index with the minimum value from this tensor segment
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (float Min, float Max, uint MinIndex, uint MaxIndex) GetMinAndMaxValues(this IReadOnlyVector vector) => vector.ReadOnlySegment.GetReadOnlySpan(x => x.GetMinAndMaxValues());

        /// <summary>
        /// Returns the index with the minimum value from this tensor segment
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static uint GetMinimumIndex(this IReadOnlyVector vector) => GetMinAndMaxValues(vector).MinIndex;

        /// <summary>
        /// Returns the index with the maximum value from this tensor segment
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static uint GetMaximumIndex(this IReadOnlyVector vector) => GetMinAndMaxValues(vector).MaxIndex;

        /// <summary>
        /// Sums all values
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float Sum(this IReadOnlyVector vector) => vector.ReadOnlySegment.GetReadOnlySpan(x => x.Sum());

        /// <summary>
        /// Returns the index with the minimum value from this tensor segment
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static MemoryOwner<float> Softmax(this IReadOnlyVector vector) => vector.ReadOnlySegment.GetReadOnlySpan(x => x.Softmax());

        /// <summary>
        /// Finds cosine distance (0 for perpendicular, 1 for orthogonal, 2 for opposite) between this and another vector
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float CosineDistance(this IReadOnlyVector vector, IReadOnlyVector other) => vector.ReadOnlySegment.GetReadOnlySpans(other.ReadOnlySegment, (x,y) => x.CosineDistance(y));

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float EuclideanDistance(this IReadOnlyVector vector, IReadOnlyVector other) => vector.ReadOnlySegment.GetReadOnlySpans(other.ReadOnlySegment, (x,y) => x.EuclideanDistance(y));

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float ManhattanDistance(this IReadOnlyVector vector, IReadOnlyVector other) => vector.ReadOnlySegment.GetReadOnlySpans(other.ReadOnlySegment, (x,y) => x.ManhattanDistance(y));
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float MeanSquaredDistance(this IReadOnlyVector vector, IReadOnlyVector other) => vector.ReadOnlySegment.GetReadOnlySpans(other.ReadOnlySegment, (x,y) => x.MeanSquaredDistance(y));
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float SquaredEuclideanDistance(this IReadOnlyVector vector, IReadOnlyVector other) => vector.ReadOnlySegment.GetReadOnlySpans(other.ReadOnlySegment, (x,y) => x.SquaredEuclideanDistance(y));
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float FindDistance(this IReadOnlyVector vector, IReadOnlyVector other, DistanceMetric distance) => vector.ReadOnlySegment.GetReadOnlySpans(other.ReadOnlySegment, (x,y) => x.FindDistance(y, distance));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static INumericSegment<float> Apply(IReadOnlyVector vector, IReadOnlyVector other, OnReadOnlySpans<float, MemoryOwner<float>> mutator)
        {
            var result = vector.ReadOnlySegment.GetReadOnlySpans(other.ReadOnlySegment, mutator);
            return new ArrayPoolTensorSegment(result);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static INumericSegment<float> Add(this IReadOnlyVector vector, IReadOnlyVector other) => Apply(vector, other, (x, y) => x.Add(y));
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static INumericSegment<float> Add(this IReadOnlyVector vector, IReadOnlyVector other, float coefficient1, float coefficient2) => Apply(vector, other, (x, y) => x.Add(y, coefficient1, coefficient2));
    }
}
