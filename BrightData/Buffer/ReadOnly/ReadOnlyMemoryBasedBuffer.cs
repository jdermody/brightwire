using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.HighPerformance;

namespace BrightData.Buffer.ReadOnly
{
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
        class ReferenceEnumerator<T> : IReadOnlyEnumerator<T>, IHaveMutableReference<T> where T : unmanaged
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
            public void Seek(uint index) => _index = (int)index - 1;
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
            public IReadOnlyEnumerator<T> GetEnumerator() => new ReferenceEnumerator<T>(Pointer, Length);
        }

        class RandomAccessBlock<T> : ICanRandomlyAccessData<T> where T : unmanaged
        {
            readonly T* _memory;

            public RandomAccessBlock(T* memory)
            {
                _memory = memory;
            }

            public void Dispose()
            {
                // nop
            }

            public T this[int index] => *(_memory + index);
            public T this[uint index] => *(_memory + index);
            public ReadOnlySpan<T> GetSpan(uint startIndex, uint count) => new(_memory + startIndex, (int)count);
        }
        readonly MemoryHandle _memory;

        public ReadOnlyMemoryBasedBuffer(ReadOnlyMemory<byte> memory)
        {
            memory.Pin();
            _memory = memory.Pin();
        }

        public void Dispose()
        {
            _memory.Dispose();
        }

        public ICanIterateData<T> GetIterator<T>(long offset, long sizeInBytes) where T : unmanaged
        {
            var ptr = (byte*)_memory.Pointer;
            return new IterableBlock<T>((T*)(ptr + offset), (int)(sizeInBytes / Unsafe.SizeOf<T>()));
        }

        public ICanRandomlyAccessData<T> GetBlock<T>(long offset, long sizeInBytes) where T : unmanaged
        {
            var ptr = (byte*)_memory.Pointer;
            return new RandomAccessBlock<T>((T*)(ptr + offset));
        }
    }
}
