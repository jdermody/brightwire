using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BrightData;
using BrightData.Helper;

namespace BrightTable.Buffers
{
    class InMemoryBuffer<T> : IDataTableSegment<T>, ITypedRowConsumer<T>, ICanWriteToBinaryWriter
    {
        readonly T[] _data;

        public InMemoryBuffer(IBrightDataContext context, ColumnType type, uint columnIndex, IMetaData metaData, uint size)
        {
            // TODO: implement max size and overflow to disk - possible using HybridBuffers?

            Context = context;
            SingleType = type;
            ColumnIndex = columnIndex;
            Types = new[] { type };
            Size = size;
            _data = new T[size];
            MetaData = metaData ?? new MetaData();
        }

        public InMemoryBuffer(IBrightDataContext context, ColumnType type, uint columnIndex, uint size, IEnumerable<T> data) : this(context, type, columnIndex, null, size)
        {
            uint index = 0;
            foreach (var item in data)
                _data[index++] = item;
        }

        public void Set(uint index, object value) => Set(index, (T) value);
        public void Set(uint index, T value)
        {
            _data[index] = value;
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

        public uint ColumnIndex { get; }
        public Type ColumnType => SingleType.GetColumnType();
    }
}
