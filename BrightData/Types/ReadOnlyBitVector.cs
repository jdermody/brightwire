using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance;

namespace BrightData.Types
{
    /// <summary>
    /// 
    /// </summary>
    public readonly ref struct ReadOnlyBitVector : IBitVector
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="size"></param>
        public ReadOnlyBitVector(ReadOnlySpan<ulong> data, uint size)
        {
            Data = data;
            Size = size;
        }

        /// <summary>
        /// Underlying data
        /// </summary>
        public ReadOnlySpan<ulong> Data { get; } 

        /// <inheritdoc />
        public uint Size { get; }

        /// <inheritdoc />
        public bool this[int bitIndex]
        {
            get
            {
                var mask = 1UL << bitIndex;
                var index = bitIndex / BitVector.NumBitsPerItem;
                if (index < 0 || index > Size)
                    throw new ArgumentException("Bit index out of bounds", nameof(bitIndex));
                return (Data[index] & mask) == mask;
            }
        }

        /// <inheritdoc />
        public ReadOnlySpan<ulong> AsSpan() => Data;

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

        /// <inheritdoc />
        public ReadOnlySpan<byte> DataAsBytes => AsSpan().AsBytes();
    }
}
