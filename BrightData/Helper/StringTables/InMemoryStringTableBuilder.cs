using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.Helper.StringTables
{
    public class InMemoryStringTableBuilder : IDisposable, IStringTableInMemory
    {
        public readonly record struct OffsetAndLength(uint Offset, uint Length);
        readonly ArrayPoolBufferWriter<byte> _writer = new();
        readonly List<OffsetAndLength> _stringTable = new();

        [SkipLocalsInit]
        public InMemoryStringTableBuilder(IIndexStrings strings, int stackBufferSize = 4096)
        {
            Span<byte> buffer = stackalloc byte[stackBufferSize];
            foreach (var str in strings.OrderedStrings) {
                var offset = (uint)_writer.WrittenCount;
                if (!Encoding.UTF8.TryGetBytes(str, buffer, out var size))
                    throw new Exception($"String was too large to encode in {stackBufferSize:N0} bytes: \"{str[..32]}...\" ({str.Length} characters)");
                _writer.Write(buffer[..size]);
                _stringTable.Add(new(offset, (uint)size));
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _writer.Dispose();
        }

        public ReadOnlySpan<byte> BufferSpan => _writer.WrittenSpan;
        public ReadOnlyMemory<byte> BufferMemory => _writer.WrittenMemory;
        public ReadOnlySpan<OffsetAndLength> StringSpan => CollectionsMarshal.AsSpan(_stringTable);
        public OffsetAndLength[] AllStrings => _stringTable.ToArray();

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

        public async Task WriteTo(string outputPath)
        {
            const uint HeaderSize = 12U;
            var sizeHeader = new byte[HeaderSize];
            var strings = AllStrings.AsMemory().Cast<OffsetAndLength, byte>();
            var header = WriteHeader(sizeHeader, [
                Size,
                HeaderSize,
                HeaderSize + (uint)strings.Length
            ]);

            // write to file
            using var file = File.OpenHandle(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, FileOptions.Asynchronous | FileOptions.SequentialScan);
            await RandomAccess.WriteAsync(file, sizeHeader, 0);
            await RandomAccess.WriteAsync(file, strings, header[1]);
            await RandomAccess.WriteAsync(file, BufferMemory, header[2]);

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
