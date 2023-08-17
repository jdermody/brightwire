using BrightData.LinearAlgebra;

namespace BrightData.Cuda
{
    /// <inheritdoc />
    public class CudaTensor4D : BrightTensor4D<CudaLinearAlgebraProvider>
    {
        /// <inheritdoc />
        public CudaTensor4D(INumericSegment<float> data, uint count, uint depth, uint rows, uint columns, CudaLinearAlgebraProvider lap) : base(data, count, depth, rows, columns, lap)
        {
        }
    }
}
