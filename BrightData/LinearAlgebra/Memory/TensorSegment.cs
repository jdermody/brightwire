using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra.Memory
{
    /// <summary>
    /// "Pointer" to a tensor block that manages reference counting
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class TensorSegment<T> : ReferenceCountedBlock<T>, ITensorSegment<T> 
        where T: struct
    {
        bool _wasDisposed = false;
        readonly T[] _array;

        public TensorSegment(IBrightDataContext context, MemoryOwner<T> data) : base(context, data)
        {
            _array = data.DangerousGetArray().Array!;
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

        public bool IsContiguous => true;

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

        public void Initialize(Span<T> initialData)
        {
            initialData.CopyTo(_data.Span);
        }
        public void WriteTo(Stream stream) => stream.Write(MemoryMarshal.Cast<T, byte>(_data.Span));

        public void CopyTo(ITensorSegment<T> segment) => segment.Initialize(_data.Span);
        public void CopyTo(Span<T> span, uint sourceIndex = 0U, uint count = 4294967295U)
        {
            var index = 0;
            var size = Math.Min(Size, count);
            for (var i = sourceIndex; i < size; i++)
                span[index++] = _array[i];
        }

        public System.Numerics.Vector<T> AsNumericsVector(int start) => new(_data.Span.Slice(start));
        public T[] GetArrayForLocalUseOnly() => _array;

        public override string ToString()
        {
            var preview = String.Join("|", Values.Take(8));
            if (Size > 8)
                preview += "|...";
            return $"Segment ({Size}): {preview}";
        }
    }
}
