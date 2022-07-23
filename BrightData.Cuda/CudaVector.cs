using BrightData.LinearAlgebra;

namespace BrightData.Cuda
{
    internal class CudaVector : BrightVector<CudaLinearAlgebraProvider>, IHaveDeviceMemory
    {
        public CudaVector(ITensorSegment data, CudaLinearAlgebraProvider lap) : base(data, lap)
        {
        }

        public IDeviceMemoryPtr Memory => CudaLinearAlgebraProvider.GetDeviceMemoryPtr(Segment);
    }
}
