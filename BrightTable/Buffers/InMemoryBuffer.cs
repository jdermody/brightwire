using System.Collections.Generic;
using System.IO;
using System.Linq;
using BrightData;
using BrightData.Helper;

namespace BrightTable.Buffers
{
    class InMemoryBuffer<T> : IDataTableSegment<T>, IEditableBuffer, ICanWriteToBinaryWriter
    {
        readonly T[] _data;

        public InMemoryBuffer(IBrightDataContext context, ColumnType type, IMetaData metaData, uint size)
        {
            // TODO: implement max size and overflow to disk?

            Context = context;
            SingleType = type;
            Types = new[] { type };
            Size = size;
            _data = new T[size];
            MetaData = metaData ?? new MetaData();
        }

        public InMemoryBuffer(IBrightDataContext context, ColumnType type, uint size, IEnumerable<T> data) : this(context, type, null, size)
        {
            uint index = 0;
            foreach (var item in data)
                _data[index++] = item;
        }

        public void Set(uint index, object value)
        {
            _data[index] = (T)value;
        }
        public IBrightDataContext Context { get; }
        public IEnumerable<T> EnumerateTyped() => _data;
        public IEnumerable<object> Enumerate() => _data.Cast<object>();
        public ColumnType[] Types { get; }
        public uint Size { get; }
        public IEnumerable<object> Data => _data.Cast<object>();
        public IMetaData MetaData { get; }
        public ColumnType SingleType { get; }
        public bool IsEncoded => false;

        public void WriteTo(BinaryWriter writer)
        {
            for (uint i = 0; i < Size; i++)
                DataEncoder.Write(writer, _data[i]);
        }

        public void Dispose()
        {
            // nop
        }

        public void Finalise()
        {
            // TODO: try to encode and analyse the column
        }
    }
}
