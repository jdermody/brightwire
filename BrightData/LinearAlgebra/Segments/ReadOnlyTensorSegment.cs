using System;
using System.Collections.Generic;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra.Segments
{
    internal class ReadOnlyTensorSegment(ReadOnlyMemory<float> data, uint offset = 0, uint? size = null)
        : IReadOnlyNumericSegment<float>, IHaveReadOnlyContiguousSpan<float>
    {
        public int AddRef() => 1;
        public int Release() => 1;
        public bool IsValid => true;
        public void Dispose()
        {
            // nop
        }

        public uint Size { get; } = size ?? (uint)data.Length;

        public ReadOnlySpan<float> GetSpan(ref SpanOwner<float> temp, out bool wasTempUsed)
        {
            wasTempUsed = false;
            return ReadOnlySpan;
        }

        public string SegmentType => Consts.MemoryBased;
        public float this[int index] => data.Span[index + (int)offset];
        public float this[uint index] => data.Span[(int)index + (int)offset];
        public float this[long index] => data.Span[(int)index + (int)offset];
        public float this[ulong index] => data.Span[(int)index + (int)offset];
        public float[] ToNewArray() => ReadOnlySpan.ToArray();

        public IEnumerable<float> Values
        {
            get
            {
                for(var i = 0; i < Size; i++)
                    yield return data.Span[i + (int)offset];
            }
        }
        public void CopyTo(INumericSegment<float> segment, uint sourceOffset = 0, uint targetOffset = 0)
        {
            segment.CopyFrom(ReadOnlySpan[(int)sourceOffset..], targetOffset);
        }

        public void CopyTo(Span<float> destination)
        {
            ReadOnlySpan.CopyTo(destination);
        }

        public unsafe void CopyTo(float* destination, int sourceOffset, int stride, int count)
        {
            fixed (float* ptr = data.Span[(sourceOffset + (int)offset)..])
            {
                var p = ptr;
                for (var i = 0; i < count; i++)
                {
                    *destination++ = *p;
                    p += stride;
                }
            }
        }

        public IHaveReadOnlyContiguousSpan<float> Contiguous => this;
        public bool IsWrapper => false;
        public ReadOnlySpan<float> ReadOnlySpan => data.Span.Slice((int)offset, (int)Size);
    }
}
