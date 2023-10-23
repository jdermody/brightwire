using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.Helper
{
    internal class StreamDataBlock : IDataBlock
    {
        readonly Stream _stream;

        public StreamDataBlock(Guid id, Stream stream)
        {
            Id = id;
            _stream = stream;
        }

        public void Dispose()
        {
            _stream.Dispose();
        }

        public uint Size => (uint)_stream.Length;
        public Guid Id { get; }
        public void Write(ReadOnlySpan<byte> data, uint offset)
        {
            _stream.Seek(offset, SeekOrigin.Begin);
            _stream.Write(data);
        }

        public ValueTask WriteAsync(ReadOnlyMemory<byte> data, uint offset)
        {
            _stream.Seek(offset, SeekOrigin.Begin);
            return _stream.WriteAsync(data);
        }

        public uint Read(Span<byte> data, uint offset)
        {
            _stream.Seek(offset, SeekOrigin.Begin);
            return (uint)_stream.Read(data);
        }

        public async Task<uint> ReadAsync(Memory<byte> data, uint offset)
        {
            _stream.Seek(offset, SeekOrigin.Begin);
            var ret = await _stream.ReadAsync(data);
            return (uint)ret;
        }
    }
}
