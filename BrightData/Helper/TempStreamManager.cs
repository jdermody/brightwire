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
        readonly ConcurrentDictionary<string, FileStream> _streamTable = new ConcurrentDictionary<string, FileStream>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="basePath">Location on disk to write new temp files</param>
        public TempStreamManager(string? basePath = null)
        {
            _basePath = basePath ?? Path.GetTempPath();
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
            foreach (var stream in _streamTable) {
                stream.Value.Flush();
                stream.Value.Dispose();
                File.Delete(stream.Value.Name);
            }
            _streamTable.Clear();
        }
    }
}
