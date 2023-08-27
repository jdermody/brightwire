using System;
using System.IO;

namespace BrightData.DataTable.TensorData
{
    internal class TempFloatData : ICanRandomlyAccessUnmanagedData<float>
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

        public uint Size => (uint)_data.Length;
        public void Get(int index, out float value) => value = _data[index];
        public void Get(uint index, out float value) => value = _data[index];

        public ReadOnlySpan<float> GetSpan(uint startIndex, uint count) => _data.AsSpan((int)startIndex, (int)count);
        public override string ToString() => _data.ToString()!;
    }
}
