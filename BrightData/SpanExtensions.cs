using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData
{
    /// <summary>
    /// Extensions that work with a span of numbers
    /// </summary>
    public static class SpanExtensions
    {
        /// <summary>
        /// Callback to calculate a new vector of Ts from two existing vectors
        /// </summary>
        /// <param name="a">First input vector</param>
        /// <param name="b">Second input vector</param>
        /// <param name="r">Result (output) vector</param>
        public delegate void ComputeVectorisedTwo<T>(in Vector<T> a, in Vector<T> b, out Vector<T> r) where T: unmanaged, INumber<T>;

        /// <summary>
        /// Callback to calculate a new vector of Ts from an existing vector
        /// </summary>
        /// <param name="a">Input vector</param>
        /// <param name="r">Result (output) vector</param>
        public delegate void ComputeVectorisedOne<T>(in Vector<T> a, out Vector<T> r) where T: unmanaged, INumber<T>;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]static MemoryOwner<T> Allocate<T>(int size) where T: unmanaged, INumber<T> => MemoryOwner<T>.Allocate(size);
        
        /// <summary>
        /// Creates a new span of numbers from applying an operation to each pair of elements from this and another span
        /// </summary>
        /// <param name="span">This span</param>
        /// <param name="other">Other span</param>
        /// <param name="func">Function that computes a new value from a pair of values</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static unsafe MemoryOwner<T> ZipParallel<T>(
            this ReadOnlySpan<T> span, 
            ReadOnlySpan<T> other, 
            Func<T, T, T> func
        ) where T: unmanaged, INumber<T> {
            var size = span.Length;
            if (size != other.Length)
                throw new ArgumentException("Spans were different sizes");

            var ret = Allocate<T>(size);
            var array = ret.DangerousGetArray().Array!;
            fixed (T* xfp = &MemoryMarshal.GetReference(span))
            fixed (T* yfp = &MemoryMarshal.GetReference(other))
            fixed (T* zfp = &array[0]){
                var xp = xfp;
                var yp = yfp;
                var zp = zfp;
                if (size >= Consts.MinimumSizeForParallel)
                    Parallel.For(0, size, i => zp[i] = func(xp[i], yp[i]));
                else {
                    for (var i = 0; i < size; i++)
                        *zp++ = func(*xp++, *yp++);
                }
            }

            return ret;
        }

        /// <summary>
        /// Applies a function across each pair of elements from this and another span
        /// </summary>
        /// <param name="span">This span</param>
        /// <param name="other">Other span</param>
        /// <param name="func1">Vector callback</param>
        /// <param name="func2">Element callback</param>
        /// <returns>Memory buffer that holds results from each callback</returns>
        /// <exception cref="ArgumentException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static MemoryOwner<T> ZipVectorised<T>(
            this ReadOnlySpan<T> span, 
            ReadOnlySpan<T> other, 
            ComputeVectorisedTwo<T> func1,
            Func<T, T, T> func2
        ) where T: unmanaged, INumber<T>
        {
            var size = span.Length;
            if (size != other.Length)
                throw new ArgumentException("Segments were different sizes");

            var ret = Allocate<T>(size);
            var resultPtr = ret.Span;
            var nextIndex = 0;
            if (size >= Consts.MinimumSizeForVectorised) {
                var vectorSize = Vector<T>.Count;
                var leftVec = MemoryMarshal.Cast<T, Vector<T>>(span);
                var rightVec = MemoryMarshal.Cast<T, Vector<T>>(other);
                var resultVec = MemoryMarshal.Cast<T, Vector<T>>(resultPtr);
                var numVectors = size / vectorSize;
                nextIndex = numVectors * vectorSize;
                for (var j = 0; j < numVectors; j++)
                    func1(leftVec[j], rightVec[j], out resultVec[j]);
            }
            for (; nextIndex < size; nextIndex++)
                resultPtr[nextIndex] = func2(span[nextIndex], other[nextIndex]);
            return ret;
        }

        /// <summary>
        /// Applies a callback to each item in the span
        /// </summary>
        /// <param name="span">Vector</param>
        /// <param name="transformer">Callback</param>
        /// <returns>Memory buffer that holds results from each callback</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static unsafe MemoryOwner<T> TransformParallel<T>(
            this ReadOnlySpan<T> span, 
            Func<T, T> transformer
        ) where T: unmanaged, INumber<T>
        {
            var size = span.Length;
            var ret = Allocate<T>(size);
            var array = ret.DangerousGetArray().Array!;

            fixed (T* xfp = &MemoryMarshal.GetReference(span))
            fixed (T* zfp = &array[0]) {
                var xp = xfp;
                var zp = zfp;
                if (size >= Consts.MinimumSizeForParallel)
                    Parallel.For(0, size, i => array[i] = transformer(xp[i]));
                else {
                    for (uint i = 0; i < size; i++)
                        *zp++ = transformer(*xp++);
                }
            }

            return ret;
        }

        /// <summary>
        /// Creates a new span from an existing span via a vectorization function
        /// </summary>
        /// <param name="span">Input buffer</param>
        /// <param name="transformer1">Vectorized transformer</param>
        /// <param name="transformer2">Sequential transformer</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static MemoryOwner<T> TransformVectorised<T>(
            this ReadOnlySpan<T> span, 
            ComputeVectorisedOne<T> transformer1, 
            Func<T, T> transformer2
        ) where T: unmanaged, INumber<T>
        {
            var size = span.Length;
            var ret = Allocate<T>(size);
            var resultPtr = ret.Span;

            var nextIndex = 0;
            if (size >= Consts.MinimumSizeForVectorised) {
                var vectorSize = Vector<T>.Count;
                var leftVec = MemoryMarshal.Cast<T, Vector<T>>(span);
                var resultVec = MemoryMarshal.Cast<T, Vector<T>>(resultPtr);
                var numVectors = size / vectorSize;
                nextIndex = numVectors * vectorSize;
                for (var j = 0; j < numVectors; j++)
                    transformer1(leftVec[j], out resultVec[j]);
            }
            for (; nextIndex < size; nextIndex++)
                resultPtr[nextIndex] = transformer2(span[nextIndex]);

            return ret;
        }

        /// <summary>
        /// Creates a new span from an existing span via a function (possibly executed in parallel) that receives an index and returns a new value
        /// </summary>
        /// <param name="span">Input span</param>
        /// <param name="transformer">Transformation function (possibly executed in parallel)</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static MemoryOwner<T> TransformParallelIndexed<T>(
            this ReadOnlySpan<T> span, 
            Func<uint, T> transformer
        ) where T: unmanaged, INumber<T>
        {
            var size = span.Length;
            var ret = Allocate<T>(size);
            var array = ret.DangerousGetArray().Array!;

            if(size >= Consts.MinimumSizeForParallel)
                Parallel.For(0, size, i => array[i] = transformer((uint)i));
            else {
                for (uint i = 0; i < size; i++)
                    array[i] = transformer(i);
            }
            return ret;
        }

        /// <summary>
        /// Updates a buffer by applying an update function that receives pairs of values from this and another span
        /// </summary>
        /// <param name="span">This span</param>
        /// <param name="other">Other span</param>
        /// <param name="func">Update function</param>
        /// <exception cref="ArgumentException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static unsafe void Mutate<T>(
            this Span<T> span, 
            ReadOnlySpan<T> other, 
            Func<T, T, T> func
        ) where T: unmanaged, INumber<T>
        {
            var size = span.Length;
            if (size != other.Length)
                throw new ArgumentException("Spans were different sizes");

            fixed (T* xfp = &MemoryMarshal.GetReference(span))
            fixed (T* yfp = &MemoryMarshal.GetReference(other)) {
                var xp = xfp;
                var yp = yfp;
                if (size >= Consts.MinimumSizeForParallel)
                    Parallel.For(0, size, i => xp[i] = func(xp[i], yp[i]));
                else {
                    for (var i = 0; i < size; i++) {
                        *xp = func(*xp, *yp++);
                        ++xp;
                    }
                }
            }
        }

        /// <summary>
        /// Updates a buffer by applying a vectorized transformation function to each pair of elements in this and another span
        /// </summary>
        /// <param name="span">This span</param>
        /// <param name="other">Other span</param>
        /// <param name="transformer1">Vectorized transformer</param>
        /// <param name="transformer2">Sequential transformer</param>
        /// <exception cref="ArgumentException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static void MutateVectorised<T>(
            this Span<T> span, 
            ReadOnlySpan<T> other, 
            ComputeVectorisedTwo<T> transformer1, 
            Func<T, T, T> transformer2
        )where T: unmanaged, INumber<T>
        {
            var size = span.Length;
            if (size != other.Length)
                throw new ArgumentException("Spans were different sizes");

            var nextIndex = 0;
            if (size >= Consts.MinimumSizeForVectorised) {
                var vectorSize = Vector<T>.Count;
                var leftVec = MemoryMarshal.Cast<T, Vector<T>>(span);
                var rightVec = MemoryMarshal.Cast<T, Vector<T>>(other);
                var numVectors = size / vectorSize;
                nextIndex = numVectors * vectorSize;

                for (var i = 0; i < numVectors; i++) {
                    transformer1(leftVec[i], rightVec[i], out var temp);
                    leftVec[i] = temp;
                }
            }
            for (; nextIndex < size; nextIndex++)
                span[nextIndex] = transformer2(span[nextIndex], other[nextIndex]);
        }

        /// <summary>
        /// Updates a span in place by applying a mutation function (potentially called in parallel) to each element
        /// </summary>
        /// <param name="span"></param>
        /// <param name="mutator"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static unsafe void MutateInPlace<T>(
            this Span<T> span, 
            Func<T, T> mutator
        ) where T: unmanaged, INumber<T>
        {
            var size = span.Length;
            fixed (T* xfp = &MemoryMarshal.GetReference(span)) {
                var xp = xfp;
                if (size >= Consts.MinimumSizeForParallel)
                    Parallel.For(0, size, i => xp[i] = mutator(xp[i]));
                else {
                    for (uint i = 0; i < size; i++) {
                        *xp = mutator(*xp);
                        ++xp;
                    }
                }
            }
        }

        /// <summary>
        /// Updates a span in place by applying a vectorization function to each value
        /// </summary>
        /// <param name="span"></param>
        /// <param name="mutator1"></param>
        /// <param name="mutator2"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static void MutateInPlaceVectorised<T>(
            this Span<T> span, 
            ComputeVectorisedOne<T> mutator1, 
            Func<T, T> mutator2
        ) where T: unmanaged, INumber<T>
        {
            var size = span.Length;
            var nextIndex = 0;
            if (size >= Consts.MinimumSizeForVectorised) {
                var vectorSize = Vector<T>.Count;
                var leftVec = MemoryMarshal.Cast<T, Vector<T>>(span);
                var numVectors = size / vectorSize;
                nextIndex = numVectors * vectorSize;
                for (var i = 0; i < numVectors; i++) {
                    mutator1(leftVec[i], out var temp);
                    leftVec[i] = temp;
                }
            }
            for (; nextIndex < size; nextIndex++)
                span[nextIndex] = mutator2(span[nextIndex]);
        }

        /// <summary>
        /// Calculates the sum of all values in this span
        /// </summary>
        /// <param name="span"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static T Sum<T>(this Span<T> span) where T: unmanaged, INumber<T> => Sum((ReadOnlySpan<T>)span);

        /// <summary>
        /// Calculates the sum of all values in this span
        /// </summary>
        /// <param name="span"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static T Sum<T>(this ReadOnlySpan<T> span) where T: unmanaged, INumber<T>
        {
            var size = span.Length;
            var nextIndex = 0;
            var ret = default(T);

            if (size >= Consts.MinimumSizeForVectorised) {
                var vectorSize = Vector<T>.Count;
                var leftVec = MemoryMarshal.Cast<T, Vector<T>>(span);
                var numVectors = size / vectorSize;
                nextIndex = numVectors * vectorSize;
                for (var i = 0; i < numVectors; i++)
                    ret += Vector.Sum(leftVec[i]);
            }
            for (; nextIndex < size; nextIndex++)
                ret += span[nextIndex];
            return ret;
        }

        /// <summary>
        /// Returns a new buffer that contains this span added to another span
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span">This span</param>
        /// <param name="other">Other span</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static MemoryOwner<T> Add<T>(
            this ReadOnlySpan<T> span, 
            ReadOnlySpan<T> other
        ) where T: unmanaged, INumber<T> => ZipVectorised(
            span, 
            other, 
            (in Vector<T> a, in Vector<T> b, out Vector<T> r) => r = a + b, 
            (a, b) => a + b
        );

        /// <summary>
        /// Returns a new buffer that contains this span added to another span where each value is multiplied by coefficients
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span">This span</param>
        /// <param name="other">Other span</param>
        /// <param name="coefficient1">Coefficient to apply to each value in this span</param>
        /// <param name="coefficient2">Coefficient to apply to each value in the other span</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static MemoryOwner<T> Add<T>(
            this ReadOnlySpan<T> span,
            ReadOnlySpan<T> other, 
            T coefficient1, 
            T coefficient2
        ) where T: unmanaged, INumber<T> => ZipVectorised(
            span, 
            other, 
            (in Vector<T> a, in Vector<T> b, out Vector<T> r) => r = a * coefficient1 + b * coefficient2,
            (a, b) => a * coefficient1 + b * coefficient2
        );

        /// <summary>
        /// Returns a new buffer that contains each value added to a scalar
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span"></param>
        /// <param name="scalar"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static MemoryOwner<T> Add<T>(
            this ReadOnlySpan<T> span,
            T scalar
        )where T: unmanaged, INumber<T>
        {
            var scalarVector = new Vector<T>(scalar);
            return TransformVectorised(
                span, 
                (in Vector<T> a, out Vector<T> r) => r = a + scalarVector, 
                a => a + scalar
            );
        }

        /// <summary>
        /// Adds another span to this span in place
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span">This span</param>
        /// <param name="other">Other span</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static void AddInPlace<T>(
            this Span<T> span, 
            ReadOnlySpan<T> other
        ) where T: unmanaged, INumber<T> => MutateVectorised(
            span, 
            other, 
            (in Vector<T> a, in Vector<T> b, out Vector<T> r) => r = a + b, 
            (a, b) => a + b
        );

        /// <summary>
        /// Adds another span to this span and applies coefficients to each value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span">This span</param>
        /// <param name="other">Other span</param>
        /// <param name="coefficient1">Coefficient to apply to each value in this span</param>
        /// <param name="coefficient2">Coefficient to apply to each value in the other span</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static void AddInPlace<T>(
            this Span<T> span, 
            ReadOnlySpan<T> other, 
            T coefficient1, 
            T coefficient2
        ) where T: unmanaged, INumber<T> => MutateVectorised(
            span, 
            other, 
            (in Vector<T> a, in Vector<T> b, out Vector<T> r) => r = (a * coefficient1) + (b * coefficient2), 
            (a,b) => (a * coefficient1) + (b * coefficient2)
        );

        /// <summary>
        /// Adds a scalar to each value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span"></param>
        /// <param name="scalar"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static void AddInPlace<T>(
            this Span<T> span, 
            T scalar
        ) where T: unmanaged, INumber<T>
        {
            var scalarVector = new Vector<T>(scalar);
            MutateInPlaceVectorised(
                span, 
                (in Vector<T> a, out Vector<T> r) => r = a + scalarVector, 
                a => a + scalar
            );
        }

        /// <summary>
        /// Multiplies each value by a scalar
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span"></param>
        /// <param name="scalar"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static void MultiplyInPlace<T>(
            this Span<T> span, 
            T scalar
        ) where T: unmanaged, INumber<T>
        {
            var scalarVector = new Vector<T>(scalar);
            MutateInPlaceVectorised(
                span, 
                (in Vector<T> a, out Vector<T> r) => r = a * scalarVector, 
                a => a * scalar
            );
        }

        /// <summary>
        /// Creates a new buffer that contains each value multiplied by a scalar
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span"></param>
        /// <param name="scalar"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static MemoryOwner<T> Multiply<T>(
            this ReadOnlySpan<T> span, 
            T scalar
        ) where T: unmanaged, INumber<T>
        {
            var scalarVector = new Vector<T>(scalar);
            return TransformVectorised(
                span, 
                (in Vector<T> a, out Vector<T> r) => r = a * scalarVector, 
                a => a * scalar
            );
        } 

        /// <summary>
        /// Creates a new buffer in which each value in another span is subtracted from the values in this span
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span">This span</param>
        /// <param name="other">Other span</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static MemoryOwner<T> Subtract<T>(
            this ReadOnlySpan<T> span, 
            ReadOnlySpan<T> other
        ) where T: unmanaged, INumber<T> => ZipVectorised(
            span, 
            other, 
            (in Vector<T> a, in Vector<T> b, out Vector<T> r) => r = a - b, 
            (a, b) => a - b
        );

        /// <summary>
        /// Creates a new buffer in which each value in another span is multiplied by the second coefficient and then subtracted from the values in this span multiplied by the first coefficient
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span">This span</param>
        /// <param name="other">Other span</param>
        /// <param name="coefficient1">Coefficient to apply to each value in this span</param>
        /// <param name="coefficient2">Coefficient to apply to each value in the other span</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static MemoryOwner<T> Subtract<T>(
            this ReadOnlySpan<T> span, 
            ReadOnlySpan<T> other, 
            T coefficient1, 
            T coefficient2
        ) where T: unmanaged, INumber<T> => ZipVectorised(
            span, 
            other, 
            (in Vector<T> a, in Vector<T> b, out Vector<T> r) => r = a * coefficient1 - b * coefficient2, 
            (a, b) => a * coefficient1 - b * coefficient2
        );

        /// <summary>
        /// Subtracts another span from this span
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span">This span</param>
        /// <param name="other">Other span</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static void SubtractInPlace<T>(
            this Span<T> span, 
            ReadOnlySpan<T> other
        ) where T: unmanaged, INumber<T> => MutateVectorised(
            span, 
            other, 
            (in Vector<T> a, in Vector<T> b, out Vector<T> r) => r = a - b, 
            (a, b) => a - b
        );

        /// <summary>
        /// Modifies this span so that each value in another span is multiplied by the second coefficient and then subtracted from the values in this span multiplied by the first coefficient
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span">This span</param>
        /// <param name="other">Other span</param>
        /// <param name="coefficient1">Coefficient to apply to each value in this span</param>
        /// <param name="coefficient2">Coefficient to apply to each value in the other span</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static void SubtractInPlace<T>(
            this Span<T> span, 
            ReadOnlySpan<T> other, 
            T coefficient1, 
            T coefficient2
        ) where T: unmanaged, INumber<T> => MutateVectorised(
            span, 
            other, 
            (in Vector<T> a, in Vector<T> b, out Vector<T> r) => r = a * coefficient1 - b * coefficient2, 
            (a, b) => a * coefficient1 - b * coefficient2
        );

        /// <summary>
        /// Creates a new buffer in which each value in this span is multiplied by the pairwise value from another span
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span">This span</param>
        /// <param name="other">Other span</param>
        /// <returns></returns>
        public static MemoryOwner<T> PointwiseMultiply<T>(
            this ReadOnlySpan<T> span, 
            ReadOnlySpan<T> other
        ) where T: unmanaged, INumber<T> => ZipVectorised(
            span, 
            other, 
            (in Vector<T> a, in Vector<T> b, out Vector<T> r) => r = a * b, 
            (a, b) => a * b
        );

        /// <summary>
        /// Modifies this span so that each value is multiplied by the pairwise value from another span
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span">This span</param>
        /// <param name="other">Other span</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static void PointwiseMultiplyInPlace<T>(
            this Span<T> span, 
            ReadOnlySpan<T> other
        ) where T: unmanaged, INumber<T> => MutateVectorised(
            span, 
            other, 
            (in Vector<T> a, in Vector<T> b, out Vector<T> r) => r = a * b, 
            (a, b) => a * b
        );

        /// <summary>
        /// Creates a new buffer in which each value in this span is divided by the pairwise value from another span
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span">This span</param>
        /// <param name="other">Other span</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static MemoryOwner<T> PointwiseDivide<T>(
            this ReadOnlySpan<T> span, 
            ReadOnlySpan<T> other
        ) where T: unmanaged, INumber<T> => ZipVectorised(
            span, 
            other, 
            (in Vector<T> a, in Vector<T> b, out Vector<T> r) => r = a / b, 
            (a, b) => a / b
        );

        /// <summary>
        /// Modifies this span so that each value in this span is divided by the pairwise value from another span
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span">This span</param>
        /// <param name="other">Other span</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static void PointwiseDivideInPlace<T>(
            this Span<T> span, 
            ReadOnlySpan<T> other
        ) where T: unmanaged, INumber<T> => MutateVectorised(
            span, 
            other, 
            (in Vector<T> a, in Vector<T> b, out Vector<T> r) => r = a / b, 
            (a, b) => a / b
        );

        /// <summary>
        /// Calculates the dot product between this span and another span
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span">This span</param>
        /// <param name="other">Other span</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static T DotProduct<T>(
            this ReadOnlySpan<T> span, 
            ReadOnlySpan<T> other
        ) where T: unmanaged, INumber<T>
        {
            var size = span.Length;
            if (size != other.Length)
                throw new ArgumentException("Spans were different sizes");

            var ret = default(T);
            var nextIndex = 0;
            if (size >= Consts.MinimumSizeForVectorised) {
                var vectorSize = Vector<T>.Count;
                var leftVec = MemoryMarshal.Cast<T, Vector<T>>(span);
                var rightVec = MemoryMarshal.Cast<T, Vector<T>>(other);
                var numVectors = size / vectorSize;
                nextIndex = numVectors * vectorSize;
                for (var j = 0; j < numVectors; j++)
                    ret += Vector.Dot(leftVec[j], rightVec[j]);
            }
            for(; nextIndex < size; nextIndex++)
                ret += span[nextIndex] * other[nextIndex];
            return ret;
        }

        /// <summary>
        /// Modifies this span so that each value falls between the min and max values
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        public static void ConstrainInPlace<T>(
            this Span<T> span, 
            T? minValue, 
            T? maxValue
        ) where T: unmanaged, INumber<T>
        {
            MutateInPlace(span, value => {
                if (minValue.HasValue && value.CompareTo(minValue.Value) < 0)
                    return minValue.Value;
                if (maxValue.HasValue && value.CompareTo(maxValue.Value) > 0)
                    return maxValue.Value;
                return value;
            });
        }

        /// <summary>
        /// Checks if each value in this span is finite (not NaN or Infinity)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static bool IsEntirelyFinite<T>(this ReadOnlySpan<T> span) where T: unmanaged, INumber<T>
        {
            for(int i = 0, len = span.Length; i < len; i++) {
                var v = span[i];
                if (T.IsNaN(v) || T.IsInfinity(v))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Reverses the span
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span"></param>
        /// <returns></returns>
        public static unsafe MemoryOwner<T> Reverse<T>(this ReadOnlySpan<T> span) where T: unmanaged, INumber<T>
        {
            var len = span.Length - 1;
            fixed (T* fp = &MemoryMarshal.GetReference(span)) {
                var p = fp;
                return TransformParallelIndexed(span, i => p[len - i]);
            }
        }

        /// <summary>
        /// Creates a new buffer in which each value in this span is squared
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static MemoryOwner<T> Squared<T>(this ReadOnlySpan<T> span) where T: unmanaged, INumber<T> => TransformVectorised(
            span, 
            (in Vector<T> a, out Vector<T> r) => r = a * a, 
            a => a * a
        );

        /// <summary>
        /// Creates a new buffer from the specified indices
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span"></param>
        /// <param name="arrayIndices"></param>
        /// <returns></returns>
        public static MemoryOwner<T> CherryPickIndices<T>(this ReadOnlySpan<T> span, uint[] arrayIndices) where T: unmanaged, INumber<T>
        {
            var ret = MemoryOwner<T>.Allocate(arrayIndices.Length);
            var ptr = ret.Span;
            for (int i = 0, len = arrayIndices.Length; i < len; i++)
                ptr[i] = span[(int)arrayIndices[i]];
            return ret;
        }

        /// <summary>
        /// Applies a callback (that might be executed in parallel) against each element in this span and its index
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span"></param>
        /// <param name="analyser">Callback that receives each value and its index</param>
        public static unsafe void Analyse<T>(ReadOnlySpan<T> span, Action<T, uint> analyser) where T: unmanaged, INumber<T>
        {
            var size = span.Length;
            fixed (T* fp = &MemoryMarshal.GetReference(span)) {
                var p = fp;
                if (size >= Consts.MinimumSizeForParallel)
                    Parallel.For(0, size, i => analyser(p[i], (uint)i));
                else {
                    for (uint i = 0; i < size; i++)
                        analyser(*p++, i);
                }
            }
        }
    }
}
