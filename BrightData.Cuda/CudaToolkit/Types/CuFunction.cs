using System.Runtime.InteropServices;

namespace BrightData.Cuda.CudaToolkit.Types
{
    [StructLayout(LayoutKind.Sequential)]
    internal record struct CuFunction
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
