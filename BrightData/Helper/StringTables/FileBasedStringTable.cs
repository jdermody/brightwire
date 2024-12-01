using System;
using System.Buffers.Binary;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using Microsoft.Win32.SafeHandles;

namespace BrightData.Helper.StringTables
{
    /// <summary>
    /// File based string table that has been created with InMemoryStringTableBuilder
    /// </summary>
    public class FileBasedStringTable : IDisposable, IStringTableInMemory, IAsyncStringTable
    {
        readonly SafeFileHandle _file;
        readonly InMemoryStringTableBuilder.OffsetAndLength[] _stringTable;
        readonly long _stringDataOffset;

        FileBasedStringTable(SafeFileHandle file, InMemoryStringTableBuilder.OffsetAndLength[] stringTable, long stringDataOffset)
        {
            _file = file;
            _stringTable = stringTable;
            _stringDataOffset = stringDataOffset;
        }

        /// <summary>
        /// Creates a string table from the file path
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static async Task<FileBasedStringTable> Create(string filePath)
        {
            var sizeBuffer = new byte[12];
            var file = File.OpenHandle(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, FileOptions.Asynchronous | FileOptions.RandomAccess);
            await RandomAccess.ReadAsync(file, sizeBuffer, 0);
            var sizeBufferSpan = sizeBuffer.AsSpan();
            var size = BinaryPrimitives.ReadUInt32LittleEndian(sizeBufferSpan[..4]);
            var stringTableOffset = BinaryPrimitives.ReadUInt32LittleEndian(sizeBufferSpan[4..8]);
            var stringDataOffset = BinaryPrimitives.ReadUInt32LittleEndian(sizeBufferSpan[8..12]);

            var stringTable = new InMemoryStringTableBuilder.OffsetAndLength[size];
            await RandomAccess.ReadAsync(file, stringTable.AsMemory().AsBytes(), stringTableOffset);
            return new(file, stringTable, stringDataOffset);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _file.Dispose();
        }

        async Task<ReadOnlyMemory<byte>> IAsyncStringTable.GetUtf8(uint index)
        {
            var (offset, size) = _stringTable[index];
            var buffer = new byte[(int)size];
            await RandomAccess.ReadAsync(_file, buffer, _stringDataOffset + offset);
            return buffer;
        }

        async Task<string> IAsyncStringTable.GetString(uint index)
        {
            var (offset, size) = _stringTable[index];
            var buffer = new byte[(int)size];
            await RandomAccess.ReadAsync(_file, buffer, _stringDataOffset + offset);
            return Encoding.UTF8.GetString(buffer);
        }

        async Task<string[]> IAsyncStringTable.GetAll(int maxStringSize)
        {
            using var buffer = MemoryOwner<char>.Allocate(maxStringSize);

            // read entire string block
            var dataSize = RandomAccess.GetLength(_file) - _stringDataOffset;
            using var data = MemoryOwner<byte>.Allocate((int)dataSize);
            await RandomAccess.ReadAsync(_file, data.Memory, _stringDataOffset);

            var ret = new string[Size];
            var span = data.Span;
            for (var i = 0U; i < Size; i++) {
                var (offset, size) = _stringTable[i];
                ret[i] = Encoding.UTF8.GetString(span.Slice((int)offset, (int)size));
            }
            return ret;
        }

        /// <inheritdoc />
        [SkipLocalsInit]
        public string GetString(uint stringIndex)
        {
            var (offset, size) = _stringTable[stringIndex];
            Span<byte> buffer = stackalloc byte[(int)size];
            RandomAccess.Read(_file, buffer, _stringDataOffset + offset);
            return Encoding.UTF8.GetString(buffer);
        }

        /// <inheritdoc />
        public ReadOnlySpan<byte> GetUtf8(uint stringIndex)
        {
            var (offset, size) = _stringTable[stringIndex];
            var buffer = new byte[(int)size];
            RandomAccess.Read(_file, buffer, _stringDataOffset + offset);
            return buffer;
        }

        /// <inheritdoc />
        public string[] GetAll(int maxStringSize = 1024)
        {
            using var buffer = SpanOwner<char>.Allocate(maxStringSize);

            // read entire string block
            var dataSize = RandomAccess.GetLength(_file) - _stringDataOffset;
            using var data = SpanOwner<byte>.Allocate((int)dataSize);
            var span = data.Span;
            RandomAccess.Read(_file, span, _stringDataOffset);

            var ret = new string[Size];
            for (var i = 0U; i < Size; i++) {
                var (offset, size) = _stringTable[i];
                ret[i] = Encoding.UTF8.GetString(span.Slice((int)offset, (int)size));
            }
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
            // read entire string block
            var dataSize = RandomAccess.GetLength(_file) - _stringDataOffset;
            using var data = MemoryOwner<byte>.Allocate((int)dataSize);
            await RandomAccess.ReadAsync(_file, data.Memory, _stringDataOffset);

            var span = data.Span;
            using var buffer = SpanOwner<char>.Allocate(maxStringSize);
            switch (type) {
                case StringIndexType.Dictionary: {
                    var ret = new DictionaryStringIndexer();
                    for (var i = 0U; i < Size; i++) {
                        var (offset, size) = _stringTable[i];
                        var str = Encoding.UTF8.GetString(span.Slice((int)offset, (int)size));
                        if (ret.GetIndex(str) != i)
                            throw new Exception("Indices did not align");
                    }
                    return ret;
                }
                case StringIndexType.Trie: {
                    var bufferSpan = buffer.Span;
                    var trieBuilder = new UniqueIndexedStringTrie<char>.Builder();
                    for (var i = 0U; i < Size; i++) {
                        var (offset, size) = _stringTable[i];
                        var bufferSize = Encoding.UTF8.GetChars(span.Slice((int)offset, (int)size), bufferSpan);
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
            // read entire string block
            var dataSize = RandomAccess.GetLength(_file) - _stringDataOffset;
            using var data = MemoryOwner<byte>.Allocate((int)dataSize);
            await RandomAccess.ReadAsync(_file, data.Memory, _stringDataOffset);

            // build the tokenized trie
            using var buffer = SpanOwner<char>.Allocate(maxStringSize);
            var bufferSpan = buffer.Span;
            var span = data.Span;
            var trieBuilder = new UniqueIndexedStringTrie<int>.Builder();
            for (var i = 0U; i < Size; i++) {
                var (offset, size) = _stringTable[i];
                var bufferSize = Encoding.UTF8.GetChars(span.Slice((int)offset, (int)size), bufferSpan);
                trieBuilder.Add(tokenizer(bufferSpan[..bufferSize]), i);
            }

            return new TokenizedTrieStringIndexer(trieBuilder.Build(), this, tokenizer);
        }
    }
}
