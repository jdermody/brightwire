using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;
using BrightData.Helper;
using BrightData.LinearAlgebra;
using BrightData.LinearAlgebra.TensorInfo;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData
{
    public static partial class ExtensionMethods
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]static MemoryOwner<float> Allocate(uint size) => MemoryOwner<float>.Allocate((int)size);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static ITensorSegment ZipParallel(this ITensorSegment segment, ITensorSegment other, Func<float, float, float> func)
        {
            var size = segment.Size;
            if (size != other.Size)
                throw new ArgumentException("Segments were different sizes");

            var ret = Allocate(size);
            var array = ret.DangerousGetArray();
            if (segment.Size >= Consts.MinimumSizeForParallel)
                Parallel.For(0, size, i => array[(int)i] = func(segment[i], other[i]));
            else {
                for (uint i = 0; i < size; i++)
                    array[(int)i] = func(segment[i], other[i]);
            }
            return new ArrayPoolTensorSegment(ret);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static ITensorSegment ZipVectorised(
            this ITensorSegment segment, 
            ITensorSegment other, 
            SpanExtensions.ComputeVectorisedTwo func1,
            Func<float, float, float> func2)
        {
            var size = segment.Size;
            if (size != other.Size)
                throw new ArgumentException("Segments were different sizes");

            MemoryOwner<float> ret;
            if (size >= Consts.MinimumSizeForVectorised) {
                SpanOwner<float> leftTemp = SpanOwner<float>.Empty, rightTemp = SpanOwner<float>.Empty;
                var leftPtr = segment.GetSpan(ref leftTemp, out var wasLefTempUsed);
                var rightPtr = other.GetSpan(ref rightTemp, out var wasRightTempUsed);
                try {
                    ret = leftPtr.ZipVectorised(rightPtr, func1, func2);
                }
                finally {
                    if (wasLefTempUsed)
                        leftTemp.Dispose();
                    if (wasRightTempUsed)
                        rightTemp.Dispose();
                }
            }
            else {
                ret = Allocate(size);
                var retPtr = ret.Span;
                for (var i = 0; i < size; i++)
                    retPtr[i] = func2(segment[i], other[i]);
            }
            return new ArrayPoolTensorSegment(ret);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static ITensorSegment TransformParallel(this ITensorSegment segment, Func<float, float> transfomer)
        {
            var size = segment.Size;
            var ret = Allocate(size);
            var array = ret.DangerousGetArray();

            if (size >= Consts.MinimumSizeForParallel)
                Parallel.For(0, size, i => array[(int)i] = transfomer(segment[i]));
            else {
                for (uint i = 0; i < size; i++)
                    array[(int)i] = transfomer(segment[i]);
            }
            return new ArrayPoolTensorSegment(ret);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static ITensorSegment TransformVectorised(
            this ITensorSegment segment, 
            SpanExtensions.ComputeVectorisedOne transfomer1, 
            Func<float, float> transfomer2)
        {
            var size = segment.Size;
            MemoryOwner<float> ret;

            if (size >= Consts.MinimumSizeForVectorised) {
                var leftTemp = SpanOwner<float>.Empty;
                var leftPtr = segment.GetSpan(ref leftTemp, out var wasLeftTempUsed);
                try {
                    ret = leftPtr.TransformVectorised(transfomer1, transfomer2);
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
                    resultPtr[i] = transfomer2(segment[i]);
            }
            return new ArrayPoolTensorSegment(ret);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static ITensorSegment TransformParallelIndexed(this ITensorSegment segment, Func<uint, float> transfomer)
        {
            var size = segment.Size;
            var ret = Allocate(size);
            var array = ret.DangerousGetArray();

            if(size >= Consts.MinimumSizeForParallel)
                Parallel.For(0, size, i => array[(int)i] = transfomer((uint)i));
            else {
                for (uint i = 0; i < size; i++)
                    array[(int)i] = transfomer(i);
            }
            return new ArrayPoolTensorSegment(ret);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static void Mutate(this ITensorSegment segment, ITensorSegment other, Func<float, float, float> func)
        {
            var size = segment.Size;
            if (size != other.Size)
                throw new ArgumentException("Segments were different sizes");

            if(size >= Consts.MinimumSizeForParallel)
                Parallel.For(0, size, i => segment[i] = func(segment[i], other[i]));
            else {
                for (uint i = 0; i < size; i++)
                    segment[i] = func(segment[i], other[i]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static unsafe void MutateVectorised(
            this ITensorSegment segment, 
            ITensorSegment other, 
            SpanExtensions.ComputeVectorisedTwo func1, 
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
                        leftMutablePtr.MutateVectorised(rightPtr, func1, func2);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static void MutateInPlace(this ITensorSegment segment, Func<float, float> mutator)
        {
            var size = segment.Size;
            if(size >= Consts.MinimumSizeForParallel)
                Parallel.For(0, size, i => segment[i] = mutator(segment[i]));
            else {
                for (uint i = 0; i < size; i++)
                    segment[i] = mutator(segment[i]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static unsafe void MutateInPlaceVectorised(
            this ITensorSegment segment, 
            SpanExtensions.ComputeVectorisedOne mutator1, 
            Func<float, float> mutator2)
        {
            var size = segment.Size;
            if (size >= Consts.MinimumSizeForVectorised) {
                var leftTemp = SpanOwner<float>.Empty;
                var leftPtr = segment.GetSpan(ref leftTemp, out var wasLeftTempUsed);
                try {
                    fixed (float* fp = &MemoryMarshal.GetReference(leftPtr)) {
                        var leftMutablePtr = new Span<float>(fp, (int)size);
                        leftMutablePtr.MutateInPlaceVectorised(mutator1, mutator2);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static float Sum(this ITensorSegment segment)
        {
            var size = segment.Size;
            if (size >= Consts.MinimumSizeForVectorised && Sse3.IsSupported) {
                var temp = SpanOwner<float>.Empty;
                var span = segment.GetSpan(ref temp, out var wasTempUsed);
                try {
                    return span.Sum();
                }
                finally {
                    if(wasTempUsed)
                        temp.Dispose();
                }
            }

            var ret = 0f;
            for (uint i = 0; i < size; i++)
                ret += segment[i];
            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static ITensorSegment Add(this ITensorSegment tensor1, ITensorSegment tensor2) => ZipVectorised(
            tensor1, 
            tensor2, 
            (in Vector<float> a, in Vector<float> b, out Vector<float> r) => r = a + b, 
            (a, b) => a + b
        );
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static ITensorSegment Add(this ITensorSegment tensor1, ITensorSegment tensor2, float coefficient1, float coefficient2) => ZipVectorised(
            tensor1, 
            tensor2, 
            (in Vector<float> a, in Vector<float> b, out Vector<float> r) => r = a * coefficient1 + b * coefficient2,
            (a, b) => a * coefficient1 + b * coefficient2
        );
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static ITensorSegment Add(this ITensorSegment tensor, float scalar)
        {
            var scalarVector = new Vector<float>(scalar);
            return TransformVectorised(
                tensor, 
                (in Vector<float> a, out Vector<float> r) => r = a + scalarVector, 
                a => a + scalar
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static void AddInPlace(this ITensorSegment target, ITensorSegment other) => MutateVectorised(
            target, 
            other, 
            (in Vector<float> a, in Vector<float> b, out Vector<float> r) => r = a + b, 
            (a, b) => a + b
        );
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static void AddInPlace(this ITensorSegment target, ITensorSegment other, float coefficient1, float coefficient2) => MutateVectorised(
            target, 
            other, 
            (in Vector<float> a, in Vector<float> b, out Vector<float> r) => r = (a * coefficient1) + (b * coefficient2), 
            (a,b) => (a * coefficient1) + (b * coefficient2)
        );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static void AddInPlace(this ITensorSegment target, float scalar)
        {
            var scalarVector = new Vector<float>(scalar);
            MutateInPlaceVectorised(
                target, 
                (in Vector<float> a, out Vector<float> r) => r = a + scalarVector, 
                a => a + scalar
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static void MultiplyInPlace(this ITensorSegment target, float scalar)
        {
            var scalarVector = new Vector<float>(scalar);
            MutateInPlaceVectorised(
                target, 
                (in Vector<float> a, out Vector<float> r) => r = a * scalarVector, 
                a => a * scalar
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static ITensorSegment Multiply(this ITensorSegment target, float scalar)
        {
            var scalarVector = new Vector<float>(scalar);
            return TransformVectorised(
                target, 
                (in Vector<float> a, out Vector<float> r) => r = a * scalarVector, 
                a => a * scalar
            );
        } 

        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static ITensorSegment Subtract(this ITensorSegment tensor1, ITensorSegment tensor2) => ZipVectorised(
            tensor1, 
            tensor2, 
            (in Vector<float> a, in Vector<float> b, out Vector<float> r) => r = a - b, 
            (a, b) => a - b
        );
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static ITensorSegment Subtract(this ITensorSegment tensor1, ITensorSegment tensor2, float coefficient1, float coefficient2) => ZipVectorised(
            tensor1, 
            tensor2, 
            (in Vector<float> a, in Vector<float> b, out Vector<float> r) => r = a * coefficient1 - b * coefficient2, 
            (a, b) => a * coefficient1 - b * coefficient2
        );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static void SubtractInPlace(this ITensorSegment target, ITensorSegment other) => MutateVectorised(
            target, 
            other, 
            (in Vector<float> a, in Vector<float> b, out Vector<float> r) => r = a - b, 
            (a, b) => a - b
        );
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static void SubtractInPlace(this ITensorSegment target, ITensorSegment other, float coefficient1, float coefficient2) => MutateVectorised(
            target, 
            other, 
            (in Vector<float> a, in Vector<float> b, out Vector<float> r) => r = a * coefficient1 - b * coefficient2, 
            (a, b) => a * coefficient1 - b * coefficient2
        );

        public static ITensorSegment PointwiseMultiply(this ITensorSegment tensor1, ITensorSegment tensor2) => ZipVectorised(
            tensor1, 
            tensor2, 
            (in Vector<float> a, in Vector<float> b, out Vector<float> r) => r = a * b, 
            (a, b) => a * b
        );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static void PointwiseMultiplyInPlace(this ITensorSegment target, ITensorSegment other) => MutateVectorised(
            target, 
            other, 
            (in Vector<float> a, in Vector<float> b, out Vector<float> r) => r = a * b, 
            (a, b) => a * b
        );
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static ITensorSegment PointwiseDivide(this ITensorSegment tensor1, ITensorSegment tensor2) => ZipVectorised(
            tensor1, 
            tensor2, 
            (in Vector<float> a, in Vector<float> b, out Vector<float> r) => r = a / b, 
            (a, b) => a / b
        );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static void PointwiseDivideInPlace(this ITensorSegment target, ITensorSegment other) => MutateVectorised(
            target, 
            other, 
            (in Vector<float> a, in Vector<float> b, out Vector<float> r) => r = a / b, 
            (a, b) => a / b
        );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static float DotProduct(this ITensorSegment segment, ITensorSegment other)
        {
            var size = segment.Size;
            if(size <= other.Size)
                throw new ArgumentException("Segments were different sizes");
            if (size >= Consts.MinimumSizeForVectorised) {
                SpanOwner<float> leftTemp = SpanOwner<float>.Empty, rightTemp = SpanOwner<float>.Empty;
                var leftPtr = segment.GetSpan(ref leftTemp, out var wasLefTempUsed);
                var rightPtr = other.GetSpan(ref rightTemp, out var wasRightTempUsed);
                try {
                    return leftPtr.DotProduct(rightPtr);
                }
                finally {
                    if(wasLefTempUsed)
                        leftTemp.Dispose();
                    if(wasRightTempUsed)
                        rightTemp.Dispose();
                }
            }

            var ret = 0f;
            for (uint i = 0; i < size; i++)
                ret += segment[i] * other[i];
            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static ITensorSegment Sqrt(this ITensorSegment tensor, float adjustment = FloatMath.AlmostZero)
        {
            Vector<float> adjustmentVector = new(adjustment);
            return TransformVectorised(tensor, 
                (in Vector<float> a, out Vector<float> r) => r = Vector.SquareRoot(a + adjustmentVector), 
                x => MathF.Sqrt(x + adjustment)
            );
        }

        public static uint? Search(this ITensorSegment segment, float value)
        {
            uint? ret = null;
            Analyse(segment, (v, index) => {
                if (Math.Abs(value - v) < FloatMath.AlmostZero)
                    ret = index;
            });
            return ret;
        }

        public static void ConstrainInPlace(this ITensorSegment segment, float? minValue, float? maxValue)
        {
            MutateInPlace(segment, value => {
                if (minValue.HasValue && value.CompareTo(minValue.Value) < 0)
                    return minValue.Value;
                if (maxValue.HasValue && value.CompareTo(maxValue.Value) > 0)
                    return maxValue.Value;
                return value;
            });
        }

        public static float Average(this ITensorSegment segment) => Sum(segment) / segment.Size;

        public static float L1Norm(this ITensorSegment segment)
        {
            var abs = Abs(segment);
            try {
                return Sum(abs);
            }
            finally {
                abs.Release();
            }
        }

        public static float L2Norm(this ITensorSegment segment)
        {
            var squared = Squared(segment);
            try {
                return FloatMath.Sqrt(Sum(squared));
            }
            finally {
                squared.Release();
            }
        }

        public static (float Min, float Max, uint MinIndex, uint MaxIndex) GetMinAndMaxValues(this ITensorSegment segment)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static bool IsEntirelyFinite(this ITensorSegment segment) => !segment.Values.Any(v => float.IsNaN(v) || float.IsInfinity(v));

        public static ITensorSegment Reverse(this ITensorSegment segment)
        {
            var len = segment.Size - 1;
            return TransformParallelIndexed(segment, i => segment[len - i]);
        }

        public static IEnumerable<ITensorSegment> Split(this ITensorSegment segment, uint blockCount)
        {
            for (uint i = 0, size = segment.Size, blockSize = size / blockCount; i < size; i += blockSize)
                yield return new TensorSegmentWrapper(segment, i, 1, blockSize);
        }

        public static float CosineDistance(this ITensorSegment tensor, ITensorSegment other)
        {
            var ab = DotProduct(tensor, other);
            var aa = DotProduct(tensor, tensor);
            var bb = DotProduct(other, other);
            return 1f - ab / (FloatMath.Sqrt(aa) * FloatMath.Sqrt(bb));
        }

        public static float EuclideanDistance(this ITensorSegment tensor, ITensorSegment other)
        {
            var distance = Subtract(tensor, other);
            try {
                var squared = Squared(distance);
                try {
                    return FloatMath.Sqrt(Sum(squared));
                }
                finally {
                    squared.Release();
                }
            }
            finally {
                distance.Release();
            }
        }

        public static float MeanSquaredDistance(this ITensorSegment tensor, ITensorSegment other)
        {
            var diff = Subtract(tensor, other);
            try {
                var num = L2Norm(diff);
                return num * num / diff.Size;
            }
            finally {
                diff.Release();
            }
        }

        public static float SquaredEuclideanDistance(this ITensorSegment tensor, ITensorSegment other)
        {
            var diff = Subtract(tensor, other);
            try {
                var num = L2Norm(diff);
                return num * num;
            }
            finally {
                diff.Release();
            }
        }

        public static float ManhattanDistance(this ITensorSegment tensor, ITensorSegment other)
        {
            var distance = Subtract(tensor, other);
            try {
                var squared = Abs(distance);
                try {
                    return Sum(squared);
                }
                finally {
                    squared.Release();
                }
            }
            finally {
                distance.Release();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static ITensorSegment Abs(this ITensorSegment tensor) => TransformVectorised(tensor, 
            (in Vector<float> a, out Vector<float> r) => r = Vector.Abs(a),
            MathF.Abs
        );
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static ITensorSegment Log(this ITensorSegment tensor) => TransformParallel(tensor, MathF.Log);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static ITensorSegment Exp(this ITensorSegment tensor) => TransformParallel(tensor, MathF.Exp);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static ITensorSegment Pow(this ITensorSegment segment, float power) => TransformParallel(segment, v => FloatMath.Pow(v, power));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static ITensorSegment Squared(this ITensorSegment tensor) => TransformVectorised(
            tensor, 
            (in Vector<float> a, out Vector<float> r) => r = a * a, 
            a => a * a
        );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static float StdDev(this ITensorSegment segment, float? mean)
        {
            var avg = mean ?? Average(segment);
            var avgVector = new Vector<float>(avg);
            var result = TransformVectorised(
                segment, 
                (in Vector<float> a, out Vector<float> r) => {
                    var s = a - avgVector;
                    r = s * s;
                }, a => {
                    var s = a - avg;
                    return s * s;
                }
            );
            try {
                return FloatMath.Sqrt(Average(result));
            }
            finally {
                result.Release();
            }
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static ITensorSegment Sigmoid(this ITensorSegment segment) => TransformParallel(segment, Sigmoid);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static ITensorSegment SigmoidDerivative(this ITensorSegment segment) => TransformParallel(segment, SigmoidDerivative);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static ITensorSegment Tanh(this ITensorSegment segment) => TransformParallel(segment, Tanh);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static ITensorSegment TanhDerivative(this ITensorSegment segment) => TransformParallel(segment, TanhDerivative);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static ITensorSegment Relu(this ITensorSegment segment) => TransformParallel(segment, Relu);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static ITensorSegment ReluDerivative(this ITensorSegment segment) => TransformParallel(segment, ReluDerivative);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static ITensorSegment LeakyRelu(this ITensorSegment segment) => TransformParallel(segment, LeakyRelu);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static ITensorSegment LeakyReluDerivative(this ITensorSegment segment) => TransformParallel(segment, LeakyReluDerivative);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static ITensorSegment Softmax(this ITensorSegment segment)
        {
            var (_, max, _, _) = GetMinAndMaxValues(segment);
            var softmax = segment.TransformParallel(v => MathF.Exp(v - max));
            var sum = Sum(softmax);
            if (FloatMath.IsNotZero(sum))
                softmax.MultiplyInPlace(1f / sum);
            return softmax;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static IMatrix SoftmaxDerivative(this ITensorSegment segment, LinearAlgebraProvider lap)
        {
            return lap.CreateMatrix(segment.Size, segment.Size, (x, y) => x == y
                ? segment[x] * (1 - segment[x])
                : -segment[x] * segment[y]
            );
        }

        public static ITensorSegment CherryPickIndices(this ITensorSegment segment, uint[] arrayIndices)
        {
            var ret = MemoryOwner<float>.Allocate(arrayIndices.Length);
            var ptr = ret.Span;
            for (int i = 0, len = arrayIndices.Length; i < len; i++)
                ptr[i] = segment[arrayIndices[i]];
            return new ArrayPoolTensorSegment(ret);
        }
        
        public static void RoundInPlace(this ITensorSegment segment, float lower, float upper, float? mid)
        {
            var compareTo = mid ?? lower + (upper - lower) / 2;
            MutateInPlace(segment, v => v >= compareTo ? upper : lower);
        }

        public static void Analyse(ITensorSegment segment, Action<float, uint> analyser)
        {
            var size = segment.Size;
            if(size >= Consts.MinimumSizeForParallel)
                Parallel.For(0, size, i => analyser(segment[i], (uint)i));
            else {
                for (uint i = 0; i < size; i++)
                    analyser(segment[i], i);
            }
        }

        public static void L1Regularisation(this ITensorSegment segment, float coefficient)
        {
            for (uint i = 0, len = segment.Size; i < len; i++) {
                var val = segment[i];
                segment[i] = val - (val > 0 ? 1 : val < 0 ? -1 : 0) * coefficient;
            }
        }

        public static float FindDistance(this IVector vector, IVector other, DistanceMetric distance) => distance switch {
            DistanceMetric.Cosine => vector.CosineDistance(other),
            DistanceMetric.Euclidean => vector.EuclideanDistance(other),
            DistanceMetric.Manhattan => vector.ManhattanDistance(other),
            DistanceMetric.MeanSquared => vector.MeanSquaredDistance(other),
            DistanceMetric.SquaredEuclidean => vector.SquaredEuclideanDistance(other),
            _ => throw new NotImplementedException(distance.ToString())
        };

        public static IVector FindDistances(this IVector compareTo, IReadOnlyList<IVector> vectors, DistanceMetric distanceMetric)
        {
            var size = (uint)vectors.Count;
            var lap = compareTo.LinearAlgebraProvider;
            var ret = lap.CreateVector(size);
            if (size >= Consts.MinimumSizeForParallel) {
                Parallel.For(0, size, ind => {
                    lap.BindThread();
                    ret[ind] = FindDistance(compareTo, vectors[(int)ind], distanceMetric);
                });
            }
            else {
                for(var i = 0; i < size; i++)
                    ret[i] = FindDistance(compareTo, vectors[i], distanceMetric);
            }

            return ret;
        }

        public static void Set(this ITensorSegment segment, Func<uint, float> getValue)
        {
            for (uint i = 0, len = segment.Size; i < len; i++)
                segment[i] = getValue(i);
        }

        public static void Set(this ITensorSegment segment, float value)
        {
            for (uint i = 0, len = segment.Size; i < len; i++)
                segment[i] = value;
        }

        public static void SetToRandom(this ITensorSegment segment, Random random)
        {
            for (uint i = 0, len = segment.Size; i < len; i++)
                segment[i] = System.Convert.ToSingle(random.NextDouble());
        }

        public static WeightedIndexList ToSparse(this IVector vector)
        {
            return WeightedIndexList.Create(vector.Segment.Values
                .Select((v, i) => new WeightedIndexList.Item((uint)i, v))
                .Where(d => FloatMath.IsNotZero(d.Weight))
            );
        }

        public static IVectorInfo ToVectorInfo(this ITensorSegment segment) => new VectorInfo(segment);

        public static IVector ToVector(this ITensorSegment segment, LinearAlgebraProvider lap) => lap.CreateVector(segment);
        public static IMatrix ToMatrix(this ITensorSegment segment, LinearAlgebraProvider lap, uint rows, uint columns) => lap.CreateMatrix(rows, columns, segment);
        public static ITensor3D ToTensor3D(this ITensorSegment segment, LinearAlgebraProvider lap, uint depth, uint rows, uint columns) => lap.CreateTensor3D(depth, rows, columns, segment);
        public static ITensor4D ToTensor4D(this ITensorSegment segment, LinearAlgebraProvider lap, uint count, uint depth, uint rows, uint columns) => lap.CreateTensor4D(count, depth, rows, columns, segment);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static void CopyTo(this ITensor2 tensor, ITensor2 other) => tensor.Segment.CopyTo(other.Segment);

        public static LinearAlgebraProvider UseDefaultLinearAlgebraProvider(this BrightDataContext context)
        {
            var ret = new LinearAlgebraProvider(context);
            ((ISetLinearAlgebraProvider) context).LinearAlgebraProvider = ret;
            return ret;
        }
    }
}
