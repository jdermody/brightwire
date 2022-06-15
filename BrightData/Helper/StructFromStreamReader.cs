using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BrightData.Buffer;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.Helper
{
    class StructFromStreamReader<T> : IDisposable, IStructEnumerator<T>
        where T : struct
    {
        readonly MemoryOwner<T> _buffer;
        readonly Stream         _stream;
        readonly long           _iterableSizeInBytes, _initialStreamPosition;
        readonly int            _sizeOfT;
        int                     _index = -1, _bufferIndex = -1, _bufferSize;

        public StructFromStreamReader(Stream stream, long iterableSizeInBytes, int bufferSize = 1024)
        {
            _stream = stream;
            _initialStreamPosition = stream.Position;
            _iterableSizeInBytes = iterableSizeInBytes;
            _sizeOfT = Unsafe.SizeOf<T>();
            _buffer = MemoryOwner<T>.Allocate((int)Math.Min(iterableSizeInBytes / _sizeOfT, bufferSize));
            ReadIntoBuffer();
        }

        public void Dispose()
        {
            _buffer.Dispose();
        }

        void ReadIntoBuffer()
        {
            var readBytes = _stream.Read(MemoryMarshal.AsBytes(_buffer.Span));
            _bufferSize = readBytes / _sizeOfT;
        }

        public StructFromStreamReader<T> GetEnumerator() => this;

        public bool MoveNext()
        {
            if ((++_index * _sizeOfT) < _iterableSizeInBytes) {
                if (++_bufferIndex == _bufferSize) {
                    ReadIntoBuffer();
                    _bufferIndex = 0;
                }
                return _bufferIndex < _bufferSize;
            }

            return false;
        }
        public void Reset()
        {
            _index = -1;
            _bufferIndex = -1;
            _stream.Seek(_initialStreamPosition, SeekOrigin.Begin);
        }
        public ref T Current => ref _buffer.Span[_bufferIndex];
    }
}
