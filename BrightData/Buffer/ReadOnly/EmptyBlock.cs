using System;

namespace BrightData.Buffer.ReadOnly
{
    /// <summary>
    /// Represents an empty block of memory
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EmptyBlock<T> : ICanRandomlyAccessUnmanagedData<T> where T : unmanaged
    {
        /// <inheritdoc />
        public void Dispose()
        {
            // nop
        }

        /// <inheritdoc />
        public uint Size => 0;
        /// <inheritdoc />
        public void Get(int index, out T value) => value = default;
        /// <inheritdoc />
        public void Get(uint index, out T value) => value = default;
        /// <inheritdoc />
        public ReadOnlySpan<T> GetSpan(uint startIndex, uint count) => new(null);
    }
}
