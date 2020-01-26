using System;
using System.Threading.Tasks;
using BrightData.Memory;

namespace BrightData.Computation
{
    abstract class ComputationBase<T> : INumericComputation<T> where T : struct, IComparable<T>, IConvertible, IEquatable<T>
    {
        protected readonly IBrightDataContext _context;

        protected ComputationBase(IBrightDataContext context)
        {
            _context = context;
        }

        public abstract T NextRandom();
        protected abstract T Aggregate(ITensorSegment<T> segment, T initial, Func<T, T, T> aggregator);

        protected abstract T Add(T a, T b);
        protected abstract T Subtract(T a, T b);
        protected abstract T Multiply(T a, T b);
        protected abstract T Divide(T a, T b);
        protected abstract T Sqrt(T a);
        protected abstract T Abs(T a);
        protected abstract T Log(T a);
        protected abstract T Exp(T a);
        protected abstract T OneMinusInput(T input);
        protected abstract T Cast(uint a);
        protected abstract T Constrain(T val);
        protected abstract T MinValue { get; }
        protected abstract T MaxValue { get; }
        protected abstract bool IsZero(T value);

        bool IsNotZero(T value) => !IsZero(value);

        public ITensorSegment<T> Add(ITensorSegment<T> tensor1, ITensorSegment<T> tensor2)
        {
            return Zip(tensor1, tensor2, Add);
        }

        public void AddInPlace(ITensorSegment<T> target, ITensorSegment<T> other)
        {
            Mutate(target, other, Add);
        }

        public void AddInPlace(ITensorSegment<T> target, T scalar)
        {
            MutateInPlace(target, v => Add(v, scalar));
        }

        public void MultiplyInPlace(ITensorSegment<T> target, T scalar)
        {
            MutateInPlace(target, v => Multiply(v, scalar));
        }

        public ITensorSegment<T> Subtract(ITensorSegment<T> tensor1, ITensorSegment<T> tensor2)
        {
            return Zip(tensor1, tensor2, Subtract);
        }

        public void SubtractInPlace(ITensorSegment<T> target, ITensorSegment<T> other)
        {
            Mutate(target, other, Subtract);
        }

        public ITensorSegment<T> PointwiseMultiply(ITensorSegment<T> tensor1, ITensorSegment<T> tensor2)
        {
            return Zip(tensor1, tensor2, Multiply);
        }

        public void PointwiseMultiplyInPlace(ITensorSegment<T> target, ITensorSegment<T> other)
        {
            Mutate(target, other, Multiply);
        }

        public ITensorSegment<T> PointwiseDivide(ITensorSegment<T> tensor1, ITensorSegment<T> tensor2)
        {
            return Zip(tensor1, tensor2, Divide);
        }

        public void PointwiseDivideInPlace(ITensorSegment<T> target, ITensorSegment<T> other)
        {
            Mutate(target, other, Divide);
        }

        public ITensorSegment<T> Abs(ITensorSegment<T> tensor)
        {
            return Transform(tensor, Abs);
        }

        public ITensorSegment<T> Log(ITensorSegment<T> tensor)
        {
            return Transform(tensor, Log);
        }

        public ITensorSegment<T> Exp(ITensorSegment<T> tensor)
        {
            return Transform(tensor, Exp);
        }

        public ITensorSegment<T> Sqrt(ITensorSegment<T> tensor)
        {
            return Transform(tensor, Sqrt);
        }

        public ITensorSegment<T> Squared(ITensorSegment<T> tensor)
        {
            return Transform(tensor, x => Multiply(x, x));
        }

        public T Sum(ITensorSegment<T> tensor)
        {
            return Aggregate(tensor, default, Add);
        }

        public uint? Search(ITensorSegment<T> segment, T value)
        {
            uint? ret = null;
            Analyse(segment, (v, index) => {
                if (value.Equals(v))
                    ret = index;
            });
            return ret;
        }

        public void ConstrainInPlace(ITensorSegment<T> segment, T? minValue, T? maxValue)
        {
            MutateInPlace(segment, value => {
                if (minValue.HasValue && value.CompareTo(minValue.Value) < 0)
                    return minValue.Value;
                if (maxValue.HasValue && value.CompareTo(maxValue.Value) > 0)
                    return maxValue.Value;
                return value;
            });
        }

        public T Average(ITensorSegment<T> segment)
        {
            var sum = Sum(segment);
            return Divide(sum, Cast(segment.Size));
        }

        public T StdDev(ITensorSegment<T> segment, T? mean)
        {
            mean ??= Average(segment);
            using var result = Transform(segment, v => {
                var s = Subtract(v, mean.Value);
                return Multiply(s, s);
            });
            return Sqrt(Average(result));
        }

        public T DotProduct(ITensorSegment<T> segment, ITensorSegment<T> other)
        {
            using var product = PointwiseMultiply(segment, other);
            return Sum(product);
        }

        public T L1Norm(ITensorSegment<T> segment)
        {
            using var temp = Abs(segment);
            return Sum(temp);
        }

        public T L2Norm(ITensorSegment<T> segment)
        {
            using var temp = Squared(segment);
            return Sqrt(Sum(temp));
        }

        public T CosineDistance(ITensorSegment<T> tensor, ITensorSegment<T> other)
        {
            var ab = DotProduct(tensor, other);
            var aa = DotProduct(tensor, tensor);
            var bb = DotProduct(other, other);
            return OneMinusInput(Divide(ab, Sqrt(Multiply(aa, bb))));
        }

        public T EuclideanDistance(ITensorSegment<T> tensor, ITensorSegment<T> other)
        {
            using var distance = Subtract(tensor, other);
            using var squared = Squared(distance);
            return Sqrt(Sum(squared));
        }

        public T ManhattanDistance(ITensorSegment<T> tensor, ITensorSegment<T> other)
        {
            using var distance = Subtract(tensor, other);
            using var squared = Abs(distance);
            return Sum(squared);
        }

        (T Min, T Max, uint MinIndex, uint MaxIndex) GetMinAndMaxValues(ITensorSegment<T> segment)
        {
            var min = MaxValue;
            var max = MinValue;
            uint minIndex = uint.MaxValue;
            uint maxIndex = uint.MaxValue;
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

        protected ITensorSegment<T> Zip(ITensorSegment<T> segment, ITensorSegment<T> other, Func<T, T, T> func)
        {
            if (segment.Size != other.Size)
                throw new ArgumentException("Segments were different sizes");

            var ret = (TensorBlock<T>)_context.TensorPool.Get<T>(segment.Size);
            Parallel.ForEach(segment.Values, (v, s, i) => { ret[i] = func(v, other[i]); });
            return ret.GetSegment();
        }

        protected ITensorSegment<T> Transform(ITensorSegment<T> segment, Func<T, T> transfomer)
        {
            var ret = (TensorBlock<T>)_context.TensorPool.Get<T>(segment.Size);
            Parallel.ForEach(segment.Values, (v, s, i) => { ret[i] = transfomer(v); });
            return ret.GetSegment();
        }

        protected void Mutate(ITensorSegment<T> segment, ITensorSegment<T> other, Func<T, T, T> func)
        {
            if (segment.Size != other.Size)
                throw new ArgumentException("Segments were different sizes");

            Parallel.ForEach(segment.Values, (v, s, i) => { segment[i] = func(v, other[(int)i]); });
        }

        protected void MutateInPlace(ITensorSegment<T> segment, Func<T, T> mutator)
        {
            Parallel.ForEach(segment.Values, (v, s, i) => { segment[i] = mutator(v); });
        }

        protected void Analyse(ITensorSegment<T> segment, Action<T, uint> analyser)
        {
            Parallel.ForEach(segment.Values, (v, s, i) => { analyser(v, (uint)i); });
        }
    }
}
