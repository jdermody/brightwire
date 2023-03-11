using System;
using System.Runtime.InteropServices;

namespace BrightData.Cuda.CudaToolkit.Types
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct CuKernel
    {
        public nint Pointer;
        public static explicit operator CuFunction(CuKernel cuKernel)
        {
            var ret = new CuFunction {
                Pointer = cuKernel.Pointer
            };
            return ret;
        }
        public CuFunction GetCuFunction()
        {
            var ret = new CuFunction();
            var res = DriverApiNativeMethods.LibraryManagement.cuKernelGetFunction(ref ret, this);
            if (res != CuResult.Success) 
                throw new CudaException(res);
            return ret;
        }
        public int GetMaxThreadsPerBlock(CuDevice device)
        {
            var temp = 0;
            var res = DriverApiNativeMethods.LibraryManagement.cuKernelGetAttribute(ref temp, CuFunctionAttribute.MaxThreadsPerBlock, this, device);
            if (res != CuResult.Success) 
                throw new CudaException(res);
            return temp;
        }
        public int GetSharedMemory(CuDevice device)
        {
            var temp = 0;
            var res = DriverApiNativeMethods.LibraryManagement.cuKernelGetAttribute(ref temp, CuFunctionAttribute.SharedSizeBytes, this, device);
            if (res != CuResult.Success) 
                throw new CudaException(res);
            return temp;
        }
        public int GetConstMemory(CuDevice device)
        {
            var temp = 0;
            var res = DriverApiNativeMethods.LibraryManagement.cuKernelGetAttribute(ref temp, CuFunctionAttribute.ConstSizeBytes, this, device);
            if (res != CuResult.Success) 
                throw new CudaException(res);
            return temp;
        }
        public int GetLocalMemory(CuDevice device)
        {
            var temp = 0;
            var res = DriverApiNativeMethods.LibraryManagement.cuKernelGetAttribute(ref temp, CuFunctionAttribute.LocalSizeBytes, this, device);
            if (res != CuResult.Success) 
                throw new CudaException(res);
            return temp;
        }
        public int GetRegisters(CuDevice device)
        {
            var temp = 0;
            var res = DriverApiNativeMethods.LibraryManagement.cuKernelGetAttribute(ref temp, CuFunctionAttribute.NumRegs, this, device);
            if (res != CuResult.Success) 
                throw new CudaException(res);
            return temp;
        }
        public Version GetPtxVersion(CuDevice device)
        {
            var temp = 0;
            var res = DriverApiNativeMethods.LibraryManagement.cuKernelGetAttribute(ref temp, CuFunctionAttribute.PtxVersion, this, device);
            if (res != CuResult.Success) 
                throw new CudaException(res);
            return new Version(temp / 10, temp % 10);
        }
        public Version GetBinaryVersion(CuDevice device)
        {
            var temp = 0;
            var res = DriverApiNativeMethods.LibraryManagement.cuKernelGetAttribute(ref temp, CuFunctionAttribute.BinaryVersion, this, device);
            if (res != CuResult.Success) 
                throw new CudaException(res);
            return new Version(temp / 10, temp % 10);
        }
        public bool GetCacheModeCa(CuDevice device)
        {
            var temp = 0;
            var res = DriverApiNativeMethods.LibraryManagement.cuKernelGetAttribute(ref temp, CuFunctionAttribute.CacheModeCa, this, device);
            if (res != CuResult.Success) 
                throw new CudaException(res);
            return temp != 0;
        }
        public int GetMaxDynamicSharedSizeBytes(CuDevice device)
        {
            var temp = 0;
            var res = DriverApiNativeMethods.LibraryManagement.cuKernelGetAttribute(ref temp, CuFunctionAttribute.MaxDynamicSharedSizeBytes, this, device);
            if (res != CuResult.Success) 
                throw new CudaException(res);
            return temp;
        }
        public void SetMaxDynamicSharedSizeBytes(int size, CuDevice device)
        {
            var res = DriverApiNativeMethods.LibraryManagement.cuKernelSetAttribute(CuFunctionAttribute.MaxDynamicSharedSizeBytes, size, this, device);
            if (res != CuResult.Success) 
                throw new CudaException(res);
        }
        public CuSharedCarveout GetPreferredSharedMemoryCarveout(CuDevice device)
        {
            var temp = 0;
            var res = DriverApiNativeMethods.LibraryManagement.cuKernelGetAttribute(ref temp, CuFunctionAttribute.PreferredSharedMemoryCarveout, this, device);
            if (res != CuResult.Success) 
                throw new CudaException(res);
            return (CuSharedCarveout)temp;
        }
        public void SetPreferredSharedMemoryCarveout(CuSharedCarveout value, CuDevice device)
        {
            var res = DriverApiNativeMethods.LibraryManagement.cuKernelSetAttribute(CuFunctionAttribute.PreferredSharedMemoryCarveout, (int)value, this, device);
            if (res != CuResult.Success) 
                throw new CudaException(res);
        }
        public bool GetClusterSizeMustBeSet(CuDevice device)
        {
            var temp = 0;
            var res = DriverApiNativeMethods.LibraryManagement.cuKernelGetAttribute(ref temp, CuFunctionAttribute.ClusterSizeMustBeSet, this, device);
            if (res != CuResult.Success) 
                throw new CudaException(res);
            return temp != 0;
        }
        public int GetRequiredClusterWidth(CuDevice device)
        {
            var temp = 0;
            var res = DriverApiNativeMethods.LibraryManagement.cuKernelGetAttribute(ref temp, CuFunctionAttribute.RequiredClusterWidth, this, device);
            if (res != CuResult.Success) 
                throw new CudaException(res);
            return temp;
        }
        public void SetRequiredClusterWidth(int value, CuDevice device)
        {
            var res = DriverApiNativeMethods.LibraryManagement.cuKernelSetAttribute(CuFunctionAttribute.RequiredClusterWidth, value, this, device);
            if (res != CuResult.Success) 
                throw new CudaException(res);
        }
        public int GetRequiredClusterHeight(CuDevice device)
        {
            var temp = 0;
            var res = DriverApiNativeMethods.LibraryManagement.cuKernelGetAttribute(ref temp, CuFunctionAttribute.RequiredClusterHeight, this, device);
            if (res != CuResult.Success) 
                throw new CudaException(res);
            return temp;
        }
        public void SetRequiredClusterHeight(int value, CuDevice device)
        {
            var res = DriverApiNativeMethods.LibraryManagement.cuKernelSetAttribute(CuFunctionAttribute.RequiredClusterHeight, value, this, device);
            if (res != CuResult.Success) 
                throw new CudaException(res);
        }
        public int GetRequiredClusterDepth(CuDevice device)
        {
            var temp = 0;
            var res = DriverApiNativeMethods.LibraryManagement.cuKernelGetAttribute(ref temp, CuFunctionAttribute.RequiredClusterDepth, this, device);
            if (res != CuResult.Success) 
                throw new CudaException(res);
            return temp;
        }
        public void SetRequiredClusterDepth(int value, CuDevice device)
        {
            var res = DriverApiNativeMethods.LibraryManagement.cuKernelSetAttribute(CuFunctionAttribute.RequiredClusterDepth, value, this, device);
            if (res != CuResult.Success) 
                throw new CudaException(res);
        }
        public bool GetNonPortableClusterSizeAllowed(CuDevice device)
        {
            var temp = 0;
            var res = DriverApiNativeMethods.LibraryManagement.cuKernelGetAttribute(ref temp, CuFunctionAttribute.NonPortableClusterSizeAllowed, this, device);
            if (res != CuResult.Success) 
                throw new CudaException(res);
            return temp != 0;
        }
        public CuClusterSchedulingPolicy GetClusterSchedulingPolicyPreference(CuDevice device)
        {
            var temp = 0;
            var res = DriverApiNativeMethods.LibraryManagement.cuKernelGetAttribute(ref temp, CuFunctionAttribute.ClusterSchedulingPolicyPreference, this, device);
            if (res != CuResult.Success) 
                throw new CudaException(res);
            return (CuClusterSchedulingPolicy)temp;
        }
        public void SetClusterSchedulingPolicyPreference(CuClusterSchedulingPolicy value, CuDevice device)
        {
            var res = DriverApiNativeMethods.LibraryManagement.cuKernelSetAttribute(CuFunctionAttribute.ClusterSchedulingPolicyPreference, (int)value, this, device);
            if (res != CuResult.Success) 
                throw new CudaException(res);
        }
        public void SetCacheConfig(CuFuncCache config, CuDevice device)
        {
            var res = DriverApiNativeMethods.LibraryManagement.cuKernelSetCacheConfig(this, config, device);
            if (res != CuResult.Success)
                throw new CudaException(res);
        }
    }
}
