using System;
using System.Buffers.Binary;
using System.Text;
using System.Threading.Tasks;
using BrightData.Buffer.MutableBlocks;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.Buffer.Composite
{
    /// <summary>
    /// A composite buffer for strings
    /// </summary>
    /// <param name="tempStreams"></param>
    /// <param name="blockSize"></param>
    /// <param name="maxInMemoryBlocks"></param>
    /// <param name="maxDistinctItems"></param>
    internal class StringCompositeBuffer(
        IProvideByteBlocks? tempStreams = null,
        int blockSize = Consts.DefaultInitialBlockSize,
        int maxBlockSize = Consts.DefaultMaxBlockSize,
        uint? maxInMemoryBlocks = null,
        uint? maxDistinctItems = null
    )
        : CompositeBufferBase<string, MutableStringBufferBlock>(x => new(x), x => new(x), tempStreams, blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems)
    {

        public override Task<ReadOnlyMemory<string>> GetTypedBlock(uint blockIndex)
        {
            if (blockIndex >= BlockCount)
                throw new ArgumentOutOfRangeException(nameof(blockIndex), $"Must be less than {BlockCount}");
            return base.GetTypedBlock(blockIndex);
        }

        public override void Append(in string item)
        {
            if (item.Length > ushort.MaxValue / 3)
                throw new ArgumentOutOfRangeException(nameof(item), $"Length cannot exceed {ushort.MaxValue / 3}");
            base.Append(item);
        }

        protected override async Task<uint> SkipFileBlock(IByteBlockSource file, uint byteOffset, uint numItemsInBlock)
        {
            var lengthBytes = new byte[4];
            await file.ReadAsync(lengthBytes, byteOffset);
            return MutableStringBufferBlock.HeaderSize + BinaryPrimitives.ReadUInt32LittleEndian(lengthBytes);
        }

        protected override async Task<(uint BlockSizeBytes, ReadOnlyMemory<string> Block)> GetBlockFromFile(IByteBlockSource file, uint byteOffset, uint numItemsInBlock)
        {
            var (numStrings, block) = await ReadBlock(file, byteOffset);
            try
            {
                var index = 0;
                var buffer = new Memory<string>(new string[(int)numStrings]);
                MutableStringBufferBlock.Decode(block.Span, chars =>
                {
                    // ReSharper disable once AccessToDisposedClosure
                    buffer.Span[index++] = new string(chars);
                });
                return ((uint)block.Length + MutableStringBufferBlock.HeaderSize, buffer);
            }
            finally
            {
                block.Dispose();
            }
        }

        static async Task<(uint NumStrings, MemoryOwner<byte> Block)> ReadBlock(IByteBlockSource file, uint offset)
        {
            var lengthBytes = new byte[MutableStringBufferBlock.HeaderSize];
            await file.ReadAsync(lengthBytes, offset);
            offset += MutableStringBufferBlock.HeaderSize;
            var blockSize = BinaryPrimitives.ReadUInt32LittleEndian(lengthBytes);
            var numStrings = BinaryPrimitives.ReadUInt32LittleEndian(lengthBytes.AsSpan(4));
            var block = MemoryOwner<byte>.Allocate((int)blockSize);
            uint readCount = 0;
            do
            {
                readCount += await file.ReadAsync(block.Memory[(int)readCount..], offset + readCount);
            } while (readCount < blockSize);

            return (numStrings, block);
        }
    }
}
