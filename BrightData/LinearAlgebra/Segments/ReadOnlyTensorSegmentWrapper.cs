using CommunityToolkit.HighPerformance.Buffers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BrightData.LinearAlgebra.Segments
{
    /// <summary>
    /// Read only tensor segment wrapper
    /// </summary>
    public class ReadOnlyTensorSegmentWrapper<T> : IReadOnlyNumericSegment<T> where T: unmanaged, INumber<T>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="segment">Tensor segment to wrap</param>
        /// <param name="offset">First index within the wrapped tensor segment</param>
        /// <param name="stride">Stride within the wrapped tensor segment</param>
        /// <param name="length">Number of values in this tensor segment</param>
        public ReadOnlyTensorSegmentWrapper(IReadOnlyNumericSegment<T> segment, uint offset, uint stride, uint length)
        {
            UnderlyingSegment = segment;
            Offset = offset;
            Stride = stride;
            Size = length;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            GC.SuppressFinalize(this);
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
        public IReadOnlyNumericSegment<T> UnderlyingSegment { get; }

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
        public T this[int index] => UnderlyingSegment[Offset + index * Stride];

        /// <inheritdoc />
        public T this[uint index] => UnderlyingSegment[Offset + index * Stride];

        /// <inheritdoc />
        public T this[long index] => UnderlyingSegment[Offset + index * Stride];

        /// <inheritdoc />
        public T this[ulong index] => UnderlyingSegment[Offset + index * Stride];

        /// <inheritdoc />
        public IEnumerable<T> Values
        {
            get
            {
                for (uint i = 0; i < Size; i++)
                    yield return this[i];
            }
        }

        /// <inheritdoc />
        public T[] ToNewArray() => Values.ToArray();

        /// <inheritdoc />
        public void CopyTo(INumericSegment<T> segment, uint sourceOffset, uint targetOffset)
        {
            if (Stride == 1 && !UnderlyingSegment.IsWrapper && UnderlyingSegment is INumericSegment<T> underlyingSegment)
            {
                var (array, offset, stride) = underlyingSegment.GetUnderlyingArray();
                if (array is not null && stride == 1)
                {
                    segment.CopyFrom(new ReadOnlySpan<T>(array, (int)(offset + Offset + sourceOffset), (int)(Size - sourceOffset)), targetOffset);
                    return;
                }
            }

            using var tempBuffer = SpanOwner<T>.Allocate((int)(Size - sourceOffset));
            var span = tempBuffer.Span;
            for (var i = 0; i < Size; i++)
                span[i] = this[i + sourceOffset];
            segment.CopyFrom(span, targetOffset);
        }

        /// <inheritdoc />
        public void CopyTo(Span<T> destination)
        {
            for (var i = 0; i < Size; i++)
                destination[i] = this[i];
        }

        /// <inheritdoc />
        public unsafe void CopyTo(T* destination, int offset, int stride, int count)
        {
            for (var i = 0; i < count; i++)
                *destination++ = this[offset + stride * i];
        }

        /// <inheritdoc />
        public IHaveReadOnlyContiguousMemory<T>? Contiguous => null;

        /// <inheritdoc />
        public ReadOnlySpan<T> GetSpan(ref SpanOwner<T> temp, out bool wasTempUsed)
        {
            if (Stride == 1)
                return UnderlyingSegment.GetSpan(ref temp, out wasTempUsed).Slice((int)Offset, (int)Size);

            temp = SpanOwner<T>.Allocate((int)Size);
            var span = temp.Span;
            CopyTo(span);
            wasTempUsed = true;
            return span;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var preview = string.Join("|", Values.Take(8));
            if (Size > 8)
                preview += "|...";
            return $"{SegmentType} [{Offset}, {Stride}] ({Size}): {preview}";
        }
    }
}
