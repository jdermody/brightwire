using System;
using System.Linq;
using System.Numerics;
using System.Threading;
using CommunityToolkit.HighPerformance;

namespace BrightData.Types
{
    /// <summary>
    /// Binary vector - each bit represents an item in the vector
    /// </summary>
    public readonly record struct BitVector : IHaveSize, IHaveDataAsReadOnlyByteSpan
    {
        const int NumBitsPerItem = 64;

        /// <summary>
        /// Creates a fixed size bit vector from a data block
        /// </summary>
        /// <param name="data"></param>
        public BitVector(ulong[] data)
        {
            Data = data;
        }

        /// <summary>
        /// Creates a fixed size bit vector
        /// </summary>
        /// <param name="size"></param>
        public BitVector(uint size)
        {
            Data = new ulong[size / NumBitsPerItem + (size % NumBitsPerItem > 0 ? 1 : 0)];
        }

        BitVector(BitVector other)
        {
            Data = other.Data.ToArray();
        }

        /// <summary>
        /// Underlying data
        /// </summary>
        public ulong[] Data { get; }

        /// <inheritdoc />
        public uint Size => (uint)Data.Length * NumBitsPerItem;

        /// <summary>
        /// Gets or sets an item in the vector
        /// </summary>
        /// <param name="bitIndex"></param>
        /// <exception cref="ArgumentException"></exception>
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

        /// <summary>
        /// Counts the bits that are true
        /// </summary>
        /// <returns></returns>
        public uint CountOfSetBits()
        {
            var ret = 0;
            foreach (var item in Data)
                ret += BitOperations.PopCount(item);
            return (uint)ret;
        }

        /// <summary>
        /// Underlying data
        /// </summary>
        /// <returns></returns>
        public ReadOnlySpan<ulong> AsSpan() => new(Data);

        /// <summary>
        /// Returns a new vector that has been XORed with this and another vector
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public BitVector XorWith(BitVector other)
        {
            var ret = new BitVector(this);
            Xor(ret.Data.AsSpan(), other.AsSpan());
            return ret;
        }

        /// <summary>
        /// Returns a new vector that is the union of this and another vector
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public BitVector UnionWith(BitVector other)
        {
            var ret = new BitVector(this);
            Or(ret.Data.AsSpan(), other.AsSpan());
            return ret;
        }

        /// <summary>
        /// Returns a new vector that is the intersection of this and another vector
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public BitVector IntersectionWith(BitVector other)
        {
            var ret = new BitVector(this);
            And(ret.Data.AsSpan(), other.AsSpan());
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public BitVector Except(BitVector other)
        {
            var ret = new BitVector(this);
            Xor(ret.Data.AsSpan(), other.AsSpan());
            And(ret.Data.AsSpan(), other.AsSpan());
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public uint HammingDistance(BitVector other)
        {
            var copy = new BitVector(this);
            Xor(copy.Data.AsSpan(), other.AsSpan());
            return copy.CountOfSetBits();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        public void XorInPlace(in BitVector other) => Xor(Data.AsSpan(), other.AsSpan());
        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        public void UnionInPlace(in BitVector other) => Or(Data.AsSpan(), other.AsSpan());
        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
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

        /// <inheritdoc />
        public ReadOnlySpan<byte> DataAsBytes => AsSpan().AsBytes();
    }
}
