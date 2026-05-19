using BrightData.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance.Buffers;
using BrightData.LinearAlgebra;
using BrightData.LinearAlgebra.ReadOnly;
using BrightData.LinearAlgebra.Segments;
using BrightData.Types;
using CommunityToolkit.HighPerformance;
using System.Runtime.CompilerServices;

namespace BrightData
{
    public partial class ExtensionMethods
    {
        /// <summary>
        /// Creates a tensor segment from a memory owner
        /// </summary>
        /// <param name="memoryOwner"></param>
        /// <returns></returns>
        public static INumericSegment<T> ToSegment<T>(this MemoryOwner<T> memoryOwner) where T: unmanaged, INumber<T> => new ArrayPoolTensorSegment<T>(memoryOwner);

        /// <summary>
        /// Converts the tensor segment to a sparse format (only non-zero entries are preserved)
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public static WeightedIndexList ToSparse(this IReadOnlyNumericSegment<float> segment)
        {
            return WeightedIndexList.Create(segment.Values
                .Select((v, i) => new WeightedIndexList.Item((uint)i, v))
                .Where(d => Math<float>.IsNotZero(d.Weight))
            );
        }

        /// <summary>
        /// Returns the index with the minimum value from this tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (T Min, T Max, uint MinIndex, uint MaxIndex) GetMinAndMaxValues<T>(this IReadOnlyNumericSegment<T> segment) where T : unmanaged, INumber<T>, IMinMaxValue<T> => segment.ApplyReadOnlySpan(x => x.GetMinAndMaxValues());

        /// <summary>
        /// Returns the index with the minimum value from this tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static uint GetMinimumIndex<T>(this IReadOnlyNumericSegment<T> segment) where T : unmanaged, INumber<T>, IMinMaxValue<T> => GetMinAndMaxValues(segment).MinIndex;

        /// <summary>
        /// Returns the index with the maximum value from this tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static uint GetMaximumIndex<T>(this IReadOnlyNumericSegment<T> segment) where T : unmanaged, INumber<T>, IMinMaxValue<T> => GetMinAndMaxValues(segment).MaxIndex;

        /// <summary>
        /// Sums all values
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static T Sum<T>(this IReadOnlyNumericSegment<T> segment) where T : unmanaged, IBinaryFloatingPointIeee754<T> => segment.ApplyReadOnlySpan(x => x.Sum());

        /// <summary>
        /// Finds cosine distance (0 for perpendicular, 1 for orthogonal, 2 for opposite) between this and another vector
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static T CosineDistance<T>(this IReadOnlyNumericSegment<T> vector, IReadOnlyNumericSegment<T> other) where T : unmanaged, IBinaryFloatingPointIeee754<T> => vector.ReduceReadOnlySpans(other, (x,y) => x.CosineDistance(y));

        /// <summary>
        /// Finds the euclidean distance between this and another vector
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static T EuclideanDistance<T>(this IReadOnlyNumericSegment<T> vector, IReadOnlyNumericSegment<T> other) where T : unmanaged, IBinaryFloatingPointIeee754<T> => vector.ReduceReadOnlySpans(other, (x,y) => x.EuclideanDistance(y));

        /// <summary>
        /// Finds the manhattan distance between this and another vector
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static T ManhattanDistance<T>(this IReadOnlyNumericSegment<T> vector, IReadOnlyNumericSegment<T> other) where T : unmanaged, IBinaryFloatingPointIeee754<T> => vector.ReduceReadOnlySpans(other, (x,y) => x.ManhattanDistance(y));

        /// <summary>
        /// Finds the mean squared distance between this and another vector
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static T MeanSquaredDistance<T>(this IReadOnlyNumericSegment<T> vector, IReadOnlyNumericSegment<T> other) where T : unmanaged, IBinaryFloatingPointIeee754<T> => vector.ReduceReadOnlySpans(other, (x,y) => x.MeanSquaredDistance(y));

        /// <summary>
        /// Finds the squared euclidean distance between this and another vector
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static T SquaredEuclideanDistance<T>(this IReadOnlyNumericSegment<T> vector, IReadOnlyNumericSegment<T> other) where T : unmanaged, IBinaryFloatingPointIeee754<T> => vector.ReduceReadOnlySpans(other, (x,y) => x.SquaredEuclideanDistance(y));

        /// <summary>
        /// Finds the distance between this and another vector
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="other"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static T FindDistance<T>(this IReadOnlyNumericSegment<T> vector, IReadOnlyNumericSegment<T> other, DistanceMetric distance) where T : unmanaged, IBinaryFloatingPointIeee754<T> => vector.ReduceReadOnlySpans(other, (x,y) => x.FindDistance(y, distance));

        /// <summary>
        /// Splits this tensor segment into multiple contiguous tensor segments
        /// </summary>
        /// <param name="segment">This tensor</param>
        /// <param name="blockCount">Number of blocks</param>
        /// <returns></returns>
        public static IEnumerable<IReadOnlyNumericSegment<T>> Split<T>(this IReadOnlyNumericSegment<T> segment, uint blockCount) where T: unmanaged, INumber<T>
        {
            for (uint i = 0, size = segment.Size, blockSize = size / blockCount; i < size; i += blockSize)
                yield return new ReadOnlyTensorSegmentWrapper<T>(segment, i, 1, blockSize);
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
        public static IReadOnlyVector<T> ToReadOnlyVector<T>(this IReadOnlyNumericSegment<T> segment) where T: unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T> => new ReadOnlyVector<T>(segment);

        /// <summary>
        /// Creates a vector from a tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="lap">Linear algebra provider</param>
        /// <returns></returns>
        public static IVector<T> ToVector<T>(this IReadOnlyNumericSegment<T> segment, LinearAlgebraProvider<T> lap) where T: unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T> => 
            lap.CreateVector(segment);

        /// <summary>
        /// Creates a matrix from a tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="lap">Linear algebra provider</param>
        /// <param name="rows">Number of rows in matrix</param>
        /// <param name="columns">Number of columns in matrix</param>
        /// <returns></returns>
        public static IMatrix<T> ToMatrix<T>(this IReadOnlyNumericSegment<T> segment, LinearAlgebraProvider<T> lap, uint rows, uint columns) where T: unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T> => 
            lap.CreateMatrix(rows, columns, segment);

        /// <summary>
        /// Creates a 3D tensor from a tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="lap">Linear algebra provider</param>
        /// <param name="depth">Number of matrices</param>
        /// <param name="rows">Number of rows in each matrix</param>
        /// <param name="columns">Number of columns in each matrix</param>
        /// <returns></returns>
        public static ITensor3D<T> ToTensor3D<T>(this IReadOnlyNumericSegment<T> segment, LinearAlgebraProvider<T> lap, uint depth, uint rows, uint columns) where T: unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T> => 
            lap.CreateTensor3D(depth, rows, columns, segment);

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
        public static ITensor4D<T> ToTensor4D<T>(this IReadOnlyNumericSegment<T> segment, LinearAlgebraProvider<T> lap, uint count, uint depth, uint rows, uint columns) where T: unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T> => 
            lap.CreateTensor4D(count, depth, rows, columns, segment);

        /// <summary>
        /// Callback that takes a span
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public delegate void UseSpan<T>(Span<T> span);


        /// <summary>
        /// Callback that takes a readonly span
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public delegate void UseReadOnlySpan<T>(ReadOnlySpan<T> span);

        /// <summary>
        /// Callback that takes a span and returns a new value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="RT"></typeparam>
        /// <param name="span"></param>
        /// <returns></returns>
        public delegate RT TransformSpan<T, out RT>(Span<T> span);

        /// <summary>
        /// Callback that takes a readonly span and returns a new value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="RT"></typeparam>
        /// <param name="span"></param>
        /// <returns></returns>
        public delegate RT TransformReadOnlySpan<T, out RT>(ReadOnlySpan<T> span);

        /// <summary>
        /// Passes the segment as a span to the callback
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="updateSegment">If true the segment will be updated from the span after the callback</param>
        /// <param name="callback"></param>
        /// <typeparam name="T"></typeparam>
        public static unsafe void ApplySpan<T>(this INumericSegment<T> segment, bool updateSegment, UseSpan<T> callback) where T : unmanaged, INumber<T>
        {
            var (array, offset, stride) = segment.GetUnderlyingArray();
            if (array is not null && stride == 1) {
                var span = new Span<T>(array, (int)offset, (int)segment.Size);
                callback(span);
            }
            else {
                var temp = SpanOwner<T>.Empty;
                var wasTempUsed = false;
                try {
                    var span = segment.GetSpan(ref temp, out wasTempUsed);
                    fixed (T* ptr = span) {
                        var span2 = new Span<T>(ptr, span.Length);
                        callback(span2);
                        if(updateSegment && wasTempUsed)
                            segment.CopyFrom(span2);
                    }
                }
                finally {
                    if (wasTempUsed)
                        temp.Dispose();
                }
            }
        }

        /// <summary>
        /// Passes the segment as a span to the callback
        /// </summary>
        /// <typeparam name="RT"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="segment"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static unsafe RT ApplySpan<T, RT>(this INumericSegment<T> segment, TransformSpan<T, RT> callback) where T : unmanaged, INumber<T>
        {
            var (array, offset, stride) = segment.GetUnderlyingArray();
            if (array is not null && stride == 1)
                return callback(new Span<T>(array, (int)offset, (int)segment.Size));
            
            var temp = SpanOwner<T>.Empty;
            var wasTempUsed = false;
            try {
                var span = segment.GetSpan(ref temp, out wasTempUsed);
                fixed (T* ptr = span) {
                    return callback(new Span<T>(ptr, span.Length));
                }
            }
            finally {
                if (wasTempUsed)
                    temp.Dispose();
            }
        }

        /// <summary>
        /// Passes the segment as a readonly span to the callback
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="callback"></param>
        /// <typeparam name="T"></typeparam>                                                                                                                                                                                                                                                                
        public static void ApplyReadOnlySpan<T>(this IReadOnlyNumericSegment<T> segment, UseReadOnlySpan<T> callback) where T : unmanaged, INumber<T>
        {
            var contiguous = segment.Contiguous;
            if (contiguous is not null)
                callback(contiguous.ReadOnlySpan);
            else {
                var temp = SpanOwner<T>.Empty;
                var wasTempUsed = false;
                try {
                    callback(segment.GetSpan(ref temp, out wasTempUsed));
                }
                finally {
                    if (wasTempUsed)
                        temp.Dispose();
                }
            }
        }

        /// <summary>
        /// Passes the segment as a readonly span to the callback
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="RT"></typeparam>
        /// <param name="segment"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static RT ApplyReadOnlySpan<T, RT>(this IReadOnlyNumericSegment<T> segment, TransformReadOnlySpan<T, RT> callback) where T : unmanaged, INumber<T>
        {
            var contiguous = segment.Contiguous;
            if (contiguous is not null)
                return callback(contiguous.ReadOnlySpan);

            var temp = SpanOwner<T>.Empty;
            var wasTempUsed = false;
            try {
                return callback(segment.GetSpan(ref temp, out wasTempUsed));
            }
            finally {
                if (wasTempUsed)
                    temp.Dispose();
            }
        }                                   

        /// <summary>
        /// Callback that takes two spans
        /// </summary>
        /// <typeparam name="T"></typeparam>                                                                                                                                                
        public delegate void UseReadOnlySpans<T>(ReadOnlySpan<T> span1, ReadOnlySpan<T> span2);

        /// <summary>
        /// Callback that takes two readonly spans
        /// </summary>
        /// <typeparam name="RT"></typeparam>
        /// <typeparam name="T"></typeparam>
        public delegate RT TransformReadOnlySpans<T, out RT>(ReadOnlySpan<T> span1, ReadOnlySpan<T> span2);

        /// <summary>
        /// Passes the readonly spans from the supplied segments into a callback function
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="segment1"></param>
        /// <param name="segment2"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static void ApplyReadOnlySpans<T>(this IReadOnlyNumericSegment<T> segment1, IReadOnlyNumericSegment<T> segment2, UseReadOnlySpans<T> callback) where T : unmanaged, INumber<T>
        {
            SpanOwner<T> temp1 = SpanOwner<T>.Empty, temp2 = SpanOwner<T>.Empty;
            bool wasTemp1Used = false, wasTemp2Used = false;
            try {
                var s1 = segment1.GetSpan(ref temp1, out wasTemp1Used);
                var s2 = segment2.GetSpan(ref temp2, out wasTemp2Used);
                callback(s1, s2);
            }
            finally {
                if (wasTemp1Used)
                    temp1.Dispose();
                if (wasTemp2Used)
                    temp2.Dispose();
            }
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
        public static RT ReduceReadOnlySpans<T, RT>(this IReadOnlyNumericSegment<T> segment1, IReadOnlyNumericSegment<T> segment2, TransformReadOnlySpans<T, RT> callback)  where T: unmanaged, INumber<T>
        {
            SpanOwner<T> temp1 = SpanOwner<T>.Empty, temp2 = SpanOwner<T>.Empty;
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
        /// <param name="updateSegment">If true the segment will be updated from the span after the callback</param>
        /// <param name="segment2"></param>
        /// <param name="callback"></param>
        /// <exception cref="ArgumentException"></exception>
        public static unsafe void ApplySpans<T>(this INumericSegment<T> segment1, bool updateSegment, IReadOnlyNumericSegment<T> segment2, OnSpans<T> callback)
            where T: unmanaged, INumber<T>
        {
            var (array1, offset1, stride1) = segment1.GetUnderlyingArray();
            if (array1 is not null && stride1 == 1) {
                var editableSpan = new Span<T>(array1, (int)offset1, (int)segment1.Size);
                var temp = SpanOwner<T>.Empty;
                var wasTempUsed = false;
                try {
                    var readOnlySpan = segment2.GetSpan(ref temp, out wasTempUsed);
                    callback(editableSpan, readOnlySpan);
                }
                finally {
                    if (wasTempUsed)
                        temp.Dispose();
                }
            }
            else {
                SpanOwner<T> temp1 = SpanOwner<T>.Empty, temp2 = SpanOwner<T>.Empty;
                bool wasTemp1Used = false, wasTemp2Used = false;
                try {
                    var s1 = segment1.GetSpan(ref temp1, out wasTemp1Used);
                    var s2 = segment2.GetSpan(ref temp2, out wasTemp2Used);
                    fixed (T* ptr = s1) {
                        var editableSpan = new Span<T>(ptr, s1.Length);
                        callback(editableSpan, s2);
                        if(updateSegment && wasTemp1Used)
                            segment1.CopyFrom(editableSpan);
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
        /// Writes the numeric segment as bytes to the binary writer
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="segment"></param>
        /// <typeparam name="T"></typeparam>
        public static void Write<T>(this BinaryWriter writer, IReadOnlyNumericSegment<T> segment) where T : unmanaged, INumber<T>
        {
            var contiguous = segment.Contiguous;
            if(contiguous is not null)
                writer.Write(contiguous.ReadOnlySpan.AsBytes());
            else {
                var temp = SpanOwner<T>.Empty;
                var wasTempUsed = false;
                try {
                    var span = segment.GetSpan(ref temp, out wasTempUsed);
                    writer.Write(span.AsBytes());
                }
                finally {
                    if (wasTempUsed)
                        temp.Dispose();
                }
            }
        }

        /// <summary>
        /// Returns the data in the segment as a span of bytes
        /// </summary>
        /// <param name="segment"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ReadOnlySpan<byte> AsBytes<T>(this IReadOnlyNumericSegment<T> segment) where T : unmanaged, INumber<T>
        {
            var contiguous = segment.Contiguous;
            if(contiguous is not null)
                return contiguous.ReadOnlySpan.AsBytes();
            else {
                var temp = SpanOwner<T>.Empty;
                var wasTempUsed = false;
                try {
                    var span = segment.GetSpan(ref temp, out wasTempUsed);
                    return span.AsBytes().ToArray();
                }
                finally {
                    if (wasTempUsed)
                        temp.Dispose();
                }
            }
        }

        /// <summary>
        /// Applies a mapping function to each value in the segment to create a new segment (potentially in parallel)
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="mapper">Mapping function that receives each value from the segment</param>
        /// <returns></returns>
        public static INumericSegment<T> MapParallel<T>(this IReadOnlyNumericSegment<T> segment, Func<T /* value */, T /* new value */> mapper) where T: unmanaged, INumber<T>
        {
            var ret = segment.ApplyReadOnlySpan(x => x.MapParallel(mapper));
            return new ArrayPoolTensorSegment<T>(ret);
        }

        /// <summary>
        /// Applies a mapping function to each value in the segment in place (potentially in parallel)
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="mapper">Mapping function that receives each value from the segment</param>
        public static void MapParallelInPlace<T>(this INumericSegment<T> segment, Func<T /* value */, T /* new value */> mapper) where T: unmanaged, INumber<T>
        {
            segment.ApplySpan(true, x => x.MutateInPlace(mapper));
        }

        /// <summary>
        /// Applies a mapping function to each value in the segment to create a new segment (potentially in parallel)
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="mapper">Mapping function that receives the index and each value from the segment</param>
        /// <returns></returns>
        public static INumericSegment<T> MapParallel<T>(this IReadOnlyNumericSegment<T> segment, Func<uint /* index */, T /* value */, T /* new value */> mapper) where T: unmanaged, INumber<T>
        {
            var ret = segment.ApplyReadOnlySpan(x => x.MapParallel(mapper));
            return new ArrayPoolTensorSegment<T>(ret);
        }

        /// <summary>
        /// Applies a mapping function to each value in the segment in place (potentially in parallel)
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="mapper">Mapping function that receives the index and each value from the segment</param>
        public static void MapParallelInPlace<T>(this INumericSegment<T> segment, Func<uint /* index */, T /* value */, T /* new value */> mapper) where T: unmanaged, INumber<T>
        {
            segment.ApplySpan(true, x => x.MutateInPlace(mapper));
        }

        /// <summary>
        /// Calculate the matrix transpose
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="rowCount"></param>
        /// <param name="columnCount"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static (IReadOnlyNumericSegment<T> Segment, uint RowCount, uint ColumnCount) Transpose<T>(this IReadOnlyNumericSegment<T> segment, uint rowCount, uint columnCount) where T: unmanaged, INumber<T>
        {
            var (buffer, newRowCount, newColumnCount) = segment.ApplyReadOnlySpan(x => x.Transpose(rowCount, columnCount));
            try {
                return (new ReadOnlyTensorSegment<T>(buffer.Memory.ToArray()), newRowCount, newColumnCount);
            }
            finally {
                buffer.Dispose();
            }
        }

        /// <summary>
        /// Returns contiguous memory from a numeric segment (will be a copy if the segment is not contiguous)
        /// </summary>
        /// <param name="segment"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ReadOnlyMemory<T> GetMemory<T>(this IReadOnlyNumericSegment<T> segment) 
            where T : unmanaged, INumber<T>
        {
            IHaveReadOnlyContiguousMemory<T>? contiguous;
            return (contiguous = segment.Contiguous) != null 
                ? contiguous.ContiguousMemory 
                : segment.ToNewArray()
            ;
        }

        /// <summary>
        /// Finds the top N indices from the segment
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="AT"></typeparam>
        /// <param name="segment"></param>
        /// <returns></returns>
        public static AT NIndices<T, AT>(this IReadOnlyNumericSegment<T> segment)
            where T : unmanaged, INumber<T>
            where AT: IFixedSizeSortedArray<uint, T>, new()
        {
            return segment.ApplyReadOnlySpan(x => x.NIndices<T, AT>());
        }

        /// <summary>
        /// Creates a new tensor segment from applying an operation to each pair of elements from this and another tensor segment
        /// </summary>
        /// <param name="segment">This tensor segment</param>
        /// <param name="other">Other tensor segment</param>
        /// <param name="func">Function that computes a new value from a pair of values</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static INumericSegment<T> ZipParallel<T>(
            this IReadOnlyNumericSegment<T> segment,
            IReadOnlyNumericSegment<T> other,
            Func<T, T, T> func
        ) where T : unmanaged, INumber<T>
        {
            var ret = segment.ReduceReadOnlySpans(other, (x, y) => x.ZipParallel(y, func));
            return new ArrayPoolTensorSegment<T>(ret);
        }

        /// <summary>
        /// Applies a function across each pair of elements from this and another tensor segment
        /// </summary>
        /// <param name="segment">This tensor segment</param>
        /// <param name="other">Other tensor segment</param>
        /// <param name="func1">Vector callback</param>
        /// <param name="func2">Element callback</param>
        /// <returns></returns>
        public static INumericSegment<T> ZipVectorized<T>(
            this IReadOnlyNumericSegment<T> segment,
            IReadOnlyNumericSegment<T> other,
            ExtensionMethods.ComputeVectorisedTwo<T> func1,
            Func<T, T, T> func2
        ) where T : unmanaged, INumber<T>
        {
            var ret = segment.ReduceReadOnlySpans(other, (x, y) => x.ZipVectorized(y, func1, func2));
            return new ArrayPoolTensorSegment<T>(ret);
        }

        /// <summary>
        /// Creates a new tensor segment from an existing tensor segment via a vectorization function
        /// </summary>
        /// <param name="segment">Input tensor segment</param>
        /// <param name="transformer1">Vectorized transformer</param>
        /// <param name="transformer2">Sequential transformer</param>
        /// <returns></returns>
        public static INumericSegment<T> TransformVectorized<T>(
            this IReadOnlyNumericSegment<T> segment,
            ExtensionMethods.ComputeVectorisedOne<T> transformer1,
            Func<T, T> transformer2
        ) where T : unmanaged, INumber<T>
        {
            var ret = segment.ApplyReadOnlySpan(x => x.TransformVectorized(transformer1, transformer2));
            return new ArrayPoolTensorSegment<T>(ret);
        }

        /// <summary>
        /// Creates a new tensor segment from an existing tensor segment via a function (possibly executed in parallel) that receives an index and returns a new value
        /// </summary>
        /// <param name="segment">Input tensor segment</param>
        /// <param name="transformer">Transformation function (possibly executed in parallel)</param>
        /// <returns></returns>
        public static INumericSegment<T> TransformParallelIndexed<T>(
            this IReadOnlyNumericSegment<T> segment,
            Func<uint, T> transformer
        ) where T : unmanaged, INumber<T>
        {
            var ret = segment.ApplyReadOnlySpan(x => x.TransformParallelIndexed(transformer));
            return new ArrayPoolTensorSegment<T>(ret);
        }

        /// <summary>
        /// Creates a new tensor segment from an existing tensor segment via a function (possibly executed in parallel) that receives an index and value and returns a new value
        /// </summary>
        /// <param name="segment">Input tensor segment</param>
        /// <param name="transformer">Transformation function (possibly executed in parallel)</param>
        /// <returns></returns>
        public static INumericSegment<T> TransformParallelIndexed<T>(
            this IReadOnlyNumericSegment<T> segment,
            Func<uint, T, T> transformer
        ) where T : unmanaged, INumber<T>
        {
            var ret = segment.ApplyReadOnlySpan(x => x.TransformParallelIndexed(transformer));
            return new ArrayPoolTensorSegment<T>(ret);
        }

        /// <summary>
        /// Updates a tensor segment by applying an update function that receives pairs of values from this and another tensor segment
        /// </summary>
        /// <param name="segment">This tensor segment</param>
        /// <param name="other">Other tensor segment</param>
        /// <param name="func">Update function</param>
        public static void Mutate<T>(
            this INumericSegment<T> segment,
            IReadOnlyNumericSegment<T> other,
            Func<T, T, T> func
        ) where T : unmanaged, INumber<T>
        {
            segment.ApplySpans(true, other, (x, y) => x.Mutate(y, func));
        }

        /// <summary>
        /// Updates a tensor segment by applying a vectorized transformation function to each pair of elements in this and another tensor segment
        /// </summary>
        /// <param name="segment">This tensor segment</param>
        /// <param name="other">Other tensor segment</param>
        /// <param name="transformer1">Vectorized transformer</param>
        /// <param name="transformer2">Sequential transformer</param>
        public static void MutateVectorized<T>(
            this INumericSegment<T> segment,
            IReadOnlyNumericSegment<T> other,
            ExtensionMethods.ComputeVectorisedTwo<T> transformer1,
            Func<T, T, T> transformer2
        ) where T : unmanaged, INumber<T>
        {
            segment.ApplySpans(true, other, (x, y) => x.MutateVectorized(y, transformer1, transformer2));
        }

        /// <summary>
        /// Updates a tensor segment in place by applying a mutation function (potentially called in parallel) to each element
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="mutator"></param>
        public static void MutateInPlace<T>(
            this INumericSegment<T> segment,
            Func<T, T> mutator
        ) where T : unmanaged, INumber<T>
        {
            segment.ApplySpan(true, x => x.MutateInPlace(mutator));
        }

        /// <summary>
        /// Updates a tensor segment in place by applying a mutation function (potentially called in parallel) to each element
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="mutator"></param>
        public static void MutateInPlace<T>(
            this INumericSegment<T> segment,
            Func<uint, T, T> mutator
        ) where T : unmanaged, INumber<T>
        {
            segment.ApplySpan(true, x => x.MutateInPlace(mutator));
        }

        /// <summary>
        /// Updates a tensor segment in place by applying a vectorization function to each value
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="mutator1"></param>
        /// <param name="mutator2"></param>
        public static void MutateInPlaceVectorized<T>(
            this INumericSegment<T> segment,
            ExtensionMethods.ComputeVectorisedOne<T> mutator1,
            Func<T, T> mutator2
        ) where T : unmanaged, INumber<T>
        {
            segment.ApplySpan(true, x => x.MutateInPlaceVectorized(mutator1, mutator2));
        }

        /// <summary>
        /// Calculates the dot product between this tensor segment and another tensor segment
        /// </summary>
        /// <param name="segment">This tensor segment</param>
        /// <param name="other">Other tensor segment</param>
        /// <returns></returns>
        public static T DotProduct<T>(
            this IReadOnlyNumericSegment<T> segment,
            IReadOnlyNumericSegment<T> other
        ) where T : unmanaged, INumber<T>
        {
            return segment.ReduceReadOnlySpans(other, (x, y) => x.DotProduct(y));
        }

        /// <summary>
        /// Returns a new tensor segment that contains this tensor segment added to another tensor segment
        /// </summary>
        /// <param name="segment">This tensor segment</param>
        /// <param name="other">Other tensor segment</param>
        /// <returns></returns>
        public static INumericSegment<T> Add<T>(
            this IReadOnlyNumericSegment<T> segment,
            IReadOnlyNumericSegment<T> other
        ) where T : unmanaged, INumber<T>
        {
            var ret = segment.ReduceReadOnlySpans(other, (x, y) => x.Add(y));
            return new ArrayPoolTensorSegment<T>(ret);
        }

        /// <summary>
        /// Returns a new tensor segment that contains this tensor segment added to another tensor segment where each value is multiplied by coefficients
        /// </summary>
        /// <param name="segment">This tensor segment</param>
        /// <param name="other">Other tensor segment</param>
        /// <param name="coefficient1">Coefficient to apply to each value in this tensor segment</param>
        /// <param name="coefficient2">Coefficient to apply to each value in the other tensor segment</param>
        /// <returns></returns>
        public static INumericSegment<T> Add<T>(
            this IReadOnlyNumericSegment<T> segment,
            IReadOnlyNumericSegment<T> other,
            T coefficient1,
            T coefficient2
        ) where T : unmanaged, INumber<T>
        {
            var ret = segment.ReduceReadOnlySpans(other, (x, y) => x.Add(y, coefficient1, coefficient2));
            return new ArrayPoolTensorSegment<T>(ret);
        }

        /// <summary>
        /// Returns a new tensor segment that contains each value added to a scalar
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="scalar"></param>
        /// <returns></returns>
        public static INumericSegment<T> Add<T>(
            this IReadOnlyNumericSegment<T> segment,
            T scalar
        ) where T : unmanaged, INumber<T>
        {
            var ret = segment.ApplyReadOnlySpan(x => x.Add(scalar));
            return new ArrayPoolTensorSegment<T>(ret);
        }

        /// <summary>
        /// Adds another tensor segment to this tensor segment in place
        /// </summary>
        /// <param name="segment">This tensor segment</param>
        /// <param name="other">Other tensor segment</param>
        public static void AddInPlace<T>(
            this INumericSegment<T> segment,
            IReadOnlyNumericSegment<T> other
        ) where T : unmanaged, INumber<T>
        {
            segment.ApplySpans(true, other, (x, y) => x.AddInPlace(y));
        }

        /// <summary>
        /// Adds another tensor segment to this tensor segment and applies coefficients to each value
        /// </summary>
        /// <param name="segment">This tensor segment</param>
        /// <param name="other">Other tensor segment</param>
        /// <param name="coefficient1">Coefficient to apply to each value in this tensor segment</param>
        /// <param name="coefficient2">Coefficient to apply to each value in the other tensor segment</param>
        public static void AddInPlace<T>(
            this INumericSegment<T> segment,
            IReadOnlyNumericSegment<T> other,
            T coefficient1,
            T coefficient2
        ) where T : unmanaged, INumber<T>
        {
            segment.ApplySpans(true, other, (x, y) => x.AddInPlace(y, coefficient1, coefficient2));
        }

        /// <summary>
        /// Adds a scalar to each value
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="scalar"></param>
        public static void AddInPlace<T>(
            this INumericSegment<T> segment,
            T scalar
        ) where T : unmanaged, INumber<T>
        {
            segment.ApplySpan(true, x => x.AddInPlace(scalar));
        }

        /// <summary>
        /// Multiplies each value by a scalar
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="scalar"></param>
        public static void MultiplyInPlace<T>(
            this INumericSegment<T> segment,
            T scalar
        ) where T : unmanaged, INumber<T>
        {
            segment.ApplySpan(true, x => x.MultiplyInPlace(scalar));
        }

        /// <summary>
        /// Creates a new tensor segment that contains each value multiplied by a scalar
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="scalar"></param>
        /// <returns></returns>
        public static INumericSegment<T> Multiply<T>(
            this IReadOnlyNumericSegment<T> segment,
            T scalar
        ) where T : unmanaged, INumber<T>
        {
            var ret = segment.ApplyReadOnlySpan(x => x.Multiply(scalar));
            return new ArrayPoolTensorSegment<T>(ret);
        }

        /// <summary>
        /// Creates a new tensor segment in which each value in another tensor segment is subtracted from the values in this tensor segment
        /// </summary>
        /// <param name="segment">This tensor segment</param>
        /// <param name="other">Other tensor segment</param>
        /// <returns></returns>
        public static INumericSegment<T> Subtract<T>(
            this IReadOnlyNumericSegment<T> segment,
            IReadOnlyNumericSegment<T> other
        ) where T : unmanaged, INumber<T>
        {
            var ret = segment.ReduceReadOnlySpans(other, (x, y) => x.Subtract(y));
            return new ArrayPoolTensorSegment<T>(ret);
        }

        /// <summary>
        /// Creates a new tensor segment in which each value in another tensor segment is multiplied by the second coefficient and then subtracted from the values in this tensor segment multiplied by the first coefficient
        /// </summary>
        /// <param name="segment">This tensor segment</param>
        /// <param name="other">Other tensor segment</param>
        /// <param name="coefficient1">Coefficient to apply to each value in this tensor segment</param>
        /// <param name="coefficient2">Coefficient to apply to each value in the other tensor segment</param>
        /// <returns></returns>
        public static INumericSegment<T> Subtract<T>(
            this IReadOnlyNumericSegment<T> segment,
            IReadOnlyNumericSegment<T> other,
            T coefficient1,
            T coefficient2
        ) where T : unmanaged, INumber<T>
        {
            var ret = segment.ReduceReadOnlySpans(other, (x, y) => x.Subtract(y, coefficient1, coefficient2));
            return new ArrayPoolTensorSegment<T>(ret);
        }

        /// <summary>
        /// Subtracts another tensor segment from this tensor segment
        /// </summary>
        /// <param name="segment">This tensor segment</param>
        /// <param name="other">Other tensor segment</param>
        public static void SubtractInPlace<T>(
            this INumericSegment<T> segment,
            IReadOnlyNumericSegment<T> other
        ) where T : unmanaged, INumber<T>
        {
            segment.ApplySpans(true, other, (x, y) => x.SubtractInPlace(y));
        }

        /// <summary>
        /// Modifies this tensor segment so that each value in another tensor segment is multiplied by the second coefficient and then subtracted from the values in this tensor segment multiplied by the first coefficient
        /// </summary>
        /// <param name="segment">This tensor segment</param>
        /// <param name="other">Other tensor segment</param>
        /// <param name="coefficient1">Coefficient to apply to each value in this tensor segment</param>
        /// <param name="coefficient2">Coefficient to apply to each value in the other tensor segment</param>
        public static void SubtractInPlace<T>(
            this INumericSegment<T> segment,
            IReadOnlyNumericSegment<T> other,
            T coefficient1,
            T coefficient2
        ) where T : unmanaged, INumber<T>
        {
            segment.ApplySpans(true, other, (x, y) => x.SubtractInPlace(y, coefficient1, coefficient2));
        }

        /// <summary>
        /// Multiplies each value by a scalar
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="scalar"></param>
        public static void SubtractInPlace<T>(
            this INumericSegment<T> segment,
            T scalar
        ) where T : unmanaged, INumber<T>
        {
            segment.ApplySpan(true, x => x.SubtractInPlace(scalar));
        }

        /// <summary>
        /// Creates a new tensor segment in which each value in this tensor segment is multiplied by the pairwise value from another tensor segment
        /// </summary>
        /// <param name="segment">This tensor segment</param>
        /// <param name="other">Other tensor segment</param>
        /// <returns></returns>
        public static INumericSegment<T> PointwiseMultiply<T>(
            this IReadOnlyNumericSegment<T> segment,
            IReadOnlyNumericSegment<T> other
        ) where T : unmanaged, INumber<T>
        {
            var ret = segment.ReduceReadOnlySpans(other, (x, y) => x.PointwiseMultiply(y));
            return new ArrayPoolTensorSegment<T>(ret);
        }

        /// <summary>
        /// Modifies this tensor segment so that each value is multiplied by the pairwise value from another tensor segment
        /// </summary>
        /// <param name="segment">This tensor segment</param>
        /// <param name="other">Other tensor segment</param>
        public static void PointwiseMultiplyInPlace<T>(
            this INumericSegment<T> segment,
            IReadOnlyNumericSegment<T> other
        ) where T : unmanaged, INumber<T>
        {
            segment.ApplySpans(true, other, (x, y) => x.PointwiseMultiplyInPlace(y));
        }

        /// <summary>
        /// Creates a new tensor segment in which each value in this tensor segment is divided by the pairwise value from another tensor segment
        /// </summary>
        /// <param name="segment">This tensor segment</param>
        /// <param name="other">Other tensor segment</param>
        /// <returns></returns>
        public static INumericSegment<T> PointwiseDivide<T>(
            this IReadOnlyNumericSegment<T> segment,
            IReadOnlyNumericSegment<T> other
        ) where T : unmanaged, INumber<T>
        {
            var ret = segment.ReduceReadOnlySpans(other, (x, y) => x.PointwiseDivide(y));
            return new ArrayPoolTensorSegment<T>(ret);
        }

        /// <summary>
        /// Modifies this tensor segment so that each value in this tensor segment is divided by the pairwise value from another tensor segment
        /// </summary>
        /// <param name="segment">This tensor segment</param>
        /// <param name="other">Other tensor segment</param>
        public static void PointwiseDivideInPlace<T>(
            this INumericSegment<T> segment,
            IReadOnlyNumericSegment<T> other
        ) where T : unmanaged, INumber<T>
        {
            segment.ApplySpans(true, other, (x, y) => x.PointwiseDivideInPlace(y));
        }

        /// <summary>
        /// Creates a new tensor segment that contains the square root of each value in this tensor segment
        /// </summary>
        /// <param name="segment">This tensor segment</param>
        /// <param name="adjustment">A small value to add to each value in case of zeros</param>
        /// <returns></returns>
        public static INumericSegment<T> Sqrt<T>(
            this IReadOnlyNumericSegment<T> segment,
            T adjustment
        ) where T : unmanaged, INumber<T>, IRootFunctions<T>
        {
            var ret = segment.ApplyReadOnlySpan(x => x.Sqrt(adjustment));
            return new ArrayPoolTensorSegment<T>(ret);
        }

        /// <summary>
        /// Modifies this tensor segment so that each value falls between the min and max values
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        public static void ConstrainInPlace<T>(
            this INumericSegment<T> segment,
            T? minValue,
            T? maxValue
        ) where T : unmanaged, INumber<T>
        {
            segment.ApplySpan(true, x => x.ConstrainInPlace(minValue, maxValue));
        }

        /// <summary>
        /// Finds the average value in this tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public static T Average<T>(this IReadOnlyNumericSegment<T> segment)
            where T : unmanaged, INumber<T>
        {
            return segment.ApplyReadOnlySpan(x => x.Average());
        }

        /// <summary>
        /// Calculates the L1 norm of this tensor segment
        /// </summary>
        /// <param name="segment">This tensor segment</param>
        /// <returns></returns>
        public static T L1Norm<T>(this IReadOnlyNumericSegment<T> segment)
            where T : unmanaged, INumber<T>
        {
            return segment.ApplyReadOnlySpan(x => x.L1Norm());
        }

        /// <summary>
        /// Calculates the L2 norm of this tensor segment
        /// </summary>
        /// <param name="segment">This tensor segment</param>
        /// <returns></returns>
        public static T L2Norm<T>(this IReadOnlyNumericSegment<T> segment) where T : unmanaged, INumber<T>, IRootFunctions<T>
        {
            return segment.ApplyReadOnlySpan(x => x.L2Norm());
        }

        /// <summary>
        /// Checks if each value in this tensor segment is finite (not NaN or Infinity)
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public static bool IsEntirelyFinite<T>(this IReadOnlyNumericSegment<T> segment) where T : unmanaged, INumber<T>
        {
            return segment.ApplyReadOnlySpan(x => x.IsEntirelyFinite());
        }

        /// <summary>
        /// Reverses the tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public static INumericSegment<T> Reverse<T>(this IReadOnlyNumericSegment<T> segment) where T : unmanaged, INumber<T>
        {
            var ret = segment.ApplyReadOnlySpan(x => x.Reverse());
            return new ArrayPoolTensorSegment<T>(ret);
        }

        /// <summary>
        /// Creates a new tensor segment that contains the absolute value of each value in this tensor segment
        /// </summary>
        /// <param name="segment">This tensor segment</param>
        /// <returns></returns>
        public static INumericSegment<T> Abs<T>(this IReadOnlyNumericSegment<T> segment) where T : unmanaged, INumber<T>
        {
            var ret = segment.ApplyReadOnlySpan(x => x.Abs());
            return new ArrayPoolTensorSegment<T>(ret);
        }

        /// <summary>
        /// Creates a new tensor segment that contains the natural logarithm of each value in this tensor segment
        /// </summary>
        /// <param name="segment">This tensor segment</param>
        /// <returns></returns>
        public static INumericSegment<T> Log<T>(this IReadOnlyNumericSegment<T> segment) where T : unmanaged, ILogarithmicFunctions<T>, INumber<T>
        {
            var ret = segment.ApplyReadOnlySpan(x => x.Log());
            return new ArrayPoolTensorSegment<T>(ret);
        }

        /// <summary>
        /// Creates a new tensor segment that contains the exponent of each value in this tensor segment
        /// </summary>
        /// <param name="segment">This tensor segment</param>
        /// <returns></returns>
        public static INumericSegment<T> Exp<T>(this IReadOnlyNumericSegment<T> segment) where T : unmanaged, IExponentialFunctions<T>, INumber<T>
        {
            var ret = segment.ApplyReadOnlySpan(x => x.Exp());
            return new ArrayPoolTensorSegment<T>(ret);
        }

        /// <summary>
        /// Creates a new tensor segment that contains each value raised by the specified power in this tensor segment
        /// </summary>
        /// <param name="segment">This tensor segment</param>
        /// <param name="power">Specified power</param>
        /// <returns></returns>
        public static INumericSegment<T> Pow<T>(this IReadOnlyNumericSegment<T> segment, T power) where T : unmanaged, IPowerFunctions<T>, INumber<T>
        {
            var ret = segment.ApplyReadOnlySpan(x => x.Pow(power));
            return new ArrayPoolTensorSegment<T>(ret);
        }

        /// <summary>
        /// Creates a new tensor segment in which each value in this tensor segment is squared
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public static INumericSegment<T> Squared<T>(this IReadOnlyNumericSegment<T> segment) where T : unmanaged, INumber<T>
        {
            var ret = segment.ApplyReadOnlySpan(x => x.Squared());
            return new ArrayPoolTensorSegment<T>(ret);
        }

        /// <summary>
        /// Calculates the standard deviation of this tensor segment
        /// </summary>
        /// <param name="segment">This tensor segment</param>
        /// <param name="mean">Mean of the tensor segment (optional)</param>
        /// <returns></returns>
        public static T StdDev<T>(this IReadOnlyNumericSegment<T> segment, T? mean) where T : unmanaged, INumber<T>, IRootFunctions<T>
        {
            return segment.ApplyReadOnlySpan(x => x.StdDev(mean));
        }

        /// <summary>
        /// Creates a new tensor segment with sigmoid function applied to each value in this tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public static INumericSegment<T> Sigmoid<T>(this IReadOnlyNumericSegment<T> segment)
            where T : unmanaged, ISignedNumber<T>, INumber<T>, IExponentialFunctions<T> => MapParallel(segment, x => Sigmoid(x));

        /// <summary>
        /// Creates a new tensor segment with sigmoid derivative applied to each value in this tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public static INumericSegment<T> SigmoidDerivative<T>(this IReadOnlyNumericSegment<T> segment)
            where T : unmanaged, ISignedNumber<T>, INumber<T>, IExponentialFunctions<T> => MapParallel(segment, x => SigmoidDerivative(x));

        /// <summary>
        /// Creates a new tensor segment with tanh function applied to each value in this tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public static INumericSegment<T> Tanh<T>(this IReadOnlyNumericSegment<T> segment)
            where T : unmanaged, INumber<T>, IHyperbolicFunctions<T> => MapParallel(segment, T.Tanh);

        /// <summary>
        /// Creates a new tensor segment with tanh derivative applied to each value in this tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public static INumericSegment<T> TanhDerivative<T>(this IReadOnlyNumericSegment<T> segment) where T : unmanaged, INumber<T>, IPowerFunctions<T>, IHyperbolicFunctions<T> => MapParallel(segment, ExtensionMethods.TanhDerivative);

        /// <summary>
        /// Creates a new tensor segment with RELU function applied to each value in this tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public static INumericSegment<T> Relu<T>(this IReadOnlyNumericSegment<T> segment) where T : unmanaged, INumber<T> => MapParallel(segment, ExtensionMethods.Relu);

        /// <summary>
        /// Creates a new tensor segment with RELU derivative applied to each value in this tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public static INumericSegment<T> ReluDerivative<T>(this IReadOnlyNumericSegment<T> segment) where T : unmanaged, INumber<T> => MapParallel(segment, ExtensionMethods.ReluDerivative);

        /// <summary>
        /// Creates a new tensor segment with Leaky RELU function applied to each value in this tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public static INumericSegment<T> LeakyRelu<T>(this IReadOnlyNumericSegment<T> segment) where T : unmanaged, INumber<T> => MapParallel(segment, ExtensionMethods.LeakyRelu);

        /// <summary>
        /// Creates a new tensor segment with Leaky RELU derivative applied to each value in this tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public static INumericSegment<T> LeakyReluDerivative<T>(this IReadOnlyNumericSegment<T> segment) where T : unmanaged, INumber<T> => MapParallel(segment, ExtensionMethods.LeakyReluDerivative);

        /// <summary>
        /// Creates a new tensor segment with softmax function applied to each value in this tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public static INumericSegment<T> Softmax<T>(this IReadOnlyNumericSegment<T> segment)
            where T : unmanaged, INumber<T>, IMinMaxValue<T>, IExponentialFunctions<T>
        {
            var ret = segment.ApplyReadOnlySpan(x => x.Softmax());
            return new ArrayPoolTensorSegment<T>(ret);
        }

        /// <summary>
        /// Creates a new tensor segment with softmax derivative applied to each value in this tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="rowCount"></param>
        /// <returns></returns>
        public static INumericSegment<T> SoftmaxDerivative<T>(this IReadOnlyNumericSegment<T> segment, int rowCount)
            where T : unmanaged, INumber<T>
        {
            var ret = segment.ApplyReadOnlySpan(x => x.SoftmaxDerivative(rowCount));
            return new ArrayPoolTensorSegment<T>(ret);
        }

        /// <summary>
        /// Rounds each value in this tensor segment to be either the lower or upper supplied parameters
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="lower"></param>
        /// <param name="upper"></param>
        public static void RoundInPlace<T>(this INumericSegment<T> segment, T lower, T upper)
            where T : unmanaged, INumber<T>
        {
            segment.ApplySpan(true, x => x.RoundInPlace(lower, upper));
        }

        /// <summary>
        /// In place L1 regularization of the tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="coefficient">Coefficient to apply to each adjusted value</param>
        public static void L1Regularization<T>(this INumericSegment<T> segment, T coefficient)
            where T : unmanaged, INumber<T>
        {
            segment.ApplySpan(true, x => x.L1Regularization(coefficient));
        }

        /// <summary>
        /// Angular distance between two vectors
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static T AngularDistance<T>(this IReadOnlyNumericSegment<T> segment, IReadOnlyNumericSegment<T> other)
            where T : unmanaged, IBinaryFloatingPointIeee754<T>
        {
            return segment.ReduceReadOnlySpans(other, (x, y) => x.AngularDistance(y));
        }

        /// <summary>
        /// Inner space distance between two vectors
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static T InnerProductSpaceDistance<T>(this IReadOnlyNumericSegment<T> segment, IReadOnlyNumericSegment<T> other)
            where T : unmanaged, IBinaryFloatingPointIeee754<T>
        {
            return segment.ReduceReadOnlySpans(other, (x, y) => x.InnerProductSpaceDistance(y));
        }

        /// <summary>
        /// Find the minimum value and index in a tensor segment
        /// </summary>
        /// <param name="segment">Tensor segment to analyze</param>
        /// <returns>Tuple containing the minimum value and its index</returns>
        public static (T Value, uint Index) Minimum<T>(this IReadOnlyNumericSegment<T> segment)
            where T : unmanaged, INumber<T>, IMinMaxValue<T>
        {
            return segment.ApplyReadOnlySpan(x => x.Minimum());
        }

        /// <summary>
        /// Returns the index of the minimum value within a tensor segment
        /// </summary>
        /// <param name="segment">Tensor segment to analyse</param>
        /// <returns></returns>
        public static uint MinimumIndex<T>(this IReadOnlyNumericSegment<T> segment)
            where T : unmanaged, INumber<T>, IMinMaxValue<T> => Minimum(segment).Index;

        /// <summary>
        /// Returns the minimum value
        /// </summary>
        /// <param name="segment">Tensor segment to analyse</param>
        /// <returns></returns>
        public static T MinimumValue<T>(this IReadOnlyNumericSegment<T> segment)
            where T : unmanaged, INumber<T>, IMinMaxValue<T> => Minimum(segment).Value;

        /// <summary>
        /// Returns the maximum value and index within a tensor segment
        /// </summary>
        /// <param name="segment">Tensor segment to analyse</param>
        /// <returns>Tuple containing the maximum value and its index</returns>
        public static (T Value, uint Index) Maximum<T>(this IReadOnlyNumericSegment<T> segment)
            where T : unmanaged, INumber<T>, IMinMaxValue<T>
        {
            return segment.ApplyReadOnlySpan(x => x.Maximum());
        }

        /// <summary>
        /// Returns the maximum value within a tensor segment
        /// </summary>
        /// <param name="segment">Tensor segment to analyse</param>
        /// <returns></returns>
        public static uint MaximumIndex<T>(this IReadOnlyNumericSegment<T> segment)
            where T : unmanaged, INumber<T>, IMinMaxValue<T> => Maximum(segment).Index;

        /// <summary>
        /// Returns the index of the maximum value within a tensor segment
        /// </summary>
        /// <param name="segment">Tensor segment to analyse</param>
        /// <returns></returns>
        public static T MaximumValue<T>(this IReadOnlyNumericSegment<T> segment)
            where T : unmanaged, INumber<T>, IMinMaxValue<T> => Maximum(segment).Value;

        /// <summary>
        /// Calculates the pearson correlation coefficient metric between two tensor segments
        /// https://en.wikipedia.org/wiki/Pearson_correlation_coefficient
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static T? PearsonCorrelationCoefficient<T>(this IReadOnlyNumericSegment<T> x, IReadOnlyNumericSegment<T> y)
            where T : unmanaged, INumber<T>, IRootFunctions<T>
        {
            return x.ReduceReadOnlySpans(y, (a, b) => a.PearsonCorrelationCoefficient(b));
        }

        /// <summary>
        /// Searches the tensor segment for the index of the first value that matches the specified value within a level of tolerance
        /// </summary>
        /// <param name="segment">This tensor segment</param>
        /// <param name="value">Value to find</param>
        /// <param name="tolerance">Degree of tolerance</param>
        /// <returns></returns>
        public static IReadOnlyCollection<uint> Search<T>(this IReadOnlyNumericSegment<T> segment, T value, T? tolerance = null)
            where T : unmanaged, INumber<T>
        {
            return segment.ApplyReadOnlySpan(x => x.Search(value, tolerance));
        }

        /// <summary>
        /// Creates a new tensor segment from the current tensor segment divided by its L2 norm
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="segment"></param>
        /// <returns></returns>
        public static INumericSegment<T> EuclideanNormalize<T>(this IReadOnlyNumericSegment<T> segment)
            where T : unmanaged, INumber<T>, IRootFunctions<T>
        {
            var ret = segment.ApplyReadOnlySpan(x => x.EuclideanNormalize());
            return new ArrayPoolTensorSegment<T>(ret);
        }

        /// <summary>
        /// Creates a new tensor segment from the current tensor segment divided by its L1 norm
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="segment"></param>
        /// <returns></returns>
        public static INumericSegment<T> ManhattanNormalize<T>(this IReadOnlyNumericSegment<T> segment)
            where T : unmanaged, INumber<T>
        {
            var ret = segment.ApplyReadOnlySpan(x => x.ManhattanNormalize());
            return new ArrayPoolTensorSegment<T>(ret);
        }
    }
}