using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance;

namespace BrightData.Helper.StringTables
{
    internal class TrieStringIndexer(UniqueIndexedStringTrie<char> trie, IStringTableInMemory stringTable) : IIndexStrings, IHaveDataAsReadOnlyByteSpan
    {
        public static async Task<TrieStringIndexer> Create(IStringTableInMemory stringTable, string filePath)
        {
            var data = await File.ReadAllBytesAsync(filePath);
            var trie = new UniqueIndexedStringTrie<char>(data.AsMemory().Cast<byte, UniqueIndexedStringTrie<char>.NodeData>());
            return new(trie, stringTable);
        }

        /// <inheritdoc />
        [SkipLocalsInit]
        public uint GetIndex(ReadOnlySpan<char> str)
        {
            Span<char> temp = stackalloc char[str.Length];
            var size = str.Trim().ToLowerInvariant(temp);

            if (size >= 0 && trie.TryGetIndex(temp[..size], out var ret))
                return ret;

            var str2 = str.ToString().Trim().ToLowerInvariant();
            if (trie.TryGetIndex(str2, out ret))
                return ret;

            return uint.MaxValue;
        }

        /// <inheritdoc />
        public IEnumerable<string> OrderedStrings
        {
            get
            {
                for (var i = 0U; i < stringTable.Size; i++)
                    yield return stringTable.GetString(i);
            }
        }

        /// <inheritdoc />
        public ReadOnlySpan<byte> DataAsBytes => trie.DataAsBytes;

        /// <inheritdoc />
        public uint Size { get; } = stringTable.Size;
    }
}
