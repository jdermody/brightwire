using System;
using System.IO;
using System.Linq;
using System.Numerics;
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
    public class ReadOnlyVector<T> : ReadOnlyTensorBase<T, IReadOnlyVector<T>>, IReadOnlyVector<T>, IEquatable<ReadOnlyVector<T>> 
        where T: unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        readonly ReadOnlyValueSemantics<ReadOnlyVector<T>, T> _valueSemantics;

        /// <summary>
        /// Creates a vector from memory
        /// </summary>
        /// <param name="data"></param>
        [OverloadResolutionPriority(1)]
        public ReadOnlyVector(ReadOnlyMemory<T> data) : base(new ReadOnlyTensorSegment<T>(data))
        {
            _valueSemantics = new(this);
        }

        /// <summary>
        /// Creates a vector from a span
        /// </summary>
        /// <param name="data"></param>
        public ReadOnlyVector(ReadOnlySpan<T> data) : base(new ReadOnlyTensorSegment<T>(data.ToArray()))
        {
            _valueSemantics = new(this);
        }

        /// <summary>
        /// Creates a vector from bytes
        /// </summary>
        /// <param name="data"></param>
        public ReadOnlyVector(ReadOnlySpan<byte> data) : this(data.Cast<byte, T>().ToArray())
        {
        }

        /// <summary>
        /// Creates an empty vector
        /// </summary>
        /// <param name="size"></param>
        public ReadOnlyVector(uint size) : this(new T[size])
        {
        }

        /// <summary>
        /// Creates a vector from the initializer
        /// </summary>
        /// <param name="size"></param>
        /// <param name="initializer"></param>
        public ReadOnlyVector(uint size, Func<uint, T> initializer) : this(size.AsRange().Select(initializer).ToArray())
        {
        }

        /// <summary>
        /// Creates a vector from a segment
        /// </summary>
        /// <param name="segment"></param>
        public ReadOnlyVector(IReadOnlyNumericSegment<T> segment) : base(segment)
        {
            _valueSemantics = new(this);
        }

        /// <inheritdoc />
        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write(1);
            writer.Write(Size);
            writer.Write(ReadOnlySegment);
        }

        /// <inheritdoc />
        public override void Initialize(BrightDataContext context, BinaryReader reader)
        {
            if (reader.ReadInt32() != 1)
                throw new ArgumentException("Unexpected array size");
            var size = reader.ReadUInt32();
            var data = reader.BaseStream.ReadArray<T>(size);
            ReadOnlySegment = new ReadOnlyTensorSegment<T>(data);
            Unsafe.AsRef(in _valueSemantics) = new(this);
        }

        /// <inheritdoc />
        public T this[int index] => ReadOnlySegment[index];

        /// <inheritdoc />
        public T this[uint index] => ReadOnlySegment[index];

        /// <inheritdoc />
        public IVector<T> Create(LinearAlgebraProvider<T> lap) => lap.CreateVector(ReadOnlySegment);

        /// <inheritdoc />
        public bool Equals(ReadOnlyVector<T>? other) => _valueSemantics.Equals(other);

        /// <inheritdoc />
        public override bool Equals(object? obj) => _valueSemantics.Equals(obj as ReadOnlyVector<T>);

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
        public override ReadOnlySpan<byte> DataAsBytes => ReadOnlySegment.AsBytes();

        /// <inheritdoc />
        protected override ReadOnlyVector<T> Create(MemoryOwner<T> memory)
        {
            try {
                return new ReadOnlyVector<T>(memory.Span.ToArray());
            }
            finally {
                memory.Dispose();
            }
        }
    }
}
