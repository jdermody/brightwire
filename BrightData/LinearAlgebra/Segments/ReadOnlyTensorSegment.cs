using System;
using System.Collections.Generic;
using System.Numerics;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra.Segments
{
    internal class ReadOnlyTensorSegment<T>(ReadOnlyMemory<T> data, uint offset = 0, uint? size = null)
        : IReadOnlyNumericSegment<T>, IHaveReadOnlyContiguousSpan<T>
    where T: unmanaged, INumber<T>
    {
        public int AddRef() => 1;
        public int Release() => 1;
        public bool IsValid => true;
        public void Dispose()
        {
            // nop
        }

        public uint Size { get; } = size ?? (uint)data.Length;

        public ReadOnlySpan<T> GetSpan(ref SpanOwner<T> temp, out bool wasTempUsed)
        {
            wasTempUsed = false;
            return ReadOnlySpan;
        }

        public string SegmentType => Consts.MemoryBased;
        public T this[int index] => data.Span[index + (int)offset];
        public T this[uint index] => data.Span[(int)index + (int)offset];
        public T this[long index] => data.Span[(int)index + (int)offset];
        public T this[ulong index] => data.Span[(int)index + (int)offset];
        public T[] ToNewArray() => ReadOnlySpan.ToArray();

        public IEnumerable<T> Values
        {
            get
            {
                for(var i = 0; i < Size; i++)
                    yield return data.Span[i + (int)offset];
            }
        }
        public void CopyTo(INumericSegment<T> segment, uint sourceOffset = 0, uint targetOffset = 0)
        {
            segment.CopyFrom(ReadOnlySpan[(int)sourceOffset..], targetOffset);
        }

        public void CopyTo(Span<T> destination)
        {
            ReadOnlySpan.CopyTo(destination);
        }

        public unsafe void CopyTo(T* destination, int sourceOffset, int stride, int count)
        {
            fixed (T* ptr = data.Span[(sourceOffset + (int)offset)..])
            {
                var p = ptr;
                for (var i = 0; i < count; i++)
                {
                    *destination++ = *p;
                    p += stride;
                }
            }
        }

        public IHaveReadOnlyContiguousSpan<T> Contiguous => this;
        public bool IsWrapper => false;
        public ReadOnlySpan<T> ReadOnlySpan => data.Span.Slice((int)offset, (int)Size);
    }
}
