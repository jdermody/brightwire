using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra
{
    /// <summary>
    /// A tensor segment that wraps another tensor segment
    /// </summary>
    public class TensorSegmentWrapper : ITensorSegment
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="segment">Tensor segment to wrap</param>
        /// <param name="offset">First index within the wrapped tensor segment</param>
        /// <param name="stride">Stride within the wrapped tensor segment</param>
        /// <param name="length">Number of values in this tensor segment</param>
        public TensorSegmentWrapper(ITensorSegment segment, uint offset, uint stride, uint length)
        {
            UnderlyingSegment = segment;
            Offset = offset;
            Stride = stride;
            Size = length;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            // nop - calling dispose here implies that no reference was made of the underlying segment
        }

        /// <inheritdoc />
        public int AddRef() => UnderlyingSegment.AddRef();

        /// <inheritdoc />
        public int Release() => UnderlyingSegment.Release();

        /// <inheritdoc />
        public bool IsValid => UnderlyingSegment.IsValid;

        /// <inheritdoc />
        public uint Size { get; }

        /// <inheritdoc />
        public string SegmentType => "segment wrapper";

        /// <summary>
        /// The segment that was wrapped by this tensor segment
        /// </summary>
        public ITensorSegment UnderlyingSegment { get; }

        /// <summary>
        /// First index within the wrapped tensor segment
        /// </summary>
        public uint Offset { get; }

        /// <summary>
        /// Stride within the wrapped tensor segment 
        /// </summary>
        public uint Stride { get; }

        /// <inheritdoc />
        public bool IsWrapper => true;

        /// <inheritdoc />
        public float this[int index]
        {
            get => UnderlyingSegment[Offset + index * Stride];
            set => UnderlyingSegment[Offset + index * Stride] = value;
        }

        /// <inheritdoc />
        public float this[uint index]
        {
            get => UnderlyingSegment[Offset + index * Stride];
            set => UnderlyingSegment[Offset + index * Stride] = value;
        }

        /// <inheritdoc />
        public float this[long index]
        {
            get => UnderlyingSegment[Offset + index * Stride];
            set => UnderlyingSegment[Offset + index * Stride] = value;
        }

        /// <inheritdoc />
        public float this[ulong index]
        {
            get => UnderlyingSegment[Offset + index * Stride];
            set => UnderlyingSegment[Offset + index * Stride] = value;
        }

        /// <inheritdoc />
        public IEnumerable<float> Values
        {
            get
            {
                for(uint i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <inheritdoc />
        public float[]? GetArrayIfEasilyAvailable() => null;

        /// <inheritdoc />
        public float[] ToNewArray() => Values.ToArray();

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
        public void CopyTo(Span<float> destination)
        {
            for(var i = 0; i < Size; i++)
                destination[i] = this[i];
        }

        /// <inheritdoc />
        public unsafe void CopyTo(float* destination, int offset, int stride, int count)
        {
            for (var i = 0; i < count; i++)
                *destination++ = this[offset + (stride * i)];
        }

        /// <inheritdoc />
        public void Clear()
        {
            for (uint i = 0; i < Size; i++)
                this[i] = 0f;
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public ReadOnlySpan<float> GetSpan(uint offset)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public (float[]? Array, uint Offset, uint Stride) GetUnderlyingArray()
        {
            var (array, offset, stride) = UnderlyingSegment.GetUnderlyingArray();
            return (array, offset + Offset, stride + Stride);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var preview = String.Join("|", Values.Take(8));
            if (Size > 8)
                preview += "|...";
            return $"{SegmentType} [{Offset}, {Stride}] ({Size}): {preview}";
        }
    }
}
