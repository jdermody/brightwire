using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance;
using Microsoft.Win32.SafeHandles;

namespace BrightData.Helper.StringTables
{
    public class FileBasedStringTable : IDisposable, IHaveSize
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

        public static async Task<FileBasedStringTable> Create(string filePath)
        {
            var sizeBuffer = new byte[12];
            var file = File.OpenHandle(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, FileOptions.Asynchronous | FileOptions.RandomAccess);
            await RandomAccess.ReadAsync(file, sizeBuffer, 0);
            var size = BinaryPrimitives.ReadUInt32LittleEndian(sizeBuffer[..4]);
            var stringTableOffset = BinaryPrimitives.ReadUInt32LittleEndian(sizeBuffer[4..8]);
            var stringDataOffset = BinaryPrimitives.ReadUInt32LittleEndian(sizeBuffer[8..12]);

            var stringTable = new InMemoryStringTableBuilder.OffsetAndLength[size];
            await RandomAccess.ReadAsync(file, stringTable.AsMemory().AsBytes(), stringTableOffset);
            return new(file, stringTable, stringDataOffset);
        }

        public void Dispose()
        {
            _file.Dispose();
        }

        [SkipLocalsInit]
        public string Get(uint stringIndex)
        {
            var (offset, size) = _stringTable[stringIndex];
            Span<byte> buffer = stackalloc byte[(int)size];
            RandomAccess.Read(_file, buffer, _stringDataOffset + offset);
            return Encoding.UTF8.GetString(buffer);
        }

        public uint Size { get; }
    }
}
