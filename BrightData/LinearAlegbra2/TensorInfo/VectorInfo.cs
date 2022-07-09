using System;
using System.IO;
using Microsoft.Toolkit.HighPerformance;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlegbra2.TensorInfo
{
    internal class VectorInfo : IVectorInfo
    {
        float[] _data;
        ITensorSegment2? _segment;

        public VectorInfo(float[] data)
        {
            _data = data;
        }
        public VectorInfo(uint size)
        {
            _data = new float[size];
        }
        public VectorInfo(uint size, Func<uint, float> initializer)
        {
            _data = new float[size];
            for (uint i = 0; i < size; i++)
                _data[i] = initializer(i);
        }

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(1);
            writer.Write(Size);
            writer.Write(_data.AsSpan().AsBytes());
        }

        public void Initialize(BrightDataContext context, BinaryReader reader)
        {
            if (reader.ReadInt32() != 1)
                throw new Exception("Unexpected array size");
            var size = reader.ReadUInt32();
            _data = reader.BaseStream.ReadArray<float>(size);
        }

        public ReadOnlySpan<float> GetSpan(ref SpanOwner<float> temp, out bool wasTempUsed)
        {
            wasTempUsed = false;
            return _data.AsSpan();
        }

        public uint Size => (uint)_data.Length;
        public float this[int index] => _data[index];
        public float this[uint index] => _data[index];
        public float[] ToArray() => _data;
        public IVector Create(LinearAlgebraProvider lap) => lap.CreateVector(_data);
        public ITensorSegment2 Segment => _segment ??= new ArrayBasedTensorSegment(_data);
    }
}
