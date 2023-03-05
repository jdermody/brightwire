using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.Cuda.CudaToolkit.Types
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct CuModule
    {
        public nint Pointer;
        public static CuModuleLoadingMode GetLoadingMode
        {
            get
            {
                var ret = new CuModuleLoadingMode();
                var res = DriverApiNativeMethods.ModuleManagement.cuModuleGetLoadingMode(ref ret);
                
                if (res != CuResult.Success) throw new CudaException(res);
                return ret;
            }
        }
    }
}
