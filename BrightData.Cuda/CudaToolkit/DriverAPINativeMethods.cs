using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using BrightData.Cuda.CudaToolkit.Types;

namespace BrightData.Cuda.CudaToolkit
{
    internal static partial class DriverApiNativeMethods
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

        [LibraryImport(CudaDriverApiDllName)]
        public static partial CuResult cuInit(CuInitializationFlags flags);
        [LibraryImport(CudaDriverApiDllName)]
        public static partial CuResult cuDriverGetVersion(ref int driverVersion);

        public static partial class DeviceManagement
        {
            static DeviceManagement()
            {
                DriverApiNativeMethods.Init();
            }
            [LibraryImport(CudaDriverApiDllName)]
            public static partial CuResult cuDeviceGet(ref CuDevice device, int ordinal);
            [LibraryImport(CudaDriverApiDllName)]
            public static partial CuResult cuDeviceGetCount(ref int count);
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
            [LibraryImport(CudaDriverApiDllName)]
            public static partial CuResult cuDeviceGetAttribute(ref int pi, CuDeviceAttribute attrib, CuDevice dev);
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
        public static partial class ContextManagement
        {
            static ContextManagement()
            {
                DriverApiNativeMethods.Init();
            }
            [LibraryImport(CudaDriverApiDllName)]
            public static partial CuResult cuCtxCreate_v2(ref CuContext pctx, CuCtxFlags flags, CuDevice dev);
            [LibraryImport(CudaDriverApiDllName)]
            public static partial CuResult cuCtxCreate_v3(ref CuContext pctx, CuExecAffinityParam[] paramsArray, int numParams, CuCtxFlags flags, CuDevice dev);
            [LibraryImport(CudaDriverApiDllName)]
            public static partial CuResult cuCtxDestroy_v2(CuContext ctx);
            [DllImport(CudaDriverApiDllName)]
            [Obsolete(CudaObsolet92)]
            public static extern CuResult cuCtxAttach(ref CuContext pctx, CuCtxAttachFlags flags);
            [DllImport(CudaDriverApiDllName)]
            [Obsolete(CudaObsolet92)]
            public static extern CuResult cuCtxDetach([In] CuContext ctx);
            [LibraryImport(CudaDriverApiDllName)]
            public static partial CuResult cuCtxPushCurrent_v2(CuContext ctx);
            [LibraryImport(CudaDriverApiDllName)]
            public static partial CuResult cuCtxPopCurrent_v2(ref CuContext pctx);
            [LibraryImport(CudaDriverApiDllName)]
            public static partial CuResult cuCtxSetCurrent(CuContext ctx);
            [LibraryImport(CudaDriverApiDllName)]
            public static partial CuResult cuCtxGetCurrent(ref CuContext pctx);
            [LibraryImport(CudaDriverApiDllName)]
            public static partial CuResult cuCtxGetDevice(ref CuDevice device);
            [LibraryImport(CudaDriverApiDllName)]
            public static partial CuResult cuCtxSynchronize();
            [LibraryImport(CudaDriverApiDllName)]
            public static partial CuResult cuCtxGetApiVersion(CuContext ctx, ref uint version);
            [LibraryImport(CudaDriverApiDllName)]
            public static partial CuResult cuCtxGetCacheConfig(ref CuFuncCache pconfig);
            [LibraryImport(CudaDriverApiDllName)]
            public static partial CuResult cuCtxSetCacheConfig(CuFuncCache config);
            [LibraryImport(CudaDriverApiDllName)]
            public static partial CuResult cuCtxGetSharedMemConfig(ref CuSharedConfig pConfig);
            [LibraryImport(CudaDriverApiDllName)]
            public static partial CuResult cuCtxSetSharedMemConfig(CuSharedConfig config);
            [LibraryImport(CudaDriverApiDllName)]
            public static partial CuResult cuCtxGetStreamPriorityRange(ref int leastPriority, ref int greatestPriority);
            [LibraryImport(CudaDriverApiDllName)]
            public static partial CuResult cuCtxResetPersistingL2Cache();
            [LibraryImport(CudaDriverApiDllName)]
            public static partial CuResult cuCtxGetExecAffinity(ref CuExecAffinityParam pExecAffinity, CuExecAffinityType type);
            [LibraryImport(CudaDriverApiDllName)]
            public static partial CuResult cuCtxGetFlags(ref CuCtxFlags flags);
            [LibraryImport(CudaDriverApiDllName)]
            public static partial CuResult cuCtxGetId(CuContext ctx, ref ulong ctxId);
            [LibraryImport(CudaDriverApiDllName)]
            public static partial CuResult cuDevicePrimaryCtxRetain(ref CuContext pctx, CuDevice dev);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuDevicePrimaryCtxRelease_v2")]
            public static partial CuResult cuDevicePrimaryCtxRelease(CuDevice dev);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuDevicePrimaryCtxSetFlags_v2")]
            public static partial CuResult cuDevicePrimaryCtxSetFlags(CuDevice dev, CuCtxFlags flags);
            [LibraryImport(CudaDriverApiDllName)]
            public static partial CuResult cuDevicePrimaryCtxGetState(CuDevice dev, ref CuCtxFlags flags, ref int active);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuDevicePrimaryCtxReset_v2")]
            public static partial CuResult cuDevicePrimaryCtxReset(CuDevice dev);
        }
        public static partial class ModuleManagement
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
        public static partial class LibraryManagement
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
        public static partial class MemoryManagement
        {
            static MemoryManagement()
            {
                DriverApiNativeMethods.Init();
            }
            [LibraryImport(CudaDriverApiDllName)] public static partial CuResult cuMemGetInfo_v2(ref SizeT free, ref SizeT total);
            [LibraryImport(CudaDriverApiDllName)] public static partial CuResult cuMemAlloc_v2(ref CuDevicePtr dptr, SizeT bytesize);
            [LibraryImport(CudaDriverApiDllName)] public static partial CuResult cuMemAllocPitch_v2(ref CuDevicePtr dptr, ref SizeT pPitch, SizeT widthInBytes, SizeT height, uint elementSizeBytes);
            [LibraryImport(CudaDriverApiDllName)] public static partial CuResult cuMemFree_v2(CuDevicePtr dptr);
            [LibraryImport(CudaDriverApiDllName)] public static partial CuResult cuMemGetAddressRange_v2(ref CuDevicePtr pbase, ref SizeT psize, CuDevicePtr dptr);
            [LibraryImport(CudaDriverApiDllName)] public static partial CuResult cuMemAllocHost_v2(ref IntPtr pp, SizeT bytesize);
            [LibraryImport(CudaDriverApiDllName)] public static partial CuResult cuMemFreeHost(IntPtr p);
            [LibraryImport(CudaDriverApiDllName)] public static partial CuResult cuMemHostAlloc(ref IntPtr pp, SizeT bytesize, CuMemHostAllocFlags flags);
            [LibraryImport(CudaDriverApiDllName)] public static partial CuResult cuMemHostGetDevicePointer_v2(ref CuDevicePtr pdptr, IntPtr p, int flags);
            [LibraryImport(CudaDriverApiDllName)] public static partial CuResult cuMemHostGetFlags(ref CuMemHostAllocFlags pFlags, IntPtr p);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemHostRegister_v2")] public static partial CuResult cuMemHostRegister(IntPtr p, SizeT byteSize, CuMemHostRegisterFlags flags);
            [LibraryImport(CudaDriverApiDllName)] public static partial CuResult cuMemHostUnregister(IntPtr p);
            [LibraryImport(CudaDriverApiDllName)] public static partial CuResult cuPointerGetAttribute(ref CuContext data, CuPointerAttribute attribute, CuDevicePtr ptr);
            [LibraryImport(CudaDriverApiDllName)] public static partial CuResult cuPointerGetAttribute(ref CuMemoryType data, CuPointerAttribute attribute, CuDevicePtr ptr);
            [LibraryImport(CudaDriverApiDllName)] public static partial CuResult cuPointerGetAttribute(ref CuDevicePtr data, CuPointerAttribute attribute, CuDevicePtr ptr);
            [LibraryImport(CudaDriverApiDllName)] public static partial CuResult cuPointerGetAttribute(ref IntPtr data, CuPointerAttribute attribute, CuDevicePtr ptr);
            [LibraryImport(CudaDriverApiDllName)] public static partial CuResult cuPointerGetAttribute(ref CudaPointerAttributeP2PTokens data, CuPointerAttribute attribute, CuDevicePtr ptr);
            [LibraryImport(CudaDriverApiDllName)] public static partial CuResult cuPointerGetAttribute(ref int data, CuPointerAttribute attribute, CuDevicePtr ptr);
            [LibraryImport(CudaDriverApiDllName)] public static partial CuResult cuPointerGetAttribute(ref ulong data, CuPointerAttribute attribute, CuDevicePtr ptr);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemPrefetchAsync")] public static partial CuResult cuMemPrefetchAsync(CuDevicePtr devPtr, SizeT count, CuDevice dstDevice, CuStream hStream); 
            [LibraryImport(CudaDriverApiDllName)] public static partial CuResult cuMemAdvise(CuDevicePtr devPtr, SizeT count, CuMemAdvise advice, CuDevice device);
            [LibraryImport(CudaDriverApiDllName)] public static partial CuResult cuMemRangeGetAttribute(IntPtr data, SizeT dataSize, CuMemRangeAttribute attribute, CuDevicePtr devPtr, SizeT count);
            [LibraryImport(CudaDriverApiDllName)] public static partial CuResult cuMemRangeGetAttributes(IntPtr[] data, SizeT[] dataSizes, CuMemRangeAttribute[] attributes, SizeT numAttributes, CuDevicePtr devPtr, SizeT count);
            [LibraryImport(CudaDriverApiDllName)] public static partial CuResult cuMemAllocManaged(ref CuDevicePtr dptr, SizeT bytesize, CuMemAttachFlags flags);
            [LibraryImport(CudaDriverApiDllName)] public static partial CuResult cuPointerSetAttribute(ref int value, CuPointerAttribute attribute, CuDevicePtr ptr);
            [LibraryImport(CudaDriverApiDllName)] public static partial CuResult cuPointerGetAttributes(uint numAttributes, CuPointerAttribute[] attributes, IntPtr data, CuDevicePtr ptr);
            [LibraryImport(CudaDriverApiDllName)] public static partial CuResult cuMemAddressReserve(ref CuDevicePtr ptr, SizeT size, SizeT alignment, CuDevicePtr addr, ulong flags);
            [LibraryImport(CudaDriverApiDllName)] public static partial CuResult cuMemAddressFree(CuDevicePtr ptr, SizeT size);
            [LibraryImport(CudaDriverApiDllName)] public static partial CuResult cuMemCreate(ref CuMemGenericAllocationHandle handle, SizeT size, ref CuMemAllocationProp prop, ulong flags);
            [LibraryImport(CudaDriverApiDllName)] public static partial CuResult cuMemRelease(CuMemGenericAllocationHandle handle);
            [LibraryImport(CudaDriverApiDllName)] public static partial CuResult cuMemMap(CuDevicePtr ptr, SizeT size, SizeT offset, CuMemGenericAllocationHandle handle, ulong flags);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "pcuMemMapArrayAsync")] public static partial CuResult pcuMemMapArrayAsync(CuArrayMapInfo[] mapInfoList, uint count, CuStream hStream);
            [LibraryImport(CudaDriverApiDllName)] public static partial CuResult cuMemUnmap(CuDevicePtr ptr, SizeT size);
            [LibraryImport(CudaDriverApiDllName)] public static partial CuResult cuMemSetAccess(CuDevicePtr ptr, SizeT size, CuMemAccessDesc[] desc, SizeT count);
            [LibraryImport(CudaDriverApiDllName)] public static partial CuResult cuMemGetAccess(ref ulong flags, ref CuMemLocation location, CuDevicePtr ptr);
            [LibraryImport(CudaDriverApiDllName)] public static partial CuResult cuMemExportToShareableHandle(IntPtr shareableHandle, CuMemGenericAllocationHandle handle, CuMemAllocationHandleType handleType, ulong flags);
            [LibraryImport(CudaDriverApiDllName)] public static partial CuResult cuMemImportFromShareableHandle(ref CuMemGenericAllocationHandle handle, IntPtr osHandle, CuMemAllocationHandleType shHandleType);
            [LibraryImport(CudaDriverApiDllName)] public static partial CuResult cuMemGetAllocationGranularity(ref SizeT granularity, ref CuMemAllocationProp prop, CuMemAllocationGranularityFlags option);
            [LibraryImport(CudaDriverApiDllName)] public static partial CuResult cuMemGetAllocationPropertiesFromHandle(ref CuMemAllocationProp prop, CuMemGenericAllocationHandle handle);
            [LibraryImport(CudaDriverApiDllName)] public static partial CuResult cuMemRetainAllocationHandle(ref CuMemGenericAllocationHandle handle, IntPtr addr);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemFreeAsync")] public static partial CuResult cuMemFreeAsync(CuDevicePtr dptr, CuStream hStream);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemAllocAsync")] public static partial CuResult cuMemAllocAsync(ref CuDevicePtr dptr, SizeT bytesize, CuStream hStream);
            [LibraryImport(CudaDriverApiDllName)] public static partial CuResult cuMemPoolTrimTo(CuMemoryPool pool, SizeT minBytesToKeep);
            [LibraryImport(CudaDriverApiDllName)] public static partial CuResult cuMemPoolSetAttribute(CuMemoryPool pool, CuMemPoolAttribute attr, ref int value);
            [LibraryImport(CudaDriverApiDllName)] public static partial CuResult cuMemPoolSetAttribute(CuMemoryPool pool, CuMemPoolAttribute attr, ref ulong value);
            [LibraryImport(CudaDriverApiDllName)] public static partial CuResult cuMemPoolGetAttribute(CuMemoryPool pool, CuMemPoolAttribute attr, ref int value);
            [LibraryImport(CudaDriverApiDllName)] public static partial CuResult cuMemPoolGetAttribute(CuMemoryPool pool, CuMemPoolAttribute attr, ref ulong value);
            [LibraryImport(CudaDriverApiDllName)] public static partial CuResult cuMemPoolSetAccess(CuMemoryPool pool, CuMemAccessDesc[] map, SizeT count);
            [LibraryImport(CudaDriverApiDllName)] public static partial CuResult cuMemPoolGetAccess(ref CuMemAccessFlags flags, CuMemoryPool memPool, ref CuMemLocation location);
            [DllImport(CudaDriverApiDllName)] public static extern CuResult cuMemPoolCreate(ref CuMemoryPool pool, ref CuMemPoolProps poolProps);
            [LibraryImport(CudaDriverApiDllName)] public static partial CuResult cuMemPoolDestroy(CuMemoryPool pool);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemAllocFromPoolAsync")] public static partial CuResult cuMemAllocFromPoolAsync(ref CuDevicePtr dptr, SizeT bytesize, CuMemoryPool pool, CuStream hStream);
            [LibraryImport(CudaDriverApiDllName)] public static partial CuResult cuMemPoolExportToShareableHandle(ref IntPtr handleOut, CuMemoryPool pool, CuMemAllocationHandleType handleType, ulong flags);
            [LibraryImport(CudaDriverApiDllName)] public static partial CuResult cuMemPoolImportFromShareableHandle(ref CuMemoryPool poolOut, IntPtr handle, CuMemAllocationHandleType handleType, ulong flags);
            [DllImport(CudaDriverApiDllName)] public static extern CuResult cuMemPoolExportPointer(ref CuMemPoolPtrExportData shareDataOut, CuDevicePtr ptr);
            [DllImport(CudaDriverApiDllName)] public static extern CuResult cuMemPoolImportPointer(ref CuDevicePtr ptrOut, CuMemoryPool pool, ref CuMemPoolPtrExportData shareData);
            [LibraryImport(CudaDriverApiDllName)] public static partial CuResult cuMemGetHandleForAddressRange(IntPtr handle, CuDevicePtr dptr, SizeT size, CuMemRangeHandleType handleType, ulong flags);
        }
        public static partial class SynchronousMemcpyV2
        {
            static SynchronousMemcpyV2()
            {
                DriverApiNativeMethods.Init();
            }
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpy")] public static partial CuResult cuMemcpy(CuDevicePtr dst, CuDevicePtr src, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyPeer")] public static partial CuResult cuMemcpyPeer(CuDevicePtr dstDevice, CuContext dstContext, CuDevicePtr srcDevice, CuContext srcContext, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpy3DPeer")] public static partial CuResult cuMemcpy3DPeer(ref CudaMemCpy3DPeer pCopy);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2")] public static partial CuResult cuMemcpyHtoD_v2(CuDevicePtr dstDevice, Dim3[] srcHost, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2")] public static partial CuResult cuMemcpyHtoD_v2(CuDevicePtr dstDevice, byte[] srcHost, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2")] public static partial CuResult cuMemcpyHtoD_v2(CuDevicePtr dstDevice, sbyte[] srcHost, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2")] public static partial CuResult cuMemcpyHtoD_v2(CuDevicePtr dstDevice, ushort[] srcHost, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2")] public static partial CuResult cuMemcpyHtoD_v2(CuDevicePtr dstDevice, short[] srcHost, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2")] public static partial CuResult cuMemcpyHtoD_v2(CuDevicePtr dstDevice, uint[] srcHost, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2")] public static partial CuResult cuMemcpyHtoD_v2(CuDevicePtr dstDevice, int[] srcHost, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2")] public static partial CuResult cuMemcpyHtoD_v2(CuDevicePtr dstDevice, ulong[] srcHost, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2")] public static partial CuResult cuMemcpyHtoD_v2(CuDevicePtr dstDevice, long[] srcHost, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2")] public static partial CuResult cuMemcpyHtoD_v2(CuDevicePtr dstDevice, float[] srcHost, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2")] public static partial CuResult cuMemcpyHtoD_v2(CuDevicePtr dstDevice, double[] srcHost, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2")] public static partial CuResult cuMemcpyHtoD_v2(CuDevicePtr dstDevice, ref Dim3 srcHost, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2")] public static partial CuResult cuMemcpyHtoD_v2(CuDevicePtr dstDevice, ref byte srcHost, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2")] public static partial CuResult cuMemcpyHtoD_v2(CuDevicePtr dstDevice, ref sbyte srcHost, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2")] public static partial CuResult cuMemcpyHtoD_v2(CuDevicePtr dstDevice, ref ushort srcHost, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2")] public static partial CuResult cuMemcpyHtoD_v2(CuDevicePtr dstDevice, ref short srcHost, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2")] public static partial CuResult cuMemcpyHtoD_v2(CuDevicePtr dstDevice, ref uint srcHost, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2")] public static partial CuResult cuMemcpyHtoD_v2(CuDevicePtr dstDevice, ref int srcHost, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2")] public static partial CuResult cuMemcpyHtoD_v2(CuDevicePtr dstDevice, ref ulong srcHost, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2")] public static partial CuResult cuMemcpyHtoD_v2(CuDevicePtr dstDevice, ref long srcHost, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2")] public static partial CuResult cuMemcpyHtoD_v2(CuDevicePtr dstDevice, ref float srcHost, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2")] public static partial CuResult cuMemcpyHtoD_v2(CuDevicePtr dstDevice, ref double srcHost, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoD_v2")] public static partial CuResult cuMemcpyHtoD_v2(CuDevicePtr dstDevice, IntPtr srcHost, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2")] public static partial CuResult cuMemcpyDtoH_v2(Dim3[] dstHost, CuDevicePtr srcDevice, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2")] public static partial CuResult cuMemcpyDtoH_v2(byte[] dstHost, CuDevicePtr srcDevice, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2")] public static partial CuResult cuMemcpyDtoH_v2(sbyte[] dstHost, CuDevicePtr srcDevice, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2")] public static partial CuResult cuMemcpyDtoH_v2(ushort[] dstHost, CuDevicePtr srcDevice, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2")] public static partial CuResult cuMemcpyDtoH_v2(short[] dstHost, CuDevicePtr srcDevice, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2")] public static partial CuResult cuMemcpyDtoH_v2(uint[] dstHost, CuDevicePtr srcDevice, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2")] public static partial CuResult cuMemcpyDtoH_v2(int[] dstHost, CuDevicePtr srcDevice, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2")] public static partial CuResult cuMemcpyDtoH_v2(ulong[] dstHost, CuDevicePtr srcDevice, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2")] public static partial CuResult cuMemcpyDtoH_v2(long[] dstHost, CuDevicePtr srcDevice, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2")] public static partial CuResult cuMemcpyDtoH_v2(float[] dstHost, CuDevicePtr srcDevice, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2")] public static partial CuResult cuMemcpyDtoH_v2(double[] dstHost, CuDevicePtr srcDevice, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2")] public static partial CuResult cuMemcpyDtoH_v2(ref Dim3 dstHost, CuDevicePtr srcDevice, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2")] public static partial CuResult cuMemcpyDtoH_v2(ref byte dstHost, CuDevicePtr srcDevice, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2")] public static partial CuResult cuMemcpyDtoH_v2(ref sbyte dstHost, CuDevicePtr srcDevice, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2")] public static partial CuResult cuMemcpyDtoH_v2(ref ushort dstHost, CuDevicePtr srcDevice, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2")] public static partial CuResult cuMemcpyDtoH_v2(ref short dstHost, CuDevicePtr srcDevice, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2")] public static partial CuResult cuMemcpyDtoH_v2(ref uint dstHost, CuDevicePtr srcDevice, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2")] public static partial CuResult cuMemcpyDtoH_v2(ref int dstHost, CuDevicePtr srcDevice, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2")] public static partial CuResult cuMemcpyDtoH_v2(ref ulong dstHost, CuDevicePtr srcDevice, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2")] public static partial CuResult cuMemcpyDtoH_v2(ref long dstHost, CuDevicePtr srcDevice, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2")] public static partial CuResult cuMemcpyDtoH_v2(ref float dstHost, CuDevicePtr srcDevice, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2")] public static partial CuResult cuMemcpyDtoH_v2(ref double dstHost, CuDevicePtr srcDevice, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoH_v2")] public static partial CuResult cuMemcpyDtoH_v2(IntPtr dstHost, CuDevicePtr srcDevice, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoD_v2")] public static partial CuResult cuMemcpyDtoD_v2(CuDevicePtr dstDevice, CuDevicePtr srcDevice, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoA_v2")] public static partial CuResult cuMemcpyDtoA_v2(CuArray dstArray, SizeT dstOffset, CuDevicePtr srcDevice, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyAtoD_v2")] public static partial CuResult cuMemcpyAtoD_v2(CuDevicePtr dstDevice, CuArray srcArray, SizeT srcOffset, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoA_v2")] public static partial CuResult cuMemcpyHtoA_v2(CuArray dstArray, SizeT dstOffset, Dim3[] srcHost, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoA_v2")] public static partial CuResult cuMemcpyHtoA_v2(CuArray dstArray, SizeT dstOffset, byte[] srcHost, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoA_v2")] public static partial CuResult cuMemcpyHtoA_v2(CuArray dstArray, SizeT dstOffset, sbyte[] srcHost, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoA_v2")] public static partial CuResult cuMemcpyHtoA_v2(CuArray dstArray, SizeT dstOffset, ushort[] srcHost, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoA_v2")] public static partial CuResult cuMemcpyHtoA_v2(CuArray dstArray, SizeT dstOffset, short[] srcHost, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoA_v2")] public static partial CuResult cuMemcpyHtoA_v2(CuArray dstArray, SizeT dstOffset, uint[] srcHost, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoA_v2")] public static partial CuResult cuMemcpyHtoA_v2(CuArray dstArray, SizeT dstOffset, int[] srcHost, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoA_v2")] public static partial CuResult cuMemcpyHtoA_v2(CuArray dstArray, SizeT dstOffset, ulong[] srcHost, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoA_v2")] public static partial CuResult cuMemcpyHtoA_v2(CuArray dstArray, SizeT dstOffset, long[] srcHost, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoA_v2")] public static partial CuResult cuMemcpyHtoA_v2(CuArray dstArray, SizeT dstOffset, float[] srcHost, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoA_v2")] public static partial CuResult cuMemcpyHtoA_v2(CuArray dstArray, SizeT dstOffset, double[] srcHost, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoA_v2")] public static partial CuResult cuMemcpyHtoA_v2(CuArray dstArray, SizeT dstOffset, IntPtr srcHost, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyAtoH_v2")] public static partial CuResult cuMemcpyAtoH_v2(Dim3[] dstHost, CuArray srcArray, SizeT srcOffset, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyAtoH_v2")] public static partial CuResult cuMemcpyAtoH_v2(byte[] dstHost, CuArray srcArray, SizeT srcOffset, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyAtoH_v2")] public static partial CuResult cuMemcpyAtoH_v2(sbyte[] dstHost, CuArray srcArray, SizeT srcOffset, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyAtoH_v2")] public static partial CuResult cuMemcpyAtoH_v2(ushort[] dstHost, CuArray srcArray, SizeT srcOffset, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyAtoH_v2")] public static partial CuResult cuMemcpyAtoH_v2(short[] dstHost, CuArray srcArray, SizeT srcOffset, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyAtoH_v2")] public static partial CuResult cuMemcpyAtoH_v2(uint[] dstHost, CuArray srcArray, SizeT srcOffset, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyAtoH_v2")] public static partial CuResult cuMemcpyAtoH_v2(int[] dstHost, CuArray srcArray, SizeT srcOffset, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyAtoH_v2")] public static partial CuResult cuMemcpyAtoH_v2(ulong[] dstHost, CuArray srcArray, SizeT srcOffset, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyAtoH_v2")] public static partial CuResult cuMemcpyAtoH_v2(long[] dstHost, CuArray srcArray, SizeT srcOffset, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyAtoH_v2")] public static partial CuResult cuMemcpyAtoH_v2(float[] dstHost, CuArray srcArray, SizeT srcOffset, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyAtoH_v2")] public static partial CuResult cuMemcpyAtoH_v2(double[] dstHost, CuArray srcArray, SizeT srcOffset, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyAtoH_v2")] public static partial CuResult cuMemcpyAtoH_v2(IntPtr dstHost, CuArray srcArray, SizeT srcOffset, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyAtoA_v2")] public static partial CuResult cuMemcpyAtoA_v2(CuArray dstArray, SizeT dstOffset, CuArray srcArray, SizeT srcOffset, SizeT byteCount);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpy2D_v2")] public static partial CuResult cuMemcpy2D_v2(ref CudaMemCpy2D pCopy);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpy2DUnaligned_v2")] public static partial CuResult cuMemcpy2DUnaligned_v2(ref CudaMemCpy2D pCopy);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpy3D_v2")] public static partial CuResult cuMemcpy3D_v2(ref CudaMemCpy3D pCopy);
        }
        public static partial class AsynchronousMemcpyV2
        {
            static AsynchronousMemcpyV2()
            {
                DriverApiNativeMethods.Init();
            }
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyAsync")] public static partial CuResult cuMemcpyAsync(CuDevicePtr dst, CuDevicePtr src, SizeT byteCount, CuStream hStream);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyPeerAsync")] public static partial CuResult cuMemcpyPeerAsync(CuDevicePtr dstDevice, CuContext dstContext, CuDevicePtr srcDevice, CuContext srcContext, SizeT byteCount, CuStream hStream);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpy3DPeerAsync")] public static partial CuResult cuMemcpy3DPeerAsync(ref CudaMemCpy3DPeer pCopy, CuStream hStream);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoDAsync_v2")] public static partial CuResult cuMemcpyHtoDAsync_v2(CuDevicePtr dstDevice, IntPtr srcHost, SizeT byteCount, CuStream hStream);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoHAsync_v2")] public static partial CuResult cuMemcpyDtoHAsync_v2(IntPtr dstHost, CuDevicePtr srcDevice, SizeT byteCount, CuStream hStream);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyDtoDAsync_v2")] public static partial CuResult cuMemcpyDtoDAsync_v2(CuDevicePtr dstDevice, CuDevicePtr srcDevice, SizeT byteCount, CuStream hStream);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyHtoAAsync_v2")] public static partial CuResult cuMemcpyHtoAAsync_v2(CuArray dstArray, SizeT dstOffset, IntPtr srcHost, SizeT byteCount, CuStream hStream);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpyAtoHAsync_v2")] public static partial CuResult cuMemcpyAtoHAsync_v2(IntPtr dstHost, CuArray srcArray, SizeT srcOffset, SizeT byteCount, CuStream hStream);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpy2DAsync_v2")] public static partial CuResult cuMemcpy2DAsync_v2(ref CudaMemCpy2D pCopy, CuStream hStream);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemcpy3DAsync_v2")] public static partial CuResult cuMemcpy3DAsync_v2(ref CudaMemCpy3D pCopy, CuStream hStream);
        }
        public static partial class Memset
        {
            static Memset()
            {
                DriverApiNativeMethods.Init();
            }
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemsetD8_v2")] public static partial CuResult cuMemsetD8_v2(CuDevicePtr dstDevice, byte b, SizeT n);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemsetD16_v2")] public static partial CuResult cuMemsetD16_v2(CuDevicePtr dstDevice, ushort us, SizeT n);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemsetD32_v2")] public static partial CuResult cuMemsetD32_v2(CuDevicePtr dstDevice, uint ui, SizeT n);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemsetD2D8_v2")] public static partial CuResult cuMemsetD2D8_v2(CuDevicePtr dstDevice, SizeT dstPitch, byte b, SizeT width, SizeT height);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemsetD2D16_v2")] public static partial CuResult cuMemsetD2D16_v2(CuDevicePtr dstDevice, SizeT dstPitch, ushort us, SizeT width, SizeT height);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemsetD2D32_v2")] public static partial CuResult cuMemsetD2D32_v2(CuDevicePtr dstDevice, SizeT dstPitch, uint ui, SizeT width, SizeT height);
        }
        public static partial class MemsetAsync
        {
            static MemsetAsync()
            {
                DriverApiNativeMethods.Init();
            }
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemsetD8Async")] public static partial CuResult cuMemsetD8Async(CuDevicePtr dstDevice, byte b, SizeT n, CuStream hStream);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemsetD16Async")] public static partial CuResult cuMemsetD16Async(CuDevicePtr dstDevice, ushort us, SizeT n, CuStream hStream);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemsetD32Async")] public static partial CuResult cuMemsetD32Async(CuDevicePtr dstDevice, uint ui, SizeT n, CuStream hStream);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemsetD2D8Async")] public static partial CuResult cuMemsetD2D8Async(CuDevicePtr dstDevice, SizeT dstPitch, byte b, SizeT width, SizeT height, CuStream hStream);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemsetD2D16Async")] public static partial CuResult cuMemsetD2D16Async(CuDevicePtr dstDevice, SizeT dstPitch, ushort us, SizeT width, SizeT height, CuStream hStream);
            [LibraryImport(CudaDriverApiDllName, EntryPoint = "cuMemsetD2D32Async")] public static partial CuResult cuMemsetD2D32Async(CuDevicePtr dstDevice, SizeT dstPitch, uint ui, SizeT width, SizeT height, CuStream hStream);
        }
        public static partial class FunctionManagement
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
        public static partial class ArrayManagement
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
        public static partial class Launch
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
        public static partial class Events
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
        public static partial class Streams
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
        public static partial class Limits
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
        public static partial class CudaPeerAccess
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
        public static partial class Profiling
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
        public static partial class ErrorHandling
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
        public static partial class Occupancy
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
        public static partial class GraphManagement
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
        public static partial class TensorCoreManagement
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