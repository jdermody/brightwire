using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BrightData;
using BrightData.Helper;
using BrightTable.Input;

namespace BrightTable.Segments
{
    class Column<T> : IDataTableSegment<T>
    {
        readonly InputBufferReader _buffer;

        public Column(IBrightDataContext context, InputBufferReader buffer, ColumnType type, IMetaData metadata)
        {
            _buffer = buffer;
            Context = context;
            SingleType = type;
            Types = new[] { type };
            Size = buffer.Length;
            MetaData = metadata;
            metadata.SetType(type);
        }

        public void Dispose()
        {
            _buffer.Dispose();
        }

        public void WriteTo(BinaryWriter writer)
        {
            foreach (var item in EnumerateTyped())
                DataEncoder.Write(writer, item);
        }

        public IBrightDataContext Context { get; }
        public ColumnType SingleType { get; }
        public IMetaData MetaData { get; }
        public ColumnType[] Types { get; }
        public uint Size { get; }
        public bool IsEncoded => false;

        public IEnumerable<T> EnumerateTyped()
        {
            lock (_buffer) {
                _buffer.Reset();
                var reader = _buffer.Reader;
                for (uint i = 0; i < Size; i++)
                    yield return Context.DataReader.Read<T>(reader);
            }
        }

        public IEnumerable<object> Enumerate()
        {
            foreach (var item in EnumerateTyped())
                yield return item;
        }
    }
}
