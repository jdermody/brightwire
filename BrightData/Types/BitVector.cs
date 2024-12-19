using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;

namespace BrightData.Types
{
    /// <summary>
    /// Binary vector - each bit represents an item in the vector
    /// </summary>
    public readonly record struct BitVector : IBitVector
    {
        internal const int NumBitsPerItem = 64;

        /// <summary>
        /// Creates a fixed size bit vector from a data block
        /// </summary>
        /// <param name="data"></param>
        /// <param name="size"></param>
        public BitVector(Memory<ulong> data, uint size)
        {
            Data = data;
            Size = size;
        }

        /// <summary>
        /// Creates a fixed size bit vector
        /// </summary>
        /// <param name="size"></param>
        public BitVector(uint size)
        {
            Data = new ulong[GetRequiredSize(size)];
            Size = size;
        }

        internal static uint GetRequiredSize(uint size) => (uint)(size / NumBitsPerItem + (size % NumBitsPerItem > 0 ? 1 : 0));

        /// <summary>
        /// Creates a fixed size bit vector
        /// </summary>
        /// <param name="data"></param>
        /// <param name="size"></param>
        public BitVector(ReadOnlySpan<ulong> data, uint size)
        {
            Data = data.ToArray();
            Size = size;
        }

        /// <summary>
        /// Underlying data
        /// </summary>
        public Memory<ulong> Data { get; }

        /// <inheritdoc />
        public uint Size { get; }

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
                return (Data.Span[index] & mask) == mask;
            }
            set
            {
                var mask = 1UL << bitIndex;
                var index = bitIndex / NumBitsPerItem;
                if (index < 0 || index > Size)
                    throw new ArgumentException("Bit index out of bounds", nameof(bitIndex));

                var span = Data.Span;
                while (true) {
                    var previousValue = span[index];
                    var newValue = value 
                        ? previousValue | mask 
                        : previousValue ^ mask
                    ;
                    if (newValue == previousValue || Interlocked.CompareExchange(ref span[index], newValue, previousValue) == previousValue)
                        break;
                }
            }
        }

        /// <summary>
        /// Sets a range of bits
        /// </summary>
        /// <param name="range"></param>
        public void SetBits(Range range)
        {
            var start = range.Start.GetOffset((int)Size);
            var end = range.End.GetOffset((int)Size);
            var keys = Enumerable.Range(start, end-start)
                .Select(x => (Index: x / NumBitsPerItem, Mask: 1UL << x))
                .GroupBy(x => x.Index)
                .Select(x => (Index: x.Key, Mask: x.Aggregate(0UL, (y, z) => y | z.Mask)))
            ;
            var span = Data.Span;
            foreach (var (index, mask) in keys) {
                while (true) {
                    var previousValue = span[index];
                    var newValue = previousValue | mask;
                    if (newValue == previousValue || Interlocked.CompareExchange(ref span[index], newValue, previousValue) == previousValue)
                        break;
                }
            }
        }

        /// <summary>
        /// Clears all bits
        /// </summary>
        public void Clear() => Data.Span.Clear();

        /// <summary>
        /// Underlying data
        /// </summary>
        /// <returns></returns>
        public ReadOnlySpan<ulong> AsSpan() => Data.Span;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        public void XorInPlace(in BitVector other) => ExtensionMethods.Xor(Data.Span, other.AsSpan());

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        public void UnionInPlace(in BitVector other) => ExtensionMethods.Or<ulong>(Data.Span, other.AsSpan());

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        public void IntersectionInPlace(in BitVector other) => ExtensionMethods.And<ulong>(Data.Span, other.AsSpan());

        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new StringBuilder();
            for (var i = 0; i < Math.Min(Size, 128); i++)
                sb.Append(this[i] ? '1' : '0');
            if (Size > 128)
                sb.Append("...");
            return sb.ToString();
        }
    }
}
