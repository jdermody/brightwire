using System;
using System.Collections.Generic;
using System.Text;

namespace BrightData.Memory
{
    class TensorSegment<T> : ITensorSegment<T>
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
                _wasDisposed = true;
                _data.Release();
            }
        }

        public uint Size => _data.Size;
        public int AddRef() => _data.AddRef();
        public int Release() => _data.Release();
        public long AllocationIndex => _data.AllocationIndex;
        public bool IsValid => !_wasDisposed && _data.IsValid;
        public IBrightDataContext Context => _data.Context;

        public ITensorBlock<T> GetBlock(ITensorPool pool)
        {
            _data.AddRef();
            return _data;
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
        public void CopyFrom(T[] array) => _data.CopyFrom(array);
        public void Initialize(Func<uint, T> initializer) => _data.Initialize(initializer);
    }
}
