using BrightData.Table.Helper;
using Microsoft.Win32.SafeHandles;
using System.Buffers.Binary;
using System.Text;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.Table.Buffer.Composite
{
    class StringCompositeBuffer : CompositeBufferBase<string, StringCompositeBuffer.Block>
    {
        const int HeaderSize = 8;
        internal record Block(string[] Data) : ICompositeBufferBlock<string>
        {
            public uint Size { get; private set; }
            public ref string GetNext() => ref Data[Size++];
            public bool HasFreeCapacity => Size < Data.Length;
            public ReadOnlySpan<string> WrittenSpan => new(Data, 0, (int)Size);
            public ReadOnlyMemory<string> WrittenMemory => new(Data, 0, (int)Size);

            public void WriteTo(ITempData file)
            {
                var offset = file.Size;
                var startOffset = offset += HeaderSize;
                for (uint i = 0; i < Size; i++)
                {
                    Encode(Data[i], bytes =>
                    {
                        file.Write(bytes, offset);
                        offset += (uint)bytes.Length;
                    });
                }
                var blockSize = (uint)(offset - startOffset);
                Span<byte> lengthBytes = stackalloc byte[8];
                BinaryPrimitives.WriteUInt32LittleEndian(lengthBytes, blockSize);
                BinaryPrimitives.WriteUInt32LittleEndian(lengthBytes[4..], Size);
                file.Write(lengthBytes, startOffset - HeaderSize);
            }
        }

        public StringCompositeBuffer(
            IProvideTempData? tempStreams = null,
            int blockSize = Consts.DefaultBlockSize,
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        ) : base(x => new(x), tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems)
        {
        }

        public override Task<ReadOnlyMemory<string>> GetBlock(uint blockIndex)
        {
            if (blockIndex >= BlockCount)
                throw new ArgumentOutOfRangeException(nameof(blockIndex), $"Must be less than {BlockCount}");
            return base.GetBlock(blockIndex);
        }

        public override void Add(in string item)
        {
            if (item.Length > ushort.MaxValue / 3)
                throw new ArgumentOutOfRangeException(nameof(item), $"Length cannot exceed {ushort.MaxValue / 3}");
            base.Add(item);
        }

        protected override async Task<uint> SkipFileBlock(ITempData file, uint offset)
        {
            var lengthBytes = new byte[4];
            await file.ReadAsync(lengthBytes, offset);
            return HeaderSize + BinaryPrimitives.ReadUInt32LittleEndian(lengthBytes);
        }

        protected override async Task<(uint, ReadOnlyMemory<string>)> GetBlockFromFile(ITempData file, uint offset)
        {
            var (numStrings, block) = await ReadBlock(file, offset);
            try
            {
                var index = 0;
                var buffer = new Memory<string>(new string[(int)numStrings]);
                Decode(block.Span, chars =>
                {
                    // ReSharper disable once AccessToDisposedClosure
                    buffer.Span[index++] = new string(chars);
                });
                return ((uint)block.Length + HeaderSize, buffer);
            }
            finally
            {
                block.Dispose();
            }
        }

        protected override async Task<uint> GetBlockFromFile(ITempData file, uint offset, BlockCallback<string> callback)
        {
            var (numStrings, block) = await ReadBlock(file, offset);
            try
            {
                var index = 0;
                using var buffer = MemoryOwner<string>.Allocate((int)numStrings);
                Decode(block.Span, chars =>
                {
                    // ReSharper disable once AccessToDisposedClosure
                    buffer.Span[index++] = new string(chars);
                });
                callback(buffer.Span);
                return (uint)block.Length + HeaderSize;
            }
            finally
            {
                block.Dispose();
            }
        }

        async Task<(uint NumStrings, MemoryOwner<byte> Block)> ReadBlock(ITempData file, uint offset)
        {
            var lengthBytes = new byte[HeaderSize];
            await file.ReadAsync(lengthBytes, offset);
            offset += HeaderSize;
            var blockSize = BinaryPrimitives.ReadUInt32LittleEndian(lengthBytes);
            var numStrings = BinaryPrimitives.ReadUInt32LittleEndian(lengthBytes[4..]);
            var block = MemoryOwner<byte>.Allocate((int)blockSize);
            uint readCount = 0;
            do
            {
                readCount += await file.ReadAsync(block.Memory[(int)readCount..], offset + readCount);
            } while (readCount < blockSize);

            return (numStrings, block);
        }

        public static void Encode(string str, BlockCallback<byte> callback)
        {
            if (str.Length <= 124 / 3)
            {
                Span<byte> buffer = stackalloc byte[128];
                var actualByteCount = Encoding.UTF8.GetBytes(str, buffer[4..]);
                BinaryPrimitives.WriteUInt16LittleEndian(buffer, (ushort)str.Length);
                BinaryPrimitives.WriteUInt16LittleEndian(buffer[2..], (ushort)actualByteCount);
                callback(buffer[..(actualByteCount + 4)]);
            }
            else
            {
                using var buffer = SpanOwner<byte>.Allocate(str.Length * 3 + 2);
                var actualByteCount = Encoding.UTF8.GetBytes(str, buffer.Span[4..]);
                BinaryPrimitives.WriteUInt16LittleEndian(buffer.Span, (ushort)str.Length);
                BinaryPrimitives.WriteUInt16LittleEndian(buffer.Span[2..], (ushort)actualByteCount);
                callback(buffer.Span[..(actualByteCount + 4)]);
            }
        }

        public static void Decode(ReadOnlySpan<byte> data, BlockCallback<char> callback)
        {
            Span<char> localBuffer = stackalloc char[128];
            do
            {
                var charSize = BinaryPrimitives.ReadUInt16LittleEndian(data);
                var byteSize = BinaryPrimitives.ReadUInt16LittleEndian(data[2..]);
                data = data[4..];
                if (charSize <= 128)
                {
                    Encoding.UTF8.GetChars(data[..byteSize], localBuffer[..charSize]);
                    callback(localBuffer[..charSize]);
                }
                else
                {
                    using var buffer = SpanOwner<char>.Allocate(charSize);
                    Encoding.UTF8.GetChars(data[..byteSize], buffer.Span);
                    callback(buffer.Span);
                }
                data = data[byteSize..];
            } while (data.Length > 0);
        }
    }
}
