using System;

namespace BrightData.Buffer
{
    /// <summary>
    /// A range (with an offset and size) of a buffer
    /// </summary>
    /// <param name="StartOffset">Start position within the buffer</param>
    /// <param name="Size">Length within the buffer</param>
    public readonly record struct OffsetAndSize(uint StartOffset, uint Size) : IHaveOffset, IHaveSize, IComparable<OffsetAndSize>
    {
        /// <summary>
        /// Returns the section referenced by this offset and length
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span">Base buffer</param>
        /// <returns></returns>
        public ReadOnlySpan<T> GetSpan<T>(ReadOnlySpan<T> span) => span.Slice((int)StartOffset, (int)Size);

        /// <summary>
        /// The end offset
        /// </summary>
        public uint EndOffset => StartOffset + Size;

        /// <summary>
        /// Checks if a position is within the range
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool Intersects(uint position) => position >= StartOffset && position <= EndOffset;

        /// <summary>
        /// Checks if another range is contained within this range
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Contains(in OffsetAndSize other) => Intersects(other.StartOffset) && Intersects(other.EndOffset);

        /// <inheritdoc />
        public int CompareTo(OffsetAndSize other)
        {
            var ret = StartOffset.CompareTo(other.StartOffset);
            if(ret != 0) 
                return ret;
            return Size.CompareTo(other.Size);
        }

        /// <summary>
        /// Converts to a range
        /// </summary>
        /// <returns></returns>
        public Range AsRange() => new((int)StartOffset, (int)EndOffset);

        /// <summary>
        /// Null indicator
        /// </summary>
        public static readonly OffsetAndSize Null = new(uint.MaxValue, 0);

        /// <summary>
        /// Checks if the offset is "null"
        /// </summary>
        /// <returns></returns>
        public bool IsNull() => StartOffset == uint.MaxValue && Size is 0 or uint.MaxValue;

        /// <summary>
        /// Checks if the offset is valid - not "null"
        /// </summary>
        /// <returns></returns>
        public bool IsValid() => !IsNull();

        uint IHaveOffset.Offset => StartOffset;
    }
}
