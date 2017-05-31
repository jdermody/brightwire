using ManagedCuda;
using ManagedCuda.BasicTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BrightWire.CUDA.Helper
{
    /// <summary>
    /// Builds list of device pointers and pointers to those pointers
    /// </summary>
    internal class DeviceMemoryPtrList : IDisposable
    {
        readonly List<CudaDeviceVariable<CUdeviceptr>> _ptrList;
        readonly CudaDeviceVariable<CUdeviceptr> _ptrToPtrList;

        public DeviceMemoryPtrList(IReadOnlyList<IReadOnlyList<IDeviceMemoryPtr>> data)
        {
            _ptrList = data.Select(d => {
                var ptr = new CudaDeviceVariable<CUdeviceptr>(d.Count);
                ptr.CopyToDevice(d.Select(m => m.DevicePointer).ToArray());
                return ptr;
            }).ToList();
            _ptrToPtrList = new CudaDeviceVariable<CUdeviceptr>(data.Count);
            _ptrToPtrList.CopyToDevice(_ptrList.Select(m => m.DevicePointer).ToArray());
        }
        public void Dispose()
        {
            foreach (var item in _ptrList)
                item.Dispose();
            _ptrToPtrList.Dispose();
        }
        public CUdeviceptr DevicePointer => _ptrToPtrList.DevicePointer;
    }
}
