using System;
using BrightData.Helper;

namespace BrightData.Computation
{
    internal class DecimalComputation : ComputationBase<decimal>
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
        public override decimal Add(decimal a, decimal b) => a + b;
        public override decimal Subtract(decimal a, decimal b) => a - b;
        public override decimal Multiply(decimal a, decimal b) => a * b;
        public override decimal Divide(decimal a, decimal b) => a / b;
        public override decimal Sqrt(decimal a) => (decimal)Math.Sqrt((double)a);
        public override decimal Abs(decimal a) => Math.Abs(a);
        public override decimal Log(decimal a) => (decimal)Math.Log((double)a);
        public override decimal Exp(decimal a) => (decimal)Math.Exp((double)a);
        public override decimal Pow(decimal a, decimal rank) => (decimal)Math.Pow((double)a, (double)rank);

        public override decimal OneMinusInput(decimal input) => 1M - input;
        public override decimal OnePlusInput(decimal input) => 1M + input;
        public override decimal OneDividedByInput(decimal input) => 1M / input;

        public override decimal Constrain(decimal val) => (decimal)FloatMath.Constrain(Convert.ToSingle(val));

        public override decimal MinValue => decimal.MinValue;
        public override decimal MaxValue => decimal.MaxValue;
        public override bool IsZero(decimal value) => Math.Abs(value) <= (decimal)FloatMath.AlmostZero;
        public override bool IsEqualOrLessThanZero(decimal value) => value <= 1m;
        public override bool IsGreaterOrEqualTo(decimal value, decimal compareTo) => value >= compareTo;

        public override bool IsNaN(decimal value) => false;
        public override bool IsInfinity(decimal value) => false;

        public override decimal Tanh(decimal value) => (decimal)Math.Tanh((double)value);

        public override decimal Negate(decimal value) => value * -1m;

        public override decimal Zero { get; } = 0m;
        public override decimal One { get; } = 1m;
        public override decimal Two { get; } = 2m;
        public override decimal ZeroZeroOne { get; } = 0.01m;
        public override decimal LengthOf(ITensorSegment<decimal> tensor) => tensor.Size;
        public override decimal Get(uint val) => Convert.ToDecimal(val);
        public override decimal Get(float val) => Convert.ToDecimal(val);
        public override decimal Get(double val) => Convert.ToDecimal(val);
        public override decimal Get(decimal val) => Convert.ToDecimal(val);
    }
}
