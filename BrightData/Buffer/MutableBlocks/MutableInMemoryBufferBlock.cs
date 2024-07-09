using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.Buffer.MutableBlocks
{
    internal class MutableInMemoryBufferBlock<T>(T[] Data) : IMutableBufferBlock<T>
    {
        public MutableInMemoryBufferBlock(T[] data, bool existing) : this(data)
        {
            if (existing)
                Size = (uint)data.Length;
        }

        public uint Size { get; private set; }
        public Task<uint> WriteTo(IByteBlockSource file)
        {
            throw new NotImplementedException();
        }

        public bool HasFreeCapacity => Size < Data.Length;
        public ReadOnlySpan<T> WrittenSpan => new(Data, 0, (int)Size);
        public ReadOnlyMemory<T> WrittenMemory => new(Data, 0, (int)Size);
        public ref T GetNext() => ref Data[Size++];
    }
}
