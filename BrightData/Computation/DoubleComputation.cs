using System;
using System.Threading;
using System.Threading.Tasks;
using BrightData.Helper;

namespace BrightData.Computation
{
    class DoubleComputation : ComputationBase<double>
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
        protected override double Add(double a, double b) => a + b;
        protected override double Subtract(double a, double b) => a - b;
        protected override double Multiply(double a, double b) => a * b;
        protected override double Divide(double a, double b) => a / b;
        protected override double Sqrt(double a) => Math.Sqrt(a);
        protected override double Abs(double a) => Math.Abs(a);
        protected override double Log(double a) => Math.Log(a);
        protected override double Exp(double a) => Math.Exp(a);
        protected override double OneMinusInput(double input) => 1 - input;
        protected override double Cast(uint a) => a;
        protected override double Constrain(double val) => FloatMath.Constrain(Convert.ToSingle(val));

        protected override double MinValue => double.MinValue;
        protected override double MaxValue => double.MaxValue;
        protected override bool IsZero(double value) => Math.Abs(value) < FloatMath.AlmostZero;
    }
}
