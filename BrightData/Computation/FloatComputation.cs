using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;
using BrightData.Helper;
using BrightData.LinearAlgebra;
using BrightData.Memory;

namespace BrightData.Computation
{
    internal class FloatComputation : INumericComputation<float>
    {
        readonly IBrightDataContext _context;

        public FloatComputation(IBrightDataContext context)
        {
            _context = context;
        }

        public ITensorSegment<float> Add(ITensorSegment<float> tensor1, ITensorSegment<float> tensor2) => ZipVectorised(tensor1, tensor2, (a, b) => a+b, (a, b) => a + b);
        public ITensorSegment<float> Add(ITensorSegment<float> tensor1, ITensorSegment<float> tensor2, float coefficient1, float coefficient2) => ZipVectorised(tensor1, tensor2, (a, b) => a * coefficient1 + b * coefficient2, (a, b) => a * coefficient1 + b * coefficient2);
        public ITensorSegment<float> Add(ITensorSegment<float> tensor1, float scalar)
        {
            var scalarVector = new System.Numerics.Vector<float>(scalar);
            return TransformVectorised(tensor1, a => a + scalarVector, a => a + scalar);
        } 

        public void AddInPlace(ITensorSegment<float> target, ITensorSegment<float> other) => MutateVectorised(target, other, (a, b) => a + b, (a, b) => a + b);
        public void AddInPlace(ITensorSegment<float> target, ITensorSegment<float> other, float coefficient1, float coefficient2) => MutateVectorised(target, other, (a,b) => a * coefficient1 + b * coefficient2, (a,b) => a * coefficient1 + b * coefficient2);

        public void AddInPlace(ITensorSegment<float> target, float scalar)
        {
            var scalarVector = new System.Numerics.Vector<float>(scalar);
            MutateInPlaceVectorised(target, v => v + scalarVector, v => v + scalar);
        }

        public void MultiplyInPlace(ITensorSegment<float> target, float scalar)
        {
            var scalarVector = new System.Numerics.Vector<float>(scalar);
            MutateInPlaceVectorised(target, v => v * scalarVector, v => v * scalar);
        }

        public ITensorSegment<float> Multiply(ITensorSegment<float> target, float scalar)
        {
            var scalarVector = new System.Numerics.Vector<float>(scalar);
            return TransformVectorised(target, v => v * scalarVector, v => v * scalar);
        } 

        public ITensorSegment<float> Subtract(ITensorSegment<float> tensor1, ITensorSegment<float> tensor2) => ZipVectorised(tensor1, tensor2, (a, b) => a - b, (a, b) => a - b);
        public ITensorSegment<float> Subtract(ITensorSegment<float> tensor1, ITensorSegment<float> tensor2, float coefficient1, float coefficient2) => ZipVectorised(tensor1, tensor2, (a, b) => a * coefficient1 - b * coefficient2, (a, b) => a * coefficient1 - b * coefficient2);

        public void SubtractInPlace(ITensorSegment<float> target, ITensorSegment<float> other) => MutateVectorised(target, other, (a, b) => a - b, (a, b) => a - b);
        public void SubtractInPlace(ITensorSegment<float> target, ITensorSegment<float> other, float coefficient1, float coefficient2) => MutateVectorised(target, other, (a, b) => a * coefficient1 - b * coefficient2, (a, b) => a * coefficient1 - b * coefficient2);

        public ITensorSegment<float> PointwiseMultiply(ITensorSegment<float> tensor1, ITensorSegment<float> tensor2) => ZipVectorised(tensor1, tensor2, (a, b) => a * b, (a, b) => a * b);

        public void PointwiseMultiplyInPlace(ITensorSegment<float> target, ITensorSegment<float> other) => MutateVectorised(target, other, (a, b) => a * b, (a, b) => a * b);
        public ITensorSegment<float> PointwiseDivide(ITensorSegment<float> tensor1, ITensorSegment<float> tensor2) => ZipVectorised(tensor1, tensor2, (a, b) => a / b, (a, b) => a / b);

        public void PointwiseDivideInPlace(ITensorSegment<float> target, ITensorSegment<float> other) => MutateVectorised(target, other, (a, b) => a / b, (a, b) => a / b);

        public float DotProduct(ITensorSegment<float> segment, ITensorSegment<float> other)
        {
            using var product = PointwiseMultiply(segment, other);
            return Sum(product);
        }

        public ITensorSegment<float> Sqrt(ITensorSegment<float> tensor) => Transform(tensor, MathF.Sqrt);

        public uint? Search(ITensorSegment<float> segment, float value)
        {
            uint? ret = null;
            Analyse(segment, (v, index) => {
                if (Math.Abs(value - v) < FloatMath.AlmostZero)
                    ret = index;
            });
            return ret;
        }

        public void ConstrainInPlace(ITensorSegment<float> segment, float? minValue, float? maxValue)
        {
            MutateInPlace(segment, value => {
                if (minValue.HasValue && value.CompareTo(minValue.Value) < 0)
                    return minValue.Value;
                if (maxValue.HasValue && value.CompareTo(maxValue.Value) > 0)
                    return maxValue.Value;
                return value;
            });
        }

        public float Average(ITensorSegment<float> segment) => Sum(segment) / segment.Size;

        public float L1Norm(ITensorSegment<float> segment)
        {
            using var abs = Abs(segment);
            return Sum(abs);
        }

        public float L2Norm(ITensorSegment<float> segment)
        {
            using var squared = Squared(segment);
            return MathF.Sqrt(Sum(squared));
        }

        public unsafe float Sum(ITensorSegment<float> segment)
        {
            if (Sse3.IsSupported) {
                float result;
                var size = segment.Size;

                fixed (float* pSource = segment.ToArray()) {
                    var vresult = Vector128<float>.Zero;

                    var i = 0;
                    var lastBlockIndex = size - (size % 4);
                    while (i < lastBlockIndex) {
                        vresult = Sse.Add(vresult, Sse.LoadVector128(pSource + i));
                        i += 4;
                    }

                    vresult = Sse3.HorizontalAdd(vresult, vresult);
                    vresult = Sse3.HorizontalAdd(vresult, vresult);
                    result = vresult.ToScalar();

                    while (i < size) {
                        result += pSource[i];
                        i++;
                    }
                }

                return result;
            }

            return segment.Values.AsParallel().Sum();
        }

        public (float Min, float Max, uint MinIndex, uint MaxIndex) GetMinAndMaxValues(ITensorSegment<float> segment)
        {
            // parallel code would need locks...
            return ComputationBase<float>.GetMinAndMaxValues(segment, float.MaxValue, float.MinValue);
        }

        public bool IsEntirelyFinite(ITensorSegment<float> segment) => !segment.Values.Any(v => float.IsNaN(v) || float.IsInfinity(v));

        public ITensorSegment<float> Reverse(ITensorSegment<float> segment)
        {
            var len = segment.Size - 1;
            return TransformIndexed(segment, i => segment[len - i]);
        }

        public List<ITensorSegment<float>> Split(ITensorSegment<float> segment, uint blockCount) => ComputationBase<float>.SplitSegment(segment, blockCount).ToList();

        public float CosineDistance(ITensorSegment<float> tensor, ITensorSegment<float> other)
        {
            var ab = DotProduct(tensor, other);
            var aa = DotProduct(tensor, tensor);
            var bb = DotProduct(other, other);
            return 1f - ab / (MathF.Sqrt(aa) * MathF.Sqrt(bb));
        }

        public float EuclideanDistance(ITensorSegment<float> tensor, ITensorSegment<float> other)
        {
            using var distance = Subtract(tensor, other);
            using var squared = Squared(distance);
            return MathF.Sqrt(Sum(squared));
        }

        public float ManhattanDistance(ITensorSegment<float> tensor, ITensorSegment<float> other)
        {
            using var distance = Subtract(tensor, other);
            using var squared = Abs(distance);
            return Sum(squared);
        }

        public ITensorSegment<float> Abs(ITensorSegment<float> tensor) => Transform(tensor, MathF.Abs);
        public ITensorSegment<float> Log(ITensorSegment<float> tensor) => Transform(tensor, MathF.Log);
        public ITensorSegment<float> Exp(ITensorSegment<float> tensor) => Transform(tensor, MathF.Exp);
        public ITensorSegment<float> Squared(ITensorSegment<float> tensor) => TransformVectorised(tensor, v => v * v, v => v * v);

        public float StdDev(ITensorSegment<float> segment, float? mean)
        {
            var avg = mean ?? Average(segment);
            var avgVector = new System.Numerics.Vector<float>(avg);
            using var result = TransformVectorised(segment, v => {
                var s = v - avgVector;
                return s * s;
            }, v => {
                var s = v - avg;
                return s * s;
            });
            return MathF.Sqrt(Average(result));
        }

        public static float Sigmoid(float val) => FloatMath.Constrain(1.0f / (1.0f + MathF.Exp(-1.0f * val)));
        public static float SigmoidDerivative(float val)
        {
            var sigmoid = Sigmoid(val);
            return FloatMath.Constrain(sigmoid * (1.0f - sigmoid));
        }
        public static float Tanh(float val) => MathF.Tanh(val);
        public static float TanhDerivative(float val) => 1.0f - MathF.Pow(Tanh(val), 2);
        public static float Relu(float val) => (val <= 0) ? 0 : FloatMath.Constrain(val);
        public static float ReluDerivative(float val) => (val <= 0) ? 0f : 1;
        public static float LeakyRelu(float val) => (val <= 0) ? 0.01f * val : FloatMath.Constrain(val);
        public static float LeakyReluDerivative(float val) => (val <= 0) ? 0.01f : 1;

        public ITensorSegment<float> Sigmoid(ITensorSegment<float> segment) => Transform(segment, Sigmoid);
        public ITensorSegment<float> SigmoidDerivative(ITensorSegment<float> segment) => Transform(segment, SigmoidDerivative);
        public ITensorSegment<float> Tanh(ITensorSegment<float> segment) => Transform(segment, Tanh);
        public ITensorSegment<float> TanhDerivative(ITensorSegment<float> segment) => Transform(segment, TanhDerivative);
        public ITensorSegment<float> Relu(ITensorSegment<float> segment) => Transform(segment, Relu);
        public ITensorSegment<float> ReluDerivative(ITensorSegment<float> segment) => Transform(segment, ReluDerivative);
        public ITensorSegment<float> LeakyRelu(ITensorSegment<float> segment) => Transform(segment, LeakyRelu);
        public ITensorSegment<float> LeakyReluDerivative(ITensorSegment<float> segment) => Transform(segment, LeakyReluDerivative);

        public ITensorSegment<float> Softmax(ITensorSegment<float> segment)
        {
            var (_, max, _, _) = GetMinAndMaxValues(segment);

            var softmax = Transform(segment, v => MathF.Exp(v - max));
            var sum = Sum(softmax);
            if (FloatMath.IsNotZero(sum)) {
                var ret = Transform(softmax, v => v / sum);
                softmax.Dispose();
                return ret;
            }

            return softmax;
        }

        public Matrix<float> SoftmaxDerivative(ITensorSegment<float> segment)
        {
            return segment.Context.CreateMatrix(segment.Size, segment.Size, (x, y) => x == y
                ? segment[x] * (1 - segment[x])
                : -segment[x] * segment[y]
            );
        }

        public ITensorSegment<float> Pow(ITensorSegment<float> segment, float power) => Transform(segment, v => FloatMath.Pow(v, power));
        public void RoundInPlace(ITensorSegment<float> segment, float lower, float upper, float? mid)
        {
            var compareTo = mid ?? lower + (upper - lower) / 2;
            MutateInPlace(segment, v => v >= compareTo ? upper : lower);
        }

        public float NextRandom() => Convert.ToSingle(_context.Random.NextDouble());

        protected ITensorSegment<float> Zip(ITensorSegment<float> segment, ITensorSegment<float> other, Func<float, float, float> func)
        {
            if (segment.Size != other.Size)
                throw new ArgumentException("Segments were different sizes");

            var ret = _context.TensorPool.Get<float>(segment.Size);
            Parallel.ForEach(segment.Values, (v, _, i) => { ret[i] = func(v, other[i]); });
            return new TensorSegment<float>(_context, ret);
        }

        protected ITensorSegment<float> ZipVectorised(
            ITensorSegment<float> segment, 
            ITensorSegment<float> other, 
            Func<System.Numerics.Vector<float>, System.Numerics.Vector<float>, System.Numerics.Vector<float>> func1,
            Func<float, float, float> func2)
        {
            var size = segment.Size;
            if (size != other.Size)
                throw new ArgumentException("Segments were different sizes");

            var ret = _context.TensorPool.Get<float>(size);
            var vectorSize = System.Numerics.Vector<float>.Count;
            var i = 0;
            if (size >= vectorSize) {
                for (; i <= size - vectorSize; i += vectorSize) {
                    var s1 = segment.AsNumericsVector(i);
                    var s2 = other.AsNumericsVector(i);
                    func1(s1, s2).CopyTo(ret, i);
                }
            }
            for (; i < size; i++)
                ret[i] = func2(segment[i], other[i]);
            return new TensorSegment<float>(_context, ret);
        }

        protected ITensorSegment<float> Transform(ITensorSegment<float> segment, Func<float, float> transfomer)
        {
            var ret = _context.TensorPool.Get<float>(segment.Size);
            Parallel.ForEach(segment.Values, (v, _, i) => { ret[i] = transfomer(v); });
            return _context.CreateSegment(ret);
        }

        protected ITensorSegment<float> TransformVectorised(ITensorSegment<float> segment, Func<System.Numerics.Vector<float>, System.Numerics.Vector<float>> transfomer1, Func<float, float> transfomer2)
        {
            var ret = _context.TensorPool.Get<float>(segment.Size);
            var vectorSize = System.Numerics.Vector<float>.Count;
            var size = segment.Size;
            var i = 0;
            if (size >= vectorSize) {
                for (; i <= size - vectorSize; i += vectorSize) {
                    var s1 = segment.AsNumericsVector(i);
                    transfomer1(s1).CopyTo(ret, i);
                }
            }
            for (; i < size; i++)
                ret[i] = transfomer2(segment[i]);
            return _context.CreateSegment(ret);
        }

        protected ITensorSegment<float> TransformIndexed(ITensorSegment<float> segment, Func<uint, float> transfomer)
        {
            var ret = _context.TensorPool.Get<float>(segment.Size);
            Parallel.ForEach(segment.Values, (_, _, i) => { ret[i] = transfomer((uint)i); });
            return _context.CreateSegment(ret);
        }

        protected static void Mutate(ITensorSegment<float> segment, ITensorSegment<float> other, Func<float, float, float> func)
        {
            if (segment.Size != other.Size)
                throw new ArgumentException("Segments were different sizes");

            Parallel.ForEach(segment.Values, (v, _, i) => { segment[i] = func(v, other[(int)i]); });
        }

        protected static void MutateVectorised(ITensorSegment<float> segment, ITensorSegment<float> other, Func<System.Numerics.Vector<float>, System.Numerics.Vector<float>, System.Numerics.Vector<float>> func1, Func<float, float, float> func2)
        {
            if (segment.Size != other.Size)
                throw new ArgumentException("Segments were different sizes");

            var ret = new float[segment.Size];
            var vectorSize = System.Numerics.Vector<float>.Count;
            var size = segment.Size;
            var i = 0;
            if (size >= vectorSize) {
                for (; i <= size - vectorSize; i += vectorSize) {
                    var s1 = segment.AsNumericsVector(i);
                    var s2 = other.AsNumericsVector(i);
                    func1(s1, s2).CopyTo(ret, i);
                }
            }
            for (; i < size; i++)
                ret[i] = func2(segment[i], other[i]);

            segment.Initialize(ret);
        }

        protected static void MutateInPlace(ITensorSegment<float> segment, Func<float, float> mutator)
        {
            Parallel.ForEach(segment.Values, (v, _, i) => { segment[i] = mutator(v); });
        }

        protected static void MutateInPlaceVectorised(ITensorSegment<float> segment, Func<System.Numerics.Vector<float>, System.Numerics.Vector<float>> mutator1, Func<float, float> mutator2)
        {
            var ret = new float[segment.Size];
            var vectorSize = System.Numerics.Vector<float>.Count;
            var size = segment.Size;
            var i = 0;
            if (size >= vectorSize) {
                for (; i <= size - vectorSize; i += vectorSize) {
                    var s1 = segment.AsNumericsVector(i);
                    mutator1(s1).CopyTo(ret, i);
                }
            }
            for (; i < size; i++)
                ret[i] = mutator2(segment[i]);

            segment.Initialize(ret);
        }

        protected static void Analyse(ITensorSegment<float> segment, Action<float, uint> analyser)
        {
            Parallel.ForEach(segment.Values, (v, _, i) => { analyser(v, (uint)i); });
        }

        public float Get(uint val) => Convert.ToSingle(val);
        public float Get(float val) => Convert.ToSingle(val);
        public float Get(double val) => Convert.ToSingle(val);
        public float Get(decimal val) => Convert.ToSingle(val);
        public float Zero { get; } = 0f;
        public float One { get; } = 1f;
    }
}
