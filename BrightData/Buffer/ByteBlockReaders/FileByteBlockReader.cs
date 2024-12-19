using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

namespace BrightData.Buffer.ByteBlockReaders
{
    /// <summary>
    /// Reads from a file
    /// </summary>
    internal class FileByteBlockReader : IByteBlockReader
    {
        readonly SafeFileHandle _file;

        public FileByteBlockReader(string path)
        {
            if (!File.Exists(path))
                throw new ArgumentException("Path does not exist on disk", nameof(path));
            Path = path;
            _file = File.OpenHandle(Path, FileMode.Open, FileAccess.ReadWrite, FileShare.None, FileOptions.Asynchronous | FileOptions.RandomAccess);
            Size = (uint)RandomAccess.GetLength(_file);
        }

        public void Dispose()
        {
            _file.Dispose();
        }

        public async Task<ReadOnlyMemory<byte>> GetBlock(uint offset, uint numBytes)
        {
            if (numBytes == 0)
                return ReadOnlyMemory<byte>.Empty;

            Memory<byte> ret = new(new byte[numBytes]);
            var readOffset = 0;
            do
            {
                readOffset += await RandomAccess.ReadAsync(_file, ret[readOffset..], offset + readOffset);
            } while (readOffset < numBytes);
            return ret;
        }

        public async Task Update(uint byteOffset, ReadOnlyMemory<byte> data)
        {
            await RandomAccess.WriteAsync(_file, data, byteOffset);
        }

        public string Path { get; set; }
        public uint Size { get; }
    }
}
