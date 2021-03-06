﻿using ManagedCuda;
using ManagedCuda.BasicTypes;

namespace BrightData.Cuda
{
    /// <summary>
    /// Helper methods when using the GPU linear algebra provider
    /// </summary>
    public interface IGpuLinearAlgebraProvider
    {
        /// <summary>
        /// Binds the current thread to the cuda context (when using the same cuda provider from multiple threads)
        /// </summary>
        void BindThread();

        /// <summary>
        /// Amount of free memory on the device in bytes
        /// </summary>
        long FreeMemory { get; }

        /// <summary>
        /// Amount of total memory on the device in bytes
        /// </summary>
        long TotalMemory { get; }
    }

    /// <summary>
    /// Wrapper for a device memory pointer
    /// </summary>
    internal interface IDeviceMemoryPtr
    {
        int AddRef();
        void Free();
        CudaDeviceVariable<float> DeviceVariable { get; }
        CUdeviceptr DevicePointer { get; }
        uint Size { get; }
        void CopyToDevice(float[] source);
        void CopyToDevice(IDeviceMemoryPtr source);
        void CopyToHost(float[] target);
        void Clear();
    }

    internal interface IHaveDeviceMemory
    {
        IDeviceMemoryPtr Memory { get; }
    }
}
