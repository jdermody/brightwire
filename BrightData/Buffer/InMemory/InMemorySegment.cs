using BrightData.Buffer.EncodedStream;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BrightData.Buffer.InMemory
{
    class InMemorySegment<T> : IDataTableSegment<T> where T : notnull
    {
        readonly InMemoryBuffer<T> _buffer = new();
        readonly ushort _maxDistinct;
        readonly HashSet<T> _unique = new();
        bool _isDistinct = true;

        public InMemorySegment(IBrightDataContext context, BrightDataType type, IMetaData metaData, ushort maxDistinct)
        {
            _maxDistinct = maxDistinct;
            MetaData = metaData;
            Context = context;
            SingleType = type;
            metaData.SetType(type);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            // nop
        }

        /// <inheritdoc />
        public IMetaData MetaData { get; }

        /// <inheritdoc />
        public BrightDataType SingleType { get; }

        /// <inheritdoc />
        public void WriteTo(BinaryWriter writer)
        {
            var encoder = EncodedStreamWriter.GetWriter(_buffer, _isDistinct);
            encoder.WriteTo(writer);
        }

        public IEnumerable<object> Enumerate() => _buffer.EnumerateTyped().Cast<object>();
        public uint Size => _buffer.Size;
        public void Add(object? obj) => Add((obj != null ? (T)obj : default) ?? throw new ArgumentException("Value cannot be null"));
        public IEnumerable<T> EnumerateTyped() => _buffer.EnumerateTyped();
        public IBrightDataContext Context { get; }

        public void Add(T obj)
        {
            _buffer.Add(obj);
            if (_isDistinct && _unique.Add(obj) && _unique.Count > _maxDistinct)
                _isDistinct = false;
        }
    }
}
