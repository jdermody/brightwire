using System;
using System.Linq;

namespace BrightData.LinearAlegbra2
{
    public class Vector2<CU> : TensorBase2<IVector, CU>, IVector
        where CU: ComputationUnit
    {
        public Vector2(ITensorSegment2 data, CU computationUnit) : base(data, computationUnit)
        {
        }

        public override uint Size => Segment.Size;

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

        public override IVector Create(ITensorSegment2 segment) => new Vector2<CU>(segment, _computationUnit);

        public IVector MapIndexed(Func<uint, float, float> mutator)
        {
            var ret = _computationUnit.MapParallel(Segment, mutator);
            return Create(ret);
        }

        public void MapIndexedInPlace(Func<uint, float, float> mutator)
        {
            var ret = _computationUnit.MapParallel(Segment, mutator);
            try {
                Segment.CopyFrom(ret.GetSpan());
            }
            finally {
                ret.Release();
            }
        }
    }

    public class Vector2 : Vector2<ComputationUnit>
    {
        public Vector2(ITensorSegment2 data, ComputationUnit computationUnit) : base(data, computationUnit)
        {
        }
    }
}
