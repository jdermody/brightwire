using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrightData.Helper;
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

        public ITensorSegment<float> Add(ITensorSegment<float> tensor1, ITensorSegment<float> tensor2) => Zip(tensor1, tensor2, (a, b) => a+b);
        public ITensorSegment<float> Add(ITensorSegment<float> tensor1, ITensorSegment<float> tensor2, float coefficient1, float coefficient2) => Zip(tensor1, tensor2, (a, b) => a * coefficient1 + b * coefficient2);
        public ITensorSegment<float> Add(ITensorSegment<float> tensor1, float scalar) => Transform(tensor1, a => a + scalar);

        public void AddInPlace(ITensorSegment<float> target, ITensorSegment<float> other) => Mutate(target, other, (a, b) => a + b);
        public void AddInPlace(ITensorSegment<float> target, ITensorSegment<float> other, float coefficient1, float coefficient2) => Mutate(target, other, (a,b) => a * coefficient1 + b * coefficient2);

        public void AddInPlace(ITensorSegment<float> target, float scalar) => MutateInPlace(target, v => v + scalar);
        public void MultiplyInPlace(ITensorSegment<float> target, float scalar) => MutateInPlace(target, v => v * scalar);
        public ITensorSegment<float> Multiply(ITensorSegment<float> target, float scalar) => Transform(target, v => v * scalar);

        public ITensorSegment<float> Subtract(ITensorSegment<float> tensor1, ITensorSegment<float> tensor2) => Zip(tensor1, tensor2, (a, b) => a - b);
        public ITensorSegment<float> Subtract(ITensorSegment<float> tensor1, ITensorSegment<float> tensor2, float coefficient1, float coefficient2) => Zip(tensor1, tensor2, (a, b) => a * coefficient1 - b * coefficient2);

        public void SubtractInPlace(ITensorSegment<float> target, ITensorSegment<float> other) => Mutate(target, other, (a, b) => a - b);
        public void SubtractInPlace(ITensorSegment<float> target, ITensorSegment<float> other, float coefficient1, float coefficient2) => Mutate(target, other, (a, b) => a * coefficient1 - b * coefficient2);

        public ITensorSegment<float> PointwiseMultiply(ITensorSegment<float> tensor1, ITensorSegment<float> tensor2) => Zip(tensor1, tensor2, (a, b) => a * b);

        public void PointwiseMultiplyInPlace(ITensorSegment<float> target, ITensorSegment<float> other) => Mutate(target, other, (a, b) => a * b);
        public ITensorSegment<float> PointwiseDivide(ITensorSegment<float> tensor1, ITensorSegment<float> tensor2) => Zip(tensor1, tensor2, (a, b) => a / b);

        public void PointwiseDivideInPlace(ITensorSegment<float> target, ITensorSegment<float> other) => Mutate(target, other, (a, b) => a / b);

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

        public float Sum(ITensorSegment<float> segment) => segment.Values.AsParallel().Sum();
        public (float Min, float Max, uint MinIndex, uint MaxIndex) GetMinAndMaxValues(ITensorSegment<float> segment) => ComputationBase<float>.GetMinAndMaxValues(segment, float.MaxValue, float.MinValue);

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
        public ITensorSegment<float> Squared(ITensorSegment<float> tensor) => Transform(tensor, v => v * v);

        public float StdDev(ITensorSegment<float> segment, float? mean)
        {
            mean ??= Average(segment);
            using var result = Transform(segment, v => {
                var s = v - mean.Value;
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
            var minMax = GetMinAndMaxValues(segment);
            var max = minMax.Max;

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

        public float NextRandom() => Convert.ToSingle(_context.Random.NextDouble());

        protected ITensorSegment<float> Zip(ITensorSegment<float> segment, ITensorSegment<float> other, Func<float, float, float> func)
        {
            if (segment.Size != other.Size)
                throw new ArgumentException("Segments were different sizes");

            var ret = _context.TensorPool.Get<float>(segment.Size);
            Parallel.ForEach(segment.Values, (v, s, i) => { ret[i] = func(v, other[i]); });
            return new TensorSegment<float>(_context, ret);
        }

        protected ITensorSegment<float> Transform(ITensorSegment<float> segment, Func<float, float> transfomer)
        {
            var ret = _context.TensorPool.Get<float>(segment.Size);
            Parallel.ForEach(segment.Values, (v, s, i) => { ret[i] = transfomer(v); });
            return _context.CreateSegment(ret);
        }

        protected ITensorSegment<float> TransformIndexed(ITensorSegment<float> segment, Func<uint, float> transfomer)
        {
            var ret = _context.TensorPool.Get<float>(segment.Size);
            Parallel.ForEach(segment.Values, (v, s, i) => { ret[i] = transfomer((uint)i); });
            return _context.CreateSegment(ret);
        }

        protected void Mutate(ITensorSegment<float> segment, ITensorSegment<float> other, Func<float, float, float> func)
        {
            if (segment.Size != other.Size)
                throw new ArgumentException("Segments were different sizes");

            Parallel.ForEach(segment.Values, (v, s, i) => { segment[i] = func(v, other[(int)i]); });
        }

        protected void MutateInPlace(ITensorSegment<float> segment, Func<float, float> mutator)
        {
            Parallel.ForEach(segment.Values, (v, s, i) => { segment[i] = mutator(v); });
        }

        protected void Analyse(ITensorSegment<float> segment, Action<float, uint> analyser)
        {
            Parallel.ForEach(segment.Values, (v, s, i) => { analyser(v, (uint)i); });
        }

        public float Get(uint val) => Convert.ToSingle(val);
        public float Get(float val) => Convert.ToSingle(val);
        public float Get(double val) => Convert.ToSingle(val);
        public float Get(decimal val) => Convert.ToSingle(val);
    }
}
