using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlegbra2
{
    public class ArrayPoolTensorSegment : ITensorSegment2
    {
        readonly MemoryOwner<float> _data;
        readonly float[] _array;
        int _refCount = 0;

        public ArrayPoolTensorSegment(MemoryOwner<float> data)
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

        public IEnumerable<float> Values
        {
            get
            {
                for (int i = 0, len = (int)Size; i < len; i++)
                    yield return _array[i];
            }
        }
        public float[] GetArrayForLocalUseOnly() => _array;
        public float[] ToNewArray() => _data.Span.ToArray();
        public void CopyFrom(ReadOnlySpan<float> span) => span.CopyTo(_data.Span);
        public void CopyTo(ITensorSegment2 segment)
        {
            var span = GetSpan();
            var destination = segment.GetArrayForLocalUseOnly();
            if(destination is not null)
                span.CopyTo(destination);
            else
                segment.CopyFrom(span);
        }
        public void CopyTo(Span<float> destination) => GetSpan().CopyTo(destination);
        public unsafe void CopyTo(float* destination, int offset, int stride, int count)
        {
            fixed (float* buffer = &_array[offset]) {
                var ptr = buffer;
                for (var i = 0; i < count; i++) {
                    *destination++ = *ptr;
                    ptr += stride;
                }
            }
        }
        public void Clear() => _data.Span.Clear();

        public ReadOnlySpan<float> GetSpan(ref SpanOwner<float> temp, out bool wasTempUsed)
        {
            wasTempUsed = false;
            return _array.AsSpan(0, (int)Size);
        }

        public ReadOnlySpan<float> GetSpan() => _array.AsSpan(0, (int)Size);

        public override string ToString()
        {
            var preview = String.Join("|", Values.Take(8));
            if (Size > 8)
                preview += "|...";
            return $"{SegmentType} ({Size}): {preview}";
        }
    }
}
