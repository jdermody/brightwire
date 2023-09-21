using System;
using System.Buffers;

namespace BrightData.Buffer.Composite
{
    internal class CompositeBufferWriter<T> : IBufferWriter<T> where T : notnull
    {
        readonly int _bufferSize;
        readonly ICompositeBuffer<T> _buffer;
        int _pos = 0;
        T[] _tempBuffer;

        public CompositeBufferWriter(ICompositeBuffer<T> buffer, int defaultBufferSize = 256)
        {
            _bufferSize = defaultBufferSize;
            _buffer = buffer;
            _tempBuffer = new T[defaultBufferSize];
        }

        public void Advance(int count)
        {
            _buffer.Add(_tempBuffer.AsSpan()[..count]);
            _pos += count;
        }

        public Memory<T> GetMemory(int sizeHint = 0)
        {
            var size = EnsureBuffer(sizeHint);
            return new Memory<T>(_tempBuffer, _pos, size);
        }

        public Span<T> GetSpan(int sizeHint = 0)
        {
            var size = EnsureBuffer(sizeHint);
            return _tempBuffer.AsSpan().Slice(_pos, size);
        }

        public int Capacity => _tempBuffer.Length - _pos;

        int EnsureBuffer(int size)
        {
            size = Math.Max(1, size);
            if (Capacity < size)
                Array.Resize(ref _tempBuffer, size);
            return size;
        }
    }
}
