using ManagedCuda;
using ManagedCuda.BasicTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire
{
    internal interface IDeviceMemoryPtr
    {
        void Free();
        CudaDeviceVariable<float> DeviceVariable { get; }
        CUdeviceptr DevicePointer { get; }
        int Size { get; }
        void CopyToDevice(float[] source);
        void CopyToDevice(IDeviceMemoryPtr source);
        void CopyToHost(float[] target);
        void Clear();
    }
}
