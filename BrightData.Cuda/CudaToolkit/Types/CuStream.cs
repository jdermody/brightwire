using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

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
                var s = new CuStream();
                s.Pointer = 0;
                return s;
            }
        }
        public static CuStream LegacyStream
        {
            get
            {
                var s = new CuStream();
                s.Pointer = 1;
                return s;
            }
        }
        public static CuStream StreamPerThread
        {
            get
            {
                var s = new CuStream();
                s.Pointer = 2;
                return s;
            }
        }
        public ulong Id
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
