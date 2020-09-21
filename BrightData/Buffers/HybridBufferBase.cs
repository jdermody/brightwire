using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using BrightData.Helper;

namespace BrightData.Buffers
{
    public abstract class HybridBufferBase<T> : IAutoGrowBuffer<T>
    {
        private readonly TempStreamManager _tempStreams;
        private readonly uint _maxBufferSize;
        readonly ConcurrentBag<T> _buffer = new ConcurrentBag<T>();
        private bool _hasWrittenToStream = false;
        protected readonly IBrightDataContext _context;
        private readonly string _id = Guid.NewGuid().ToString("n");
        private uint _size;

        protected HybridBufferBase(IBrightDataContext context, TempStreamManager tempStreams, uint maxBufferSize = 32768)
        {
            _context = context;
            _tempStreams = tempStreams;
            _maxBufferSize = maxBufferSize;
        }

        public void WriteTo(BinaryWriter writer)
        {
            if (_hasWrittenToStream) {
                var stream = _tempStreams.Get(_id);
                lock (stream) {
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.CopyTo(writer.BaseStream);
                }
            }

            Write(_buffer, writer);
        }

        public abstract void Write(IReadOnlyCollection<T> items, BinaryWriter writer);
        protected abstract IEnumerable<T> _Read(Stream stream);

        public IEnumerable<T> EnumerateTyped()
        {
            if (_hasWrittenToStream) {
                var stream = _tempStreams.Get(_id);
                lock (stream) {
                    stream.Seek(0, SeekOrigin.Begin);
                    foreach (var item in _Read(stream))
                        yield return item;
                }
            }

            foreach (var item in _buffer)
                yield return item;
        }
        public IEnumerable<object> Enumerate()
        {
            foreach (var item in EnumerateTyped())
                yield return item;
        }

        public uint Size => _size + (uint)_buffer.Count;
        void IAutoGrowBuffer.Add(object obj) => Add((T)obj);
        public void Add(T typedObject)
        {
            if (_buffer.Count == _maxBufferSize) {
                var stream = _tempStreams.Get(_id);
                lock (stream) {
                    if (_buffer.Count == _maxBufferSize) {
                        using var writer = new BinaryWriter(stream, Encoding.UTF8, true);
                        Write(_buffer, writer);
                        _size += (uint)_buffer.Count;
                        _buffer.Clear();
                    }
                }
            }

            _buffer.Add(typedObject);
        }
    }
}
