using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using BrightData.Buffer;

namespace BrightData.Helper.StringTables
{
    internal class FrozenDictionaryStringIndexer : IIndexStrings
    {
        readonly FrozenDictionary<string, uint> _data;

        [SkipLocalsInit]
        public FrozenDictionaryStringIndexer(ReadOnlySpan<OffsetAndSize> stringPointers, ReadOnlySpan<byte> utf8Data, int maxStringSize)
        {
            Span<char> stringBuffer = stackalloc char[maxStringSize];
            var buffer = new KeyValuePair<string, uint>[stringPointers.Length];
            var index = 0U;
            foreach (ref readonly var str in stringPointers) {
                var utf8 = str.GetSpan(utf8Data);
                var bufferSize = Encoding.UTF8.GetChars(utf8, stringBuffer);
                buffer[index] = new(new(stringBuffer[..bufferSize]), index);
                ++index;
            }
            _data = buffer.ToFrozenDictionary();
        }

        public uint Size => (uint)_data.Count;

        [SkipLocalsInit]
        public uint GetIndex(ReadOnlySpan<char> str)
        {
            Span<char> temp = stackalloc char[str.Length];
            var size = str.Trim().ToLowerInvariant(temp);
            var lowerStr = temp[..size].AsReadOnly();

            var lookup = _data.GetAlternateLookup<ReadOnlySpan<char>>();
            if (lookup.TryGetValue(lowerStr, out var ret))
                return ret;
            return uint.MaxValue;
        }

        public IEnumerable<string> OrderedStrings => _data.Keys.Order();
    }
}
