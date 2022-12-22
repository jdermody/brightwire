using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra
{
    /// <summary>
    /// A tensor segment based on a float array
    /// </summary>
    public class ArrayBasedTensorSegment : ITensorSegment
    {
        readonly float[] _data;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Array of values</param>
        public ArrayBasedTensorSegment(params float[] data)
        {
            _data = data;
        }

        /// <inheritdoc />
        public int AddRef()
        {
            // nop
            return 1;
        }

        /// <inheritdoc />
        public int Release()
        {
            // nop
            return 1;
        }

        /// <inheritdoc />
        public bool IsValid => true;

        /// <inheritdoc />
        public void Dispose()
        {
            // nop
        }

        /// <inheritdoc />
        public uint Size => (uint)_data.Length;

        /// <inheritdoc />
        public string SegmentType => Consts.ArrayBased;

        /// <inheritdoc />
        public float this[int index]
        {
            get => _data[index];
            set => _data[index] = value;
        }

        /// <inheritdoc />
        public float this[uint index]
        {
            get => _data[index];
            set => _data[index] = value;
        }

        /// <inheritdoc />
        public float this[long index]
        {
            get => _data[index];
            set => _data[index] = value;
        }

        /// <inheritdoc />
        public float this[ulong index]
        {
            get => _data[index];
            set => _data[index] = value;
        }

        /// <inheritdoc />
        public IEnumerable<float> Values => _data;

        /// <inheritdoc />
        public float[] GetArrayIfEasilyAvailable() => _data;

        /// <inheritdoc />
        public float[] ToNewArray() => _data.ToArray();

        /// <inheritdoc />
        public void CopyFrom(ReadOnlySpan<float> span, uint targetOffset)
        {
            span.CopyTo(_data.AsSpan((int)targetOffset));
        }

        /// <inheritdoc />
        public void CopyTo(ITensorSegment segment, uint sourceOffset, uint targetOffset)
        {
            segment.CopyFrom(_data.AsSpan((int)sourceOffset), targetOffset);
        }

        /// <inheritdoc />
        public void CopyTo(Span<float> destination)
        {
            _data.CopyTo(destination);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public unsafe void Clear()
        {
            fixed (float* ptr = &_data[0]) {
                Unsafe.InitBlock(ptr, 0, (uint)_data.Length * sizeof(float));
            }
        }

        /// <inheritdoc />
        public ReadOnlySpan<float> GetSpan(ref SpanOwner<float> temp, out bool wasTempUsed)
        {
            wasTempUsed = false;
            return new(_data); 
        }

        /// <inheritdoc />
        public ReadOnlySpan<float> GetSpan(uint offset = 0) => _data.AsSpan((int)offset);

        /// <inheritdoc />
        public (float[] Array, uint Offset, uint Stride) GetUnderlyingArray() => (_data, 0, 1);

        /// <inheritdoc />
        public bool IsWrapper => false;

        /// <inheritdoc />
        public override string ToString()
        {
            var preview = String.Join("|", Values.Take(8));
            if (Size > 8)
                preview += "|...";
            return $"{SegmentType} ({Size}): {preview}";
        }
    }
}
