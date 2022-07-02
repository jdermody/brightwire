using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlegbra2
{
    public class TensorSegmentWrapper2 : ITensorSegment2
    {
        readonly ITensorSegment2 _segment;
        readonly uint _offset, _stride;

        public TensorSegmentWrapper2(ITensorSegment2 segment, uint offset, uint stride, uint length)
        {
            _segment = segment;
            _offset = offset;
            _stride = stride;
            Size = length;
        }

        public void Dispose()
        {
            // nop - calling dispose here implies that no reference was made of the underlying segment
        }

        public int AddRef() => _segment.AddRef();
        public int Release() => _segment.Release();

        public bool IsValid => _segment.IsValid;
        public uint Size { get; }
        public string SegmentType => "segment wrapper";

        public float this[int index]
        {
            get => _segment[_offset + index * _stride];
            set => _segment[_offset + index * _stride] = value;
        }

        public float this[uint index]
        {
            get => _segment[_offset + index * _stride];
            set => _segment[_offset + index * _stride] = value;
        }

        public float this[long index]
        {
            get => _segment[_offset + index * _stride];
            set => _segment[_offset + index * _stride] = value;
        }

        public float this[ulong index]
        {
            get => _segment[_offset + index * _stride];
            set => _segment[_offset + index * _stride] = value;
        }

        public IEnumerable<float> Values
        {
            get
            {
                for(uint i = 0; i < Size; i++)
                    yield return this[i];
            }
        }
        public float[]? GetArrayForLocalUseOnly() => null;
        public float[] ToNewArray() => Values.ToArray();
        public void CopyFrom(ReadOnlySpan<float> span)
        {
            uint index = 0;
            foreach (var val in span)
                this[index++] = val;
        }

        public void CopyTo(ITensorSegment2 segment)
        {
            using var tempBuffer = SpanOwner<float>.Allocate((int)Size);
            var span = tempBuffer.Span;
            for(var i = 0; i < Size; i++)
                span[i] = this[i];
            segment.CopyFrom(span);
        }

        public void CopyTo(Span<float> destination)
        {
            for(var i = 0; i < Size; i++)
                destination[i] = this[i];
        }
        public unsafe void CopyTo(float* destination, int offset, int stride, int count)
        {
            for (var i = 0; i < count; i++)
                *destination++ = this[offset + (stride * i)];
        }

        public void Clear()
        {
            for (uint i = 0; i < Size; i++)
                this[i] = 0f;
        }

        public ReadOnlySpan<float> GetSpan(ref SpanOwner<float> temp, out bool wasTempUsed)
        {
            if (_stride == 1)
                return _segment.GetSpan(ref temp, out wasTempUsed).Slice((int)_offset, (int)Size);

            temp = SpanOwner<float>.Allocate((int)Size);
            var span = temp.Span;
            CopyTo(span);
            wasTempUsed = true;
            return span;
        }

        public ReadOnlySpan<float> GetSpan()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            var preview = String.Join("|", Values.Take(8));
            if (Size > 8)
                preview += "|...";
            return $"{SegmentType} [{_offset}, {_stride}] ({Size}): {preview}";
        }
    }
}
