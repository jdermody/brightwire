using BrightData.LinearAlgebra;

namespace BrightData.MKL
{
    internal class MklTensor4D(INumericSegment<float> data, uint count, uint depth, uint rows, uint columns, MklLinearAlgebraProvider lap)
        : MutableTensor4D<MklLinearAlgebraProvider>(data, count, depth, rows, columns, lap);
}
