using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using BrightData.LinearAlgebra.ReadOnlyTensorValueSemantics;
using BrightData.LinearAlgebra.Segments;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra.ReadOnly
{
    /// <summary>
    /// Read only vector
    /// </summary>
    public class ReadOnlyVector : IReadOnlyVector, IEquatable<ReadOnlyVector>, IHaveDataAsReadOnlyByteSpan
    {
        readonly ReadOnlyValueSemantics<ReadOnlyVector, float> _valueSemantics;

        /// <summary>
        /// Creates a vector from float memory
        /// </summary>
        /// <param name="data"></param>
        public ReadOnlyVector(ReadOnlyMemory<float> data)
        {
            ReadOnlySegment = new ReadOnlyTensorSegment(data);
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

        /// <summary>
        /// Creates a vector from a segment
        /// </summary>
        /// <param name="segment"></param>
        public ReadOnlyVector(IReadOnlyNumericSegment<float> segment)
        {
            ReadOnlySegment = segment;
            _valueSemantics = new(this);
        }

        /// <inheritdoc />
        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(1);
            writer.Write(Size);
            writer.Write(ReadOnlySegment);
        }

        /// <inheritdoc />
        public void Initialize(BrightDataContext context, BinaryReader reader)
        {
            if (reader.ReadInt32() != 1)
                throw new Exception("Unexpected array size");
            var size = reader.ReadUInt32();
            var data = reader.BaseStream.ReadArray<float>(size);
            ReadOnlySegment = new ReadOnlyTensorSegment(data);
            Unsafe.AsRef(in _valueSemantics) = new(this);
        }

        /// <inheritdoc />
        public ReadOnlySpan<float> GetSpan(ref SpanOwner<float> temp, out bool wasTempUsed) => ReadOnlySegment.GetSpan(ref temp, out wasTempUsed);

        /// <inheritdoc />
        public uint Size => ReadOnlySegment.Size;

        /// <inheritdoc />
        public float this[int index] => ReadOnlySegment[index];

        /// <inheritdoc />
        public float this[uint index] => ReadOnlySegment[index];

        /// <inheritdoc />
        public IVector Create(LinearAlgebraProvider lap) => lap.CreateVector(ReadOnlySegment);

        /// <inheritdoc />
        public IReadOnlyNumericSegment<float> ReadOnlySegment { get; private set; }

        /// <inheritdoc />
        public bool Equals(ReadOnlyVector? other) => _valueSemantics.Equals(other);

        /// <inheritdoc />
        public override bool Equals(object? obj) => _valueSemantics.Equals(obj as ReadOnlyVector);

        /// <inheritdoc />
        public override int GetHashCode() => _valueSemantics.GetHashCode();

        /// <inheritdoc />
        public override string ToString()
        {
            var preview = String.Join("|", ReadOnlySegment.Values.Take(Consts.DefaultPreviewSize));
            if (Size > Consts.DefaultPreviewSize)
                preview += "|...";
            return $"Read Only Vector ({Size}): {preview}";
        }

        /// <inheritdoc />
        public ReadOnlySpan<byte> DataAsBytes => ReadOnlySegment.AsBytes();
    }
}
