using System;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using CommunityToolkit.HighPerformance;

namespace BrightData.Buffer
{
    /// <summary>
    /// A header that points to the location of a block within a buffer
    /// </summary>
    /// <param name="Offset">The start offset of the block in the buffer</param>
    /// <param name="Size">The size of the block in the buffer</param>
    public readonly partial record struct BlockHeader(int Offset, int Size) : IHaveSize
    {
        /// <summary>
        /// The end offset of the block in the buffer
        /// </summary>
        public int EndOffset => Offset + Size;

        /// <summary>
        /// Returns the block
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="memory">Buffer</param>
        /// <returns></returns>
        public ReadOnlySpan<T> Get<T>(ReadOnlySpan<T> memory) => memory.Slice(Offset, Size);

        /// <summary>
        /// Returns the block
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="memory">Buffer</param>
        /// <returns></returns>
        public ReadOnlyMemory<T> Get<T>(ReadOnlyMemory<T> memory) => memory.Slice(Offset, Size);

        /// <summary>
        /// Returns the block
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="memory">Buffer</param>
        /// <returns></returns>
        public Memory<T> Get<T>(Memory<T> memory) => memory.Slice(Offset, Size);

        /// <summary>
        /// Returns the block
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="memory">Buffer</param>
        /// <returns></returns>
        public Span<T> Get<T>(Span<T> memory) => memory.Slice(Offset, Size);

        uint IHaveSize.Size => (uint)Size;

        /// <summary>
        /// The size (in bytes) of the BlockHeader structure
        /// </summary>
        public static int StructSize => Unsafe.SizeOf<BlockHeader>();

        /// <summary>
        /// Create an array of block headers, each pointing to contiguous blocks within a buffer
        /// </summary>
        /// <param name="blockSizes">The array of block sizes</param>
        /// <returns></returns>
        public static ReadOnlyMemory<BlockHeader> Create(params int[] blockSizes)
        {
            Memory<BlockHeader> ret = new BlockHeader[blockSizes.Length];
            var retSpan = ret.Span;
            var headerBytes = retSpan.Cast<BlockHeader, byte>();
            var index = 0;
            foreach (var length in blockSizes) {
                retSpan[index] = new(index == 0 ? headerBytes.Length : retSpan[index - 1].EndOffset, length);
                ++index;
            }
            return ret;
        }

        /// <summary>
        /// Returns a single buffer of bytes that consists of all the items in the span tuple arranged contiguously
        /// </summary>
        /// <param name="tuple"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ReadOnlyMemory<byte> Combine<T>(in T tuple) where T: IAmTupleOfSpans, allows ref struct
        {
            var sizes = tuple.Sizes;
            var header = Create(sizes);
            var headerBytes = header.Span.AsBytes();
            var totalSize = headerBytes.Length + sizes.Sum();

            Memory<byte> ret = new byte[totalSize];
            headerBytes.CopyTo(header.Span[0].Get(ret).Span);
            tuple.ForEach<byte>((x, i) => x.CopyTo(header.Span[i].Get(ret).Span));
            return ret;
        }
    }
}
