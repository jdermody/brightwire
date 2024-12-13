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
        public bool Intersects(uint position) => position >= StartOffset && position < EndOffset;

        /// <summary>
        /// Checks if another range intersects
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Intersects(in OffsetAndSize other) => Intersects(other.StartOffset) || Intersects(other.EndOffset);

        /// <inheritdoc />
        public int CompareTo(OffsetAndSize other)
        {
            var ret = StartOffset.CompareTo(other.StartOffset);
            if(ret != 0) return ret;
            return Size.CompareTo(other.Size);
        }

        uint IHaveOffset.Offset => StartOffset;
    }
}
