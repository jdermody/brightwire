using BrightWire.Models;
using System.Collections.Generic;

namespace BrightWire.TrainingData.Helper
{
    /// <summary>
    /// Assigns string indices to strings
    /// </summary>
    public class StringTableBuilder
    {
        readonly Dictionary<string, uint> _stringTable = new();
        readonly List<string> _stringList = [];

        /// <summary>
        /// Creates an empty string table builder
        /// </summary>
        public StringTableBuilder() { }

        /// <summary>
        /// Creates a string table builder pre-populated with an existing string table
        /// </summary>
        /// <param name="stringTable">The string table to pre-populate</param>
        public StringTableBuilder(StringTable stringTable) : this(stringTable.Data) { }

        /// <summary>
        /// Creates a string table builder pre-populated with an existing string table
        /// </summary>
        /// <param name="stringTable">The string table to pre-populate</param>
        public StringTableBuilder(string[] stringTable)
        {
            for (uint i = 0; i < stringTable.Length; i++) {
                var str = stringTable[i];
                _stringTable.Add(str, i);
                _stringList.Add(str);
            }
        }

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
            if (!_stringTable.TryGetValue(str, out uint ret)) {
                _stringTable.Add(str, ret = (uint)_stringTable.Count);
                _stringList.Add(str);
            }
            return ret;
        }

        /// <summary>
        /// Serialises the string table
        /// </summary>
        public StringTable StringTable => new() {
	        Data = _stringList.ToArray()
        };

	    /// <summary>
        /// Returns the size of the string table
        /// </summary>
        public uint Size => (uint)_stringTable.Count;

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
