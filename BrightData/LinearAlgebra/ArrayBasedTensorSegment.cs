using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra
{
    public class ArrayBasedTensorSegment : ITensorSegment
    {
        readonly float[] _data;

        public ArrayBasedTensorSegment(float[] data)
        {
            _data = data;
        }

        public int AddRef()
        {
            // nop
            return 1;
        }

        public int Release()
        {
            // nop
            return 1;
        }

        public bool IsValid => true;
        public void Dispose()
        {
            // nop
        }

        public uint Size => (uint)_data.Length;
        public string SegmentType => Consts.ArrayBased;

        public float this[int index]
        {
            get => _data[index];
            set => _data[index] = value;
        }
        public float this[uint index]
        {
            get => _data[index];
            set => _data[index] = value;
        }
        public float this[long index]
        {
            get => _data[index];
            set => _data[index] = value;
        }
        public float this[ulong index]
        {
            get => _data[index];
            set => _data[index] = value;
        }

        public IEnumerable<float> Values => _data;
        public float[] GetArrayIfEasilyAvailable() => _data;

        public float[] ToNewArray() => _data.ToArray();

        public void CopyFrom(ReadOnlySpan<float> span, uint targetOffset)
        {
            span.CopyTo(_data.AsSpan((int)targetOffset));
        }

        public void CopyTo(ITensorSegment segment, uint sourceOffset, uint targetOffset)
        {
            segment.CopyFrom(_data.AsSpan((int)sourceOffset), targetOffset);
        }

        public void CopyTo(Span<float> destination)
        {
            _data.CopyTo(destination);
        }
        public unsafe void CopyTo(float* destination, int offset, int stride, int count)
        {
            fixed (float* ptr = &_data[offset]) {
                var p = ptr;
                for (var i = 0; i < count; i++) {
                    *destination++ = *p;
                    p += stride;
                }
            }
        }

        public unsafe void Clear()
        {
            fixed (float* ptr = &_data[0]) {
                Unsafe.InitBlock(ptr, 0, (uint)_data.Length * sizeof(float));
            }
        }

        public ReadOnlySpan<float> GetSpan(ref SpanOwner<float> temp, out bool wasTempUsed)
        {
            wasTempUsed = false;
            return new(_data); 
        }

        public ReadOnlySpan<float> GetSpan(uint offset = 0) => _data.AsSpan((int)offset);
        public (float[] Array, uint Offset, uint Stride) GetUnderlyingArray() => (_data, 0, 1);
        public bool IsWrapper => false;

        public override string ToString()
        {
            var preview = String.Join("|", Values.Take(8));
            if (Size > 8)
                preview += "|...";
            return $"{SegmentType} ({Size}): {preview}";
        }
    }
}
