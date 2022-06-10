using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.Memory
{
    /// <summary>
    /// "Pointer" to a tensor block that manages reference counting
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class TensorSegment<T> : ReferenceCountedBlock<T>, ITensorSegment<T> 
        where T: struct
    {
        bool _wasDisposed = false;
        readonly ArraySegment<T> _array;

        public TensorSegment(IBrightDataContext context, MemoryOwner<T> data) : base(context, data)
        {
            _array = data.DangerousGetArray();
        }

        public void Dispose()
        {
            if (!_wasDisposed) {
                lock (this) {
                    if (!_wasDisposed) {
                        _wasDisposed = true;
                        Release();
                    }
                }
            }
        }

        public bool IsContiguous { get; } = true;

        public T this[uint index] {
            get => _array[(int)index];
            set => _array[(int)index] = value;
        }
        public T this[long index] {
            get => _array[(int)index];
            set => _array[(int)index] = value;
        }

        public MemoryOwner<T> Clone()
        {
            var ret = MemoryOwner<T>.Allocate((int)Size);
            _data.Span.CopyTo(ret.Span);
            return ret;
        }

        public IEnumerable<T> Values
        {
            get
            {
                for (var i = 0; i < Size; i++)
                    yield return _array[i];
            }
        }
        public void InitializeFrom(Stream stream) => stream.Read(MemoryMarshal.Cast<T, byte>(_data.Span));

        public void Initialize(Func<uint, T> initializer)
        {
            for (uint i = 0; i < Size; i++)
                _array[(int)i] = initializer(i);
        }

        public void Initialize(T initializer)
        {
            for (var i = 0; i < Size; i++)
                _array[i] = initializer;
        }

        public void Initialize(T[] initialData)
        {
            for (var i = 0; i < initialData.Length; i++)
                _array[i] = initialData[i];
        }
        public void Initialize(MemoryOwner<T> initialData)
        {
            initialData.Span.CopyTo(_data.Span);
        }
        public void WriteTo(Stream stream) => stream.Write(MemoryMarshal.Cast<T, byte>(_data.Span));

        public void CopyTo(MemoryOwner<T> memoryOwner, uint sourceIndex, uint destinationIndex, uint count)
        {
            CopyTo(memoryOwner.DangerousGetArray(), sourceIndex, destinationIndex, count);
        }
        public void CopyTo(ITensorSegment<T> segment) => segment.Initialize(_data);
        public void CopyTo(ArraySegment<T> array, uint sourceIndex = 0, uint destinationIndex = 0, uint count = UInt32.MaxValue)
        {
            var size = Math.Min(Size, count);

            for (uint i = sourceIndex; i < size; i++)
                array[(int)(destinationIndex + i)] = this[i];
        }

        public System.Numerics.Vector<T> AsNumericsVector(int start) => new(_data.Span.Slice(start));
        public T[] GetArrayForLocalUseOnly() => _array.Array!;

        public override string ToString()
        {
            var preview = String.Join("|", Values.Take(8));
            if (Size > 8)
                preview += "|...";
            return $"Segment ({Size}): {preview}";
        }
    }
}
