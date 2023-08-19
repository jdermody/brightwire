using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Win32.SafeHandles;

namespace BrightData.Table.Helper
{
    internal class TempFileProvider : IDisposable, IProvideTempStreams
    {
        readonly string _basePath;
        // lazy to prevent multiple files from being created per key
        readonly ConcurrentDictionary<Guid, Lazy<SafeFileHandle>> _fileTable = new();

        public TempFileProvider(string? basePath = null)
        {
            _basePath = basePath ?? Path.GetTempPath();
        }

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

        public SafeFileHandle Get(Guid id)
        {
            return _fileTable.GetOrAdd(id, fid => 
                new Lazy<SafeFileHandle>(() => File.OpenHandle(Path.Combine(_basePath, fid.ToString("n")), FileMode.Create, FileAccess.ReadWrite, FileShare.Read, FileOptions.DeleteOnClose | FileOptions.Asynchronous))
            ).Value;
        }
    }
}
