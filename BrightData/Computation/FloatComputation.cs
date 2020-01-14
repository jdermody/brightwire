using System;
using System.Threading;
using System.Threading.Tasks;
using BrightData.Helper;

namespace BrightData.Computation
{
    class FloatComputation : ComputationBase<float>
    {
        public FloatComputation(ITensorPool tensorPool) : base(tensorPool)
        {
        }

        protected override float Add(float a, float b) => a + b;
        protected override float Subtract(float a, float b) => a - b;
        protected override float Multiply(float a, float b) => a * b;
        protected override float Divide(float a, float b) => a / b;
        protected override float Sqrt(float a) => FloatMath.Sqrt(a);
        protected override float Abs(float a) => MathF.Abs(a);
        protected override float Log(float a) => FloatMath.Log(a);
        protected override float Exp(float a) => FloatMath.Exp(a);
        protected override float OneMinusInput(float input) => 1f - input;
        protected override float Cast(uint a) => a;

        protected override float Aggregate(ITensorSegment<float> segment, float initial, Func<float, float, float> aggregator)
        {
            float ret = initial;
            Parallel.ForEach(segment.Values, v => {
                float initialValue, computedValue;
                do {
                    initialValue = ret;
                    computedValue = aggregator(initialValue, v);
                }
                while (Math.Abs(initialValue - Interlocked.CompareExchange(ref ret, computedValue, initialValue)) > FloatMath.AlmostZero);
            });
            return ret;
        }

        public override float SumIndexedProducts(uint size, Func<uint, float> p1, Func<uint, float> p2)
        {
            float sum = 0;
            for (uint i = 0; i < size; i++)
                sum += p1(i) * p2(i);
            return sum;
        }
    }
}
