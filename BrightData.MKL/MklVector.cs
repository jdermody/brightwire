using BrightData.LinearAlgebra;
using MKLNET;

namespace BrightData.MKL
{
    public class MklVector : BrightVector<MklLinearAlgebraProvider>
    {
        public MklVector(ITensorSegment data, MklLinearAlgebraProvider computationUnit) : base(data, computationUnit)
        {
        }

        public override IVector Create(ITensorSegment segment) => new MklVector(segment, Lap);
    }
}