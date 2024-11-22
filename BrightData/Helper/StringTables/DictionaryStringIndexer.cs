using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace BrightData.Helper.StringTables
{
    /// <summary>
    /// Indexes strings in memory using a dictionary
    /// </summary>
    public class DictionaryStringIndexer : IIndexStrings
    {
        readonly Dictionary<string, uint> _index = [];
        readonly Lock _lock = new();

        /// <summary>
        /// Creates a string indexer
        /// </summary>
        /// <param name="strings">Initial strings in table</param>
        public DictionaryStringIndexer(params string[] strings)
        {
            foreach (var str in strings)
                GetIndex(str);
        }

        /// <summary>
        /// Returns the index of a string (creates it if not already in table)
        /// </summary>
        /// <param name="str">String to search</param>
        /// <returns>String index</returns>
        [SkipLocalsInit]
        public uint GetIndex(ReadOnlySpan<char> str)
        {
            // convert to lowercase
            uint ret;
            Span<char> temp = stackalloc char[str.Length];
            var size = str.Trim().ToLowerInvariant(temp);

            // fast path
            if (size >= 0) {
                var lookup = _index.GetAlternateLookup<ReadOnlySpan<char>>();
                var lowerStr = temp[..size].AsReadOnly();
                if (!lookup.TryGetValue(lowerStr, out ret)) {
                    lock (_lock) {
                        if (lookup.TryGetValue(lowerStr, out ret))
                            return ret;
                        if (!lookup.TryAdd(lowerStr, ret = (uint)_index.Count))
                            throw new Exception($"Not able to add {lowerStr} to dictionary");
                    }
                }
                return ret;
            }

            // fallback
            var str2 = str.Trim().ToString().ToLowerInvariant();
            if (_index.TryGetValue(str2, out ret))
                return ret;
            lock (_lock) {
                if (_index.TryGetValue(str2, out ret))
                    return ret;
                if (!_index.TryAdd(str2, ret = (uint)_index.Count))
                    throw new Exception($"Not able to add {str2} to dictionary");
            }
            return ret;
        }

        /// <summary>
        /// Size of the string table
        /// </summary>
        public uint Size => (uint)_index.Count;

        /// <summary>
        /// Returns all strings by indexed order
        /// </summary>
        public IEnumerable<string> OrderedStrings => _index
            .OrderBy(kv => kv.Value)
            .Select(kv => kv.Key)
        ;
    }
}
