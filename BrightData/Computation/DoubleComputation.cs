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

        public override double SumIndexedProducts(uint size, Func<uint, double> p1, Func<uint, double> p2)
        {
            double sum = 0;
            for (uint i = 0; i < size; i++)
                sum += p1(i) * p2(i);
            return sum;
        }

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
        protected override double One => 1;
        protected override double Zero => 0;
        protected override double PointZeroOne => 0.01;
        protected override double MinusOne => -1;
        protected override double MinValue => double.MinValue;
        protected override double MaxValue => double.MaxValue;
        protected override bool IsZero(double value) => Math.Abs(value) < FloatMath.AlmostZero;
        protected override bool IsLessOrEqualToThanZero(double value) => value <= 0;
        protected override double Negate(double value) => value * -1;
        protected override double Tanh(double value) => Math.Tanh(value);
    }
}
