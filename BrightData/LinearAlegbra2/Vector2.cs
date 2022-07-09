using System;
using System.Linq;

namespace BrightData.LinearAlegbra2
{
    public class Vector2<LAP> : TensorBase2<IVector, LAP>, IVector
        where LAP: LinearAlgebraProvider
    {
        public Vector2(ITensorSegment2 data, LAP lap) : base(data, lap)
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
        IVector IVectorInfo.Create(LinearAlgebraProvider lap) => lap.CreateVector(ToArray());

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
            var preview = String.Join("|", Segment.Values.Take(8));
            if (Size > 8)
                preview += "|...";
            return $"Vector ({Size}): {preview}";
        }

        public override IVector Create(ITensorSegment2 segment) => new Vector2<LAP>(segment, _lap);

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

    public class Vector2 : Vector2<LinearAlgebraProvider>
    {
        public Vector2(ITensorSegment2 data, LinearAlgebraProvider computationUnit) : base(data, computationUnit)
        {
        }
    }

    public class ArrayBasedVector : Vector2<ArrayBasedLinearAlgebraProvider>
    {
        public ArrayBasedVector(ITensorSegment2 data, ArrayBasedLinearAlgebraProvider computationUnit) : base(data, computationUnit)
        {
        }
    }
}
