using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using BrightData.Buffer;
using BrightData.Types;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.Helper.StringTables
{
    /// <summary>
    /// File based string table that has been created with InMemoryStringTableBuilder
    /// </summary>
    public class StringTable(ReadOnlyMemory<OffsetAndSize> stringTable, ReadOnlyMemory<byte> stringData) : IStringTableInMemory, IHaveMemory<byte>
    {
        readonly ReadOnlyMemory<OffsetAndSize> _stringTable = stringTable;
        readonly ReadOnlyMemory<byte> _stringData = stringData;

        public static StringTable Create(ReadOnlyMemory<byte> data)
        {
            var blocks = data.GetTupleFromBlockHeader<OffsetAndSize, byte>();
            return new StringTable(
                blocks.Item1,
                blocks.Item2
            );
        }

        public static async Task<StringTable> Create(string filePath)
        {
            return Create(await File.ReadAllBytesAsync(filePath));
        }

        /// <inheritdoc />
        public ReadOnlyMemory<byte> ReadOnlyMemory => BlockHeader.Combine(ReadOnlyMultiTypeSpanTuple.Create(
            _stringTable.Span,
            _stringData.Span
        ));

        /// <inheritdoc />
        public string GetString(uint stringIndex) => Encoding.UTF8.GetString(GetUtf8(stringIndex));

        /// <inheritdoc />
        public ReadOnlySpan<byte> GetUtf8(uint stringIndex) => _stringTable.Span[(int)stringIndex].GetSpan(_stringData.Span);

        /// <inheritdoc />
        public string[] GetAll(int maxStringSize = 1024)
        {
            var span = _stringTable.Span;
            var dataSpan = _stringData.Span;

            var ret = new string[Size];
            for (var i = 0U; i < Size; i++)
                ret[i] = Encoding.UTF8.GetString(span[(int)i].GetSpan(dataSpan));
            return ret;
        }

        /// <inheritdoc />
        public uint Size => (uint)_stringTable.Length;

        /// <summary>
        /// Creates a string indexer
        /// </summary>
        /// <param name="type">Type of string indexer to create</param>
        /// <param name="maxStringSize">Max string size to index</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<IIndexStrings> GetStringIndexer(StringIndexType type = StringIndexType.Dictionary, int maxStringSize = 1024)
        {
            var span = _stringTable.Span;
            var dataSpan = _stringData.Span;
            switch (type) {
                case StringIndexType.Dictionary: {
                    return new FrozenDictionaryStringIndexer(span, dataSpan, maxStringSize);
                }
                case StringIndexType.Trie: {
                    using var buffer = SpanOwner<char>.Allocate(maxStringSize);
                    var bufferSpan = buffer.Span;
                    var trieBuilder = new UniqueIndexedStringTrie<char>.Builder();
                    for (var i = 0U; i < Size; i++) {
                        var utf8 = span[(int)i].GetSpan(dataSpan);
                        var bufferSize = Encoding.UTF8.GetChars(utf8, bufferSpan);
                        trieBuilder.Add(bufferSpan[..bufferSize], i);
                    }
                    return new TrieStringIndexer(trieBuilder.Build(), this);
                }
                default:
                    throw new NotImplementedException(type.ToString());
            }
        }

        /// <summary>
        /// Creates a string indexer from a tokenizer (characters are mapped to a series of integers, for example from Byte Pair Encoding)
        /// </summary>
        /// <param name="tokenizer"></param>
        /// <param name="maxStringSize"></param>
        /// <returns></returns>
        public async Task<IIndexStrings> GetStringIndexer(Func<ReadOnlySpan<char>, ReadOnlySpan<int>> tokenizer, int maxStringSize = 1024)
        {
            // build the tokenized trie
            using var buffer = SpanOwner<char>.Allocate(maxStringSize);
            var bufferSpan = buffer.Span;
            var span = _stringTable.Span;
            var dataSpan = _stringData.Span;
            var trieBuilder = new UniqueIndexedStringTrie<int>.Builder();
            for (var i = 0U; i < Size; i++) {
                var utf8 = span[(int)i].GetSpan(dataSpan);
                var bufferSize = Encoding.UTF8.GetChars(utf8, bufferSpan);
                trieBuilder.Add(tokenizer(bufferSpan[..bufferSize]), i);
            }

            return new TokenizedTrieStringIndexer(trieBuilder.Build(), this, tokenizer);
        }
    }
}
