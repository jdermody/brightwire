using System;
using System.Collections.Generic;
using System.Linq;

namespace BrightData.Helper
{
    /// <summary>
    /// Builds a string table
    /// </summary>
    public class StringIndexer : IIndexStrings
    {
        readonly Dictionary<string, uint> _index = new();

        /// <summary>
        /// Creates a string indexer
        /// </summary>
        /// <param name="strings">Initial strings in table</param>
        public StringIndexer(params string[] strings)
        {
            foreach (var str in strings)
                GetIndex(str);
        }

        /// <summary>
        /// Creates a string indexer
        /// </summary>
        /// <param name="strings">Initial strings in table</param>
        [Obsolete("Please use standard constructor instead")]
        public static StringIndexer Create(params string[] strings) => new StringIndexer(strings);

        /// <summary>
        /// Returns the index of a string (creates it if not already in table)
        /// </summary>
        /// <param name="str">String to search</param>
        /// <returns>String index</returns>
        public uint GetIndex(string str)
        {
            if (_index.TryGetValue(str, out var ret))
                return ret;
            _index.Add(str, ret = (uint)_index.Count);
            return ret;
        }

        /// <summary>
        /// Size of the string table
        /// </summary>
        public uint OutputSize => (uint) _index.Count;

        /// <summary>
        /// Returns all strings by indexed order
        /// </summary>
        public IEnumerable<string> OrderedStrings => _index.OrderBy(kv => kv.Value).Select(kv => kv.Key);
    }
}
