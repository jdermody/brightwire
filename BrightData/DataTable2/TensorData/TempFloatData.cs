using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.DataTable2.TensorData
{
    internal class TempFloatData : ICanRandomlyAccessData<float>
    {
        readonly float[] _data;

        public TempFloatData(BinaryReader reader, uint size)
        {
            _data = reader.BaseStream.ReadArray<float>(size);
        }

        public void Dispose()
        {
            // nop
        }

        public float this[int index] => _data[index];
        public float this[uint index] => _data[index];
        public ReadOnlySpan<float> GetSpan(uint startIndex, uint count) => _data.AsSpan((int)startIndex, (int)count);
    }
}
