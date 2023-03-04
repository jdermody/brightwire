using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace BrightData.Cuda.CudaToolkit
{
    internal static class DriverApiNativeMethods
    {
        internal const string CudaDriverApiDllName = "nvcuda";
        internal const string CublasApiDllName = "cublas64_12";
        internal const string CusolveApiDllName = "cusolver64_11.dll";
        internal const string CudaObsolet92 = "Don't use this CUDA API call with CUDA version >= 9.2.";
        internal const string CudaObsolet11 = "Don't use this CUDA API call with CUDA version >= 11";
        internal const string CudaObsolet12 = "Don't use this CUDA API call with CUDA version >= 12";
        internal const string CudaDriverApiDllNameLinux = "libcuda";
        internal const string CublasApiDllNameLinux = "cublas";
        internal const string CusolveApiDllNameLinux = "cusolver";

        static DriverApiNativeMethods()
        {
            NativeLibrary.SetDllImportResolver(typeof(DriverApiNativeMethods).Assembly, ImportResolver);
        }

        static IntPtr ImportResolver(string libraryName, System.Reflection.Assembly assembly, DllImportSearchPath? searchPath)
        {
            var libHandle = IntPtr.Zero;

            if (libraryName == CudaDriverApiDllName) {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                    var res = NativeLibrary.TryLoad(CudaDriverApiDllNameLinux, assembly, DllImportSearchPath.SafeDirectories, out libHandle);
                    if (!res) {
                        Debug.WriteLine("Failed to load '" + CudaDriverApiDllNameLinux + "' shared library. Falling back to (Windows-) default library name '"
                                        + CudaDriverApiDllName + "'. Check LD_LIBRARY_PATH environment variable for correct paths.");
                    }
                }
            }

            if (libraryName == CublasApiDllName) {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                    var res = NativeLibrary.TryLoad(CublasApiDllNameLinux, assembly, DllImportSearchPath.SafeDirectories, out libHandle);
                    if (!res) {
                        Debug.WriteLine("Failed to load '" + CublasApiDllNameLinux + "' shared library. Falling back to (Windows-) default library name '"
                                        + CublasApiDllName + "'. Check LD_LIBRARY_PATH environment variable for correct paths.");
                    }
                }
            }

            if (libraryName == CusolveApiDllName)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    var res = NativeLibrary.TryLoad(CusolveApiDllNameLinux, assembly, DllImportSearchPath.SafeDirectories, out libHandle);
                    if (!res)
                    {
                        Debug.WriteLine("Failed to load '" + CusolveApiDllNameLinux + "' shared library. Falling back to (Windows-) default library name '"
                                        + CusolveApiDllName + "'. Check LD_LIBRARY_PATH environment variable for correct paths.");
                    }
                }
            }
            return libHandle;
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        internal static void Init()
        {
        }

        //internal const string CudaPtds = "_ptds";
		//internal const string CudaPtsz = "_ptsz";
        internal const string CudaPtds = "";
        internal const string CudaPtsz = "";

        public static Version Version => new Version(12, 0);

        [DllImport(CudaDriverApiDllName)]
        public static extern CuResult cuInit(CuInitializationFlags flags);
        [DllImport(CudaDriverApiDllName)]
        public static extern CuResult cuDriverGetVersion(ref int driverVersion);
        public static class DeviceManagement
        {
            static DeviceManagement()
            {
                DriverApiNativeMethods.Init();
            }
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuDeviceGet(ref CUdevice device, int ordinal);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuDeviceGetCount(ref int count);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuDeviceGetName([Out] byte[] name, int len, CUdevice dev);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuDeviceGetUuid(ref CUuuid uuid, CUdevice dev);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuDeviceGetUuid_v2(ref CUuuid uuid, CUdevice dev);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuDeviceGetLuid(ref Luid luid, ref uint deviceNodeMask, CUdevice dev);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuDeviceTotalMem_v2(ref SizeT bytes, CUdevice dev);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuDeviceGetTexture1DLinearMaxWidth(ref SizeT maxWidthInElements, CuArrayFormat format, uint numChannels, CUdevice dev);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuDeviceGetAttribute(ref int pi, CuDeviceAttribute attrib, CUdevice dev);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuDeviceGetNvSciSyncAttributes(IntPtr nvSciSyncAttrList, CUdevice dev, NvSciSyncAttr flags);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuDeviceSetMemPool(CUdevice dev, CUmemoryPool pool);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuDeviceGetMemPool(ref CUmemoryPool pool, CUdevice dev);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuDeviceGetDefaultMemPool(ref CUmemoryPool poolOut, CUdevice dev);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuDeviceGetByPCIBusId(ref CUdevice dev, [In, Out] byte[] pciBusId);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuDeviceGetPCIBusId([In, Out] byte[] pciBusId, int len, CUdevice dev);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuIpcGetEventHandle(ref CUipcEventHandle pHandle, CUevent cuevent);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuIpcOpenEventHandle(ref CUevent phEvent, CUipcEventHandle handle);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuIpcGetMemHandle(ref CUipcMemHandle pHandle, CUdeviceptr dptr);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuIpcOpenMemHandle_v2")]
            public static extern CuResult cuIpcOpenMemHandle(ref CUdeviceptr pdptr, CUipcMemHandle handle, uint flags);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuIpcCloseMemHandle(CUdeviceptr dptr);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuDeviceGetExecAffinitySupport(ref int pi, CUexecAffinityType type, CUdevice dev);
        }
        
        public static class ContextManagement
        {
            static ContextManagement()
            {
                DriverApiNativeMethods.Init();
            }
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuCtxCreate_v2(ref CUcontext pctx, CuCtxFlags flags, CUdevice dev);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuCtxCreate_v3(ref CUcontext pctx, CUexecAffinityParam[] paramsArray, int numParams, CuCtxFlags flags, CUdevice dev);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuCtxDestroy_v2(CUcontext ctx);
            [DllImport(CudaDriverApiDllName)]
            [Obsolete(CudaObsolet92)]
            public static extern CuResult cuCtxAttach(ref CUcontext pctx, CuCtxAttachFlags flags);
            [DllImport(CudaDriverApiDllName)]
            [Obsolete(CudaObsolet92)]
            public static extern CuResult cuCtxDetach([In] CUcontext ctx);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuCtxPushCurrent_v2([In] CUcontext ctx);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuCtxPopCurrent_v2(ref CUcontext pctx);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuCtxSetCurrent([In] CUcontext ctx);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuCtxGetCurrent(ref CUcontext pctx);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuCtxGetDevice(ref CUdevice device);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuCtxSynchronize();
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuCtxGetApiVersion(CUcontext ctx, ref uint version);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuCtxGetCacheConfig(ref CuFuncCache pconfig);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuCtxSetCacheConfig(CuFuncCache config);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuCtxGetSharedMemConfig(ref CUsharedconfig pConfig);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuCtxSetSharedMemConfig(CUsharedconfig config);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuCtxGetStreamPriorityRange(ref int leastPriority, ref int greatestPriority);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuCtxResetPersistingL2Cache();
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuCtxGetExecAffinity(ref CUexecAffinityParam pExecAffinity, CUexecAffinityType type);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuCtxGetFlags(ref CuCtxFlags flags);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuCtxGetId(CUcontext ctx, ref ulong ctxId);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuDevicePrimaryCtxRetain(ref CUcontext pctx, CUdevice dev);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuDevicePrimaryCtxRelease_v2")]
            public static extern CuResult cuDevicePrimaryCtxRelease(CUdevice dev);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuDevicePrimaryCtxSetFlags_v2")]
            public static extern CuResult cuDevicePrimaryCtxSetFlags(CUdevice dev, CuCtxFlags flags);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuDevicePrimaryCtxGetState(CUdevice dev, ref CuCtxFlags flags, ref int active);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuDevicePrimaryCtxReset_v2")]
            public static extern CuResult cuDevicePrimaryCtxReset(CUdevice dev);
        }
        
        public static class ModuleManagement
        {
            static ModuleManagement()
            {
                DriverApiNativeMethods.Init();
            }
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuModuleLoad(ref CUmodule module, string fname);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuModuleLoadData(ref CUmodule module, [In] byte[] image);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuModuleLoadDataEx(ref CUmodule module, [In] byte[] image, uint numOptions, [In] CujitOption[]? options, [In, Out] IntPtr[]? optionValues);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuModuleLoadFatBinary(ref CUmodule module, [In] byte[] fatCubin);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuModuleUnload(CUmodule hmod);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuModuleGetFunction(ref CUfunction hfunc, CUmodule hmod, string name);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuModuleGetGlobal_v2(ref CUdeviceptr dptr, ref SizeT bytes, CUmodule hmod, string name);
            [DllImport(CudaDriverApiDllName)]
            [Obsolete(CudaObsolet12)]
            public static extern CuResult cuModuleGetTexRef(ref CUtexref pTexRef, CUmodule hmod, string name);
            [DllImport(CudaDriverApiDllName)]
            [Obsolete(CudaObsolet12)]
            public static extern CuResult cuModuleGetSurfRef(ref CUsurfref pSurfRef, CUmodule hmod, string name);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuLinkCreate_v2")]
            public static extern CuResult cuLinkCreate(uint numOptions, CujitOption[] options, [In, Out] IntPtr[] optionValues, ref CUlinkState stateOut);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuLinkAddData_v2")]
            public static extern CuResult cuLinkAddData(CUlinkState state, CujitInputType type, byte[] data, SizeT size, [MarshalAs(UnmanagedType.LPStr)] string name,
                uint numOptions, CujitOption[] options, IntPtr[] optionValues);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuLinkAddFile_v2")]
            public static extern CuResult cuLinkAddFile(CUlinkState state, CujitInputType type, string path, uint numOptions, CujitOption[] options, IntPtr[] optionValues);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuLinkComplete(CUlinkState state, ref IntPtr cubinOut, ref SizeT sizeOut);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuLinkDestroy(CUlinkState state);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuModuleGetLoadingMode(ref CUmoduleLoadingMode mode);
        }
        
        public static class LibraryManagement
        {
            static LibraryManagement()
            {
                DriverApiNativeMethods.Init();
            }
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuLibraryLoadData(ref CUlibrary library, byte[] code,
                CujitOption[] jitOptions, IntPtr[] jitOptionsValues, uint numJitOptions,
                CUlibraryOption[] libraryOptions, IntPtr[] libraryOptionValues, uint numLibraryOptions);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuLibraryLoadData(ref CUlibrary library, IntPtr code,
                CujitOption[] jitOptions, IntPtr[] jitOptionsValues, uint numJitOptions,
                CUlibraryOption[] libraryOptions, IntPtr[] libraryOptionValues, uint numLibraryOptions);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuLibraryLoadFromFile(ref CUlibrary library, [MarshalAs(UnmanagedType.LPStr)] string fileName,
                CujitOption[] jitOptions, IntPtr[] jitOptionsValues, uint numJitOptions,
                CUlibraryOption[] libraryOptions, IntPtr[] libraryOptionValues, uint numLibraryOptions);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuLibraryUnload(CUlibrary library);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuLibraryGetKernel(ref CUkernel pKernel, CUlibrary library, [MarshalAs(UnmanagedType.LPStr)] string name);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuLibraryGetModule(ref CUmodule pMod, CUlibrary library);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuKernelGetFunction(ref CUfunction pFunc, CUkernel kernel);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuLibraryGetGlobal(ref CUdeviceptr dptr, ref SizeT bytes, CUlibrary library, [MarshalAs(UnmanagedType.LPStr)] string name);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuLibraryGetManaged(ref CUdeviceptr dptr, ref SizeT bytes, CUlibrary library, [MarshalAs(UnmanagedType.LPStr)] string name);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuLibraryGetUnifiedFunction(ref IntPtr fptr, CUlibrary library, [MarshalAs(UnmanagedType.LPStr)] string symbol);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuKernelGetAttribute(ref int pi, CuFunctionAttribute attrib, CUkernel kernel, CUdevice dev);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuKernelSetAttribute(CuFunctionAttribute attrib, int val, CUkernel kernel, CUdevice dev);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuKernelSetCacheConfig(CUkernel kernel, CuFuncCache config, CUdevice dev);
        }
        
        public static class MemoryManagement
        {
            static MemoryManagement()
            {
                DriverApiNativeMethods.Init();
            }
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemGetInfo_v2(ref SizeT free, ref SizeT total);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemAlloc_v2(ref CUdeviceptr dptr, SizeT bytesize);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemAllocPitch_v2(ref CUdeviceptr dptr, ref SizeT pPitch, SizeT widthInBytes, SizeT height, uint elementSizeBytes);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemFree_v2(CUdeviceptr dptr);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemGetAddressRange_v2(ref CUdeviceptr pbase, ref SizeT psize, CUdeviceptr dptr);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemAllocHost_v2(ref IntPtr pp, SizeT bytesize);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemFreeHost(IntPtr p);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemHostAlloc(ref IntPtr pp, SizeT bytesize, CuMemHostAllocFlags flags);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemHostGetDevicePointer_v2(ref CUdeviceptr pdptr, IntPtr p, int flags);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemHostGetFlags(ref CuMemHostAllocFlags pFlags, IntPtr p);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemHostRegister_v2")]
            public static extern CuResult cuMemHostRegister(IntPtr p, SizeT byteSize, CuMemHostRegisterFlags flags);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemHostUnregister(IntPtr p);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuPointerGetAttribute(ref CUcontext data, CuPointerAttribute attribute, CUdeviceptr ptr);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuPointerGetAttribute(ref CuMemoryType data, CuPointerAttribute attribute, CUdeviceptr ptr);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuPointerGetAttribute(ref CUdeviceptr data, CuPointerAttribute attribute, CUdeviceptr ptr);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuPointerGetAttribute(ref IntPtr data, CuPointerAttribute attribute, CUdeviceptr ptr);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuPointerGetAttribute(ref CudaPointerAttributeP2PTokens data, CuPointerAttribute attribute, CUdeviceptr ptr);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuPointerGetAttribute(ref int data, CuPointerAttribute attribute, CUdeviceptr ptr);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuPointerGetAttribute(ref ulong data, CuPointerAttribute attribute, CUdeviceptr ptr);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemPrefetchAsync" + CudaPtsz)]
            public static extern CuResult cuMemPrefetchAsync(CUdeviceptr devPtr, SizeT count, CUdevice dstDevice, CUstream hStream);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemAdvise(CUdeviceptr devPtr, SizeT count, CUmemAdvise advice, CUdevice device);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemRangeGetAttribute(IntPtr data, SizeT dataSize, CUmemRangeAttribute attribute, CUdeviceptr devPtr, SizeT count);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemRangeGetAttributes([In, Out] IntPtr[] data, [In, Out] SizeT[] dataSizes, [In, Out] CUmemRangeAttribute[] attributes, SizeT numAttributes, CUdeviceptr devPtr, SizeT count);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemAllocManaged(ref CUdeviceptr dptr, SizeT bytesize, CUmemAttachFlags flags);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuPointerSetAttribute(ref int value, CuPointerAttribute attribute, CUdeviceptr ptr);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuPointerGetAttributes(uint numAttributes, [In, Out] CuPointerAttribute[] attributes, IntPtr data, CUdeviceptr ptr);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemAddressReserve(ref CUdeviceptr ptr, SizeT size, SizeT alignment, CUdeviceptr addr, ulong flags);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemAddressFree(CUdeviceptr ptr, SizeT size);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemCreate(ref CUmemGenericAllocationHandle handle, SizeT size, ref CUmemAllocationProp prop, ulong flags);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemRelease(CUmemGenericAllocationHandle handle);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemMap(CUdeviceptr ptr, SizeT size, SizeT offset, CUmemGenericAllocationHandle handle, ulong flags);
            [DllImport(CudaDriverApiDllName, EntryPoint = "pcuMemMapArrayAsync" + CudaPtsz)]
            public static extern CuResult pcuMemMapArrayAsync(CUarrayMapInfo[] mapInfoList, uint count, CUstream hStream);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemUnmap(CUdeviceptr ptr, SizeT size);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemSetAccess(CUdeviceptr ptr, SizeT size, CUmemAccessDesc[] desc, SizeT count);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemGetAccess(ref ulong flags, ref CUmemLocation location, CUdeviceptr ptr);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemExportToShareableHandle(IntPtr shareableHandle, CUmemGenericAllocationHandle handle, CUmemAllocationHandleType handleType, ulong flags);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemImportFromShareableHandle(ref CUmemGenericAllocationHandle handle, IntPtr osHandle, CUmemAllocationHandleType shHandleType);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemGetAllocationGranularity(ref SizeT granularity, ref CUmemAllocationProp prop, CUmemAllocationGranularityFlags option);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemGetAllocationPropertiesFromHandle(ref CUmemAllocationProp prop, CUmemGenericAllocationHandle handle);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemRetainAllocationHandle(ref CUmemGenericAllocationHandle handle, IntPtr addr);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemFreeAsync" + CudaPtsz)]
            public static extern CuResult cuMemFreeAsync(CUdeviceptr dptr, CUstream hStream);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemAllocAsync" + CudaPtsz)]
            public static extern CuResult cuMemAllocAsync(ref CUdeviceptr dptr, SizeT bytesize, CUstream hStream);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemPoolTrimTo(CUmemoryPool pool, SizeT minBytesToKeep);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemPoolSetAttribute(CUmemoryPool pool, CUmemPoolAttribute attr, ref int value);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemPoolSetAttribute(CUmemoryPool pool, CUmemPoolAttribute attr, ref ulong value);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemPoolGetAttribute(CUmemoryPool pool, CUmemPoolAttribute attr, ref int value);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemPoolGetAttribute(CUmemoryPool pool, CUmemPoolAttribute attr, ref ulong value);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemPoolSetAccess(CUmemoryPool pool, CUmemAccessDesc[] map, SizeT count);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemPoolGetAccess(ref CUmemAccessFlags flags, CUmemoryPool memPool, ref CUmemLocation location);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemPoolCreate(ref CUmemoryPool pool, ref CUmemPoolProps poolProps);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemPoolDestroy(CUmemoryPool pool);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemAllocFromPoolAsync" + CudaPtsz)]
            public static extern CuResult cuMemAllocFromPoolAsync(ref CUdeviceptr dptr, SizeT bytesize, CUmemoryPool pool, CUstream hStream);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemPoolExportToShareableHandle(ref IntPtr handleOut, CUmemoryPool pool, CUmemAllocationHandleType handleType, ulong flags);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemPoolImportFromShareableHandle(
                ref CUmemoryPool poolOut,
                IntPtr handle,
                CUmemAllocationHandleType handleType,
                ulong flags);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemPoolExportPointer(ref CUmemPoolPtrExportData shareDataOut, CUdeviceptr ptr);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemPoolImportPointer(ref CUdeviceptr ptrOut, CUmemoryPool pool, ref CUmemPoolPtrExportData shareData);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemGetHandleForAddressRange(IntPtr handle, CUdeviceptr dptr, SizeT size, CUmemRangeHandleType handleType, ulong flags);
        }
        
        public static class SynchronousMemcpyV2
        {
            static SynchronousMemcpyV2()
            {
                DriverApiNativeMethods.Init();
            }
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpy" + CudaPtds)]
            public static extern CuResult cuMemcpy(CUdeviceptr dst, CUdeviceptr src, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyPeer" + CudaPtds)]
            public static extern CuResult cuMemcpyPeer(CUdeviceptr dstDevice, CUcontext dstContext, CUdeviceptr srcDevice, CUcontext srcContext, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpy3DPeer" + CudaPtds)]
            public static extern CuResult cuMemcpy3DPeer(ref CudaMemCpy3DPeer pCopy);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyHtoD_v2(CUdeviceptr dstDevice, [In] Dim3[] srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyHtoD_v2(CUdeviceptr dstDevice, [In] byte[] srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyHtoD_v2(CUdeviceptr dstDevice, [In] sbyte[] srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyHtoD_v2(CUdeviceptr dstDevice, [In] ushort[] srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyHtoD_v2(CUdeviceptr dstDevice, [In] short[] srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyHtoD_v2(CUdeviceptr dstDevice, [In] uint[] srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyHtoD_v2(CUdeviceptr dstDevice, [In] int[] srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyHtoD_v2(CUdeviceptr dstDevice, [In] ulong[] srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyHtoD_v2(CUdeviceptr dstDevice, [In] long[] srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyHtoD_v2(CUdeviceptr dstDevice, [In] float[] srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyHtoD_v2(CUdeviceptr dstDevice, [In] double[] srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyHtoD_v2(CUdeviceptr dstDevice, [In] ref Dim3 srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyHtoD_v2(CUdeviceptr dstDevice, [In] ref byte srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyHtoD_v2(CUdeviceptr dstDevice, [In] ref sbyte srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyHtoD_v2(CUdeviceptr dstDevice, [In] ref ushort srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyHtoD_v2(CUdeviceptr dstDevice, [In] ref short srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyHtoD_v2(CUdeviceptr dstDevice, [In] ref uint srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyHtoD_v2(CUdeviceptr dstDevice, [In] ref int srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyHtoD_v2(CUdeviceptr dstDevice, [In] ref ulong srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyHtoD_v2(CUdeviceptr dstDevice, [In] ref long srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyHtoD_v2(CUdeviceptr dstDevice, [In] ref float srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyHtoD_v2(CUdeviceptr dstDevice, [In] ref double srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyHtoD_v2(CUdeviceptr dstDevice, [In] IntPtr srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyDtoH_v2([Out] Dim3[] dstHost, CUdeviceptr srcDevice, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyDtoH_v2([Out] byte[] dstHost, CUdeviceptr srcDevice, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyDtoH_v2([Out] sbyte[] dstHost, CUdeviceptr srcDevice, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyDtoH_v2([Out] ushort[] dstHost, CUdeviceptr srcDevice, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyDtoH_v2([Out] short[] dstHost, CUdeviceptr srcDevice, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyDtoH_v2([Out] uint[] dstHost, CUdeviceptr srcDevice, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyDtoH_v2([Out] int[] dstHost, CUdeviceptr srcDevice, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyDtoH_v2([Out] ulong[] dstHost, CUdeviceptr srcDevice, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyDtoH_v2([Out] long[] dstHost, CUdeviceptr srcDevice, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyDtoH_v2([Out] float[] dstHost, CUdeviceptr srcDevice, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyDtoH_v2([Out] double[] dstHost, CUdeviceptr srcDevice, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyDtoH_v2(ref Dim3 dstHost, CUdeviceptr srcDevice, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyDtoH_v2(ref byte dstHost, CUdeviceptr srcDevice, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyDtoH_v2(ref sbyte dstHost, CUdeviceptr srcDevice, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyDtoH_v2(ref ushort dstHost, CUdeviceptr srcDevice, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyDtoH_v2(ref short dstHost, CUdeviceptr srcDevice, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyDtoH_v2(ref uint dstHost, CUdeviceptr srcDevice, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyDtoH_v2(ref int dstHost, CUdeviceptr srcDevice, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyDtoH_v2(ref ulong dstHost, CUdeviceptr srcDevice, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyDtoH_v2(ref long dstHost, CUdeviceptr srcDevice, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyDtoH_v2(ref float dstHost, CUdeviceptr srcDevice, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyDtoH_v2(ref double dstHost, CUdeviceptr srcDevice, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyDtoH_v2([Out] IntPtr dstHost, CUdeviceptr srcDevice, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoD_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyDtoD_v2(CUdeviceptr dstDevice, CUdeviceptr srcDevice, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoA_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyDtoA_v2(CUarray dstArray, SizeT dstOffset, CUdeviceptr srcDevice, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyAtoD_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyAtoD_v2(CUdeviceptr dstDevice, CUarray srcArray, SizeT srcOffset, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoA_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyHtoA_v2(CUarray dstArray, SizeT dstOffset, [In] Dim3[] srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoA_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyHtoA_v2(CUarray dstArray, SizeT dstOffset, [In] byte[] srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoA_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyHtoA_v2(CUarray dstArray, SizeT dstOffset, [In] sbyte[] srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoA_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyHtoA_v2(CUarray dstArray, SizeT dstOffset, [In] ushort[] srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoA_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyHtoA_v2(CUarray dstArray, SizeT dstOffset, [In] short[] srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoA_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyHtoA_v2(CUarray dstArray, SizeT dstOffset, [In] uint[] srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoA_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyHtoA_v2(CUarray dstArray, SizeT dstOffset, [In] int[] srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoA_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyHtoA_v2(CUarray dstArray, SizeT dstOffset, [In] ulong[] srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoA_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyHtoA_v2(CUarray dstArray, SizeT dstOffset, [In] long[] srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoA_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyHtoA_v2(CUarray dstArray, SizeT dstOffset, [In] float[] srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoA_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyHtoA_v2(CUarray dstArray, SizeT dstOffset, [In] double[] srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoA_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyHtoA_v2(CUarray dstArray, SizeT dstOffset, [In] IntPtr srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyAtoH_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyAtoH_v2([Out] Dim3[] dstHost, CUarray srcArray, SizeT srcOffset, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyAtoH_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyAtoH_v2([Out] byte[] dstHost, CUarray srcArray, SizeT srcOffset, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyAtoH_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyAtoH_v2([Out] sbyte[] dstHost, CUarray srcArray, SizeT srcOffset, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyAtoH_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyAtoH_v2([Out] ushort[] dstHost, CUarray srcArray, SizeT srcOffset, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyAtoH_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyAtoH_v2([Out] short[] dstHost, CUarray srcArray, SizeT srcOffset, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyAtoH_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyAtoH_v2([Out] uint[] dstHost, CUarray srcArray, SizeT srcOffset, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyAtoH_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyAtoH_v2([Out] int[] dstHost, CUarray srcArray, SizeT srcOffset, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyAtoH_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyAtoH_v2([Out] ulong[] dstHost, CUarray srcArray, SizeT srcOffset, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyAtoH_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyAtoH_v2([Out] long[] dstHost, CUarray srcArray, SizeT srcOffset, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyAtoH_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyAtoH_v2([Out] float[] dstHost, CUarray srcArray, SizeT srcOffset, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyAtoH_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyAtoH_v2([Out] double[] dstHost, CUarray srcArray, SizeT srcOffset, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyAtoH_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyAtoH_v2([Out] IntPtr dstHost, CUarray srcArray, SizeT srcOffset, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyAtoA_v2" + CudaPtds)]
            public static extern CuResult cuMemcpyAtoA_v2(CUarray dstArray, SizeT dstOffset, CUarray srcArray, SizeT srcOffset, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpy2D_v2" + CudaPtds)]
            public static extern CuResult cuMemcpy2D_v2(ref CudaMemCpy2D pCopy);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpy2DUnaligned_v2" + CudaPtds)]
            public static extern CuResult cuMemcpy2DUnaligned_v2(ref CudaMemCpy2D pCopy);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpy3D_v2" + CudaPtds)]
            public static extern CuResult cuMemcpy3D_v2(ref CudaMemCpy3D pCopy);
        }
        
        public static class AsynchronousMemcpyV2
        {
            static AsynchronousMemcpyV2()
            {
                DriverApiNativeMethods.Init();
            }
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyAsync" + CudaPtsz)]
            public static extern CuResult cuMemcpyAsync(CUdeviceptr dst, CUdeviceptr src, SizeT byteCount, CUstream hStream);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyPeerAsync" + CudaPtsz)]
            public static extern CuResult cuMemcpyPeerAsync(CUdeviceptr dstDevice, CUcontext dstContext, CUdeviceptr srcDevice, CUcontext srcContext, SizeT byteCount, CUstream hStream);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpy3DPeerAsync" + CudaPtsz)]
            public static extern CuResult cuMemcpy3DPeerAsync(ref CudaMemCpy3DPeer pCopy, CUstream hStream);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoDAsync_v2" + CudaPtsz)]
            public static extern CuResult cuMemcpyHtoDAsync_v2(CUdeviceptr dstDevice, [In] IntPtr srcHost, SizeT byteCount, CUstream hStream);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoHAsync_v2" + CudaPtsz)]
            public static extern CuResult cuMemcpyDtoHAsync_v2([Out] IntPtr dstHost, CUdeviceptr srcDevice, SizeT byteCount, CUstream hStream);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoDAsync_v2" + CudaPtsz)]
            public static extern CuResult cuMemcpyDtoDAsync_v2(CUdeviceptr dstDevice, CUdeviceptr srcDevice, SizeT byteCount, CUstream hStream);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoAAsync_v2" + CudaPtsz)]
            public static extern CuResult cuMemcpyHtoAAsync_v2(CUarray dstArray, SizeT dstOffset, [In] IntPtr srcHost, SizeT byteCount, CUstream hStream);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyAtoHAsync_v2" + CudaPtsz)]
            public static extern CuResult cuMemcpyAtoHAsync_v2([Out] IntPtr dstHost, CUarray srcArray, SizeT srcOffset, SizeT byteCount, CUstream hStream);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpy2DAsync_v2" + CudaPtsz)]
            public static extern CuResult cuMemcpy2DAsync_v2(ref CudaMemCpy2D pCopy, CUstream hStream);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpy3DAsync_v2" + CudaPtsz)]
            public static extern CuResult cuMemcpy3DAsync_v2(ref CudaMemCpy3D pCopy, CUstream hStream);
        }
        
        public static class Memset
        {
            static Memset()
            {
                DriverApiNativeMethods.Init();
            }
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemsetD8_v2" + CudaPtds)]
            public static extern CuResult cuMemsetD8_v2(CUdeviceptr dstDevice, byte b, SizeT n);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemsetD16_v2" + CudaPtds)]
            public static extern CuResult cuMemsetD16_v2(CUdeviceptr dstDevice, ushort us, SizeT n);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemsetD32_v2" + CudaPtds)]
            public static extern CuResult cuMemsetD32_v2(CUdeviceptr dstDevice, uint ui, SizeT n);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemsetD2D8_v2" + CudaPtds)]
            public static extern CuResult cuMemsetD2D8_v2(CUdeviceptr dstDevice, SizeT dstPitch, byte b, SizeT width, SizeT height);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemsetD2D16_v2" + CudaPtds)]
            public static extern CuResult cuMemsetD2D16_v2(CUdeviceptr dstDevice, SizeT dstPitch, ushort us, SizeT width, SizeT height);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemsetD2D32_v2" + CudaPtds)]
            public static extern CuResult cuMemsetD2D32_v2(CUdeviceptr dstDevice, SizeT dstPitch, uint ui, SizeT width, SizeT height);
        }
        
        public static class MemsetAsync
        {
            static MemsetAsync()
            {
                DriverApiNativeMethods.Init();
            }
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemsetD8Async" + CudaPtsz)]
            public static extern CuResult cuMemsetD8Async(CUdeviceptr dstDevice, byte b, SizeT n, CUstream hStream);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemsetD16Async" + CudaPtsz)]
            public static extern CuResult cuMemsetD16Async(CUdeviceptr dstDevice, ushort us, SizeT n, CUstream hStream);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemsetD32Async" + CudaPtsz)]
            public static extern CuResult cuMemsetD32Async(CUdeviceptr dstDevice, uint ui, SizeT n, CUstream hStream);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemsetD2D8Async" + CudaPtsz)]
            public static extern CuResult cuMemsetD2D8Async(CUdeviceptr dstDevice, SizeT dstPitch, byte b, SizeT width, SizeT height, CUstream hStream);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemsetD2D16Async" + CudaPtsz)]
            public static extern CuResult cuMemsetD2D16Async(CUdeviceptr dstDevice, SizeT dstPitch, ushort us, SizeT width, SizeT height, CUstream hStream);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemsetD2D32Async" + CudaPtsz)]
            public static extern CuResult cuMemsetD2D32Async(CUdeviceptr dstDevice, SizeT dstPitch, uint ui, SizeT width, SizeT height, CUstream hStream);
        }
        
        public static class FunctionManagement
        {
            static FunctionManagement()
            {
                DriverApiNativeMethods.Init();
            }
            [DllImport(CudaDriverApiDllName)]
            [Obsolete(CudaObsolet92)]
            public static extern CuResult cuFuncSetBlockShape(CUfunction hfunc, int x, int y, int z);
            [DllImport(CudaDriverApiDllName)]
            [Obsolete(CudaObsolet92)]
            public static extern CuResult cuFuncSetSharedSize(CUfunction hfunc, uint bytes);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuFuncGetAttribute(ref int pi, CuFunctionAttribute attrib, CUfunction hfunc);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuFuncSetAttribute(CUfunction hfunc, CuFunctionAttribute attrib, int value);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuFuncSetCacheConfig(CUfunction hfunc, CuFuncCache config);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuFuncSetSharedMemConfig(CUfunction hfunc, CUsharedconfig config);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuFuncGetModule(ref CUmodule hmod, CUfunction hfunc);
        }
        
        public static class ArrayManagement
        {
            static ArrayManagement()
            {
                DriverApiNativeMethods.Init();
            }
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuArrayCreate_v2(ref CUarray pHandle, ref CudaArrayDescriptor pAllocateArray);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuArrayGetDescriptor_v2(ref CudaArrayDescriptor pArrayDescriptor, CUarray hArray);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuArrayGetSparseProperties(ref CudaArraySparseProperties sparseProperties, CUarray array);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMipmappedArrayGetSparseProperties(ref CudaArraySparseProperties sparseProperties, CUmipmappedArray mipmap);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuArrayGetPlane(ref CUarray pPlaneArray, CUarray hArray, uint planeIdx);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuArrayDestroy(CUarray hArray);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuArray3DCreate_v2(ref CUarray pHandle, ref CudaArray3DDescriptor pAllocateArray);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuArray3DGetDescriptor_v2(ref CudaArray3DDescriptor pArrayDescriptor, CUarray hArray);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMipmappedArrayCreate(ref CUmipmappedArray pHandle, ref CudaArray3DDescriptor pMipmappedArrayDesc, uint numMipmapLevels);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMipmappedArrayGetLevel(ref CUarray pLevelArray, CUmipmappedArray hMipmappedArray, uint level);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMipmappedArrayDestroy(CUmipmappedArray hMipmappedArray);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuArrayGetMemoryRequirements(ref CudaArrayMemoryRequirements memoryRequirements, CUarray array, CUdevice device);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMipmappedArrayGetMemoryRequirements(ref CudaArrayMemoryRequirements memoryRequirements, CUmipmappedArray mipmap, CUdevice device);
        }
        
        public static class Launch
        {
            static Launch()
            {
                DriverApiNativeMethods.Init();
            }
            [DllImport(CudaDriverApiDllName)]
            [Obsolete(CudaObsolet92)]
            public static extern CuResult cuLaunch([In] CUfunction f);
            [DllImport(CudaDriverApiDllName)]
            [Obsolete(CudaObsolet92)]
            public static extern CuResult cuLaunchGrid([In] CUfunction f, [In] int gridWidth, [In] int gridHeight);
            [DllImport(CudaDriverApiDllName)]
            [Obsolete(CudaObsolet92)]
            public static extern CuResult cuLaunchGridAsync([In] CUfunction f, [In] int gridWidth, [In] int gridHeight, [In] CUstream hStream);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuLaunchKernel" + CudaPtsz)]
            public static extern CuResult cuLaunchKernel(CUfunction f,
                uint gridDimX,
                uint gridDimY,
                uint gridDimZ,
                uint blockDimX,
                uint blockDimY,
                uint blockDimZ,
                uint sharedMemBytes,
                CUstream hStream,
                IntPtr[] kernelParams,
                IntPtr[] extra);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuLaunchCooperativeKernel" + CudaPtsz)]
            public static extern CuResult cuLaunchCooperativeKernel(CUfunction f,
                uint gridDimX,
                uint gridDimY,
                uint gridDimZ,
                uint blockDimX,
                uint blockDimY,
                uint blockDimZ,
                uint sharedMemBytes,
                CUstream hStream,
                IntPtr[] kernelParams);
            [DllImport(CudaDriverApiDllName)]
            [Obsolete("This function is deprecated as of CUDA 11.3")]
            public static extern CuResult cuLaunchCooperativeKernelMultiDevice(CudaLaunchParams[] launchParamsList, uint numDevices, CudaCooperativeLaunchMultiDeviceFlags flags);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuLaunchHostFunc" + CudaPtsz)]
            public static extern CuResult cuLaunchHostFunc(CUstream hStream, CUhostFn fn, IntPtr userData);
            public static CuResult CuLaunchKernelEx(ref CUlaunchConfig config, CUfunction f, IntPtr[] kernelParams, IntPtr[] extra)
            {
                var retVal = CuResult.ErrorInvalidValue;
                var conf = new CUlaunchConfigInternal();
                conf.GridDimX = config.GridDimX;
                conf.GridDimY = config.GridDimY;
                conf.GridDimZ = config.GridDimZ;
                conf.BlockDimX = config.BlockDimX;
                conf.BlockDimY = config.BlockDimY;
                conf.BlockDimZ = config.BlockDimZ;
                conf.SharedMemBytes = config.SharedMemBytes;
                conf.HStream = config.HStream;
                conf.NumAttrs = 0;
                conf.Attrs = IntPtr.Zero;

                try {
                    var arraySize = 0;
                    if (config.Attrs != null) {
                        arraySize = config.Attrs.Length;
                        conf.NumAttrs = (uint)arraySize;
                    }

                    var paramsSize = Marshal.SizeOf(typeof(CUlaunchAttribute));

                    if (arraySize > 0) {
                        conf.Attrs = Marshal.AllocHGlobal(arraySize * paramsSize);
                    }

                    for (var i = 0; i < arraySize; i++) {
                        Marshal.StructureToPtr(config.Attrs![i], conf.Attrs + (paramsSize * i), false);
                    }

                    retVal = cuLaunchKernelExInternal(ref conf, f, kernelParams, extra);
                }
                catch {
                    retVal = CuResult.ErrorInvalidValue;
                }
                finally {
                    if (conf.Attrs != IntPtr.Zero) {
                        Marshal.FreeHGlobal(conf.Attrs);
                    }
                }

                return retVal;
            }

            [DllImport(CudaDriverApiDllName, EntryPoint = "cuLaunchKernelEx" + CudaPtsz)]
            static extern CuResult cuLaunchKernelExInternal(ref CUlaunchConfigInternal config, CUfunction f, IntPtr[] kernelParams, IntPtr[] extra);
        }
        
        public static class Events
        {
            static Events()
            {
                DriverApiNativeMethods.Init();
            }
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuEventCreate(ref CUevent phEvent, CuEventFlags flags);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuEventRecord" + CudaPtsz)]
            public static extern CuResult cuEventRecord(CUevent hEvent, CUstream hStream);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuEventRecordWithFlags" + CudaPtsz)]
            public static extern CuResult cuEventRecordWithFlags(CUevent hEvent, CUstream hStream, CuEventRecordFlags flags);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuEventQuery(CUevent hEvent);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuEventSynchronize(CUevent hEvent);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuEventDestroy_v2(CUevent hEvent);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuEventElapsedTime(ref float pMilliseconds, CUevent hStart, CUevent hEnd);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuStreamWaitValue32_v2" + CudaPtsz)]
            public static extern CuResult cuStreamWaitValue32(CUstream stream, CUdeviceptr addr, uint value, CUstreamWaitValueFlags flags);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuStreamWaitValue64_v2" + CudaPtsz)]
            public static extern CuResult cuStreamWaitValue64(CUstream stream, CUdeviceptr addr, ulong value, CUstreamWaitValueFlags flags);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuStreamWriteValue32_v2" + CudaPtsz)]
            public static extern CuResult cuStreamWriteValue32(CUstream stream, CUdeviceptr addr, uint value, CUstreamWriteValueFlags flags);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuStreamWriteValue64_v2" + CudaPtsz)]
            public static extern CuResult cuStreamWriteValue64(CUstream stream, CUdeviceptr addr, ulong value, CUstreamWriteValueFlags flags);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuStreamBatchMemOp_v2" + CudaPtsz)]
            public static extern CuResult cuStreamBatchMemOp(CUstream stream, uint count, CUstreamBatchMemOpParams[] paramArray, uint flags);
        }
        
        public static class Streams
        {
            static Streams()
            {
                DriverApiNativeMethods.Init();
            }
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuStreamCreate(ref CUstream phStream, CuStreamFlags flags);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuStreamQuery" + CudaPtsz)]
            public static extern CuResult cuStreamQuery(CUstream hStream);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuStreamSynchronize" + CudaPtsz)]
            public static extern CuResult cuStreamSynchronize(CUstream hStream);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuStreamDestroy_v2(CUstream hStream);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuStreamCopyAttributes" + CudaPtsz)]
            public static extern CuResult cuStreamCopyAttributes(CUstream dst, CUstream src);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuStreamGetAttribute" + CudaPtsz)]
            public static extern CuResult cuStreamGetAttribute(CUstream hStream, CUstreamAttrId attr, ref CUstreamAttrValue valueOut);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuStreamSetAttribute" + CudaPtsz)]
            public static extern CuResult cuStreamSetAttribute(CUstream hStream, CUstreamAttrId attr, ref CUstreamAttrValue value);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuStreamWaitEvent" + CudaPtsz)]
            public static extern CuResult cuStreamWaitEvent(CUstream hStream, CUevent hEvent, uint flags);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuStreamAddCallback" + CudaPtsz)]
            public static extern CuResult cuStreamAddCallback(CUstream hStream, CUstreamCallback callback, IntPtr userData, CuStreamAddCallbackFlags flags);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuStreamCreateWithPriority(ref CUstream phStream, CuStreamFlags flags, int priority);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuStreamGetPriority" + CudaPtsz)]
            public static extern CuResult cuStreamGetPriority(CUstream hStream, ref int priority);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuStreamGetFlags" + CudaPtsz)]
            public static extern CuResult cuStreamGetFlags(CUstream hStream, ref CuStreamFlags flags);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuStreamGetCtx" + CudaPtsz)]
            public static extern CuResult cuStreamGetCtx(CUstream hStream, ref CUcontext pctx);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuStreamAttachMemAsync" + CudaPtsz)]
            public static extern CuResult cuStreamAttachMemAsync(CUstream hStream, CUdeviceptr dptr, SizeT length, CUmemAttachFlags flags);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuStreamBeginCapture_v2" + CudaPtsz)]
            public static extern CuResult cuStreamBeginCapture(CUstream hStream, CUstreamCaptureMode mode);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuStreamEndCapture" + CudaPtsz)]
            public static extern CuResult cuStreamEndCapture(CUstream hStream, ref CUgraph phGraph);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuStreamIsCapturing" + CudaPtsz)]
            public static extern CuResult cuStreamIsCapturing(CUstream hStream, ref CUstreamCaptureStatus captureStatus);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuThreadExchangeStreamCaptureMode(ref CUstreamCaptureMode mode);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuStreamGetCaptureInfo_v2" + CudaPtsz)]
            public static extern CuResult cuStreamGetCaptureInfo(CUstream hStream, ref CUstreamCaptureStatus captureStatusOut,
                ref ulong idOut, ref CUgraph graphOut, ref IntPtr dependenciesOut, ref SizeT numDependenciesOut);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuStreamUpdateCaptureDependencies" + CudaPtsz)]
            public static extern CuResult cuStreamUpdateCaptureDependencies(CUstream hStream, CUgraphNode[] dependencies, SizeT numDependencies, uint flags);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuStreamGetId" + CudaPtsz)]
            public static extern CuResult cuStreamGetId(CUstream hStream, ref ulong streamId);
        }
        
        public static class GraphicsInterop
        {
            static GraphicsInterop()
            {
                DriverApiNativeMethods.Init();
            }
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphicsUnregisterResource(CUgraphicsResource resource);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphicsSubResourceGetMappedArray(ref CUarray pArray, CUgraphicsResource resource, uint arrayIndex, uint mipLevel);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphicsResourceGetMappedMipmappedArray(ref CUmipmappedArray pMipmappedArray, CUgraphicsResource resource);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphicsResourceGetMappedPointer_v2(ref CUdeviceptr pDevPtr, ref SizeT pSize, CUgraphicsResource resource);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuGraphicsResourceSetMapFlags_v2")]
            public static extern CuResult cuGraphicsResourceSetMapFlags(CUgraphicsResource resource, CuGraphicsMapResourceFlags flags);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuGraphicsMapResources" + CudaPtsz)]
            public static extern CuResult cuGraphicsMapResources(uint count, ref CUgraphicsResource resources, CUstream hStream);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuGraphicsMapResources" + CudaPtsz)]
            public static extern CuResult cuGraphicsMapResources(uint count, CUgraphicsResource[] resources, CUstream hStream);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuGraphicsUnmapResources" + CudaPtsz)]
            public static extern CuResult cuGraphicsUnmapResources(uint count, ref CUgraphicsResource resources, CUstream hStream);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuGraphicsUnmapResources" + CudaPtsz)]
            public static extern CuResult cuGraphicsUnmapResources(uint count, CUgraphicsResource[] resources, CUstream hStream);
        }
        
        public static class ExportTables
        {
            static ExportTables()
            {
                DriverApiNativeMethods.Init();
            }
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGetExportTable(ref IntPtr ppExportTable, ref CUuuid pExportTableId);
        }
        
        public static class Limits
        {
            static Limits()
            {
                DriverApiNativeMethods.Init();
            }
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuCtxSetLimit(CuLimit limit, SizeT value);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuCtxGetLimit(ref SizeT pvalue, CuLimit limit);
        }
        
        public static class CudaPeerAccess
        {
            static CudaPeerAccess()
            {
                DriverApiNativeMethods.Init();
            }
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuDeviceCanAccessPeer(ref int canAccessPeer, CUdevice dev, CUdevice peerDev);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuCtxEnablePeerAccess(CUcontext peerContext, CtxEnablePeerAccessFlags flags);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuCtxDisablePeerAccess(CUcontext peerContext);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuDeviceGetP2PAttribute(ref int value, CUdeviceP2PAttribute attrib, CUdevice srcDevice, CUdevice dstDevice);
        }
        
        public static class Profiling
        {
            static Profiling()
            {
                DriverApiNativeMethods.Init();
            }
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuProfilerInitialize(string configFile, string outputFile, CUoutputMode outputMode);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuProfilerStart();
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuProfilerStop();
        }
        
        public static class ErrorHandling
        {
            static ErrorHandling()
            {
                DriverApiNativeMethods.Init();
            }
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGetErrorString(CuResult error, ref IntPtr pStr);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGetErrorName(CuResult error, ref IntPtr pStr);
        }
        
        public static class Occupancy
        {
            static Occupancy()
            {
                DriverApiNativeMethods.Init();
            }
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuOccupancyMaxActiveBlocksPerMultiprocessor(ref int numBlocks, CUfunction func, int blockSize, SizeT dynamicSMemSize);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuOccupancyMaxPotentialBlockSize(ref int minGridSize, ref int blockSize, CUfunction func, DelCUoccupancyB2DSize blockSizeToDynamicSMemSize, SizeT dynamicSMemSize, int blockSizeLimit);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuOccupancyMaxActiveBlocksPerMultiprocessorWithFlags(ref int numBlocks, CUfunction func, int blockSize, SizeT dynamicSMemSize, CUoccupancyFlags flags);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuOccupancyMaxPotentialBlockSizeWithFlags(ref int minGridSize, ref int blockSize, CUfunction func, DelCUoccupancyB2DSize blockSizeToDynamicSMemSize, SizeT dynamicSMemSize, int blockSizeLimit, CUoccupancyFlags flags);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuOccupancyAvailableDynamicSMemPerBlock(ref SizeT dynamicSmemSize, CUfunction func, int numBlocks, int blockSize);
            public static CuResult CuOccupancyMaxPotentialClusterSize(ref int clusterSize, CUfunction func, ref CUlaunchConfig config)
            {
                var retVal = CuResult.ErrorInvalidValue;
                var conf = new CUlaunchConfigInternal();
                conf.GridDimX = config.GridDimX;
                conf.GridDimY = config.GridDimY;
                conf.GridDimZ = config.GridDimZ;
                conf.BlockDimX = config.BlockDimX;
                conf.BlockDimY = config.BlockDimY;
                conf.BlockDimZ = config.BlockDimZ;
                conf.SharedMemBytes = config.SharedMemBytes;
                conf.HStream = config.HStream;
                conf.NumAttrs = 0;
                conf.Attrs = IntPtr.Zero;

                try {
                    var arraySize = 0;
                    if (config.Attrs != null) {
                        arraySize = config.Attrs.Length;
                        conf.NumAttrs = (uint)arraySize;
                    }

                    var paramsSize = Marshal.SizeOf(typeof(CUlaunchAttribute));

                    if (arraySize > 0) {
                        conf.Attrs = Marshal.AllocHGlobal(arraySize * paramsSize);
                    }

                    for (var i = 0; i < arraySize; i++) {
                        Marshal.StructureToPtr(config.Attrs![i], conf.Attrs + (paramsSize * i), false);
                    }

                    retVal = cuOccupancyMaxPotentialClusterSizeInternal(ref clusterSize, func, ref conf);
                }
                catch {
                    retVal = CuResult.ErrorInvalidValue;
                }
                finally {
                    if (conf.Attrs != IntPtr.Zero) {
                        Marshal.FreeHGlobal(conf.Attrs);
                    }
                }

                return retVal;
            }

            [DllImport(CudaDriverApiDllName, EntryPoint = "cuOccupancyMaxPotentialClusterSize")]
            static extern CuResult cuOccupancyMaxPotentialClusterSizeInternal(ref int clusterSize, CUfunction func, ref CUlaunchConfigInternal config);
            public static CuResult CuOccupancyMaxActiveClusters(ref int numClusters, CUfunction func, ref CUlaunchConfig config)
            {
                var retVal = CuResult.ErrorInvalidValue;
                var conf = new CUlaunchConfigInternal();
                conf.GridDimX = config.GridDimX;
                conf.GridDimY = config.GridDimY;
                conf.GridDimZ = config.GridDimZ;
                conf.BlockDimX = config.BlockDimX;
                conf.BlockDimY = config.BlockDimY;
                conf.BlockDimZ = config.BlockDimZ;
                conf.SharedMemBytes = config.SharedMemBytes;
                conf.HStream = config.HStream;
                conf.NumAttrs = 0;
                conf.Attrs = IntPtr.Zero;

                try {
                    var arraySize = 0;
                    if (config.Attrs != null) {
                        arraySize = config.Attrs.Length;
                        conf.NumAttrs = (uint)arraySize;
                    }

                    var paramsSize = Marshal.SizeOf(typeof(CUlaunchAttribute));
                    if (arraySize > 0) {
                        conf.Attrs = Marshal.AllocHGlobal(arraySize * paramsSize);
                    }

                    for (var i = 0; i < arraySize; i++) {
                        Marshal.StructureToPtr(config.Attrs![i], conf.Attrs + (paramsSize * i), false);
                    }

                    retVal = cuOccupancyMaxActiveClustersInternal(ref numClusters, func, ref conf);
                }
                catch {
                    retVal = CuResult.ErrorInvalidValue;
                }
                finally {
                    if (conf.Attrs != IntPtr.Zero) {
                        Marshal.FreeHGlobal(conf.Attrs);
                    }
                }

                return retVal;
            }

            [DllImport(CudaDriverApiDllName)]
            static extern CuResult cuOccupancyMaxActiveClustersInternal(ref int numClusters, CUfunction func, ref CUlaunchConfigInternal config);
        }
        
        public static class ExternResources
        {
            static ExternResources()
            {
                DriverApiNativeMethods.Init();
            }
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuImportExternalMemory(ref CUexternalMemory extMemOut, ref CudaExternalMemoryHandleDesc memHandleDesc);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuExternalMemoryGetMappedBuffer(ref CUdeviceptr devPtr, CUexternalMemory extMem, ref CudaExternalMemoryBufferDesc bufferDesc);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuExternalMemoryGetMappedMipmappedArray(ref CUmipmappedArray mipmap, CUexternalMemory extMem, ref CudaExternalMemoryMipmappedArrayDesc mipmapDesc);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuDestroyExternalMemory(CUexternalMemory extMem);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuImportExternalSemaphore(ref CUexternalSemaphore extSemOut, ref CudaExternalSemaphoreHandleDesc semHandleDesc);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuSignalExternalSemaphoresAsync" + CudaPtsz)]
            public static extern CuResult cuSignalExternalSemaphoresAsync([In, Out] CUexternalSemaphore[] extSemArray, [In, Out] CudaExternalSemaphoreSignalParams[] paramsArray, uint numExtSems, CUstream stream);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuWaitExternalSemaphoresAsync" + CudaPtsz)]
            public static extern CuResult cuWaitExternalSemaphoresAsync([In, Out] CUexternalSemaphore[] extSemArray, [In, Out] CudaExternalSemaphoreWaitParams[] paramsArray, uint numExtSems, CUstream stream);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuDestroyExternalSemaphore(CUexternalSemaphore extSem);
        }
        public static class GraphManagment
        {
            static GraphManagment()
            {
                DriverApiNativeMethods.Init();
            }
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphCreate(ref CUgraph phGraph, uint flags);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuGraphAddKernelNode_v2")]
            public static extern CuResult cuGraphAddKernelNode(ref CUgraphNode phGraphNode, CUgraph hGraph, CUgraphNode[] dependencies, SizeT numDependencies, ref CudaKernelNodeParams nodeParams);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuGraphKernelNodeGetParams_v2")]
            public static extern CuResult cuGraphKernelNodeGetParams(CUgraphNode hNode, ref CudaKernelNodeParams nodeParams);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuGraphKernelNodeSetParams_v2")]
            public static extern CuResult cuGraphKernelNodeSetParams(CUgraphNode hNode, ref CudaKernelNodeParams nodeParams);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphAddMemcpyNode(ref CUgraphNode phGraphNode, CUgraph hGraph, CUgraphNode[] dependencies, SizeT numDependencies, ref CudaMemCpy3D copyParams, CUcontext ctx);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphMemcpyNodeGetParams(CUgraphNode hNode, ref CudaMemCpy3D nodeParams);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphMemcpyNodeSetParams(CUgraphNode hNode, ref CudaMemCpy3D nodeParams);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphAddMemsetNode(ref CUgraphNode phGraphNode, CUgraph hGraph, CUgraphNode[] dependencies, SizeT numDependencies, ref CudaMemsetNodeParams memsetParams, CUcontext ctx);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphMemsetNodeGetParams(CUgraphNode hNode, ref CudaMemsetNodeParams nodeParams);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphMemsetNodeSetParams(CUgraphNode hNode, ref CudaMemsetNodeParams nodeParams);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphAddHostNode(ref CUgraphNode phGraphNode, CUgraph hGraph, CUgraphNode[] dependencies, SizeT numDependencies, ref CudaHostNodeParams nodeParams);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphHostNodeGetParams(CUgraphNode hNode, ref CudaHostNodeParams nodeParams);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphHostNodeSetParams(CUgraphNode hNode, ref CudaHostNodeParams nodeParams);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphAddChildGraphNode(ref CUgraphNode phGraphNode, CUgraph hGraph, CUgraphNode[] dependencies, SizeT numDependencies, CUgraph childGraph);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphChildGraphNodeGetGraph(CUgraphNode hNode, ref CUgraph phGraph);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphAddEmptyNode(ref CUgraphNode phGraphNode, CUgraph hGraph, CUgraphNode[] dependencies, SizeT numDependencies);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphAddEventRecordNode(ref CUgraphNode phGraphNode, CUgraph hGraph, CUgraphNode[] dependencies, SizeT numDependencies, CUevent @event);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphEventRecordNodeGetEvent(CUgraphNode hNode, ref CUevent eventOut);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphEventRecordNodeSetEvent(CUgraphNode hNode, CUevent @event);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphAddEventWaitNode(ref CUgraphNode phGraphNode, CUgraph hGraph, CUgraphNode[] dependencies, SizeT numDependencies, CUevent @event);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphEventWaitNodeGetEvent(CUgraphNode hNode, ref CUevent eventOut);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphEventWaitNodeSetEvent(CUgraphNode hNode, CUevent @event);
            public static CuResult CuGraphAddExternalSemaphoresSignalNode(ref CUgraphNode phGraphNode, CUgraph hGraph, CUgraphNode[] dependencies, SizeT numDependencies, CudaExtSemSignalNodeParams nodeParams)
            {
                var extSemPtr = IntPtr.Zero;
                var paramsPtr = IntPtr.Zero;
                var mainPtr = IntPtr.Zero;

                var retVal = CuResult.ErrorInvalidValue;

                try {
                    var arraySize = 0;
                    if (nodeParams is { ExtSemArray: { }, ParamsArray: { } }) {
                        if (nodeParams.ExtSemArray.Length != nodeParams.ParamsArray.Length) {
                            return CuResult.ErrorInvalidValue;
                        }

                        arraySize = nodeParams.ExtSemArray.Length;
                    }

                    var paramsSize = Marshal.SizeOf(typeof(CudaExternalSemaphoreSignalParams));

                    mainPtr = Marshal.AllocHGlobal(2 * IntPtr.Size + sizeof(int));

                    if (arraySize > 0) {
                        extSemPtr = Marshal.AllocHGlobal(arraySize * IntPtr.Size);
                        paramsPtr = Marshal.AllocHGlobal(arraySize * paramsSize);
                    }

                    Marshal.WriteIntPtr(mainPtr + 0, extSemPtr);
                    Marshal.WriteIntPtr(mainPtr + IntPtr.Size, paramsPtr);
                    Marshal.WriteInt32(mainPtr + 2 * IntPtr.Size, arraySize);

                    for (var i = 0; i < arraySize; i++) {
                        Marshal.StructureToPtr(nodeParams.ExtSemArray[i], extSemPtr + (IntPtr.Size * i), false);
                        Marshal.StructureToPtr(nodeParams.ParamsArray[i], paramsPtr + (paramsSize * i), false);
                    }

                    retVal = cuGraphAddExternalSemaphoresSignalNodeInternal(ref phGraphNode, hGraph, dependencies, numDependencies, mainPtr);
                }
                catch {
                    retVal = CuResult.ErrorInvalidValue;
                }
                finally {
                    if (mainPtr != IntPtr.Zero) {
                        Marshal.FreeHGlobal(mainPtr);
                    }

                    if (extSemPtr != IntPtr.Zero) {
                        Marshal.FreeHGlobal(extSemPtr);
                    }

                    if (paramsPtr != IntPtr.Zero) {
                        Marshal.FreeHGlobal(paramsPtr);
                    }
                }

                return retVal;
            }

            [DllImport(CudaDriverApiDllName, EntryPoint = "cuGraphAddExternalSemaphoresSignalNode")]
            static extern CuResult cuGraphAddExternalSemaphoresSignalNodeInternal(ref CUgraphNode phGraphNode, CUgraph hGraph, CUgraphNode[] dependencies, SizeT numDependencies, IntPtr nodeParams);
            public static CuResult CuGraphExternalSemaphoresSignalNodeGetParams(CUgraphNode hNode, CudaExtSemSignalNodeParams paramsOut)
            {
                var mainPtr = IntPtr.Zero;

                var retVal = CuResult.ErrorInvalidValue;

                try {
                    var arraySize = 0;
                    var paramsSize = Marshal.SizeOf(typeof(CudaExternalSemaphoreSignalParams));
                    mainPtr = Marshal.AllocHGlobal(2 * IntPtr.Size + sizeof(int));

                    Marshal.WriteIntPtr(mainPtr + 0, IntPtr.Zero);
                    Marshal.WriteIntPtr(mainPtr + IntPtr.Size, IntPtr.Zero);
                    Marshal.WriteInt32(mainPtr + 2 * IntPtr.Size, arraySize);

                    retVal = cuGraphExternalSemaphoresSignalNodeGetParamsInternal(hNode, mainPtr);

                    var length = Marshal.ReadInt32(mainPtr + 2 * IntPtr.Size);

                    var array1 = new CUexternalSemaphore[length];
                    var array2 = new CudaExternalSemaphoreSignalParams[length];
                    var ptr1 = Marshal.ReadIntPtr(mainPtr);
                    var ptr2 = Marshal.ReadIntPtr(mainPtr + IntPtr.Size);

                    for (var i = 0; i < length; i++) {
                        array1[i] = (CUexternalSemaphore)Marshal.PtrToStructure(ptr1 + (IntPtr.Size * i), typeof(CUexternalSemaphore));
                        array2[i] = (CudaExternalSemaphoreSignalParams)Marshal.PtrToStructure(ptr2 + (paramsSize * i), typeof(CudaExternalSemaphoreSignalParams));
                    }

                    paramsOut.ExtSemArray = array1;
                    paramsOut.ParamsArray = array2;
                }
                catch {
                    retVal = CuResult.ErrorInvalidValue;
                }
                finally {
                    if (mainPtr != IntPtr.Zero) {
                        Marshal.FreeHGlobal(mainPtr);
                    }
                }

                return retVal;
            }

            [DllImport(CudaDriverApiDllName, EntryPoint = "cuGraphExternalSemaphoresSignalNodeGetParams")]
            static extern CuResult cuGraphExternalSemaphoresSignalNodeGetParamsInternal(CUgraphNode hNode, IntPtr paramsOut);
            public static CuResult CuGraphExternalSemaphoresSignalNodeSetParams(CUgraphNode hNode, CudaExtSemSignalNodeParams nodeParams)
            {
                var extSemPtr = IntPtr.Zero;
                var paramsPtr = IntPtr.Zero;
                var mainPtr = IntPtr.Zero;

                var retVal = CuResult.ErrorInvalidValue;

                try {
                    var arraySize = 0;
                    if (nodeParams.ExtSemArray != null && nodeParams.ParamsArray != null) {
                        if (nodeParams.ExtSemArray.Length != nodeParams.ParamsArray.Length) {
                            return CuResult.ErrorInvalidValue;
                        }

                        arraySize = nodeParams.ExtSemArray.Length;
                    }

                    var paramsSize = Marshal.SizeOf(typeof(CudaExternalSemaphoreSignalParams));

                    mainPtr = Marshal.AllocHGlobal(2 * IntPtr.Size + sizeof(int));

                    if (arraySize > 0) {
                        extSemPtr = Marshal.AllocHGlobal(arraySize * IntPtr.Size);
                        paramsPtr = Marshal.AllocHGlobal(arraySize * paramsSize);
                    }

                    Marshal.WriteIntPtr(mainPtr + 0, extSemPtr);
                    Marshal.WriteIntPtr(mainPtr + IntPtr.Size, paramsPtr);
                    Marshal.WriteInt32(mainPtr + 2 * IntPtr.Size, arraySize);

                    for (var i = 0; i < arraySize; i++) {
                        Marshal.StructureToPtr(nodeParams.ExtSemArray[i], extSemPtr + (IntPtr.Size * i), false);
                        Marshal.StructureToPtr(nodeParams.ParamsArray[i], paramsPtr + (paramsSize * i), false);
                    }

                    retVal = cuGraphExternalSemaphoresSignalNodeSetParamsInternal(hNode, mainPtr);
                }
                catch {
                    retVal = CuResult.ErrorInvalidValue;
                }
                finally {
                    if (mainPtr != IntPtr.Zero) {
                        Marshal.FreeHGlobal(mainPtr);
                    }

                    if (extSemPtr != IntPtr.Zero) {
                        Marshal.FreeHGlobal(extSemPtr);
                    }

                    if (paramsPtr != IntPtr.Zero) {
                        Marshal.FreeHGlobal(paramsPtr);
                    }
                }

                return retVal;
            }

            [DllImport(CudaDriverApiDllName, EntryPoint = "cuGraphExternalSemaphoresSignalNodeSetParams")]
            static extern CuResult cuGraphExternalSemaphoresSignalNodeSetParamsInternal(CUgraphNode hNode, IntPtr nodeParams);
            public static CuResult CuGraphAddExternalSemaphoresWaitNode(ref CUgraphNode phGraphNode, CUgraph hGraph, CUgraphNode[] dependencies, SizeT numDependencies, CudaExtSemWaitNodeParams nodeParams)
            {
                var extSemPtr = IntPtr.Zero;
                var paramsPtr = IntPtr.Zero;
                var mainPtr = IntPtr.Zero;

                var retVal = CuResult.ErrorInvalidValue;

                try {
                    var arraySize = 0;
                    if (nodeParams.ExtSemArray != null && nodeParams.ParamsArray != null) {
                        if (nodeParams.ExtSemArray.Length != nodeParams.ParamsArray.Length) {
                            return CuResult.ErrorInvalidValue;
                        }

                        arraySize = nodeParams.ExtSemArray.Length;
                    }

                    var paramsSize = Marshal.SizeOf(typeof(CudaExternalSemaphoreWaitParams));

                    mainPtr = Marshal.AllocHGlobal(2 * IntPtr.Size + sizeof(int));

                    if (arraySize > 0) {
                        extSemPtr = Marshal.AllocHGlobal(arraySize * IntPtr.Size);
                        paramsPtr = Marshal.AllocHGlobal(arraySize * paramsSize);
                    }

                    Marshal.WriteIntPtr(mainPtr + 0, extSemPtr);
                    Marshal.WriteIntPtr(mainPtr + IntPtr.Size, paramsPtr);
                    Marshal.WriteInt32(mainPtr + 2 * IntPtr.Size, arraySize);

                    for (var i = 0; i < arraySize; i++) {
                        Marshal.StructureToPtr(nodeParams.ExtSemArray[i], extSemPtr + (IntPtr.Size * i), false);
                        Marshal.StructureToPtr(nodeParams.ParamsArray[i], paramsPtr + (paramsSize * i), false);
                    }

                    retVal = cuGraphAddExternalSemaphoresWaitNodeInternal(ref phGraphNode, hGraph, dependencies, numDependencies, mainPtr);
                }
                catch {
                    retVal = CuResult.ErrorInvalidValue;
                }
                finally {
                    if (mainPtr != IntPtr.Zero) {
                        Marshal.FreeHGlobal(mainPtr);
                    }

                    if (extSemPtr != IntPtr.Zero) {
                        Marshal.FreeHGlobal(extSemPtr);
                    }

                    if (paramsPtr != IntPtr.Zero) {
                        Marshal.FreeHGlobal(paramsPtr);
                    }
                }

                return retVal;
            }

            [DllImport(CudaDriverApiDllName, EntryPoint = "cuGraphAddExternalSemaphoresWaitNode")]
            static extern CuResult cuGraphAddExternalSemaphoresWaitNodeInternal(ref CUgraphNode phGraphNode, CUgraph hGraph, CUgraphNode[] dependencies, SizeT numDependencies, IntPtr nodeParams);
            public static CuResult CuGraphExternalSemaphoresWaitNodeGetParams(CUgraphNode hNode, CudaExtSemWaitNodeParams paramsOut)
            {
                var mainPtr = IntPtr.Zero;

                var retVal = CuResult.ErrorInvalidValue;

                try {
                    var arraySize = 0;
                    var paramsSize = Marshal.SizeOf(typeof(CudaExternalSemaphoreWaitParams));
                    mainPtr = Marshal.AllocHGlobal(2 * IntPtr.Size + sizeof(int));

                    Marshal.WriteIntPtr(mainPtr + 0, IntPtr.Zero);
                    Marshal.WriteIntPtr(mainPtr + IntPtr.Size, IntPtr.Zero);
                    Marshal.WriteInt32(mainPtr + 2 * IntPtr.Size, arraySize);

                    retVal = cuGraphExternalSemaphoresWaitNodeGetParamsInternal(hNode, mainPtr);

                    var length = Marshal.ReadInt32(mainPtr + 2 * IntPtr.Size);

                    var array1 = new CUexternalSemaphore[length];
                    var array2 = new CudaExternalSemaphoreWaitParams[length];
                    var ptr1 = Marshal.ReadIntPtr(mainPtr);
                    var ptr2 = Marshal.ReadIntPtr(mainPtr + IntPtr.Size);

                    for (var i = 0; i < length; i++) {
                        array1[i] = (CUexternalSemaphore)Marshal.PtrToStructure(ptr1 + (IntPtr.Size * i), typeof(CUexternalSemaphore));
                        array2[i] = (CudaExternalSemaphoreWaitParams)Marshal.PtrToStructure(ptr2 + (paramsSize * i), typeof(CudaExternalSemaphoreWaitParams));
                    }

                    paramsOut.ExtSemArray = array1;
                    paramsOut.ParamsArray = array2;
                }
                catch {
                    retVal = CuResult.ErrorInvalidValue;
                }
                finally {
                    if (mainPtr != IntPtr.Zero) {
                        Marshal.FreeHGlobal(mainPtr);
                    }
                }

                return retVal;
            }

            [DllImport(CudaDriverApiDllName, EntryPoint = "cuGraphExternalSemaphoresWaitNodeGetParams")]
            static extern CuResult cuGraphExternalSemaphoresWaitNodeGetParamsInternal(CUgraphNode hNode, IntPtr paramsOut);
            public static CuResult CuGraphExternalSemaphoresWaitNodeSetParams(CUgraphNode hNode, CudaExtSemWaitNodeParams nodeParams)
            {
                var extSemPtr = IntPtr.Zero;
                var paramsPtr = IntPtr.Zero;
                var mainPtr = IntPtr.Zero;

                var retVal = CuResult.ErrorInvalidValue;

                try {
                    var arraySize = 0;
                    if (nodeParams.ExtSemArray != null && nodeParams.ParamsArray != null) {
                        if (nodeParams.ExtSemArray.Length != nodeParams.ParamsArray.Length) {
                            return CuResult.ErrorInvalidValue;
                        }

                        arraySize = nodeParams.ExtSemArray.Length;
                    }

                    var paramsSize = Marshal.SizeOf(typeof(CudaExternalSemaphoreWaitParams));

                    mainPtr = Marshal.AllocHGlobal(2 * IntPtr.Size + sizeof(int));

                    if (arraySize > 0) {
                        extSemPtr = Marshal.AllocHGlobal(arraySize * IntPtr.Size);
                        paramsPtr = Marshal.AllocHGlobal(arraySize * paramsSize);
                    }

                    Marshal.WriteIntPtr(mainPtr + 0, extSemPtr);
                    Marshal.WriteIntPtr(mainPtr + IntPtr.Size, paramsPtr);
                    Marshal.WriteInt32(mainPtr + 2 * IntPtr.Size, arraySize);

                    for (var i = 0; i < arraySize; i++) {
                        Marshal.StructureToPtr(nodeParams.ExtSemArray[i], extSemPtr + (IntPtr.Size * i), false);
                        Marshal.StructureToPtr(nodeParams.ParamsArray[i], paramsPtr + (paramsSize * i), false);
                    }

                    retVal = cuGraphExternalSemaphoresWaitNodeSetParamsInternal(hNode, mainPtr);
                }
                catch {
                    retVal = CuResult.ErrorInvalidValue;
                }
                finally {
                    if (mainPtr != IntPtr.Zero) {
                        Marshal.FreeHGlobal(mainPtr);
                    }

                    if (extSemPtr != IntPtr.Zero) {
                        Marshal.FreeHGlobal(extSemPtr);
                    }

                    if (paramsPtr != IntPtr.Zero) {
                        Marshal.FreeHGlobal(paramsPtr);
                    }
                }

                return retVal;
            }
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuGraphExternalSemaphoresWaitNodeSetParams")]
            static extern CuResult cuGraphExternalSemaphoresWaitNodeSetParamsInternal(CUgraphNode hNode, IntPtr nodeParams);
            public static CuResult CuGraphAddBatchMemOpNode(ref CUgraphNode phGraphNode, CUgraph hGraph, CUgraphNode[] dependencies, SizeT numDependencies, ref CudaBatchMemOpNodeParams nodeParams)
            {
                var retVal = CuResult.ErrorInvalidValue;
                var parameters = new CudaBatchMemOpNodeParamsInternal();
                parameters.Ctx = nodeParams.Ctx;
                parameters.Count = 0;
                parameters.ParamArray = IntPtr.Zero;
                parameters.Flags = nodeParams.Flags;

                try {
                    var arraySize = 0;
                    if (nodeParams.ParamArray != null) {
                        arraySize = nodeParams.ParamArray.Length;
                        parameters.Count = (uint)arraySize;
                    }

                    var paramsSize = Marshal.SizeOf(typeof(CUstreamBatchMemOpParams));

                    if (arraySize > 0) {
                        parameters.ParamArray = Marshal.AllocHGlobal(arraySize * paramsSize);
                    }

                    for (var i = 0; i < arraySize; i++) {
                        Marshal.StructureToPtr(nodeParams.ParamArray[i], parameters.ParamArray + (paramsSize * i), false);
                    }

                    retVal = cuGraphAddBatchMemOpNodeInternal(ref phGraphNode, hGraph, dependencies, numDependencies, ref parameters);
                }
                catch {
                    retVal = CuResult.ErrorInvalidValue;
                }
                finally {
                    if (parameters.ParamArray != IntPtr.Zero) {
                        Marshal.FreeHGlobal(parameters.ParamArray);
                    }
                }

                return retVal;
            }

            [DllImport(CudaDriverApiDllName, EntryPoint = "cuGraphAddBatchMemOpNode")]
            static extern CuResult cuGraphAddBatchMemOpNodeInternal(ref CUgraphNode phGraphNode, CUgraph hGraph, CUgraphNode[] dependencies, SizeT numDependencies, ref CudaBatchMemOpNodeParamsInternal nodeParams);
            public static CuResult CuGraphBatchMemOpNodeGetParams(CUgraphNode hNode, ref CudaBatchMemOpNodeParams nodeParamsOut)
            {
                var retVal = CuResult.ErrorInvalidValue;
                var parameters = new CudaBatchMemOpNodeParamsInternal();

                try {
                    retVal = cuGraphBatchMemOpNodeGetParamsInternal(hNode, ref parameters);

                    var arraySize = (int)parameters.Count;

                    var paramsSize = Marshal.SizeOf(typeof(CUstreamBatchMemOpParams));

                    if (arraySize > 0) {
                        nodeParamsOut.ParamArray = new CUstreamBatchMemOpParams[arraySize];

                        for (var i = 0; i < arraySize; i++) {
                            nodeParamsOut.ParamArray[i] = (CUstreamBatchMemOpParams)Marshal.PtrToStructure(parameters.ParamArray + (paramsSize * i), typeof(CUstreamBatchMemOpParams));
                        }
                    }

                    nodeParamsOut.Ctx = parameters.Ctx;
                    nodeParamsOut.Flags = parameters.Flags;
                }
                catch {
                    retVal = CuResult.ErrorInvalidValue;
                }
                finally {
                }

                return retVal;
            }

            [DllImport(CudaDriverApiDllName, EntryPoint = "cuGraphBatchMemOpNodeGetParams")]
            static extern CuResult cuGraphBatchMemOpNodeGetParamsInternal(CUgraphNode hNode, ref CudaBatchMemOpNodeParamsInternal nodeParamsOut);
            public static CuResult CuGraphBatchMemOpNodeSetParams(CUgraphNode hNode, ref CudaBatchMemOpNodeParams nodeParams)
            {
                var retVal = CuResult.ErrorInvalidValue;
                var parameters = new CudaBatchMemOpNodeParamsInternal();
                parameters.Ctx = nodeParams.Ctx;
                parameters.Count = 0;
                parameters.ParamArray = IntPtr.Zero;
                parameters.Flags = nodeParams.Flags;

                try {
                    var arraySize = 0;
                    if (nodeParams.ParamArray != null) {
                        arraySize = nodeParams.ParamArray.Length;
                        parameters.Count = (uint)arraySize;
                    }

                    var paramsSize = Marshal.SizeOf(typeof(CUstreamBatchMemOpParams));

                    if (arraySize > 0) {
                        parameters.ParamArray = Marshal.AllocHGlobal(arraySize * paramsSize);
                    }

                    for (var i = 0; i < arraySize; i++) {
                        Marshal.StructureToPtr(nodeParams.ParamArray[i], parameters.ParamArray + (paramsSize * i), false);
                    }

                    retVal = cuGraphBatchMemOpNodeSetParamsInternal(hNode, ref parameters);
                }
                catch {
                    retVal = CuResult.ErrorInvalidValue;
                }
                finally {
                    if (parameters.ParamArray != IntPtr.Zero) {
                        Marshal.FreeHGlobal(parameters.ParamArray);
                    }
                }

                return retVal;
            }

            [DllImport(CudaDriverApiDllName, EntryPoint = "cuGraphBatchMemOpNodeSetParams")]
            static extern CuResult cuGraphBatchMemOpNodeSetParamsInternal(CUgraphNode hNode, ref CudaBatchMemOpNodeParamsInternal nodeParams);
            public static CuResult CuGraphExecBatchMemOpNodeSetParams(CUgraphExec hGraphExec, CUgraphNode hNode, ref CudaBatchMemOpNodeParams nodeParams)
            {
                var retVal = CuResult.ErrorInvalidValue;
                var parameters = new CudaBatchMemOpNodeParamsInternal();
                parameters.Ctx = nodeParams.Ctx;
                parameters.Count = 0;
                parameters.ParamArray = IntPtr.Zero;
                parameters.Flags = nodeParams.Flags;

                try {
                    var arraySize = 0;
                    if (nodeParams.ParamArray != null) {
                        arraySize = nodeParams.ParamArray.Length;
                        parameters.Count = (uint)arraySize;
                    }

                    var paramsSize = Marshal.SizeOf(typeof(CUstreamBatchMemOpParams));

                    if (arraySize > 0) {
                        parameters.ParamArray = Marshal.AllocHGlobal(arraySize * paramsSize);
                    }

                    for (var i = 0; i < arraySize; i++) {
                        Marshal.StructureToPtr(nodeParams.ParamArray[i], parameters.ParamArray + (paramsSize * i), false);
                    }

                    retVal = cuGraphExecBatchMemOpNodeSetParamsInternal(hGraphExec, hNode, ref parameters);
                }
                catch {
                    retVal = CuResult.ErrorInvalidValue;
                }
                finally {
                    if (parameters.ParamArray != IntPtr.Zero) {
                        Marshal.FreeHGlobal(parameters.ParamArray);
                    }
                }

                return retVal;
            }

            [DllImport(CudaDriverApiDllName, EntryPoint = "cuGraphExecBatchMemOpNodeSetParams")]
            static extern CuResult cuGraphExecBatchMemOpNodeSetParamsInternal(CUgraphExec hGraphExec, CUgraphNode hNode, ref CudaBatchMemOpNodeParamsInternal nodeParams);
            public static CuResult CuGraphAddMemAllocNode(ref CUgraphNode phGraphNode, CUgraph hGraph, CUgraphNode[] dependencies, SizeT numDependencies, ref CudaMemAllocNodeParams nodeParams)
            {
                var retVal = CuResult.ErrorInvalidValue;
                var parameters = new CudaMemAllocNodeParamsInternal();
                parameters.poolProps = nodeParams.poolProps;
                parameters.dptr = nodeParams.dptr;
                parameters.bytesize = nodeParams.bytesize;
                parameters.accessDescCount = 0;
                parameters.accessDescs = IntPtr.Zero;

                try {
                    var arraySize = 0;
                    if (nodeParams.accessDescs != null) {
                        arraySize = nodeParams.accessDescs.Length;
                        parameters.accessDescCount = (uint)arraySize;
                    }

                    var paramsSize = Marshal.SizeOf(typeof(CUmemAccessDesc));

                    if (arraySize > 0) {
                        parameters.accessDescs = Marshal.AllocHGlobal(arraySize * paramsSize);
                    }

                    for (var i = 0; i < arraySize; i++) {
                        Marshal.StructureToPtr(nodeParams.accessDescs[i], parameters.accessDescs + (paramsSize * i), false);
                    }

                    retVal = cuGraphAddMemAllocNodeInternal(ref phGraphNode, hGraph, dependencies, numDependencies, ref parameters);
                    nodeParams.dptr = parameters.dptr;
                }
                catch {
                    retVal = CuResult.ErrorInvalidValue;
                }
                finally {
                    if (parameters.accessDescs != IntPtr.Zero) {
                        Marshal.FreeHGlobal(parameters.accessDescs);
                    }
                }

                return retVal;
            }

            [DllImport(CudaDriverApiDllName)]
            static extern CuResult cuGraphAddMemAllocNodeInternal(ref CUgraphNode phGraphNode, CUgraph hGraph, CUgraphNode[] dependencies, SizeT numDependencies, ref CudaMemAllocNodeParamsInternal nodeParams);
            public static CuResult CuGraphMemAllocNodeGetParams(CUgraphNode hNode, ref CudaMemAllocNodeParams paramsOut)
            {
                var retVal = CuResult.ErrorInvalidValue;
                var parameters = new CudaMemAllocNodeParamsInternal();

                try {
                    retVal = cuGraphMemAllocNodeGetParamsInternal(hNode, ref parameters);

                    var arraySize = (int)parameters.accessDescCount;

                    var paramsSize = Marshal.SizeOf(typeof(CUmemAccessDesc));

                    if (arraySize > 0) {
                        paramsOut.accessDescs = new CUmemAccessDesc[arraySize];

                        for (var i = 0; i < arraySize; i++) {
                            paramsOut.accessDescs[i] = (CUmemAccessDesc)Marshal.PtrToStructure(parameters.accessDescs + (paramsSize * i), typeof(CUmemAccessDesc));
                        }
                    }

                    paramsOut.poolProps = parameters.poolProps;
                    paramsOut.dptr = parameters.dptr;
                    paramsOut.bytesize = parameters.bytesize;
                }
                catch {
                    retVal = CuResult.ErrorInvalidValue;
                }
                finally {
                }

                return retVal;
            }

            [DllImport(CudaDriverApiDllName)]
            static extern CuResult cuGraphMemAllocNodeGetParamsInternal(CUgraphNode hNode, ref CudaMemAllocNodeParamsInternal paramsOut);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphAddMemFreeNode(ref CUgraphNode phGraphNode, CUgraph hGraph, CUgraphNode[] dependencies, SizeT numDependencies, CUdeviceptr dptr);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphMemFreeNodeGetParams(CUgraphNode hNode, ref CUdeviceptr dptrOut);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuDeviceGraphMemTrim(CUdevice device);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuDeviceGetGraphMemAttribute(CUdevice device, CUgraphMemAttribute attr, ref ulong value);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuDeviceSetGraphMemAttribute(CUdevice device, CUgraphMemAttribute attr, ref ulong value);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphClone(ref CUgraph phGraphClone, CUgraph originalGraph);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphNodeFindInClone(ref CUgraphNode phNode, CUgraphNode hOriginalNode, CUgraph hClonedGraph);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphNodeGetType(CUgraphNode hNode, ref CUgraphNodeType type);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphGetNodes(CUgraph hGraph, [In, Out] CUgraphNode[] nodes, ref SizeT numNodes);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphGetRootNodes(CUgraph hGraph, [In, Out] CUgraphNode[] rootNodes, ref SizeT numRootNodes);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphGetEdges(CUgraph hGraph, [In, Out] CUgraphNode[] from, [In, Out] CUgraphNode[] to, ref SizeT numEdges);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphNodeGetDependencies(CUgraphNode hNode, [In, Out] CUgraphNode[]? dependencies, ref SizeT numDependencies);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphNodeGetDependentNodes(CUgraphNode hNode, [In, Out] CUgraphNode[]? dependentNodes, ref SizeT numDependentNodes);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphAddDependencies(CUgraph hGraph, CUgraphNode[] from, CUgraphNode[] to, SizeT numDependencies);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphRemoveDependencies(CUgraph hGraph, CUgraphNode[] from, CUgraphNode[] to, SizeT numDependencies);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphDestroyNode(CUgraphNode hNode);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuGraphInstantiateWithFlags")]
            public static extern CuResult cuGraphInstantiate(ref CUgraphExec phGraphExec, CUgraph hGraph, CUgraphInstantiateFlags flags);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuGraphInstantiateWithParams" + CudaPtsz)]
            public static extern CuResult cuGraphInstantiateWithParams(ref CUgraphExec phGraphExec, CUgraph hGraph, ref CudaGraphInstantiateParams instantiateParams);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphExecGetFlags(CUgraphExec hGraphExec, ref CUgraphInstantiateFlags flags);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuGraphExecKernelNodeSetParams_v2")]
            public static extern CuResult cuGraphExecKernelNodeSetParams(CUgraphExec hGraphExec, CUgraphNode hNode, ref CudaKernelNodeParams nodeParams);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphExecMemcpyNodeSetParams(CUgraphExec hGraphExec, CUgraphNode hNode, ref CudaMemCpy3D copyParams, CUcontext ctx);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphExecMemsetNodeSetParams(CUgraphExec hGraphExec, CUgraphNode hNode, ref CudaMemsetNodeParams memsetParams, CUcontext ctx);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphExecHostNodeSetParams(CUgraphExec hGraphExec, CUgraphNode hNode, ref CudaHostNodeParams nodeParams);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphExecChildGraphNodeSetParams(CUgraphExec hGraphExec, CUgraphNode hNode, CUgraph childGraph);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphExecEventRecordNodeSetEvent(CUgraphExec hGraphExec, CUgraphNode hNode, CUevent @event);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphExecEventWaitNodeSetEvent(CUgraphExec hGraphExec, CUgraphNode hNode, CUevent @event);
            public static CuResult CuGraphExecExternalSemaphoresSignalNodeSetParams(CUgraphExec hGraphExec, CUgraphNode hNode, CudaExtSemSignalNodeParams nodeParams)
            {
                var extSemPtr = IntPtr.Zero;
                var paramsPtr = IntPtr.Zero;
                var mainPtr = IntPtr.Zero;

                var retVal = CuResult.ErrorInvalidValue;

                try {
                    var arraySize = 0;
                    if (nodeParams.ExtSemArray != null && nodeParams.ParamsArray != null) {
                        if (nodeParams.ExtSemArray.Length != nodeParams.ParamsArray.Length) {
                            return CuResult.ErrorInvalidValue;
                        }

                        arraySize = nodeParams.ExtSemArray.Length;
                    }

                    var paramsSize = Marshal.SizeOf(typeof(CudaExternalSemaphoreSignalParams));

                    mainPtr = Marshal.AllocHGlobal(2 * IntPtr.Size + sizeof(int));

                    if (arraySize > 0) {
                        extSemPtr = Marshal.AllocHGlobal(arraySize * IntPtr.Size);
                        paramsPtr = Marshal.AllocHGlobal(arraySize * paramsSize);
                    }

                    Marshal.WriteIntPtr(mainPtr + 0, extSemPtr);
                    Marshal.WriteIntPtr(mainPtr + IntPtr.Size, paramsPtr);
                    Marshal.WriteInt32(mainPtr + 2 * IntPtr.Size, arraySize);

                    for (var i = 0; i < arraySize; i++) {
                        Marshal.StructureToPtr(nodeParams.ExtSemArray[i], extSemPtr + (IntPtr.Size * i), false);
                        Marshal.StructureToPtr(nodeParams.ParamsArray[i], paramsPtr + (paramsSize * i), false);
                    }

                    retVal = cuGraphExecExternalSemaphoresSignalNodeSetParamsInternal(hGraphExec, hNode, mainPtr);
                }
                catch {
                    retVal = CuResult.ErrorInvalidValue;
                }
                finally {
                    if (mainPtr != IntPtr.Zero) {
                        Marshal.FreeHGlobal(mainPtr);
                    }

                    if (extSemPtr != IntPtr.Zero) {
                        Marshal.FreeHGlobal(extSemPtr);
                    }

                    if (paramsPtr != IntPtr.Zero) {
                        Marshal.FreeHGlobal(paramsPtr);
                    }
                }

                return retVal;
            }

            [DllImport(CudaDriverApiDllName, EntryPoint = "cuGraphExecExternalSemaphoresSignalNodeSetParams")]
            static extern CuResult cuGraphExecExternalSemaphoresSignalNodeSetParamsInternal(CUgraphExec hGraphExec, CUgraphNode hNode, IntPtr nodeParams);
            public static CuResult CuGraphExecExternalSemaphoresWaitNodeSetParams(CUgraphExec hGraphExec, CUgraphNode hNode, CudaExtSemWaitNodeParams nodeParams)
            {
                var extSemPtr = IntPtr.Zero;
                var paramsPtr = IntPtr.Zero;
                var mainPtr = IntPtr.Zero;

                var retVal = CuResult.ErrorInvalidValue;

                try {
                    var arraySize = 0;
                    if (nodeParams.ExtSemArray != null && nodeParams.ParamsArray != null) {
                        if (nodeParams.ExtSemArray.Length != nodeParams.ParamsArray.Length) {
                            return CuResult.ErrorInvalidValue;
                        }

                        arraySize = nodeParams.ExtSemArray.Length;
                    }

                    var paramsSize = Marshal.SizeOf(typeof(CudaExternalSemaphoreWaitParams));

                    mainPtr = Marshal.AllocHGlobal(2 * IntPtr.Size + sizeof(int));

                    if (arraySize > 0) {
                        extSemPtr = Marshal.AllocHGlobal(arraySize * IntPtr.Size);
                        paramsPtr = Marshal.AllocHGlobal(arraySize * paramsSize);
                    }

                    Marshal.WriteIntPtr(mainPtr + 0, extSemPtr);
                    Marshal.WriteIntPtr(mainPtr + IntPtr.Size, paramsPtr);
                    Marshal.WriteInt32(mainPtr + 2 * IntPtr.Size, arraySize);

                    for (var i = 0; i < arraySize; i++) {
                        Marshal.StructureToPtr(nodeParams.ExtSemArray[i], extSemPtr + (IntPtr.Size * i), false);
                        Marshal.StructureToPtr(nodeParams.ParamsArray[i], paramsPtr + (paramsSize * i), false);
                    }

                    retVal = cuGraphExecExternalSemaphoresWaitNodeSetParamsInternal(hGraphExec, hNode, mainPtr);
                }
                catch {
                    retVal = CuResult.ErrorInvalidValue;
                }
                finally {
                    if (mainPtr != IntPtr.Zero) {
                        Marshal.FreeHGlobal(mainPtr);
                    }

                    if (extSemPtr != IntPtr.Zero) {
                        Marshal.FreeHGlobal(extSemPtr);
                    }

                    if (paramsPtr != IntPtr.Zero) {
                        Marshal.FreeHGlobal(paramsPtr);
                    }
                }

                return retVal;
            }

            [DllImport(CudaDriverApiDllName, EntryPoint = "cuGraphExecExternalSemaphoresWaitNodeSetParams")]
            static extern CuResult cuGraphExecExternalSemaphoresWaitNodeSetParamsInternal(CUgraphExec hGraphExec, CUgraphNode hNode, IntPtr nodeParams);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphNodeSetEnabled(CUgraphExec hGraphExec, CUgraphNode hNode, uint isEnabled);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphNodeGetEnabled(CUgraphExec hGraphExec, CUgraphNode hNode, ref int isEnabled);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuGraphUpload" + CudaPtsz)]
            public static extern CuResult cuGraphUpload(CUgraphExec hGraphExec, CUstream hStream);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuGraphLaunch" + CudaPtsz)]
            public static extern CuResult cuGraphLaunch(CUgraphExec hGraphExec, CUstream hStream);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphExecDestroy(CUgraphExec hGraphExec);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphDestroy(CUgraph hGraph);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuGraphExecUpdate_v2")]
            public static extern CuResult cuGraphExecUpdate(CUgraphExec hGraphExec, CUgraph hGraph, ref CUgraphExecUpdateResultInfo resultInfo);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphKernelNodeCopyAttributes(CUgraphNode dst, CUgraphNode src);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphKernelNodeGetAttribute(CUgraphNode hNode, CUkernelNodeAttrId attr, ref CUkernelNodeAttrValue valueOut);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphKernelNodeSetAttribute(CUgraphNode hNode, CUkernelNodeAttrId attr, ref CUkernelNodeAttrValue value);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphDebugDotPrint(CUgraph hGraph, [MarshalAs(UnmanagedType.LPStr)] string path, CUgraphDebugDotFlags flags);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuUserObjectCreate(ref CUuserObject objectOut, IntPtr ptr, CUhostFn destroy,
                uint initialRefcount, CUuserObjectFlags flags);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuUserObjectRetain(CUuserObject obj, uint count);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuUserObjectRelease(CUuserObject obj, uint count);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphRetainUserObject(CUgraph graph, CUuserObject obj, uint count, CUuserObjectRetainFlags flags);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphReleaseUserObject(CUgraph graph, CUuserObject obj, uint count);
        }
        public static class RdmaDirect
        {
            static RdmaDirect()
            {
                DriverApiNativeMethods.Init();
            }
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuFlushGPUDirectRDMAWrites(CUflushGpuDirectRdmaWritesTarget target, CUflushGpuDirectRdmaWritesScope scope);
        }
        public static class TensorCoreManagment
        {
            static TensorCoreManagment()
            {
                DriverApiNativeMethods.Init();
            }
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuTensorMapEncodeTiled(ref CUtensorMap tensorMap, CUtensorMapDataType tensorDataType, uint tensorRank, CUdeviceptr globalAddress, ulong[] globalDim, ulong[] globalStrides, uint[] boxDim, uint[] elementStrides, CUtensorMapInterleave interleave, CUtensorMapSwizzle swizzle, CUtensorMapL2Promotion l2Promotion, CUtensorMapFloatOoBfill oobFill);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuTensorMapEncodeIm2col(ref CUtensorMap tensorMap, CUtensorMapDataType tensorDataType, uint tensorRank, CUdeviceptr globalAddress, ulong[] globalDim, ulong[] globalStrides, int[] pixelBoxLowerCorner, int[] pixelBoxUpperCorner, uint channelsPerPixel, uint pixelsPerColumn, uint[] elementStrides, CUtensorMapInterleave interleave, CUtensorMapSwizzle swizzle, CUtensorMapL2Promotion l2Promotion, CUtensorMapFloatOoBfill oobFill);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuTensorMapReplaceAddress(ref CUtensorMap tensorMap, CUdeviceptr globalAddress);
        }
    }
}