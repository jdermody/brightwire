using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra.Segments
{
    /// <summary>
    /// A tensor segment that temporarily owns a buffer from an array pool
    /// </summary>
    public class ArrayPoolTensorSegment : ITensorSegment
    {
        readonly MemoryOwner<float> _data;
        readonly float[] _array;
        int _refCount = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Rented buffer from pool</param>
        public ArrayPoolTensorSegment(MemoryOwner<float> data)
        {
            _data = data;
            _array = data.DangerousGetArray().Array!;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Release();
        }

        /// <inheritdoc />
        public int AddRef() => Interlocked.Increment(ref _refCount);

        /// <inheritdoc />
        public int Release()
        {
            var ret = Interlocked.Decrement(ref _refCount);
            if (IsValid && ret <= 0)
            {
                _data.Dispose();
                IsValid = false;
            }

            return ret;
        }

        /// <inheritdoc />
        public bool IsValid { get; private set; } = true;

        /// <inheritdoc />
        public uint Size => (uint)_data.Length;

        /// <inheritdoc />
        public string SegmentType => "memory owner";

        /// <inheritdoc />
        public bool IsWrapper => false;

        /// <inheritdoc cref="ITensorSegment" />
        public float this[int index]
        {
            get => _array[index];
            set => _array[index] = value;
        }

        /// <inheritdoc cref="ITensorSegment" />
        public float this[uint index]
        {
            get => _array[index];
            set => _array[index] = value;
        }

        /// <inheritdoc cref="ITensorSegment" />
        public float this[long index]
        {
            get => _array[index];
            set => _array[index] = value;
        }

        /// <inheritdoc cref="ITensorSegment" />
        public float this[ulong index]
        {
            get => _array[index];
            set => _array[index] = value;
        }

        /// <inheritdoc />
        public IEnumerable<float> Values
        {
            get
            {
                for (int i = 0, len = (int)Size; i < len; i++)
                    yield return _array[i];
            }
        }

        /// <inheritdoc />
        public float[] GetArrayIfEasilyAvailable() => _array;

        /// <inheritdoc />
        public float[] ToNewArray() => _data.Span.ToArray();

        /// <inheritdoc />
        public void CopyFrom(ReadOnlySpan<float> span, uint targetOffset) => span.CopyTo(targetOffset == 0 ? _data.Span : _data.Span[(int)targetOffset..]);

        /// <inheritdoc />
        public void CopyTo(ITensorSegment segment, uint sourceOffset, uint targetOffset)
        {
            var span = GetSpan(sourceOffset);
            var destination = segment.GetArrayIfEasilyAvailable();
            if (destination is not null)
                span.CopyTo(destination.AsSpan((int)targetOffset, (int)(segment.Size - targetOffset)));
            else
                segment.CopyFrom(span, targetOffset);
        }

        /// <inheritdoc />
        public void CopyTo(Span<float> destination) => GetSpan().CopyTo(destination);

        /// <inheritdoc />
        public unsafe void CopyTo(float* destination, int sourceOffset, int stride, int count)
        {
            fixed (float* ptr = &_array[sourceOffset])
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
        public unsafe void Clear()
        {
            fixed (float* ptr = &_array[0])
            {
                Unsafe.InitBlock(ptr, 0, (uint)_data.Length * sizeof(float));
            }
        }

        /// <inheritdoc />
        public ReadOnlySpan<float> GetSpan(ref SpanOwner<float> temp, out bool wasTempUsed)
        {
            wasTempUsed = false;
            return _array.AsSpan(0, (int)Size);
        }

        /// <inheritdoc />
        public ReadOnlySpan<float> GetSpan(uint offset = 0) => _array.AsSpan((int)offset, (int)Size);

        /// <inheritdoc />
        public (float[] Array, uint Offset, uint Stride) GetUnderlyingArray() => (_array, 0, 1);

        /// <inheritdoc />
        public override string ToString()
        {
            var preview = string.Join("|", Values.Take(8));
            if (Size > 8)
                preview += "|...";
            return $"{SegmentType} ({Size}): {preview}";
        }
    }
}
