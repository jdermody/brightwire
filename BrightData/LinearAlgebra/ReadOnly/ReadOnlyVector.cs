using System;
using System.IO;
using System.Linq;
using BrightData.LinearAlgebra.ReadOnlyTensorValueSemantics;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra.ReadOnly
{
    internal class ReadOnlyVector : IReadOnlyVector, IEquatable<ReadOnlyVector>
    {
        readonly ReadOnlyVectorValueSemantics<ReadOnlyVector> _valueSemantics;
        float[]? _data = null;
        ITensorSegment? _segment = null;

        ReadOnlyVector()
        {
            _valueSemantics = new(this);
        }
        public ReadOnlyVector(float[] data) : this()
        {
            _data = data;
        }
        public ReadOnlyVector(ITensorSegment? segment) : this()
        {
            _segment = segment;
        }
        public ReadOnlyVector(uint size) : this()
        {
            _data = new float[size];
        }
        public ReadOnlyVector(uint size, Func<uint, float> initializer) : this()
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

        public ReadOnlySpan<float> GetFloatSpan(ref SpanOwner<float> temp, out bool wasTempUsed)
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

        // value semantics
        public bool Equals(ReadOnlyVector? other) => _valueSemantics.Equals(other);
        public override bool Equals(object? obj) => _valueSemantics.Equals(obj as ReadOnlyVector);
        public override int GetHashCode() => _valueSemantics.GetHashCode();

        public override string ToString()
        {
            if (_data is not null) {
                var preview = String.Join("|", _data.Take(Consts.DefaultPreviewSize));
                if (Size > Consts.DefaultPreviewSize)
                    preview += "|...";
                return $"Vector Info ({Size}): {preview}";
            } 
            return $"Read Only Vector ({_segment})";
        }
    }
}
