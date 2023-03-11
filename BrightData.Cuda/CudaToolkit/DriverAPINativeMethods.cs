using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using BrightData.Cuda.CudaToolkit.Types;

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
        public static Version Version => new(12, 0);
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
            public static extern CuResult cuDeviceGet(ref CuDevice device, int ordinal);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuDeviceGetCount(ref int count);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuDeviceGetName([Out] byte[] name, int len, CuDevice dev);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuDeviceGetUuid(ref CuUuid uuid, CuDevice dev);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuDeviceGetUuid_v2(ref CuUuid uuid, CuDevice dev);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuDeviceGetLuid(ref Luid luid, ref uint deviceNodeMask, CuDevice dev);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuDeviceTotalMem_v2(ref SizeT bytes, CuDevice dev);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuDeviceGetTexture1DLinearMaxWidth(ref SizeT maxWidthInElements, CuArrayFormat format, uint numChannels, CuDevice dev);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuDeviceGetAttribute(ref int pi, CuDeviceAttribute attrib, CuDevice dev);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuDeviceGetNvSciSyncAttributes(IntPtr nvSciSyncAttrList, CuDevice dev, NvSciSyncAttr flags);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuDeviceSetMemPool(CuDevice dev, CuMemoryPool pool);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuDeviceGetMemPool(ref CuMemoryPool pool, CuDevice dev);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuDeviceGetDefaultMemPool(ref CuMemoryPool poolOut, CuDevice dev);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuDeviceGetByPCIBusId(ref CuDevice dev, [In, Out] byte[] pciBusId);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuDeviceGetPCIBusId([In, Out] byte[] pciBusId, int len, CuDevice dev);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuIpcGetEventHandle(ref CuIpcEventHandle pHandle, CuEvent cuevent);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuIpcOpenEventHandle(ref CuEvent phEvent, CuIpcEventHandle handle);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuIpcGetMemHandle(ref CuIpcMemHandle pHandle, CuDevicePtr dptr);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuIpcOpenMemHandle_v2")]
            public static extern CuResult cuIpcOpenMemHandle(ref CuDevicePtr pdptr, CuIpcMemHandle handle, uint flags);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuIpcCloseMemHandle(CuDevicePtr dptr);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuDeviceGetExecAffinitySupport(ref int pi, CuExecAffinityType type, CuDevice dev);
        }
        public static class ContextManagement
        {
            static ContextManagement()
            {
                DriverApiNativeMethods.Init();
            }
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuCtxCreate_v2(ref CuContext pctx, CuCtxFlags flags, CuDevice dev);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuCtxCreate_v3(ref CuContext pctx, CuExecAffinityParam[] paramsArray, int numParams, CuCtxFlags flags, CuDevice dev);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuCtxDestroy_v2(CuContext ctx);
            [DllImport(CudaDriverApiDllName)]
            [Obsolete(CudaObsolet92)]
            public static extern CuResult cuCtxAttach(ref CuContext pctx, CuCtxAttachFlags flags);
            [DllImport(CudaDriverApiDllName)]
            [Obsolete(CudaObsolet92)]
            public static extern CuResult cuCtxDetach([In] CuContext ctx);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuCtxPushCurrent_v2([In] CuContext ctx);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuCtxPopCurrent_v2(ref CuContext pctx);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuCtxSetCurrent([In] CuContext ctx);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuCtxGetCurrent(ref CuContext pctx);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuCtxGetDevice(ref CuDevice device);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuCtxSynchronize();
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuCtxGetApiVersion(CuContext ctx, ref uint version);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuCtxGetCacheConfig(ref CuFuncCache pconfig);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuCtxSetCacheConfig(CuFuncCache config);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuCtxGetSharedMemConfig(ref CuSharedConfig pConfig);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuCtxSetSharedMemConfig(CuSharedConfig config);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuCtxGetStreamPriorityRange(ref int leastPriority, ref int greatestPriority);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuCtxResetPersistingL2Cache();
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuCtxGetExecAffinity(ref CuExecAffinityParam pExecAffinity, CuExecAffinityType type);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuCtxGetFlags(ref CuCtxFlags flags);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuCtxGetId(CuContext ctx, ref ulong ctxId);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuDevicePrimaryCtxRetain(ref CuContext pctx, CuDevice dev);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuDevicePrimaryCtxRelease_v2")]
            public static extern CuResult cuDevicePrimaryCtxRelease(CuDevice dev);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuDevicePrimaryCtxSetFlags_v2")]
            public static extern CuResult cuDevicePrimaryCtxSetFlags(CuDevice dev, CuCtxFlags flags);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuDevicePrimaryCtxGetState(CuDevice dev, ref CuCtxFlags flags, ref int active);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuDevicePrimaryCtxReset_v2")]
            public static extern CuResult cuDevicePrimaryCtxReset(CuDevice dev);
        }
        public static class ModuleManagement
        {
            static ModuleManagement()
            {
                DriverApiNativeMethods.Init();
            }
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuModuleLoad(ref CuModule module, string fname);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuModuleLoadData(ref CuModule module, [In] byte[] image);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuModuleLoadDataEx(ref CuModule module, [In] byte[] image, uint numOptions, [In] CuJitOption[]? options, [In, Out] IntPtr[]? optionValues);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuModuleLoadFatBinary(ref CuModule module, [In] byte[] fatCubin);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuModuleUnload(CuModule hmod);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuModuleGetFunction(ref CuFunction hfunc, CuModule hmod, string name);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuModuleGetGlobal_v2(ref CuDevicePtr dptr, ref SizeT bytes, CuModule hmod, string name);
            [DllImport(CudaDriverApiDllName)]
            [Obsolete(CudaObsolet12)]
            public static extern CuResult cuModuleGetTexRef(ref CuTexRef pTexRef, CuModule hmod, string name);
            [DllImport(CudaDriverApiDllName)]
            [Obsolete(CudaObsolet12)]
            public static extern CuResult cuModuleGetSurfRef(ref CuSurfRef pSurfRef, CuModule hmod, string name);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuLinkCreate_v2")]
            public static extern CuResult cuLinkCreate(uint numOptions, CuJitOption[] options, [In, Out] IntPtr[] optionValues, ref CulinkState stateOut);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuLinkAddData_v2")]
            public static extern CuResult cuLinkAddData(CulinkState state, CuJitInputType type, byte[] data, SizeT size, [MarshalAs(UnmanagedType.LPStr)] string name,
                uint numOptions, CuJitOption[] options, IntPtr[] optionValues);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuLinkAddFile_v2")]
            public static extern CuResult cuLinkAddFile(CulinkState state, CuJitInputType type, string path, uint numOptions, CuJitOption[] options, IntPtr[] optionValues);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuLinkComplete(CulinkState state, ref IntPtr cubinOut, ref SizeT sizeOut);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuLinkDestroy(CulinkState state);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuModuleGetLoadingMode(ref CuModuleLoadingMode mode);
        }
        public static class LibraryManagement
        {
            static LibraryManagement()
            {
                DriverApiNativeMethods.Init();
            }
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuLibraryLoadData(ref CuLibrary library, byte[] code,
                CuJitOption[] jitOptions, IntPtr[] jitOptionsValues, uint numJitOptions,
                CuLibraryOption[] libraryOptions, IntPtr[] libraryOptionValues, uint numLibraryOptions);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuLibraryLoadData(ref CuLibrary library, IntPtr code,
                CuJitOption[] jitOptions, IntPtr[] jitOptionsValues, uint numJitOptions,
                CuLibraryOption[] libraryOptions, IntPtr[] libraryOptionValues, uint numLibraryOptions);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuLibraryLoadFromFile(ref CuLibrary library, [MarshalAs(UnmanagedType.LPStr)] string fileName,
                CuJitOption[] jitOptions, IntPtr[] jitOptionsValues, uint numJitOptions,
                CuLibraryOption[] libraryOptions, IntPtr[] libraryOptionValues, uint numLibraryOptions);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuLibraryUnload(CuLibrary library);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuLibraryGetKernel(ref CuKernel pKernel, CuLibrary library, [MarshalAs(UnmanagedType.LPStr)] string name);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuLibraryGetModule(ref CuModule pMod, CuLibrary library);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuKernelGetFunction(ref CuFunction pFunc, CuKernel kernel);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuLibraryGetGlobal(ref CuDevicePtr dptr, ref SizeT bytes, CuLibrary library, [MarshalAs(UnmanagedType.LPStr)] string name);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuLibraryGetManaged(ref CuDevicePtr dptr, ref SizeT bytes, CuLibrary library, [MarshalAs(UnmanagedType.LPStr)] string name);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuLibraryGetUnifiedFunction(ref IntPtr fptr, CuLibrary library, [MarshalAs(UnmanagedType.LPStr)] string symbol);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuKernelGetAttribute(ref int pi, CuFunctionAttribute attrib, CuKernel kernel, CuDevice dev);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuKernelSetAttribute(CuFunctionAttribute attrib, int val, CuKernel kernel, CuDevice dev);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuKernelSetCacheConfig(CuKernel kernel, CuFuncCache config, CuDevice dev);
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
            public static extern CuResult cuMemAlloc_v2(ref CuDevicePtr dptr, SizeT bytesize);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemAllocPitch_v2(ref CuDevicePtr dptr, ref SizeT pPitch, SizeT widthInBytes, SizeT height, uint elementSizeBytes);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemFree_v2(CuDevicePtr dptr);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemGetAddressRange_v2(ref CuDevicePtr pbase, ref SizeT psize, CuDevicePtr dptr);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemAllocHost_v2(ref IntPtr pp, SizeT bytesize);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemFreeHost(IntPtr p);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemHostAlloc(ref IntPtr pp, SizeT bytesize, CuMemHostAllocFlags flags);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemHostGetDevicePointer_v2(ref CuDevicePtr pdptr, IntPtr p, int flags);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemHostGetFlags(ref CuMemHostAllocFlags pFlags, IntPtr p);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemHostRegister_v2")]
            public static extern CuResult cuMemHostRegister(IntPtr p, SizeT byteSize, CuMemHostRegisterFlags flags);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemHostUnregister(IntPtr p);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuPointerGetAttribute(ref CuContext data, CuPointerAttribute attribute, CuDevicePtr ptr);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuPointerGetAttribute(ref CuMemoryType data, CuPointerAttribute attribute, CuDevicePtr ptr);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuPointerGetAttribute(ref CuDevicePtr data, CuPointerAttribute attribute, CuDevicePtr ptr);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuPointerGetAttribute(ref IntPtr data, CuPointerAttribute attribute, CuDevicePtr ptr);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuPointerGetAttribute(ref CudaPointerAttributeP2PTokens data, CuPointerAttribute attribute, CuDevicePtr ptr);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuPointerGetAttribute(ref int data, CuPointerAttribute attribute, CuDevicePtr ptr);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuPointerGetAttribute(ref ulong data, CuPointerAttribute attribute, CuDevicePtr ptr);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemPrefetchAsync")]
            public static extern CuResult cuMemPrefetchAsync(CuDevicePtr devPtr, SizeT count, CuDevice dstDevice, CuStream hStream);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemAdvise(CuDevicePtr devPtr, SizeT count, CuMemAdvise advice, CuDevice device);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemRangeGetAttribute(IntPtr data, SizeT dataSize, CuMemRangeAttribute attribute, CuDevicePtr devPtr, SizeT count);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemRangeGetAttributes([In, Out] IntPtr[] data, [In, Out] SizeT[] dataSizes, [In, Out] CuMemRangeAttribute[] attributes, SizeT numAttributes, CuDevicePtr devPtr, SizeT count);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemAllocManaged(ref CuDevicePtr dptr, SizeT bytesize, CuMemAttachFlags flags);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuPointerSetAttribute(ref int value, CuPointerAttribute attribute, CuDevicePtr ptr);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuPointerGetAttributes(uint numAttributes, [In, Out] CuPointerAttribute[] attributes, IntPtr data, CuDevicePtr ptr);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemAddressReserve(ref CuDevicePtr ptr, SizeT size, SizeT alignment, CuDevicePtr addr, ulong flags);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemAddressFree(CuDevicePtr ptr, SizeT size);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemCreate(ref CuMemGenericAllocationHandle handle, SizeT size, ref CuMemAllocationProp prop, ulong flags);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemRelease(CuMemGenericAllocationHandle handle);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemMap(CuDevicePtr ptr, SizeT size, SizeT offset, CuMemGenericAllocationHandle handle, ulong flags);
            [DllImport(CudaDriverApiDllName, EntryPoint = "pcuMemMapArrayAsync")]
            public static extern CuResult pcuMemMapArrayAsync(CuArrayMapInfo[] mapInfoList, uint count, CuStream hStream);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemUnmap(CuDevicePtr ptr, SizeT size);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemSetAccess(CuDevicePtr ptr, SizeT size, CuMemAccessDesc[] desc, SizeT count);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemGetAccess(ref ulong flags, ref CuMemLocation location, CuDevicePtr ptr);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemExportToShareableHandle(IntPtr shareableHandle, CuMemGenericAllocationHandle handle, CuMemAllocationHandleType handleType, ulong flags);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemImportFromShareableHandle(ref CuMemGenericAllocationHandle handle, IntPtr osHandle, CuMemAllocationHandleType shHandleType);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemGetAllocationGranularity(ref SizeT granularity, ref CuMemAllocationProp prop, CuMemAllocationGranularityFlags option);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemGetAllocationPropertiesFromHandle(ref CuMemAllocationProp prop, CuMemGenericAllocationHandle handle);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemRetainAllocationHandle(ref CuMemGenericAllocationHandle handle, IntPtr addr);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemFreeAsync")]
            public static extern CuResult cuMemFreeAsync(CuDevicePtr dptr, CuStream hStream);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemAllocAsync")]
            public static extern CuResult cuMemAllocAsync(ref CuDevicePtr dptr, SizeT bytesize, CuStream hStream);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemPoolTrimTo(CuMemoryPool pool, SizeT minBytesToKeep);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemPoolSetAttribute(CuMemoryPool pool, CuMemPoolAttribute attr, ref int value);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemPoolSetAttribute(CuMemoryPool pool, CuMemPoolAttribute attr, ref ulong value);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemPoolGetAttribute(CuMemoryPool pool, CuMemPoolAttribute attr, ref int value);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemPoolGetAttribute(CuMemoryPool pool, CuMemPoolAttribute attr, ref ulong value);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemPoolSetAccess(CuMemoryPool pool, CuMemAccessDesc[] map, SizeT count);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemPoolGetAccess(ref CuMemAccessFlags flags, CuMemoryPool memPool, ref CuMemLocation location);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemPoolCreate(ref CuMemoryPool pool, ref CuMemPoolProps poolProps);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemPoolDestroy(CuMemoryPool pool);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemAllocFromPoolAsync")]
            public static extern CuResult cuMemAllocFromPoolAsync(ref CuDevicePtr dptr, SizeT bytesize, CuMemoryPool pool, CuStream hStream);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemPoolExportToShareableHandle(ref IntPtr handleOut, CuMemoryPool pool, CuMemAllocationHandleType handleType, ulong flags);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemPoolImportFromShareableHandle(
                ref CuMemoryPool poolOut,
                IntPtr handle,
                CuMemAllocationHandleType handleType,
                ulong flags);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemPoolExportPointer(ref CuMemPoolPtrExportData shareDataOut, CuDevicePtr ptr);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemPoolImportPointer(ref CuDevicePtr ptrOut, CuMemoryPool pool, ref CuMemPoolPtrExportData shareData);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMemGetHandleForAddressRange(IntPtr handle, CuDevicePtr dptr, SizeT size, CuMemRangeHandleType handleType, ulong flags);
        }
        public static class SynchronousMemcpyV2
        {
            static SynchronousMemcpyV2()
            {
                DriverApiNativeMethods.Init();
            }
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpy")]
            public static extern CuResult cuMemcpy(CuDevicePtr dst, CuDevicePtr src, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyPeer")]
            public static extern CuResult cuMemcpyPeer(CuDevicePtr dstDevice, CuContext dstContext, CuDevicePtr srcDevice, CuContext srcContext, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpy3DPeer")]
            public static extern CuResult cuMemcpy3DPeer(ref CudaMemCpy3DPeer pCopy);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2")]
            public static extern CuResult cuMemcpyHtoD_v2(CuDevicePtr dstDevice, [In] Dim3[] srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2")]
            public static extern CuResult cuMemcpyHtoD_v2(CuDevicePtr dstDevice, [In] byte[] srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2")]
            public static extern CuResult cuMemcpyHtoD_v2(CuDevicePtr dstDevice, [In] sbyte[] srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2")]
            public static extern CuResult cuMemcpyHtoD_v2(CuDevicePtr dstDevice, [In] ushort[] srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2")]
            public static extern CuResult cuMemcpyHtoD_v2(CuDevicePtr dstDevice, [In] short[] srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2")]
            public static extern CuResult cuMemcpyHtoD_v2(CuDevicePtr dstDevice, [In] uint[] srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2")]
            public static extern CuResult cuMemcpyHtoD_v2(CuDevicePtr dstDevice, [In] int[] srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2")]
            public static extern CuResult cuMemcpyHtoD_v2(CuDevicePtr dstDevice, [In] ulong[] srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2")]
            public static extern CuResult cuMemcpyHtoD_v2(CuDevicePtr dstDevice, [In] long[] srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2")]
            public static extern CuResult cuMemcpyHtoD_v2(CuDevicePtr dstDevice, [In] float[] srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2")]
            public static extern CuResult cuMemcpyHtoD_v2(CuDevicePtr dstDevice, [In] double[] srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2")]
            public static extern CuResult cuMemcpyHtoD_v2(CuDevicePtr dstDevice, [In] ref Dim3 srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2")]
            public static extern CuResult cuMemcpyHtoD_v2(CuDevicePtr dstDevice, [In] ref byte srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2")]
            public static extern CuResult cuMemcpyHtoD_v2(CuDevicePtr dstDevice, [In] ref sbyte srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2")]
            public static extern CuResult cuMemcpyHtoD_v2(CuDevicePtr dstDevice, [In] ref ushort srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2")]
            public static extern CuResult cuMemcpyHtoD_v2(CuDevicePtr dstDevice, [In] ref short srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2")]
            public static extern CuResult cuMemcpyHtoD_v2(CuDevicePtr dstDevice, [In] ref uint srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2")]
            public static extern CuResult cuMemcpyHtoD_v2(CuDevicePtr dstDevice, [In] ref int srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2")]
            public static extern CuResult cuMemcpyHtoD_v2(CuDevicePtr dstDevice, [In] ref ulong srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2")]
            public static extern CuResult cuMemcpyHtoD_v2(CuDevicePtr dstDevice, [In] ref long srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2")]
            public static extern CuResult cuMemcpyHtoD_v2(CuDevicePtr dstDevice, [In] ref float srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2")]
            public static extern CuResult cuMemcpyHtoD_v2(CuDevicePtr dstDevice, [In] ref double srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2")]
            public static extern CuResult cuMemcpyHtoD_v2(CuDevicePtr dstDevice, [In] IntPtr srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2")]
            public static extern CuResult cuMemcpyDtoH_v2([Out] Dim3[] dstHost, CuDevicePtr srcDevice, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2")]
            public static extern CuResult cuMemcpyDtoH_v2([Out] byte[] dstHost, CuDevicePtr srcDevice, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2")]
            public static extern CuResult cuMemcpyDtoH_v2([Out] sbyte[] dstHost, CuDevicePtr srcDevice, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2")]
            public static extern CuResult cuMemcpyDtoH_v2([Out] ushort[] dstHost, CuDevicePtr srcDevice, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2")]
            public static extern CuResult cuMemcpyDtoH_v2([Out] short[] dstHost, CuDevicePtr srcDevice, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2")]
            public static extern CuResult cuMemcpyDtoH_v2([Out] uint[] dstHost, CuDevicePtr srcDevice, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2")]
            public static extern CuResult cuMemcpyDtoH_v2([Out] int[] dstHost, CuDevicePtr srcDevice, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2")]
            public static extern CuResult cuMemcpyDtoH_v2([Out] ulong[] dstHost, CuDevicePtr srcDevice, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2")]
            public static extern CuResult cuMemcpyDtoH_v2([Out] long[] dstHost, CuDevicePtr srcDevice, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2")]
            public static extern CuResult cuMemcpyDtoH_v2([Out] float[] dstHost, CuDevicePtr srcDevice, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2")]
            public static extern CuResult cuMemcpyDtoH_v2([Out] double[] dstHost, CuDevicePtr srcDevice, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2")]
            public static extern CuResult cuMemcpyDtoH_v2(ref Dim3 dstHost, CuDevicePtr srcDevice, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2")]
            public static extern CuResult cuMemcpyDtoH_v2(ref byte dstHost, CuDevicePtr srcDevice, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2")]
            public static extern CuResult cuMemcpyDtoH_v2(ref sbyte dstHost, CuDevicePtr srcDevice, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2")]
            public static extern CuResult cuMemcpyDtoH_v2(ref ushort dstHost, CuDevicePtr srcDevice, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2")]
            public static extern CuResult cuMemcpyDtoH_v2(ref short dstHost, CuDevicePtr srcDevice, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2")]
            public static extern CuResult cuMemcpyDtoH_v2(ref uint dstHost, CuDevicePtr srcDevice, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2")]
            public static extern CuResult cuMemcpyDtoH_v2(ref int dstHost, CuDevicePtr srcDevice, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2")]
            public static extern CuResult cuMemcpyDtoH_v2(ref ulong dstHost, CuDevicePtr srcDevice, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2")]
            public static extern CuResult cuMemcpyDtoH_v2(ref long dstHost, CuDevicePtr srcDevice, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2")]
            public static extern CuResult cuMemcpyDtoH_v2(ref float dstHost, CuDevicePtr srcDevice, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2")]
            public static extern CuResult cuMemcpyDtoH_v2(ref double dstHost, CuDevicePtr srcDevice, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2")]
            public static extern CuResult cuMemcpyDtoH_v2([Out] IntPtr dstHost, CuDevicePtr srcDevice, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoD_v2")]
            public static extern CuResult cuMemcpyDtoD_v2(CuDevicePtr dstDevice, CuDevicePtr srcDevice, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoA_v2")]
            public static extern CuResult cuMemcpyDtoA_v2(CuArray dstArray, SizeT dstOffset, CuDevicePtr srcDevice, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyAtoD_v2")]
            public static extern CuResult cuMemcpyAtoD_v2(CuDevicePtr dstDevice, CuArray srcArray, SizeT srcOffset, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoA_v2")]
            public static extern CuResult cuMemcpyHtoA_v2(CuArray dstArray, SizeT dstOffset, [In] Dim3[] srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoA_v2")]
            public static extern CuResult cuMemcpyHtoA_v2(CuArray dstArray, SizeT dstOffset, [In] byte[] srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoA_v2")]
            public static extern CuResult cuMemcpyHtoA_v2(CuArray dstArray, SizeT dstOffset, [In] sbyte[] srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoA_v2")]
            public static extern CuResult cuMemcpyHtoA_v2(CuArray dstArray, SizeT dstOffset, [In] ushort[] srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoA_v2")]
            public static extern CuResult cuMemcpyHtoA_v2(CuArray dstArray, SizeT dstOffset, [In] short[] srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoA_v2")]
            public static extern CuResult cuMemcpyHtoA_v2(CuArray dstArray, SizeT dstOffset, [In] uint[] srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoA_v2")]
            public static extern CuResult cuMemcpyHtoA_v2(CuArray dstArray, SizeT dstOffset, [In] int[] srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoA_v2")]
            public static extern CuResult cuMemcpyHtoA_v2(CuArray dstArray, SizeT dstOffset, [In] ulong[] srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoA_v2")]
            public static extern CuResult cuMemcpyHtoA_v2(CuArray dstArray, SizeT dstOffset, [In] long[] srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoA_v2")]
            public static extern CuResult cuMemcpyHtoA_v2(CuArray dstArray, SizeT dstOffset, [In] float[] srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoA_v2")]
            public static extern CuResult cuMemcpyHtoA_v2(CuArray dstArray, SizeT dstOffset, [In] double[] srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoA_v2")]
            public static extern CuResult cuMemcpyHtoA_v2(CuArray dstArray, SizeT dstOffset, [In] IntPtr srcHost, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyAtoH_v2")]
            public static extern CuResult cuMemcpyAtoH_v2([Out] Dim3[] dstHost, CuArray srcArray, SizeT srcOffset, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyAtoH_v2")]
            public static extern CuResult cuMemcpyAtoH_v2([Out] byte[] dstHost, CuArray srcArray, SizeT srcOffset, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyAtoH_v2")]
            public static extern CuResult cuMemcpyAtoH_v2([Out] sbyte[] dstHost, CuArray srcArray, SizeT srcOffset, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyAtoH_v2")]
            public static extern CuResult cuMemcpyAtoH_v2([Out] ushort[] dstHost, CuArray srcArray, SizeT srcOffset, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyAtoH_v2")]
            public static extern CuResult cuMemcpyAtoH_v2([Out] short[] dstHost, CuArray srcArray, SizeT srcOffset, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyAtoH_v2")]
            public static extern CuResult cuMemcpyAtoH_v2([Out] uint[] dstHost, CuArray srcArray, SizeT srcOffset, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyAtoH_v2")]
            public static extern CuResult cuMemcpyAtoH_v2([Out] int[] dstHost, CuArray srcArray, SizeT srcOffset, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyAtoH_v2")]
            public static extern CuResult cuMemcpyAtoH_v2([Out] ulong[] dstHost, CuArray srcArray, SizeT srcOffset, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyAtoH_v2")]
            public static extern CuResult cuMemcpyAtoH_v2([Out] long[] dstHost, CuArray srcArray, SizeT srcOffset, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyAtoH_v2")]
            public static extern CuResult cuMemcpyAtoH_v2([Out] float[] dstHost, CuArray srcArray, SizeT srcOffset, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyAtoH_v2")]
            public static extern CuResult cuMemcpyAtoH_v2([Out] double[] dstHost, CuArray srcArray, SizeT srcOffset, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyAtoH_v2")]
            public static extern CuResult cuMemcpyAtoH_v2([Out] IntPtr dstHost, CuArray srcArray, SizeT srcOffset, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyAtoA_v2")]
            public static extern CuResult cuMemcpyAtoA_v2(CuArray dstArray, SizeT dstOffset, CuArray srcArray, SizeT srcOffset, SizeT byteCount);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpy2D_v2")]
            public static extern CuResult cuMemcpy2D_v2(ref CudaMemCpy2D pCopy);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpy2DUnaligned_v2")]
            public static extern CuResult cuMemcpy2DUnaligned_v2(ref CudaMemCpy2D pCopy);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpy3D_v2")]
            public static extern CuResult cuMemcpy3D_v2(ref CudaMemCpy3D pCopy);
        }
        public static class AsynchronousMemcpyV2
        {
            static AsynchronousMemcpyV2()
            {
                DriverApiNativeMethods.Init();
            }
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyAsync")]
            public static extern CuResult cuMemcpyAsync(CuDevicePtr dst, CuDevicePtr src, SizeT byteCount, CuStream hStream);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyPeerAsync")]
            public static extern CuResult cuMemcpyPeerAsync(CuDevicePtr dstDevice, CuContext dstContext, CuDevicePtr srcDevice, CuContext srcContext, SizeT byteCount, CuStream hStream);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpy3DPeerAsync")]
            public static extern CuResult cuMemcpy3DPeerAsync(ref CudaMemCpy3DPeer pCopy, CuStream hStream);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoDAsync_v2")]
            public static extern CuResult cuMemcpyHtoDAsync_v2(CuDevicePtr dstDevice, [In] IntPtr srcHost, SizeT byteCount, CuStream hStream);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoHAsync_v2")]
            public static extern CuResult cuMemcpyDtoHAsync_v2([Out] IntPtr dstHost, CuDevicePtr srcDevice, SizeT byteCount, CuStream hStream);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoDAsync_v2")]
            public static extern CuResult cuMemcpyDtoDAsync_v2(CuDevicePtr dstDevice, CuDevicePtr srcDevice, SizeT byteCount, CuStream hStream);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoAAsync_v2")]
            public static extern CuResult cuMemcpyHtoAAsync_v2(CuArray dstArray, SizeT dstOffset, [In] IntPtr srcHost, SizeT byteCount, CuStream hStream);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyAtoHAsync_v2")]
            public static extern CuResult cuMemcpyAtoHAsync_v2([Out] IntPtr dstHost, CuArray srcArray, SizeT srcOffset, SizeT byteCount, CuStream hStream);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpy2DAsync_v2")]
            public static extern CuResult cuMemcpy2DAsync_v2(ref CudaMemCpy2D pCopy, CuStream hStream);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemcpy3DAsync_v2")]
            public static extern CuResult cuMemcpy3DAsync_v2(ref CudaMemCpy3D pCopy, CuStream hStream);
        }
        public static class Memset
        {
            static Memset()
            {
                DriverApiNativeMethods.Init();
            }
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemsetD8_v2")]
            public static extern CuResult cuMemsetD8_v2(CuDevicePtr dstDevice, byte b, SizeT n);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemsetD16_v2")]
            public static extern CuResult cuMemsetD16_v2(CuDevicePtr dstDevice, ushort us, SizeT n);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemsetD32_v2")]
            public static extern CuResult cuMemsetD32_v2(CuDevicePtr dstDevice, uint ui, SizeT n);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemsetD2D8_v2")]
            public static extern CuResult cuMemsetD2D8_v2(CuDevicePtr dstDevice, SizeT dstPitch, byte b, SizeT width, SizeT height);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemsetD2D16_v2")]
            public static extern CuResult cuMemsetD2D16_v2(CuDevicePtr dstDevice, SizeT dstPitch, ushort us, SizeT width, SizeT height);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemsetD2D32_v2")]
            public static extern CuResult cuMemsetD2D32_v2(CuDevicePtr dstDevice, SizeT dstPitch, uint ui, SizeT width, SizeT height);
        }
        public static class MemsetAsync
        {
            static MemsetAsync()
            {
                DriverApiNativeMethods.Init();
            }
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemsetD8Async")]
            public static extern CuResult cuMemsetD8Async(CuDevicePtr dstDevice, byte b, SizeT n, CuStream hStream);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemsetD16Async")]
            public static extern CuResult cuMemsetD16Async(CuDevicePtr dstDevice, ushort us, SizeT n, CuStream hStream);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemsetD32Async")]
            public static extern CuResult cuMemsetD32Async(CuDevicePtr dstDevice, uint ui, SizeT n, CuStream hStream);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemsetD2D8Async")]
            public static extern CuResult cuMemsetD2D8Async(CuDevicePtr dstDevice, SizeT dstPitch, byte b, SizeT width, SizeT height, CuStream hStream);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemsetD2D16Async")]
            public static extern CuResult cuMemsetD2D16Async(CuDevicePtr dstDevice, SizeT dstPitch, ushort us, SizeT width, SizeT height, CuStream hStream);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuMemsetD2D32Async")]
            public static extern CuResult cuMemsetD2D32Async(CuDevicePtr dstDevice, SizeT dstPitch, uint ui, SizeT width, SizeT height, CuStream hStream);
        }
        public static class FunctionManagement
        {
            static FunctionManagement()
            {
                DriverApiNativeMethods.Init();
            }
            [DllImport(CudaDriverApiDllName)]
            [Obsolete(CudaObsolet92)]
            public static extern CuResult cuFuncSetBlockShape(CuFunction hfunc, int x, int y, int z);
            [DllImport(CudaDriverApiDllName)]
            [Obsolete(CudaObsolet92)]
            public static extern CuResult cuFuncSetSharedSize(CuFunction hfunc, uint bytes);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuFuncGetAttribute(ref int pi, CuFunctionAttribute attrib, CuFunction hfunc);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuFuncSetAttribute(CuFunction hfunc, CuFunctionAttribute attrib, int value);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuFuncSetCacheConfig(CuFunction hfunc, CuFuncCache config);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuFuncSetSharedMemConfig(CuFunction hfunc, CuSharedConfig config);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuFuncGetModule(ref CuModule hmod, CuFunction hfunc);
        }
        public static class ArrayManagement
        {
            static ArrayManagement()
            {
                DriverApiNativeMethods.Init();
            }
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuArrayCreate_v2(ref CuArray pHandle, ref CudaArrayDescriptor pAllocateArray);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuArrayGetDescriptor_v2(ref CudaArrayDescriptor pArrayDescriptor, CuArray hArray);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuArrayGetSparseProperties(ref CudaArraySparseProperties sparseProperties, CuArray array);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMipmappedArrayGetSparseProperties(ref CudaArraySparseProperties sparseProperties, CuMipMappedArray mipmap);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuArrayGetPlane(ref CuArray pPlaneArray, CuArray hArray, uint planeIdx);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuArrayDestroy(CuArray hArray);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuArray3DCreate_v2(ref CuArray pHandle, ref CudaArray3DDescriptor pAllocateArray);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuArray3DGetDescriptor_v2(ref CudaArray3DDescriptor pArrayDescriptor, CuArray hArray);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMipmappedArrayCreate(ref CuMipMappedArray pHandle, ref CudaArray3DDescriptor pMipmappedArrayDesc, uint numMipmapLevels);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMipmappedArrayGetLevel(ref CuArray pLevelArray, CuMipMappedArray hMipmappedArray, uint level);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMipmappedArrayDestroy(CuMipMappedArray hMipmappedArray);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuArrayGetMemoryRequirements(ref CudaArrayMemoryRequirements memoryRequirements, CuArray array, CuDevice device);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuMipmappedArrayGetMemoryRequirements(ref CudaArrayMemoryRequirements memoryRequirements, CuMipMappedArray mipmap, CuDevice device);
        }
        public static class Launch
        {
            static Launch()
            {
                DriverApiNativeMethods.Init();
            }
            [DllImport(CudaDriverApiDllName)]
            [Obsolete(CudaObsolet92)]
            public static extern CuResult cuLaunch([In] CuFunction f);
            [DllImport(CudaDriverApiDllName)]
            [Obsolete(CudaObsolet92)]
            public static extern CuResult cuLaunchGrid([In] CuFunction f, [In] int gridWidth, [In] int gridHeight);
            [DllImport(CudaDriverApiDllName)]
            [Obsolete(CudaObsolet92)]
            public static extern CuResult cuLaunchGridAsync([In] CuFunction f, [In] int gridWidth, [In] int gridHeight, [In] CuStream hStream);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuLaunchKernel")]
            public static extern CuResult cuLaunchKernel(CuFunction f,
                uint gridDimX,
                uint gridDimY,
                uint gridDimZ,
                uint blockDimX,
                uint blockDimY,
                uint blockDimZ,
                uint sharedMemBytes,
                CuStream hStream,
                IntPtr[] kernelParams,
                IntPtr[]? extra);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuLaunchCooperativeKernel")]
            public static extern CuResult cuLaunchCooperativeKernel(CuFunction f,
                uint gridDimX,
                uint gridDimY,
                uint gridDimZ,
                uint blockDimX,
                uint blockDimY,
                uint blockDimZ,
                uint sharedMemBytes,
                CuStream hStream,
                IntPtr[] kernelParams);
            [DllImport(CudaDriverApiDllName)]
            [Obsolete("This function is deprecated as of CUDA 11.3")]
            public static extern CuResult cuLaunchCooperativeKernelMultiDevice(CudaLaunchParams[] launchParamsList, uint numDevices, CudaCooperativeLaunchMultiDeviceFlags flags);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuLaunchHostFunc")]
            public static extern CuResult cuLaunchHostFunc(CuStream hStream, CUhostFn fn, IntPtr userData);
            public static CuResult CuLaunchKernelEx(ref CuLaunchConfig config, CuFunction f, IntPtr[] kernelParams, IntPtr[] extra)
            {
                CuResult retVal;
                var conf = new CuLaunchConfigInternal {
                    GridDimX = config.GridDimX,
                    GridDimY = config.GridDimY,
                    GridDimZ = config.GridDimZ,
                    BlockDimX = config.BlockDimX,
                    BlockDimY = config.BlockDimY,
                    BlockDimZ = config.BlockDimZ,
                    SharedMemBytes = config.SharedMemBytes,
                    HStream = config.HStream,
                    NumAttrs = 0,
                    Attrs = IntPtr.Zero
                };

                try {
                    var arraySize = 0;
                    if (config.Attrs != null) {
                        arraySize = config.Attrs.Length;
                        conf.NumAttrs = (uint)arraySize;
                    }

                    var paramsSize = Marshal.SizeOf(typeof(CuLaunchAttribute));

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

            [DllImport(CudaDriverApiDllName, EntryPoint = "cuLaunchKernelEx")]
            static extern CuResult cuLaunchKernelExInternal(ref CuLaunchConfigInternal config, CuFunction f, IntPtr[] kernelParams, IntPtr[] extra);
        }
        public static class Events
        {
            static Events()
            {
                DriverApiNativeMethods.Init();
            }
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuEventCreate(ref CuEvent phEvent, CuEventFlags flags);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuEventRecord")]
            public static extern CuResult cuEventRecord(CuEvent hEvent, CuStream hStream);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuEventRecordWithFlags")]
            public static extern CuResult cuEventRecordWithFlags(CuEvent hEvent, CuStream hStream, CuEventRecordFlags flags);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuEventQuery(CuEvent hEvent);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuEventSynchronize(CuEvent hEvent);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuEventDestroy_v2(CuEvent hEvent);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuEventElapsedTime(ref float pMilliseconds, CuEvent hStart, CuEvent hEnd);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuStreamWaitValue32_v2")]
            public static extern CuResult cuStreamWaitValue32(CuStream stream, CuDevicePtr addr, uint value, CuStreamWaitValueFlags flags);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuStreamWaitValue64_v2")]
            public static extern CuResult cuStreamWaitValue64(CuStream stream, CuDevicePtr addr, ulong value, CuStreamWaitValueFlags flags);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuStreamWriteValue32_v2")]
            public static extern CuResult cuStreamWriteValue32(CuStream stream, CuDevicePtr addr, uint value, CuStreamWriteValueFlags flags);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuStreamWriteValue64_v2")]
            public static extern CuResult cuStreamWriteValue64(CuStream stream, CuDevicePtr addr, ulong value, CuStreamWriteValueFlags flags);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuStreamBatchMemOp_v2")]
            public static extern CuResult cuStreamBatchMemOp(CuStream stream, uint count, CuStreamBatchMemOpParams[] paramArray, uint flags);
        }
        public static class Streams
        {
            static Streams()
            {
                DriverApiNativeMethods.Init();
            }
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuStreamCreate(ref CuStream phStream, CuStreamFlags flags);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuStreamQuery")]
            public static extern CuResult cuStreamQuery(CuStream hStream);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuStreamSynchronize")]
            public static extern CuResult cuStreamSynchronize(CuStream hStream);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuStreamDestroy_v2(CuStream hStream);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuStreamCopyAttributes")]
            public static extern CuResult cuStreamCopyAttributes(CuStream dst, CuStream src);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuStreamGetAttribute")]
            public static extern CuResult cuStreamGetAttribute(CuStream hStream, CuStreamAttrId attr, ref CuStreamAttrValue valueOut);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuStreamSetAttribute")]
            public static extern CuResult cuStreamSetAttribute(CuStream hStream, CuStreamAttrId attr, ref CuStreamAttrValue value);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuStreamWaitEvent")]
            public static extern CuResult cuStreamWaitEvent(CuStream hStream, CuEvent hEvent, uint flags);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuStreamAddCallback")]
            public static extern CuResult cuStreamAddCallback(CuStream hStream, CuStreamCallback callback, IntPtr userData, CuStreamAddCallbackFlags flags);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuStreamCreateWithPriority(ref CuStream phStream, CuStreamFlags flags, int priority);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuStreamGetPriority")]
            public static extern CuResult cuStreamGetPriority(CuStream hStream, ref int priority);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuStreamGetFlags")]
            public static extern CuResult cuStreamGetFlags(CuStream hStream, ref CuStreamFlags flags);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuStreamGetCtx")]
            public static extern CuResult cuStreamGetCtx(CuStream hStream, ref CuContext pctx);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuStreamAttachMemAsync")]
            public static extern CuResult cuStreamAttachMemAsync(CuStream hStream, CuDevicePtr dptr, SizeT length, CuMemAttachFlags flags);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuStreamBeginCapture_v2")]
            public static extern CuResult cuStreamBeginCapture(CuStream hStream, CuStreamCaptureMode mode);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuStreamEndCapture")]
            public static extern CuResult cuStreamEndCapture(CuStream hStream, ref CuGraph phGraph);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuStreamIsCapturing")]
            public static extern CuResult cuStreamIsCapturing(CuStream hStream, ref CuStreamCaptureStatus captureStatus);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuThreadExchangeStreamCaptureMode(ref CuStreamCaptureMode mode);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuStreamGetCaptureInfo_v2")]
            public static extern CuResult cuStreamGetCaptureInfo(CuStream hStream, ref CuStreamCaptureStatus captureStatusOut,
                ref ulong idOut, ref CuGraph graphOut, ref IntPtr dependenciesOut, ref SizeT numDependenciesOut);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuStreamUpdateCaptureDependencies")]
            public static extern CuResult cuStreamUpdateCaptureDependencies(CuStream hStream, CuGraphNode[] dependencies, SizeT numDependencies, uint flags);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuStreamGetId")]
            public static extern CuResult cuStreamGetId(CuStream hStream, ref ulong streamId);
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
            public static extern CuResult cuDeviceCanAccessPeer(ref int canAccessPeer, CuDevice dev, CuDevice peerDev);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuCtxEnablePeerAccess(CuContext peerContext, CtxEnablePeerAccessFlags flags);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuCtxDisablePeerAccess(CuContext peerContext);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuDeviceGetP2PAttribute(ref int value, CuDeviceP2PAttribute attrib, CuDevice srcDevice, CuDevice dstDevice);
        }
        public static class Profiling
        {
            static Profiling()
            {
                DriverApiNativeMethods.Init();
            }
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuProfilerInitialize(string configFile, string outputFile, CuOutputMode outputMode);
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
            public static extern CuResult cuOccupancyMaxActiveBlocksPerMultiprocessor(ref int numBlocks, CuFunction func, int blockSize, SizeT dynamicSMemSize);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuOccupancyMaxPotentialBlockSize(ref int minGridSize, ref int blockSize, CuFunction func, DelCuOccupancyB2DSize blockSizeToDynamicSMemSize, SizeT dynamicSMemSize, int blockSizeLimit);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuOccupancyMaxActiveBlocksPerMultiprocessorWithFlags(ref int numBlocks, CuFunction func, int blockSize, SizeT dynamicSMemSize, CuOccupancyFlags flags);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuOccupancyMaxPotentialBlockSizeWithFlags(ref int minGridSize, ref int blockSize, CuFunction func, DelCuOccupancyB2DSize blockSizeToDynamicSMemSize, SizeT dynamicSMemSize, int blockSizeLimit, CuOccupancyFlags flags);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuOccupancyAvailableDynamicSMemPerBlock(ref SizeT dynamicSmemSize, CuFunction func, int numBlocks, int blockSize);
            public static CuResult CuOccupancyMaxPotentialClusterSize(ref int clusterSize, CuFunction func, ref CuLaunchConfig config)
            {
                CuResult retVal;
                var conf = new CuLaunchConfigInternal {
                    GridDimX = config.GridDimX,
                    GridDimY = config.GridDimY,
                    GridDimZ = config.GridDimZ,
                    BlockDimX = config.BlockDimX,
                    BlockDimY = config.BlockDimY,
                    BlockDimZ = config.BlockDimZ,
                    SharedMemBytes = config.SharedMemBytes,
                    HStream = config.HStream,
                    NumAttrs = 0,
                    Attrs = IntPtr.Zero
                };

                try {
                    var arraySize = 0;
                    if (config.Attrs != null) {
                        arraySize = config.Attrs.Length;
                        conf.NumAttrs = (uint)arraySize;
                    }

                    var paramsSize = Marshal.SizeOf(typeof(CuLaunchAttribute));

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
            static extern CuResult cuOccupancyMaxPotentialClusterSizeInternal(ref int clusterSize, CuFunction func, ref CuLaunchConfigInternal config);
            public static CuResult CuOccupancyMaxActiveClusters(ref int numClusters, CuFunction func, ref CuLaunchConfig config)
            {
                CuResult retVal;
                var conf = new CuLaunchConfigInternal {
                    GridDimX = config.GridDimX,
                    GridDimY = config.GridDimY,
                    GridDimZ = config.GridDimZ,
                    BlockDimX = config.BlockDimX,
                    BlockDimY = config.BlockDimY,
                    BlockDimZ = config.BlockDimZ,
                    SharedMemBytes = config.SharedMemBytes,
                    HStream = config.HStream,
                    NumAttrs = 0,
                    Attrs = IntPtr.Zero
                };

                try {
                    var arraySize = 0;
                    if (config.Attrs != null) {
                        arraySize = config.Attrs.Length;
                        conf.NumAttrs = (uint)arraySize;
                    }

                    var paramsSize = Marshal.SizeOf(typeof(CuLaunchAttribute));
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
            static extern CuResult cuOccupancyMaxActiveClustersInternal(ref int numClusters, CuFunction func, ref CuLaunchConfigInternal config);
        }
        public static class GraphManagement
        {
            static GraphManagement()
            {
                DriverApiNativeMethods.Init();
            }
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphCreate(ref CuGraph phGraph, uint flags);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuGraphAddKernelNode_v2")]
            public static extern CuResult cuGraphAddKernelNode(ref CuGraphNode phGraphNode, CuGraph hGraph, CuGraphNode[] dependencies, SizeT numDependencies, ref CudaKernelNodeParams nodeParams);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuGraphKernelNodeGetParams_v2")]
            public static extern CuResult cuGraphKernelNodeGetParams(CuGraphNode hNode, ref CudaKernelNodeParams nodeParams);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuGraphKernelNodeSetParams_v2")]
            public static extern CuResult cuGraphKernelNodeSetParams(CuGraphNode hNode, ref CudaKernelNodeParams nodeParams);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphAddMemcpyNode(ref CuGraphNode phGraphNode, CuGraph hGraph, CuGraphNode[] dependencies, SizeT numDependencies, ref CudaMemCpy3D copyParams, CuContext ctx);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphMemcpyNodeGetParams(CuGraphNode hNode, ref CudaMemCpy3D nodeParams);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphMemcpyNodeSetParams(CuGraphNode hNode, ref CudaMemCpy3D nodeParams);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphAddMemsetNode(ref CuGraphNode phGraphNode, CuGraph hGraph, CuGraphNode[] dependencies, SizeT numDependencies, ref CudaMemsetNodeParams memsetParams, CuContext ctx);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphMemsetNodeGetParams(CuGraphNode hNode, ref CudaMemsetNodeParams nodeParams);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphMemsetNodeSetParams(CuGraphNode hNode, ref CudaMemsetNodeParams nodeParams);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphAddHostNode(ref CuGraphNode phGraphNode, CuGraph hGraph, CuGraphNode[] dependencies, SizeT numDependencies, ref CudaHostNodeParams nodeParams);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphHostNodeGetParams(CuGraphNode hNode, ref CudaHostNodeParams nodeParams);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphHostNodeSetParams(CuGraphNode hNode, ref CudaHostNodeParams nodeParams);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphAddChildGraphNode(ref CuGraphNode phGraphNode, CuGraph hGraph, CuGraphNode[] dependencies, SizeT numDependencies, CuGraph childGraph);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphChildGraphNodeGetGraph(CuGraphNode hNode, ref CuGraph phGraph);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphAddEmptyNode(ref CuGraphNode phGraphNode, CuGraph hGraph, CuGraphNode[] dependencies, SizeT numDependencies);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphAddEventRecordNode(ref CuGraphNode phGraphNode, CuGraph hGraph, CuGraphNode[] dependencies, SizeT numDependencies, CuEvent @event);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphEventRecordNodeGetEvent(CuGraphNode hNode, ref CuEvent eventOut);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphEventRecordNodeSetEvent(CuGraphNode hNode, CuEvent @event);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphAddEventWaitNode(ref CuGraphNode phGraphNode, CuGraph hGraph, CuGraphNode[] dependencies, SizeT numDependencies, CuEvent @event);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphEventWaitNodeGetEvent(CuGraphNode hNode, ref CuEvent eventOut);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphEventWaitNodeSetEvent(CuGraphNode hNode, CuEvent @event);
            public static CuResult CuGraphAddExternalSemaphoresSignalNode(ref CuGraphNode phGraphNode, CuGraph hGraph, CuGraphNode[] dependencies, SizeT numDependencies, CudaExtSemSignalNodeParams nodeParams)
            {
                var extSemPtr = IntPtr.Zero;
                var paramsPtr = IntPtr.Zero;
                var mainPtr = IntPtr.Zero;
                CuResult retVal;

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
                        Marshal.StructureToPtr(nodeParams.ExtSemArray![i], extSemPtr + (IntPtr.Size * i), false);
                        Marshal.StructureToPtr(nodeParams.ParamsArray![i], paramsPtr + (paramsSize * i), false);
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
            static extern CuResult cuGraphAddExternalSemaphoresSignalNodeInternal(ref CuGraphNode phGraphNode, CuGraph hGraph, CuGraphNode[] dependencies, SizeT numDependencies, IntPtr nodeParams);
            public static CuResult CuGraphExternalSemaphoresSignalNodeGetParams(CuGraphNode hNode, CudaExtSemSignalNodeParams paramsOut)
            {
                var mainPtr = IntPtr.Zero;

                CuResult retVal;

                try {
                    const int arraySize = 0;
                    var paramsSize = Marshal.SizeOf(typeof(CudaExternalSemaphoreSignalParams));
                    mainPtr = Marshal.AllocHGlobal(2 * IntPtr.Size + sizeof(int));

                    Marshal.WriteIntPtr(mainPtr + 0, IntPtr.Zero);
                    Marshal.WriteIntPtr(mainPtr + IntPtr.Size, IntPtr.Zero);
                    Marshal.WriteInt32(mainPtr + 2 * IntPtr.Size, arraySize);

                    retVal = cuGraphExternalSemaphoresSignalNodeGetParamsInternal(hNode, mainPtr);

                    var length = Marshal.ReadInt32(mainPtr + 2 * IntPtr.Size);

                    var array1 = new CuExternalSemaphore[length];
                    var array2 = new CudaExternalSemaphoreSignalParams[length];
                    var ptr1 = Marshal.ReadIntPtr(mainPtr);
                    var ptr2 = Marshal.ReadIntPtr(mainPtr + IntPtr.Size);

                    for (var i = 0; i < length; i++) {
                        array1[i] = (CuExternalSemaphore)(Marshal.PtrToStructure(ptr1 + (IntPtr.Size * i), typeof(CuExternalSemaphore)) ?? throw new InvalidOperationException());
                        array2[i] = (CudaExternalSemaphoreSignalParams)(Marshal.PtrToStructure(ptr2 + (paramsSize * i), typeof(CudaExternalSemaphoreSignalParams)) ?? throw new InvalidOperationException());
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
            static extern CuResult cuGraphExternalSemaphoresSignalNodeGetParamsInternal(CuGraphNode hNode, IntPtr paramsOut);
            public static CuResult CuGraphExternalSemaphoresSignalNodeSetParams(CuGraphNode hNode, CudaExtSemSignalNodeParams nodeParams)
            {
                var extSemPtr = IntPtr.Zero;
                var paramsPtr = IntPtr.Zero;
                var mainPtr = IntPtr.Zero;
                CuResult retVal;

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
                        Marshal.StructureToPtr(nodeParams.ExtSemArray![i], extSemPtr + (IntPtr.Size * i), false);
                        Marshal.StructureToPtr(nodeParams.ParamsArray![i], paramsPtr + (paramsSize * i), false);
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
            static extern CuResult cuGraphExternalSemaphoresSignalNodeSetParamsInternal(CuGraphNode hNode, IntPtr nodeParams);
            public static CuResult CuGraphAddExternalSemaphoresWaitNode(ref CuGraphNode phGraphNode, CuGraph hGraph, CuGraphNode[] dependencies, SizeT numDependencies, CudaExtSemWaitNodeParams nodeParams)
            {
                var extSemPtr = IntPtr.Zero;
                var paramsPtr = IntPtr.Zero;
                var mainPtr = IntPtr.Zero;
                CuResult retVal;

                try {
                    var arraySize = 0;
                    if (nodeParams is { ExtSemArray: { }, ParamsArray: { } }) {
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
                        Marshal.StructureToPtr(nodeParams.ExtSemArray![i], extSemPtr + (IntPtr.Size * i), false);
                        Marshal.StructureToPtr(nodeParams.ParamsArray![i], paramsPtr + (paramsSize * i), false);
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
            static extern CuResult cuGraphAddExternalSemaphoresWaitNodeInternal(ref CuGraphNode phGraphNode, CuGraph hGraph, CuGraphNode[] dependencies, SizeT numDependencies, IntPtr nodeParams);
            public static CuResult CuGraphExternalSemaphoresWaitNodeGetParams(CuGraphNode hNode, CudaExtSemWaitNodeParams paramsOut)
            {
                var mainPtr = IntPtr.Zero;
                CuResult retVal;

                try {
                    const int arraySize = 0;
                    var paramsSize = Marshal.SizeOf(typeof(CudaExternalSemaphoreWaitParams));
                    mainPtr = Marshal.AllocHGlobal(2 * IntPtr.Size + sizeof(int));

                    Marshal.WriteIntPtr(mainPtr + 0, IntPtr.Zero);
                    Marshal.WriteIntPtr(mainPtr + IntPtr.Size, IntPtr.Zero);
                    Marshal.WriteInt32(mainPtr + 2 * IntPtr.Size, arraySize);

                    retVal = cuGraphExternalSemaphoresWaitNodeGetParamsInternal(hNode, mainPtr);
                    var length = Marshal.ReadInt32(mainPtr + 2 * IntPtr.Size);

                    var array1 = new CuExternalSemaphore[length];
                    var array2 = new CudaExternalSemaphoreWaitParams[length];
                    var ptr1 = Marshal.ReadIntPtr(mainPtr);
                    var ptr2 = Marshal.ReadIntPtr(mainPtr + IntPtr.Size);

                    for (var i = 0; i < length; i++) {
                        array1[i] = (CuExternalSemaphore)(Marshal.PtrToStructure(ptr1 + (IntPtr.Size * i), typeof(CuExternalSemaphore)) ?? throw new InvalidOperationException());
                        array2[i] = (CudaExternalSemaphoreWaitParams)(Marshal.PtrToStructure(ptr2 + (paramsSize * i), typeof(CudaExternalSemaphoreWaitParams)) ?? throw new InvalidOperationException());
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
            static extern CuResult cuGraphExternalSemaphoresWaitNodeGetParamsInternal(CuGraphNode hNode, IntPtr paramsOut);
            public static CuResult CuGraphExternalSemaphoresWaitNodeSetParams(CuGraphNode hNode, CudaExtSemWaitNodeParams nodeParams)
            {
                var extSemPtr = IntPtr.Zero;
                var paramsPtr = IntPtr.Zero;
                var mainPtr = IntPtr.Zero;
                CuResult retVal;

                try {
                    var arraySize = 0;
                    if (nodeParams is { ExtSemArray: { }, ParamsArray: { } }) {
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
                        Marshal.StructureToPtr(nodeParams.ExtSemArray![i], extSemPtr + (IntPtr.Size * i), false);
                        Marshal.StructureToPtr(nodeParams.ParamsArray![i], paramsPtr + (paramsSize * i), false);
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
            static extern CuResult cuGraphExternalSemaphoresWaitNodeSetParamsInternal(CuGraphNode hNode, IntPtr nodeParams);

            [DllImport(CudaDriverApiDllName, EntryPoint = "cuGraphAddBatchMemOpNode")]
            public static extern CuResult cuGraphAddBatchMemOpNodeInternal(ref CuGraphNode phGraphNode, CuGraph hGraph, CuGraphNode[] dependencies, SizeT numDependencies, ref CudaBatchMemOpNodeParamsInternal nodeParams);
            public static CuResult CuGraphBatchMemOpNodeGetParams(CuGraphNode hNode, ref CudaBatchMemOpNodeParams nodeParamsOut)
            {
                CuResult retVal;
                var parameters = new CudaBatchMemOpNodeParamsInternal();

                try {
                    retVal = cuGraphBatchMemOpNodeGetParamsInternal(hNode, ref parameters);
                    var arraySize = (int)parameters.Count;
                    var paramsSize = Marshal.SizeOf(typeof(CuStreamBatchMemOpParams));

                    if (arraySize > 0) {
                        nodeParamsOut.ParamArray = new CuStreamBatchMemOpParams[arraySize];

                        for (var i = 0; i < arraySize; i++) {
                            nodeParamsOut.ParamArray[i] = (CuStreamBatchMemOpParams)(Marshal.PtrToStructure(parameters.ParamArray + (paramsSize * i), typeof(CuStreamBatchMemOpParams)) ?? throw new InvalidOperationException());
                        }
                    }

                    nodeParamsOut.Ctx = parameters.Ctx;
                    nodeParamsOut.Flags = parameters.Flags;
                }
                catch {
                    retVal = CuResult.ErrorInvalidValue;
                }

                return retVal;
            }

            [DllImport(CudaDriverApiDllName, EntryPoint = "cuGraphBatchMemOpNodeGetParams")]
            static extern CuResult cuGraphBatchMemOpNodeGetParamsInternal(CuGraphNode hNode, ref CudaBatchMemOpNodeParamsInternal nodeParamsOut);
            public static CuResult CuGraphBatchMemOpNodeSetParams(CuGraphNode hNode, ref CudaBatchMemOpNodeParams nodeParams)
            {
                CuResult retVal;
                var parameters = new CudaBatchMemOpNodeParamsInternal {
                    Ctx = nodeParams.Ctx,
                    Count = 0,
                    ParamArray = IntPtr.Zero,
                    Flags = nodeParams.Flags
                };

                try {
                    var arraySize = 0;
                    if (nodeParams.ParamArray != null) {
                        arraySize = nodeParams.ParamArray.Length;
                        parameters.Count = (uint)arraySize;
                    }

                    var paramsSize = Marshal.SizeOf(typeof(CuStreamBatchMemOpParams));
                    if (arraySize > 0) {
                        parameters.ParamArray = Marshal.AllocHGlobal(arraySize * paramsSize);
                    }

                    for (var i = 0; i < arraySize; i++) {
                        Marshal.StructureToPtr(nodeParams.ParamArray![i], parameters.ParamArray + (paramsSize * i), false);
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
            static extern CuResult cuGraphBatchMemOpNodeSetParamsInternal(CuGraphNode hNode, ref CudaBatchMemOpNodeParamsInternal nodeParams);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuGraphExecBatchMemOpNodeSetParams")]
            public static extern CuResult cuGraphExecBatchMemOpNodeSetParamsInternal(CuGraphExec hGraphExec, CuGraphNode hNode, ref CudaBatchMemOpNodeParamsInternal nodeParams);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphAddMemAllocNodeInternal(ref CuGraphNode phGraphNode, CuGraph hGraph, CuGraphNode[] dependencies, SizeT numDependencies, ref CudaMemAllocNodeParamsInternal nodeParams);
            public static CuResult CuGraphMemAllocNodeGetParams(CuGraphNode hNode, ref CudaMemAllocNodeParams paramsOut)
            {
                CuResult retVal;
                var parameters = new CudaMemAllocNodeParamsInternal();

                try {
                    retVal = cuGraphMemAllocNodeGetParamsInternal(hNode, ref parameters);
                    var arraySize = (int)parameters.accessDescCount;
                    var paramsSize = Marshal.SizeOf(typeof(CuMemAccessDesc));

                    if (arraySize > 0) {
                        paramsOut.accessDescs = new CuMemAccessDesc[arraySize];

                        for (var i = 0; i < arraySize; i++) {
                            paramsOut.accessDescs[i] = (CuMemAccessDesc)(Marshal.PtrToStructure(parameters.accessDescs + (paramsSize * i), typeof(CuMemAccessDesc)) ?? throw new InvalidOperationException());
                        }
                    }

                    paramsOut.poolProps = parameters.poolProps;
                    paramsOut.dptr = parameters.dptr;
                    paramsOut.bytesize = parameters.bytesize;
                }
                catch {
                    retVal = CuResult.ErrorInvalidValue;
                }

                return retVal;
            }

            [DllImport(CudaDriverApiDllName)]
            static extern CuResult cuGraphMemAllocNodeGetParamsInternal(CuGraphNode hNode, ref CudaMemAllocNodeParamsInternal paramsOut);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphAddMemFreeNode(ref CuGraphNode phGraphNode, CuGraph hGraph, CuGraphNode[] dependencies, SizeT numDependencies, CuDevicePtr dptr);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphMemFreeNodeGetParams(CuGraphNode hNode, ref CuDevicePtr dptrOut);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuDeviceGraphMemTrim(CuDevice device);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuDeviceGetGraphMemAttribute(CuDevice device, CuGraphMemAttribute attr, ref ulong value);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuDeviceSetGraphMemAttribute(CuDevice device, CuGraphMemAttribute attr, ref ulong value);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphClone(ref CuGraph phGraphClone, CuGraph originalGraph);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphNodeFindInClone(ref CuGraphNode phNode, CuGraphNode hOriginalNode, CuGraph hClonedGraph);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphNodeGetType(CuGraphNode hNode, ref CuGraphNodeType type);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphGetNodes(CuGraph hGraph, [In, Out] CuGraphNode[] nodes, ref SizeT numNodes);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphGetRootNodes(CuGraph hGraph, [In, Out] CuGraphNode[] rootNodes, ref SizeT numRootNodes);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphGetEdges(CuGraph hGraph, [In, Out] CuGraphNode[] from, [In, Out] CuGraphNode[] to, ref SizeT numEdges);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphNodeGetDependencies(CuGraphNode hNode, [In, Out] CuGraphNode[]? dependencies, ref SizeT numDependencies);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphNodeGetDependentNodes(CuGraphNode hNode, [In, Out] CuGraphNode[]? dependentNodes, ref SizeT numDependentNodes);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphAddDependencies(CuGraph hGraph, CuGraphNode[] from, CuGraphNode[] to, SizeT numDependencies);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphRemoveDependencies(CuGraph hGraph, CuGraphNode[] from, CuGraphNode[] to, SizeT numDependencies);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphDestroyNode(CuGraphNode hNode);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuGraphInstantiateWithFlags")]
            public static extern CuResult cuGraphInstantiate(ref CuGraphExec phGraphExec, CuGraph hGraph, CuGraphInstantiateFlags flags);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuGraphInstantiateWithParams")]
            public static extern CuResult cuGraphInstantiateWithParams(ref CuGraphExec phGraphExec, CuGraph hGraph, ref CudaGraphInstantiateParams instantiateParams);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphExecGetFlags(CuGraphExec hGraphExec, ref CuGraphInstantiateFlags flags);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuGraphExecKernelNodeSetParams_v2")]
            public static extern CuResult cuGraphExecKernelNodeSetParams(CuGraphExec hGraphExec, CuGraphNode hNode, ref CudaKernelNodeParams nodeParams);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphExecMemcpyNodeSetParams(CuGraphExec hGraphExec, CuGraphNode hNode, ref CudaMemCpy3D copyParams, CuContext ctx);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphExecMemsetNodeSetParams(CuGraphExec hGraphExec, CuGraphNode hNode, ref CudaMemsetNodeParams memsetParams, CuContext ctx);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphExecHostNodeSetParams(CuGraphExec hGraphExec, CuGraphNode hNode, ref CudaHostNodeParams nodeParams);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphExecChildGraphNodeSetParams(CuGraphExec hGraphExec, CuGraphNode hNode, CuGraph childGraph);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphExecEventRecordNodeSetEvent(CuGraphExec hGraphExec, CuGraphNode hNode, CuEvent @event);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphExecEventWaitNodeSetEvent(CuGraphExec hGraphExec, CuGraphNode hNode, CuEvent @event);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuGraphExecExternalSemaphoresSignalNodeSetParams")]
            public static extern CuResult cuGraphExecExternalSemaphoresSignalNodeSetParamsInternal(CuGraphExec hGraphExec, CuGraphNode hNode, IntPtr nodeParams);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuGraphExecExternalSemaphoresWaitNodeSetParams")]
            public static extern CuResult cuGraphExecExternalSemaphoresWaitNodeSetParamsInternal(CuGraphExec hGraphExec, CuGraphNode hNode, IntPtr nodeParams);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphNodeSetEnabled(CuGraphExec hGraphExec, CuGraphNode hNode, uint isEnabled);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphNodeGetEnabled(CuGraphExec hGraphExec, CuGraphNode hNode, ref int isEnabled);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuGraphUpload")]
            public static extern CuResult cuGraphUpload(CuGraphExec hGraphExec, CuStream hStream);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuGraphLaunch")]
            public static extern CuResult cuGraphLaunch(CuGraphExec hGraphExec, CuStream hStream);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphExecDestroy(CuGraphExec hGraphExec);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphDestroy(CuGraph hGraph);
            [DllImport(CudaDriverApiDllName, EntryPoint = "cuGraphExecUpdate_v2")]
            public static extern CuResult cuGraphExecUpdate(CuGraphExec hGraphExec, CuGraph hGraph, ref CuGraphExecUpdateResultInfo resultInfo);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphKernelNodeCopyAttributes(CuGraphNode dst, CuGraphNode src);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphKernelNodeGetAttribute(CuGraphNode hNode, CuKernelNodeAttrId attr, ref CuKernelNodeAttrValue valueOut);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphKernelNodeSetAttribute(CuGraphNode hNode, CuKernelNodeAttrId attr, ref CuKernelNodeAttrValue value);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphDebugDotPrint(CuGraph hGraph, [MarshalAs(UnmanagedType.LPStr)] string path, CuGraphDebugDotFlags flags);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuUserObjectCreate(ref CuUserObject objectOut, IntPtr ptr, CUhostFn destroy, uint initialRefcount, CuUserObjectFlags flags);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuUserObjectRetain(CuUserObject obj, uint count);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuUserObjectRelease(CuUserObject obj, uint count);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphRetainUserObject(CuGraph graph, CuUserObject obj, uint count, CuUserObjectRetainFlags flags);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuGraphReleaseUserObject(CuGraph graph, CuUserObject obj, uint count);
        }
        public static class TensorCoreManagement
        {
            static TensorCoreManagement()
            {
                DriverApiNativeMethods.Init();
            }
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuTensorMapEncodeTiled(ref CuTensorMap tensorMap, CuTensorMapDataType tensorDataType, uint tensorRank, CuDevicePtr globalAddress, ulong[] globalDim, ulong[] globalStrides, uint[] boxDim, uint[] elementStrides, CuTensorMapInterleave interleave, CuTensorMapSwizzle swizzle, CuTensorMapL2Promotion l2Promotion, CuTensorMapFloatOoBfill oobFill);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuTensorMapEncodeIm2col(ref CuTensorMap tensorMap, CuTensorMapDataType tensorDataType, uint tensorRank, CuDevicePtr globalAddress, ulong[] globalDim, ulong[] globalStrides, int[] pixelBoxLowerCorner, int[] pixelBoxUpperCorner, uint channelsPerPixel, uint pixelsPerColumn, uint[] elementStrides, CuTensorMapInterleave interleave, CuTensorMapSwizzle swizzle, CuTensorMapL2Promotion l2Promotion, CuTensorMapFloatOoBfill oobFill);
            [DllImport(CudaDriverApiDllName)]
            public static extern CuResult cuTensorMapReplaceAddress(ref CuTensorMap tensorMap, CuDevicePtr globalAddress);
        }
    }
}