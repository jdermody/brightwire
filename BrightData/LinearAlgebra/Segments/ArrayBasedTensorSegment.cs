using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using BrightData;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra.Segments
{
    /// <summary>
    /// A tensor segment based on a float array
    /// </summary>
    public class ArrayBasedTensorSegment : INumericSegment<float>
    {
        protected readonly float[] _data;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Array of values</param>
        public ArrayBasedTensorSegment(params float[] data)
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
        public virtual float[] ToNewArray() => _data.ToArray();

        /// <inheritdoc />
        public virtual IEnumerable<float> Values => _data;

        /// <inheritdoc />
        public virtual void CopyTo(INumericSegment<float> segment, uint sourceOffset, uint targetOffset)
        {
            segment.CopyFrom(_data.AsSpan((int)sourceOffset), targetOffset);
        }

        /// <inheritdoc />
        public virtual void CopyTo(Span<float> destination) => GetSpan().CopyTo(destination);

        /// <inheritdoc />
        public unsafe void CopyTo(float* destination, int offset, int stride, int count)
        {
            fixed (float* ptr = &_data[offset])
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
        public virtual void CopyFrom(ReadOnlySpan<float> span, uint targetOffset)
        {
            span.CopyTo(_data.AsSpan((int)targetOffset));
        }

        /// <inheritdoc />
        public ReadOnlySpan<float> GetSpan(ref SpanOwner<float> temp, out bool wasTempUsed)
        {
            wasTempUsed = false;
            return GetSpan();
        }

        /// <inheritdoc />
        public virtual ReadOnlySpan<float> GetSpan(uint offset = 0) => _data.AsSpan((int)offset);

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

        /// <inheritdoc cref="INumericSegment" />
        public float this[int index]
        {
            get => _data[index];
            set => _data[index] = value;
        }

        /// <inheritdoc cref="INumericSegment" />
        public float this[uint index]
        {
            get => _data[index];
            set => _data[index] = value;
        }

        /// <inheritdoc cref="INumericSegment" />
        public float this[long index]
        {
            get => _data[index];
            set => _data[index] = value;
        }

        /// <inheritdoc cref="INumericSegment" />
        public float this[ulong index]
        {
            get => _data[index];
            set => _data[index] = value;
        }

        /// <inheritdoc />
        public float[] GetArrayIfEasilyAvailable() => _data;

        /// <inheritdoc />
        public unsafe void Clear()
        {
            fixed (float* ptr = &_data[0])
            {
                Unsafe.InitBlock(ptr, 0, Size * sizeof(float));
            }
        }

        /// <inheritdoc />
        public (float[] Array, uint Offset, uint Stride) GetUnderlyingArray() => (_data, 0, 1);
    }
}
