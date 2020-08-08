using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BrightData;

namespace BrightTable.Segments
{
    public class GrowableSegment<T> : ISingleTypeTableSegment, IAutoGrowBuffer<T>
    {
        private readonly IAutoGrowBuffer<T> _buffer;

        public GrowableSegment(ColumnType type, IMetaData metaData, IAutoGrowBuffer<T> buffer)
        {
            _buffer = buffer;
            MetaData = metaData;
            SingleType = type;
            metaData.Set(Consts.Type, type);
        }

        public void Dispose()
        {
            // nop
        }

        public IMetaData MetaData { get; }
        public ColumnType SingleType { get; }
        public void WriteTo(BinaryWriter writer) => _buffer.WriteTo(writer);
        public IEnumerable<object> Enumerate() => _buffer.Enumerate();
        public uint Size => _buffer.Size;
        public bool IsEncoded => (_buffer as IHaveEncodedData)?.IsEncoded == true;
        public void Add(object obj) => _buffer.Add(obj);
        public void Add(T obj) => _buffer.Add(obj);
        public IEnumerable<T> EnumerateTyped() => _buffer.EnumerateTyped();
        public void Write(IReadOnlyCollection<T> items, BinaryWriter writer) => _buffer.Write(items, writer);
    }
}
