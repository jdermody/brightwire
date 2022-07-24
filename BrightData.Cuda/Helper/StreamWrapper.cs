using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManagedCuda;
using ManagedCuda.BasicTypes;

namespace BrightData.Cuda.Helper
{
    unsafe struct StreamWrapper : IDisposable
    {
        public StreamWrapper()
        {
            var stream = new CUstream();
            DriverAPINativeMethods.Streams.cuStreamCreate(ref stream, CUStreamFlags.Default).CheckResult();
            Stream = stream;
        }
        public CUstream Stream { get; }

        public IDeviceMemoryPtr CopyResultAndDispose(IDeviceMemoryPtr block, CudaProvider provider)
        {
            var ret = provider.Allocate(block.Size);
            DriverAPINativeMethods.AsynchronousMemcpy_v2.cuMemcpyAsync(ret.DevicePointer, block.DevicePointer, block.Size * sizeof(float), Stream).CheckResult();
            DriverAPINativeMethods.Streams.cuStreamSynchronize(Stream).CheckResult();
            block.Dispose();
            return ret;
        }

        public void Dispose()
        {
            DriverAPINativeMethods.Streams.cuStreamDestroy_v2(Stream).CheckResult();
        }
    }
}
