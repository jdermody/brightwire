using System;
using System.Linq;

namespace BrightData.LinearAlgebra
{
    /// <summary>
    /// Vector
    /// </summary>
    /// <typeparam name="LAP"></typeparam>
    public class BrightVector<LAP> : BrightTensorBase<IVector, LAP>, IVector
        where LAP: LinearAlgebraProvider
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Tensor segment</param>
        /// <param name="lap">Linear algebra provider</param>
        public BrightVector(ITensorSegment data, LAP lap) : base(data, lap)
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

        /// <summary>
        /// Returns a value from the vector
        /// </summary>
        /// <param name="index">Index to return</param>
        /// <returns></returns>
        public float this[int index]
        {
            get => Segment[index];
            set => Segment[index] = value;
        }

        /// <summary>
        /// Returns a value from the vector
        /// </summary>
        /// <param name="index">Index to return</param>
        /// <returns></returns>
        public float this[uint index]
        {
            get => Segment[index];
            set => Segment[index] = value;
        }

        /// <summary>
        /// Returns a value from the vector
        /// </summary>
        /// <param name="index">Index to return</param>
        /// <returns></returns>
        public float this[long index]
        {
            get => Segment[index];
            set => Segment[index] = value;
        }

        /// <summary>
        /// Returns a value from the vector
        /// </summary>
        /// <param name="index">Index to return</param>
        /// <returns></returns>
        public float this[ulong index]
        {
            get => Segment[index];
            set => Segment[index] = value;
        }

        /// <inheritdoc />
        public float[] ToArray() => Segment.ToNewArray();
        IVector IReadOnlyVector.Create(LinearAlgebraProvider lap) => lap.CreateVector(ToArray());

        /// <inheritdoc />
        public override string ToString()
        {
            var preview = String.Join("|", Segment.Values.Take(Consts.DefaultPreviewSize));
            if (Size > Consts.DefaultPreviewSize)
                preview += "|...";
            return $"Vector ({Size}): {preview}";
        }

        /// <inheritdoc />
        public override IVector Create(ITensorSegment segment) => Lap.CreateVector(segment);

        /// <inheritdoc />
        public IVector MapIndexed(Func<uint, float, float> mutator)
        {
            var ret = Lap.MapParallel(Segment, mutator);
            return Create(ret);
        }

        /// <inheritdoc />
        public void MapIndexedInPlace(Func<uint, float, float> mutator)
        {
            var ret = Lap.MapParallel(Segment, mutator);
            try {
                ret.CopyTo(Segment);
            }
            finally {
                ret.Release();
            }
        }
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
        public BrightVector(ITensorSegment data, LinearAlgebraProvider lap) : base(data, lap)
        {
        }
    }
}
