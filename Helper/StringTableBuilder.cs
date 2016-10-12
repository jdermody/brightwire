using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BrightWire.Models;

namespace BrightWire.Helper
{
    public class StringTableBuilder
    {
        readonly Dictionary<string, uint> _stringTable = new Dictionary<string, uint>();

        public uint GetIndex(string str)
        {
            uint ret;
            if (!_stringTable.TryGetValue(str, out ret))
                _stringTable.Add(str, ret = (uint)_stringTable.Count);
            return ret;
        }

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
