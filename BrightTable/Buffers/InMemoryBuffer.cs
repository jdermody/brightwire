using System.Collections.Generic;
using System.IO;
using System.Linq;
using BrightData;
using BrightData.Helper;
using BrightTable.Segments;

namespace BrightTable.Buffers
{
    class InMemoryBuffer<T> : IDataTableSegment<T>, ITypedRowConsumer<T>
    {
        readonly GrowableSegment<T> _segment;

        public InMemoryBuffer(IBrightDataContext context, IColumnInfo column, uint size, IProvideTempStreams tempStream)
        {
            Context = context;
            SingleType = column.ColumnType;
            ColumnIndex = column.Index;
            Types = new[] { SingleType };
            Size = size;
            var buffer = column.MetaData.GetGrowableSegment(column.ColumnType, context, tempStream);

            MetaData = column.MetaData;
            _segment = new GrowableSegment<T>(SingleType, MetaData, buffer as IHybridBuffer<T>);
        }

        public InMemoryBuffer(IBrightDataContext context, IColumnInfo column, uint size, IProvideTempStreams tempStream, IEnumerable<T> data) : this(context, column, size, tempStream)
        {
            foreach (var item in data)
                _segment.Add(item);
        }

        public void Add(object value) => Add((T) value);
        public void Add(T value)
        {
            _segment.Add(value);
        }
        public IBrightDataContext Context { get; }
        public IEnumerable<T> EnumerateTyped() => _segment.EnumerateTyped();
        public IEnumerable<object> Enumerate() => EnumerateTyped().Cast<object>();
        public ColumnType[] Types { get; }
        public uint Size { get; }
        public IEnumerable<object> Data => _segment.Enumerate();
        public IMetaData MetaData { get; }
        public ColumnType SingleType { get; }
        public bool IsEncoded => false;

        public void WriteTo(BinaryWriter writer) => _segment.WriteTo(writer);

        public void Dispose()
        {
            _segment.Dispose();
        }

        public uint ColumnIndex { get; }
        public ColumnType ColumnType => SingleType;
    }
}
