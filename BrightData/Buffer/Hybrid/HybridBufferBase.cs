using BrightData.Buffer.EncodedStream;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace BrightData.Buffer.Hybrid
{
    /// <summary>
    /// Hybrid buffers write to disk after their in memory cache is exhausted
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal abstract class HybridBufferBase<T> : IHybridBuffer<T> where T : notnull
    {
        readonly uint _maxCount;
        readonly IProvideTempStreams _tempStream;
        readonly string _id;
        protected readonly List<T> _tempBuffer;
        readonly ushort _maxDistinct = 0;

        protected HybridBufferBase(IProvideTempStreams tempStream, uint maxCount, ushort? maxDistinct)
        {
            _id = Guid.NewGuid().ToString("n");
            _tempStream = tempStream;
            _maxCount = maxCount;
            _tempBuffer = new();

            if (maxDistinct > 0) {
                DistinctItems = new Dictionary<T, uint>();
                _maxDistinct = maxDistinct.Value;
            }
        }

        public void Add(T item)
        {
            if (_tempBuffer.Count == _maxCount) {
                var stream = _tempStream.Get(_id);
                stream.Seek(0, SeekOrigin.End);
                WriteTo(GetTempBuffer(), stream);
                _tempBuffer.Clear();
            }

            _tempBuffer.Add(item);
            if (DistinctItems?.TryAdd(item, Size) == true && DistinctItems.Count > _maxDistinct)
                DistinctItems = null;
            ++Size;
        }

        protected ReadOnlySpan<T> GetTempBuffer() => CollectionsMarshal.AsSpan(_tempBuffer);//((Span<T>)_tempBuffer)[.._index];

        public IEnumerable<T> Values
        {
            get
            {
                // read from the stream
                if (_tempStream.HasStream(_id)) {
                    var stream = _tempStream.Get(_id);
                    stream.Seek(0, SeekOrigin.Begin);
                    var buffer = new T[_maxCount];
                    while (stream.Position < stream.Length) {
                        var count = ReadTo(stream, _maxCount, buffer);
                        for (uint i = 0; i < count; i++)
                            yield return buffer[i];
                    }
                }

                // then from the buffer
                foreach (var item in _tempBuffer)
                    yield return item;
            }
        }

        public void CopyTo(Stream stream) => EncodedStreamWriter.CopyTo(this, stream);

        IEnumerable<object> ICanEnumerate.Values => Values.Select(o => (object)o);
        public uint Size { get; private set; } = 0;
        public uint? NumDistinct => (uint?)DistinctItems?.Count;
        public void AddObject(object obj) => Add((T)obj);
        public Type DataType { get; } = typeof(T);
        protected abstract void WriteTo(ReadOnlySpan<T> ptr, Stream stream);
        protected abstract uint ReadTo(Stream stream, uint count, T[] buffer);
        public Dictionary<T, uint>? DistinctItems { get; private set; } = null;
    }
}
