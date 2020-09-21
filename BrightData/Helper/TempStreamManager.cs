using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace BrightData.Helper
{
    public class TempStreamManager : IProvideTempStreams
    {
        readonly string _basePath;
        readonly ConcurrentDictionary<string, FileStream> _streamTable = new ConcurrentDictionary<string, FileStream>();

        public TempStreamManager(string basePath = null)
        {
            _basePath = basePath ?? Path.GetTempPath();
        }

        public Stream Get(string uniqueId)
        {
            return _streamTable.GetOrAdd(uniqueId, id => 
                new FileStream(Path.Combine(_basePath, id), FileMode.CreateNew, FileAccess.ReadWrite)
            );
        }

        public bool HasStream(string uniqueId) => _streamTable.ContainsKey(uniqueId);

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
