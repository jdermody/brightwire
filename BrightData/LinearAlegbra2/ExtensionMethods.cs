using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;
using BrightData.Helper;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlegbra2
{
    public static class ExtensionMethods
    {
        public static float[] GetLocalOrNewArray(this ITensorSegment2 segment) => segment.GetArrayForLocalUseOnly() ?? segment.ToNewArray();

        public static Span<float> GetSpan(this ITensorSegment2 segment, uint startPosition = 0, uint endPosition = uint.MaxValue)
        {
            var end = Math.Min(segment.Size, endPosition);
            var size = (int)(end - startPosition);
            var localArray = segment.GetArrayForLocalUseOnly();
            if (localArray != null)
                return new Span<float>(localArray, (int)startPosition, size);

            // create a new array
            var values = segment.Values;
            if (startPosition > 0)
                values = values.Skip((int)startPosition);
            var newArray = values.Take(size).ToArray();
            return new Span<float>(newArray);
        }

        public static System.Numerics.Vector<float> AsNumericsVector(this ITensorSegment2 segment, int start)
        {
            var ptr = segment.GetSpan((uint)start);
            return new System.Numerics.Vector<float>(ptr);
        }

        static MemoryOwner<float> Allocate(uint size) => MemoryOwner<float>.Allocate((int)size);

        public static ITensorSegment2 ZipParallel(this ITensorSegment2 segment, ITensorSegment2 other, Func<float, float, float> func)
        {
            if (segment.Size != other.Size)
                throw new ArgumentException("Segments were different sizes");

            var ret = Allocate(segment.Size);
            var array = ret.DangerousGetArray();
            Parallel.ForEach(segment.Values, (v, _, i) => { array[(int)i] = func(v, other[i]); });
            return new TensorSegment2(ret);
        }

        public static ITensorSegment2 ZipVectorised(
            this ITensorSegment2 segment, 
            ITensorSegment2 other, 
            Func<System.Numerics.Vector<float>, System.Numerics.Vector<float>, System.Numerics.Vector<float>> func1,
            Func<float, float, float> func2)
        {
            var size = segment.Size;
            if (size != other.Size)
                throw new ArgumentException("Segments were different sizes");

            var ret = Allocate(size);
            var ptr = ret.Span;
            var vectorSize = System.Numerics.Vector<float>.Count;
            var i = 0;
            if (size >= vectorSize) {
                for (; i <= size - vectorSize; i += vectorSize) {
                    var s1 = segment.AsNumericsVector(i);
                    var s2 = other.AsNumericsVector(i);
                    func1(s1, s2).CopyTo(ptr.Slice(i));
                }
            }
            for (; i < size; i++)
                ptr[i] = func2(segment[i], other[i]);
            return new TensorSegment2(ret);
        }

        public static ITensorSegment2 TransformParallel(this ITensorSegment2 segment, Func<float, float> transfomer)
        {
            var ret = Allocate(segment.Size);
            var array = ret.DangerousGetArray();
            Parallel.ForEach(segment.Values, (v, _, i) => { array[(int)i] = transfomer(v); });
            return new TensorSegment2(ret);
        }

        public static ITensorSegment2 TransformVectorised(this ITensorSegment2 segment, Func<System.Numerics.Vector<float>, System.Numerics.Vector<float>> transfomer1, Func<float, float> transfomer2)
        {
            var ret = Allocate(segment.Size);
            var ptr = ret.Span;
            var vectorSize = System.Numerics.Vector<float>.Count;
            var size = segment.Size;
            var i = 0;
            if (size >= vectorSize) {
                for (; i <= size - vectorSize; i += vectorSize) {
                    var s1 = segment.AsNumericsVector((int)i);
                    transfomer1(s1).CopyTo(ptr[i..]);
                }
            }
            for (; i < size; i++)
                ptr[i] = transfomer2(segment[i]);
            return new TensorSegment2(ret);
        }

        public static ITensorSegment2 TransformParallelIndexed(this ITensorSegment2 segment, Func<uint, float> transfomer)
        {
            var ret = Allocate(segment.Size);
            var array = ret.DangerousGetArray();
            Parallel.ForEach(segment.Values, (_, _, i) => { array[(int)i] = transfomer((uint)i); });
            return new TensorSegment2(ret);
        }

        public static void Mutate(this ITensorSegment2 segment, ITensorSegment2 other, Func<float, float, float> func)
        {
            if (segment.Size != other.Size)
                throw new ArgumentException("Segments were different sizes");

            Parallel.ForEach(segment.Values, (v, _, i) => { segment[i] = func(v, other[(int)i]); });
        }

        public static void MutateVectorised(this ITensorSegment2 segment, ITensorSegment2 other, Func<System.Numerics.Vector<float>, System.Numerics.Vector<float>, System.Numerics.Vector<float>> func1, Func<float, float, float> func2)
        {
            if (segment.Size != other.Size)
                throw new ArgumentException("Segments were different sizes");

            using var ret = SpanOwner<float>.Allocate((int)segment.Size);
            var ptr = ret.Span;
            var vectorSize = System.Numerics.Vector<float>.Count;
            var size = segment.Size;
            var i = 0;
            if (size >= vectorSize) {
                for (; i <= size - vectorSize; i += vectorSize) {
                    var s1 = segment.AsNumericsVector(i);
                    var s2 = other.AsNumericsVector(i);
                    func1(s1, s2).CopyTo(ptr[i..]);
                }
            }
            for (; i < size; i++)
                ptr[i] = func2(segment[i], other[i]);

            segment.CopyFrom(ptr);
        }

        public static void MutateInPlace(this ITensorSegment2 segment, Func<float, float> mutator)
        {
            Parallel.ForEach(segment.Values, (v, _, i) => { segment[i] = mutator(v); });
        }

        public static void MutateInPlaceVectorised(this ITensorSegment2 segment, Func<System.Numerics.Vector<float>, System.Numerics.Vector<float>> mutator1, Func<float, float> mutator2)
        {
            using var ret = SpanOwner<float>.Allocate((int)segment.Size);
            var ptr = ret.Span;
            var vectorSize = System.Numerics.Vector<float>.Count;
            var size = segment.Size;
            var i = 0;
            if (size >= vectorSize) {
                for (; i <= size - vectorSize; i += vectorSize) {
                    var s1 = segment.AsNumericsVector(i);
                    mutator1(s1).CopyTo(ptr[i..]);
                }
            }
            for (; i < size; i++)
                ptr[i] = mutator2(segment[i]);

            segment.CopyFrom(ptr);
        }

        public static unsafe float Sum(this ITensorSegment2 segment)
        {
            if (Sse3.IsSupported) {
                float result;
                var size = segment.Size;

                fixed (float* pSource = segment.GetLocalOrNewArray()) {
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

                return result;
            }

            return segment.Values.AsParallel().Sum();
        }

        public static ITensorSegment2 Add(this ITensorSegment2 tensor1, ITensorSegment2 tensor2) => ZipVectorised(tensor1, tensor2, (a, b) => a+b, (a, b) => a + b);
        public static ITensorSegment2 Add(this ITensorSegment2 tensor1, ITensorSegment2 tensor2, float coefficient1, float coefficient2) => ZipVectorised(tensor1, tensor2, (a, b) => a * coefficient1 + b * coefficient2, (a, b) => a * coefficient1 + b * coefficient2);
        public static ITensorSegment2 Add(this ITensorSegment2 tensor, float scalar)
        {
            var scalarVector = new System.Numerics.Vector<float>(scalar);
            return TransformVectorised(tensor, a => a + scalarVector, a => a + scalar);
        }

        public static void AddInPlace(this ITensorSegment2 target, ITensorSegment2 other) => MutateVectorised(target, other, (a, b) => a + b, (a, b) => a + b);
        public static void AddInPlace(this ITensorSegment2 target, ITensorSegment2 other, float coefficient1, float coefficient2) => MutateVectorised(target, other, (a,b) => a * coefficient1 + b * coefficient2, (a,b) => a * coefficient1 + b * coefficient2);

        public static void AddInPlace(this ITensorSegment2 target, float scalar)
        {
            var scalarVector = new System.Numerics.Vector<float>(scalar);
            MutateInPlaceVectorised(target, v => v + scalarVector, v => v + scalar);
        }

        public static void MultiplyInPlace(this ITensorSegment2 target, float scalar)
        {
            var scalarVector = new System.Numerics.Vector<float>(scalar);
            MutateInPlaceVectorised(target, v => v * scalarVector, v => v * scalar);
        }

        public static ITensorSegment2 Multiply(this ITensorSegment2 target, float scalar)
        {
            var scalarVector = new System.Numerics.Vector<float>(scalar);
            return TransformVectorised(target, v => v * scalarVector, v => v * scalar);
        } 

        public static ITensorSegment2 Subtract(this ITensorSegment2 tensor1, ITensorSegment2 tensor2) => ZipVectorised(tensor1, tensor2, (a, b) => a - b, (a, b) => a - b);
        public static ITensorSegment2 Subtract(this ITensorSegment2 tensor1, ITensorSegment2 tensor2, float coefficient1, float coefficient2) => ZipVectorised(tensor1, tensor2, (a, b) => a * coefficient1 - b * coefficient2, (a, b) => a * coefficient1 - b * coefficient2);

        public static void SubtractInPlace(this ITensorSegment2 target, ITensorSegment2 other) => MutateVectorised(target, other, (a, b) => a - b, (a, b) => a - b);
        public static void SubtractInPlace(this ITensorSegment2 target, ITensorSegment2 other, float coefficient1, float coefficient2) => MutateVectorised(target, other, (a, b) => a * coefficient1 - b * coefficient2, (a, b) => a * coefficient1 - b * coefficient2);

        public static ITensorSegment2 PointwiseMultiply(this ITensorSegment2 tensor1, ITensorSegment2 tensor2) => ZipVectorised(tensor1, tensor2, (a, b) => a * b, (a, b) => a * b);

        public static void PointwiseMultiplyInPlace(this ITensorSegment2 target, ITensorSegment2 other) => MutateVectorised(target, other, (a, b) => a * b, (a, b) => a * b);
        public static ITensorSegment2 PointwiseDivide(this ITensorSegment2 tensor1, ITensorSegment2 tensor2) => ZipVectorised(tensor1, tensor2, (a, b) => a / b, (a, b) => a / b);

        public static void PointwiseDivideInPlace(this ITensorSegment2 target, ITensorSegment2 other) => MutateVectorised(target, other, (a, b) => a / b, (a, b) => a / b);

        public static float DotProduct(this ITensorSegment2 segment, ITensorSegment2 other)
        {
            var product = PointwiseMultiply(segment, other);
            try {
                return Sum(product);
            }
            finally {
                product.Release();
            }
        }

        public static ITensorSegment2 Sqrt(this ITensorSegment2 tensor) => TransformParallel(tensor, MathF.Sqrt);

        public static uint? Search(this ITensorSegment2 segment, float value)
        {
            uint? ret = null;
            Analyse(segment, (v, index) => {
                if (Math.Abs(value - v) < FloatMath.AlmostZero)
                    ret = index;
            });
            return ret;
        }

        public static void ConstrainInPlace(this ITensorSegment2 segment, float? minValue, float? maxValue)
        {
            MutateInPlace(segment, value => {
                if (minValue.HasValue && value.CompareTo(minValue.Value) < 0)
                    return minValue.Value;
                if (maxValue.HasValue && value.CompareTo(maxValue.Value) > 0)
                    return maxValue.Value;
                return value;
            });
        }

        public static float Average(this ITensorSegment2 segment) => Sum(segment) / segment.Size;

        public static float L1Norm(this ITensorSegment2 segment)
        {
            var abs = Abs(segment);
            try {
                return Sum(abs);
            }
            finally {
                abs.Release();
            }
        }

        public static float L2Norm(this ITensorSegment2 segment)
        {
            var squared = Squared(segment);
            try {
                return MathF.Sqrt(Sum(squared));
            }
            finally {
                squared.Release();
            }
        }

        public static (float Min, float Max, uint MinIndex, uint MaxIndex) GetMinAndMaxValues(this ITensorSegment2 segment)
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

        public static bool IsEntirelyFinite(this ITensorSegment2 segment) => !segment.Values.Any(v => float.IsNaN(v) || float.IsInfinity(v));

        public static ITensorSegment2 Reverse(this ITensorSegment2 segment)
        {
            var len = segment.Size - 1;
            return TransformParallelIndexed(segment, i => segment[len - i]);
        }

        public static IEnumerable<ITensorSegment2> Split(this ITensorSegment2 segment, uint blockCount)
        {
            for (uint i = 0, size = segment.Size, blockSize = size / blockCount; i < size; i += blockSize)
                yield return new TensorSegmentWrapper2(segment, i, 1, blockSize);
        }

        public static float CosineDistance(this ITensorSegment2 tensor, ITensorSegment2 other)
        {
            var ab = DotProduct(tensor, other);
            var aa = DotProduct(tensor, tensor);
            var bb = DotProduct(other, other);
            return 1f - ab / (MathF.Sqrt(aa) * MathF.Sqrt(bb));
        }

        public static float EuclideanDistance(this ITensorSegment2 tensor, ITensorSegment2 other)
        {
            var distance = Subtract(tensor, other);
            try {
                var squared = Squared(distance);
                try {
                    return MathF.Sqrt(Sum(squared));
                }
                finally {
                    squared.Release();
                }
            }
            finally {
                distance.Release();
            }
        }

        public static float MeanSquaredDistance(this ITensorSegment2 tensor, ITensorSegment2 other)
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

        public static float SquaredEuclideanDistance(this ITensorSegment2 tensor, ITensorSegment2 other)
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

        public static float ManhattanDistance(this ITensorSegment2 tensor, ITensorSegment2 other)
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

        public static ITensorSegment2 Abs(this ITensorSegment2 tensor) => TransformParallel(tensor, MathF.Abs);
        public static ITensorSegment2 Log(this ITensorSegment2 tensor) => TransformParallel(tensor, MathF.Log);
        public static ITensorSegment2 Exp(this ITensorSegment2 tensor) => TransformParallel(tensor, MathF.Exp);
        public static ITensorSegment2 Squared(this ITensorSegment2 tensor) => TransformVectorised(tensor, v => v * v, v => v * v);

        public static float StdDev(this ITensorSegment2 segment, float? mean)
        {
            var avg = mean ?? Average(segment);
            var avgVector = new System.Numerics.Vector<float>(avg);
            var result = TransformVectorised(segment, v => {
                var s = v - avgVector;
                return s * s;
            }, v => {
                var s = v - avg;
                return s * s;
            });
            try {
                return MathF.Sqrt(Average(result));
            }
            finally {
                result.Release();
            }
        }

        static float Sigmoid(float val) => FloatMath.Constrain(1.0f / (1.0f + MathF.Exp(-1.0f * val)));
        static float SigmoidDerivative(float val)
        {
            var sigmoid = Sigmoid(val);
            return FloatMath.Constrain(sigmoid * (1.0f - sigmoid));
        }
        static float Tanh(float val) => MathF.Tanh(val);
        static float TanhDerivative(float val) => 1.0f - MathF.Pow(Tanh(val), 2);
        static float Relu(float val) => (val <= 0) ? 0 : FloatMath.Constrain(val);
        static float ReluDerivative(float val) => (val <= 0) ? 0f : 1;
        static float LeakyRelu(float val) => (val <= 0) ? 0.01f * val : FloatMath.Constrain(val);
        static float LeakyReluDerivative(float val) => (val <= 0) ? 0.01f : 1;

        public static ITensorSegment2 Sigmoid(this ITensorSegment2 segment) => TransformParallel(segment, Sigmoid);
        public static ITensorSegment2 SigmoidDerivative(this ITensorSegment2 segment) => TransformParallel(segment, SigmoidDerivative);
        public static ITensorSegment2 Tanh(this ITensorSegment2 segment) => TransformParallel(segment, Tanh);
        public static ITensorSegment2 TanhDerivative(this ITensorSegment2 segment) => TransformParallel(segment, TanhDerivative);
        public static ITensorSegment2 Relu(this ITensorSegment2 segment) => TransformParallel(segment, Relu);
        public static ITensorSegment2 ReluDerivative(this ITensorSegment2 segment) => TransformParallel(segment, ReluDerivative);
        public static ITensorSegment2 LeakyRelu(this ITensorSegment2 segment) => TransformParallel(segment, LeakyRelu);
        public static ITensorSegment2 LeakyReluDerivative(this ITensorSegment2 segment) => TransformParallel(segment, LeakyReluDerivative);

        public static ITensorSegment2 CherryPickIndices(this ITensorSegment2 segment, uint[] arrayIndices)
        {
            var ret = MemoryOwner<float>.Allocate(arrayIndices.Length);
            var ptr = ret.Span;
            for (int i = 0, len = arrayIndices.Length; i < len; i++)
                ptr[i] = segment[arrayIndices[i]];
            return new TensorSegment2(ret);
        }

        public static ITensorSegment2 Softmax(this ITensorSegment2 segment)
        {
            var (_, max, _, _) = GetMinAndMaxValues(segment);

            var softmax = TransformParallel(segment, v => MathF.Exp(v - max));
            var sum = Sum(softmax);
            if (FloatMath.IsNotZero(sum)) {
                var ret = TransformParallel(softmax, v => v / sum);
                softmax.Release();
                return ret;
            }

            return softmax;
        }

        public static IMatrix SoftmaxDerivative(this ITensorSegment2 segment, ComputationUnit computationUnit)
        {
            return computationUnit.CreateMatrix(segment.Size, segment.Size, (x, y) => x == y
                ? segment[x] * (1 - segment[x])
                : -segment[x] * segment[y]
            );
        }

        public static ITensorSegment2 Pow(this ITensorSegment2 segment, float power) => TransformParallel(segment, v => FloatMath.Pow(v, power));
        public static void RoundInPlace(this ITensorSegment2 segment, float lower, float upper, float? mid)
        {
            var compareTo = mid ?? lower + (upper - lower) / 2;
            MutateInPlace(segment, v => v >= compareTo ? upper : lower);
        }

        static void Analyse(ITensorSegment2 segment, Action<float, uint> analyser)
        {
            Parallel.ForEach(segment.Values, (v, _, i) => { analyser(v, (uint)i); });
        }

        static void FeatureScaleNormalization(this ITensorSegment2 segment)
        {
            var (min, max, _, _) = GetMinAndMaxValues(segment);
            var range = max - min;
        }
    }
}
