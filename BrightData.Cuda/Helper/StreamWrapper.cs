using System;
using BrightData.Cuda.CudaToolkit;
using BrightData.Cuda.CudaToolkit.Types;

namespace BrightData.Cuda.Helper
{
    readonly struct StreamWrapper : IDisposable
    {
        public StreamWrapper()
        {
            var stream = new CuStream();
            DriverApiNativeMethods.Streams.cuStreamCreate(ref stream, CuStreamFlags.Default).CheckResult();
            Stream = stream;
        }
        public CuStream Stream { get; }

        public IDeviceMemoryPtr AllocateMasterBlock(uint size) => new StreamDeviceMemoryBlock(Stream, size, true);

        public void Dispose()
        {
            DriverApiNativeMethods.Streams.cuStreamSynchronize(Stream).CheckResult();
            DriverApiNativeMethods.Streams.cuStreamDestroy_v2(Stream).CheckResult();
        }
    }
}
