using BrightData.LinearAlgebra;

namespace BrightData.Cuda
{
    /// <inheritdoc />
    /// <inheritdoc />
    public class CudaTensor3D(INumericSegment<float> data, uint depth, uint rowCount, uint columnCount, CudaLinearAlgebraProvider lap)
        : MutableTensor3D<CudaLinearAlgebraProvider>(data, depth, rowCount, columnCount, lap)
    {
        /// <summary>
        /// Associated CUDA provider
        /// </summary>
        public CudaProvider Provider = lap.Provider;

        /// <inheritdoc />
        public override IMatrix GetMatrix(uint index)
        {
            var ptr = CudaProvider.OffsetByBlock(Segment.GetDeviceMemoryPtr(), index, MatrixSize);
            return Lap.CreateMatrix(RowCount, ColumnCount, Lap.CreateCudaTensorSegment(ptr));
        }
    }
}
