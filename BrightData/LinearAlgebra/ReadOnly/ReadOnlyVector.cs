using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BrightData.LinearAlgebra.ReadOnlyTensorValueSemantics;
using BrightData.LinearAlgebra.Segments;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra.ReadOnly
{
    public class ReadOnlyVector : IReadOnlyVector, IEquatable<ReadOnlyVector>, IHaveReadOnlyContiguousSpan<float>, IHaveDataAsReadOnlyByteSpan
    {
        readonly ReadOnlyValueSemantics<ReadOnlyVector, float> _valueSemantics;
        ReadOnlyMemoryTensorSegment? _segment = null;
        ReadOnlyMemory<float> _data;

        public ReadOnlyVector(ReadOnlyMemory<float> data)
        {
            _data = data;
            _valueSemantics = new(this);
        }
        public ReadOnlyVector(ReadOnlySpan<byte> data) : this(data.Cast<byte, float>().ToArray())
        {
        }
        public ReadOnlyVector(uint size) : this(new float[size])
        {
        }
        public ReadOnlyVector(uint size, Func<uint, float> initializer) : this(size.AsRange().Select(initializer).ToArray())
        {
        }
        public ReadOnlyVector(IReadOnlyVector vector) : this(vector.ToArray())
        {
        }

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(1);
            writer.Write(Size);
            writer.Write(_data.Span.AsBytes());
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
            return _data.Span;
        }

        public uint Size => (uint)_data.Length;
        public float this[int index] => _data.Span[index];
        public float this[uint index] => _data.Span[(int)index];
        public IVector Create(LinearAlgebraProvider lap) => lap.CreateVector(_data.Span);
        public IReadOnlyNumericSegment<float> ReadOnlySegment => _segment ??= new ReadOnlyMemoryTensorSegment(_data);

        // value semantics
        public bool Equals(ReadOnlyVector? other) => _valueSemantics.Equals(other);
        public override bool Equals(object? obj) => _valueSemantics.Equals(obj as ReadOnlyVector);
        public override int GetHashCode() => _valueSemantics.GetHashCode();

        public override string ToString()
        {
            var preview = String.Join("|", Values.Take(Consts.DefaultPreviewSize));
            if (Size > Consts.DefaultPreviewSize)
                preview += "|...";
            return $"Vector Info ({Size}): {preview}";
        }

        public ReadOnlySpan<byte> DataAsBytes => ReadOnlySpan.Cast<float, byte>();
        public ReadOnlySpan<float> ReadOnlySpan => _data.Span;

        public IEnumerable<float> Values
        {
            get
            {
                for(var i = 0; i < _data.Length; i++)
                    yield return _data.Span[i];
            }
        }
    }
}
