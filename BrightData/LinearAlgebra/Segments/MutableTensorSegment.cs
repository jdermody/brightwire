using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra.Segments
{
    /// <summary>
    /// A tensor segment based on a float array
    /// </summary>
    public class MutableTensorSegment<T> : INumericSegment<T>, IHaveReadOnlyContiguousMemory<T> where T: unmanaged, INumber<T>
    {
        /// <summary>
        /// Underlying array
        /// </summary>
        protected readonly T[] _data;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Array of values</param>
        public MutableTensorSegment(params T[] data)
        {
            _data = data;
        }

        /// <inheritdoc />
        public virtual int AddRef()
        {
            // nop
            return 1;
        }

        /// <inheritdoc />
        public virtual int Release()
        {
            // nop
            return 1;
        }

        /// <inheritdoc />
        public virtual bool IsValid => true;

        /// <inheritdoc />
        public virtual void Dispose()
        {
            // nop
        }

        /// <inheritdoc />
        public virtual uint Size => (uint)_data.Length;

        /// <inheritdoc />
        public virtual string SegmentType => Consts.ArrayBased;

        /// <inheritdoc />
        public virtual T[] ToNewArray() => _data.ToArray();

        /// <inheritdoc />
        public virtual IEnumerable<T> Values => _data;

        /// <inheritdoc />
        public virtual void CopyTo(INumericSegment<T> segment, uint sourceOffset, uint targetOffset)
        {
            segment.CopyFrom(_data.AsSpan((int)sourceOffset), targetOffset);
        }

        /// <inheritdoc />
        public virtual void CopyTo(Span<T> destination) => ReadOnlySpan.CopyTo(destination);

        /// <inheritdoc />
        public unsafe void CopyTo(T* destination, int offset, int stride, int count)
        {
            fixed (T* ptr = &_data[offset])
            {
                var p = ptr;
                for (var i = 0; i < count; i++)
                {
                    *destination++ = *p;
                    p += stride;
                }
            }
        }

        /// <inheritdoc />
        public IHaveReadOnlyContiguousMemory<T> Contiguous => this;

        /// <inheritdoc />
        public virtual void CopyFrom(ReadOnlySpan<T> span, uint targetOffset)
        {
            span.CopyTo(_data.AsSpan((int)targetOffset));
        }

        /// <inheritdoc />
        public ReadOnlySpan<T> GetSpan(ref SpanOwner<T> temp, out bool wasTempUsed)
        {
            wasTempUsed = false;
            return ReadOnlySpan;
        }

        /// <inheritdoc />
        public bool IsWrapper => false;

        /// <inheritdoc />
        public override string ToString()
        {
            var preview = string.Join("|", Values.Take(8));
            if (Size > 8)
                preview += "|...";
            return $"{SegmentType} ({Size}): {preview}";
        }

        /// <inheritdoc cref="INumericSegment{T}" />
        public T this[int index]
        {
            get => _data[index];
            set => _data[index] = value;
        }

        /// <inheritdoc cref="INumericSegment{T}" />
        public T this[uint index]
        {
            get => _data[index];
            set => _data[index] = value;
        }

        /// <inheritdoc cref="INumericSegment{T}" />
        public T this[long index]
        {
            get => _data[index];
            set => _data[index] = value;
        }

        /// <inheritdoc cref="INumericSegment{T}" />
        public T this[ulong index]
        {
            get => _data[index];
            set => _data[index] = value;
        }

        /// <inheritdoc />
        public unsafe void Clear()
        {
            fixed (T* ptr = &_data[0])
            {
                Unsafe.InitBlock(ptr, 0, Size * sizeof(float));
            }
        }

        /// <inheritdoc />
        public (T[] Array, uint Offset, uint Stride) GetUnderlyingArray() => (_data, 0, 1);

        /// <inheritdoc />
        public virtual ReadOnlySpan<T> ReadOnlySpan => new(_data, 0, (int)Size);

        /// <inheritdoc />
        public ReadOnlyMemory<T> ContiguousMemory => new(_data, 0, (int)Size);
    }
}
