using BrightData.LinearAlgebra;

namespace BrightData.Cuda
{
    /// <inheritdoc />
    /// <inheritdoc />
    public class CudaTensor3D(INumericSegment<float> data, uint depth, uint rowCount, uint columnCount, CudaLinearAlgebraProvider lap)
        : BrightTensor3D<CudaLinearAlgebraProvider>(data, depth, rowCount, columnCount, lap)
    ;
}
