using System;
using System.Buffers.Binary;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.Buffer.MutableBlocks
{
    internal class MutableStringBufferBlock(string[] Data) : IMutableBufferBlock<string>
    {
        public MutableStringBufferBlock(string[] data, bool existing) : this(data)
        {
            if (existing)
                Size = (uint)data.Length;
        }

        public uint Size { get; private set; }
        public ref string GetNext() => ref Data[Size++];
        public bool HasFreeCapacity => Size < Data.Length;
        public ReadOnlySpan<string> WrittenSpan => new(Data, 0, (int)Size);
        public ReadOnlyMemory<string> WrittenMemory => new(Data, 0, (int)Size);

        public const int HeaderSize = 8;
        public async Task<uint> WriteTo(IByteBlockSource file)
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
            var blockSize = offset - startOffset;
            Memory<byte> lengthBytes = new byte[HeaderSize];
            BinaryPrimitives.WriteUInt32LittleEndian(lengthBytes.Span, blockSize);
            BinaryPrimitives.WriteUInt32LittleEndian(lengthBytes.Span[4..], Size);
            await file.WriteAsync(lengthBytes, startOffset - HeaderSize);
            return blockSize + HeaderSize;
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
