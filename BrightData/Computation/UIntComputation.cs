using System;
using System.Threading;
using System.Threading.Tasks;

namespace BrightData.Computation
{
    class UIntComputation : ComputationBase<uint>
    {
        public UIntComputation(ITensorPool tensorPool) : base(tensorPool) { }

        public override uint SumIndexedProducts(uint size, Func<uint, uint> p1, Func<uint, uint> p2)
        {
            uint sum = 0;
            for (uint i = 0; i < size; i++)
                sum += p1(i) * p2(i);
            return sum;
        }

        protected override uint Aggregate(ITensorSegment<uint> segment, uint initial, Func<uint, uint, uint> aggregator)
        {
            long ret = initial;
            Parallel.ForEach(segment.Values, v => {
                uint initialValue, computedValue;
                do {
                    initialValue = (uint)ret;
                    computedValue = aggregator(initialValue, v);
                }
                while (initialValue != Interlocked.CompareExchange(ref ret, computedValue, initialValue));
            });
            return (uint)ret;
        }

        protected override uint Add(uint a, uint b) => a + b;
        protected override uint Subtract(uint a, uint b) => a - b;
        protected override uint Multiply(uint a, uint b) => a * b;
        protected override uint Divide(uint a, uint b) => a / b;
        protected override uint Sqrt(uint a) => (uint)Math.Sqrt(a);
        protected override uint Abs(uint a) => (uint)Math.Abs(a);
        protected override uint Log(uint a) => (uint)Math.Log(a);
        protected override uint Exp(uint a) => (uint)Math.Exp(a);
        protected override uint OneMinusInput(uint input) => 1 - input;
        protected override uint Cast(uint a) => a;
    }
}
