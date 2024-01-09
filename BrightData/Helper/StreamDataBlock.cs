using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.Helper
{
    internal class StreamDataBlock(Guid id, Stream stream) : IDataBlock
    {
        public void Dispose()
        {
            stream.Dispose();
        }

        public uint Size => (uint)stream.Length;
        public Guid Id { get; } = id;

        public void Write(ReadOnlySpan<byte> data, uint offset)
        {
            stream.Seek(offset, SeekOrigin.Begin);
            stream.Write(data);
        }

        public ValueTask WriteAsync(ReadOnlyMemory<byte> data, uint offset)
        {
            stream.Seek(offset, SeekOrigin.Begin);
            return stream.WriteAsync(data);
        }

        public uint Read(Span<byte> data, uint offset)
        {
            stream.Seek(offset, SeekOrigin.Begin);
            return (uint)stream.Read(data);
        }

        public async Task<uint> ReadAsync(Memory<byte> data, uint offset)
        {
            stream.Seek(offset, SeekOrigin.Begin);
            var ret = await stream.ReadAsync(data);
            return (uint)ret;
        }
    }
}
