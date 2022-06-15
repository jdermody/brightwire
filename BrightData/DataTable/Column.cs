using System.Collections.Generic;
using System.IO;

namespace BrightData.DataTable
{
    /// <summary>
    /// A data table column
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class Column<T> : IDataTableSegment<T>, IColumnInfo
        where T: notnull
    {
        readonly MetaData _metadata;
        readonly ICanEnumerateWithSize<T> _reader;
        readonly ICanReadSection _stream;

        public Column(uint index, BrightDataType columnType, MetaData metaData, IBrightDataContext context, ICloneStreams streamCloner, uint inMemorySize)
        {
            _metadata = metaData;

            Index = index;
            SingleType = ColumnType = columnType;
            Types = new[] { SingleType };
            Context = context;

            _stream = streamCloner.Clone();
            _reader = context.GetBufferReader<T>(_stream.GetReader(), inMemorySize);
        }

        public uint Index { get; }
        public BrightDataType ColumnType { get; }
        public IMetaData MetaData => _metadata;

        public void Dispose()
        {
            _stream.Dispose();
        }

        public void WriteTo(BinaryWriter writer)
        {
            ((ICanWriteToBinaryWriter)_reader).WriteTo(writer);
        }

        public IBrightDataContext Context { get; }
        public BrightDataType SingleType { get; }
        public BrightDataType[] Types { get; }
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

        public IHaveDictionary? Dictionary => _reader as IHaveDictionary;
    }
}
