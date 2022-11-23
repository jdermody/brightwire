using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra
{
    public class ArrayPoolTensorSegment : ITensorSegment
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
        public bool IsWrapper => false;

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
        public float[] GetArrayIfEasilyAvailable() => _array;
        public float[] ToNewArray() => _data.Span.ToArray();
        public void CopyFrom(ReadOnlySpan<float> span, uint targetOffset) => span.CopyTo(targetOffset == 0 ? _data.Span : _data.Span[(int)targetOffset..]);
        public void CopyTo(ITensorSegment segment, uint sourceOffset, uint targetOffset)
        {
            var span = GetSpan(sourceOffset);
            var destination = segment.GetArrayIfEasilyAvailable();
            if(destination is not null)
                span.CopyTo(destination.AsSpan((int)targetOffset, (int)(segment.Size - targetOffset)));
            else
                segment.CopyFrom(span, targetOffset);
        }

        public void CopyTo(Span<float> destination) => GetSpan().CopyTo(destination);
        public unsafe void CopyTo(float* destination, int sourceOffset, int stride, int count)
        {
            fixed (float* ptr = &_array[sourceOffset]) {
                var p = ptr;
                for (var i = 0; i < count; i++) {
                    *destination++ = *p;
                    p += stride;
                }
            }
        }

        public unsafe void Clear()
        {
            fixed (float* ptr = &_array[0]) {
                Unsafe.InitBlock(ptr, 0, (uint)_data.Length * sizeof(float));
            }
        }

        public ReadOnlySpan<float> GetSpan(ref SpanOwner<float> temp, out bool wasTempUsed)
        {
            wasTempUsed = false;
            return _array.AsSpan(0, (int)Size);
        }

        public ReadOnlySpan<float> GetSpan(uint offset = 0) => _array.AsSpan((int)offset, (int)Size);
        public (float[] Array, uint Offset, uint Stride) GetUnderlyingArray() => (_array, 0, 1);

        public override string ToString()
        {
            var preview = String.Join("|", Values.Take(8));
            if (Size > 8)
                preview += "|...";
            return $"{SegmentType} ({Size}): {preview}";
        }
    }
}
