using System;
using BrightData.Cuda.CudaToolkit;

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
    internal interface IDeviceMemoryPtr : ICountReferences, IHaveSize, IDisposable
    {
        /// <summary>
        /// CUDA device variable - float array
        /// </summary>
        CudaDeviceVariable<float> DeviceVariable { get; }

        /// <summary>
        /// CUDA device pointer
        /// </summary>
        CUdeviceptr DevicePointer { get; }

        /// <summary>
        /// Copies from the array to the device (in this block)
        /// </summary>
        /// <param name="source"></param>
        void CopyToDevice(float[] source);

        /// <summary>
        /// Copies from an existing block to the device (in this block)
        /// </summary>
        /// <param name="source"></param>
        void CopyToDevice(IDeviceMemoryPtr source);

        /// <summary>
        /// Copies from the span to the device (in this block)
        /// </summary>
        /// <param name="span"></param>
        /// <param name="targetOffset">Offset in this block to copy to</param>
        void CopyToDevice(ReadOnlySpan<float> span, uint targetOffset);

        /// <summary>
        /// Copies from a pointer to the device (in this block)
        /// </summary>
        /// <param name="ptr">Pointer to float buffer</param>
        /// <param name="sourceOffset">Offset from pointer to copy from</param>
        /// <param name="targetOffset">Offset in this block to copy to</param>
        /// <param name="sizeInFloats">Number of floats to copy to the device</param>
        unsafe void CopyToDevice(float* ptr, uint sourceOffset, uint targetOffset, uint sizeInFloats);

        /// <summary>
        /// Copies from the device to an array
        /// </summary>
        /// <param name="target">Array to copy to</param>
        void CopyToHost(float[] target);

        /// <summary>
        /// Copies from the device to an array segment
        /// </summary>
        /// <param name="target">Array segment to copy to</param>
        void CopyToHost(ArraySegment<float> target);

        /// <summary>
        /// Sets each value in the block to zero
        /// </summary>
        void Clear();

        /// <summary>
        /// Gets a new pointer, offset from this block
        /// </summary>
        /// <param name="offsetInFloats">Offset to return</param>
        /// <param name="size">Size of the block</param>
        /// <returns></returns>
        IDeviceMemoryPtr Offset(uint offsetInFloats, uint size);
    }

    internal interface IHaveDeviceMemory
    {
        IDeviceMemoryPtr Memory { get; }
    }
}
