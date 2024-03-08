using System;
using System.Linq;
using System.Numerics;
using BrightData.LinearAlgebra.Segments;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra
{
    /// <summary>
    /// Mutable vector
    /// </summary>
    /// <typeparam name="LAP">Linear algebra provider</typeparam>
    /// <typeparam name="T">Data type</typeparam>
    /// <param name="data">Tensor segment</param>
    /// <param name="lap">Linear algebra provider</param>
    public class MutableVector<T, LAP>(INumericSegment<T> data, LAP lap) : MutableTensorBase<T, IReadOnlyVector<T>, IVector<T>, LAP>(data, lap), IVector<T>
        where T: unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
        where LAP: LinearAlgebraProvider<T>
    {
        /// <inheritdoc />
        public sealed override uint TotalSize
        {
            get => Segment.Size;
            protected set => throw new NotSupportedException();
        }

        /// <inheritdoc />
        public sealed override uint[] Shape
        {
            get => [Size];
            protected set
            {
                if (value.Length != 1 && value[0] != Size)
                    throw new ArgumentException("Unexpected shape");
            }
        }

        /// <inheritdoc cref="IVector{T}" />
        public T this[int index]
        {
            get => Segment[index];
            set => Segment[index] = value;
        }

        /// <inheritdoc cref="IVector{T}" />
        public T this[uint index]
        {
            get => Segment[index];
            set => Segment[index] = value;
        }

        /// <inheritdoc />
        public T this[long index]
        {
            get => Segment[index];
            set => Segment[index] = value;
        }

        /// <inheritdoc />
        public T this[ulong index]
        {
            get => Segment[index];
            set => Segment[index] = value;
        }

        /// <inheritdoc />
        public override ReadOnlySpan<byte> DataAsBytes => Segment.AsBytes();

        /// <inheritdoc />
        protected override IReadOnlyVector<T> Create(MemoryOwner<T> memory) => new MutableVector<T, LAP>(new ArrayPoolTensorSegment<T>(memory), Lap);

        /// <inheritdoc />
        public override string ToString()
        {
            var preview = String.Join("|", Segment.Values.Take(Consts.DefaultPreviewSize));
            if (Size > Consts.DefaultPreviewSize)
                preview += "|...";
            return $"Vector ({Size}): {preview}";
        }

        /// <inheritdoc />
        public override IVector<T> Create(INumericSegment<T> segment) => Lap.CreateVector(segment);

        /// <inheritdoc />
        public IVector<T> MapIndexed(Func<uint, T, T> mutator)
        {
            var ret = Segment.MapParallel(mutator);
            return Create(ret);
        }

        /// <inheritdoc />
        public void MapIndexedInPlace(Func<uint, T, T> mutator)
        {
            Segment.MapParallelInPlace(mutator);
        }

        /// <inheritdoc />
        public IVector<T> Create(LinearAlgebraProvider<T> lap) => lap.CreateVector((IReadOnlyVector<T>)this);
    }
}
