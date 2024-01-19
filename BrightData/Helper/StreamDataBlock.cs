using System;
using System.IO;
using System.Threading.Tasks;

namespace BrightData.Helper
{
    /// <summary>
    /// Stream to data block adaptor
    /// </summary>
    /// <param name="id"></param>
    /// <param name="stream"></param>
    internal class StreamDataBlock(Guid id, Stream stream) : IByteBlockSource
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
