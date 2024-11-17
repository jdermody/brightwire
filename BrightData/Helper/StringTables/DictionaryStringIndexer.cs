using System;
using System.Collections.Generic;
using System.Linq;

namespace BrightData.Helper.StringTables
{
    /// <summary>
    /// Builds a string table in memory using a dictionary
    /// </summary>
    public class DictionaryStringIndexer : IIndexStrings
    {
        readonly Dictionary<string, uint> _index = new();

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
        public uint GetIndex(string str)
        {
            if (_index.TryGetValue(str, out var ret))
                return ret;
            lock (_index) {
                if (_index.TryGetValue(str, out ret))
                    return ret;
                _index.Add(str, ret = (uint)_index.Count);
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
