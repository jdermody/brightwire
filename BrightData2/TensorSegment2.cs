using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData2
{
    internal class TensorSegment2 : IDisposableTensorSegment
    {
        readonly MemoryOwner<float> _data;
        readonly float[] _array;
        int _refCount = 0;

        public TensorSegment2(MemoryOwner<float> data)
        {
            _data = data;
            _array = data.DangerousGetArray().Array!;
        }

        public void Dispose()
        {
            Release();
        }

        public int AddRef() => Interlocked.Increment(ref _refCount);
        public int Release()
        {
            var ret = Interlocked.Decrement(ref _refCount);
            if (IsValid && ret <= 0) {
                _data.Dispose();
                IsValid = false;
            }

            return ret;
        }

        public bool IsValid { get; private set; } = true;
        public uint Size => (uint)_data.Length;
        public string SegmentType => "memory owner";

        public float this[int index]
        {
            get => _array[index];
            set => _array[index] = value;
        }

        public float this[uint index]
        {
            get => _array[index];
            set => _array[index] = value;
        }

        public float this[long index]
        {
            get => _array[index];
            set => _array[index] = value;
        }

        public float this[ulong index]
        {
            get => _array[index];
            set => _array[index] = value;
        }

        public IEnumerable<float> Values => _array;
        public float[] GetArrayForLocalUseOnly() => _array;
        public float[] ToNewArray() => (float[])_array.Clone();
        public void CopyFrom(Span<float> span) => span.CopyTo(_data.Span);
    }
}
