using System;
using System.Collections.Generic;
using System.IO;

namespace BrightData.DataTable
{
    internal class ColumnSegment<CT, T> : ITableSegment<T>, ICanEnumerateWithSize<T>, ICanEnumerateDisposable
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
            SegmentType = dataType;
            MetaData = metaData;
            Context = context;
            Size = size;
        }

        public IEnumerable<T> Values => _reader.Values;
        public MetaData MetaData { get; }
        public void WriteTo(BinaryWriter writer)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _reader.Dispose();
        }

        public BrightDataType SegmentType { get; }
        IEnumerable<object> ICanEnumerate.Values
        {
            get
            {
                foreach (var item in Values)
                    yield return item;
            }
        }

        public uint Size { get; }
        public BrightDataContext Context { get; }
    }
}
