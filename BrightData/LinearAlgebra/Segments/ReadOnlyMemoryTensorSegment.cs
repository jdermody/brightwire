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
        readonly ReadOnlyMemory<float> _data;

        public ReadOnlyMemoryTensorSegment(ReadOnlyMemory<float> data)
        {
            _data = data;
        }

        public int AddRef() => 1;
        public int Release() => 1;
        public bool IsValid => true;
        public void Dispose()
        {
            // nop
        }

        public uint Size => (uint)_data.Length;
        public ReadOnlySpan<float> GetSpan(ref SpanOwner<float> temp, out bool wasTempUsed)
        {
            wasTempUsed = false;
            return _data.Span;
        }

        public string SegmentType => Consts.MemoryBased;
        public float this[int index] => _data.Span[index];
        public float this[uint index] => _data.Span[(int)index];
        public float this[long index] => _data.Span[(int)index];
        public float this[ulong index] => _data.Span[(int)index];
        public float[] ToNewArray() => _data.ToArray();

        public IEnumerable<float> Values
        {
            get
            {
                for(var i = 0; i < _data.Length; i++)
                    yield return _data.Span[i];
            }
        }
        public void CopyTo(INumericSegment<float> segment, uint sourceOffset = 0, uint targetOffset = 0)
        {
            segment.CopyFrom(_data.Span[(int)sourceOffset..], targetOffset);
        }

        public void CopyTo(Span<float> destination)
        {
            _data.Span.CopyTo(destination);
        }

        public unsafe void CopyTo(float* destination, int sourceOffset, int stride, int count)
        {
            fixed (float* ptr = _data.Span[sourceOffset..])
            {
                var p = ptr;
                for (var i = 0; i < count; i++)
                {
                    *destination++ = *p;
                    p += stride;
                }
            }
        }

        public IHaveReadOnlyContiguousSpan<float>? Contiguous => this;
        public bool IsWrapper => false;
        public ReadOnlySpan<float> ReadOnlySpan => _data.Span;
    }
}
