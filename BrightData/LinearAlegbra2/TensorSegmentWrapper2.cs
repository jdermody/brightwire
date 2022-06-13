using System;
using System.Collections.Generic;
using System.Linq;

namespace BrightData.LinearAlegbra2
{
    public class TensorSegmentWrapper2 : IDisposableTensorSegmentWrapper
    {
        readonly ITensorSegment2 _segment;
        readonly uint _offset, _stride;

        public TensorSegmentWrapper2(ITensorSegment2 segment, uint offset, uint stride, uint length)
        {
            _segment = segment;
            _offset = offset;
            _stride = stride;
            Size = length;
        }

        public void Dispose()
        {
            // nop - calling dispose here implies that no reference was made of the underlying segment
        }

        public int AddRef() => _segment.AddRef();
        public int Release() => _segment.Release();

        public bool IsValid => _segment.IsValid;
        public uint Size { get; }
        public string SegmentType => "segment wrapper";

        public float this[int index]
        {
            get => _segment[_offset + index * _stride];
            set => _segment[_offset + index * _stride] = value;
        }

        public float this[uint index]
        {
            get => _segment[_offset + index * _stride];
            set => _segment[_offset + index * _stride] = value;
        }

        public float this[long index]
        {
            get => _segment[_offset + index * _stride];
            set => _segment[_offset + index * _stride] = value;
        }

        public float this[ulong index]
        {
            get => _segment[_offset + index * _stride];
            set => _segment[_offset + index * _stride] = value;
        }

        public IEnumerable<float> Values
        {
            get
            {
                for(uint i = 0; i < Size; i++)
                    yield return this[i];
            }
        }
        public float[]? GetArrayForLocalUseOnly() => null;
        public float[] ToNewArray() => Values.ToArray();
        public void CopyFrom(Span<float> span)
        {
            uint index = 0;
            foreach (var val in span)
                this[index++] = val;
        }

        public void CopyTo(ITensorSegment2 segment)
        {
            for(uint i = 0; i < Size; i++)
                segment[i] = this[i];
        }

        public void Clear()
        {
            for (uint i = 0; i < Size; i++)
                this[i] = 0f;
        }
    }
}
