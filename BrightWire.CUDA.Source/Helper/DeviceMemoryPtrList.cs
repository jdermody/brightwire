using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ManagedCuda;
using ManagedCuda.BasicTypes;

namespace BrightWire.CUDA.Source.Helper
{
    class DeviceMemoryPtrList : IDisposable
    {
	    readonly CudaDeviceVariable<CUdeviceptr> _ptr;

	    public DeviceMemoryPtrList(IReadOnlyList<IDeviceMemoryPtr> data)
	    {
		    _ptr = new CudaDeviceVariable<CUdeviceptr>(data.Count);
		    _ptr.CopyToDevice(data.Select(m => m.DevicePointer).ToArray());
	    }
	    public void Dispose()
	    {
		    _ptr.Dispose();
	    }
	    public CUdeviceptr DevicePointer => _ptr.DevicePointer;
    }
}
