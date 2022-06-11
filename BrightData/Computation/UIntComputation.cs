using System;
using System.Threading;
using System.Threading.Tasks;

namespace BrightData.Computation
{
    internal class UIntComputation : ComputationBase<uint>
    {
        public UIntComputation(IBrightDataContext context) : base(context) { }

        public override uint NextRandom() => (uint) _context.Random.Next();
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
        public override uint Add(uint a, uint b) => a + b;
        public override uint Subtract(uint a, uint b) => a - b;
        public override uint Multiply(uint a, uint b) => a * b;
        public override uint Divide(uint a, uint b) => a / b;
        public override uint Sqrt(uint a) => (uint)Math.Sqrt(a);
        public override uint Abs(uint a) => a; // unsigned integers are always positive
        public override uint Log(uint a) => (uint)Math.Log(a);
        public override uint Exp(uint a) => (uint)Math.Exp(a);
        public override uint Pow(uint a, uint rank) => (uint) Math.Pow(a, rank);

        public override uint OneMinusInput(uint input) => 1 - input;
        public override uint OnePlusInput(uint input) => 1 + input;

        public override uint OneDividedByInput(uint input) => throw new NotSupportedException();
        public override uint Constrain(uint val) => throw new NotSupportedException();

        public override uint MinValue => uint.MinValue;
        public override uint MaxValue => uint.MaxValue;
        public override bool IsZero(uint value) => value == 0;
        public override bool IsEqualOrLessThanZero(uint value) => value == 0;
        public override bool IsGreaterOrEqualTo(uint value, uint compareTo) => value >= compareTo;

        public override bool IsNaN(uint value) => false;
        public override bool IsInfinity(uint value) => false;

        public override uint Tanh(uint value) => throw new NotSupportedException();

        public override uint Negate(uint value) => throw new NotSupportedException();

        public override uint Zero { get; } = 0;
        public override uint One { get; } = 1;
        public override uint Two { get; } = 2;
        public override uint ZeroZeroOne => throw new NotSupportedException();
        public override uint LengthOf(ITensorSegment<uint> tensor) => tensor.Size;
        public override uint Get(uint val) => Convert.ToUInt32(val);
        public override uint Get(float val) => Convert.ToUInt32(val);
        public override uint Get(double val) => Convert.ToUInt32(val);
        public override uint Get(decimal val) => Convert.ToUInt32(val);
    }
}
