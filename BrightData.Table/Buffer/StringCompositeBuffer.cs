using BrightData.Table.Helper;
using Microsoft.Win32.SafeHandles;
using System.Buffers.Binary;
using System.Text;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.Table.Buffer
{
    class StringCompositeBuffer : CompositeBufferBase<string, StringCompositeBuffer.Block>
    {
        internal record Block(string[] Data) : ICompositeBufferBlock<string>
        {
            public uint Size { get; private set; }
            public ref string GetNext() => ref Data[Size++];
            public bool HasFreeCapacity => Size < Data.Length;
            public ReadOnlySpan<string> WrittenSpan => new(Data, 0, (int)Size);
            public ReadOnlyMemory<string> WrittenMemory => new(Data, 0, (int)Size);

            public void WriteTo(SafeFileHandle file)
            {
                var offset = RandomAccess.GetLength(file);
                var startOffset = offset += 8;
                for (uint i = 0; i < Size; i++) {
                    Encode(Data[i], bytes => {
                        RandomAccess.Write(file, bytes, offset);
                        offset += bytes.Length;
                    });
                }
                var blockSize = (uint)(offset - startOffset);
                Span<byte> lengthBytes = stackalloc byte[8];
                BinaryPrimitives.WriteUInt32LittleEndian(lengthBytes, blockSize);
                BinaryPrimitives.WriteUInt32LittleEndian(lengthBytes[4..], Size);
                RandomAccess.Write(file, lengthBytes, startOffset-8);
            }
        }

        public StringCompositeBuffer(
            IProvideTempStreams? tempStreams = null,
            int blockSize = Consts.DefaultBlockSize,
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        ) : base(x => new(x), tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems)
        {
        }

        public override Task<ReadOnlyMemory<string>> GetBlock(uint blockIndex)
        {
            if(blockIndex >= BlockCount)
                throw new ArgumentOutOfRangeException(nameof(blockIndex), $"Must be less than {BlockCount}");
            return base.GetBlock(blockIndex);
        }

        public override void Add(in string item)
        {
            if(item.Length > ushort.MaxValue/3)
                throw new ArgumentOutOfRangeException(nameof(item), $"Length cannot exceed {ushort.MaxValue/3}");
            base.Add(item);
        }

        protected override uint SkipFileBlock(SafeFileHandle file, long offset)
        {
            Span<byte> lengthBytes = stackalloc byte[4];
            RandomAccess.Read(file, lengthBytes, offset);
            return 8 + BinaryPrimitives.ReadUInt32LittleEndian(lengthBytes);
        }

        protected override ReadOnlyMemory<string> GetBlockFromFile(SafeFileHandle file, long offset)
        {
            var (numStrings, block) = ReadBlock(file, offset);
            try {
                var index = 0;
                var buffer = new Memory<string>(new string[(int)numStrings]);
                Decode(block.Span, chars => {
                    // ReSharper disable once AccessToDisposedClosure
                    buffer.Span[index++] = new string(chars);
                });
                return buffer;
            }
            finally {
                block.Dispose();
            }
        }

        protected override uint GetBlockFromFile(SafeFileHandle file, long offset, BlockCallback<string> callback)
        {
            var (numStrings, block) = ReadBlock(file, offset);
            try {
                var index = 0;
                using var buffer = MemoryOwner<string>.Allocate((int)numStrings);
                Decode(block.Span, chars => {
                    // ReSharper disable once AccessToDisposedClosure
                    buffer.Span[index++] = new string(chars);
                });
                callback(buffer.Span);
                return (uint)block.Length + 8;
            }
            finally {
                block.Dispose();
            }
        }

        (uint NumStrings, MemoryOwner<byte> Block) ReadBlock(SafeFileHandle file, long offset)
        {
            Span<byte> lengthBytes = stackalloc byte[8];
            RandomAccess.Read(file, lengthBytes, offset);
            offset += 8;
            var blockSize = BinaryPrimitives.ReadUInt32LittleEndian(lengthBytes[..4]);
            var numStrings = BinaryPrimitives.ReadUInt32LittleEndian(lengthBytes[4..]);
            var block = MemoryOwner<byte>.Allocate((int)blockSize);
            var readCount = 0;
            do {
                readCount += RandomAccess.Read(file, block.Span[readCount..], offset + readCount);
            }while(readCount < blockSize);

            return (numStrings, block);
        }

        public static void Encode(string str, BlockCallback<byte> callback)
        {
            if (str.Length <= 124 / 3) {
                Span<byte> buffer = stackalloc byte[128];
                var actualByteCount = Encoding.UTF8.GetBytes(str, buffer[4..]);
                BinaryPrimitives.WriteUInt16LittleEndian(buffer, (ushort)str.Length);
                BinaryPrimitives.WriteUInt16LittleEndian(buffer[2..], (ushort)actualByteCount);
                callback(buffer[..(actualByteCount + 4)]);
            }
            else {
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
            do {
                var charSize = BinaryPrimitives.ReadUInt16LittleEndian(data);
                var byteSize = BinaryPrimitives.ReadUInt16LittleEndian(data[2..]);
                data = data[4..];
                if (charSize <= 128) {
                    Encoding.UTF8.GetChars(data[..byteSize], localBuffer[..charSize]);
                    callback(localBuffer[..charSize]);
                }
                else {
                    using var buffer = SpanOwner<char>.Allocate(charSize);
                    Encoding.UTF8.GetChars(data[..byteSize], buffer.Span);
                    callback(buffer.Span);
                }
                data = data[byteSize..];
            } while (data.Length > 0);
        }
    }
}
