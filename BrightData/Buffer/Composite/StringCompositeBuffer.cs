using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.Buffer.Composite
{
    /// <summary>
    /// A composite buffer for strings
    /// </summary>
    /// <param name="tempStreams"></param>
    /// <param name="blockSize"></param>
    /// <param name="maxBlockSize"></param>
    /// <param name="maxInMemoryBlocks"></param>
    /// <param name="maxDistinctItems"></param>
    internal class StringCompositeBuffer(
        IProvideByteBlocks? tempStreams = null,
        int blockSize = Consts.DefaultInitialBlockSize,
        int maxBlockSize = Consts.DefaultMaxBlockSize,
        uint? maxInMemoryBlocks = null,
        uint? maxDistinctItems = null
    )
        : CompositeBufferBase<string, StringCompositeBuffer.Block>(x => new(x), x => new(x), tempStreams, blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems)
    {
        public class Block(Memory<string> Data) : IMutableBufferBlock<string>
        {
            public Block(ReadOnlyMemory<string> data) : this(Unsafe.As<ReadOnlyMemory<string>, Memory<string>>(ref data))
            {
                Size = (uint)data.Length;
            }

            public uint Size { get; private set; }
            public ref string GetNext() => ref Data.Span[(int)Size++];
            public bool HasFreeCapacity => Size < Data.Length;
            public ReadOnlySpan<string> WrittenSpan => Data.Span[..(int)Size];
            public ReadOnlyMemory<string> WrittenMemory => Data[..(int)Size];

            public const int HeaderSize = 8;
            public Task<uint> WriteToAsync(IByteBlockSource file) => Task.FromResult(WriteTo(file));
            public uint WriteTo(IByteBlockSource file)
            {
                var offset = file.Size;
                var startOffset = offset += HeaderSize;
                for (var i = 0; i < Size; i++)
                {
                    Encode(Data.Span[i], bytes =>
                    {
                        file.Write(bytes, offset);
                        offset += (uint)bytes.Length;
                    });
                }
                var blockSize = offset - startOffset;
                Span<byte> lengthBytes = stackalloc byte[HeaderSize];
                BinaryPrimitives.WriteUInt32LittleEndian(lengthBytes, blockSize);
                BinaryPrimitives.WriteUInt32LittleEndian(lengthBytes[4..], Size);
                file.Write(lengthBytes, startOffset - HeaderSize);
                return blockSize + HeaderSize;
            }

            public static void Encode(string str, BlockCallback<byte> callback)
            {
                if (str.Length <= 124 / 4)
                {
                    Span<byte> buffer = stackalloc byte[128];
                    var actualByteCount = Encoding.UTF8.GetBytes(str, buffer[4..]);
                    BinaryPrimitives.WriteUInt16LittleEndian(buffer, (ushort)str.Length);
                    BinaryPrimitives.WriteUInt16LittleEndian(buffer[2..], (ushort)actualByteCount);
                    callback(buffer[..(actualByteCount + 4)]);
                }
                else
                {
                    using var buffer = SpanOwner<byte>.Allocate(str.Length * 4 + 4);
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
            return Block.HeaderSize + BinaryPrimitives.ReadUInt32LittleEndian(lengthBytes);
        }

        protected override async Task<(uint BlockSizeBytes, ReadOnlyMemory<string> Block)> GetBlockFromFile(IByteBlockSource file, uint byteOffset, uint numItemsInBlock)
        {
            var (numStrings, block) = await ReadBlock(file, byteOffset);
            try
            {
                var index = 0;
                var buffer = new Memory<string>(new string[(int)numStrings]);
                Block.Decode(block.Span, chars =>
                {
                    // ReSharper disable once AccessToDisposedClosure
                    buffer.Span[index++] = new string(chars);
                });
                return ((uint)block.Length + Block.HeaderSize, buffer);
            }
            finally
            {
                block.Dispose();
            }
        }

        static async Task<(uint NumStrings, MemoryOwner<byte> Block)> ReadBlock(IByteBlockSource file, uint offset)
        {
            var lengthBytes = new byte[Block.HeaderSize];
            await file.ReadAsync(lengthBytes, offset);
            offset += Block.HeaderSize;
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
