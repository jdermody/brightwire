using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightTable.Input
{
    class StringBuffer : IDisposable, IProvideStrings
    {
        const int BUFFER_SIZE = 8096;
        readonly List<IMemoryOwner<char>> _bufferList = new List<IMemoryOwner<char>>();
        readonly List<(long, int)> _stringOffset = new List<(long, int)>();
        long _size = 0;

        public StringBuffer(uint initialEmptyCount)
        {
            for (uint i = 0; i < initialEmptyCount; i++)
                _stringOffset.Add((0, 0));
        }

        public void Dispose()
        {
            foreach(var buffer in _bufferList)
                buffer.Dispose();
            _bufferList.Clear();
        }

        public uint Count => (uint)_stringOffset.Count;

        public void Clear()
        {
            _bufferList.Clear();
            _stringOffset.Clear();
            _size = 0;
        }

        public void Add(ReadOnlySpan<char> str)
        {
            _stringOffset.Add((_size, str.Length));
            foreach (var ch in str)
                _Add(ch);
        }

        void IProvideStrings.Reset()
        {
            // nop
        }

        public IEnumerable<string> All => _stringOffset.Select(so => Get(so.Item1, so.Item2));

        public string Get(uint stringIndex)
        {
            var str = _stringOffset[(int)stringIndex];
            return Get(str.Item1, str.Item2);
        }

        public string Get(long start, int length)
        {
            if (length > 0) {
                var buffer = new char[length];
                for (var i = 0; i < length; i++)
                    buffer[i] = _Get(start + i);
                return new string(buffer);
            }

            return String.Empty;
        }

        void _Add(char ch)
        {
            var bufferIndex = _size / BUFFER_SIZE;
            var bufferOffset = _size % BUFFER_SIZE;
            while (_bufferList.Count <= bufferIndex)
                _bufferList.Add(MemoryPool<char>.Shared.Rent(BUFFER_SIZE));
            var span = _bufferList[(int)bufferIndex].Memory.Span;
            span[(int)bufferOffset] = ch;
            ++_size;
        }

        char _Get(long index)
        {
            var bufferIndex = index / BUFFER_SIZE;
            var bufferOffset = index % BUFFER_SIZE;
            var span = _bufferList[(int)bufferIndex].Memory.Span;
            return span[(int)bufferOffset];
        }
    }
}
