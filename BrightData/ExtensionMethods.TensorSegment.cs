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
        public static ITensorSegment ToSegment(this MemoryOwner<float> memoryOwner) => new ArrayPoolTensorSegment(memoryOwner);

        readonly struct ZipAction : IAction
        {
            readonly IReadOnlyTensorSegment _segment;
            readonly IReadOnlyTensorSegment _other;
            readonly Func<float, float, float> _action;
            readonly float[] _ret;

            public ZipAction(IReadOnlyTensorSegment segment, IReadOnlyTensorSegment other, Func<float, float, float> action, float[] ret)
            {
                _segment = segment;
                _other = other;
                _action = action;
                _ret = ret;
            }

            public void Invoke(int i) => _ret[i] = _action(_segment[i], _other[i]);
        }
        readonly struct TransformAction : IAction
        {
            readonly IReadOnlyTensorSegment _segment;
            readonly Func<float, float> _action;
            readonly float[] _ret;

            public TransformAction(IReadOnlyTensorSegment segment, Func<float, float> action, float[] ret)
            {
                _segment = segment;
                _action = action;
                _ret = ret;
            }

            public void Invoke(int i) => _ret[i] = _action(_segment[i]);
        }
        readonly struct TransformIndexedAction : IAction
        {
            readonly Func<uint, float> _action;
            readonly float[] _ret;

            public TransformIndexedAction(Func<uint, float> action, float[] ret)
            {
                _action = action;
                _ret = ret;
            }

            public void Invoke(int i) => _ret[i] = _action((uint)i);
        }
        readonly struct MutateAction : IAction
        {
            readonly Func<float, float> _action;
            readonly ITensorSegment _segment;

            public MutateAction(Func<float, float> action, ITensorSegment segment)
            {
                _action = action;
                _segment = segment;
            }

            public void Invoke(int i) => _segment[i] = _action(_segment[i]);
        }

        /// <summary>
        /// Returns an array from a tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static float[] GetLocalOrNewArray(this ITensorSegment segment) => segment.GetArrayIfEasilyAvailable() ?? segment.ToNewArray();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static MemoryOwner<float> Allocate(uint size) => MemoryOwner<float>.Allocate((int)size);

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
        /// Creates a new tensor segment from this tensor segment (in parallel)
        /// </summary>
        /// <param name="segment">This tensor</param>
        /// <param name="transformer">Value transformer</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ITensorSegment TransformParallel(this IReadOnlyTensorSegment segment, Func<float, float> transformer)
        {
            var size = segment.Size;
            var ret = Allocate(size);
            var array = ret.DangerousGetArray().Array!;

            if (size >= Consts.MinimumSizeForParallel)
                ParallelHelper.For(0, (int)size, new TransformAction(segment, transformer, array));
            else {
                for (uint i = 0; i < size; i++)
                    array[(int)i] = transformer(segment[i]);
            }

            return new ArrayPoolTensorSegment(ret);
        }

        /// <summary>
        /// Creates a new tensor segment from this tensor segment (vectorized)
        /// </summary>
        /// <param name="segment">This segment</param>
        /// <param name="transformer1">Vectorized value transformer</param>
        /// <param name="transformer2">Value transformer</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ITensorSegment TransformVectorized(
            this IReadOnlyTensorSegment segment,
            ComputeVectorisedOne<float> transformer1,
            Func<float, float> transformer2)
        {
            var size = segment.Size;
            MemoryOwner<float> ret;

            if (size >= Consts.MinimumSizeForVectorised) {
                var leftTemp = SpanOwner<float>.Empty;
                var leftPtr = segment.GetSpan(ref leftTemp, out var wasLeftTempUsed);
                try {
                    ret = leftPtr.TransformVectorized(transformer1, transformer2);
                }
                finally {
                    if (wasLeftTempUsed)
                        leftTemp.Dispose();
                }
            }
            else {
                ret = Allocate(segment.Size);
                var resultPtr = ret.Span;
                for (var i = 0; i < size; i++)
                    resultPtr[i] = transformer2(segment[i]);
            }

            return new ArrayPoolTensorSegment(ret);
        }

        
        /// <summary>
        /// Creates a new tensor segment from this tensor segment (in parallel)
        /// </summary>
        /// <param name="segment">This tensor</param>
        /// <param name="transformer">Indexed transformer</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static ITensorSegment TransformParallelIndexed(this IReadOnlyTensorSegment segment, Func<uint, float> transformer)
        {
            var size = segment.Size;
            var ret = Allocate(size);
            var array = ret.DangerousGetArray().Array!;

            if (size >= Consts.MinimumSizeForParallel)
                ParallelHelper.For(0, (int)size, new TransformIndexedAction(transformer, array));
            else {
                for (uint i = 0; i < size; i++)
                    array[(int)i] = transformer(i);
            }

            return new ArrayPoolTensorSegment(ret);
        }

        /// <summary>
        /// In place update of this tensor segment with pairwise values from another tensor segment (in parallel)
        /// </summary>
        /// <param name="segment">This tensor</param>
        /// <param name="other">Other tensor</param>
        /// <param name="func">Update function</param>
        /// <exception cref="ArgumentException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MutateParallel(this ITensorSegment segment, ITensorSegment other, Func<float, float, float> func)
        {
            var size = segment.Size;
            if (size != other.Size)
                throw new ArgumentException("Segments were different sizes");

            if (size >= Consts.MinimumSizeForParallel)
                Parallel.For(0, size, i => segment[i] = func(segment[i], other[i]));
            else {
                for (uint i = 0; i < size; i++)
                    segment[i] = func(segment[i], other[i]);
            }
        }

        /// <summary>
        /// In place update of this tensor segment with pairwise values from another tensor segment (vectorized)
        /// </summary>
        /// <param name="segment">This tensor</param>
        /// <param name="other">Other tensor</param>
        /// <param name="func1">Vectorized update function</param>
        /// <param name="func2">Update function</param>
        /// <exception cref="ArgumentException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void MutateVectorized(
            this ITensorSegment segment,
            IReadOnlyTensorSegment other,
            ComputeVectorisedTwo<float> func1,
            Func<float, float, float> func2)
        {
            var size = segment.Size;
            if (size != other.Size)
                throw new ArgumentException("Segments were different sizes");

            if (size >= Consts.MinimumSizeForVectorised) {
                // get pointers to the segments
                SpanOwner<float> leftTemp = SpanOwner<float>.Empty, rightTemp = SpanOwner<float>.Empty;
                var leftPtr = segment.GetSpan(ref leftTemp, out var wasLeftTempUsed);
                var rightPtr = other.GetSpan(ref rightTemp, out var wasRightTempUsed);
                try {
                    fixed (float* fp = &MemoryMarshal.GetReference(leftPtr)) {
                        var leftMutablePtr = new Span<float>(fp, (int)size);
                        leftMutablePtr.MutateVectorized(rightPtr, func1, func2);
                    }
                }
                finally {
                    if (wasLeftTempUsed)
                        leftTemp.Dispose();
                    if (wasRightTempUsed)
                        rightTemp.Dispose();
                }
            }
            else {
                for (var i = 0; i < size; i++)
                    segment[i] = func2(segment[i], other[i]);
            }
        }

        /// <summary>
        /// In place update of tensor segment (in parallel)
        /// </summary>
        /// <param name="segment">This tensor</param>
        /// <param name="mutator">Update function</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MutateInPlaceParallel(this ITensorSegment segment, Func<float, float> mutator)
        {
            var size = segment.Size;
            if (size >= Consts.MinimumSizeForParallel)
                ParallelHelper.For(0, (int)size, new MutateAction(mutator, segment));
            else {
                for (uint i = 0; i < size; i++)
                    segment[i] = mutator(segment[i]);
            }
        }

        /// <summary>
        /// In place update of tensor segment (vectorized)
        /// </summary>
        /// <param name="segment">This tensor</param>
        /// <param name="mutator1">Vectorized update function</param>
        /// <param name="mutator2">Update function</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void MutateInPlaceVectorized(
            this ITensorSegment segment,
            ComputeVectorisedOne<float> mutator1,
            Func<float, float> mutator2)
        {
            var size = segment.Size;
            if (size >= Consts.MinimumSizeForVectorised) {
                var leftTemp = SpanOwner<float>.Empty;
                var leftPtr = segment.GetSpan(ref leftTemp, out var wasLeftTempUsed);
                try {
                    fixed (float* fp = &MemoryMarshal.GetReference(leftPtr)) {
                        var leftMutablePtr = new Span<float>(fp, (int)size);
                        leftMutablePtr.MutateInPlaceVectorized(mutator1, mutator2);
                    }
                }
                finally {
                    if (wasLeftTempUsed)
                        leftTemp.Dispose();
                }
            }
            else {
                for (var i = 0; i < size; i++)
                    segment[i] = mutator2(segment[i]);
            }
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
                var span = segment.GetSpan(ref temp, out var wasTempUsed);
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
        /// Adds another tensor segment in place to this tensor segment
        /// </summary>
        /// <param name="target">This tensor</param>
        /// <param name="other">Other tensor</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddInPlace(this ITensorSegment target, IReadOnlyTensorSegment other) => MutateVectorized(
            target,
            other,
            (in Vector<float> a, in Vector<float> b, out Vector<float> r) => r = a + b,
            (a, b) => a + b
        );

        /// <summary>
        /// Multiplies each value in this tensor segment by a scalar (in place)
        /// </summary>
        /// <param name="target">This tensor</param>
        /// <param name="scalar">Scalar to multiply</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MultiplyInPlace(this ITensorSegment target, float scalar)
        {
            var scalarVector = new Vector<float>(scalar);
            MutateInPlaceVectorized(
                target,
                (in Vector<float> a, out Vector<float> r) => r = a * scalarVector,
                a => a * scalar
            );
        }

        /// <summary>
        /// Calculates the dot product of this with another tensor
        /// </summary>
        /// <param name="segment">This tensor</param>
        /// <param name="other">Other tensor</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DotProduct(this IReadOnlyTensorSegment segment, IReadOnlyTensorSegment other)
        {
            var size = segment.Size;
            if (size != other.Size)
                throw new ArgumentException("Segments were different sizes");
            if (size >= Consts.MinimumSizeForVectorised) {
                SpanOwner<float> leftTemp = SpanOwner<float>.Empty, rightTemp = SpanOwner<float>.Empty;
                var leftPtr = segment.GetSpan(ref leftTemp, out var wasLefTempUsed);
                var rightPtr = other.GetSpan(ref rightTemp, out var wasRightTempUsed);
                try {
                    return leftPtr.DotProduct(rightPtr);
                }
                finally {
                    if (wasLefTempUsed)
                        leftTemp.Dispose();
                    if (wasRightTempUsed)
                        rightTemp.Dispose();
                }
            }

            var ret = 0f;
            for (uint i = 0; i < size; i++)
                ret += segment[i] * other[i];
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
        /// Finds the average value in this tensor segment
        /// </summary>
        /// <param name="segment">This tensor</param>
        /// <returns></returns>
        public static float Average(this IReadOnlyTensorSegment segment) => Sum(segment) / segment.Size;

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
        /// Creates a new tensor segment that contains each value squared in this tensor segment
        /// </summary>
        /// <param name="tensor">This tensor</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ITensorSegment Squared(this IReadOnlyTensorSegment tensor) => TransformVectorized(
            tensor,
            (in Vector<float> a, out Vector<float> r) => r = a * a,
            a => a * a
        );

        /// <summary>
        /// Creates a new tensor segment with softmax function applied to each value in this tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ITensorSegment Softmax(this IReadOnlyTensorSegment segment)
        {
            var (_, max, _, _) = GetMinAndMaxValues(segment);
            var softmax = segment.TransformParallel(v => MathF.Exp(v - max));
            var sum = Sum(softmax);
            if (FloatMath.IsNotZero(sum))
                softmax.MultiplyInPlace(1f / sum);
            return softmax;
        }

        /// <summary>
        /// Creates a new tensor segment with softmax derivative applied to each value in this tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="lap"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IMatrix SoftmaxDerivative(this IReadOnlyTensorSegment segment, LinearAlgebraProvider lap)
        {
            return lap.CreateMatrix(segment.Size, segment.Size, (x, y) => {
                var xVal = segment[x];
                return x == y
                    ? xVal * (1 - xVal)
                    : -xVal * segment[y];
            });
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
        /// In place L1 regularization of the tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="coefficient">Coefficient to apply to each adjusted value</param>
        public static void L1Regularization(this ITensorSegment segment, float coefficient)
        {
            for (uint i = 0, len = segment.Size; i < len; i++) {
                var val = segment[i];
                segment[i] = val - (val > 0 ? 1 : val < 0 ? -1 : 0) * coefficient;
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
        public static RT GetReadOnlySpan<RT>(this IReadOnlyTensorSegment segment, OnReadOnlyFloatSpan<RT> callback)
        {
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

        public delegate RT OnReadOnlyFloatSpans<out RT>(ReadOnlySpan<float> span1, ReadOnlySpan<float> span2);
        public static RT GetReadOnlySpans<RT>(this ITensorSegment segment1, ITensorSegment segment2, OnReadOnlyFloatSpans<RT> callback)
        {
            var array1 = segment1.GetArrayIfEasilyAvailable();
            var array2 = segment2.GetArrayIfEasilyAvailable();
            if (array1 is not null && array2 is not null)
                return callback(new Span<float>(array1, 0, (int)segment1.Size), new Span<float>(array2, 0, (int)segment2.Size));
            else {
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
        }

        public delegate void OnFloatSpans(Span<float> span1, ReadOnlySpan<float> span2);
        public static unsafe void GetSpan(this ITensorSegment segment1, ITensorSegment segment2, OnFloatSpans callback)
        {
            if (segment1.IsWrapper)
                throw new ArgumentException($"Tensor segment wrappers cannot be modified");
            var array1 = segment1.GetArrayIfEasilyAvailable();
            var array2 = segment2.GetArrayIfEasilyAvailable();
            if (array1 is not null && array2 is not null)
                callback(new Span<float>(array1, 0, (int)segment1.Size), new Span<float>(array2, 0, (int)segment2.Size));
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

        public delegate void OnFloatSpan(Span<float> span);
        public static unsafe void ApplySpan(this ITensorSegment segment, OnFloatSpan callback)
        {
            if (segment.IsWrapper)
                throw new ArgumentException($"Tensor segment wrappers cannot be modified");
            var array = segment.GetArrayIfEasilyAvailable();
            if (array is not null)
                callback(new Span<float>(array, 0, (int)segment.Size));
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
    }
}
