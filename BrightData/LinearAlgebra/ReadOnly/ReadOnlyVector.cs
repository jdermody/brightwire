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
    /// <summary>
    /// Read only vector
    /// </summary>
    public class ReadOnlyVector : IReadOnlyVector, IEquatable<ReadOnlyVector>, IHaveReadOnlyContiguousSpan<float>, IHaveDataAsReadOnlyByteSpan
    {
        ReadOnlyValueSemantics<ReadOnlyVector, float> _valueSemantics;
        ReadOnlyMemoryTensorSegment? _segment = null;
        ReadOnlyMemory<float> _data;

        /// <summary>
        /// Creates a vector from float memory
        /// </summary>
        /// <param name="data"></param>
        public ReadOnlyVector(ReadOnlyMemory<float> data)
        {
            _data = data;
            _valueSemantics = new(this);
        }

        /// <summary>
        /// Creates a vector from bytes
        /// </summary>
        /// <param name="data"></param>
        public ReadOnlyVector(ReadOnlySpan<byte> data) : this(data.Cast<byte, float>().ToArray())
        {
        }

        /// <summary>
        /// Creates an empty vector
        /// </summary>
        /// <param name="size"></param>
        public ReadOnlyVector(uint size) : this(new float[size])
        {
        }

        /// <summary>
        /// Creates a vector from the initializer
        /// </summary>
        /// <param name="size"></param>
        /// <param name="initializer"></param>
        public ReadOnlyVector(uint size, Func<uint, float> initializer) : this(size.AsRange().Select(initializer).ToArray())
        {
        }

        /// <inheritdoc />
        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(1);
            writer.Write(Size);
            writer.Write(_data.Span.AsBytes());
        }

        /// <inheritdoc />
        public void Initialize(BrightDataContext context, BinaryReader reader)
        {
            if (reader.ReadInt32() != 1)
                throw new Exception("Unexpected array size");
            var size = reader.ReadUInt32();
            _data = reader.BaseStream.ReadArray<float>(size);
            _valueSemantics = new(this);
        }

        /// <inheritdoc />
        public ReadOnlySpan<float> GetSpan(ref SpanOwner<float> temp, out bool wasTempUsed)
        {
            wasTempUsed = false;
            return _data.Span;
        }


        /// <inheritdoc />
        public uint Size => (uint)_data.Length;


        /// <inheritdoc />
        public float this[int index] => _data.Span[index];

        /// <inheritdoc />
        public float this[uint index] => _data.Span[(int)index];

        /// <inheritdoc />
        public IVector Create(LinearAlgebraProvider lap) => lap.CreateVector(_data.Span);

        /// <inheritdoc />
        public IReadOnlyNumericSegment<float> ReadOnlySegment => _segment ??= new ReadOnlyMemoryTensorSegment(_data);

        /// <inheritdoc />
        public bool Equals(ReadOnlyVector? other) => _valueSemantics.Equals(other);

        /// <inheritdoc />
        public override bool Equals(object? obj) => _valueSemantics.Equals(obj as ReadOnlyVector);

        /// <inheritdoc />
        public override int GetHashCode() => _valueSemantics.GetHashCode();

        /// <inheritdoc />
        public override string ToString()
        {
            var preview = String.Join("|", Values.Take(Consts.DefaultPreviewSize));
            if (Size > Consts.DefaultPreviewSize)
                preview += "|...";
            return $"Vector Info ({Size}): {preview}";
        }

        /// <inheritdoc />
        public ReadOnlySpan<byte> DataAsBytes => ReadOnlySpan.Cast<float, byte>();

        /// <inheritdoc />
        public ReadOnlySpan<float> ReadOnlySpan => _data.Span;

        /// <summary>
        /// Enumerates all values in the vector
        /// </summary>
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
