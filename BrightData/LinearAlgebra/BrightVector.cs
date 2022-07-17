using System;
using System.Linq;

namespace BrightData.LinearAlgebra
{
    public class BrightVector<LAP> : BrightTensorBase<IVector, LAP>, IVector
        where LAP: LinearAlgebraProvider
    {
        public BrightVector(ITensorSegment data, LAP lap) : base(data, lap)
        {
        }

        public uint Size => Segment.Size;
        public sealed override uint TotalSize
        {
            get => Segment.Size;
            protected set => throw new NotSupportedException();
        }

        public sealed override uint[] Shape
        {
            get => new[] { Size };
            protected set
            {
                if (value.Length != 1 && value[0] != Size)
                    throw new ArgumentException("Unexpected shape");
            }
        }

        public float this[int index]
        {
            get => Segment[index];
            set => Segment[index] = value;
        }
        public float this[uint index]
        {
            get => Segment[index];
            set => Segment[index] = value;
        }

        public float[] ToArray() => Segment.ToNewArray();
        IVector IReadOnlyVector.Create(LinearAlgebraProvider lap) => lap.CreateVector(ToArray());

        public float this[long index]
        {
            get => Segment[index];
            set => Segment[index] = value;
        }
        public float this[ulong index]
        {
            get => Segment[index];
            set => Segment[index] = value;
        }

        public override string ToString()
        {
            var preview = String.Join("|", Segment.Values.Take(Consts.PreviewSize));
            if (Size > Consts.PreviewSize)
                preview += "|...";
            return $"Vector ({Size}): {preview}";
        }

        public override IVector Create(ITensorSegment segment) => _lap.CreateVector(segment);

        public IVector MapIndexed(Func<uint, float, float> mutator)
        {
            var ret = _lap.MapParallel(Segment, mutator);
            return Create(ret);
        }

        public void MapIndexedInPlace(Func<uint, float, float> mutator)
        {
            var ret = _lap.MapParallel(Segment, mutator);
            try {
                ret.CopyTo(Segment);
            }
            finally {
                ret.Release();
            }
        }
    }

    public class BrightVector : BrightVector<LinearAlgebraProvider>
    {
        public BrightVector(ITensorSegment data, LinearAlgebraProvider computationUnit) : base(data, computationUnit)
        {
        }
    }
}
