using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightData.Helper
{
    class TensorSegmentWrapper<T> : ITensorSegment<T>
    {
        readonly ITensorSegment<T> _segment;
        readonly uint _offset, _stride, _length;
        bool _wasDisposed = false;

        public TensorSegmentWrapper(ITensorSegment<T> segment, uint offset, uint stride, uint length)
        {
            _segment = segment;
            _segment.AddRef();
            _offset = offset;
            _stride = stride;
            _length = length;
            segment.Context.MemoryLayer.Add(this);
        }

        public void Dispose()
        {
            if (!_wasDisposed) {
                _wasDisposed = true;
                _segment.Release();
            }
        }

        public uint Size => _length;
        public int AddRef() => _segment.AddRef();
        public int Release() => _segment.Release();
        public long AllocationIndex => _segment.AllocationIndex;
        public bool IsValid => !_wasDisposed && _segment.IsValid;
        public T[] ToArray() => Values.ToArray();
        public IBrightDataContext Context => _segment.Context;

        public IEnumerable<T> Values
        {
            get
            {
                for (uint i = 0; i < _length; i++)
                    yield return this[i];
            }
        }

        public ITensorBlock<T> GetBlock(ITensorPool pool)
        {
            var ret = pool.Get<T>(_length);
            using(var segment = ret.GetSegment())
                segment.Initialize(i => this[i]);
            return ret;
        }

        public T this[uint index] {
            get => _segment[_offset + index * _stride];
            set => _segment[_offset + index * _stride] = value;
        }
        public T this[long index] {
            get => _segment[_offset + index * _stride];
            set => _segment[_offset + index * _stride] = value;
        }

        public void CopyFrom(T[] array)
        {
            uint index = 0;
            foreach (var item in array)
                this[index++] = item;
        }

        public void Initialize(Func<uint, T> initializer)
        {
            for (uint i = 0; i < _length; i++)
                this[i] = initializer(i);
        }
    }
}
