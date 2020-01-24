using System;
using BrightData.Helper;

namespace BrightData.Computation
{
    class DecimalComputation : ComputationBase<decimal>
    {
        public DecimalComputation(ITensorPool tensorPool) : base(tensorPool) { }

        public override decimal SumIndexedProducts(uint size, Func<uint, decimal> p1, Func<uint, decimal> p2)
        {
            decimal sum = 0;
            for (uint i = 0; i < size; i++)
                sum += p1(i) * p2(i);
            return sum;
        }

        protected override decimal Aggregate(ITensorSegment<decimal> segment, decimal initial, Func<decimal, decimal, decimal> aggregator)
        {
            decimal ret = initial;
            // no threading support for decimals, so not able to use parallel
            foreach(var value in segment.Values) {
                ret = aggregator(ret, value);
            }
            return ret;
        }

        protected override decimal Add(decimal a, decimal b) => a + b;
        protected override decimal Subtract(decimal a, decimal b) => a - b;
        protected override decimal Multiply(decimal a, decimal b) => a * b;
        protected override decimal Divide(decimal a, decimal b) => a / b;
        protected override decimal Sqrt(decimal a) => (decimal)Math.Sqrt((double)a);
        protected override decimal Abs(decimal a) => Math.Abs(a);
        protected override decimal Log(decimal a) => (decimal)Math.Log((double)a);
        protected override decimal Exp(decimal a) => (decimal)Math.Exp((double)a);
        protected override decimal OneMinusInput(decimal input) => 1M - input;
        protected override decimal Cast(uint a) => a;
        protected override decimal Constrain(decimal val) => (decimal)FloatMath.Constrain(Convert.ToSingle(val));
        protected override decimal One => 1M;
        protected override decimal Zero => 0M;
        protected override decimal PointZeroOne => 0.01M;
        protected override decimal MinusOne => -1M;
        protected override decimal MinValue => decimal.MinValue;
        protected override decimal MaxValue => decimal.MaxValue;
        protected override bool IsZero(decimal value) => Math.Abs(value) <= (decimal)FloatMath.AlmostZero;
        protected override bool IsLessOrEqualToThanZero(decimal value) => value <= 0M;
        protected override decimal Negate(decimal value) => value * -1M;
        protected override decimal Tanh(decimal value) => (decimal)Math.Tanh((double)value);
    }
}
