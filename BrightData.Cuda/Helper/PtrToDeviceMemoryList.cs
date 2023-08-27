using System;
using System.Linq;
using BrightData.Cuda.CudaToolkit.Types;

namespace BrightData.Cuda.Helper
{
    internal class PtrToDeviceMemoryList : IDisposable
    {
		readonly CudaDeviceVariable<CuDevicePtr> _ptr;

	    public PtrToDeviceMemoryList(IHaveDeviceMemory[] list)
	    {
		    _ptr = new CudaDeviceVariable<CuDevicePtr>(list.Length);
		    _ptr.CopyToDevice(list.Select(d => d.Memory.DevicePointer).ToArray());
	    }

        public CuDevicePtr DevicePointer => _ptr.DevicePointer;

	    public void Dispose()
	    {
		    _ptr.Dispose();
	    }
    }
}
