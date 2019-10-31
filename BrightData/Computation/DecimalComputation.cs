using System;
using System.Collections.Generic;
using System.Text;

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
        protected override decimal OneMinusInput(decimal input) => 1 - input;
        protected override decimal Cast(uint a) => a;
    }
}
