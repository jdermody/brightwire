﻿using System;
using System.Linq;
using System.Numerics;

namespace BrightData.LinearAlgebra.Segments
{
    /// <summary>
    /// A tensor segment that wraps another tensor segment
    /// </summary>
    public class MutableTensorSegmentWrapper<T> : ReadOnlyTensorSegmentWrapper<T>, INumericSegment<T> where T: unmanaged, INumber<T>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="segment">Tensor segment to wrap</param>
        /// <param name="offset">First index within the wrapped tensor segment</param>
        /// <param name="stride">Stride within the wrapped tensor segment</param>
        /// <param name="length">Number of values in this tensor segment</param>
        public MutableTensorSegmentWrapper(INumericSegment<T> segment, uint offset, uint stride, uint length) : base(segment, offset, stride, length)
        {
            UnderlyingSegment = segment;
        }

        /// <summary>
        /// The segment that was wrapped by this tensor segment
        /// </summary>
        public new INumericSegment<T> UnderlyingSegment { get; }

        /// <inheritdoc cref="INumericSegment{T}" />
        public new T this[int index]
        {
            get => UnderlyingSegment[Offset + index * Stride];
            set => UnderlyingSegment[Offset + index * Stride] = value;
        }

        /// <inheritdoc cref="INumericSegment{T}" />
        public new T this[uint index]
        {
            get => UnderlyingSegment[Offset + index * Stride];
            set => UnderlyingSegment[Offset + index * Stride] = value;
        }

        /// <inheritdoc cref="INumericSegment{T}" />
        public new T this[long index]
        {
            get => UnderlyingSegment[Offset + index * Stride];
            set => UnderlyingSegment[Offset + index * Stride] = value;
        }

        /// <inheritdoc cref="INumericSegment{T}" />
        public new T this[ulong index]
        {
            get => UnderlyingSegment[Offset + index * Stride];
            set => UnderlyingSegment[Offset + index * Stride] = value;
        }

        /// <inheritdoc />
        public void CopyFrom(ReadOnlySpan<T> span, uint targetOffset)
        {
            if (Stride == 1 && !UnderlyingSegment.IsWrapper)
            {
                var (array, offset, stride) = UnderlyingSegment.GetUnderlyingArray();
                if (array is not null && stride == 1)
                {
                    span.CopyTo(new Span<T>(array, (int)(offset + Offset + targetOffset), (int)(Size - targetOffset)));
                    return;
                }
            }

            var index = targetOffset;
            foreach (var val in span)
                this[index++] = val;
        }

        /// <inheritdoc />
        public void Clear()
        {
            for (uint i = 0; i < Size; i++)
                this[i] = T.Zero;
        }

        /// <inheritdoc />
        public (T[]? Array, uint Offset, uint Stride) GetUnderlyingArray()
        {
            var (array, offset, stride) = UnderlyingSegment.GetUnderlyingArray();
            return (array, offset + Offset, stride + Stride);
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
