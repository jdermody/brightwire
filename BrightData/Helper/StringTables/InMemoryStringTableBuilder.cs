using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BrightData.Buffer;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.Helper.StringTables
{
    /// <summary>
    /// Builds utf-8 based string data in memory and writes to file
    /// </summary>
    public class InMemoryStringTableBuilder : IDisposable, IStringTableInMemory
    {
        readonly ArrayPoolBufferWriter<byte> _dataWriter = new();
        readonly ArrayPoolBufferWriter<OffsetAndSize> _stringTable = new();

        /// <summary>
        /// Creates a string table builder from a string indexer
        /// </summary>
        /// <param name="stringIndexer"></param>
        /// <param name="maxStringSizeInBytes"></param>
        /// <exception cref="Exception"></exception>
        [SkipLocalsInit]
        public InMemoryStringTableBuilder(IIndexStrings stringIndexer, int maxStringSizeInBytes = 1024)
        {
            Span<byte> buffer = stackalloc byte[maxStringSizeInBytes];
            foreach (var str in stringIndexer.OrderedStrings) {
                var offset = (uint)_dataWriter.WrittenCount;
                if (!Encoding.UTF8.TryGetBytes(str, buffer, out var size))
                    throw new ArgumentException($"String was too large to encode in {maxStringSizeInBytes:N0} bytes: \"{str[..32]}...\" ({str.Length} characters)");
                _dataWriter.Write(buffer[..size]);
                _stringTable.Write(new OffsetAndSize(offset, (uint)size));
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _dataWriter.Dispose();
            _stringTable.Dispose();
        }

        /// <inheritdoc />
        public uint Size => (uint)_stringTable.WrittenCount;

        /// <inheritdoc />
        public ReadOnlySpan<byte> GetUtf8(uint index) => _stringTable.WrittenSpan[(int)index].GetSpan(_dataWriter.WrittenSpan);

        /// <inheritdoc />
        public string GetString(uint index) => Encoding.UTF8.GetString(GetUtf8(index));

        /// <inheritdoc />
        public string[] GetAll(int maxStringSize)
        {
            var ret = new string[Size];
            for (uint i = 0U, len = Size; i < len; i++)
                ret[i] = Encoding.UTF8.GetString(GetUtf8(i)[..maxStringSize]);
            return ret;
        }

        /// <summary>
        /// Writes the string table to a file
        /// </summary>
        /// <param name="outputPath"></param>
        public async Task WriteTo(string outputPath)
        {
            var stringTable = new StringTable(_stringTable.WrittenMemory, _dataWriter.WrittenMemory);
            await File.WriteAllBytesAsync(outputPath, stringTable.ReadOnlyMemory);
        }
    }
}
