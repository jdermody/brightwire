using BrightData.LinearAlgebra;

namespace BrightData.MKL
{
    /// <inheritdoc />
    public class MklVector : BrightVector<MklLinearAlgebraProvider>
    {
        /// <inheritdoc />
        public MklVector(INumericSegment<float> data, MklLinearAlgebraProvider computationUnit) : base(data, computationUnit)
        {
        }

        /// <inheritdoc />
        public override IVector Create(INumericSegment<float> segment) => new MklVector(segment, Lap);
    }
}