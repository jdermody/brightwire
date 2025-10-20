using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using BrightData.Helper;
using BrightData.LinearAlgebra.ReadOnly;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using CommunityToolkit.HighPerformance.Helpers;

namespace BrightData
{
    /// <summary>
    /// Extensions that work with a span of numbers
    /// </summary>
    public partial class ExtensionMethods
    {
        /// <summary>
        /// Converts the span to a read only span
        /// </summary>
        /// <param name="span"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> AsReadOnly<T>(this Span<T> span) => span;

        /// <summary>
        /// Callback to calculate a new vector of Ts from two existing vectors
        /// </summary>
        /// <param name="a">First input vector</param>
        /// <param name="b">Second input vector</param>
        /// <param name="r">Result (output) vector</param>
        public delegate void ComputeVectorisedTwo<T>(in Vector<T> a, in Vector<T> b, out Vector<T> r);

        /// <summary>
        /// Callback to calculate a new vector of Ts from an existing vector
        /// </summary>
        /// <param name="a">Input vector</param>
        /// <param name="r">Result (output) vector</param>
        public delegate void ComputeVectorisedOne<T>(in Vector<T> a, out Vector<T> r);

        readonly unsafe struct ZipAction<T>(T* segment, T* other, Func<T, T, T> action, T[] ret)
            : IAction
            where T : unmanaged
        {
            public void Invoke(int i) => ret[i] = action(segment[i], other[i]);
        }

        readonly unsafe struct TransformAction<T>(T* segment, Func<T, T> action, T[] ret) : IAction
            where T : unmanaged
        {
            public void Invoke(int i) => ret[i] = action(segment[i]);
        }

        readonly unsafe struct ZipInPlaceAction<T>(T* segment, T* other, Func<T, T, T> action) : IAction
            where T : unmanaged
        {
            public void Invoke(int i) => segment[i] = action(segment[i], other[i]);
        }

        readonly struct TransformIndexedAction<T>(Func<uint, T> action, T[] ret) : IAction
        {
            public void Invoke(int i) => ret[i] = action((uint)i);
        }

        readonly unsafe struct TransformIndexedWithValueAction<T>(T* segment, Func<uint, T, T> action, T[] ret) : IAction
            where T : unmanaged
        {
            public void Invoke(int i) => ret[i] = action((uint)i, segment[i]);
        }

        readonly unsafe struct MutateAction<T>(Func<T, T> action, T* segment) : IAction
            where T : unmanaged
        {
            public void Invoke(int i) => segment[i] = action(segment[i]);
        }

        readonly unsafe struct MutateIndexedAction<T>(Func<uint, T, T> action, T* segment) : IAction
            where T : unmanaged
        {
            public void Invoke(int i) => segment[i] = action((uint)i, segment[i]);
        }

        readonly unsafe struct AnalyseAction<T>(Action<T, uint> action, T* segment) : IAction
            where T : unmanaged
        {
            public void Invoke(int i) => action(segment[i], (uint)i);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static MemoryOwner<T> Allocate<T>(int size, bool clear) => MemoryOwner<T>.Allocate(size, clear ? AllocationMode.Clear : AllocationMode.Default);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static MemoryOwner<T> Allocate<T>(uint size, bool clear) => MemoryOwner<T>.Allocate((int)size, clear ? AllocationMode.Clear : AllocationMode.Default);

        /// <summary>
        /// Creates a new span of numbers from applying an operation to each pair of elements from this and another span
        /// </summary>
        /// <param name="span">This span</param>
        /// <param name="other">Other span</param>
        /// <param name="func">Function that computes a new value from a pair of values</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe MemoryOwner<T> ZipParallel<T>(
            this ReadOnlySpan<T> span,
            ReadOnlySpan<T> other,
            Func<T, T, T> func
        ) where T : unmanaged
        {
            var size = span.Length;
            if (size != other.Length)
                throw new ArgumentException("Spans were different sizes");

            var ret = Allocate<T>(size, false);
            var array = ret.DangerousGetArray().Array!;
            fixed (T* xfp = span)
            fixed (T* yfp = other)
            fixed (T* zfp = &array[0]) {
                if (size >= Consts.MinimumSizeForParallel)
                    ParallelHelper.For(0, size, new ZipAction<T>(xfp, yfp, func, array));
                else {
                    var xp = xfp;
                    var yp = yfp;
                    var zp = zfp;
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemoryOwner<T> ZipVectorized<T>(
            this ReadOnlySpan<T> span,
            ReadOnlySpan<T> other,
            ComputeVectorisedTwo<T> func1,
            Func<T, T, T> func2
        ) where T : unmanaged
        {
            var size = span.Length;
            if (size != other.Length)
                throw new ArgumentException("Segments were different sizes");

            var ret = Allocate<T>(size, false);
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe MemoryOwner<T> MapParallel<T>(
            this ReadOnlySpan<T> span,
            Func<T, T> transformer
        ) where T : unmanaged
        {
            var size = span.Length;
            var ret = Allocate<T>(size, false);
            var array = ret.DangerousGetArray().Array!;

            fixed (T* xfp = span)
            fixed (T* zfp = &array[0]) {
                if (size >= Consts.MinimumSizeForParallel)
                    ParallelHelper.For(0, size, new TransformAction<T>(xfp, transformer, array));
                else {
                    var xp = xfp;
                    var zp = zfp;
                    for (var i = 0; i < size; i++)
                        *zp++ = transformer(*xp++);
                }
            }

            return ret;
        }

        /// <summary>
        /// Applies a callback to each item in the span
        /// </summary>
        /// <param name="span">Vector</param>
        /// <param name="transformer">Callback</param>
        /// <returns>Memory buffer that holds results from each callback</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe MemoryOwner<T> MapParallel<T>(
            this ReadOnlySpan<T> span,
            Func<uint, T, T> transformer
        ) where T : unmanaged
        {
            var size = span.Length;
            var ret = Allocate<T>(size, false);
            var array = ret.DangerousGetArray().Array!;

            fixed (T* xfp = span)
            fixed (T* zfp = &array[0]) {
                if (size >= Consts.MinimumSizeForParallel)
                    ParallelHelper.For(0, size, new TransformIndexedWithValueAction<T>(xfp, transformer, array));
                else {
                    var xp = xfp;
                    var zp = zfp;
                    for (uint i = 0; i < size; i++)
                        *zp++ = transformer(i, *xp++);
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemoryOwner<T> TransformVectorized<T>(
            this ReadOnlySpan<T> span,
            ComputeVectorisedOne<T> transformer1,
            Func<T, T> transformer2
        ) where T : unmanaged
        {
            var size = span.Length;
            var ret = Allocate<T>(size, false);
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemoryOwner<T> TransformParallelIndexed<T>(
            this ReadOnlySpan<T> span,
            Func<uint, T> transformer
        ) where T : unmanaged
        {
            var size = span.Length;
            var ret = Allocate<T>(size, false);
            var array = ret.DangerousGetArray().Array!;

            if (size >= Consts.MinimumSizeForParallel)
                ParallelHelper.For(0, size, new TransformIndexedAction<T>(transformer, array));
            else {
                for (uint i = 0; i < size; i++)
                    array[i] = transformer(i);
            }

            return ret;
        }

        /// <summary>
        /// Creates a new span from an existing span via a function (possibly executed in parallel) that receives an index and returns a new value
        /// </summary>
        /// <param name="span">Input span</param>
        /// <param name="transformer">Transformation function (possibly executed in parallel)</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe MemoryOwner<T> TransformParallelIndexed<T>(
            this ReadOnlySpan<T> span,
            Func<uint, T, T> transformer
        ) where T : unmanaged
        {
            var size = span.Length;
            var ret = Allocate<T>(size, false);
            var array = ret.DangerousGetArray().Array!;

            fixed (T* xfp = span) {
                if (size >= Consts.MinimumSizeForParallel)
                    ParallelHelper.For(0, size, new TransformIndexedWithValueAction<T>(xfp, transformer, array));
                else {
                    var xp = xfp;
                    for (uint i = 0; i < size; i++)
                        array[i] = transformer(i, *xp++);
                }
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Mutate<T>(
            this Span<T> span,
            ReadOnlySpan<T> other,
            Func<T, T, T> func
        ) where T : unmanaged
        {
            var size = span.Length;
            if (size != other.Length)
                throw new ArgumentException("Spans were different sizes");

            fixed (T* xfp = span)
            fixed (T* yfp = other) {
                if (size >= Consts.MinimumSizeForParallel)
                    ParallelHelper.For(0, size, new ZipInPlaceAction<T>(xfp, yfp, func));
                else {
                    var xp = xfp;
                    var yp = yfp;
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MutateVectorized<T>(
            this Span<T> span,
            ReadOnlySpan<T> other,
            ComputeVectorisedTwo<T> transformer1,
            Func<T, T, T> transformer2
        ) where T : unmanaged
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void MutateInPlace<T>(
            this Span<T> span,
            Func<T, T> mutator
        ) where T : unmanaged
        {
            var size = span.Length;
            fixed (T* xfp = span) {
                if (size >= Consts.MinimumSizeForParallel)
                    ParallelHelper.For(0, size, new MutateAction<T>(mutator, xfp));
                else {
                    var xp = xfp;
                    for (uint i = 0; i < size; i++) {
                        *xp = mutator(*xp);
                        ++xp;
                    }
                }
            }
        }

        /// <summary>
        /// Updates a span in place by applying a mutation function (potentially called in parallel) to each element
        /// </summary>
        /// <param name="span"></param>
        /// <param name="mutator"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void MutateInPlace<T>(
            this Span<T> span,
            Func<uint, T, T> mutator
        ) where T : unmanaged
        {
            var size = span.Length;
            fixed (T* xfp = span) {
                if (size >= Consts.MinimumSizeForParallel)
                    ParallelHelper.For(0, size, new MutateIndexedAction<T>(mutator, xfp));
                else {
                    var xp = xfp;
                    for (uint i = 0; i < size; i++) {
                        *xp = mutator(i, *xp);
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MutateInPlaceVectorized<T>(
            this Span<T> span,
            ComputeVectorisedOne<T> mutator1,
            Func<T, T> mutator2
        ) where T : unmanaged
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
        /// Applies a callback (that might be executed in parallel) against each element in this span and its index
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span"></param>
        /// <param name="analyser">Callback that receives each value and its index</param>
        public static unsafe void Analyse<T>(ReadOnlySpan<T> span, Action<T, uint> analyser) where T : unmanaged
        {
            var size = span.Length;
            fixed (T* fp = span) {
                if (size >= Consts.MinimumSizeForParallel)
                    ParallelHelper.For(0, size, new AnalyseAction<T>(analyser, fp));
                else {
                    var p = fp;
                    for (uint i = 0; i < size; i++)
                        analyser(*p++, i);
                }
            }
        }

        /// <summary>
        /// Calculates the sum of all values in this span
        /// </summary>
        /// <param name="span"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Sum<T>(this ReadOnlySpan<T> span) where T : unmanaged, INumber<T>
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemoryOwner<T> Add<T>(
            this ReadOnlySpan<T> span,
            ReadOnlySpan<T> other
        ) where T : unmanaged, INumber<T> => ZipVectorized(
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemoryOwner<T> Add<T>(
            this ReadOnlySpan<T> span,
            ReadOnlySpan<T> other,
            T coefficient1,
            T coefficient2
        ) where T : unmanaged, INumber<T> => ZipVectorized(
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemoryOwner<T> Add<T>(
            this ReadOnlySpan<T> span,
            T scalar
        ) where T : unmanaged, INumber<T>
        {
            var scalarVector = new Vector<T>(scalar);
            return TransformVectorized(
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddInPlace<T>(
            this Span<T> span,
            ReadOnlySpan<T> other
        ) where T : unmanaged, INumber<T> => MutateVectorized(
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddInPlace<T>(
            this Span<T> span,
            ReadOnlySpan<T> other,
            T coefficient1,
            T coefficient2
        ) where T : unmanaged, INumber<T> => MutateVectorized(
            span,
            other,
            (in Vector<T> a, in Vector<T> b, out Vector<T> r) => r = (a * coefficient1) + (b * coefficient2),
            (a, b) => (a * coefficient1) + (b * coefficient2)
        );

        /// <summary>
        /// Adds a scalar to each value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span"></param>
        /// <param name="scalar"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddInPlace<T>(
            this Span<T> span,
            T scalar
        ) where T : unmanaged, INumber<T>
        {
            var scalarVector = new Vector<T>(scalar);
            MutateInPlaceVectorized(
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MultiplyInPlace<T>(
            this Span<T> span,
            T scalar
        ) where T : unmanaged, INumber<T>
        {
            var scalarVector = new Vector<T>(scalar);
            MutateInPlaceVectorized(
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemoryOwner<T> Multiply<T>(
            this ReadOnlySpan<T> span,
            T scalar
        ) where T : unmanaged, INumber<T>
        {
            var scalarVector = new Vector<T>(scalar);
            return TransformVectorized(
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemoryOwner<T> Subtract<T>(
            this ReadOnlySpan<T> span,
            ReadOnlySpan<T> other
        ) where T : unmanaged, INumber<T> => ZipVectorized(
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemoryOwner<T> Subtract<T>(
            this ReadOnlySpan<T> span,
            ReadOnlySpan<T> other,
            T coefficient1,
            T coefficient2
        ) where T : unmanaged, INumber<T> => ZipVectorized(
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SubtractInPlace<T>(
            this Span<T> span,
            ReadOnlySpan<T> other
        ) where T : unmanaged, INumber<T> => MutateVectorized(
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SubtractInPlace<T>(
            this Span<T> span,
            ReadOnlySpan<T> other,
            T coefficient1,
            T coefficient2
        ) where T : unmanaged, INumber<T> => MutateVectorized(
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
        ) where T : unmanaged, INumber<T> => ZipVectorized(
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PointwiseMultiplyInPlace<T>(
            this Span<T> span,
            ReadOnlySpan<T> other
        ) where T : unmanaged, INumber<T> => MutateVectorized(
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemoryOwner<T> PointwiseDivide<T>(
            this ReadOnlySpan<T> span,
            ReadOnlySpan<T> other
        ) where T : unmanaged, INumber<T> => ZipVectorized(
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PointwiseDivideInPlace<T>(
            this Span<T> span,
            ReadOnlySpan<T> other
        ) where T : unmanaged, INumber<T> => MutateVectorized(
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T DotProduct<T>(
            this ReadOnlySpan<T> span,
            ReadOnlySpan<T> other
        ) where T : unmanaged, INumber<T>
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

            for (; nextIndex < size; nextIndex++)
                ret += span[nextIndex] * other[nextIndex];
            return ret;
        }

        /// <summary>
        /// Creates a new vector that contains the square root of each value in this tensor segment
        /// </summary>
        /// <param name="span">This tensor</param>
        /// <param name="adjustment">A small value to add to each value in case of zeros</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemoryOwner<T> Sqrt<T>(
            this ReadOnlySpan<T> span,
            T adjustment
        ) where T : unmanaged, INumber<T>, IRootFunctions<T>
        {
            Vector<T> adjustmentVector = new(adjustment);
            return TransformVectorized(span,
                (in Vector<T> a, out Vector<T> r) => r = Vector.SquareRoot(a + adjustmentVector),
                x => T.Sqrt(x + adjustment)
            );
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
        ) where T : unmanaged, INumber<T>
        {
            MutateInPlace(span, value => {
                if (T.IsNaN(value))
                    return T.Zero;
                if (minValue.HasValue && (value.CompareTo(minValue.Value) < 0 || T.IsNegativeInfinity(value)))
                    return minValue.Value;
                if (maxValue.HasValue && (value.CompareTo(maxValue.Value) > 0 || T.IsPositiveInfinity(value)))
                    return maxValue.Value;

                return value;
            });
        }

        /// <summary>
        /// Finds the average value in this vector
        /// </summary>
        /// <param name="vector">This tensor</param>
        /// <returns></returns>
        public static T Average<T>(this ReadOnlySpan<T> vector)
            where T : unmanaged, INumber<T>
        {
            if (vector.Length == 0)
                return T.Zero;
            return Sum(vector) / T.CreateSaturating(vector.Length);
        }

        /// <summary>
        /// Calculates the L1 norm of this vector
        /// </summary>
        /// <param name="vector">This tensor</param>
        /// <returns></returns>
        public static T L1Norm<T>(this ReadOnlySpan<T> vector)
            where T : unmanaged, INumber<T>
        {
            using var abs = Abs(vector);
            return Sum(abs.Span.AsReadOnly());
        }

        /// <summary>
        /// Calculates the L2 norm of this tensor segment
        /// </summary>
        /// <param name="segment">This tensor</param>
        /// <returns></returns>
        public static T L2Norm<T>(this ReadOnlySpan<T> segment) where T : unmanaged, INumber<T>, IRootFunctions<T>
        {
            using var squared = Squared(segment);
            return T.Sqrt(Sum(squared.Span.AsReadOnly()));
        }

        /// <summary>
        /// Finds the min and max values (and their indices) of this tensor segment
        /// </summary>
        /// <param name="segment">This tensor</param>
        /// <returns></returns>
        public static (T Min, T Max, uint MinIndex, uint MaxIndex) GetMinAndMaxValues<T>(this ReadOnlySpan<T> segment)
            where T : unmanaged, INumber<T>, IMinMaxValue<T>
        {
            var min = T.MaxValue;
            var max = T.MinValue;
            var minIndex = uint.MaxValue;
            var maxIndex = uint.MaxValue;
            uint index = 0;

            foreach (var value in segment) {
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
        /// Checks if each value in this span is finite (not NaN or Infinity)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEntirelyFinite<T>(this ReadOnlySpan<T> span) where T : unmanaged, INumber<T>
        {
            for (int i = 0, len = span.Length; i < len; i++) {
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
        public static unsafe MemoryOwner<T> Reverse<T>(this ReadOnlySpan<T> span) where T : unmanaged, INumber<T>
        {
            var len = span.Length - 1;
            fixed (T* fp = span) {
                var p = fp;
                return TransformParallelIndexed(span, i => p[len - i]);
            }
        }

        /// <summary>
        /// Calculates the mean squared distance between this and another tensor segment
        /// </summary>
        /// <param name="tensor">This tensor</param>
        /// <param name="other">Other tensor</param>
        /// <returns></returns>
        public static T MeanSquaredDistance<T>(this ReadOnlySpan<T> tensor, ReadOnlySpan<T> other) where T : unmanaged, INumber<T>, IRootFunctions<T>
        {
            if (tensor.Length == 0)
                throw new ArgumentException("Zero length", nameof(tensor));
            var diff = Subtract(tensor, other);
            try {
                var num = L2Norm<T>(diff.Span);
                return num * num / T.CreateSaturating(diff.Length);
            }
            finally {
                diff.Dispose();
            }
        }

        /// <summary>
        /// Calculates the squared euclidean distance between this and another tensor segment
        /// </summary>
        /// <param name="tensor">This tensor</param>
        /// <param name="other">Other tensor</param>
        /// <returns></returns>
        public static T SquaredEuclideanDistance<T>(this ReadOnlySpan<T> tensor, ReadOnlySpan<T> other) where T : unmanaged, INumber<T>, IRootFunctions<T>
        {
            var diff = Subtract(tensor, other);
            try {
                var num = L2Norm<T>(diff.Span);
                return num * num;
            }
            finally {
                diff.Dispose();
            }
        }

        /// <summary>
        /// Calculates the manhattan distance between this and another tensor segment
        /// </summary>
        /// <param name="tensor">This tensor</param>
        /// <param name="other">Other tensor</param>
        /// <returns></returns>
        public static T ManhattanDistance<T>(this ReadOnlySpan<T> tensor, ReadOnlySpan<T> other) where T : unmanaged, INumber<T>
        {
            var distance = Subtract(tensor, other);
            try {
                var squared = Abs<T>(distance.Span);
                try {
                    return Sum(squared.Span.AsReadOnly());
                }
                finally {
                    squared.Dispose();
                }
            }
            finally {
                distance.Dispose();
            }
        }

        /// <summary>
        /// Calculates the euclidean distance between this and another tensor segment
        /// </summary>
        /// <param name="tensor">This tensor</param>
        /// <param name="other">Other tensor</param>
        /// <returns></returns>
        public static T EuclideanDistance<T>(this ReadOnlySpan<T> tensor, ReadOnlySpan<T> other) where T : unmanaged, INumber<T>, IRootFunctions<T>
        {
            var distance = Subtract(tensor, other);
            try {
                var squared = Squared<T>(distance.Span);
                try {
                    return T.Sqrt(squared.Span.AsReadOnly().Sum());
                }
                finally {
                    squared.Dispose();
                }
            }
            finally {
                distance.Dispose();
            }
        }

        /// <summary>
        /// Creates a new tensor segment that contains the absolute value of each value in this tensor segment
        /// </summary>
        /// <param name="tensor">This tensor</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemoryOwner<T> Abs<T>(this ReadOnlySpan<T> tensor) where T : unmanaged, INumber<T> => TransformVectorized(tensor,
            (in Vector<T> a, out Vector<T> r) => r = Vector.Abs(a),
            T.Abs
        );

        /// <summary>
        /// Creates a new tensor segment that contains the natural logarithm of each value in this tensor segment
        /// </summary>
        /// <param name="tensor">This tensor</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemoryOwner<T> Log<T>(this ReadOnlySpan<T> tensor) where T : unmanaged, ILogarithmicFunctions<T> => MapParallel(tensor, T.Log);

        /// <summary>
        /// Creates a new tensor segment that contains the exponent of each value in this tensor segment
        /// </summary>
        /// <param name="tensor">This tensor</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemoryOwner<T> Exp<T>(this ReadOnlySpan<T> tensor) where T : unmanaged, IExponentialFunctions<T> => MapParallel(tensor, T.Exp);

        /// <summary>
        /// Creates a new tensor segment that contains each value raised by the specified power in this tensor segment
        /// </summary>
        /// <param name="segment">This tensor</param>
        /// <param name="power">Specified power</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemoryOwner<T> Pow<T>(this ReadOnlySpan<T> segment, T power) where T : unmanaged, IPowerFunctions<T> => MapParallel(segment, v => T.Pow(v, power));

        /// <summary>
        /// Creates a new buffer in which each value in this span is squared
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemoryOwner<T> Squared<T>(this ReadOnlySpan<T> span) where T : unmanaged, INumber<T> => TransformVectorized(
            span,
            (in Vector<T> a, out Vector<T> r) => r = a * a,
            a => a * a
        );

        /// <summary>
        /// Calculates the standard deviation of this tensor segment
        /// </summary>
        /// <param name="segment">This tensor segment</param>
        /// <param name="mean">Mean of the tensor segment (optional)</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T StdDev<T>(this ReadOnlySpan<T> segment, T? mean) where T : unmanaged, INumber<T>, IRootFunctions<T>
        {
            var avg = mean ?? Average(segment);
            var avgVector = new Vector<T>(avg);
            var result = TransformVectorized(
                segment,
                (in Vector<T> a, out Vector<T> r) => {
                    var s = a - avgVector;
                    r = s * s;
                }, a => {
                    var s = a - avg;
                    return s * s;
                }
            );
            try {
                return T.Sqrt(Average<T>(result.Span));
            }
            finally {
                result.Dispose();
            }
        }

        /// <summary>
        /// Calculates sigmoid
        /// </summary>
        /// <param name="val"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Sigmoid<T>(T val) where T : unmanaged, ISignedNumber<T>, IExponentialFunctions<T> => T.One / (T.One + T.Exp(T.NegativeOne * val));

        /// <summary>
        /// Calculates sigmoid derivative
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T SigmoidDerivative<T>(T val) where T : unmanaged, ISignedNumber<T>, IExponentialFunctions<T>
        {
            var sigmoid = Sigmoid(val);
            return sigmoid * (T.One - sigmoid);
        }

        /// <summary>
        /// Calculates tanh derivative
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T TanhDerivative<T>(T val) where T : unmanaged, INumber<T>, IPowerFunctions<T>, IHyperbolicFunctions<T> => T.One - T.Pow(T.Tanh(val), T.One + T.One);

        /// <summary>
        /// Calculates RELU
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Relu<T>(T val) where T : unmanaged, INumber<T> => (val <= T.Zero) ? T.Zero : val;

        /// <summary>
        /// Calculates RELU derivative
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ReluDerivative<T>(T val) where T : unmanaged, INumber<T> => (val <= T.Zero) ? T.Zero : T.One;

        /// <summary>
        /// Calculates leaky RELU
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T LeakyRelu<T>(T val) where T : unmanaged, INumber<T> => (val <= T.Zero) ? T.CreateChecked(0.01f) * val : val;

        /// <summary>
        /// Calculates leaky RELU derivative
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T LeakyReluDerivative<T>(T val) where T : unmanaged, INumber<T> => (val <= T.Zero) ? T.CreateChecked(0.01f) : T.One;

        /// <summary>
        /// Creates a new tensor segment with sigmoid function applied to each value in this tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemoryOwner<T> Sigmoid<T>(this ReadOnlySpan<T> segment)
            where T : unmanaged, ISignedNumber<T>, IExponentialFunctions<T> => MapParallel(segment, Sigmoid);

        /// <summary>
        /// Creates a new tensor segment with sigmoid derivative applied to each value in this tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemoryOwner<T> SigmoidDerivative<T>(this ReadOnlySpan<T> segment)
            where T : unmanaged, ISignedNumber<T>, IExponentialFunctions<T> => MapParallel(segment, SigmoidDerivative);

        /// <summary>
        /// Creates a new tensor segment with tanh function applied to each value in this tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemoryOwner<T> Tanh<T>(this ReadOnlySpan<T> segment)
            where T : unmanaged, IHyperbolicFunctions<T> => MapParallel(segment, T.Tanh);

        /// <summary>
        /// Creates a new tensor segment with tanh derivative applied to each value in this tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemoryOwner<T> TanhDerivative<T>(this ReadOnlySpan<T> segment) where T : unmanaged, INumber<T>, IPowerFunctions<T>, IHyperbolicFunctions<T> => MapParallel(segment, TanhDerivative);

        /// <summary>
        /// Creates a new tensor segment with RELU function applied to each value in this tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemoryOwner<T> Relu<T>(this ReadOnlySpan<T> segment) where T : unmanaged, INumber<T> => MapParallel(segment, Relu);

        /// <summary>
        /// Creates a new tensor segment with RELU derivative applied to each value in this tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemoryOwner<T> ReluDerivative<T>(this ReadOnlySpan<T> segment) where T : unmanaged, INumber<T> => MapParallel(segment, ReluDerivative);

        /// <summary>
        /// Creates a new tensor segment with Leaky RELU function applied to each value in this tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemoryOwner<T> LeakyRelu<T>(this ReadOnlySpan<T> segment) where T : unmanaged, INumber<T> => MapParallel(segment, LeakyRelu);

        /// <summary>
        /// Creates a new tensor segment with Leaky RELU derivative applied to each value in this tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemoryOwner<T> LeakyReluDerivative<T>(this ReadOnlySpan<T> segment) where T : unmanaged, INumber<T> => MapParallel(segment, LeakyReluDerivative);

        /// <summary>
        /// Creates a new tensor segment with softmax function applied to each value in this tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemoryOwner<T> Softmax<T>(this ReadOnlySpan<T> segment)
            where T : unmanaged, INumber<T>, IMinMaxValue<T>, IExponentialFunctions<T>
        {
            var (_, max, _, _) = GetMinAndMaxValues(segment);
            var softmax = segment.MapParallel(v => T.Exp(v - max));
            var sum = Sum(softmax.Span.AsReadOnly());
            if (!T.IsZero(sum))
                softmax.Span.MultiplyInPlace(T.One / sum);
            return softmax;
        }

        /// <summary>
        /// Creates a new tensor segment with softmax derivative applied to each value in this tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="rowCount"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemoryOwner<T> SoftmaxDerivative<T>(this ReadOnlySpan<T> segment, int rowCount)
            where T : unmanaged, INumber<T>
        {
            var ret = Allocate<T>(segment.Length * segment.Length, false);
            var span = ret.Span;
            for (int i = 0, len = ret.Length; i < len; i++) {
                var x = i % rowCount;
                var y = i / rowCount;
                var xVal = segment[x];
                span[i] = (x == y)
                    ? xVal * (T.One - xVal)
                    : -xVal * segment[y];
            }

            return ret;
        }

        /// <summary>
        /// Creates a new buffer from the specified indices
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span"></param>
        /// <param name="indices"></param>
        /// <returns></returns>
        public static MemoryOwner<T> CherryPickIndices<T>(this ReadOnlySpan<T> span, ReadOnlySpan<uint> indices)
        {
            var ret = MemoryOwner<T>.Allocate(indices.Length);
            var ptr = ret.Span;
            for (int i = 0, len = indices.Length; i < len; i++)
                ptr[i] = span[(int)indices[i]];
            return ret;
        }

        /// <summary>
        /// Rounds each value in this tensor segment to be either the lower or upper supplied parameters
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="lower"></param>
        /// <param name="upper"></param>
        public static void RoundInPlace<T>(this Span<T> segment, T lower, T upper)
            where T : unmanaged, INumber<T>
        {
            var compareTo = lower + (upper - lower) / (T.One + T.One);
            MutateInPlace(segment, v => v >= compareTo ? upper : lower);
        }

        /// <summary>
        /// In place L1 regularization of the tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="coefficient">Coefficient to apply to each adjusted value</param>
        public static void L1Regularization<T>(this Span<T> segment, T coefficient)
            where T : unmanaged, INumber<T>
        {
            for (int i = 0, len = segment.Length; i < len; i++) {
                var val = segment[i];
                segment[i] = val - (val > T.Zero ? T.One : val < T.Zero ? -T.One : T.Zero) * coefficient;
            }
        }

        /// <summary>
        /// Applies a distance metric to two vectors and returns the distance between them
        /// </summary>
        /// <param name="vector">First vector</param>
        /// <param name="other">Second vector</param>
        /// <param name="distance">Distance metric</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static T FindDistance<T>(this ReadOnlySpan<T> vector, ReadOnlySpan<T> other, DistanceMetric distance) where T : unmanaged, IBinaryFloatingPointIeee754<T> => distance switch {
            DistanceMetric.Cosine => vector.CosineDistance(other),
            DistanceMetric.Angular => vector.AngularDistance(other),
            DistanceMetric.Euclidean => vector.EuclideanDistance(other),
            DistanceMetric.Manhattan => vector.ManhattanDistance(other),
            DistanceMetric.MeanSquared => vector.MeanSquaredDistance(other),
            DistanceMetric.SquaredEuclidean => vector.SquaredEuclideanDistance(other),
            DistanceMetric.InnerProductSpace => vector.InnerProductSpaceDistance(other),
            _ => throw new NotImplementedException(distance.ToString())
        };

        /// <summary>
        /// Vectorized cosine distance (0 for perpendicular, 1 for orthogonal, 2 for opposite)
        /// </summary>
        /// <param name="v1">First vector</param>
        /// <param name="v2">Second vector</param>
        /// <returns>Cosine distance between the two vectors</returns>
        /// <exception cref="ArgumentException"></exception>
        public static T CosineDistance<T>(this ReadOnlySpan<T> v1, ReadOnlySpan<T> v2)
            where T : unmanaged, IBinaryFloatingPointIeee754<T>
        {
            var length = v1.Length;
            if (length != v2.Length)
                throw new ArgumentException($"Spans were of different size: ({v1.Length} vs {v2.Length})");

            if (length >= Consts.MinimumSizeForVectorised) {
                var leftVec = MemoryMarshal.Cast<T, Vector<T>>(v1);
                var rightVec = MemoryMarshal.Cast<T, Vector<T>>(v2);
                var numVectors = length / Vector<T>.Count;
                var nextIndex = numVectors * Vector<T>.Count;
                Vector<T> ab = new(T.Zero), aa = new(T.Zero), bb = new(T.Zero);
                for (var i = 0; i < numVectors; i++) {
                    ab += leftVec[i] * rightVec[i];
                    aa += leftVec[i] * leftVec[i];
                    bb += rightVec[i] * rightVec[i];
                }

                T ab2 = Vector.Dot(ab, Vector<T>.One), aa2 = Vector.Dot(aa, Vector<T>.One), bb2 = Vector.Dot(bb, Vector<T>.One);
                for (; nextIndex < length; nextIndex++) {
                    T a = v1[nextIndex], b = v2[nextIndex];
                    ab2 += a * b;
                    aa2 += a * a;
                    bb2 += b * b;
                }

                return T.One - ab2 / (T.Sqrt(aa2) * T.Sqrt(bb2));
            }
            else {
                T aa = T.Zero, bb = T.Zero, ab = T.Zero;
                for (int i = 0, len = v1.Length; i < len; i++) {
                    var a = v1[i];
                    var b = v2[i];
                    ab += a * b;
                    aa += a * a;
                    bb += b * b;
                }

                return T.One - ab / (T.Sqrt(aa) * T.Sqrt(bb));
            }
        }

        /// <summary>
        /// Angular distance between two vectors
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static T AngularDistance<T>(this ReadOnlySpan<T> v1, ReadOnlySpan<T> v2)
            where T : unmanaged, IBinaryFloatingPointIeee754<T>
        {
            return T.Acos(T.One - CosineDistance(v1, v2)) / T.Pi;
        }

        /// <summary>
        /// Inner space distance between two vectors
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static T InnerProductSpaceDistance<T>(this ReadOnlySpan<T> v1, ReadOnlySpan<T> v2)
            where T : unmanaged, IBinaryFloatingPointIeee754<T>
        {
            var dot = v1.DotProduct(v2);
            var magnitude = v1.L1Norm() * v2.L1Norm();
            return T.Acos(dot / magnitude);
        }

        /// <summary>
        /// Find the minimum value and index in a vector
        /// </summary>
        /// <param name="vector">Vector to analyze</param>
        /// <returns>Tuple containing the minimum value and its index</returns>
        public static (T Value, uint Index) Minimum<T>(this ReadOnlySpan<T> vector)
            where T : unmanaged, INumber<T>, IMinMaxValue<T>
        {
            var ret = uint.MaxValue;
            var lowestValue = T.MaxValue;

            for (uint i = 0, len = (uint)vector.Length; i < len; i++) {
                var val = vector[(int)i];
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
        public static uint MinimumIndex<T>(this ReadOnlySpan<T> vector)
            where T : unmanaged, INumber<T>, IMinMaxValue<T> => Minimum(vector).Index;

        /// <summary>
        /// Returns the minimum value
        /// </summary>
        /// <param name="vector">Vector to analyse</param>
        /// <returns></returns>
        public static T MinimumValue<T>(this ReadOnlySpan<T> vector)
            where T : unmanaged, INumber<T>, IMinMaxValue<T> => Minimum(vector).Value;

        /// <summary>
        /// Returns the maximum value and index within a vector
        /// </summary>
        /// <param name="vector">Vector to analyse</param>
        /// <returns>Tuple containing the maximum value and its index</returns>
        public static (T Value, uint Index) Maximum<T>(this ReadOnlySpan<T> vector)
            where T : unmanaged, INumber<T>, IMinMaxValue<T>
        {
            var ret = uint.MaxValue;
            var highestValue = T.MinValue;

            for (uint i = 0, len = (uint)vector.Length; i < len; i++) {
                var val = vector[(int)i];
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
        public static uint MaximumIndex<T>(this ReadOnlySpan<T> vector)
            where T : unmanaged, INumber<T>, IMinMaxValue<T> => Maximum(vector).Index;

        /// <summary>
        /// Returns the index of the maximum value within a vector
        /// </summary>
        /// <param name="vector">Vector to analyse</param>
        /// <returns></returns>
        public static T MaximumValue<T>(this ReadOnlySpan<T> vector)
            where T : unmanaged, INumber<T>, IMinMaxValue<T> => Maximum(vector).Value;

        /// <summary>
        /// Calculates the pearson correlation coefficient metric between two tensor segments
        /// https://en.wikipedia.org/wiki/Pearson_correlation_coefficient
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="DivideByZeroException"></exception>
        public static T? PearsonCorrelationCoefficient<T>(this ReadOnlySpan<T> x, ReadOnlySpan<T> y)
            where T : unmanaged, INumber<T>, IRootFunctions<T>
        {
            var size = x.Length;
            if (size != y.Length)
                throw new ArgumentException("The segments must have the same length.");

            // calculate the means of x and y
            var xMean = x.Average();
            var yMean = y.Average();

            // calculate the numerator and denominator of the Pearson similarity formula
            var numerator = T.Zero;
            var denominatorX = T.Zero;
            var denominatorY = T.Zero;

            for (var i = 0; i < size; i++) {
                // get the deviations from the means
                var xDeviation = x[i] - xMean;
                var yDeviation = y[i] - yMean;

                // update the numerator and denominator
                numerator += xDeviation * yDeviation;
                denominatorX += xDeviation * xDeviation;
                denominatorY += yDeviation * yDeviation;
            }

            // calculate the Pearson similarity
            var denominator = T.Sqrt(denominatorX * denominatorY);

            // check if the denominator is zero
            if (T.IsZero(denominator))
                throw new DivideByZeroException("The standard deviations of x and y are zero.");

            return numerator / denominator;
        }

        /// <summary>
        /// Searches the span for the index of the first value that matches the specified value within a level of tolerance
        /// </summary>
        /// <param name="segment">This tensor</param>
        /// <param name="value">Value to find</param>
        /// <param name="tolerance">Degree of tolerance</param>
        /// <returns></returns>
        public static GenericIndexedEnumerator<T> Search<T>(this ReadOnlySpan<T> segment, T value, T? tolerance = null)
            where T : unmanaged, INumber<T>
        {
            var results = new List<uint>();
            var spinLock = new SpinLock();
            tolerance ??= T.CreateSaturating(1E-08f);
            Analyse(segment, (v, index) => {
                if (T.Abs(value - v) < tolerance) {
                    using var l = spinLock.Enter();
                    results.Add(index);
                }
            });
            return new(segment, CollectionsMarshal.AsSpan(results));
        }

        /// <summary>
        /// Calculate the matrix transpose
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="rowCount"></param>
        /// <param name="columnCount"></param>
        /// <returns></returns>
        public static unsafe (MemoryOwner<T> Data, uint RowCount, uint ColumnCount) Transpose<T>(this ReadOnlySpan<T> matrix, uint rowCount, uint columnCount) where T : unmanaged, INumber<T>
        {
            var ret = Allocate<T>(columnCount * rowCount, false);
            fixed (T* matrixPtr = matrix)
            fixed (T* retPtr = ret.Span) {
                CacheTranspose(matrixPtr, rowCount, columnCount, 0, rowCount, 0, columnCount, retPtr);
            }

            return (ret, columnCount, rowCount);
        }

        static unsafe void CacheTranspose<T>(T* from, uint rows, uint columns, uint rb, uint re, uint cb, uint ce, T* to) where T : unmanaged
        {
            uint r = re - rb, c = ce - cb;
            if (r <= 16 && c <= 16) {
                for (var i = rb; i < re; i++) {
                    for (var j = cb; j < ce; j++) {
                        to[i * columns + j] = from[j * rows + i];
                    }
                }
            }
            else if (r >= c) {
                CacheTranspose(from, rows, columns, rb, rb + (r / 2), cb, ce, to);
                CacheTranspose(from, rows, columns, rb + (r / 2), re, cb, ce, to);
            }
            else {
                CacheTranspose(from, rows, columns, rb, re, cb, cb + (c / 2), to);
                CacheTranspose(from, rows, columns, rb, re, cb + (c / 2), ce, to);
            }
        }

        /// <summary>
        /// Returns the index of each element of span, ordered from lowest to highest
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span"></param>
        /// <returns></returns>
        public static uint[] GetRankedIndices<T>(this ReadOnlySpan<T> span) where T : unmanaged, INumber<T>
        {
            var len = span.Length;
            using var temp = SpanOwner<T>.Allocate(len);
            var copy = temp.Span;
            span.CopyTo(copy);

            using var indices = SpanOwner<uint>.Allocate(len);
            var indicesSpan = indices.Span;
            for (var i = 0; i < len; i++)
                indicesSpan[i] = (uint)i;

            copy.Sort(indicesSpan);

            var ret = len.AsRange().ToArray();
            for (var i = 0; i < len; i++)
                ret[indicesSpan[i]] = (uint)i;
            return ret;
        }

        /// <summary>
        /// Returns the top n indices from the span
        /// </summary>
        /// <param name="span"></param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="AT"></typeparam>
        /// <returns></returns>
        public static AT NIndices<T, AT>(this ReadOnlySpan<T> span)
            where AT : IFixedSizeSortedArray<uint, T>, new()
        {
            var ret = new AT();
            for (int i = 0, len = span.Length; i < len; i++)
                ret.TryAdd((uint)i, span[i]);
            return ret;
        }

        /// <summary>
        /// Creates a new span from the current span divided by its L2 norm
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span"></param>
        /// <returns></returns>
        public static MemoryOwner<T> EuclideanNormalize<T>(this ReadOnlySpan<T> span)
            where T : unmanaged, INumber<T>, IRootFunctions<T>
        {
            var magnitude = span.L2Norm();
            if (magnitude == T.Zero) {
                var ret = Allocate<T>((uint)span.Length, false);
                span.CopyTo(ret.Span);
                return ret;
            }

            return span.Multiply(T.One / magnitude);
        }

        /// <summary>
        /// Creates a new span from the current span divided by its L1 norm
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span"></param>
        /// <returns></returns>
        public static MemoryOwner<T> ManhattanNormalize<T>(this ReadOnlySpan<T> span)
            where T : unmanaged, INumber<T>
        {
            var magnitude = span.L1Norm();
            if (magnitude == T.Zero) {
                var ret = Allocate<T>((uint)span.Length, false);
                span.CopyTo(ret.Span);
                return ret;
            }

            return span.Multiply(T.One / magnitude);
        }

        /// <summary>
        /// Returns the element indices that match the comparator using binary search - span should be sorted
        /// </summary>
        /// <param name="span"></param>
        /// <param name="comparator"></param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="CT"></typeparam>
        /// <returns></returns>
        public static uint[] MultiBinarySearchIndices<T, CT>(this ReadOnlySpan<T> span, CT comparator)
            where CT: IComparable<T>, allows ref struct
        {
            var index = span.BinarySearch(comparator);
            if (index >= 0) {
                int first = index, last = index;
                while (first >= 1 && comparator.CompareTo(span[first - 1]) == 0)
                    --first;
                while (last >= 0 && last < span.Length - 1 && comparator.CompareTo(span[last + 1]) == 0)
                    ++last;
                var ind = 0;
                var ret = new uint[last - first + 1];
                for (var i = first; i <= last; i++)
                    ret[ind++] = (uint)i;
                return ret;
            }
            return [];
        }
    
        /// <summary>
        /// Returns the span of elements that match the comparator using binary search - span should be sorted
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="CT"></typeparam>
        /// <param name="span"></param>
        /// <param name="comparator"></param>
        /// <returns></returns>
        public static ReadOnlySpan<T> MultiBinarySearchSpan<T, CT>(this ReadOnlySpan<T> span, CT comparator)
            where CT: IComparable<T>, allows ref struct
        {
            var index = span.BinarySearch(comparator);
            if (index >= 0) {
                int first = index, last = index+1;
                while (first >= 1 && comparator.CompareTo(span[first - 1]) == 0)
                    --first;
                while (last >= 0 && last < span.Length - 1 && comparator.CompareTo(span[last + 1]) == 0)
                    ++last;
                return span[first..last];
            }
            return [];
        }

        /// <summary>
        /// Calculates the XOR of the span with another span
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="other"></param>
        public static void Xor<T>(Span<T> data, ReadOnlySpan<T> other) where T : unmanaged, INumber<T>, IBitwiseOperators<T, T, T> => data.MutateVectorized(
            other,
            (in Vector<T> a, in Vector<T> b, out Vector<T> r) => r = Vector.Xor(a, b),
            (a, b) => a ^ b
        );

        /// <summary>
        /// Calculates the OR of the span with another span
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="other"></param>
        public static void Or<T>(Span<T> data, ReadOnlySpan<T> other) where T : unmanaged, IBitwiseOperators<T, T, T> => data.MutateVectorized(
            other,
            (in Vector<T> a, in Vector<T> b, out Vector<T> r) => r = Vector.BitwiseOr(a, b),
            (a, b) => a | b
        );

        /// <summary>
        /// Calculates the AND of the span with another span
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="other"></param>
        public static void And<T>(Span<T> data, ReadOnlySpan<T> other) where T : unmanaged, IBitwiseOperators<T, T, T> => data.MutateVectorized(
            other,
            (in Vector<T> a, in Vector<T> b, out Vector<T> r) => r = Vector.BitwiseAnd(a, b),
            (a, b) => a & b
        );

        /// <summary>
        /// Creates a read only vector from the span
        /// </summary>
        /// <param name="span"></param>
        /// <returns></returns>
        public static ReadOnlyVector<T> ToReadOnlyVector<T>(this ReadOnlySpan<T> span) where T: unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T> => new(span.ToArray());

        /// <summary>
        /// Creates a read only vector from the span
        /// </summary>
        /// <param name="span"></param>
        /// <returns></returns>
        public static ReadOnlyVector<T> ToReadOnlyVector<T>(this Span<T> span) where T: unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T> => new(span.ToArray());
    }
}
