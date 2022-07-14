using System;
using System.Collections.Generic;
using System.IO;

namespace BrightData.DataTable
{
    internal class ColumnSegment<CT, T> : IDataTableSegment<T>, ICanEnumerateWithSize<T>, ICanEnumerateDisposable
        where CT : unmanaged
        where T: notnull
    {
        readonly SequentialColumnReader<CT, T> _reader;

        public ColumnSegment(
            BrightDataContext context,
            BrightDataType dataType,
            uint size,
            SequentialColumnReader<CT, T> reader, 
            MetaData metaData)
        {
            _reader = reader;
            SingleType = dataType;
            MetaData = metaData;
            Context = context;
            Size = size;
        }

        public IEnumerable<T> EnumerateTyped() => _reader.EnumerateTyped();
        public MetaData MetaData { get; }
        public void WriteTo(BinaryWriter writer)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _reader.Dispose();
        }

        public BrightDataType SingleType { get; }
        public IEnumerable<object> Enumerate()
        {
            foreach (var item in EnumerateTyped())
                yield return item;
        }

        public uint Size { get; }
        public BrightDataContext Context { get; }
    }
}
