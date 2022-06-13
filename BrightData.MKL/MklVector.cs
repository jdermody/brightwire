using BrightData.LinearAlegbra2;
using BrightData.LinearAlgebra;
using MKLNET;

namespace BrightData.MKL
{
    public class MklVector : Vector2<MklComputationUnit>
    {
        public MklVector(ITensorSegment2 data, MklComputationUnit computationUnit) : base(data, computationUnit)
        {
        }

        public override IVector Create(ITensorSegment2 segment) => new MklVector(segment, _computationUnit);
    }
}