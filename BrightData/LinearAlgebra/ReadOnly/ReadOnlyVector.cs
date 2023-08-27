using System;
using System.IO;
using System.Linq;
using BrightData.LinearAlgebra.ReadOnlyTensorValueSemantics;
using BrightData.LinearAlgebra.Segments;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra.ReadOnly
{
    internal class ReadOnlyVector : IReadOnlyVector, IEquatable<ReadOnlyVector>
    {
        readonly ReadOnlyVectorValueSemantics<ReadOnlyVector> _valueSemantics;
        ArrayBasedTensorSegment? _segment = null;
        float[] _data;

        public ReadOnlyVector(float[] data)
        {
            _data = data;
            _valueSemantics = new(this);
        }
        public ReadOnlyVector(uint size) : this(new float[size])
        {
        }
        public ReadOnlyVector(uint size, Func<uint, float> initializer) : this(size)
        {
            for (uint i = 0; i < size; i++)
                _data[i] = initializer(i);
        }

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(1);
            writer.Write(Size);
            writer.Write((_data).AsSpan().AsBytes());
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
        public bool IsReadOnly => true;
        public float this[int index] => _data[index];
        public float this[uint index] => _data[index];
        public float[] ToArray() => _data;
        public IVector Create(LinearAlgebraProvider lap) => lap.CreateVector(_data);
        public IReadOnlyNumericSegment<float> ReadOnlySegment => _segment ??= new ArrayBasedTensorSegment(_data);

        // value semantics
        public bool Equals(ReadOnlyVector? other) => _valueSemantics.Equals(other);
        public override bool Equals(object? obj) => _valueSemantics.Equals(obj as ReadOnlyVector);
        public override int GetHashCode() => _valueSemantics.GetHashCode();

        public override string ToString()
        {
            var preview = String.Join("|", _data.Take(Consts.DefaultPreviewSize));
            if (Size > Consts.DefaultPreviewSize)
                preview += "|...";
            return $"Vector Info ({Size}): {preview}";
        }
    }
}
