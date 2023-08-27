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
        readonly string                                         _basePath;
        readonly ConcurrentDictionary<string, Lazy<FileStream>> _streamTable = new();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="basePath">Location on disk to write new temp files or null to use default system temp path</param>
        public TempStreamManager(string? basePath = null)
        {
            _basePath = basePath ?? Path.GetTempPath();
        }

        /// <inheritdoc />
        public string GetTempPath(string id) => Path.Combine(_basePath, id);

        /// <summary>
        /// Returns an existing (or creates a new) temp stream
        /// </summary>
        /// <param name="uniqueId">Unique identifier</param>
        public Stream Get(string uniqueId)
        {
            return _streamTable.GetOrAdd(uniqueId, id => 
                new Lazy<FileStream>(() => new FileStream(GetTempPath(id), FileMode.CreateNew, FileAccess.ReadWrite))
            ).Value;
        }

        /// <summary>
        /// Checks if the the stream has been created
        /// </summary>
        /// <param name="uniqueId">Unique identifier</param>
        /// <returns>True if the stream has been created</returns>
        public bool HasStream(string uniqueId) => _streamTable.ContainsKey(uniqueId);

        void IDisposable.Dispose()
        {
            GC.SuppressFinalize(this);
            foreach (var (_, value) in _streamTable) {
                if (value.IsValueCreated) {
                    var file = value.Value;
                    file.Flush();
                    file.Dispose();
                    File.Delete(file.Name);
                }
            }
            _streamTable.Clear();
        }
    }
}
