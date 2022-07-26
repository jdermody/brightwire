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

        public IDeviceMemoryPtr AllocateMasterBlock(uint size) => new StreamDeviceMemoryBlock(Stream, size, true);

        public void Dispose()
        {
            DriverAPINativeMethods.Streams.cuStreamSynchronize(Stream).CheckResult();
            DriverAPINativeMethods.Streams.cuStreamDestroy_v2(Stream).CheckResult();
        }
    }
}
