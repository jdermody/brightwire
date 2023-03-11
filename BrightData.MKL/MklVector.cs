using BrightData.LinearAlgebra;

namespace BrightData.MKL
{
    /// <inheritdoc />
    public class MklVector : BrightVector<MklLinearAlgebraProvider>
    {
        /// <inheritdoc />
        public MklVector(ITensorSegment data, MklLinearAlgebraProvider computationUnit) : base(data, computationUnit)
        {
        }

        /// <inheritdoc />
        public override IVector Create(ITensorSegment segment) => new MklVector(segment, Lap);
    }
}