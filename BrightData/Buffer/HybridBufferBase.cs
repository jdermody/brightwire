using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BrightData.Buffer
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
        protected readonly T[] _tempBuffer;
        readonly ushort _maxDistinct = 0;

        HashSet<T>? _distinctSet = null;
        int _index = 0;

        protected HybridBufferBase(IProvideTempStreams tempStream, uint maxCount, ushort? maxDistinct)
        {
            _id = Guid.NewGuid().ToString("n");
            _tempStream = tempStream;
            _maxCount = maxCount;
            _tempBuffer = new T[maxCount];

            if (maxDistinct > 0) {
                _distinctSet = new HashSet<T>();
                _maxDistinct = maxDistinct.Value;
            }
        }

        public void Add(T item)
        {
            if (_index == _maxCount) {
                var stream = _tempStream.Get(_id);
                stream.Seek(0, SeekOrigin.End);
                WriteTo(GetTempBuffer(), stream);
                _index = 0;
            }

            _tempBuffer[_index++] = item;
            ++Size;

            if (_distinctSet?.Add(item) == true && _distinctSet.Count > _maxDistinct)
                _distinctSet = null;
        }

        protected Span<T> GetTempBuffer() => ((Span<T>) _tempBuffer)[.._index];

        public IEnumerable<T> EnumerateTyped()
        {
            // read from the stream
            if (_tempStream.HasStream(_id)) {
                var stream = _tempStream.Get(_id);
                stream.Seek(0, SeekOrigin.Begin);
                var buffer = new T[_maxCount];
                while (stream.Position < stream.Length) {
                    var count = ReadTo(stream, _maxCount, buffer);
                    for(uint i = 0; i < count; i++)
                        yield return buffer[i];
                }
            }

            // then from the buffer
            for(uint i = 0; i < _index; i++)
                yield return _tempBuffer[i];
        }

        public void CopyTo(Stream stream) => EncodedStreamWriter.CopyTo(this, stream);

        public IEnumerable<object> Enumerate() => EnumerateTyped().Select(o => (object)o);
        public uint Size { get; private set; } = 0;
        public uint? NumDistinct => (uint?) _distinctSet?.Count;
        public void Add(object obj) => Add((T) obj);

        protected abstract void WriteTo(ReadOnlySpan<T> ptr, Stream stream);
        protected abstract uint ReadTo(Stream stream, uint count, T[] buffer);
    }
}
