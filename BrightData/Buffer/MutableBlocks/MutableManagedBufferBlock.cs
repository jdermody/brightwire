using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.Buffer.MutableBlocks
{
    internal class MutableManagedBufferBlock<T>(T[] Data) : IMutableBufferBlock<T>
        where T : IHaveDataAsReadOnlyByteSpan
    {
        public MutableManagedBufferBlock(T[] data, bool existing) : this(data)
        {
            if (existing)
                Size = (uint)data.Length;
        }

        public const int HeaderSize = 8;
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
}
