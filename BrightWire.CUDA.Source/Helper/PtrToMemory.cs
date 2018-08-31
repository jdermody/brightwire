using System;
using ManagedCuda;
using ManagedCuda.BasicTypes;

namespace BrightWire.Cuda.Helper
{
    /// <summary>
    /// A pointer to a block of device memory (the block is owned by another pointer)
    /// </summary>
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
            throw new NotImplementedException();
        }

        public void CopyToDevice(IDeviceMemoryPtr source)
        {
            throw new NotImplementedException();
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
