using BrightData.LinearAlgebra;

namespace BrightData.Cuda
{
    /// <inheritdoc />
    /// <inheritdoc />
    public class CudaTensor4D(INumericSegment<float> data, uint count, uint depth, uint rows, uint columns, CudaLinearAlgebraProvider lap)
        : BrightTensor4D<CudaLinearAlgebraProvider>(data, count, depth, rows, columns, lap)
    ;
}
