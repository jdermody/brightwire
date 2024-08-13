using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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
        IProvideByteBlocks? tempStreams = null,
        int blockSize = Consts.DefaultInitialBlockSize,
        int maxBlockSize = Consts.DefaultMaxBlockSize,
        uint? maxInMemoryBlocks = null,
        uint? maxDistinctItems = null
    )
        : CompositeBufferBase<T, ManagedCompositeBuffer<T>.Block>(x => new(x), x => new(x), tempStreams, blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems)
        where T : IHaveDataAsReadOnlyByteSpan
    {
        public class Block(Memory<T> Data) : IMutableBufferBlock<T>
        {
            public Block(ReadOnlyMemory<T> data) : this(Unsafe.As<ReadOnlyMemory<T>, Memory<T>>(ref data))
            {
                Size = (uint)data.Length;
            }

            public const int HeaderSize = 8;
            public uint Size { get; private set; }
            public ref T GetNext() => ref Data.Span[(int)Size++];
            public bool HasFreeCapacity => Size < Data.Length;
            public ReadOnlySpan<T> WrittenSpan => Data.Span[..(int)Size];
            public ReadOnlyMemory<T> WrittenMemory => Data[..(int)Size];

            public async Task<uint> WriteTo(IByteBlockSource file)
            {
                var offset = file.Size;
                using var writer = new ArrayPoolBufferWriter<byte>();
                var memoryOwner = (IMemoryOwner<byte>)writer;
                writer.Advance(HeaderSize);
                uint size = 0;
                for (var i = 0; i < Size; i++)
                    size += WriteBlock(Data.Span[i].DataAsBytes, writer);
                BinaryPrimitives.WriteUInt32LittleEndian(memoryOwner.Memory.Span, Size);
                BinaryPrimitives.WriteUInt32LittleEndian(memoryOwner.Memory.Span[4..], size);
                await file.WriteAsync(writer.WrittenMemory, offset);
                return size + HeaderSize;
            }

            static uint WriteBlock(ReadOnlySpan<byte> itemData, ArrayPoolBufferWriter<byte> writer)
            {
                var span = writer.GetSpan(itemData.Length + 4);
                BinaryPrimitives.WriteUInt32LittleEndian(span, (uint)itemData.Length);
                itemData.CopyTo(span[4..]);
                writer.Advance(itemData.Length + 4);
                return (uint)itemData.Length + 4;
            }
        }
        protected override async Task<uint> SkipFileBlock(IByteBlockSource file, uint offset, uint numItemsInBlock)
        {
            var lengthBytes = new byte[Block.HeaderSize];
            await file.ReadAsync(lengthBytes, offset);
            var blockSize = BinaryPrimitives.ReadUInt32LittleEndian(lengthBytes);
            return blockSize + Block.HeaderSize;
        }

        protected override async Task<(uint, ReadOnlyMemory<T>)> GetBlockFromFile(IByteBlockSource file, uint offset, uint numItemsInBlock)
        {
            var (blockByteSize, buffer) = await ReadBuffer(file, offset);
            try
            {
                var buffer2 = new Memory<T>(new T[numItemsInBlock]);
                Copy(buffer.Span, buffer2.Span, numItemsInBlock);
                return (blockByteSize + Block.HeaderSize, buffer2);
            }
            finally
            {
                buffer.Dispose();
            }
        }

        void Copy(ReadOnlySpan<byte> inputSpan, Span<T> outputSpan, uint numItemsInBlock)
        {
            for (var i = 0; i < numItemsInBlock; i++)
            {
                var itemSize = BinaryPrimitives.ReadUInt32LittleEndian(inputSpan);
                var itemData = inputSpan[4..(int)(itemSize + 4)];
                outputSpan[i] = createItem(itemData);
                inputSpan = inputSpan[(int)(itemSize + 4)..];
            }
        }

        static async Task<(uint BlockByteSize, MemoryOwner<byte> Buffer)> ReadBuffer(IByteBlockSource file, uint offset)
        {
            var lengthBytes = new byte[Block.HeaderSize];
            await file.ReadAsync(lengthBytes, offset);
            var blockSize = BinaryPrimitives.ReadUInt32LittleEndian(lengthBytes);
            offset += Block.HeaderSize;
            var buffer = MemoryOwner<byte>.Allocate((int)blockSize);
            await file.ReadAsync(buffer.Memory, offset);
            return (blockSize, buffer);
        }
    }
}
