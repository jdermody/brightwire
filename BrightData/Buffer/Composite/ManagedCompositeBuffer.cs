using System;
using System.Buffers;
using System.Buffers.Binary;
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
        IProvideDataBlocks? tempStreams = null,
        int blockSize = Consts.DefaultBlockSize,
        uint? maxInMemoryBlocks = null,
        uint? maxDistinctItems = null
    )
        : CompositeBufferBase<T, ManagedCompositeBuffer<T>.Block>((x, existing) => new(x, existing), tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems)
        where T : IHaveDataAsReadOnlyByteSpan
    {
        public const int HeaderSize = 8;
        internal record Block(T[] Data) : ICompositeBufferBlock<T>
        {
            public Block(T[] data, bool existing) : this(data)
            {
                if (existing)
                    Size = (uint)data.Length;
            }

            public uint Size { get; private set; }
            public ref T GetNext() => ref Data[Size++];
            public bool HasFreeCapacity => Size < Data.Length;
            public ReadOnlySpan<T> WrittenSpan => new(Data, 0, (int)Size);
            public ReadOnlyMemory<T> WrittenMemory => new(Data, 0, (int)Size);

            public async Task<uint> WriteTo(IByteBlockSource file)
            {
                var offset = file.Size;
                using var writer = new ArrayPoolBufferWriter<byte>();
                var memoryOwner = (IMemoryOwner<byte>)writer;
                writer.Advance(HeaderSize);
                uint size = 0;
                for (uint i = 0; i < Size; i++)
                    size += WriteBlock(Data[i].DataAsBytes, writer);
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

        protected override async Task<uint> SkipFileBlock(IByteBlockSource file, uint offset)
        {
            var lengthBytes = new byte[HeaderSize];
            await file.ReadAsync(lengthBytes, offset);
            var blockSize = BinaryPrimitives.ReadUInt32LittleEndian(lengthBytes);
            return blockSize + HeaderSize;
        }

        protected override async Task<(uint, ReadOnlyMemory<T>)> GetBlockFromFile(IByteBlockSource file, uint offset)
        {
            var (blockSize, buffer) = await ReadBuffer(file, offset);
            try
            {
                var buffer2 = new Memory<T>(new T[_blockSize]);
                Copy(buffer.Span, buffer2.Span);
                return (blockSize + HeaderSize, buffer2);
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
                return blockSize + HeaderSize;
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
            var lengthBytes = new byte[HeaderSize];
            await file.ReadAsync(lengthBytes, offset);
            var blockSize = BinaryPrimitives.ReadUInt32LittleEndian(lengthBytes);
            offset += HeaderSize;
            var buffer = MemoryOwner<byte>.Allocate((int)blockSize);
            await file.ReadAsync(buffer.Memory, offset);
            return (blockSize, buffer);
        }
    }
}
