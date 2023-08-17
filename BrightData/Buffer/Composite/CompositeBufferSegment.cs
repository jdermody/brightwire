using System;
using System.Collections.Generic;
using System.IO;

namespace BrightData.Buffer.Composite
{
    /// <summary>
    /// A single type data table segment that can grow in size
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class CompositeBufferSegment<T> : ITableSegment<T>, ICompositeBufferWithMetaData, ICompositeBufferWithMetaData<T>
        where T : notnull
    {
        readonly ICompositeBuffer<T> _buffer;

        public CompositeBufferSegment(BrightDataContext context, BrightDataType type, MetaData metaData, ICompositeBuffer<T> buffer)
        {
            _buffer = buffer;
            Context = context;
            MetaData = metaData;
            SegmentType = type;
            metaData.SetType(type);
        }

        public void Dispose()
        {
            // nop
        }

        public BrightDataContext Context { get; }
        public MetaData MetaData { get; }
        public BrightDataType SegmentType { get; }
        public void WriteTo(BinaryWriter writer) => _buffer.CopyTo(writer.BaseStream);
        public Predicate<T>? ConstraintValidator { get; set; } = null;
        public void CopyTo(Stream stream) => _buffer.CopyTo(stream);
        IEnumerable<object> ICanEnumerate.Values => ((ICanEnumerate)_buffer).Values;
        public uint? NumDistinct => _buffer.NumDistinct;
        public uint Size => _buffer.Size;
        public void AddObject(object? obj) => Add((obj != null ? (T)obj : default) ?? throw new ArgumentException("Value cannot be null"));
        public Type DataType { get; } = typeof(T);
        public IEnumerable<T> Values => _buffer.Values;
        public Dictionary<T, uint>? DistinctItems => _buffer.DistinctItems;

        public void Add(T obj)
        {
            if (ConstraintValidator?.Invoke(obj) == false)
                throw new InvalidOperationException($"Failed to add item to buffer as it failed validation: {obj}");
            _buffer.Add(obj);
        }
    }
}
