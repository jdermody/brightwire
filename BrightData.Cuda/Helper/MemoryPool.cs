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
    public class MemoryPool : IDisposable
    {
        readonly uint _maxSize;
        uint _currentSize = 0;
        ConcurrentDictionary<uint, ConcurrentStack<CUdeviceptr>> _memoryPool = new();

        public MemoryPool(uint maxSize)
        {
            _maxSize = maxSize;
        }

        public void Dispose()
        {
            foreach (var block in _memoryPool.Values) {
                foreach (var ptr in block)
                    DriverAPINativeMethods.MemoryManagement.cuMemFree_v2(ptr);
            }
        }

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

        public void Recycle(uint sizeInBytes, CUdeviceptr ptr)
        {
            GetBlock(sizeInBytes).Push(ptr);
        }

        ConcurrentStack<CUdeviceptr> GetBlock(uint sizeInBytes) => _memoryPool.AddOrUpdate(sizeInBytes, _ => new(), (_, existing) => existing);
    }
}
