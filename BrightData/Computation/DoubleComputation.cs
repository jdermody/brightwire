using System;
using System.Threading;
using System.Threading.Tasks;
using BrightData.Helper;

namespace BrightData.Computation
{
    internal class DoubleComputation : ComputationBase<double>
    {
        public DoubleComputation(IBrightDataContext context) : base(context)
        {
        }

        public override double NextRandom() => _context.Random.NextDouble();
        protected override double Aggregate(ITensorSegment<double> segment, double initial, Func<double, double, double> aggregator)
        {
            double ret = initial;
            Parallel.ForEach(segment.Values, v => {
                double initialValue, computedValue;
                do {
                    initialValue = ret;
                    computedValue = aggregator(initialValue, v);
                }
                while (Math.Abs(initialValue - Interlocked.CompareExchange(ref ret, computedValue, initialValue)) > FloatMath.AlmostZero);
            });
            return ret;
        }
        public override double Add(double a, double b) => a + b;
        public override double Subtract(double a, double b) => a - b;
        public override double Multiply(double a, double b) => a * b;
        public override double Divide(double a, double b) => a / b;
        public override double Sqrt(double a) => Math.Sqrt(a);
        public override double Abs(double a) => Math.Abs(a);
        public override double Log(double a) => Math.Log(a);
        public override double Exp(double a) => Math.Exp(a);
        public override double Pow(double a, double rank) => Math.Pow(a, rank);

        public override double OneMinusInput(double input) => 1 - input;
        public override double OnePlusInput(double input) => 1 + input;
        public override double OneDividedByInput(double input) => 1 / input;
        public override double Constrain(double val) => FloatMath.Constrain(Convert.ToSingle(val));

        public override double MinValue => double.MinValue;
        public override double MaxValue => double.MaxValue;
        public override bool IsZero(double value) => Math.Abs(value) < FloatMath.AlmostZero;
        public override bool IsEqualOrLessThanZero(double value) => value <= 0;
        public override bool IsGreaterOrEqualTo(double value, double compareTo) => value >= compareTo;

        public override bool IsNaN(double value) => double.IsNaN(value);

        public override bool IsInfinity(double value) => double.IsInfinity(value);

        public override double Tanh(double value) => Math.Tanh(value);

        public override double Negate(double value) => -value;

        public override double Zero { get; } = 0;
        public override double One { get; } = 1;
        public override double Two { get; } = 2;
        public override double ZeroZeroOne { get; } = 0.01;
        public override double LengthOf(ITensorSegment<double> tensor) => tensor.Size;
        public override double Get(uint val) => Convert.ToDouble(val);
        public override double Get(float val) => Convert.ToDouble(val);
        public override double Get(double val) => Convert.ToDouble(val);
        public override double Get(decimal val) => Convert.ToDouble(val);
    }
}
