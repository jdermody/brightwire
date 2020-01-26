using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BrightData;
using BrightData.Helper;
using BrightTable.Input;

namespace BrightTable.Segments
{
    class EncodedColumn<T> : IDataTableSegment<T>
    {
        readonly T[] _data;
        readonly InputBufferReader _buffer;
        readonly IBrightDataContext _context;

        public EncodedColumn(IBrightDataContext context, InputBufferReader buffer, ColumnType type, IMetaData metadata)
        {
            _context = context;
            _buffer = buffer;
            SingleType = type;
            Types = new[] { type };
            Size = buffer.Length;
            MetaData = metadata;

            _data = context.DataReader.ReadArray<T>(buffer.Reader);
            buffer.ResetStartPosition();
        }

        public void Dispose()
        {
            _buffer.Dispose();
        }

        public void WriteTo(BinaryWriter writer)
        {
            DataEncoder.Write(writer, _data);

            lock (_buffer) {
                _buffer.Reset();
                var reader = _buffer.Reader;
                var size = reader.ReadByte();
                writer.Write(size);
                for (uint i = 0; i < Size; i++) {
                    if (size == 8)
                        writer.Write(reader.ReadByte());
                    else if (size == 16)
                        writer.Write(reader.ReadUInt16());
                    else if (size == 32)
                        writer.Write(reader.ReadUInt32());
                }
            }
        }

        public ColumnType SingleType { get; }
        public IMetaData MetaData { get; }
        public ColumnType[] Types { get; }
        public uint Size { get; }
        public bool IsEncoded => true;

        public IEnumerable<T> EnumerateTyped()
        {
            lock (_buffer) {
                _buffer.Reset();
                var reader = _buffer.Reader;
                var size = reader.ReadByte();
                for (uint i = 0; i < Size; i++)
                    yield return _Read(size, reader);
            }
        }

        T _Read(byte size, BinaryReader reader)
        {
            if (size == 8) {
                var index = reader.ReadByte();
                return _data[index];
            }
            if (size == 16) {
                var index = reader.ReadUInt16();
                return _data[index];
            }
            if (size == 32) {
                var index = reader.ReadUInt32();
                return _data[index];
            }
            throw new NotImplementedException();
        }

        public IEnumerable<object> Enumerate()
        {
            foreach(var item in EnumerateTyped())
                yield return item;
        }
    }
}
