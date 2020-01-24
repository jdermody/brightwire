using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BrightData.Helper;
using BrightData.Memory;

namespace BrightData.Computation
{
    class FloatComputation : INumericComputation<float>
    {
        private readonly ITensorPool _tensorPool;

        public FloatComputation(ITensorPool tensorPool)
        {
            _tensorPool = tensorPool;
        }

        public ITensorSegment<float> Add(ITensorSegment<float> tensor1, ITensorSegment<float> tensor2) => Zip(tensor1, tensor2, (a, b) => a+b);
        public void AddInPlace(ITensorSegment<float> target, ITensorSegment<float> other) => Mutate(target, other, (a, b) => a + b);
        public void AddInPlace(ITensorSegment<float> target, float scalar) => MutateInPlace(target, v => v + scalar);
        public ITensorSegment<float> Subtract(ITensorSegment<float> tensor1, ITensorSegment<float> tensor2) => Zip(tensor1, tensor2, (a, b) => a - b);
        public void SubtractInPlace(ITensorSegment<float> target, ITensorSegment<float> other) => Mutate(target, other, (a, b) => a - b);
        public ITensorSegment<float> Multiply(ITensorSegment<float> tensor1, ITensorSegment<float> tensor2) => Zip(tensor1, tensor2, (a, b) => a * b);
        public void MultiplyInPlace(ITensorSegment<float> target, ITensorSegment<float> other) => Mutate(target, other, (a, b) => a * b);
        public ITensorSegment<float> Divide(ITensorSegment<float> tensor1, ITensorSegment<float> tensor2) => Zip(tensor1, tensor2, (a, b) => a / b);
        public void DivideInPlace(ITensorSegment<float> target, ITensorSegment<float> other) => Mutate(target, other, (a, b) => a / b);

        public float SumIndexedProducts(uint size, Func<uint, float> p1, Func<uint, float> p2)
        {
            var bag = new ConcurrentBag<float>();
            Parallel.For(0, (int)size, i => bag.Add(p1((uint)i) * p2((uint)i)));
            return bag.AsParallel().Sum();
        }

        public float DotProduct(ITensorSegment<float> segment, ITensorSegment<float> other)
        {
            using var product = Multiply(segment, other);
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

        protected ITensorSegment<float> Zip(ITensorSegment<float> segment, ITensorSegment<float> other, Func<float, float, float> func)
        {
            if (segment.Size != other.Size)
                throw new ArgumentException("Segments were different sizes");

            var ret = (TensorBlock<float>)_tensorPool.Get<float>(segment.Size);
            Parallel.ForEach(segment.Values, (v, s, i) => { ret[i] = func(v, other[i]); });
            return ret.GetSegment();
        }

        protected ITensorSegment<float> Transform(ITensorSegment<float> segment, Func<float, float> transfomer)
        {
            var ret = (TensorBlock<float>)_tensorPool.Get<float>(segment.Size);
            Parallel.ForEach(segment.Values, (v, s, i) => { ret[i] = transfomer(v); });
            return ret.GetSegment();
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
    }
}
