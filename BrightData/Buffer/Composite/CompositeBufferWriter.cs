using System;
using System.Buffers;

namespace BrightData.Buffer.Composite
{
    /// <summary>
    /// Adapts composite buffers to buffer writers 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="buffer"></param>
    /// <param name="defaultBufferSize"></param>
    internal class CompositeBufferWriter<T>(IAppendBlocks<T> buffer, int defaultBufferSize = 256) : IBufferWriter<T>
        where T : notnull
    {
        int _pos = 0;
        T[] _tempBuffer = new T[defaultBufferSize];

        public void Advance(int count)
        {
            buffer.Append(_tempBuffer.AsSpan()[..count]);
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
