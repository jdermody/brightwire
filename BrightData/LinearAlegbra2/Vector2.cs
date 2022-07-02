using System;
using System.Linq;

namespace BrightData.LinearAlegbra2
{
    public class Vector2<CU> : TensorBase2<IVector, CU>, IVector
        where CU: LinearAlgebraProvider
    {
        public Vector2(ITensorSegment2 data, CU computationUnit) : base(data, computationUnit)
        {
        }

        public sealed override uint Size
        {
            get => Segment.Size;
            protected set => throw new NotSupportedException();
        }

        public override uint[] Shape
        {
            get => new[] { Size };
            protected set { /*nop*/ }
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

        public override IVector Create(ITensorSegment2 segment) => new Vector2<CU>(segment, _lap);

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
