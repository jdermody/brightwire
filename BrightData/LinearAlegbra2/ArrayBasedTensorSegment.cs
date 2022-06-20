using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.LinearAlegbra2
{
    internal class ArrayBasedTensorSegment : ITensorSegment2
    {
        readonly float[] _data;

        public ArrayBasedTensorSegment(float[] data)
        {
            _data = data;
        }

        public int AddRef()
        {
            // nop
            return 1;
        }

        public int Release()
        {
            // nop
            return 1;
        }

        public bool IsValid => true;
        public void Dispose()
        {
            // nop
        }

        public uint Size => (uint)_data.Length;
        public string SegmentType => Consts.ArrayBased;

        public float this[int index]
        {
            get => _data[index];
            set => _data[index] = value;
        }
        public float this[uint index]
        {
            get => _data[index];
            set => _data[index] = value;
        }
        public float this[long index]
        {
            get => _data[index];
            set => _data[index] = value;
        }
        public float this[ulong index]
        {
            get => _data[index];
            set => _data[index] = value;
        }

        public IEnumerable<float> Values => _data;
        public float[]? GetArrayForLocalUseOnly() => _data;

        public float[] ToNewArray() => _data.ToArray();

        public void CopyFrom(ReadOnlySpan<float> span)
        {
            span.CopyTo(_data);
        }

        public void CopyTo(ITensorSegment2 segment)
        {
            segment.CopyFrom(_data);
        }

        public void CopyTo(Span<float> destination)
        {
            _data.CopyTo(destination);
        }

        public void Clear()
        {
            for(var i = 0; i < _data.Length; i++)
                _data[i] = 0f;
        }

        public override string ToString()
        {
            var preview = String.Join("|", Values.Take(8));
            if (Size > 8)
                preview += "|...";
            return $"{SegmentType} ({Size}): {preview}";
        }
    }
}
