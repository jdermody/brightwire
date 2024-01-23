﻿using BrightData.LinearAlgebra;

namespace BrightData.Cuda
{
    internal class CudaVector(INumericSegment<float> data, CudaLinearAlgebraProvider lap) : MutableVector<CudaLinearAlgebraProvider>(data, lap), IHaveDeviceMemory
    {
        public IDeviceMemoryPtr Memory => CudaLinearAlgebraProvider.GetDeviceMemoryPtr(Segment);
    }
}
