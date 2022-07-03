using System;
using System.IO;
using BrightData.LinearAlegbra2;
using BrightData.Serialisation;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.DataTable2.TensorData
{
    internal class VectorData : IVectorInfo
    {
        ICanRandomlyAccessData<float> _data;
        uint _startIndex;

        internal VectorData(ICanRandomlyAccessData<float> data, uint startIndex, uint size)
        {
            _data = data;
            _startIndex = startIndex;
            Size = size;
        }

        public uint Size { get; }

        public ReadOnlySpan<float> GetSpan(ref SpanOwner<float> temp, out bool wasTempUsed)
        {
            wasTempUsed = false;
            return _data.GetSpan(_startIndex, Size);
        }

        public IVector Create(LinearAlgebraProvider lap)
        {
            var span = _data.GetSpan(_startIndex, Size);
            var segment = lap.CreateSegment(Size);
            segment.CopyFrom(span);
            return lap.CreateVector(segment);
        }

        public float this[int index] => _data[index];
        public float this[uint index] => _data[index];
        public float[] ToArray() => _data.GetSpan(_startIndex, Size).ToArray();

        public void Initialize(BrightDataContext context, BinaryReader reader)
        {
            throw new System.NotImplementedException();
        }

        public void WriteTo(BinaryWriter writer)
        {
            throw new System.NotImplementedException();
        }
    }
}
