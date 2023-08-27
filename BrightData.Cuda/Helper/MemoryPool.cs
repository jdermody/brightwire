using System;
using System.Collections.Concurrent;
using BrightData.Cuda.CudaToolkit;
using BrightData.Cuda.CudaToolkit.Types;

namespace BrightData.Cuda.Helper
{
    /// <summary>
    /// Maintains a memory pool for CUDA blocks
    /// </summary>
    public class MemoryPool : IDisposable
    {
        readonly ConcurrentDictionary<uint, ConcurrentStack<CuDevicePtr>> _memoryPool = new();

        /// <inheritdoc />
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            foreach (var block in _memoryPool.Values) {
                foreach (var ptr in block)
                    DriverApiNativeMethods.MemoryManagement.cuMemFree_v2(ptr);
            }
        }

        /// <summary>
        /// Gets a new or existing CUDA memory block
        /// </summary>
        /// <param name="sizeInBytes"></param>
        /// <returns></returns>
        /// <exception cref="CudaException"></exception>
        public CuDevicePtr GetPtr(uint sizeInBytes)
        {
            if (_memoryPool.TryGetValue(sizeInBytes, out var block) && block.TryPop(out var ret))
                return ret;

            ret = new CuDevicePtr();
            var status = DriverApiNativeMethods.MemoryManagement.cuMemAlloc_v2(ref ret, sizeInBytes);
            if (status != CuResult.Success)
                throw new CudaException(status);
            return ret;
        }

        /// <summary>
        /// Recycles a memory block (for reuse)
        /// </summary>
        /// <param name="sizeInBytes"></param>
        /// <param name="ptr"></param>
        public void Recycle(uint sizeInBytes, CuDevicePtr ptr) => GetBlock(sizeInBytes).Push(ptr);

        ConcurrentStack<CuDevicePtr> GetBlock(uint sizeInBytes) => _memoryPool.AddOrUpdate(sizeInBytes, _ => new(), (_, existing) => existing);
    }
}
