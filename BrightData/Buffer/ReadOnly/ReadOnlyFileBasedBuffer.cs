using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.Buffer.ReadOnly
{
    /// <summary>
    /// A read only buffer from a file or memory mapped file
    /// </summary>
    public class ReadOnlyFileBasedBuffer : IReadOnlyBuffer
    {
        class ReferenceStructFromStreamReader<T> : IReadOnlyUnmanagedEnumerator<T>, IHaveMutableReference<T>
            where T : unmanaged
        {
            readonly MemoryOwner<T>         _buffer;
            readonly MemoryMappedViewStream _stream;
            readonly long                   _iterableCount, _initialStreamPosition;
            readonly int                    _sizeOfT;
            int                             _index = -1, _bufferIndex = -1, _bufferSize;

            public ReferenceStructFromStreamReader(MemoryMappedViewStream stream, long iterableCount, int bufferSize = 1024)
            {
                _stream                = stream;
                _initialStreamPosition = stream.Position;
                _iterableCount         = iterableCount;
                _sizeOfT               = Unsafe.SizeOf<T>();
                _buffer                = MemoryOwner<T>.Allocate((int)Math.Min(_iterableCount, bufferSize));
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

            public bool MoveNext()
            {
                if (++_index < _iterableCount) {
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
            public ref readonly T Current => ref _buffer.Span[_bufferIndex];
            ref T IHaveMutableReference<T>.Current => ref _buffer.Span[_bufferIndex];
        }
        class IterableBlock<T> : ICanIterateData<T> where T : unmanaged
        {
            readonly MemoryMappedViewStream _stream;
            readonly int _sizeOfT;

            public IterableBlock(MemoryMappedViewStream stream)
            {
                _stream = stream;
                _sizeOfT = Unsafe.SizeOf<T>();
            }

            public void Dispose()
            {
                _stream.Dispose();
            }

            public IEnumerable<T> Enumerate() => _stream.Enumerate<T>((uint)(_stream.Length / _sizeOfT));
            public IReadOnlyUnmanagedEnumerator<T> GetEnumerator() => new ReferenceStructFromStreamReader<T>(_stream, _stream.Length / _sizeOfT);
        }
        class RandomAccessBlock<T> : ICanRandomlyAccessUnmanagedData<T> where T : unmanaged
        {
            readonly MemoryMappedViewAccessor _accessor;
            readonly int _sizeOfT;

            public RandomAccessBlock(MemoryMappedViewAccessor accessor, uint sizeInBytes)
            {
                _accessor = accessor;
                _sizeOfT = Unsafe.SizeOf<T>();
                Size = sizeInBytes / (uint)_sizeOfT;
            }

            public void Dispose()
            {
                _accessor.Dispose();
            }
            public uint Size { get; }
            public void Get(int index, out T value) => _accessor.Read(index * _sizeOfT, out value);
            public void Get(uint index, out T value) => _accessor.Read(index * _sizeOfT, out value);

            public ReadOnlySpan<T> GetSpan(uint startIndex, uint count)
            {
                var ret = new T[count];
                _accessor.ReadArray(startIndex * _sizeOfT, ret, 0, (int)count);
                return ret;
            }
        }
        readonly MemoryMappedFile _file;

        /// <summary>
        /// Creates from a file
        /// </summary>
        /// <param name="file">File to access</param>
        public ReadOnlyFileBasedBuffer(FileStream file)
        {
            _file = MemoryMappedFile.CreateFromFile(file, null, 0, MemoryMappedFileAccess.Read, HandleInheritability.None, false);
        }

        /// <summary>
        /// Creates from a memory mapped file
        /// </summary>
        /// <param name="file">Memory mapped file to access</param>
        public ReadOnlyFileBasedBuffer(MemoryMappedFile file)
        {
            _file = file;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _file.Dispose();
        }

        /// <inheritdoc />
        public ICanIterateData<T> GetIterator<T>(long offset, long sizeInBytes) where T : unmanaged
        {
            var viewStream = _file.CreateViewStream(offset, sizeInBytes, MemoryMappedFileAccess.Read);
            return new IterableBlock<T>(viewStream);
        }

        /// <inheritdoc />
        public ICanRandomlyAccessUnmanagedData<T> GetBlock<T>(long offset, long sizeInBytes) where T : unmanaged
        {
            var viewAccessor = _file.CreateViewAccessor(offset, sizeInBytes, MemoryMappedFileAccess.Read);
            return new RandomAccessBlock<T>(viewAccessor, (uint)sizeInBytes);
        }
    }
}
