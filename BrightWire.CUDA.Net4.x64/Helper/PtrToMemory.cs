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

        public PtrToMemory(CUdeviceptr ptr, SizeT size)
        {
            _ptr = new CudaDeviceVariable<float>(ptr, false, size);
        }

        public CudaDeviceVariable<float> DeviceVariable => _ptr;
        public CUdeviceptr DevicePointer => _ptr.DevicePointer;
        public int Size => _ptr.Size;

        public void Clear()
        {
            _ptr.Memset(0);
        }

        public void CopyToDevice(float[] source)
        {
            _ptr.CopyToDevice(source);
        }

        public void CopyToDevice(IDeviceMemoryPtr source)
        {
            _ptr.CopyToDevice(source.DeviceVariable);
        }

        public void CopyToHost(float[] target)
        {
            _ptr.CopyToDevice(target);
        }

        public void Free()
        {
            _ptr.Dispose();
        }
    }
}
