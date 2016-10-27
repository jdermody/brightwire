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
        readonly List<string> _stringList = new List<string>();

        /// <summary>
        /// Returns true if the string has already been added
        /// </summary>
        /// <param name="str">The string to check</param>
        /// <param name="ret">The string index</param>
        public bool TryGetIndex(string str, out uint ret)
        {
            return _stringTable.TryGetValue(str, out ret);
        }

        /// <summary>
        /// Gets a string index for a string (creates a new index if not found)
        /// </summary>
        /// <param name="str">The string to look up</param>
        public uint GetIndex(string str)
        {
            uint ret;
            if (!_stringTable.TryGetValue(str, out ret)) {
                _stringTable.Add(str, ret = (uint)_stringTable.Count);
                _stringList.Add(str);
            }
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
                    Data = _stringList.ToArray()
                };
            }
        }

        /// <summary>
        /// Returns the size of the string table
        /// </summary>
        public uint Size { get { return (uint)_stringTable.Count; } }

        /// <summary>
        /// Returns the string at the specified index
        /// </summary>
        /// <param name="index">The string index</param>
        public string GetString(uint index)
        {
            return _stringList[(int)index];
        }
    }
}
