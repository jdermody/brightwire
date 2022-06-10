using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrightData.LinearAlgebra;
using BrightData.Memory;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.Computation
{
    internal abstract class ComputationBase<T> : INumericComputation<T> where T : struct, IComparable<T>, IConvertible, IEquatable<T>
    {
        protected readonly IBrightDataContext _context;

        protected ComputationBase(IBrightDataContext context)
        {
            _context = context;
        }

        public abstract T NextRandom();
        protected abstract T Aggregate(ITensorSegment<T> segment, T initial, Func<T, T, T> aggregator);

        public abstract T Add(T a, T b);
        public abstract T Subtract(T a, T b);
        public abstract T Multiply(T a, T b);
        public abstract T Divide(T a, T b);
        public abstract T Sqrt(T a);
        public abstract T Abs(T a);
        public abstract T Log(T a);
        public abstract T Exp(T a);
        public abstract T Pow(T a, T rank);
        public abstract T OneMinusInput(T input);
        public abstract T OnePlusInput(T input);
        public abstract T OneDividedByInput(T input);
        public abstract T Constrain(T val);
        public abstract T MinValue { get; }
        public abstract T MaxValue { get; }
        public abstract bool IsZero(T value);
        public abstract bool IsEqualOrLessThanZero(T value);
        public abstract bool IsGreaterOrEqualTo(T value, T compareTo);
        public bool IsNotZero(T value) => !IsZero(value);
        public abstract bool IsNaN(T value);
        public abstract bool IsInfinity(T value);
        public abstract T Tanh(T value);
        public abstract T Negate(T value);
        public abstract T Zero { get; }
        public abstract T One { get; }
        public abstract T Two { get; }
        public abstract T ZeroZeroOne { get; }

        public ITensorSegment<T> Add(ITensorSegment<T> tensor1, ITensorSegment<T> tensor2)
        {
            return Zip(tensor1, tensor2, Add);
        }

        public ITensorSegment<T> Add(ITensorSegment<T> tensor1, ITensorSegment<T> tensor2, T coefficient1, T coefficient2)
        {
            return Zip(tensor1, tensor2, (x, y) => Add(Multiply(x, coefficient1), Multiply(y, coefficient2)));
        }

        public ITensorSegment<T> Add(ITensorSegment<T> tensor1, T scalar)
        {
            return Transform(tensor1, a => Add(a, scalar));
        }

        public void AddInPlace(ITensorSegment<T> target, ITensorSegment<T> other)
        {
            Mutate(target, other, Add);
        }

        public void AddInPlace(ITensorSegment<T> target, ITensorSegment<T> other, T coefficient1, T coefficient2)
        {
            Mutate(target, other, (x, y) => Add(Multiply(x, coefficient1), Multiply(y, coefficient2)));
        }

        public void AddInPlace(ITensorSegment<T> target, T scalar)
        {
            MutateInPlace(target, v => Add(v, scalar));
        }

        public void MultiplyInPlace(ITensorSegment<T> target, T scalar)
        {
            MutateInPlace(target, v => Multiply(v, scalar));
        }
        public ITensorSegment<T> Multiply(ITensorSegment<T> target, T scalar) => Transform(target, v => Multiply(v, scalar));

        public ITensorSegment<T> Subtract(ITensorSegment<T> tensor1, ITensorSegment<T> tensor2)
        {
            return Zip(tensor1, tensor2, Subtract);
        }

        public ITensorSegment<T> Subtract(ITensorSegment<T> tensor1, ITensorSegment<T> tensor2, T coefficient1, T coefficient2)
        {
            return Zip(tensor1, tensor2, (x, y) => Subtract(Multiply(x, coefficient1), Multiply(y, coefficient2)));
        }

        public void SubtractInPlace(ITensorSegment<T> target, ITensorSegment<T> other)
        {
            Mutate(target, other, Subtract);
        }

        public void SubtractInPlace(ITensorSegment<T> target, ITensorSegment<T> other, T coefficient1, T coefficient2)
        {
            Mutate(target, other, (x, y) => Subtract(Multiply(x, coefficient1), Multiply(y, coefficient2)));
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

        public (T Min, T Max, uint MinIndex, uint MaxIndex) GetMinAndMaxValues(ITensorSegment<T> segment) => GetMinAndMaxValues(segment, MaxValue, MinValue);

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
            return Divide(sum, Get(segment.Size));
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

        public ITensorSegment<T> Pow(ITensorSegment<T> segment, T power) => Transform(segment, v => Pow(v, power));
        public void RoundInPlace(ITensorSegment<T> segment, T lower, T upper, T? mid)
        {
            var compareTo = mid ?? Add(lower, Divide(Subtract(upper, lower), Two));
            MutateInPlace(segment, v => IsGreaterOrEqualTo(v, compareTo) ? upper : lower);
        }

        public abstract T Get(uint val);
        public abstract T Get(float val);
        public abstract T Get(double val);
        public abstract T Get(decimal val);

        public static (T Min, T Max, uint MinIndex, uint MaxIndex) GetMinAndMaxValues(ITensorSegment<T> segment, T min, T max)
        {
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

        protected ITensorSegment<T> Zip(ITensorSegment<T> segment, ITensorSegment<T> other, Func<T, T, T> func)
        {
            if (segment.Size != other.Size)
                throw new ArgumentException("Segments were different sizes");

            var ret = _context.TensorPool.Get<T>(segment.Size);
            var array = ret.DangerousGetArray();
            Parallel.ForEach(segment.Values, (v, _, i) => { array[(int)i] = func(v, other[i]); });
            return _context.CreateSegment(ret);
        }

        protected ITensorSegment<T> Transform(ITensorSegment<T> segment, Func<T, T> transfomer)
        {
            var ret = _context.TensorPool.Get<T>(segment.Size);
            var array = ret.DangerousGetArray();
            Parallel.ForEach(segment.Values, (v, _, i) => { array[(int)i] = transfomer(v); });
            return _context.CreateSegment(ret);
        }

        protected static void Mutate(ITensorSegment<T> segment, ITensorSegment<T> other, Func<T, T, T> func)
        {
            if (segment.Size != other.Size)
                throw new ArgumentException("Segments were different sizes");

            Parallel.ForEach(segment.Values, (v, _, i) => { segment[i] = func(v, other[(int)i]); });
        }

        protected static void MutateInPlace(ITensorSegment<T> segment, Func<T, T> mutator)
        {
            Parallel.ForEach(segment.Values, (v, _, i) => { segment[i] = mutator(v); });
        }

        protected static void Analyse(ITensorSegment<T> segment, Action<T, uint> analyser)
        {
            Parallel.ForEach(segment.Values, (v, _, i) => { analyser(v, (uint)i); });
        }

        public bool IsEntirelyFinite(ITensorSegment<T> segment)
        {
            return !segment.Values.Any(v => IsNaN(v) || IsInfinity(v));
        }

        public ITensorSegment<T> Reverse(ITensorSegment<T> segment)
        {
            var len = segment.Size;
            var ret = _context.TensorPool.Get<T>(segment.Size);
            var array = ret.DangerousGetArray();
            Parallel.ForEach(segment.Values, (v, _, i) => { array[(int)(len - i)] = v; });
            return _context.CreateSegment(ret);
        }

        public static IEnumerable<ITensorSegment<T>> SplitSegment(ITensorSegment<T> segment, uint blockCount)
        {
            for (uint i = 0, size = segment.Size, blockSize = size / blockCount; i < size; i += blockSize)
                yield return new TensorSegmentWrapper<T>(segment, i, 1, blockSize);
        }

        public List<ITensorSegment<T>> Split(ITensorSegment<T> segment, uint blockCount)
        {
            return SplitSegment(segment, blockCount).ToList();
        }

        public T Sigmoid(T val) => Constrain(OneDividedByInput(OnePlusInput(Exp(Negate(val)))));
        public T SigmoidDerivative(T val)
        {
            var score = Sigmoid(val);
            return Constrain(Multiply(score, OneMinusInput(score)));
        }
        public T TanhDerivative(T val) => Constrain(OneMinusInput(Pow(Tanh(val), Get(2))));
        public T Relu(T val) => IsEqualOrLessThanZero(val) ? Zero : Constrain(val);
        public T ReluDerivative(T val) => IsEqualOrLessThanZero(val) ? Zero : One;
        public T LeakyRelu(T val) => IsEqualOrLessThanZero(val) ? Multiply(ZeroZeroOne, val) : Constrain(val);
        public T LeakyReluDerivative(T val) => IsEqualOrLessThanZero(val) ? ZeroZeroOne : One;
        public ITensorSegment<T> Sigmoid(ITensorSegment<T> segment) => Transform(segment, Sigmoid);
        public ITensorSegment<T> SigmoidDerivative(ITensorSegment<T> segment) => Transform(segment, SigmoidDerivative);
        public ITensorSegment<T> Tanh(ITensorSegment<T> segment) => Transform(segment, Tanh);
        public ITensorSegment<T> TanhDerivative(ITensorSegment<T> segment) => Transform(segment, TanhDerivative);
        public ITensorSegment<T> Relu(ITensorSegment<T> segment) => Transform(segment, Relu);
        public ITensorSegment<T> ReluDerivative(ITensorSegment<T> segment) => Transform(segment, ReluDerivative);
        public ITensorSegment<T> LeakyRelu(ITensorSegment<T> segment) => Transform(segment, LeakyRelu);
        public ITensorSegment<T> LeakyReluDerivative(ITensorSegment<T> segment) => Transform(segment, LeakyReluDerivative);

        public ITensorSegment<T> Softmax(ITensorSegment<T> segment)
        {
            var (_, max, _, _) = GetMinAndMaxValues(segment);

            var softmax = Transform(segment, v => Exp(Subtract(v, max)));
            var sum = Sum(softmax);
            if (IsNotZero(sum)) {
                var ret = Transform(softmax, v => Divide(v, sum));
                softmax.Dispose();
                return ret;
            }

            return softmax;
        }

        public Matrix<T> SoftmaxDerivative(ITensorSegment<T> segment)
        {
            return segment.Context.CreateMatrix(segment.Size, segment.Size, (x, y) => x == y
                ? Multiply(segment[x], OneMinusInput(segment[x]))
                : Multiply(Negate(segment[x]), segment[y])
            );
        }
    }
}
