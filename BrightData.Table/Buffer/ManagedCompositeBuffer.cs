using BrightData.Table.Helper;
using CommunityToolkit.HighPerformance.Buffers;
using Microsoft.Win32.SafeHandles;
using System.Buffers;
using System.Buffers.Binary;

namespace BrightData.Table.Buffer
{
    internal class ManagedCompositeBuffer<T> : CompositeBufferBase<T, ManagedCompositeBuffer<T>.Block> where T: IHaveDataAsReadOnlyByteSpan
    {
        internal record Block(T[] Data) : ICompositeBufferBlock<T>
        {
            public uint Size { get; private set; }
            public ref T GetNext() => ref Data[Size++];
            public bool HasFreeCapacity => Size < Data.Length;
            public ReadOnlySpan<T> WrittenSpan => new(Data, 0, (int)Size);
            public ReadOnlyMemory<T> WrittenMemory => new(Data, 0, (int)Size);

            public void WriteTo(SafeFileHandle file)
            {
                var offset = RandomAccess.GetLength(file);
                using var writer = new ArrayPoolBufferWriter<byte>();
                var memoryOwner = (IMemoryOwner<byte>)writer;
                writer.Advance(4);
                var size = 0;
                for (uint i = 0; i < Size; i++) {
                    var itemData = Data[i].DataAsBytes;
                    var span = writer.GetSpan(itemData.Length + 4);
                    BinaryPrimitives.WriteUInt32LittleEndian(span, (uint)itemData.Length);
                    itemData.CopyTo(span[4..]);
                    writer.Advance(itemData.Length + 4);
                    size += itemData.Length + 4;
                }
                BinaryPrimitives.WriteUInt32LittleEndian(memoryOwner.Memory.Span, (uint)size);
                RandomAccess.Write(file, writer.WrittenSpan, offset);
            }
        }

        readonly CreateFromReadOnlyByteSpan<T> _createItem;

        public ManagedCompositeBuffer(
            CreateFromReadOnlyByteSpan<T> createItem,
            IProvideTempStreams? tempStreams = null,
            int blockSize = Consts.DefaultBlockSize,
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        ) : base(x => new(x), tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems)
        {
            _createItem            = createItem;
        }

        protected override uint SkipFileBlock(SafeFileHandle file, long offset)
        {
            Span<byte> lengthBytes = stackalloc byte[4];
            var blockSize = BinaryPrimitives.ReadUInt32LittleEndian(lengthBytes);
            return blockSize + 4;
        }

        protected override ReadOnlyMemory<T> GetBlockFromFile(SafeFileHandle file, long offset)
        {
            var (_, buffer) = ReadBuffer(file, offset);
            try {
                var buffer2 = new Memory<T>(new T[_blockSize]);
                Copy(buffer.Span, buffer2.Span);
                return buffer2;
            }
            finally {
                buffer.Dispose();
            }
        }

        protected override uint GetBlockFromFile(SafeFileHandle file, long offset, BlockCallback<T> callback)
        {
            var (blockSize, buffer) = ReadBuffer(file, offset);
            try {
                using var buffer2 = SpanOwner<T>.Allocate(_blockSize);
                Copy(buffer.Span, buffer2.Span);
                callback(buffer2.Span);
                return blockSize + 4;
            }
            finally {
                buffer.Dispose();
            }
        }

        void Copy(ReadOnlySpan<byte> inputSpan, Span<T> outputSpan)
        {
            for (var i = 0; i < _blockSize; i++) {
                var itemSize = BinaryPrimitives.ReadUInt32LittleEndian(inputSpan);
                var itemData = inputSpan[4..(int)(itemSize + 4)];
                outputSpan[i] = _createItem(itemData);
                inputSpan = inputSpan[(int)(itemSize + 4)..];
            }
        }

        static (uint BlockSize, MemoryOwner<byte> Buffer) ReadBuffer(SafeFileHandle file, long offset)
        {
            Span<byte> lengthBytes = stackalloc byte[4];
            RandomAccess.Read(file, lengthBytes, offset);
            var blockSize = BinaryPrimitives.ReadUInt32LittleEndian(lengthBytes);
            offset += 4;
            var buffer = MemoryOwner<byte>.Allocate((int)blockSize);
            var inputSpan = buffer.Span;
            var readCount = 0;
            do {
                readCount += RandomAccess.Read(file, inputSpan[readCount..], offset + readCount);
            }while(readCount < blockSize);

            return (blockSize, buffer);
        }
    }
}
