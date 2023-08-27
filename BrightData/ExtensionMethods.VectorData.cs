using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData
{
    public partial class ExtensionMethods
    {
        /// <summary>
        /// Returns the index with the minimum value from this tensor segment
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (float Min, float Max, uint MinIndex, uint MaxIndex) GetMinAndMaxValues(this IVectorData vector) => vector.ReadOnlySegment.GetReadOnlySpan(x => x.GetMinAndMaxValues());

        /// <summary>
        /// Returns the index with the minimum value from this tensor segment
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static uint GetMinimumIndex(this IVectorData vector) => 
            GetMinAndMaxValues(vector).MinIndex;

        /// <summary>
        /// Returns the index with the maximum value from this tensor segment
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static uint GetMaximumIndex(this IVectorData vector) => 
            GetMinAndMaxValues(vector).MaxIndex;

        /// <summary>
        /// Sums all values
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float Sum(this IVectorData vector) => 
            vector.ReadOnlySegment.GetReadOnlySpan(x => x.Sum());

        /// <summary>
        /// Returns the index with the minimum value from this tensor segment
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static MemoryOwner<float> Softmax(this IVectorData vector) => 
            vector.ReadOnlySegment.GetReadOnlySpan(x => x.Softmax());

        /// <summary>
        /// Finds cosine distance (0 for perpendicular, 1 for orthogonal, 2 for opposite) between this and another vector
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float CosineDistance(this IVectorData vector, IVectorData other) => 
            vector.ReadOnlySegment.GetReadOnlySpans(other.ReadOnlySegment, (x,y) => x.CosineDistance(y));
    }
}
