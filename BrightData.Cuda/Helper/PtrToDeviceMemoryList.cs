using System;
using System.Linq;
using ManagedCuda;
using ManagedCuda.BasicTypes;

namespace BrightData.Cuda.Helper
{
    internal class PtrToDeviceMemoryList : IDisposable
    {
		readonly CudaDeviceVariable<CUdeviceptr> _ptr;

	    public PtrToDeviceMemoryList(IHaveDeviceMemory[] list)
	    {
		    _ptr = new CudaDeviceVariable<CUdeviceptr>(list.Length);
		    _ptr.CopyToDevice(list.Select(d => d.Memory.DevicePointer).ToArray());
	    }

	    //public PtrToDeviceMemoryList(CUdeviceptr[] ptrList)
	    //{
		   // _ptr = new CudaDeviceVariable<CUdeviceptr>(ptrList.Length);
		   // _ptr.CopyToDevice(ptrList);
	    //}

	    public CUdeviceptr DevicePointer => _ptr.DevicePointer;

	    public void Dispose()
	    {
		    _ptr.Dispose();
	    }
    }
}
