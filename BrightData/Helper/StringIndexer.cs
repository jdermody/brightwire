using System.Collections.Generic;

namespace BrightData.Helper
{
    /// <summary>
    /// Builds a string table
    /// </summary>
    public class StringIndexer : IIndexStrings
    {
        readonly Dictionary<string, uint> _index = new Dictionary<string, uint>();

        StringIndexer()
        {
        }

        /// <summary>
        /// Creates a string indexer
        /// </summary>
        /// <param name="strings">Initial strings in table</param>
        public static StringIndexer Create(params string[] strings)
        {
            var ret = new StringIndexer();
            foreach (var str in strings)
                ret.GetIndex(str);
            return ret;
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
            _index.Add(str, ret = (uint)_index.Count);
            return ret;
        }

        /// <summary>
        /// Size of the string table
        /// </summary>
        public uint OutputSize => (uint) _index.Count;
    }
}
