using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance;

namespace BrightData.Buffer.MutableBlocks
{
    internal class MutableUnmanagedBufferBlock<T>(Memory<T> Data) : IMutableBufferBlock<T>
        where T : unmanaged
    {
        public MutableUnmanagedBufferBlock(ReadOnlyMemory<T> data) : this(Unsafe.As<ReadOnlyMemory<T>, Memory<T>>(ref data))
        {
            Size = (uint)data.Length;
        }

        public uint Size { get; private set; }
        public ref T GetNext() => ref Data.Span[(int)Size++];
        public bool HasFreeCapacity => Size < Data.Length;
        public uint AvailableCapacity => (uint)Data.Length - Size;
        public ReadOnlySpan<T> WrittenSpan => Data.Span[..(int)Size];
        public ReadOnlyMemory<T> WrittenMemory => Data[..(int)Size];

        public async Task<uint> WriteTo(IByteBlockSource file)
        {
            var bytes = WrittenMemory.Cast<T, byte>();
            await file.WriteAsync(bytes, file.Size);
            return (uint)bytes.Length;
        }

        public void Write(ReadOnlySpan<T> data)
        {
            data.CopyTo(Data.Span.Slice((int)Size, (int)AvailableCapacity));
            Size += (uint)data.Length;
        }
    }
}
