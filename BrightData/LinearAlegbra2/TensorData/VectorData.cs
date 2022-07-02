using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlegbra2.TensorData
{
    internal class VectorData : IVectorInfo
    {
        readonly TensorSegmentWrapper2 _segment;

        public VectorData(TensorSegmentWrapper2 segment)
        {
            _segment = segment;
        }

        public void WriteTo(BinaryWriter writer)
        {
            throw new NotImplementedException();
        }

        public void Initialize(BrightDataContext context, BinaryReader reader)
        {
            throw new NotImplementedException();
        }

        public ReadOnlySpan<float> GetSpan(ref SpanOwner<float> temp, out bool wasTempUsed) => _segment.GetSpan(ref temp, out wasTempUsed);
        public uint Size => _segment.Size;
        public float this[int index] => _segment[index];
        public float this[uint index] => _segment[index];
        public float[] ToArray() => _segment.ToNewArray();
        public IVector Create(LinearAlgebraProvider lap) => lap.CreateVector(_segment);
    }
}
