using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BrightData.Buffer.ReadOnly
{
    /// <summary>
    /// Read only buffer from memory block
    /// </summary>
    public unsafe class ReadOnlyMemoryBasedBuffer : IReadOnlyBuffer
    {
        class Enumerator<T> : IEnumerator<T>, IEnumerable where T : unmanaged
        {
            int _index = -1;
            readonly T* _ptr;
            readonly int _length;

            public Enumerator(T* ptr, int length)
            {
                _ptr = ptr;
                _length = length;
            }

            public bool MoveNext() => ++_index < _length;
            public void Reset() => _index = -1;
            public T Current => *(_ptr + _index);
            object IEnumerator.Current => Current;

            public void Dispose()
            {
                // nop
            }

            public IEnumerator GetEnumerator() => this;
        }
        class ReferenceEnumerator<T> : IReadOnlyUnmanagedEnumerator<T>, IHaveMutableReference<T> where T : unmanaged
        {
            int _index = -1;
            readonly T* _ptr;
            readonly int _length;

            public ReferenceEnumerator(T* ptr, int length)
            {
                _ptr = ptr;
                _length = length;
            }

            public bool MoveNext() => ++_index < _length;
            public void Reset() => _index = -1;
            public ref readonly T Current => ref *(_ptr + _index);
            ref T IHaveMutableReference<T>.Current => ref *(_ptr + _index);

            public void Dispose()
            {
                // nop
            }
        }
        class EnumerableBlock<T> : IEnumerable<T> where T : unmanaged
        {
            readonly IterableBlock<T> _block;

            public EnumerableBlock(IterableBlock<T> block)
            {
                _block = block;
            }

            public IEnumerator<T> GetEnumerator() => new Enumerator<T>(_block.Pointer, _block.Length);
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
        class IterableBlock<T> : ICanIterateData<T> where T : unmanaged
        {
            public IterableBlock(T* ptr, int length)
            {
                Pointer = ptr;
                Length = length;
            }

            public T* Pointer { get; }
            public int Length { get; }

            public void Dispose()
            {
                // nop
            }

            public IEnumerable<T> Enumerate() => new EnumerableBlock<T>(this);
            public IReadOnlyUnmanagedEnumerator<T> GetEnumerator() => new ReferenceEnumerator<T>(Pointer, Length);
        }
        class RandomAccessBlock<T> : ICanRandomlyAccessUnmanagedData<T> where T : unmanaged
        {
            readonly T* _memory;

            public RandomAccessBlock(T* memory, uint sizeInBytes)
            {
                _memory = memory;
                Size = sizeInBytes / (uint)Unsafe.SizeOf<T>();
            }

            public void Dispose()
            {
                // nop
            }

            public uint Size { get; }
            public void Get(int index, out T value) => value = *(_memory + index);
            public void Get(uint index, out T value) => value = *(_memory + index);

            public ReadOnlySpan<T> GetSpan(uint startIndex, uint count) => new(_memory + startIndex, (int)count);
        }

        MemoryHandle _memory;

        /// <summary>
        /// Creates from a memory buffer
        /// </summary>
        /// <param name="memory"></param>
        public ReadOnlyMemoryBasedBuffer(ReadOnlyMemory<byte> memory)
        {
            memory.Pin();
            _memory = memory.Pin();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _memory.Dispose();
        }

        /// <inheritdoc />
        public ICanIterateData<T> GetIterator<T>(long offset, long sizeInBytes) where T : unmanaged
        {
            var ptr = (byte*)_memory.Pointer;
            return new IterableBlock<T>((T*)(ptr + offset), (int)(sizeInBytes / Unsafe.SizeOf<T>()));
        }

        /// <inheritdoc />
        public ICanRandomlyAccessUnmanagedData<T> GetBlock<T>(long offset, long sizeInBytes) where T : unmanaged
        {
            var ptr = (byte*)_memory.Pointer;
            return new RandomAccessBlock<T>((T*)(ptr + offset), (uint)sizeInBytes);
        }
    }
}
