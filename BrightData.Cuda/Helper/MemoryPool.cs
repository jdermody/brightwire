using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManagedCuda;
using ManagedCuda.BasicTypes;

namespace BrightData.Cuda.Helper
{
    /// <summary>
    /// Maintains a memory pool for CUDA blocks
    /// </summary>
    public class MemoryPool : IDisposable
    {
        readonly ConcurrentDictionary<uint, ConcurrentStack<CUdeviceptr>> _memoryPool = new();

        /// <inheritdoc />
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            foreach (var block in _memoryPool.Values) {
                foreach (var ptr in block)
                    DriverAPINativeMethods.MemoryManagement.cuMemFree_v2(ptr);
            }
        }

        /// <summary>
        /// Gets a new or existing CUDA memory block
        /// </summary>
        /// <param name="sizeInBytes"></param>
        /// <returns></returns>
        /// <exception cref="CudaException"></exception>
        public CUdeviceptr GetPtr(uint sizeInBytes)
        {
            CUdeviceptr ret;
            if (_memoryPool.TryGetValue(sizeInBytes, out var block)) {
                if (block.TryPop(out ret))
                    return ret;
            }

            ret = new CUdeviceptr();
            var status = DriverAPINativeMethods.MemoryManagement.cuMemAlloc_v2(ref ret, sizeInBytes);
            if (status != CUResult.Success)
                throw new CudaException(status);
            return ret;
        }

        /// <summary>
        /// Recycles a memory block (for reuse)
        /// </summary>
        /// <param name="sizeInBytes"></param>
        /// <param name="ptr"></param>
        public void Recycle(uint sizeInBytes, CUdeviceptr ptr) => GetBlock(sizeInBytes).Push(ptr);

        ConcurrentStack<CUdeviceptr> GetBlock(uint sizeInBytes) => _memoryPool.AddOrUpdate(sizeInBytes, _ => new(), (_, existing) => existing);
    }
}
