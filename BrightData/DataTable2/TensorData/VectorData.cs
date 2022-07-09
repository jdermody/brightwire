using System;
using System.IO;
using BrightData.LinearAlegbra2;
using BrightData.Serialisation;
using Microsoft.Toolkit.HighPerformance;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.DataTable2.TensorData
{
    internal class VectorData : IVectorInfo
    {
        ICanRandomlyAccessData<float> _data;
        ITensorSegment2? _segment;
        uint _startIndex;
        uint _stride;

        internal VectorData(ICanRandomlyAccessData<float> data, uint startIndex, uint stride, uint size)
        {
            _data = data;
            _startIndex = startIndex;
            _stride = stride;
            Size = size;
        }

        public uint Size { get; private set; }

        public ReadOnlySpan<float> GetSpan(ref SpanOwner<float> temp, out bool wasTempUsed)
        {
            if (_stride == 1) {
                wasTempUsed = false;
                return _data.GetSpan(_startIndex, Size);
            }
            temp = SpanOwner<float>.Allocate((int)Size);
            wasTempUsed = true;
            var ret = temp.Span;
            for(var i = 0; i < Size; i++)
                ret[i] = this[i];
            return ret;
        }

        public IVector Create(LinearAlgebraProvider lap)
        {
            var span = _data.GetSpan(_startIndex, Size);
            var segment = lap.CreateSegment(Size);
            segment.CopyFrom(span);
            return lap.CreateVector(segment);
        }

        public ITensorSegment2 Segment => _segment ??= new ArrayBasedTensorSegment(this.ToArray());
        public float this[int index] => _data[(int)(_startIndex + index * _stride)];
        public float this[uint index] => _data[_startIndex + index * _stride];

        public void Initialize(BrightDataContext context, BinaryReader reader)
        {
            if (reader.ReadInt32() != 1)
                throw new Exception("Unexpected array size");
            Size = reader.ReadUInt32();
            _startIndex = 0;
            _data = new TempFloatData(reader, Size);
        }

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(1);
            writer.Write(Size);
            writer.Write(_data.GetSpan(_startIndex, Size).AsBytes());
        }
    }
}
