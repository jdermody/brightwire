using System;
using System.Linq;

namespace BrightData.LinearAlgebra
{
    /// <summary>
    /// Vector
    /// </summary>
    /// <typeparam name="LAP"></typeparam>
    public class BrightVector<LAP> : BrightTensorBase<IVector, LAP>, IVector, IReadOnlyVector
        where LAP: LinearAlgebraProvider
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Tensor segment</param>
        /// <param name="lap">Linear algebra provider</param>
        public BrightVector(INumericSegment<float> data, LAP lap) : base(data, lap)
        {
        }

        /// <inheritdoc />
        public uint Size => Segment.Size;

        /// <inheritdoc />
        public sealed override uint TotalSize
        {
            get => Segment.Size;
            protected set => throw new NotSupportedException();
        }

        /// <inheritdoc />
        public sealed override uint[] Shape
        {
            get => new[] { Size };
            protected set
            {
                if (value.Length != 1 && value[0] != Size)
                    throw new ArgumentException("Unexpected shape");
            }
        }

        /// <inheritdoc cref="IVector" />
        public float this[int index]
        {
            get => Segment[index];
            set => Segment[index] = value;
        }

        /// <inheritdoc />
        public float this[uint index]
        {
            get => Segment[index];
            set => Segment[index] = value;
        }

        /// <inheritdoc />
        public float this[long index]
        {
            get => Segment[index];
            set => Segment[index] = value;
        }

        /// <inheritdoc />
        public float this[ulong index]
        {
            get => Segment[index];
            set => Segment[index] = value;
        }

        /// <inheritdoc />
        public float[] ToArray() => Segment.ToNewArray();

        /// <inheritdoc />
        public IVector Clone(LinearAlgebraProvider? lap = null) => (lap ?? LinearAlgebraProvider).CreateVector(ToArray());

        /// <inheritdoc />
        public override string ToString()
        {
            var preview = String.Join("|", Segment.Values.Take(Consts.DefaultPreviewSize));
            if (Size > Consts.DefaultPreviewSize)
                preview += "|...";
            return $"Vector ({Size}): {preview}";
        }

        /// <inheritdoc />
        public override IVector Create(INumericSegment<float> segment) => Lap.CreateVector(segment);

        /// <inheritdoc />
        public IVector MapIndexed(Func<uint, float, float> mutator)
        {
            var ret = Lap.MapParallel(Segment, mutator);
            return Create(ret);
        }

        /// <inheritdoc />
        public void MapIndexedInPlace(Func<uint, float, float> mutator)
        {
            Lap.MapParallelInPlace(Segment, mutator);
        }

        /// <inheritdoc />
        public IVector Create(LinearAlgebraProvider lap) => lap.CreateVector((IReadOnlyVector)this);
    }

    /// <summary>
    /// Vector
    /// </summary>
    public class BrightVector : BrightVector<LinearAlgebraProvider>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Tensor segment</param>
        /// <param name="lap">Linear algebra provider</param>
        public BrightVector(INumericSegment<float> data, LinearAlgebraProvider lap) : base(data, lap)
        {
        }
    }
}
