using System;
using System.Collections.Concurrent;
using System.IO;

namespace BrightData.Helper
{
    /// <summary>
    /// Manages a collection of temp files
    /// </summary>
    public class TempStreamManager : IProvideTempStreams
    {
        readonly string _basePath;
        readonly ConcurrentDictionary<string, FileStream> _streamTable = new();
        readonly ConcurrentBag<string> _tempPaths = new();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="basePath">Location on disk to write new temp files</param>
        public TempStreamManager(string? basePath = null)
        {
            _basePath = basePath ?? Path.GetTempPath();
        }

        /// <inheritdoc />
        public string GetNewTempPath()
        {
            var ret = Path.Combine(_basePath, Guid.NewGuid().ToString("n"));
            _tempPaths.Add(ret);
            return ret;
        }

        /// <summary>
        /// Returns an existing (or creates a new) temp stream
        /// </summary>
        /// <param name="uniqueId">Unique identifier</param>
        public Stream Get(string uniqueId)
        {
            return _streamTable.GetOrAdd(uniqueId, id => 
                new FileStream(Path.Combine(_basePath, id), FileMode.CreateNew, FileAccess.ReadWrite)
            );
        }

        /// <summary>
        /// Checks if the the stream has been created
        /// </summary>
        /// <param name="uniqueId">Unique identifier</param>
        /// <returns>True if the stream has been created</returns>
        public bool HasStream(string uniqueId) => _streamTable.ContainsKey(uniqueId);

        void IDisposable.Dispose()
        {
            foreach (var (_, value) in _streamTable) {
                value.Flush();
                value.Dispose();
                File.Delete(value.Name);
            }
            _streamTable.Clear();

            foreach (var filePath in _tempPaths) {
                if(File.Exists(filePath))
                    File.Delete(filePath);
            }
            _tempPaths.Clear();
        }
    }
}
