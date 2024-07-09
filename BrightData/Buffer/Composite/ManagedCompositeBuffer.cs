using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Threading.Tasks;
using BrightData.Buffer.MutableBlocks;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.Buffer.Composite
{
    /// <summary>
    /// A composite buffer for managed types
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="createItem"></param>
    /// <param name="tempStreams"></param>
    /// <param name="blockSize"></param>
    /// <param name="maxInMemoryBlocks"></param>
    /// <param name="maxDistinctItems"></param>
    internal class ManagedCompositeBuffer<T>(
        CreateFromReadOnlyByteSpan<T> createItem,
        IProvideDataBlocks? tempStreams = null,
        int blockSize = Consts.DefaultBlockSize,
        uint? maxInMemoryBlocks = null,
        uint? maxDistinctItems = null
    )
        : CompositeBufferBase<T, MutableManagedBufferBlock<T>>((x, existing) => new(x, existing), tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems)
        where T : IHaveDataAsReadOnlyByteSpan
    {
        protected override async Task<uint> SkipFileBlock(IByteBlockSource file, uint offset)
        {
            var lengthBytes = new byte[MutableManagedBufferBlock<T>.HeaderSize];
            await file.ReadAsync(lengthBytes, offset);
            var blockSize = BinaryPrimitives.ReadUInt32LittleEndian(lengthBytes);
            return blockSize + MutableManagedBufferBlock<T>.HeaderSize;
        }

        protected override async Task<(uint, ReadOnlyMemory<T>)> GetBlockFromFile(IByteBlockSource file, uint offset)
        {
            var (blockSize, buffer) = await ReadBuffer(file, offset);
            try
            {
                var buffer2 = new Memory<T>(new T[_blockSize]);
                Copy(buffer.Span, buffer2.Span);
                return (blockSize + MutableManagedBufferBlock<T>.HeaderSize, buffer2);
            }
            finally
            {
                buffer.Dispose();
            }
        }

        protected override async Task<uint> GetBlockFromFile(IByteBlockSource file, uint offset, BlockCallback<T> callback)
        {
            var (blockSize, buffer) = await ReadBuffer(file, offset);
            try
            {
                using var buffer2 = MemoryOwner<T>.Allocate(_blockSize);
                Copy(buffer.Span, buffer2.Span);
                callback(buffer2.Span);
                return blockSize + MutableManagedBufferBlock<T>.HeaderSize;
            }
            finally
            {
                buffer.Dispose();
            }
        }

        void Copy(ReadOnlySpan<byte> inputSpan, Span<T> outputSpan)
        {
            for (var i = 0; i < _blockSize; i++)
            {
                var itemSize = BinaryPrimitives.ReadUInt32LittleEndian(inputSpan);
                var itemData = inputSpan[4..(int)(itemSize + 4)];
                outputSpan[i] = createItem(itemData);
                inputSpan = inputSpan[(int)(itemSize + 4)..];
            }
        }

        static async Task<(uint BlockSize, MemoryOwner<byte> Buffer)> ReadBuffer(IByteBlockSource file, uint offset)
        {
            var lengthBytes = new byte[MutableManagedBufferBlock<T>.HeaderSize];
            await file.ReadAsync(lengthBytes, offset);
            var blockSize = BinaryPrimitives.ReadUInt32LittleEndian(lengthBytes);
            offset += MutableManagedBufferBlock<T>.HeaderSize;
            var buffer = MemoryOwner<byte>.Allocate((int)blockSize);
            await file.ReadAsync(buffer.Memory, offset);
            return (blockSize, buffer);
        }
    }
}
