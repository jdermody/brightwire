using System;
using System.Collections.Generic;
using System.IO;

namespace BrightData.Helper
{
    public class TempStreamManager : IDisposable
    {
        readonly string _basePath;
        readonly Dictionary<uint, FileStream> _streamTable = new Dictionary<uint, FileStream>();

        public TempStreamManager(string basePath = null)
        {
            _basePath = basePath ?? Path.GetTempPath();
        }

        public Stream Get(uint index)
        {
            lock (_streamTable) {
                if (!_streamTable.TryGetValue(index, out var stream))
                    _streamTable.Add(index, stream = new FileStream(Path.Combine(_basePath, Guid.NewGuid().ToString("n")), FileMode.CreateNew, FileAccess.ReadWrite));
                return stream;
            }
        }

        public void Dispose()
        {
            foreach (var stream in _streamTable) {
                stream.Value.Flush();
                stream.Value.Dispose();
                File.Delete(stream.Value.Name);
            }
            _streamTable.Clear();
        }
    }
}
