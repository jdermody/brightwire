using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using BrightData.Helper;
using BrightData.LinearAlegbra2;
using BrightData.LinearAlgebra;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData
{
    public static partial class ExtensionMethods
    {
        public static float[] GetLocalOrNewArray(this ITensorSegment2 segment) => segment.GetArrayForLocalUseOnly() ?? segment.ToNewArray();

        //public static Span<float> GetSpan(this ITensorSegment2 segment)
        //{
        //    var localArray = segment.GetArrayForLocalUseOnly();
        //    return localArray != null 
        //        ? new Span<float>(localArray, 0, (int)segment.Size) 
        //        : new Span<float>(segment.ToNewArray());
        //}

        //public static ReadOnlySpan<float> GetSpan(this ITensorSegment2 segment, uint startPosition)
        //{
        //    var start = (int)startPosition;
        //    var end = (int)(segment.Size - startPosition);
        //    var localArray = segment.GetArrayForLocalUseOnly();
            
        //    return localArray != null 
        //        ? new ReadOnlySpan<float>(localArray, start, end) 
        //        : new ReadOnlySpan<float>(segment.ToNewArray(), start, end);
        //}

        public static ReadOnlySpan<float> GetSpan(this ITensorSegment2 segment, ref SpanOwner<float> temp, out bool wasTempUsed)
        {
            ReadOnlySpan<float> ret;
            var array = segment.GetArrayForLocalUseOnly();
            if (array == null) {
                temp = SpanOwner<float>.Allocate((int)segment.Size);
                var span = temp.Span;
                segment.CopyTo(span);
                ret = span;
                wasTempUsed = true;
            }
            else {
                ret = array;
                wasTempUsed = false;
            }

            return ret[..(int)segment.Size];
        }

        static MemoryOwner<float> Allocate(uint size) => MemoryOwner<float>.Allocate((int)size);
        internal static readonly int NumericsVectorSize = System.Numerics.Vector<float>.Count;

        public static ITensorSegment2 ZipParallel(this ITensorSegment2 segment, ITensorSegment2 other, Func<float, float, float> func)
        {
            if (segment.Size != other.Size)
                throw new ArgumentException("Segments were different sizes");

            var ret = Allocate(segment.Size);
            var array = ret.DangerousGetArray();
            Parallel.ForEach(segment.Values, (v, _, i) => { array[(int)i] = func(v, other[i]); });
            return new ArrayPoolTensorSegment(ret);
        }

        public delegate void ComputeVectorisedTwo(in System.Numerics.Vector<float> a, in System.Numerics.Vector<float> b, out System.Numerics.Vector<float> r);
        public delegate void ComputeVectorisedOne(in System.Numerics.Vector<float> a, out System.Numerics.Vector<float> r);
        public static ITensorSegment2 ZipVectorised(
            this ITensorSegment2 segment, 
            ITensorSegment2 other, 
            ComputeVectorisedTwo func1,
            Func<float, float, float> func2)
        {
            var size = segment.Size;
            if (size != other.Size)
                throw new ArgumentException("Segments were different sizes");

            // get pointers to the segments
            SpanOwner<float> leftTemp = SpanOwner<float>.Empty, rightTemp = SpanOwner<float>.Empty;
            var leftPtr = segment.GetSpan(ref leftTemp, out var wasLefTempUsed);
            var rightPtr = other.GetSpan(ref rightTemp, out var wasRightTempUsed);
            try {
                var leftVec = MemoryMarshal.Cast<float, System.Numerics.Vector<float>>(leftPtr);
                var rightVec = MemoryMarshal.Cast<float, System.Numerics.Vector<float>>(rightPtr);

                var ret = Allocate(size);
                var resultPtr = ret.Span;
                var resultVec = MemoryMarshal.Cast<float, System.Numerics.Vector<float>>(resultPtr);
                var numVectors = (int)size / NumericsVectorSize;
                var ceiling = numVectors * NumericsVectorSize;

                for (var i = 0; i < numVectors; i++)
                    func1(leftVec[i], rightVec[i], out resultVec[i]);
                for (var i = ceiling; i < size; i++)
                    resultPtr[i] = func2(leftPtr[i], rightPtr[i]);

                return new ArrayPoolTensorSegment(ret);
            }
            finally {
                if(wasLefTempUsed)
                    leftTemp.Dispose();
                if(wasRightTempUsed)
                    rightTemp.Dispose();
            }
        }

        public static ITensorSegment2 TransformParallel(this ITensorSegment2 segment, Func<float, float> transfomer)
        {
            var ret = Allocate(segment.Size);
            var array = ret.DangerousGetArray();
            Parallel.ForEach(segment.Values, (v, _, i) => { array[(int)i] = transfomer(v); });
            return new ArrayPoolTensorSegment(ret);
        }

        public static ITensorSegment2 TransformVectorised(
            this ITensorSegment2 segment, 
            ComputeVectorisedOne transfomer1, 
            Func<float, float> transfomer2)
        {
            var size = segment.Size;
            var leftTemp = SpanOwner<float>.Empty;
            var leftPtr = segment.GetSpan(ref leftTemp, out var wasLeftTempUsed);

            try {
                var leftVec = MemoryMarshal.Cast<float, System.Numerics.Vector<float>>(leftPtr);

                var ret = Allocate(segment.Size);
                var resultPtr = ret.Span;
                var resultVec = MemoryMarshal.Cast<float, System.Numerics.Vector<float>>(resultPtr);
                var numVectors = (int)size / NumericsVectorSize;
                var ceiling = numVectors * NumericsVectorSize;

                for (var i = 0; i < numVectors; i++)
                    transfomer1(leftVec[i], out resultVec[i]);
                for (var i = ceiling; i < size; i++)
                    resultPtr[i] = transfomer2(leftPtr[i]);

                return new ArrayPoolTensorSegment(ret);
            }
            finally {
                if(wasLeftTempUsed)
                    leftTemp.Dispose();
            }
        }

        public static ITensorSegment2 TransformParallelIndexed(this ITensorSegment2 segment, Func<uint, float> transfomer)
        {
            var ret = Allocate(segment.Size);
            var array = ret.DangerousGetArray();
            Parallel.ForEach(segment.Values, (_, _, i) => { array[(int)i] = transfomer((uint)i); });
            return new ArrayPoolTensorSegment(ret);
        }

        public static void Mutate(this ITensorSegment2 segment, ITensorSegment2 other, Func<float, float, float> func)
        {
            if (segment.Size != other.Size)
                throw new ArgumentException("Segments were different sizes");

            Parallel.ForEach(segment.Values, (v, _, i) => { segment[i] = func(v, other[(int)i]); });
        }

        public static void MutateVectorised(
            this ITensorSegment2 segment, 
            ITensorSegment2 other, 
            ComputeVectorisedTwo func1, 
            Func<float, float, float> func2)
        {
            var size = segment.Size;
            if (size != other.Size)
                throw new ArgumentException("Segments were different sizes");

            // get pointers to the segments
            SpanOwner<float> leftTemp = SpanOwner<float>.Empty, rightTemp = SpanOwner<float>.Empty;
            var leftPtr = segment.GetSpan(ref leftTemp, out var wasLeftTempUsed);
            var rightPtr = other.GetSpan(ref rightTemp, out var wasRightTempUsed);
            try {
                var leftVec = MemoryMarshal.Cast<float, System.Numerics.Vector<float>>(leftPtr);
                var rightVec = MemoryMarshal.Cast<float, System.Numerics.Vector<float>>(rightPtr);

                using var ret = SpanOwner<float>.Allocate((int)segment.Size);
                var resultPtr = ret.Span;
                var resultVec = MemoryMarshal.Cast<float, System.Numerics.Vector<float>>(resultPtr);
                var numVectors = (int)size / NumericsVectorSize;
                var ceiling = numVectors * NumericsVectorSize;

                for (var i = 0; i < numVectors; i++)
                    func1(leftVec[i], rightVec[i], out resultVec[i]);
                for (var i = ceiling; i < size; i++)
                    resultPtr[i] = func2(leftPtr[i], rightPtr[i]);

                segment.CopyFrom(resultPtr);
            }
            finally {
                if(wasLeftTempUsed)
                    leftTemp.Dispose();
                if(wasRightTempUsed)
                    rightTemp.Dispose();
            }
        }

        public static void MutateInPlace(this ITensorSegment2 segment, Func<float, float> mutator)
        {
            Parallel.ForEach(segment.Values, (v, _, i) => { segment[i] = mutator(v); });
        }

        public static void MutateInPlaceVectorised(
            this ITensorSegment2 segment, 
            ComputeVectorisedOne mutator1, 
            Func<float, float> mutator2)
        {
            var size = segment.Size;
            var leftTemp = SpanOwner<float>.Empty;
            var leftPtr = segment.GetSpan(ref leftTemp, out var wasLeftTempUsed);

            try {
                var leftVec = MemoryMarshal.Cast<float, System.Numerics.Vector<float>>(leftPtr);
                using var ret = SpanOwner<float>.Allocate((int)segment.Size);
                var resultPtr = ret.Span;
                var resultVec = MemoryMarshal.Cast<float, System.Numerics.Vector<float>>(resultPtr);
                var numVectors = (int)size / NumericsVectorSize;
                var ceiling = numVectors * NumericsVectorSize;

                for (var i = 0; i < numVectors; i++)
                    mutator1(leftVec[i], out resultVec[i]);
                for (var i = ceiling; i < size; i++)
                    resultPtr[i] = mutator2(leftPtr[i]);

                segment.CopyFrom(resultPtr);
            }
            finally {
                if(wasLeftTempUsed)
                    leftTemp.Dispose();
            }
        }

        public static unsafe float Sum(this Span<float> span) => Sum((ReadOnlySpan<float>)span);
        public static unsafe float Sum(this ReadOnlySpan<float> span)
        {
            var result = 0f;
            if (Sse3.IsSupported) {
                var size = span.Length;
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

        public static float Sum(this ITensorSegment2 segment)
        {
            if (Sse3.IsSupported) {
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

            return segment.Values/*.AsParallel()*/.Sum();
        }

        public static ITensorSegment2 Add(this ITensorSegment2 tensor1, ITensorSegment2 tensor2) => ZipVectorised(
            tensor1, 
            tensor2, 
            (in System.Numerics.Vector<float> a, in System.Numerics.Vector<float> b, out System.Numerics.Vector<float> r) => r = a + b, 
            (a, b) => a + b
        );
        public static ITensorSegment2 Add(this ITensorSegment2 tensor1, ITensorSegment2 tensor2, float coefficient1, float coefficient2) => ZipVectorised(
            tensor1, 
            tensor2, 
            (in System.Numerics.Vector<float> a, in System.Numerics.Vector<float> b, out System.Numerics.Vector<float> r) => r = a * coefficient1 + b * coefficient2,
            (a, b) => a * coefficient1 + b * coefficient2
        );
        public static ITensorSegment2 Add(this ITensorSegment2 tensor, float scalar)
        {
            var scalarVector = new System.Numerics.Vector<float>(scalar);
            return TransformVectorised(
                tensor, 
                (in System.Numerics.Vector<float> a, out System.Numerics.Vector<float> r) => r = a + scalarVector, 
                a => a + scalar
            );
        }

        public static void AddInPlace(this ITensorSegment2 target, ITensorSegment2 other) => MutateVectorised(
            target, 
            other, 
            (in System.Numerics.Vector<float> a, in System.Numerics.Vector<float> b, out System.Numerics.Vector<float> r) => r = a + b, 
            (a, b) => a + b
        );
        public static void AddInPlace(this ITensorSegment2 target, ITensorSegment2 other, float coefficient1, float coefficient2) => MutateVectorised(
            target, 
            other, 
            (in System.Numerics.Vector<float> a, in System.Numerics.Vector<float> b, out System.Numerics.Vector<float> r) => r = (a * coefficient1) + (b * coefficient2), 
            (a,b) => (a * coefficient1) + (b * coefficient2)
        );

        public static void AddInPlace(this ITensorSegment2 target, float scalar)
        {
            var scalarVector = new System.Numerics.Vector<float>(scalar);
            MutateInPlaceVectorised(
                target, 
                (in System.Numerics.Vector<float> a, out System.Numerics.Vector<float> r) => r = a + scalarVector, 
                a => a + scalar
            );
        }

        public static void MultiplyInPlace(this ITensorSegment2 target, float scalar)
        {
            var scalarVector = new System.Numerics.Vector<float>(scalar);
            MutateInPlaceVectorised(
                target, 
                (in System.Numerics.Vector<float> a, out System.Numerics.Vector<float> r) => r = a * scalarVector, 
                a => a * scalar
            );
        }

        public static ITensorSegment2 Multiply(this ITensorSegment2 target, float scalar)
        {
            var scalarVector = new System.Numerics.Vector<float>(scalar);
            return TransformVectorised(
                target, 
                (in System.Numerics.Vector<float> a, out System.Numerics.Vector<float> r) => r = a * scalarVector, 
                a => a * scalar
            );
        } 

        public static ITensorSegment2 Subtract(this ITensorSegment2 tensor1, ITensorSegment2 tensor2) => ZipVectorised(
            tensor1, 
            tensor2, 
            (in System.Numerics.Vector<float> a, in System.Numerics.Vector<float> b, out System.Numerics.Vector<float> r) => r = a - b, 
            (a, b) => a - b
        );
        public static ITensorSegment2 Subtract(this ITensorSegment2 tensor1, ITensorSegment2 tensor2, float coefficient1, float coefficient2) => ZipVectorised(
            tensor1, 
            tensor2, 
            (in System.Numerics.Vector<float> a, in System.Numerics.Vector<float> b, out System.Numerics.Vector<float> r) => r = a * coefficient1 - b * coefficient2, 
            (a, b) => a * coefficient1 - b * coefficient2
        );

        public static void SubtractInPlace(this ITensorSegment2 target, ITensorSegment2 other) => MutateVectorised(
            target, 
            other, 
            (in System.Numerics.Vector<float> a, in System.Numerics.Vector<float> b, out System.Numerics.Vector<float> r) => r = a - b, 
            (a, b) => a - b
        );
        public static void SubtractInPlace(this ITensorSegment2 target, ITensorSegment2 other, float coefficient1, float coefficient2) => MutateVectorised(
            target, 
            other, 
            (in System.Numerics.Vector<float> a, in System.Numerics.Vector<float> b, out System.Numerics.Vector<float> r) => r = a * coefficient1 - b * coefficient2, 
            (a, b) => a * coefficient1 - b * coefficient2
        );

        public static ITensorSegment2 PointwiseMultiply(this ITensorSegment2 tensor1, ITensorSegment2 tensor2) => ZipVectorised(
            tensor1, 
            tensor2, 
            (in System.Numerics.Vector<float> a, in System.Numerics.Vector<float> b, out System.Numerics.Vector<float> r) => r = a * b, 
            (a, b) => a * b
        );

        public static void PointwiseMultiplyInPlace(this ITensorSegment2 target, ITensorSegment2 other) => MutateVectorised(
            target, 
            other, 
            (in System.Numerics.Vector<float> a, in System.Numerics.Vector<float> b, out System.Numerics.Vector<float> r) => r = a * b, 
            (a, b) => a * b
        );
        public static ITensorSegment2 PointwiseDivide(this ITensorSegment2 tensor1, ITensorSegment2 tensor2) => ZipVectorised(
            tensor1, 
            tensor2, 
            (in System.Numerics.Vector<float> a, in System.Numerics.Vector<float> b, out System.Numerics.Vector<float> r) => r = a / b, 
            (a, b) => a / b
        );

        public static void PointwiseDivideInPlace(this ITensorSegment2 target, ITensorSegment2 other) => MutateVectorised(
            target, 
            other, 
            (in System.Numerics.Vector<float> a, in System.Numerics.Vector<float> b, out System.Numerics.Vector<float> r) => r = a / b, 
            (a, b) => a / b
        );

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

        public static ITensorSegment2 Sqrt(this ITensorSegment2 tensor) => TransformParallel(tensor, x => FloatMath.Sqrt(x));

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
                return FloatMath.Sqrt(Sum(squared));
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
            return 1f - ab / (FloatMath.Sqrt(aa) * FloatMath.Sqrt(bb));
        }

        public static float EuclideanDistance(this ITensorSegment2 tensor, ITensorSegment2 other)
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
        public static ITensorSegment2 Squared(this ITensorSegment2 tensor) => TransformVectorised(
            tensor, 
            (in System.Numerics.Vector<float> a, out System.Numerics.Vector<float> r) => r = a * a, 
            a => a * a
        );

        public static float StdDev(this ITensorSegment2 segment, float? mean)
        {
            var avg = mean ?? Average(segment);
            var avgVector = new System.Numerics.Vector<float>(avg);
            var result = TransformVectorised(
                segment, 
                (in System.Numerics.Vector<float> a, out System.Numerics.Vector<float> r) => {
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
            return new ArrayPoolTensorSegment(ret);
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

        public static IMatrix SoftmaxDerivative(this ITensorSegment2 segment, LinearAlgebraProvider computationUnit)
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

        public static void Analyse(ITensorSegment2 segment, Action<float, uint> analyser)
        {
            Parallel.ForEach(segment.Values, (v, _, i) => { analyser(v, (uint)i); });
        }

        //public static void FeatureScaleNormalization(this ITensorSegment2 segment)
        //{
        //    var (min, max, _, _) = GetMinAndMaxValues(segment);
        //    var range = max - min;
        //}

        public static void L1Regularisation(this ITensorSegment2 segment, float coefficient)
        {
            for (uint i = 0, len = segment.Size; i < len; i++) {
                var val = segment[i];
                segment[i] = val - (val > 0 ? 1 : val < 0 ? -1 : 0) * coefficient;
            }
        }

        //public static IVector ToArrayBased(this IVector vector)
        //{
        //    if (vector.Segment.SegmentType == Consts.ArrayBased)
        //        return vector;

        //    var lap = vector.Context.ArrayBasedLinearAlgebraProvider;
        //    var segment = lap.CreateSegment(vector.Size);
        //    vector.Segment.CopyTo(segment);
        //    return new ArrayBasedVector(segment, lap);
        //}

        //public static IMatrix ToArrayBased(this IMatrix matrix)
        //{
        //    if (matrix.Segment.SegmentType == Consts.ArrayBased)
        //        return matrix;

        //    var lap = matrix.Context.ArrayBasedLinearAlgebraProvider;
        //    var segment = lap.CreateSegment(matrix.Segment.Size);
        //    matrix.Segment.CopyTo(segment);
        //    return new ArrayBasedMatrix(segment, matrix.RowCount, matrix.ColumnCount, lap);
        //}

        //public static ITensor3D ToArrayBased(this ITensor3D tensor)
        //{
        //    if (tensor.Segment.SegmentType == Consts.ArrayBased)
        //        return tensor;

        //    var lap = tensor.Context.ArrayBasedLinearAlgebraProvider;
        //    var segment = lap.CreateSegment(tensor.Segment.Size);
        //    tensor.Segment.CopyTo(segment);
        //    return new ArrayBasedTensor3D(segment, tensor.Depth, tensor.RowCount, tensor.ColumnCount, lap);
        //}

        //public static ITensor4D ToArrayBased(this ITensor4D tensor)
        //{
        //    if (tensor.Segment.SegmentType == Consts.ArrayBased)
        //        return tensor;

        //    var lap = tensor.Context.ArrayBasedLinearAlgebraProvider;
        //    var segment = lap.CreateSegment(tensor.Segment.Size);
        //    tensor.Segment.CopyTo(segment);
        //    return new ArrayBasedTensor4D(segment, tensor.Count, tensor.Depth, tensor.RowCount, tensor.ColumnCount, lap);
        //}

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
            Parallel.For(0, size, ind => {
                lap.BindThread();
                ret[ind] = FindDistance(compareTo, vectors[(int)ind], distanceMetric);
            });
            return ret;
        }

        public static void Set(this ITensorSegment2 segment, Func<uint, float> getValue)
        {
            for (uint i = 0, len = segment.Size; i < len; i++)
                segment[i] = getValue(i);
        }

        public static void Set(this ITensorSegment2 segment, float value)
        {
            for (uint i = 0, len = segment.Size; i < len; i++)
                segment[i] = value;
        }

        public static void SetToRandom(this ITensorSegment2 segment, Random random)
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

        public static IVector ToVector(this ITensorSegment2 segment, LinearAlgebraProvider lap) => lap.CreateVector(segment);
        public static IMatrix ToMatrix(this ITensorSegment2 segment, LinearAlgebraProvider lap, uint rows, uint columns) => lap.CreateMatrix(rows, columns, segment);
        public static ITensor3D ToTensor3D(this ITensorSegment2 segment, LinearAlgebraProvider lap, uint depth, uint rows, uint columns) => lap.CreateTensor3D(depth, rows, columns, segment);
        public static ITensor4D ToTensor4D(this ITensorSegment2 segment, LinearAlgebraProvider lap, uint count, uint depth, uint rows, uint columns) => lap.CreateTensor4D(count, depth, rows, columns, segment);

        public static void CopyTo(this ITensor2 tensor, ITensor2 other) => tensor.Segment.CopyTo(other.Segment);
    }
}
