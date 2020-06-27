using System;
using System.Collections.Generic;
using System.IO;

namespace BrightData.Memory
{
    /// <summary>
    /// "Pointer" to a tensor block that manages reference counting
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class TensorSegment<T> : ITensorSegment<T> where T: struct
    {
        readonly TensorBlock<T> _data;
        bool _wasDisposed = false;

        public TensorSegment(TensorBlock<T> data)
        {
            _data = data;
            data.AddRef();
            data.Context.MemoryLayer.Add(this);
        }

        public void Dispose()
        {
            if (!_wasDisposed) {
                lock (this) {
                    if (!_wasDisposed) {
                        _wasDisposed = true;
                        _data.Release();
                    }
                }
            }
        }

        public uint Size => _data.Size;
        public int AddRef() => _data.AddRef();
        public int Release() => _data.Release();
        public long AllocationIndex => _data.AllocationIndex;
        public bool IsValid => !_wasDisposed && _data.IsValid;
        public IBrightDataContext Context => _data.Context;

        public (ITensorBlock<T> Block, bool IsNewCopy) GetBlock(ITensorPool pool)
        {
            _data.AddRef();
            return (_data, false);
        }

        public T this[uint index] {
            get => _data[index];
            set => _data[index] = value;
        }
        public T this[long index] {
            get => _data[index];
            set => _data[index] = value;
        }

        public T[] ToArray() => _data.ToArray();
        public IEnumerable<T> Values => _data.Values;
        public void InitializeFrom(Stream stream) => _data.InitializeFrom(stream);
        public void Initialize(Func<uint, T> initializer) => _data.Initialize(initializer);
        public void Initialize(T initializer) => _data.Initialize(initializer);
        public void Initialize(T[] initialData) => initialData.CopyTo(_data.Data);
        public void WriteTo(Stream stream) => _data.WriteTo(stream);
    }
}
