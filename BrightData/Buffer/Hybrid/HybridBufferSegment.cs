using System;
using System.Collections.Generic;
using System.IO;

namespace BrightData.Buffer.Hybrid
{
    /// <summary>
    /// A single type data table segment that can grow in size
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class HybridBufferSegment<T> : ISingleTypeTableSegment, IHybridBufferWithMetaData, IHybridBufferWithMetaData<T>
        where T : notnull
    {
        readonly IHybridBuffer<T> _buffer;

        public HybridBufferSegment(BrightDataType type, MetaData metaData, IHybridBuffer<T> buffer)
        {
            _buffer = buffer;
            MetaData = metaData;
            SingleType = type;
            metaData.SetType(type);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            // nop
        }

        /// <inheritdoc />
        public MetaData MetaData { get; }

        /// <inheritdoc />
        public BrightDataType SingleType { get; }

        /// <inheritdoc />
        public void WriteTo(BinaryWriter writer) => _buffer.CopyTo(writer.BaseStream);

        /// <inheritdoc />
        public void CopyTo(Stream stream) => _buffer.CopyTo(stream);
        public IEnumerable<object> Enumerate() => _buffer.Enumerate();
        public uint? NumDistinct => _buffer.NumDistinct;
        public uint Size => _buffer.Size;
        public void AddObject(object? obj) => _buffer.Add((obj != null ? (T)obj : default) ?? throw new ArgumentException("Value cannot be null"));
        public Type DataType { get; } = typeof(T);
        public void Add(T obj) => _buffer.Add(obj);
        public IEnumerable<T> EnumerateTyped() => _buffer.EnumerateTyped();
        public Dictionary<T, uint>? DistinctItems => _buffer.DistinctItems;
    }
}
