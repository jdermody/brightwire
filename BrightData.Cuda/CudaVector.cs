using BrightData.LinearAlgebra;

namespace BrightData.Cuda
{
    internal class CudaVector(INumericSegment<float> data, CudaLinearAlgebraProvider lap) : MutableVector<float, CudaLinearAlgebraProvider>(data, lap), IHaveDeviceMemory
    {
        public IDeviceMemoryPtr Memory => Segment.GetDeviceMemoryPtr();
    }
}
