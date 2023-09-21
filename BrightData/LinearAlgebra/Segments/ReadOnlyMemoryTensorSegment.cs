using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra.Segments
{
    internal class ReadOnlyMemoryTensorSegment : IReadOnlyNumericSegment<float>, IHaveReadOnlyContiguousSpan<float>
    {
        readonly uint _offset;
        readonly ReadOnlyMemory<float> _data;

        public ReadOnlyMemoryTensorSegment(ReadOnlyMemory<float> data, uint offset = 0, uint? size = null)
        {
            _data = data;
            Size = size ?? (uint)data.Length;
            _offset = offset;
        }

        public int AddRef() => 1;
        public int Release() => 1;
        public bool IsValid => true;
        public void Dispose()
        {
            // nop
        }

        public uint Size { get; }
        public ReadOnlySpan<float> GetSpan(ref SpanOwner<float> temp, out bool wasTempUsed)
        {
            wasTempUsed = false;
            return ReadOnlySpan;
        }

        public string SegmentType => Consts.MemoryBased;
        public float this[int index] => _data.Span[index + (int)_offset];
        public float this[uint index] => _data.Span[(int)index + (int)_offset];
        public float this[long index] => _data.Span[(int)index + (int)_offset];
        public float this[ulong index] => _data.Span[(int)index + (int)_offset];
        public float[] ToNewArray() => ReadOnlySpan.ToArray();

        public IEnumerable<float> Values
        {
            get
            {
                for(var i = 0; i < Size; i++)
                    yield return _data.Span[i + (int)_offset];
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
            fixed (float* ptr = _data.Span[(sourceOffset + (int)_offset)..])
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
        public ReadOnlySpan<float> ReadOnlySpan => _data.Span.Slice((int)_offset, (int)Size);
    }
}
