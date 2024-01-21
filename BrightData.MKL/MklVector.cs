using BrightData.LinearAlgebra;

namespace BrightData.MKL
{
    /// <inheritdoc />
    /// <inheritdoc />
    public class MklVector(INumericSegment<float> data, MklLinearAlgebraProvider computationUnit)
        : BrightVector<MklLinearAlgebraProvider>(data, computationUnit)
    {

        /// <inheritdoc />
        public override IVector Create(INumericSegment<float> segment) => new MklVector(segment, Lap);
    }
}