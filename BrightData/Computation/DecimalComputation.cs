using System;
using BrightData.Helper;

namespace BrightData.Computation
{
    class DecimalComputation : ComputationBase<decimal>
    {
        public DecimalComputation(IBrightDataContext context) : base(context)
        {
        }

        public override decimal NextRandom() => (decimal) _context.Random.NextDouble();
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

        protected override decimal MinValue => decimal.MinValue;
        protected override decimal MaxValue => decimal.MaxValue;
        protected override bool IsZero(decimal value) => Math.Abs(value) <= (decimal)FloatMath.AlmostZero;
    }
}
