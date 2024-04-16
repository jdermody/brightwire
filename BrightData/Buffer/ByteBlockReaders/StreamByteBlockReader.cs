using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BrightData.Buffer.ByteBlockReaders
{
    /// <summary>
    /// Reads from a stream
    /// </summary>
    /// <param name="stream"></param>
    internal class StreamByteBlockReader(Stream stream) : IByteBlockReader
    {
        readonly SemaphoreSlim _semaphore = new(1);

        public void Dispose()
        {
            stream.Dispose();
        }

        public uint Size => (uint)stream.Length;
        public async Task<ReadOnlyMemory<byte>> GetBlock(uint byteOffset, uint numBytes)
        {
            await _semaphore.WaitAsync();
            try {
                stream.Seek(byteOffset, SeekOrigin.Begin);
                var ret = new byte[numBytes];
                await stream.ReadExactlyAsync(ret);
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
                stream.Seek(byteOffset, SeekOrigin.Begin);
                await stream.WriteAsync(data);
            }
            finally {
                _semaphore.Release();
            }
        }
    }
}
