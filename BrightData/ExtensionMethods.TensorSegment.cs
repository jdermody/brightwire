using BrightData.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance.Buffers;
using CommunityToolkit.HighPerformance.Helpers;
using BrightData.LinearAlgebra;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using BrightData.LinearAlgebra.ReadOnly;
using BrightData.LinearAlgebra.Segments;

namespace BrightData
{
    public partial class ExtensionMethods
    {
        /// <summary>
        /// Creates a tensor segment from a memory owner
        /// </summary>
        /// <param name="memoryOwner"></param>
        /// <returns></returns>
        public static ITensorSegment ToSegment(this MemoryOwner<float> memoryOwner) => new ArrayPoolTensorSegment(memoryOwner);

        /// <summary>
        /// Returns an array from a tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static float[] GetLocalOrNewArray(this ITensorSegment segment) => segment.GetArrayIfEasilyAvailable() ?? segment.ToNewArray();

        /// <summary>
        /// Converts the tensor segment to a sparse format (only non zero entries are preserved)
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public static WeightedIndexList ToSparse(this IReadOnlyTensorSegment segment)
        {
            return WeightedIndexList.Create(segment.Values
                .Select((v, i) => new WeightedIndexList.Item((uint)i, v))
                .Where(d => FloatMath.IsNotZero(d.Weight))
            );
        }

        /// <summary>
        /// Sums all values
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sum(this IReadOnlyTensorSegment segment)
        {
            var size = segment.Size;
            if (size >= Consts.MinimumSizeForVectorised && Sse3.IsSupported) {
                var temp = SpanOwner<float>.Empty;
                var span = segment.GetFloatSpan(ref temp, out var wasTempUsed);
                try {
                    return span.Sum();
                }
                finally {
                    if (wasTempUsed)
                        temp.Dispose();
                }
            }

            var ret = 0f;
            for (uint i = 0; i < size; i++)
                ret += segment[i];
            return ret;
        }

        /// <summary>
        /// Searches this tensor segment for the index of the first value that matches the specified value within a level of tolerance
        /// </summary>
        /// <param name="segment">This tensor</param>
        /// <param name="value">Value to find</param>
        /// <param name="tolerance">Degree of tolerance</param>
        /// <returns></returns>
        public static uint? Search(this IReadOnlyTensorSegment segment, float value, float tolerance = FloatMath.AlmostZero)
        {
            uint? ret = null;
            Analyze(segment, (v, index) => {
                if (Math.Abs(value - v) < tolerance)
                    ret = index;
            });
            return ret;
        }

        /// <summary>
        /// Finds the min and max values (and their indices) of this tensor segment
        /// </summary>
        /// <param name="segment">This tensor</param>
        /// <returns></returns>
        public static (float Min, float Max, uint MinIndex, uint MaxIndex) GetMinAndMaxValues(this IReadOnlyTensorSegment segment)
        {
            var min = float.MaxValue;
            var max = float.MinValue;
            var minIndex = uint.MaxValue;
            var maxIndex = uint.MaxValue;
            uint index = 0;

            foreach (var value in segment.Values) {
                if (value.CompareTo(max) > 0) {
                    max = value;
                    maxIndex = index;
                }

                if (value.CompareTo(min) < 0) {
                    min = value;
                    minIndex = index;
                }

                ++index;
            }

            return (min, max, minIndex, maxIndex);
        }

        /// <summary>
        /// Splits this tensor segment into multiple contiguous tensor segments
        /// </summary>
        /// <param name="segment">This tensor</param>
        /// <param name="blockCount">Number of blocks</param>
        /// <returns></returns>
        public static IEnumerable<IReadOnlyTensorSegment> Split(this IReadOnlyTensorSegment segment, uint blockCount)
        {
            for (uint i = 0, size = segment.Size, blockSize = size / blockCount; i < size; i += blockSize)
                yield return new ReadOnlyTensorSegmentWrapper(segment, i, 1, blockSize);
        }

        /// <summary>
        /// Invokes a callback on each element of the tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="analyser">Callback that will receive each value and its corresponding index in the segment</param>
        public static void Analyze(this IReadOnlyTensorSegment segment, Action<float /* value */, uint /* index */> analyser)
        {
            var size = segment.Size;
            if (size >= Consts.MinimumSizeForParallel)
                Parallel.For(0, size, i => analyser(segment[i], (uint)i));
            else {
                for (uint i = 0; i < size; i++)
                    analyser(segment[i], i);
            }
        }

        /// <summary>
        /// Sets all values of the tensor segment via a callback that receives each value
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="setValue"></param>
        public static void Set(this ITensorSegment segment, Func<float /* old value */, float /* new value */> setValue)
        {
            for (uint i = 0, len = segment.Size; i < len; i++)
                segment[i] = setValue(segment[i]);
        }

        /// <summary>
        /// Sets all values of the tensor segment via a callback that receives the index of each value
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="setValue"></param>
        public static void Set(this ITensorSegment segment, Func<uint /* index */, float> setValue)
        {
            for (uint i = 0, len = segment.Size; i < len; i++)
                segment[i] = setValue(i);
        }

        /// <summary>
        /// Sets all values of the tensor segment to a single value
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="value">Value to set</param>
        public static void Set(this ITensorSegment segment, float value)
        {
            var size = segment.Size;
            if (size >= Consts.MinimumSizeForParallel) {
                Parallel.For(0, size, ind => segment[ind] = value);
            }
            else {
                for (uint i = 0, len = segment.Size; i < len; i++)
                    segment[i] = value;
            }
        }

        /// <summary>
        /// Sets all values of this tensor segment to a random floating point number
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="random">Random number generator</param>
        public static void SetToRandom(this ITensorSegment segment, Random random)
        {
            for (uint i = 0, len = segment.Size; i < len; i++)
                segment[i] = System.Convert.ToSingle(random.NextDouble());
        }

        /// <summary>
        /// Converts the tensor segment to a read only vector
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public static IReadOnlyVector ToReadOnlyVector(this IReadOnlyTensorSegment segment) => new ReadOnlyVectorWrapper(segment);

        /// <summary>
        /// Creates a vector from a tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="lap">Linear algebra provider</param>
        /// <returns></returns>
        public static IVector ToVector(this IReadOnlyTensorSegment segment, LinearAlgebraProvider lap) => lap.CreateVector(segment);

        /// <summary>
        /// Creates a matrix from a tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="lap">Linear algebra provider</param>
        /// <param name="rows">Number of rows in matrix</param>
        /// <param name="columns">Number of columns in matrix</param>
        /// <returns></returns>
        public static IMatrix ToMatrix(this IReadOnlyTensorSegment segment, LinearAlgebraProvider lap, uint rows, uint columns) => lap.CreateMatrix(rows, columns, segment);

        /// <summary>
        /// Creates a 3D tensor from a tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="lap">Linear algebra provider</param>
        /// <param name="depth">Number of matrices</param>
        /// <param name="rows">Number of rows in each matrix</param>
        /// <param name="columns">Number of columns in each matrix</param>
        /// <returns></returns>
        public static ITensor3D ToTensor3D(this IReadOnlyTensorSegment segment, LinearAlgebraProvider lap, uint depth, uint rows, uint columns) => lap.CreateTensor3D(depth, rows, columns, segment);

        /// <summary>
        /// Creates a 4D tensor from a tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="lap">Linear algebra provider</param>
        /// <param name="count">Number of 3D tensors</param>
        /// <param name="depth">Number of matrices in each 3D tensor</param>
        /// <param name="rows">Number of rows in each matrix</param>
        /// <param name="columns">Number of columns in each matrix</param>
        /// <returns></returns>
        public static ITensor4D ToTensor4D(this IReadOnlyTensorSegment segment, LinearAlgebraProvider lap, uint count, uint depth, uint rows, uint columns) => lap.CreateTensor4D(count, depth, rows, columns, segment);

        public delegate RT OnReadOnlyFloatSpan<out RT>(ReadOnlySpan<float> span);
        public static RT GetReadOnlySpan<RT>(this ITensorSegment segment, OnReadOnlyFloatSpan<RT> callback)
        {
            var array = segment.GetArrayIfEasilyAvailable();
            if (array is not null)
                return callback(new Span<float>(array, 0, (int)segment.Size));
            return GetReadOnlySpan<IReadOnlyTensorSegment, RT>(segment, callback);
        }
        public static RT GetReadOnlySpan<T, RT>(this T segment, OnReadOnlyFloatSpan<RT> callback) where T : IReadOnlyTensorSegment
        {
            var temp = SpanOwner<float>.Empty;
            var wasTempUsed = false;
            try {
                return callback(segment.GetFloatSpan(ref temp, out wasTempUsed));
            }
            finally {
                if (wasTempUsed)
                    temp.Dispose();
            }
        }

        public delegate RT OnReadOnlyFloatSpans<out RT>(ReadOnlySpan<float> span1, ReadOnlySpan<float> span2);
        public static RT GetReadOnlySpans<RT>(this ITensorSegment segment1, ITensorSegment segment2, OnReadOnlyFloatSpans<RT> callback)
        {
            var array1 = segment1.GetArrayIfEasilyAvailable();
            var array2 = segment2.GetArrayIfEasilyAvailable();
            if (array1 is not null && array2 is not null)
                return callback(new Span<float>(array1, 0, (int)segment1.Size), new Span<float>(array2, 0, (int)segment2.Size));
            return GetReadOnlySpans<IReadOnlyTensorSegment, RT>(segment1, segment2, callback);
        }
        public static RT GetReadOnlySpans<T, RT>(this T segment1, IReadOnlyTensorSegment segment2, OnReadOnlyFloatSpans<RT> callback)  where T: IReadOnlyTensorSegment
        {
            SpanOwner<float> temp1 = SpanOwner<float>.Empty, temp2 = SpanOwner<float>.Empty;
            bool wasTemp1Used = false, wasTemp2Used = false;
            try {
                var s1 = segment1.GetFloatSpan(ref temp1, out wasTemp1Used);
                var s2 = segment2.GetFloatSpan(ref temp2, out wasTemp2Used);
                return callback(s1, s2);
            }
            finally {
                if (wasTemp1Used)
                    temp1.Dispose();
                if (wasTemp2Used)
                    temp2.Dispose();
            }
        }

        public delegate void OnFloatSpans(Span<float> span1, ReadOnlySpan<float> span2);
        public static unsafe void GetSpans(this ITensorSegment segment1, ITensorSegment segment2, OnFloatSpans callback)
        {
            if (segment1.IsWrapper)
                throw new ArgumentException("Tensor segment wrappers cannot be modified");
            var array1 = segment1.GetArrayIfEasilyAvailable();
            var array2 = segment2.GetArrayIfEasilyAvailable();
            if (array1 is not null && array2 is not null)
                callback(new Span<float>(array1, 0, (int)segment1.Size), new Span<float>(array2, 0, (int)segment2.Size));
            else {
                SpanOwner<float> temp1 = SpanOwner<float>.Empty, temp2 = SpanOwner<float>.Empty;
                bool wasTemp1Used = false, wasTemp2Used = false;
                try {
                    var s1 = segment1.GetFloatSpan(ref temp1, out wasTemp1Used);
                    var s2 = segment2.GetFloatSpan(ref temp2, out wasTemp2Used);
                    fixed (float* ptr = s1) {
                        callback(new Span<float>(ptr, s1.Length), s2);
                    }
                }
                finally {
                    if (wasTemp1Used)
                        temp1.Dispose();
                    if (wasTemp2Used)
                        temp2.Dispose();
                }
            }
        }

        public delegate void OnFloatSpan(Span<float> span);
        public static unsafe void GetSpan(this ITensorSegment segment, OnFloatSpan callback)
        {
            if (segment.IsWrapper)
                throw new ArgumentException("Tensor segment wrappers cannot be modified");
            var array = segment.GetArrayIfEasilyAvailable();
            if (array is not null)
                callback(new Span<float>(array, 0, (int)segment.Size));
            else {
                var temp = SpanOwner<float>.Empty;
                var wasTempUsed = false;
                try {
                    var span = segment.GetFloatSpan(ref temp, out wasTempUsed);
                    fixed (float* ptr = span) {
                        callback(new Span<float>(ptr, span.Length));
                    }
                }
                finally {
                    if (wasTempUsed)
                        temp.Dispose();
                }
            }
        }
    }
}
