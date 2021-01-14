using System;
using System.Collections.Generic;
using System.IO;
using BrightData;

namespace BrightTable.Segments
{
    public class GrowableSegment<T> : ISingleTypeTableSegment, IHybridBuffer<T>
    {
        private readonly IHybridBuffer<T> _buffer;

        public GrowableSegment(ColumnType type, IMetaData metaData, IHybridBuffer<T> buffer)
        {
            _buffer = buffer;
            MetaData = metaData;
            SingleType = type;
            metaData.SetType(type);
        }

        public void Dispose()
        {
            // nop
        }

        public IMetaData MetaData { get; }
        public ColumnType SingleType { get; }
        public void WriteTo(BinaryWriter writer) => _buffer.CopyTo(writer.BaseStream);
        public void CopyTo(Stream stream) => _buffer.CopyTo(stream);
        public IEnumerable<object?> Enumerate() => _buffer.Enumerate();
        public uint Length => _buffer.Length;
        public uint? NumDistinct => _buffer.NumDistinct;
        public uint Size => _buffer.Length;
        public bool IsEncoded { get; } = true;
        public void Add(object? obj) => _buffer.Add((obj != null ? (T)obj : default) ?? throw new ArgumentException("Value cannot be null"));
        public void Add(T obj) => _buffer.Add(obj);
        public IEnumerable<T> EnumerateTyped() => _buffer.EnumerateTyped();
    }
}
