using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.Cuda.CudaToolkit.Types
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct CuFunction
    {
        public nint Pointer;
        public CuModule GetModule()
        {
            var temp = new CuModule();
            var res = DriverApiNativeMethods.FunctionManagement.cuFuncGetModule(ref temp, this);
            
            if (res != CuResult.Success) throw new CudaException(res);
            return temp;
        }
    }
}
