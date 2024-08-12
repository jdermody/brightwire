using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace BrightData.Buffer.MutableBlocks
{
    internal class MutableInMemoryBufferBlock<T>(Memory<T> Data) : IMutableBufferBlock<T>
    {
        public MutableInMemoryBufferBlock(ReadOnlyMemory<T> data) : this(Unsafe.As<ReadOnlyMemory<T>, Memory<T>>(ref data))
        {
            Size = (uint)data.Length;
        }

        public uint Size { get; private set; }
        public Task<uint> WriteTo(IByteBlockSource file)
        {
            throw new NotImplementedException();
        }

        public bool HasFreeCapacity => Size < Data.Length;
        public ReadOnlySpan<T> WrittenSpan => Data.Span[..(int)Size];
        public ReadOnlyMemory<T> WrittenMemory => Data[..(int)Size];
        public ref T GetNext() => ref Data.Span[(int)Size++];
    }
}
