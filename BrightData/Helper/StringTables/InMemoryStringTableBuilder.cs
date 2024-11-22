using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.Helper.StringTables
{
    /// <summary>
    /// Builds utf-8 based string data in memory and writes to file
    /// </summary>
    public class InMemoryStringTableBuilder : IDisposable, IStringTableInMemory
    {
        internal readonly record struct OffsetAndLength(uint Offset, uint Length);
        readonly ArrayPoolBufferWriter<byte> _writer = new();
        readonly List<OffsetAndLength> _stringTable = [];

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
                var offset = (uint)_writer.WrittenCount;
                if (!Encoding.UTF8.TryGetBytes(str, buffer, out var size))
                    throw new Exception($"String was too large to encode in {maxStringSizeInBytes:N0} bytes: \"{str[..32]}...\" ({str.Length} characters)");
                _writer.Write(buffer[..size]);
                _stringTable.Add(new(offset, (uint)size));
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _writer.Dispose();
        }

        /// <inheritdoc />
        public uint Size => (uint)_stringTable.Count;

        /// <inheritdoc />
        public ReadOnlySpan<byte> GetUtf8(uint index)
        {
            var (offset, size) = _stringTable[(int)index];
            return _writer.WrittenSpan.Slice((int)offset, (int)size);
        }

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
            const uint HeaderSize = 12U;
            var sizeHeader = new byte[HeaderSize];
            var strings = _stringTable.ToArray().AsMemory().Cast<OffsetAndLength, byte>();
            var header = WriteHeader(sizeHeader, [
                Size,
                HeaderSize,
                HeaderSize + (uint)strings.Length
            ]);

            // write to file
            using var file = File.OpenHandle(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, FileOptions.Asynchronous | FileOptions.SequentialScan);
            await RandomAccess.WriteAsync(file, sizeHeader, 0);
            await RandomAccess.WriteAsync(file, strings, header[1]);
            await RandomAccess.WriteAsync(file, _writer.WrittenMemory, header[2]);
            return;

            static uint[] WriteHeader(Span<byte> span, uint[] header)
            {
                BinaryPrimitives.WriteUInt32LittleEndian(span[..4], header[0]);
                BinaryPrimitives.WriteUInt32LittleEndian(span[4..8], header[1]);
                BinaryPrimitives.WriteUInt32LittleEndian(span[8..12], header[2]);
                return header;
            }
        }
    }
}
