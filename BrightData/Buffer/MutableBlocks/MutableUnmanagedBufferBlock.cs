using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance;

namespace BrightData.Buffer.MutableBlocks
{
    internal class MutableUnmanagedBufferBlock<T>(T[] Data) : IMutableBufferBlock<T>
        where T : unmanaged
    {
        public MutableUnmanagedBufferBlock(T[] data, bool existing) : this(data)
        {
            if (existing)
                Size = (uint)data.Length;
        }

        public uint Size { get; private set; }
        public ref T GetNext() => ref Data[Size++];
        public bool HasFreeCapacity => Size < Data.Length;
        public uint AvailableCapacity => (uint)Data.Length - Size;
        public ReadOnlySpan<T> WrittenSpan => new(Data, 0, (int)Size);
        public ReadOnlyMemory<T> WrittenMemory => new(Data, 0, (int)Size);

        public async Task<uint> WriteTo(IByteBlockSource file)
        {
            var bytes = WrittenMemory.Cast<T, byte>();
            await file.WriteAsync(bytes, file.Size);
            return (uint)bytes.Length;
        }

        public void Write(ReadOnlySpan<T> data)
        {
            data.CopyTo(Data.AsSpan((int)Size, (int)AvailableCapacity));
            Size += (uint)data.Length;
        }
    }
}
