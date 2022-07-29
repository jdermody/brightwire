﻿using System;
using ManagedCuda;
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
    public interface IDeviceMemoryPtr : ICountReferences, IHaveSize, IDisposable
    {
        CudaDeviceVariable<float> DeviceVariable { get; }
        CUdeviceptr DevicePointer { get; }
        void CopyToDevice(float[] source);
        void CopyToDevice(IDeviceMemoryPtr source);
        void CopyToDevice(ReadOnlySpan<float> span, uint targetOffset);
        unsafe void CopyToDevice(float* ptr, uint sourceOffset, uint targetOffset, uint size);
        void CopyToHost(float[] target);
        void CopyToHost(ArraySegment<float> target);
        void Clear();
        IDeviceMemoryPtr Offset(uint offsetInFloats, uint size);
    }

    internal interface IHaveDeviceMemory
    {
        IDeviceMemoryPtr Memory { get; }
    }
}
