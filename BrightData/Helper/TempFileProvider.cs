using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Win32.SafeHandles;

namespace BrightData.Table.Helper
{
    internal class TempFileProvider(string? basePath = null) : IProvideDataBlocks
    {
        class TempData(Guid id, string path) : IDataBlock
        {
            readonly SafeFileHandle _file = File.OpenHandle(path, FileMode.Create, FileAccess.ReadWrite, FileShare.Read, FileOptions.DeleteOnClose | FileOptions.Asynchronous);

            public void Dispose()
            {
                _file.Dispose();
            }

            public Guid Id { get; } = id;

            public void Write(ReadOnlySpan<byte> data, uint offset)
            {
                RandomAccess.Write(_file, data, offset);
            }

            public ValueTask WriteAsync(ReadOnlyMemory<byte> data, uint offset)
            {
                return RandomAccess.WriteAsync(_file, data, offset);
            }

            public uint Read(Span<byte> data, uint offset)
            {
                return (uint)RandomAccess.Read(_file, data, offset);;
            }
            public async Task<uint> ReadAsync(Memory<byte> data, uint offset)
            {
                return (uint)await RandomAccess.ReadAsync(_file, data, offset);
            }

            public uint Size => (uint)RandomAccess.GetLength(_file);
        }
        readonly string _basePath = basePath ?? Path.GetTempPath();
        // lazy to prevent multiple files from being created per key
        readonly ConcurrentDictionary<Guid, Lazy<IDataBlock>> _fileTable = new();

        ~TempFileProvider()
        {
            Clear();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Clear();
        }

        public void Clear()
        {
            foreach (var (_, file) in _fileTable) {
                if (file.IsValueCreated)
                    file.Value.Dispose();
            }
            _fileTable.Clear();
        }

        public IDataBlock Get(Guid id)
        {
            return _fileTable.GetOrAdd(id, fid => 
                new Lazy<IDataBlock>(() => new TempData(id, Path.Combine(_basePath, fid.ToString("n"))))
            ).Value;
        }
    }
}
