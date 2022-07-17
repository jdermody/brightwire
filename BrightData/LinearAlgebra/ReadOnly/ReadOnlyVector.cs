using System;
using System.IO;
using System.Linq;
using Microsoft.Toolkit.HighPerformance;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra.ReadOnly
{
    internal class ReadOnlyVector : IReadOnlyVector
    {
        float[]? _data = null;
        ITensorSegment? _segment = null;

        public ReadOnlyVector(float[] data)
        {
            _data = data;
        }
        public ReadOnlyVector(ITensorSegment? segment)
        {
            _segment = segment;
        }
        public ReadOnlyVector(uint size)
        {
            _data = new float[size];
        }
        public ReadOnlyVector(uint size, Func<uint, float> initializer)
        {
            _data = new float[size];
            for (uint i = 0; i < size; i++)
                _data[i] = initializer(i);
        }

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(1);
            writer.Write(Size);
            writer.Write((_data ?? _segment!.ToNewArray()).AsSpan().AsBytes());
        }

        public void Initialize(BrightDataContext context, BinaryReader reader)
        {
            if (reader.ReadInt32() != 1)
                throw new Exception("Unexpected array size");
            var size = reader.ReadUInt32();
            _data = reader.BaseStream.ReadArray<float>(size);
            _segment = null;
        }

        public ReadOnlySpan<float> GetSpan(ref SpanOwner<float> temp, out bool wasTempUsed)
        {
            if (_data is not null) {
                wasTempUsed = false;
                return _data.AsSpan();
            }
            return _segment!.GetSpan(ref temp, out wasTempUsed);
        }

        public uint Size => (uint?)_data?.Length ?? _segment!.Size;
        public float this[int index] => _data is null ? _segment![index] : _data[index];
        public float this[uint index] => _data is null ? _segment![index] : _data[index];
        public float[] ToArray() => _data ?? _segment!.ToNewArray();
        public IVector Create(LinearAlgebraProvider lap) => _data is null ? lap.CreateVector(_segment!) : lap.CreateVector(_data);
        public ITensorSegment Segment => _segment ??= new ArrayBasedTensorSegment(_data!);
        public override string ToString()
        {
            if (_data is not null) {
                var preview = String.Join("|", _data.Take(Consts.PreviewSize));
                if (Size > Consts.PreviewSize)
                    preview += "|...";
                return $"Vector Info ({Size}): {preview}";
            } 
            return $"Vector Info ({_segment})";
        }
    }
}
