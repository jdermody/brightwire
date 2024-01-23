using BrightData.LinearAlgebra;

namespace BrightData.MKL
{
    internal class MklTensor3D(INumericSegment<float> data, uint depth, uint rowCount, uint columnCount, MklLinearAlgebraProvider lap)
        : MutableTensor3D<MklLinearAlgebraProvider>(data, depth, rowCount, columnCount, lap);
}
