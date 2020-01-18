using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using BrightData;
using BrightTable.Input;

namespace BrightTable.Segments
{
    public class StructColumn<T> : IDataTableSegment<T>
        where T : struct
    {
        const int CHUNK_SIZE = 32768;

        readonly InputBufferReader _buffer;
        readonly T[] _data;
        readonly bool _isFullyRead;

        public StructColumn(IBrightDataContext context, InputBufferReader buffer, ColumnType type, IMetaData metadata)
        {
            var reader = buffer.Reader;
            SingleType = type;
            MetaData = metadata;
            _buffer = buffer;

            Size = buffer.Length;
            if (Size < CHUNK_SIZE)
            {
                _data = new T[Size];
                var bytes = MemoryMarshal.Cast<T, byte>(_data);
                reader.Read(bytes);
                _isFullyRead = true;
            }
            else
            {
                _data = new T[CHUNK_SIZE];
                buffer.ResetStartPosition();
                _isFullyRead = false;
            }
        }

        public void Dispose()
        {
            // nop
        }

        public static void WriteData(T[] data, BinaryWriter writer)
        {
            var bytes = MemoryMarshal.Cast<T, byte>(data);
            writer.Write(bytes);
        }

        public void WriteTo(BinaryWriter writer)
        {
            if(_isFullyRead)
                WriteData(_data, writer);
            else
            {
                lock (_buffer)
                {
                    _buffer.Reset();
                    var bytes = MemoryMarshal.Cast<T, byte>(_data);

                    uint pos = 0;
                    var reader = _buffer.Reader;
                    var size = Marshal.SizeOf<T>();
                    var maxSize = size * CHUNK_SIZE;
                    while (pos < Size)
                    {
                        var readCount = reader.Read(bytes);
                        if(readCount == maxSize)
                            writer.Write(bytes);
                        else
                            writer.Write(bytes.Slice(0, readCount));
                        pos += (uint)(readCount / size);
                    }
                }
            }
        }

        public ColumnType SingleType { get; }
        public uint Size { get; }
        public bool IsEncoded => false;
        public IMetaData MetaData { get; }

        public IEnumerable<object> Enumerate()
        {
            foreach (var item in EnumerateTyped())
                yield return item;
        }

        public IEnumerable<T> EnumerateTyped()
        {
            if (_isFullyRead)
            {
                foreach(var item in _data)
                    yield return item;
            }
            else
            {
                lock (_buffer)
                {
                    _buffer.Reset();

                    uint pos = 0;
                    var reader = _buffer.Reader;
                    while (pos < Size)
                    {
                        reader.Read(MemoryMarshal.Cast<T, byte>(_data));
                        for (var i = 0; i < CHUNK_SIZE && pos < Size; i++)
                        {
                            yield return _data[i];
                            ++pos;
                        }
                    }
                }
            }
        }
    }
}
