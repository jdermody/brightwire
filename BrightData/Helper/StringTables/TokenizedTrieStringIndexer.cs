using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BrightData.Helper.StringTables
{
    internal class TokenizedTrieStringIndexer(
        UniqueIndexedStringTrie<int> trie, 
        IStringTableInMemory stringTable, 
        Func<ReadOnlySpan<char>, ReadOnlySpan<int>> tokenizer
    ) : IIndexStrings, IHaveDataAsReadOnlyByteSpan
    {
        [SkipLocalsInit]
        public uint GetIndex(ReadOnlySpan<char> str)
        {
            if (trie.TryGetIndex(tokenizer(str), out var ret))
                return ret;

            return uint.MaxValue;
        }

        public IEnumerable<string> OrderedStrings
        {
            get
            {
                for (uint i = 0, len = Size; i < len; i++)
                    yield return stringTable.GetString(i);
            }
        }

        public ReadOnlySpan<byte> DataAsBytes => trie.DataAsBytes;
        public uint Size => stringTable.Size;
    }
}
