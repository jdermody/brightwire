using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;
using BrightData.Helper;
using BrightData.LinearAlgebra;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData
{
    /// <summary>
    /// Extensions that work with a span of floats
    /// </summary>
    public static class SpanExtensions
    {
        /// <summary>
        /// Hardware dependent size of a numeric vector of floats
        /// </summary>
        public static readonly int NumericsVectorSize = Vector<float>.Count;

        /// <summary>
        /// Callback to calculate a new vector of floats from two existing vectors
        /// </summary>
        /// <param name="a">First input vector</param>
        /// <param name="b">Second input vector</param>
        /// <param name="r">Result (output) vector</param>
        public delegate void ComputeVectorisedTwo(in Vector<float> a, in Vector<float> b, out Vector<float> r);

        /// <summary>
        /// Callback to calculate a new vector of floats from an existing vector
        /// </summary>
        /// <param name="a">Input vector</param>
        /// <param name="r">Result (output) vector</param>
        public delegate void ComputeVectorisedOne(in Vector<float> a, out Vector<float> r);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]static MemoryOwner<float> Allocate(int size) => MemoryOwner<float>.Allocate(size);
        
        /// <summary>
        /// Creates a new buffer from applying an operation to each pair of elements from two spans
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="other"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static unsafe MemoryOwner<float> ZipParallel(
            this ReadOnlySpan<float> segment, 
            ReadOnlySpan<float> other, 
            Func<float, float, float> func
        ) {
            var size = segment.Length;
            if (size != other.Length)
                throw new ArgumentException("Spans were different sizes");

            var ret = Allocate(size);
            var array = ret.DangerousGetArray().Array!;
            fixed (float* xfp = &MemoryMarshal.GetReference(segment))
            fixed (float* yfp = &MemoryMarshal.GetReference(other))
            fixed (float* zfp = &array[0]){
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
        /// Applies a function across each pair of elements from two vectors
        /// </summary>
        /// <param name="segment">First vector</param>
        /// <param name="other">Second vector</param>
        /// <param name="func1">Vector callback</param>
        /// <param name="func2">Element callback</param>
        /// <returns>Memory buffer that holds results from each callback</returns>
        /// <exception cref="ArgumentException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static MemoryOwner<float> ZipVectorised(
            this ReadOnlySpan<float> segment, 
            ReadOnlySpan<float> other, 
            ComputeVectorisedTwo func1,
            Func<float, float, float> func2)
        {
            var size = segment.Length;
            if (size != other.Length)
                throw new ArgumentException("Segments were different sizes");

            var ret = Allocate(size);
            var resultPtr = ret.Span;
            var nextIndex = 0;
            if (size >= Consts.MinimumSizeForVectorised) {
                var leftVec = MemoryMarshal.Cast<float, Vector<float>>(segment);
                var rightVec = MemoryMarshal.Cast<float, Vector<float>>(other);
                var resultVec = MemoryMarshal.Cast<float, Vector<float>>(resultPtr);
                var numVectors = size / NumericsVectorSize;
                nextIndex = numVectors * NumericsVectorSize;
                for (var j = 0; j < numVectors; j++)
                    func1(leftVec[j], rightVec[j], out resultVec[j]);
            }
            for (; nextIndex < size; nextIndex++)
                resultPtr[nextIndex] = func2(segment[nextIndex], other[nextIndex]);
            return ret;
        }

        /// <summary>
        /// Applies a callback to each item in the span
        /// </summary>
        /// <param name="segment">Vector</param>
        /// <param name="transformer">Callback</param>
        /// <returns>Memory buffer that holds results from each callback</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static unsafe MemoryOwner<float> TransformParallel(this ReadOnlySpan<float> segment, Func<float, float> transformer)
        {
            var size = segment.Length;
            var ret = Allocate(size);
            var array = ret.DangerousGetArray().Array!;

            fixed (float* xfp = &MemoryMarshal.GetReference(segment))
            fixed (float* zfp = &array[0]) {
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

        ///
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static MemoryOwner<float> TransformVectorised(
            this ReadOnlySpan<float> segment, 
            ComputeVectorisedOne transformer1, 
            Func<float, float> transformer2)
        {
            var size = segment.Length;
            var ret = Allocate(size);
            var resultPtr = ret.Span;

            var nextIndex = 0;
            if (size >= Consts.MinimumSizeForVectorised) {
                var leftVec = MemoryMarshal.Cast<float, Vector<float>>(segment);
                var resultVec = MemoryMarshal.Cast<float, Vector<float>>(resultPtr);
                var numVectors = size / NumericsVectorSize;
                nextIndex = numVectors * NumericsVectorSize;
                for (var j = 0; j < numVectors; j++)
                    transformer1(leftVec[j], out resultVec[j]);
            }
            for (; nextIndex < size; nextIndex++)
                resultPtr[nextIndex] = transformer2(segment[nextIndex]);

            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static MemoryOwner<float> TransformParallelIndexed(this ReadOnlySpan<float> segment, Func<uint, float> transformer)
        {
            var size = segment.Length;
            var ret = Allocate(size);
            var array = ret.DangerousGetArray().Array!;

            if(size >= Consts.MinimumSizeForParallel)
                Parallel.For(0, size, i => array[i] = transformer((uint)i));
            else {
                for (uint i = 0; i < size; i++)
                    array[i] = transformer(i);
            }
            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static unsafe void Mutate(this Span<float> segment, ReadOnlySpan<float> other, Func<float, float, float> func)
        {
            var size = segment.Length;
            if (size != other.Length)
                throw new ArgumentException("Spans were different sizes");

            fixed (float* xfp = &MemoryMarshal.GetReference(segment))
            fixed (float* yfp = &MemoryMarshal.GetReference(other)) {
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static void MutateVectorised(
            this Span<float> segment, 
            ReadOnlySpan<float> other, 
            ComputeVectorisedTwo func1, 
            Func<float, float, float> func2)
        {
            var size = segment.Length;
            if (size != other.Length)
                throw new ArgumentException("Spans were different sizes");

            var nextIndex = 0;
            if (size >= Consts.MinimumSizeForVectorised) {
                var leftVec = MemoryMarshal.Cast<float, Vector<float>>(segment);
                var rightVec = MemoryMarshal.Cast<float, Vector<float>>(other);
                var numVectors = size / NumericsVectorSize;
                nextIndex = numVectors * NumericsVectorSize;

                for (var i = 0; i < numVectors; i++) {
                    func1(leftVec[i], rightVec[i], out var temp);
                    leftVec[i] = temp;
                }
            }
            for (; nextIndex < size; nextIndex++)
                segment[nextIndex] = func2(segment[nextIndex], other[nextIndex]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static unsafe void MutateInPlace(this Span<float> segment, Func<float, float> mutator)
        {
            var size = segment.Length;
            fixed (float* xfp = &MemoryMarshal.GetReference(segment)) {
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static void MutateInPlaceVectorised(
            this Span<float> segment, 
            ComputeVectorisedOne mutator1, 
            Func<float, float> mutator2)
        {
            var size = segment.Length;
            var nextIndex = 0;
            if (size >= Consts.MinimumSizeForVectorised) {
                var leftVec = MemoryMarshal.Cast<float, Vector<float>>(segment);
                var numVectors = size / NumericsVectorSize;
                nextIndex = numVectors * NumericsVectorSize;
                for (var i = 0; i < numVectors; i++) {
                    mutator1(leftVec[i], out var temp);
                    leftVec[i] = temp;
                }
            }
            for (; nextIndex < size; nextIndex++)
                segment[nextIndex] = mutator2(segment[nextIndex]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static float Sum(this Span<float> span) => Sum((ReadOnlySpan<float>)span);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static unsafe float Sum(this ReadOnlySpan<float> span)
        {
            var result = 0f;
            var size = span.Length;

            if (size >= Consts.MinimumSizeForVectorised && Sse3.IsSupported) {
                fixed (float* pSource = &MemoryMarshal.GetReference(span)) {
                    var vResult = Vector128<float>.Zero;

                    var i = 0;
                    var lastBlockIndex = size - (size % 4);
                    while (i < lastBlockIndex) {
                        vResult = Sse.Add(vResult, Sse.LoadVector128(pSource + i));
                        i += 4;
                    }

                    vResult = Sse3.HorizontalAdd(vResult, vResult);
                    vResult = Sse3.HorizontalAdd(vResult, vResult);
                    result = vResult.ToScalar();

                    while (i < size) {
                        result += pSource[i];
                        i++;
                    }
                }
            }
            else {
                foreach (var item in span)
                    result += item;
            }
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static MemoryOwner<float> Add(this ReadOnlySpan<float> tensor1, ReadOnlySpan<float> tensor2) => ZipVectorised(
            tensor1, 
            tensor2, 
            (in Vector<float> a, in Vector<float> b, out Vector<float> r) => r = a + b, 
            (a, b) => a + b
        );
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static MemoryOwner<float> Add(this ReadOnlySpan<float> tensor1, ReadOnlySpan<float> tensor2, float coefficient1, float coefficient2) => ZipVectorised(
            tensor1, 
            tensor2, 
            (in Vector<float> a, in Vector<float> b, out Vector<float> r) => r = a * coefficient1 + b * coefficient2,
            (a, b) => a * coefficient1 + b * coefficient2
        );
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static MemoryOwner<float> Add(this ReadOnlySpan<float> tensor, float scalar)
        {
            var scalarVector = new Vector<float>(scalar);
            return TransformVectorised(
                tensor, 
                (in Vector<float> a, out Vector<float> r) => r = a + scalarVector, 
                a => a + scalar
            );
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static void AddInPlace(this Span<float> target, ReadOnlySpan<float> other) => MutateVectorised(
            target, 
            other, 
            (in Vector<float> a, in Vector<float> b, out Vector<float> r) => r = a + b, 
            (a, b) => a + b
        );
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static void AddInPlace(this Span<float> target, ReadOnlySpan<float> other, float coefficient1, float coefficient2) => MutateVectorised(
            target, 
            other, 
            (in Vector<float> a, in Vector<float> b, out Vector<float> r) => r = (a * coefficient1) + (b * coefficient2), 
            (a,b) => (a * coefficient1) + (b * coefficient2)
        );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static void AddInPlace(this Span<float> target, float scalar)
        {
            var scalarVector = new Vector<float>(scalar);
            MutateInPlaceVectorised(
                target, 
                (in Vector<float> a, out Vector<float> r) => r = a + scalarVector, 
                a => a + scalar
            );
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static void MultiplyInPlace(this Span<float> target, float scalar)
        {
            var scalarVector = new Vector<float>(scalar);
            MutateInPlaceVectorised(
                target, 
                (in Vector<float> a, out Vector<float> r) => r = a * scalarVector, 
                a => a * scalar
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static MemoryOwner<float> Multiply(this ReadOnlySpan<float> target, float scalar)
        {
            var scalarVector = new Vector<float>(scalar);
            return TransformVectorised(
                target, 
                (in Vector<float> a, out Vector<float> r) => r = a * scalarVector, 
                a => a * scalar
            );
        } 

        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static MemoryOwner<float> Subtract(this ReadOnlySpan<float> tensor1, ReadOnlySpan<float> tensor2) => ZipVectorised(
            tensor1, 
            tensor2, 
            (in Vector<float> a, in Vector<float> b, out Vector<float> r) => r = a - b, 
            (a, b) => a - b
        );
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static MemoryOwner<float> Subtract(this ReadOnlySpan<float> tensor1, ReadOnlySpan<float> tensor2, float coefficient1, float coefficient2) => ZipVectorised(
            tensor1, 
            tensor2, 
            (in Vector<float> a, in Vector<float> b, out Vector<float> r) => r = a * coefficient1 - b * coefficient2, 
            (a, b) => a * coefficient1 - b * coefficient2
        );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static void SubtractInPlace(this Span<float> target, ReadOnlySpan<float> other) => MutateVectorised(
            target, 
            other, 
            (in Vector<float> a, in Vector<float> b, out Vector<float> r) => r = a - b, 
            (a, b) => a - b
        );
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static void SubtractInPlace(this Span<float> target, ReadOnlySpan<float> other, float coefficient1, float coefficient2) => MutateVectorised(
            target, 
            other, 
            (in Vector<float> a, in Vector<float> b, out Vector<float> r) => r = a * coefficient1 - b * coefficient2, 
            (a, b) => a * coefficient1 - b * coefficient2
        );

        public static MemoryOwner<float> PointwiseMultiply(this ReadOnlySpan<float> tensor1, ReadOnlySpan<float> tensor2) => ZipVectorised(
            tensor1, 
            tensor2, 
            (in Vector<float> a, in Vector<float> b, out Vector<float> r) => r = a * b, 
            (a, b) => a * b
        );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static void PointwiseMultiplyInPlace(this Span<float> target, ReadOnlySpan<float> other) => MutateVectorised(
            target, 
            other, 
            (in Vector<float> a, in Vector<float> b, out Vector<float> r) => r = a * b, 
            (a, b) => a * b
        );
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static MemoryOwner<float> PointwiseDivide(this ReadOnlySpan<float> tensor1, ReadOnlySpan<float> tensor2) => ZipVectorised(
            tensor1, 
            tensor2, 
            (in Vector<float> a, in Vector<float> b, out Vector<float> r) => r = a / b, 
            (a, b) => a / b
        );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static void PointwiseDivideInPlace(this Span<float> target, ReadOnlySpan<float> other) => MutateVectorised(
            target, 
            other, 
            (in Vector<float> a, in Vector<float> b, out Vector<float> r) => r = a / b, 
            (a, b) => a / b
        );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static float DotProduct(this ReadOnlySpan<float> segment, ReadOnlySpan<float> other)
        {
            var size = segment.Length;
            if (size != other.Length)
                throw new ArgumentException("Spans were different sizes");

            var ret = 0f;
            var nextIndex = 0;
            if (size >= Consts.MinimumSizeForVectorised) {
                var leftVec = MemoryMarshal.Cast<float, Vector<float>>(segment);
                var rightVec = MemoryMarshal.Cast<float, Vector<float>>(other);
                var numVectors = size / NumericsVectorSize;
                nextIndex = numVectors * NumericsVectorSize;
                for (var j = 0; j < numVectors; j++)
                    ret += Vector.Dot(leftVec[j], rightVec[j]);
            }
            for(; nextIndex < size; nextIndex++)
                ret += segment[nextIndex] * other[nextIndex];
            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static MemoryOwner<float> Sqrt(this ReadOnlySpan<float> tensor, float adjustment = FloatMath.AlmostZero)
        {
            Vector<float> adjustmentVector = new(adjustment);
            return TransformVectorised(tensor, 
                (in Vector<float> a, out Vector<float> r) => r = Vector.SquareRoot(a + adjustmentVector), 
                x => MathF.Sqrt(x + adjustment)
            );
        }

        public static uint? Search(this ReadOnlySpan<float> segment, float value)
        {
            uint? ret = null;
            Analyse(segment, (v, index) => {
                if (Math.Abs(value - v) < FloatMath.AlmostZero)
                    ret = index;
            });
            return ret;
        }

        public static void ConstrainInPlace(this Span<float> segment, float? minValue, float? maxValue)
        {
            MutateInPlace(segment, value => {
                if (minValue.HasValue && value.CompareTo(minValue.Value) < 0)
                    return minValue.Value;
                if (maxValue.HasValue && value.CompareTo(maxValue.Value) > 0)
                    return maxValue.Value;
                return value;
            });
        }

        public static float Average(this ReadOnlySpan<float> segment) => Sum(segment) / segment.Length;

        public static float L1Norm(this ReadOnlySpan<float> segment)
        {
            using var abs = Abs(segment);
            return Sum(abs.Span);
        }

        public static float L2Norm(this ReadOnlySpan<float> segment)
        {
            using var squared = Squared(segment);
            return FloatMath.Sqrt(Sum(squared.Span));
        }

        public static (float Min, float Max, uint MinIndex, uint MaxIndex) GetMinAndMaxValues(this ReadOnlySpan<float> segment)
        {
            var min = float.MaxValue;
            var max = float.MinValue;
            var minIndex = uint.MaxValue;
            var maxIndex = uint.MaxValue;

            for(uint i = 0, len = (uint)segment.Length; i < len; i++) {
                var value = segment[(int)i];
                if (value.CompareTo(max) > 0) {
                    max = value;
                    maxIndex = i;
                }

                if (value.CompareTo(min) < 0) {
                    min = value;
                    minIndex = i;
                }
            }

            return (min, max, minIndex, maxIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static bool IsEntirelyFinite(this ReadOnlySpan<float> segment)
        {
            for(int i = 0, len = segment.Length; i < len; i++) {
                var v = segment[i];
                if (float.IsNaN(v) || float.IsInfinity(v))
                    return false;
            }

            return true;
        }

        public static unsafe MemoryOwner<float> Reverse(this ReadOnlySpan<float> segment)
        {
            var len = segment.Length - 1;
            fixed (float* fp = &MemoryMarshal.GetReference(segment)) {
                var p = fp;
                return TransformParallelIndexed(segment, i => p[len - i]);
            }
        }

        public static float CosineDistance(this ReadOnlySpan<float> tensor, ReadOnlySpan<float> other)
        {
            var ab = DotProduct(tensor, other);
            var aa = DotProduct(tensor, tensor);
            var bb = DotProduct(other, other);
            return 1f - ab / (FloatMath.Sqrt(aa) * FloatMath.Sqrt(bb));
        }

        public static float EuclideanDistance(this ReadOnlySpan<float> tensor, ReadOnlySpan<float> other)
        {
            using var distance = Subtract(tensor, other);
            using var squared = Squared(distance.Span);
            return FloatMath.Sqrt(Sum(squared.Span));
        }

        public static float MeanSquaredDistance(this ReadOnlySpan<float> tensor, ReadOnlySpan<float> other)
        {
            using var diff = Subtract(tensor, other);
            var num = L2Norm(diff.Span);
            return num * num / diff.Length;
        }

        public static float SquaredEuclideanDistance(this ReadOnlySpan<float> tensor, ReadOnlySpan<float> other)
        {
            using var diff = Subtract(tensor, other);
            var num = L2Norm(diff.Span);
            return num * num;
        }

        public static float ManhattanDistance(this ReadOnlySpan<float> tensor, ReadOnlySpan<float> other)
        {
            using var distance = Subtract(tensor, other);
            using var squared = Abs(distance.Span);
            return Sum(squared.Span);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static MemoryOwner<float> Abs(this ReadOnlySpan<float> tensor) => TransformVectorised(tensor, 
            (in Vector<float> a, out Vector<float> r) => r = Vector.Abs(a),
            MathF.Abs
        );
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static MemoryOwner<float> Log(this ReadOnlySpan<float> tensor) => TransformParallel(tensor, MathF.Log);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static MemoryOwner<float> Exp(this ReadOnlySpan<float> tensor) => TransformParallel(tensor, MathF.Exp);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static MemoryOwner<float> Pow(this ReadOnlySpan<float> segment, float power) => TransformParallel(segment, v => FloatMath.Pow(v, power));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static MemoryOwner<float> Squared(this ReadOnlySpan<float> tensor) => TransformVectorised(
            tensor, 
            (in Vector<float> a, out Vector<float> r) => r = a * a, 
            a => a * a
        );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static float StdDev(this ReadOnlySpan<float> segment, float? mean)
        {
            var avg = mean ?? Average(segment);
            var avgVector = new Vector<float>(avg);
            using var result = TransformVectorised(
                segment, 
                (in Vector<float> a, out Vector<float> r) => {
                    var s = a - avgVector;
                    r = s * s;
                }, a => {
                    var s = a - avg;
                    return s * s;
                }
            );
            return FloatMath.Sqrt(Average(result.Span));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]static float Sigmoid(float val) => 1.0f / (1.0f + MathF.Exp(-1.0f * val));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]static float SigmoidDerivative(float val)
        {
            var sigmoid = Sigmoid(val);
            return sigmoid * (1.0f - sigmoid);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]static float Tanh(float val) => MathF.Tanh(val);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]static float TanhDerivative(float val) => 1.0f - MathF.Pow(Tanh(val), 2);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]static float Relu(float val) => (val <= 0) ? 0 : val;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]static float ReluDerivative(float val) => (val <= 0) ? 0f : 1;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]static float LeakyRelu(float val) => (val <= 0) ? 0.01f * val : val;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]static float LeakyReluDerivative(float val) => (val <= 0) ? 0.01f : 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static MemoryOwner<float> Sigmoid(this ReadOnlySpan<float> segment) => TransformParallel(segment, Sigmoid);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static MemoryOwner<float> SigmoidDerivative(this ReadOnlySpan<float> segment) => TransformParallel(segment, SigmoidDerivative);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static MemoryOwner<float> Tanh(this ReadOnlySpan<float> segment) => TransformParallel(segment, Tanh);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static MemoryOwner<float> TanhDerivative(this ReadOnlySpan<float> segment) => TransformParallel(segment, TanhDerivative);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static MemoryOwner<float> Relu(this ReadOnlySpan<float> segment) => TransformParallel(segment, Relu);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static MemoryOwner<float> ReluDerivative(this ReadOnlySpan<float> segment) => TransformParallel(segment, ReluDerivative);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static MemoryOwner<float> LeakyRelu(this ReadOnlySpan<float> segment) => TransformParallel(segment, LeakyRelu);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static MemoryOwner<float> LeakyReluDerivative(this ReadOnlySpan<float> segment) => TransformParallel(segment, LeakyReluDerivative);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static MemoryOwner<float> Softmax(this ReadOnlySpan<float> segment)
        {
            var (_, max, _, _) = GetMinAndMaxValues(segment);
            var softmax = segment.TransformParallel(v => MathF.Exp(v - max));
            var span = softmax.Span;
            var sum = Sum(span);
            if (FloatMath.IsNotZero(sum))
                MultiplyInPlace(span, 1f / sum);
            return softmax;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static unsafe IMatrix SoftmaxDerivative(this ReadOnlySpan<float> segment, LinearAlgebraProvider lap)
        {
            fixed (float* fp = &MemoryMarshal.GetReference(segment)) {
                var p = fp;
                return lap.CreateMatrix((uint)segment.Length, (uint)segment.Length, (x, y) => x == y
                    ? p[x] * (1 - p[x])
                    : -p[x] * p[y]
                );
            }
        }

        public static MemoryOwner<float> CherryPickIndices(this ReadOnlySpan<float> segment, uint[] arrayIndices)
        {
            var ret = MemoryOwner<float>.Allocate(arrayIndices.Length);
            var ptr = ret.Span;
            for (int i = 0, len = arrayIndices.Length; i < len; i++)
                ptr[i] = segment[(int)arrayIndices[i]];
            return ret;
        }
        
        public static void RoundInPlace(this Span<float> segment, float lower, float upper, float? mid)
        {
            var compareTo = mid ?? lower + (upper - lower) / 2;
            MutateInPlace(segment, v => v >= compareTo ? upper : lower);
        }

        public static unsafe void Analyse(ReadOnlySpan<float> segment, Action<float, uint> analyser)
        {
            var size = segment.Length;
            fixed (float* fp = &MemoryMarshal.GetReference(segment)) {
                var p = fp;
                if (size >= Consts.MinimumSizeForParallel)
                    Parallel.For(0, size, i => analyser(p[i], (uint)i));
                else {
                    for (uint i = 0; i < size; i++)
                        analyser(*p++, i);
                }
            }
        }

        public static void L1Regularisation(this Span<float> segment, float coefficient)
        {
            for (int i = 0, len = segment.Length; i < len; i++) {
                var val = segment[i];
                segment[i] = val - (val > 0 ? 1 : val < 0 ? -1 : 0) * coefficient;
            }
        }

        public static void Set(this Span<float> segment, Func<uint, float> getValue)
        {
            for (uint i = 0, len = (uint)segment.Length; i < len; i++)
                segment[(int)i] = getValue(i);
        }

        public static void Set(this Span<float> segment, float value)
        {
            for (int i = 0, len = segment.Length; i < len; i++)
                segment[i] = value;
        }

        public static void SetToRandom(this Span<float> segment, Random random)
        {
            for (int i = 0, len = segment.Length; i < len; i++)
                segment[i] = Convert.ToSingle(random.NextDouble());
        }
    }
}
