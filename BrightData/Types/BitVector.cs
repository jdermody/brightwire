using System;
using System.Linq;
using System.Numerics;
using System.Threading;

namespace BrightData.Types
{
    public readonly record struct BitVector : IHaveSize
    {
        const int NumBitsPerItem = 64;

        public BitVector(ulong[] data)
        {
            Data = data;
        }

        public BitVector(uint size)
        {
            Data = new ulong[size / NumBitsPerItem + (size % NumBitsPerItem > 0 ? 1 : 0)];
        }

        BitVector(BitVector other)
        {
            Data = other.Data.ToArray();
        }

        public ulong[] Data { get; }

        /// <inheritdoc />
        public uint Size => (uint)Data.Length * NumBitsPerItem;

        public bool this[int bitIndex]
        {
            get
            {
                var mask = 1UL << bitIndex;
                var index = bitIndex / NumBitsPerItem;
                if (index < 0 || index > Size)
                    throw new ArgumentException("Bit index out of bounds", nameof(bitIndex));
                return Data[index] * mask == mask;
            }
            set
            {
                var mask = 1UL << bitIndex;
                var index = bitIndex / NumBitsPerItem;
                if (index < 0 || index > Size)
                    throw new ArgumentException("Bit index out of bounds", nameof(bitIndex));

                while (true) {
                    var previousValue = Data[index];
                    var newValue = value 
                        ? previousValue | mask 
                        : previousValue ^ mask
                    ;
                    if (Interlocked.CompareExchange(ref Data[index], newValue, previousValue) == previousValue)
                        break;
                }
            }
        }

        public uint GetSetBits()
        {
            var ret = 0;
            foreach (var item in Data)
                ret += BitOperations.PopCount(item);
            return (uint)ret;
        }

        public ReadOnlySpan<ulong> AsSpan() => new(Data);

        public BitVector XorWith(BitVector other)
        {
            var ret = new BitVector(this);
            Xor(ret.Data.AsSpan(), other.AsSpan());
            return ret;
        }

        public BitVector UnionWith(BitVector other)
        {
            var ret = new BitVector(this);
            Or(ret.Data.AsSpan(), other.AsSpan());
            return ret;
        }

        public BitVector IntersectionWith(BitVector other)
        {
            var ret = new BitVector(this);
            And(ret.Data.AsSpan(), other.AsSpan());
            return ret;
        }

        public BitVector Except(BitVector other)
        {
            var ret = new BitVector(this);
            Xor(ret.Data.AsSpan(), other.AsSpan());
            And(ret.Data.AsSpan(), other.AsSpan());
            return ret;
        }

        public uint HammingDistance(BitVector other)
        {
            var copy = new BitVector(this);
            Xor(copy.Data.AsSpan(), other.AsSpan());
            return copy.GetSetBits();
        }

        public void XorInPlace(in BitVector other) => Xor(Data.AsSpan(), other.AsSpan());
        public void UnionInPlace(in BitVector other) => Or(Data.AsSpan(), other.AsSpan());
        public void IntersectionInPlace(in BitVector other) => And(Data.AsSpan(), other.AsSpan());

        static void Xor(Span<ulong> data, ReadOnlySpan<ulong> other) => data.MutateVectorized(
            other,
            (in Vector<ulong> a, in Vector<ulong> b, out Vector<ulong> r) => r = Vector.Xor(a, b),
            (a, b) => a ^ b
        );
        static void Or(Span<ulong> data, ReadOnlySpan<ulong> other) => data.MutateVectorized(
            other,
            (in Vector<ulong> a, in Vector<ulong> b, out Vector<ulong> r) => r = Vector.BitwiseOr(a, b),
            (a, b) => a | b
        );
        static void And(Span<ulong> data, ReadOnlySpan<ulong> other) => data.MutateVectorized(
            other,
            (in Vector<ulong> a, in Vector<ulong> b, out Vector<ulong> r) => r = Vector.BitwiseAnd(a, b),
            (a, b) => a & b
        );
    }
}
