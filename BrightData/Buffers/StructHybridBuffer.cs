using System;
using System.IO;
using System.Runtime.InteropServices;

namespace BrightData.Buffers
{
    internal class StructHybridBuffer<T> : HybridBufferBase<T>
        where T: struct
    {
        public StructHybridBuffer(IProvideTempStreams tempStream, uint maxCount, ushort maxDistinct) : base(tempStream, maxCount, maxDistinct)
        {
        }

        protected override void _WriteTo(ReadOnlySpan<T> ptr, Stream stream)
        {
            var bytes = MemoryMarshal.Cast<T, byte>(ptr);
            stream.Write(bytes);
        }

        protected override uint _ReadTo(Stream stream, uint count, T[] buffer)
        {
            var ptr = MemoryMarshal.Cast<T, byte>(buffer);
            var len = stream.Read(ptr);
            var sizeOfT = ptr.Length / buffer.Length;
            return (uint)len / (uint)sizeOfT;
        }
    }
}
