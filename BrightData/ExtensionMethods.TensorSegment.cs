using BrightData.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance.Buffers;
using BrightData.LinearAlgebra;
using System.Runtime.Intrinsics.X86;
using BrightData.LinearAlgebra.ReadOnly;
using BrightData.LinearAlgebra.Segments;

namespace BrightData
{
    public partial class ExtensionMethods
    {
        /// <summary>
        /// Converts the tensor segment to a sparse format (only non zero entries are preserved)
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public static WeightedIndexList ToSparse(this IReadOnlyNumericSegment<float> segment)
        {
            return WeightedIndexList.Create(segment.Values
                .Select((v, i) => new WeightedIndexList.Item((uint)i, v))
                .Where(d => FloatMath.IsNotZero(d.Weight))
            );
        }

        /// <summary>
        /// Splits this tensor segment into multiple contiguous tensor segments
        /// </summary>
        /// <param name="segment">This tensor</param>
        /// <param name="blockCount">Number of blocks</param>
        /// <returns></returns>
        public static IEnumerable<IReadOnlyNumericSegment<float>> Split(this IReadOnlyNumericSegment<float> segment, uint blockCount)
        {
            for (uint i = 0, size = segment.Size, blockSize = size / blockCount; i < size; i += blockSize)
                yield return new ReadOnlyTensorSegmentWrapper(segment, i, 1, blockSize);
        }

        /// <summary>
        /// Sets all values of the tensor segment via a callback that receives each value
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="setValue"></param>
        public static void Set(this INumericSegment<float> segment, Func<float /* old value */, float /* new value */> setValue)
        {
            for (uint i = 0, len = segment.Size; i < len; i++)
                segment[i] = setValue(segment[i]);
        }

        /// <summary>
        /// Sets all values of the tensor segment via a callback that receives the index of each value
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="setValue"></param>
        public static void Set(this INumericSegment<float> segment, Func<uint /* index */, float> setValue)
        {
            for (uint i = 0, len = segment.Size; i < len; i++)
                segment[i] = setValue(i);
        }

        /// <summary>
        /// Sets all values of the tensor segment to a single value
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="value">Value to set</param>
        public static void Set(this INumericSegment<float> segment, float value)
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
        public static void SetToRandom(this INumericSegment<float> segment, Random random)
        {
            for (uint i = 0, len = segment.Size; i < len; i++)
                segment[i] = System.Convert.ToSingle(random.NextDouble());
        }

        /// <summary>
        /// Converts the tensor segment to a read only vector
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public static IReadOnlyVector ToReadOnlyVector(this IReadOnlyNumericSegment<float> segment) => new ReadOnlyVectorWrapper(segment);

        /// <summary>
        /// Creates a vector from a tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="lap">Linear algebra provider</param>
        /// <returns></returns>
        public static IVector ToVector(this IReadOnlyNumericSegment<float> segment, LinearAlgebraProvider lap) => lap.CreateVector(segment);

        /// <summary>
        /// Creates a matrix from a tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="lap">Linear algebra provider</param>
        /// <param name="rows">Number of rows in matrix</param>
        /// <param name="columns">Number of columns in matrix</param>
        /// <returns></returns>
        public static IMatrix ToMatrix(this IReadOnlyNumericSegment<float> segment, LinearAlgebraProvider lap, uint rows, uint columns) => lap.CreateMatrix(rows, columns, segment);

        /// <summary>
        /// Creates a 3D tensor from a tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="lap">Linear algebra provider</param>
        /// <param name="depth">Number of matrices</param>
        /// <param name="rows">Number of rows in each matrix</param>
        /// <param name="columns">Number of columns in each matrix</param>
        /// <returns></returns>
        public static ITensor3D ToTensor3D(this IReadOnlyNumericSegment<float> segment, LinearAlgebraProvider lap, uint depth, uint rows, uint columns) => lap.CreateTensor3D(depth, rows, columns, segment);

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
        public static ITensor4D ToTensor4D(this IReadOnlyNumericSegment<float> segment, LinearAlgebraProvider lap, uint count, uint depth, uint rows, uint columns) => lap.CreateTensor4D(count, depth, rows, columns, segment);

        /// <summary>
        /// Callback that takes a readonly span
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="RT"></typeparam>
        /// <param name="span"></param>
        /// <returns></returns>
        public delegate RT OnReadOnlySpan<T, out RT>(ReadOnlySpan<T> span);

        /// <summary>
        /// Passes the segment as a readonly span to the callback
        /// </summary>
        /// <typeparam name="RT"></typeparam>
        /// <param name="segment"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static RT GetReadOnlySpan<RT>(this INumericSegment<float> segment, OnReadOnlySpan<float, RT> callback)
        {
            var (array, offset, stride) = segment.GetUnderlyingArray();
            if (array is not null && stride == 1)
                return callback(new Span<float>(array, (int)offset, (int)segment.Size));
            return GetReadOnlySpan<IReadOnlyNumericSegment<float>, RT>(segment, callback);
        }

        /// <summary>
        /// Passes the segment as a readonly span to the callback
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="RT"></typeparam>
        /// <param name="segment"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static RT GetReadOnlySpan<T, RT>(this T segment, OnReadOnlySpan<float, RT> callback) where T : IReadOnlyNumericSegment<float>
        {
            var contiguous = segment.Contiguous;
            if (contiguous is not null)
                return callback(contiguous.ReadOnlySpan);
            else {
                var temp = SpanOwner<float>.Empty;
                var wasTempUsed = false;
                try {
                    return callback(segment.GetSpan(ref temp, out wasTempUsed));
                }
                finally {
                    if (wasTempUsed)
                        temp.Dispose();
                }
            }
        }

        /// <summary>
        /// Callback that takes two readonly spans
        /// </summary>
        /// <typeparam name="RT"></typeparam>
        /// <typeparam name="T"></typeparam>
        public delegate RT OnReadOnlySpans<T, out RT>(ReadOnlySpan<T> span1, ReadOnlySpan<T> span2);

        /// <summary>
        /// Passes the readonly spans from the supplied segments into a callback function
        /// </summary>
        /// <typeparam name="RT"></typeparam>
        /// <param name="segment1"></param>
        /// <param name="segment2"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static RT GetReadOnlySpans<RT>(this INumericSegment<float> segment1, INumericSegment<float> segment2, OnReadOnlySpans<float, RT> callback)
        {
            var (array1, offset1, stride1) = segment1.GetUnderlyingArray();
            var (array2, offset2, stride2) = segment2.GetUnderlyingArray();
            if (array1 is not null && array2 is not null && stride1 == 1 && stride2 == 1)
                return callback(new Span<float>(array1, (int)offset1, (int)segment1.Size), new Span<float>(array2, (int)offset2, (int)segment2.Size));
            return GetReadOnlySpans<IReadOnlyNumericSegment<float>, RT>(segment1, segment2, callback);
        }

        /// <summary>
        /// Passes the readonly spans from the supplied segments into a callback function
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="RT"></typeparam>
        /// <param name="segment1"></param>
        /// <param name="segment2"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static RT GetReadOnlySpans<T, RT>(this T segment1, IReadOnlyNumericSegment<float> segment2, OnReadOnlySpans<float, RT> callback)  where T: IReadOnlyNumericSegment<float>
        {
            SpanOwner<float> temp1 = SpanOwner<float>.Empty, temp2 = SpanOwner<float>.Empty;
            bool wasTemp1Used = false, wasTemp2Used = false;
            try {
                var s1 = segment1.GetSpan(ref temp1, out wasTemp1Used);
                var s2 = segment2.GetSpan(ref temp2, out wasTemp2Used);
                return callback(s1, s2);
            }
            finally {
                if (wasTemp1Used)
                    temp1.Dispose();
                if (wasTemp2Used)
                    temp2.Dispose();
            }
        }

        /// <summary>
        /// Callback that takes one mutable span and one readonly span
        /// </summary>
        /// <param name="span1"></param>
        /// <param name="span2"></param>
        public delegate void OnSpans<T>(Span<T> span1, ReadOnlySpan<T> span2);

        /// <summary>
        /// Passes the first as a mutable span and the second as a readonly span from the supplied segments into a callback function
        /// </summary>
        /// <param name="segment1"></param>
        /// <param name="segment2"></param>
        /// <param name="callback"></param>
        /// <exception cref="ArgumentException"></exception>
        public static unsafe void GetSpans(this INumericSegment<float> segment1, INumericSegment<float> segment2, OnSpans<float> callback)
        {
            if (segment1.IsWrapper)
                throw new ArgumentException("Tensor segment wrappers cannot be modified");
            var (array1, offset1, stride1) = segment1.GetUnderlyingArray();
            var (array2, offset2, stride2) = segment2.GetUnderlyingArray();
            if (array1 is not null && array2 is not null && stride1 == 1 && stride2 == 1)
                callback(new Span<float>(array1, (int)offset1, (int)segment1.Size), new Span<float>(array2, (int)offset2, (int)segment2.Size));
            else {
                SpanOwner<float> temp1 = SpanOwner<float>.Empty, temp2 = SpanOwner<float>.Empty;
                bool wasTemp1Used = false, wasTemp2Used = false;
                try {
                    var s1 = segment1.GetSpan(ref temp1, out wasTemp1Used);
                    var s2 = segment2.GetSpan(ref temp2, out wasTemp2Used);
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

        /// <summary>
        /// Callback that takes a single mutable span
        /// </summary>
        /// <param name="span"></param>
        public delegate void OnSpan<T>(Span<T> span);

        /// <summary>
        /// Passes the segment as a mutable span into a callback function
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="callback"></param>
        /// <exception cref="ArgumentException"></exception>
        public static unsafe void GetSpan(this INumericSegment<float> segment, OnSpan<float> callback)
        {
            if (segment.IsWrapper)
                throw new ArgumentException("Tensor segment wrappers cannot be modified");
            var (array, offset, stride) = segment.GetUnderlyingArray();
            if (array is not null && stride == 1)
                callback(new Span<float>(array, (int)offset, (int)segment.Size));
            else {
                var temp = SpanOwner<float>.Empty;
                var wasTempUsed = false;
                try {
                    var span = segment.GetSpan(ref temp, out wasTempUsed);
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

        /// <summary>
        /// Callback that takes a single readonly span
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span"></param>
        public delegate void OnReadOnlySpan<T>(ReadOnlySpan<T> span);

        /// <summary>
        /// Passes the segment as a readonly span into a callback function
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="callback"></param>
        /// <typeparam name="CT"></typeparam>
        public static void GetReadOnlySpan<CT>(this IReadOnlyNumericSegment<float> segment, OnReadOnlySpan<float> callback)
        {
            var contiguous = segment.Contiguous;
            if (contiguous is not null)
                callback(contiguous.ReadOnlySpan);
            else {
                GetReadOnlySpan(segment, callback);
                var temp = SpanOwner<float>.Empty;
                var wasTempUsed = false;
                try {
                    var span = segment.GetSpan(ref temp, out wasTempUsed);
                    callback(span);
                }
                finally {
                    if (wasTempUsed)
                        temp.Dispose();
                }
            }
        }

        /// <summary>
        /// Invokes the callback with the contents of the span
        /// </summary>
        /// <param name="spanContainer"></param>
        /// <param name="callback"></param>
        /// <typeparam name="T"></typeparam>
        public static void GetReadOnlySpan<T>(this IHaveSpanOf<T> spanContainer, OnReadOnlySpan<T> callback)
        {
            var temp = SpanOwner<T>.Empty;
            var wasTempUsed = false;
            try {
                callback(spanContainer.GetSpan(ref temp, out wasTempUsed));
            }
            finally {
                if (wasTempUsed)
                    temp.Dispose();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static INumericSegment<float> Apply(IReadOnlyNumericSegment<float> vector, IReadOnlyNumericSegment<float> other, OnReadOnlySpans<float, MemoryOwner<float>> mutator)
        {
            var result = vector.GetReadOnlySpans(other, mutator);
            return new ArrayPoolTensorSegment(result);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static INumericSegment<float> Add(this IReadOnlyNumericSegment<float> vector, IReadOnlyNumericSegment<float> other) => Apply(vector, other, (x, y) => x.Add(y));
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static INumericSegment<float> Add(this IReadOnlyNumericSegment<float> vector, IReadOnlyNumericSegment<float> other, float coefficient1, float coefficient2) => Apply(vector, other, (x, y) => x.Add(y, coefficient1, coefficient2));
    }
}
