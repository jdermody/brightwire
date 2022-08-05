using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra
{
    public class TensorSegmentWrapper : ITensorSegment
    {
        public TensorSegmentWrapper(ITensorSegment segment, uint offset, uint stride, uint length)
        {
            UnderlyingSegment = segment;
            Offset = offset;
            Stride = stride;
            Size = length;
        }

        public void Dispose()
        {
            // nop - calling dispose here implies that no reference was made of the underlying segment
        }

        public int AddRef() => UnderlyingSegment.AddRef();
        public int Release() => UnderlyingSegment.Release();

        public bool IsValid => UnderlyingSegment.IsValid;
        public uint Size { get; }
        public string SegmentType => "segment wrapper";
        public ITensorSegment UnderlyingSegment { get; }
        public uint Offset { get; }
        public uint Stride { get; }
        public bool IsWrapper => true;

        public float this[int index]
        {
            get => UnderlyingSegment[Offset + index * Stride];
            set => UnderlyingSegment[Offset + index * Stride] = value;
        }

        public float this[uint index]
        {
            get => UnderlyingSegment[Offset + index * Stride];
            set => UnderlyingSegment[Offset + index * Stride] = value;
        }

        public float this[long index]
        {
            get => UnderlyingSegment[Offset + index * Stride];
            set => UnderlyingSegment[Offset + index * Stride] = value;
        }

        public float this[ulong index]
        {
            get => UnderlyingSegment[Offset + index * Stride];
            set => UnderlyingSegment[Offset + index * Stride] = value;
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
        public void CopyFrom(ReadOnlySpan<float> span, uint targetOffset)
        {
            if (Stride == 1 && !UnderlyingSegment.IsWrapper) {
                var (array, offset, stride) = UnderlyingSegment.GetUnderlyingArray();
                if (array is not null && stride == 1) {
                    span.CopyTo(new Span<float>(array, (int)(offset + Offset + targetOffset), (int)(Size - targetOffset)));
                    return;
                }
            }

            var index = targetOffset;
            foreach (var val in span)
                this[index++] = val;
        }

        public void CopyTo(ITensorSegment segment, uint sourceOffset, uint targetoffset)
        {
            if (Stride == 1 && !UnderlyingSegment.IsWrapper) {
                var (array, offset, stride) = UnderlyingSegment.GetUnderlyingArray();
                if (array is not null && stride == 1) {
                    segment.CopyFrom(new ReadOnlySpan<float>(array, (int)(offset + Offset + sourceOffset), (int)(Size - sourceOffset)), targetoffset);
                    return;
                }
            }

            using var tempBuffer = SpanOwner<float>.Allocate((int)(Size - sourceOffset));
            var span = tempBuffer.Span;
            for(var i = 0; i < Size; i++)
                span[i] = this[i + sourceOffset];
            segment.CopyFrom(span, targetoffset);
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
            if (Stride == 1)
                return UnderlyingSegment.GetSpan(ref temp, out wasTempUsed).Slice((int)Offset, (int)Size);

            temp = SpanOwner<float>.Allocate((int)Size);
            var span = temp.Span;
            CopyTo(span);
            wasTempUsed = true;
            return span;
        }

        public ReadOnlySpan<float> GetSpan(uint offset)
        {
            throw new NotImplementedException();
        }

        public (float[]? Array, uint Offset, uint Stride) GetUnderlyingArray()
        {
            var (array, offset, stride) = UnderlyingSegment.GetUnderlyingArray();
            return (array, offset + Offset, stride + Stride);
        }

        public override string ToString()
        {
            var preview = String.Join("|", Values.Take(8));
            if (Size > 8)
                preview += "|...";
            return $"{SegmentType} [{Offset}, {Stride}] ({Size}): {preview}";
        }
    }
}
