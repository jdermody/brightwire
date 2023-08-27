using System.Runtime.InteropServices;

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
