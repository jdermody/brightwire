using System;
using System.Threading.Tasks;
using BrightData.Memory;

namespace BrightData.Computation
{
    abstract class ComputationBase<T> : INumericComputation<T>
        where T: struct, IComparable<T>, IConvertible, IEquatable<T>
    {
        readonly ITensorPool _tensorPool;

        protected ComputationBase(ITensorPool tensorPool)
        {
            _tensorPool = tensorPool;
        }

        public abstract T SumIndexedProducts(uint size, Func<uint, T> p1, Func<uint, T> p2);
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

        public ITensorSegment<T> Add(ITensorSegment<T> tensor1, ITensorSegment<T> tensor2)
        {
            return Zip(tensor1, tensor2, Add);
        }

        public void AddInPlace(ITensorSegment<T> target, ITensorSegment<T> other)
        {
            Mutate(target, other, Add);
        }

        public ITensorSegment<T> Subtract(ITensorSegment<T> tensor1, ITensorSegment<T> tensor2)
        {
            return Zip(tensor1, tensor2, Subtract);
        }

        public void SubtractInPlace(ITensorSegment<T> target, ITensorSegment<T> other)
        {
            Mutate(target, other, Subtract);
        }

        public ITensorSegment<T> Multiply(ITensorSegment<T> tensor1, ITensorSegment<T> tensor2)
        {
            return Zip(tensor1, tensor2, Multiply);
        }

        public void MultiplyInPlace(ITensorSegment<T> target, ITensorSegment<T> other)
        {
            Mutate(target, other, Multiply);
        }

        public ITensorSegment<T> Divide(ITensorSegment<T> tensor1, ITensorSegment<T> tensor2)
        {
            return Zip(tensor1, tensor2, Divide);
        }

        public void DivideInPlace(ITensorSegment<T> target, ITensorSegment<T> other)
        {
            Mutate(target, other, Divide);
        }

        public ITensorSegment<T> Abs(ITensorSegment<T> tensor)
        {
            return Transform(tensor, Abs);
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

        public void Constrain(ITensorSegment<T> segment, T? minValue, T? maxValue)
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

        public T DotProduct(ITensorSegment<T> segment, ITensorSegment<T> other)
        {
            using var product = Multiply(segment, other);
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

        protected (T Min, T Max, uint MinIndex, uint MaxIndex) GetMinAndMaxValues(ITensorSegment<T> segment, T initialMin, T initialMax)
        {
            var min = initialMin;
            var max = initialMax;
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

            var ret = (TensorBlock<T>)_tensorPool.Get<T>(segment.Size);
            Parallel.ForEach(segment.Values, (v, s, i) => { ret[i] = func(v, other[i]); });
            return ret.GetSegment();
        }

        protected ITensorSegment<T> Transform(ITensorSegment<T> segment, Func<T, T> transfomer)
        {
            var ret = (TensorBlock<T>)_tensorPool.Get<T>(segment.Size);
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
