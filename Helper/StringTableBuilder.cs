using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BrightWire.Models;

namespace BrightWire.Helper
{
    /// <summary>
    /// Assigns string indices to strings
    /// </summary>
    public class StringTableBuilder
    {
        readonly Dictionary<string, uint> _stringTable = new Dictionary<string, uint>();

        /// <summary>
        /// Gets a string index for a string
        /// </summary>
        /// <param name="str">The string to look up</param>
        public uint GetIndex(string str)
        {
            uint ret;
            if (!_stringTable.TryGetValue(str, out ret))
                _stringTable.Add(str, ret = (uint)_stringTable.Count);
            return ret;
        }

        /// <summary>
        /// Serialises the string table
        /// </summary>
        public StringTable StringTable
        {
            get
            {
                return new StringTable {
                    Data = _stringTable.OrderBy(kv => kv.Value).Select(kv => kv.Key).ToArray()
                };
            }
        }
    }
}
