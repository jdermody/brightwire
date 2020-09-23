using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BrightData;
using BrightData.Buffers;
using BrightData.Helper;
using BrightTable.Input;

namespace BrightTable.Segments
{
    class Column<T> : IDataTableSegment<T>, IColumnInfo
    {
        readonly MetaData _metadata;
        private readonly ICanEnumerate<T> _reader;

        public Column(uint index, ColumnType columnType, MetaData metaData, IBrightDataContext context, Stream stream, uint inMemorySize)
        {
            _metadata = metaData;

            Index = index;
            SingleType = ColumnType = columnType;
            Types = new[] { SingleType };
            Context = context;

            _reader = BufferedStreamReader.GetReader<T>(stream, context, inMemorySize);
        }

        public uint Index { get; }
        public ColumnType ColumnType { get; }
        public IMetaData MetaData => _metadata;

        public void Dispose()
        {
            //_buffer.Dispose();
        }

        public void WriteTo(BinaryWriter writer)
        {
            ((ICanWriteToBinaryWriter)_reader).WriteTo(writer);
        }

        public IBrightDataContext Context { get; }
        public ColumnType SingleType { get; }
        public ColumnType[] Types { get; }
        public uint Size => _reader.Size;

        public IEnumerable<T> EnumerateTyped() => _reader.EnumerateTyped();

        public IEnumerable<object> Enumerate()
        {
            foreach (var item in EnumerateTyped())
                yield return item;
        }

        public override string ToString()
        {
            return $"[{ColumnType}] {MetaData}";
        }
    }
}
