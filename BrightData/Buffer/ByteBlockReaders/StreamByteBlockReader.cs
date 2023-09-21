using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BrightData.Buffer.ByteBlockReaders
{
    internal class StreamByteBlockReader : IByteBlockReader
    {
        readonly SemaphoreSlim _semaphore = new(1);
        readonly Stream _stream;

        public StreamByteBlockReader(Stream stream)
        {
            _stream = stream;
        }

        public void Dispose()
        {
            _stream.Dispose();
        }

        public uint Size => (uint)_stream.Length;
        public async Task<ReadOnlyMemory<byte>> GetBlock(uint byteOffset, uint numBytes)
        {
            await _semaphore.WaitAsync();
            try {
                _stream.Seek(byteOffset, SeekOrigin.Begin);
                var ret = new byte[numBytes];
                await _stream.ReadExactlyAsync(ret);
                return ret;
            }
            finally {
                _semaphore.Release();
            }
        }

        public async Task Update(uint byteOffset, ReadOnlyMemory<byte> data)
        {
            await _semaphore.WaitAsync();
            try {
                _stream.Seek(byteOffset, SeekOrigin.Begin);
                await _stream.WriteAsync(data);
            }
            finally {
                _semaphore.Release();
            }
        }
    }
}
