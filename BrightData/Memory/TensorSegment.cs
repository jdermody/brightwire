using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

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

        public TensorSegment(IBrightDataContext context, T[] data) : base(context, data)
        {
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
            get => _data[index];
            set => _data[index] = value;
        }
        public T this[long index] {
            get => _data[index];
            set => _data[index] = value;
        }

        public T[] ToArray() => _data.ToArray();
        public IEnumerable<T> Values => _data;
        public void InitializeFrom(Stream stream) => stream.Read(MemoryMarshal.Cast<T, byte>(_data));

        public void Initialize(Func<uint, T> initializer)
        {
            for (uint i = 0; i < Size; i++)
                _data[(int) i] = initializer(i);
        }
        public void Initialize(T initializer) => Array.Fill(_data, initializer);
        public void Initialize(T[] initialData) => Array.Copy(initialData, _data, _data.Length);
        public void WriteTo(Stream stream) => stream.Write(MemoryMarshal.Cast<T, byte>(_data));
        public void CopyTo(T[] array) => Array.Copy(_data, array, _data.Length);
        public void CopyTo(ITensorSegment<T> segment) => segment.Initialize(i => _data[i]);

        public override string ToString()
        {
            var preview = String.Join("|", Values.Take(8));
            if (Size > 8)
                preview += "|...";
            return $"Segment ({Size}): {preview}";
        }
    }
}
