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

            //writer.Write(Size);
            _buffer.Reset();
            var reader = _buffer.Reader;
            for(uint i = 0; i < Size; i++) {
                writer.Write(reader.ReadUInt32());
            }
        }

        public ColumnType SingleType { get; }
        public IMetaData MetaData { get; }
        public ColumnType[] Types { get; }
        public uint Size { get; }
        public bool IsEncoded => true;

        public IEnumerable<T> EnumerateTyped()
        {
            _buffer.Reset();
            var reader = _buffer.Reader;
            for(uint i = 0; i < Size; i++) {
                var index = reader.ReadUInt32();
                yield return _data[index];
            }
        }

        public IEnumerable<object> Enumerate()
        {
            foreach(var item in EnumerateTyped())
                yield return item;
        }
    }
}
