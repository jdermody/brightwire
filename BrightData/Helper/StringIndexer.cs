using System;
using System.Collections.Generic;
using System.Text;

namespace BrightData.Helper
{
    public class StringIndexer : IIndexStrings
    {
        readonly Dictionary<string, uint> _index = new Dictionary<string, uint>();

        public static StringIndexer Create(params string[] strings)
        {
            var ret = new StringIndexer();
            foreach (var str in strings)
                ret.GetIndex(str);
            return ret;
        }

        public uint GetIndex(string str)
        {
            if (_index.TryGetValue(str, out var ret))
                return ret;
            _index.Add(str, ret = (uint)_index.Count);
            return ret;
        }

        public uint OutputSize => (uint) _index.Count;
    }
}
