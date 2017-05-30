using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManagedCuda;
using ManagedCuda.BasicTypes;

namespace BrightWire.CUDA.Helper
{
    class PtrToMemory : IDeviceMemoryPtr
    {
        readonly CudaDeviceVariable<float> _ptr;
        readonly CudaContext _context;

        public PtrToMemory(CudaContext context, CUdeviceptr ptr, SizeT size)
        {
            _context = context;
            _ptr = new CudaDeviceVariable<float>(ptr, size);
        }

        public CudaDeviceVariable<float> DeviceVariable => _ptr;
        public CUdeviceptr DevicePointer => _ptr.DevicePointer;
        public int Size => _ptr.Size;

        public void Clear()
        {
            _context.ClearMemory(_ptr.DevicePointer, 0, _ptr.SizeInBytes);
        }

        public void CopyToDevice(float[] source)
        {
            throw new Exception("Tried to update read only memory - try cloning the the object to create a new copy");
        }

        public void CopyToDevice(IDeviceMemoryPtr source)
        {
            throw new Exception("Tried to update read only memory - try cloning the the object to create a new copy");
        }

        public void CopyToHost(float[] target)
        {
            _context.CopyToHost<float>(target, _ptr.DevicePointer);
        }

        public void Free()
        {
            _ptr.Dispose();
        }
    }
}
