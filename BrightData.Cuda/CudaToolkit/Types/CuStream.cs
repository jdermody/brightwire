using System.Runtime.InteropServices;

namespace BrightData.Cuda.CudaToolkit.Types
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct CuStream
    {
        public nint Pointer;
        public static CuStream NullStream
        {
            get
            {
                var s = new CuStream {
                    Pointer = 0
                };
                return s;
            }
        }
        public static CuStream LegacyStream
        {
            get
            {
                var s = new CuStream {
                    Pointer = 1
                };
                return s;
            }
        }
        public static CuStream StreamPerThread
        {
            get
            {
                var s = new CuStream {
                    Pointer = 2
                };
                return s;
            }
        }
        public readonly ulong Id
        {
            get
            {
                ulong ret = 0;
                var res = DriverApiNativeMethods.Streams.cuStreamGetId(this, ref ret);
                
                if (res != CuResult.Success) throw new CudaException(res);
                return ret;
            }
        }
    }
}
