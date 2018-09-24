using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ManagedCuda;
using ManagedCuda.BasicTypes;

namespace BrightWire.Cuda.Helper
{
    class PtrToDeviceMemoryList : IDisposable
    {
		readonly CudaDeviceVariable<CUdeviceptr> _ptr;

	    public PtrToDeviceMemoryList(IReadOnlyList<IHaveDeviceMemory> list)
	    {
		    _ptr = new CudaDeviceVariable<CUdeviceptr>(list.Count);
		    _ptr.CopyToDevice(list.Select(d => d.Memory.DevicePointer).ToArray());
	    }

	    public PtrToDeviceMemoryList(CUdeviceptr[] ptrList)
	    {
		    _ptr = new CudaDeviceVariable<CUdeviceptr>(ptrList.Length);
		    _ptr.CopyToDevice(ptrList);
	    }

	    public CUdeviceptr DevicePointer => _ptr.DevicePointer;

	    public void Dispose()
	    {
		    _ptr.Dispose();
	    }
    }
}
