using Microsoft.Win32.SafeHandles;
using System.IO.MemoryMappedFiles;

namespace BrightData.Table.ByteReaders
{
    internal class FileByteReader : IByteReader
    {
        SafeFileHandle? _file;

        public FileByteReader(string path)
        {
            if (!File.Exists(path))
                throw new ArgumentException("Path does not exist on disk", nameof(path));
            Path = path;
        }

        public void Dispose()
        {
            if(_file is not null)
                _file.Dispose();
        }

        public async Task<ReadOnlyMemory<byte>> GetBlock(uint offset, uint numBytes)
        {
            Memory<byte> ret = new(new byte[numBytes]);
            var readOffset = 0;
            do {
                var file = _file ??= File.OpenHandle(Path, FileMode.Open, FileAccess.Read, FileShare.Read, FileOptions.Asynchronous | FileOptions.RandomAccess);
                readOffset += await RandomAccess.ReadAsync(file, ret[readOffset..], offset + readOffset);
            } while (readOffset < numBytes);
            return ret;
        }

        public string Path { get; set; }
    }
}
