using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.Cuda.CudaToolkit
{
    /// <summary>
    /// Cuda function / kernel
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CUfunction
    {
        /// <summary>
        /// 
        /// </summary>
        public IntPtr Pointer;

        /// <summary>
        /// Returns a module handle<para/>
        /// Returns in \p *hmod the handle of the module that function \p hfunc
        /// is located in. The lifetime of the module corresponds to the lifetime of
        /// the context it was loaded in or until the module is explicitly unloaded.<para/>
        /// The CUDA runtime manages its own modules loaded into the primary context.
        /// If the handle returned by this API refers to a module loaded by the CUDA runtime,
        /// calling ::cuModuleUnload() on that module will result in undefined behavior.
        /// </summary>
        public CUmodule GetModule()
        {
            CUmodule temp = new CUmodule();
            CUResult res = DriverAPINativeMethods.FunctionManagement.cuFuncGetModule(ref temp, this);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuFuncGetModule", res));
            if (res != CUResult.Success) throw new CudaException(res);
            return temp;
        }
    }

    /// <summary>
    /// Cuda stream
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CUstream
    {
        /// <summary>
        /// 
        /// </summary>
        public IntPtr Pointer;

        /// <summary>
        /// Returns the CUDA NULL stream (0)
        /// </summary>
        public static CUstream NullStream
        {
            get
            {
                CUstream s = new CUstream();
                s.Pointer = (IntPtr)0;
                return s;
            }
        }

        /// <summary>
        /// Stream handle that can be passed as a CUstream to use an implicit stream
        /// with legacy synchronization behavior.
        /// </summary>
        public static CUstream LegacyStream
        {
            get
            {
                CUstream s = new CUstream();
                s.Pointer = (IntPtr)1;
                return s;
            }
        }

        /// <summary>
        /// Stream handle that can be passed as a CUstream to use an implicit stream
        /// with per-thread synchronization behavior.
        /// </summary>
        public static CUstream StreamPerThread
        {
            get
            {
                CUstream s = new CUstream();
                s.Pointer = (IntPtr)2;
                return s;
            }
        }

        /// <summary>
        /// Returns the unique Id associated with the stream handle
        /// </summary>
        public ulong ID
        {
            get
            {
                ulong ret = 0;
                CUResult res = DriverAPINativeMethods.Streams.cuStreamGetId(this, ref ret);
                Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuStreamGetId", res));
                if (res != CUResult.Success) throw new CudaException(res);
                return ret;
            }
        }
    }

    /// <summary>
    /// Error codes returned by CUDA driver API calls
    /// </summary>
    public enum CUResult
    {
        /// <summary>
        /// No errors
        /// </summary>
        Success = 0,

        /// <summary>
        /// Invalid value
        /// </summary>
        ErrorInvalidValue = 1,

        /// <summary>
        /// Out of memory
        /// </summary>
        ErrorOutOfMemory = 2,

        /// <summary>
        /// Driver not initialized
        /// </summary>
        ErrorNotInitialized = 3,

        /// <summary>
        /// Driver deinitialized
        /// </summary>
        ErrorDeinitialized = 4,

        /// <summary>
        /// This indicates profiler is not initialized for this run. This can happen when the application is running with external profiling tools
        /// like visual profiler.
        /// </summary>
        ErrorProfilerDisabled = 5,

        /// <summary>
        /// This error return is deprecated as of CUDA 5.0. It is no longer an error
        /// to attempt to enable/disable the profiling via ::cuProfilerStart or
        /// ::cuProfilerStop without initialization.
        /// </summary>
        [Obsolete("deprecated as of CUDA 5.0")]
        ErrorProfilerNotInitialized = 6,

        /// <summary>
        /// This error return is deprecated as of CUDA 5.0. It is no longer an error
        /// to call cuProfilerStart() when profiling is already enabled.
        /// </summary>
        [Obsolete("deprecated as of CUDA 5.0")]
        ErrorProfilerAlreadyStarted = 7,

        /// <summary>
        /// This error return is deprecated as of CUDA 5.0. It is no longer an error
        /// to call cuProfilerStop() when profiling is already disabled.
        /// </summary>
        [Obsolete("deprecated as of CUDA 5.0")]
        ErrorProfilerAlreadyStopped = 8,

        /// <summary>
        /// This indicates that the CUDA driver that the application has loaded is a
        /// stub library. Applications that run with the stub rather than a real
        /// driver loaded will result in CUDA API returning this error.
        /// </summary>
        ErrorStubLibrary = 34,

        /// <summary>
        /// This indicates that requested CUDA device is unavailable at the current
        /// time. Devices are often unavailable due to use of
        /// ::CU_COMPUTEMODE_EXCLUSIVE_PROCESS or ::CU_COMPUTEMODE_PROHIBITED.
        /// </summary>
        ErrorDeviceUnavailable = 46,

        /// <summary>
        /// No CUDA-capable device available
        /// </summary>
        ErrorNoDevice = 100,

        /// <summary>
        /// Invalid device
        /// </summary>
        ErrorInvalidDevice = 101,

        /// <summary>
        /// This error indicates that the Grid license is not applied.
        /// </summary>
        DeviceNotLicensed = 102,



        /// <summary>
        /// Invalid kernel image
        /// </summary>
        ErrorInvalidImage = 200,

        /// <summary>
        /// Invalid context
        /// </summary>
        ErrorInvalidContext = 201,

        /// <summary>
        /// Context already current
        /// </summary>
        [Obsolete("This error return is deprecated as of CUDA 3.2. It is no longer an error to attempt to push the active context via cuCtxPushCurrent().")]
        ErrorContextAlreadyCurrent = 202,

        /// <summary>
        /// Map failed
        /// </summary>
        ErrorMapFailed = 205,

        /// <summary>
        /// Unmap failed
        /// </summary>
        ErrorUnmapFailed = 206,

        /// <summary>
        /// Array is mapped
        /// </summary>
        ErrorArrayIsMapped = 207,

        /// <summary>
        /// Already mapped
        /// </summary>
        ErrorAlreadyMapped = 208,

        /// <summary>
        /// No binary for GPU
        /// </summary>
        ErrorNoBinaryForGPU = 209,

        /// <summary>
        /// Already acquired
        /// </summary>
        ErrorAlreadyAcquired = 210,

        /// <summary>
        /// Not mapped
        /// </summary>
        ErrorNotMapped = 211,

        /// <summary>
        /// Mapped resource not available for access as an array
        /// </summary>
        ErrorNotMappedAsArray = 212,

        /// <summary>
        /// Mapped resource not available for access as a pointer
        /// </summary>
        ErrorNotMappedAsPointer = 213,

        /// <summary>
        /// Uncorrectable ECC error detected
        /// </summary>
        ErrorECCUncorrectable = 214,

        /// <summary>
        /// CULimit not supported by device
        /// </summary>
        ErrorUnsupportedLimit = 215,

        /// <summary>
        /// This indicates that the <see cref="CUcontext"/> passed to the API call can
        /// only be bound to a single CPU thread at a time but is already 
        /// bound to a CPU thread.
        /// </summary>
        ErrorContextAlreadyInUse = 216,

        /// <summary>
        /// This indicates that peer access is not supported across the given devices.
        /// </summary>
        ErrorPeerAccessUnsupported = 217,

        /// <summary>
        /// This indicates that a PTX JIT compilation failed.
        /// </summary>
        ErrorInvalidPtx = 218,

        /// <summary>
        /// This indicates an error with OpenGL or DirectX context.
        /// </summary>
        ErrorInvalidGraphicsContext = 219,

        /// <summary>
        /// This indicates that an uncorrectable NVLink error was detected during the execution.
        /// </summary>
        NVLinkUncorrectable = 220,

        /// <summary>
        /// This indicates that the PTX JIT compiler library was not found.
        /// </summary>
        JITCompilerNotFound = 221,

        /// <summary>
        /// This indicates that the provided PTX was compiled with an unsupported toolchain.
        /// </summary>
        UnsupportedPTXVersion = 222,

        /// <summary>
        /// This indicates that the PTX JIT compilation was disabled.
        /// </summary>
        JITCompilationDisabled = 223,

        /// <summary>
        /// This indicates that the ::CUexecAffinityType passed to the API call is not supported by the active device.
        /// </summary>
        UnsupportedExecAffinity = 224,

        /// <summary>
        /// This indicates that the device kernel source is invalid. This includes
        /// compilation/linker errors encountered in device code or user error.
        /// </summary>
        ErrorInvalidSource = 300,

        /// <summary>
        /// File not found
        /// </summary>
        ErrorFileNotFound = 301,

        /// <summary>
        /// Link to a shared object failed to resolve
        /// </summary>
        ErrorSharedObjectSymbolNotFound = 302,

        /// <summary>
        /// Shared object initialization failed
        /// </summary>
        ErrorSharedObjectInitFailed = 303,

        /// <summary>
        /// OS call failed
        /// </summary>
        ErrorOperatingSystem = 304,

        /// <summary>
        /// Invalid handle
        /// </summary>
        ErrorInvalidHandle = 400,

        /// <summary>
        /// This indicates that a resource required by the API call is not in a
        /// valid state to perform the requested operation.
        /// </summary>
        ErrorIllegalState = 401,

        /// <summary>
        /// Not found
        /// </summary>
        ErrorNotFound = 500,


        /// <summary>
        /// CUDA not ready
        /// </summary>
        ErrorNotReady = 600,


        /// <summary>
        /// While executing a kernel, the device encountered a
        /// load or store instruction on an invalid memory address.
        /// This leaves the process in an inconsistent state and any further CUDA work
        /// will return the same error. To continue using CUDA, the process must be terminated
        /// and relaunched.
        /// </summary>
        ErrorIllegalAddress = 700,

        /// <summary>
        /// Launch exceeded resources
        /// </summary>
        ErrorLaunchOutOfResources = 701,

        /// <summary>
        /// This indicates that the device kernel took too long to execute. This can
        /// only occur if timeouts are enabled - see the device attribute
        /// ::CU_DEVICE_ATTRIBUTE_KERNEL_EXEC_TIMEOUT for more information.
        /// This leaves the process in an inconsistent state and any further CUDA work
        /// will return the same error. To continue using CUDA, the process must be terminated
        /// and relaunched.
        /// </summary>
        ErrorLaunchTimeout = 702,

        /// <summary>
        /// Launch with incompatible texturing
        /// </summary>
        ErrorLaunchIncompatibleTexturing = 703,

        /// <summary>
        /// This error indicates that a call to <see cref="DriverAPINativeMethods.CudaPeerAccess.cuCtxEnablePeerAccess"/> is
        /// trying to re-enable peer access to a context which has already
        /// had peer access to it enabled.
        /// </summary>
        ErrorPeerAccessAlreadyEnabled = 704,

        /// <summary>
        /// This error indicates that <see cref="DriverAPINativeMethods.CudaPeerAccess.cuCtxDisablePeerAccess"/> is 
        /// trying to disable peer access which has not been enabled yet 
        /// via <see cref="DriverAPINativeMethods.CudaPeerAccess.cuCtxEnablePeerAccess"/>. 
        /// </summary>
        ErrorPeerAccessNotEnabled = 705,

        /// <summary>
        /// This error indicates that the primary context for the specified device
        /// has already been initialized.
        /// </summary>
        ErrorPrimaryContextActice = 708,

        /// <summary>
        /// This error indicates that the context current to the calling thread
        /// has been destroyed using <see cref="DriverAPINativeMethods.ContextManagement.cuCtxDestroy_v2"/>, or is a primary context which
        /// has not yet been initialized. 
        /// </summary>
        ErrorContextIsDestroyed = 709,

        /// <summary>
        /// A device-side assert triggered during kernel execution. The context
        /// cannot be used anymore, and must be destroyed. All existing device 
        /// memory allocations from this context are invalid and must be 
        /// reconstructed if the program is to continue using CUDA.
        /// </summary>
        ErrorAssert = 710,

        /// <summary>
        /// This error indicates that the hardware resources required to enable
        /// peer access have been exhausted for one or more of the devices 
        /// passed to ::cuCtxEnablePeerAccess().
        /// </summary>
        ErrorTooManyPeers = 711,

        /// <summary>
        /// This error indicates that the memory range passed to ::cuMemHostRegister()
        /// has already been registered.
        /// </summary>
        ErrorHostMemoryAlreadyRegistered = 712,

        /// <summary>
        /// This error indicates that the pointer passed to ::cuMemHostUnregister()
        /// does not correspond to any currently registered memory region.
        /// </summary>
        ErrorHostMemoryNotRegistered = 713,

        /// <summary>
        /// While executing a kernel, the device encountered a stack error.
        /// This can be due to stack corruption or exceeding the stack size limit.
        /// This leaves the process in an inconsistent state and any further CUDA work
        /// will return the same error. To continue using CUDA, the process must be terminated
        /// and relaunched.
        /// </summary>
        ErrorHardwareStackError = 714,

        /// <summary>
        /// While executing a kernel, the device encountered an illegal instruction.
        /// This leaves the process in an inconsistent state and any further CUDA work
        /// will return the same error. To continue using CUDA, the process must be terminated
        /// and relaunched.
        /// </summary>
        ErrorIllegalInstruction = 715,

        /// <summary>
        /// While executing a kernel, the device encountered a load or store instruction
        /// on a memory address which is not aligned.
        /// This leaves the process in an inconsistent state and any further CUDA work
        /// will return the same error. To continue using CUDA, the process must be terminated
        /// and relaunched.
        /// </summary>
        ErrorMisalignedAddress = 716,

        /// <summary>
        /// While executing a kernel, the device encountered an instruction
        /// which can only operate on memory locations in certain address spaces
        /// (global, shared, or local), but was supplied a memory address not
        /// belonging to an allowed address space.
        /// This leaves the process in an inconsistent state and any further CUDA work
        /// will return the same error. To continue using CUDA, the process must be terminated
        /// and relaunched.
        /// </summary>
        ErrorInvalidAddressSpace = 717,

        /// <summary>
        /// While executing a kernel, the device program counter wrapped its address space.
        /// This leaves the process in an inconsistent state and any further CUDA work
        /// will return the same error. To continue using CUDA, the process must be terminated
        /// and relaunched.
        /// </summary>
        ErrorInvalidPC = 718,

        /// <summary>
        /// An exception occurred on the device while executing a kernel. Common
        /// causes include dereferencing an invalid device pointer and accessing
        /// out of bounds shared memory. This leaves the process in an inconsistent state and any further CUDA work
        /// will return the same error. To continue using CUDA, the process must be terminated
        /// and relaunched.
        /// </summary>
        ErrorLaunchFailed = 719,

        /// <summary>
        /// This error indicates that the number of blocks launched per grid for a kernel that was
        /// launched via either ::cuLaunchCooperativeKernel or ::cuLaunchCooperativeKernelMultiDevice
        /// exceeds the maximum number of blocks as allowed by ::cuOccupancyMaxActiveBlocksPerMultiprocessor
        /// or ::cuOccupancyMaxActiveBlocksPerMultiprocessorWithFlags times the number of multiprocessors
        /// as specified by the device attribute ::CU_DEVICE_ATTRIBUTE_MULTIPROCESSOR_COUNT.
        /// </summary>
        ErrorCooperativeLaunchTooLarge = 720,




        //Removed in update CUDA version 3.1 -> 3.2
        ///// <summary>
        ///// Attempted to retrieve 64-bit pointer via 32-bit API function
        ///// </summary>
        //ErrorPointerIs64Bit = 800,

        ///// <summary>
        ///// Attempted to retrieve 64-bit size via 32-bit API function
        ///// </summary>
        //ErrorSizeIs64Bit = 801, 

        /// <summary>
        /// This error indicates that the attempted operation is not permitted.
        /// </summary>
        ErrorNotPermitted = 800,

        /// <summary>
        /// This error indicates that the attempted operation is not supported
        /// on the current system or device.
        /// </summary>
        ErrorNotSupported = 801,

        /// <summary>
        /// This error indicates that the system is not yet ready to start any CUDA
        /// work.  To continue using CUDA, verify the system configuration is in a
        /// valid state and all required driver daemons are actively running.
        /// </summary>
        ErrorSystemNotReady = 802,

        /// <summary>
        /// This error indicates that there is a mismatch between the versions of
        /// the display driver and the CUDA driver. Refer to the compatibility documentation
        /// for supported versions.
        /// </summary>
        ErrorSystemDriverMismatch = 803,

        /// <summary>
        /// This error indicates that the system was upgraded to run with forward compatibility
        /// but the visible hardware detected by CUDA does not support this configuration.
        /// Refer to the compatibility documentation for the supported hardware matrix or ensure
        /// that only supported hardware is visible during initialization via the CUDA_VISIBLE_DEVICES
        /// environment variable.
        /// </summary>
        ErrorCompatNotSupportedOnDevice = 804,


        /// <summary>
        /// This error indicates that the MPS client failed to connect to the MPS control daemon or the MPS server.
        /// </summary>
        MpsConnectionFailed = 805,

        /// <summary>
        /// This error indicates that the remote procedural call between the MPS server and the MPS client failed.
        /// </summary>
        MpsRpcFailure = 806,

        /// <summary>
        /// This error indicates that the MPS server is not ready to accept new MPS client requests. This error can be returned when the MPS server is in the process of recovering from a fatal failure.
        /// </summary>
        MpsServerNotReady = 807,

        /// <summary>
        /// This error indicates that the hardware resources required to create MPS client have been exhausted.
        /// </summary>
        MpsMaxClientsReached = 808,

        /// <summary>
        /// This error indicates the the hardware resources required to support device connections have been exhausted.
        /// </summary>
        MpsMaxConnectionsReached = 809,

        /// <summary>
        /// This error indicates that the MPS client has been terminated by the server. To continue using CUDA, the process must be terminated and relaunched.
        /// </summary>
        ErrorMPSClinetTerminated = 810,

        /// <summary>
        /// This error indicates that the module is using CUDA Dynamic Parallelism, but the current configuration, like MPS, does not support it.
        /// </summary>
        ErrorCDPNotSupported = 811,

        /// <summary>
        /// This error indicates that a module contains an unsupported interaction between different versions of CUDA Dynamic Parallelism.
        /// </summary>
        ErrorCDPVersionMismatch = 812,

        /// <summary>
        /// This error indicates that the operation is not permitted when the stream is capturing.
        /// </summary>
        ErrorStreamCaptureUnsupported = 900,

        /// <summary>
        /// This error indicates that the current capture sequence on the stream
        /// has been invalidated due to a previous error.
        /// </summary>
        ErrorStreamCaptureInvalidated = 901,

        /// <summary>
        /// This error indicates that the operation would have resulted in a merge of two independent capture sequences.
        /// </summary>
        ErrorStreamCaptureMerge = 902,

        /// <summary>
        /// This error indicates that the capture was not initiated in this stream.
        /// </summary>
        ErrorStreamCaptureUnmatched = 903,

        /// <summary>
        /// This error indicates that the capture sequence contains a fork that was not joined to the primary stream.
        /// </summary>
        ErrorStreamCaptureUnjoined = 904,

        /// <summary>
        /// This error indicates that a dependency would have been created which
        /// crosses the capture sequence boundary. Only implicit in-stream ordering
        /// dependencies are allowed to cross the boundary.
        /// </summary>
        ErrorStreamCaptureIsolation = 905,

        /// <summary>
        /// This error indicates a disallowed implicit dependency on a current capture sequence from cudaStreamLegacy.
        /// </summary>
        ErrorStreamCaptureImplicit = 906,

        /// <summary>
        /// This error indicates that the operation is not permitted on an event which
        /// was last recorded in a capturing stream.
        /// </summary>
        ErrorCapturedEvent = 907,

        /// <summary>
        /// A stream capture sequence not initiated with the ::CU_STREAM_CAPTURE_MODE_RELAXED
        /// argument to ::cuStreamBeginCapture was passed to ::cuStreamEndCapture in a
        /// different thread.
        /// </summary>
        ErrorStreamCaptureWrongThread = 908,

        /// <summary>
        /// This error indicates that the timeout specified for the wait operation has lapsed.
        /// </summary>
        ErrorTimeOut = 909,

        /// <summary>
        /// This error indicates that the graph update was not performed because it included 
        /// changes which violated constraints specific to instantiated graph update.
        /// </summary>
        ErrorGraphExecUpdateFailure = 910,

        /// <summary>
        /// This indicates that an async error has occurred in a device outside of CUDA.
        /// If CUDA was waiting for an external device's signal before consuming shared data,
        /// the external device signaled an error indicating that the data is not valid for
        /// consumption. This leaves the process in an inconsistent state and any further CUDA
        /// work will return the same error. To continue using CUDA, the process must be
        /// terminated and relaunched.
        /// </summary>
        ErrorExternalDevice = 911,

        /// <summary>
        /// Indicates a kernel launch error due to cluster misconfiguration.
        /// </summary>
        ErrorInvalidClusterSize = 912,
        /// <summary>
        /// Unknown error
        /// </summary>
        ErrorUnknown = 999,
    }

    /// <summary>
    /// Cuda device
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CUdevice
    {
        /// <summary>
        /// 
        /// </summary>
        public int Pointer;

        /// <summary>
        /// Device that represents the CPU
        /// </summary>
        static CUdevice CPU
        {
            get
            {
                CUdevice cpu = new CUdevice();
                cpu.Pointer = -1;
                return cpu;
            }
        }

        /// <summary>
        /// Device that represents an invalid device
        /// </summary>
        static CUdevice Invalid
        {
            get
            {
                CUdevice invalid = new CUdevice();
                invalid.Pointer = -2;
                return invalid;
            }
        }

        /// <summary>
        /// Sets the current memory pool of a device<para/>
        /// The memory pool must be local to the specified device.
        /// ::cuMemAllocAsync allocates from the current mempool of the provided stream's device.
        /// By default, a device's current memory pool is its default memory pool.
        /// <para/>
        /// note Use ::cuMemAllocFromPoolAsync to specify asynchronous allocations from a device different than the one the stream runs on.
        /// </summary>
        public void SetMemoryPool(CudaMemoryPool memPool)
        {
            CUResult res = DriverAPINativeMethods.DeviceManagement.cuDeviceSetMemPool(this, memPool.MemoryPool);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuDeviceSetMemPool", res));
            if (res != CUResult.Success) throw new CudaException(res);
        }

        /// <summary>
        /// Gets the current memory pool of the CUdevice. 
        /// </summary>
        public CudaMemoryPool GetMemoryPool()
        {
            return new CudaMemoryPool(this, false);
        }

        /// <summary>
        /// Gets the default memory pool of the CUdevice. 
        /// </summary>
        public CudaMemoryPool GetDefaultMemoryPool()
        {
            return new CudaMemoryPool(this, true);
        }

        /// <summary>
        /// Return an UUID for the device (11.4+)<para/>
        /// Returns 16-octets identifing the device \p dev in the structure
        /// pointed by the \p uuid.If the device is in MIG mode, returns its
        /// MIG UUID which uniquely identifies the subscribed MIG compute instance.
        /// Returns 16-octets identifing the device \p dev in the structure pointed by the \p uuid.
        /// </summary>
        public CUuuid Uuid
        {
            get
            {
                CUuuid uuid = new CUuuid();
                CUResult res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetUuid_v2(ref uuid, this);
                Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuDeviceGetUuid_v2", res));
                if (res != CUResult.Success) throw new CudaException(res);
                return uuid;
            }
        }

        /// <summary>
        /// Returns information about the execution affinity support of the device.<para/>
        /// Returns in \p *pi whether execution affinity type \p type is supported by device \p dev.<para/>
        /// The supported types are:<para/>
        /// - ::CU_EXEC_AFFINITY_TYPE_SM_COUNT: 1 if context with limited SMs is supported by the device,
        /// or 0 if not;
        /// </summary>
        public bool GetExecAffinitySupport(CUexecAffinityType type)
        {
            int pi = 0;
            CUResult res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetExecAffinitySupport(ref pi, type, this);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuDeviceGetExecAffinitySupport", res));
            if (res != CUResult.Success) throw new CudaException(res);
            return pi > 0;
        }

        /// <summary>
        /// Free unused memory that was cached on the specified device for use with graphs back to the OS.<para/>
        /// Blocks which are not in use by a graph that is either currently executing or scheduled to execute are freed back to the operating system.
        /// </summary>
        public void GraphMemTrim()
        {
            CUResult res = DriverAPINativeMethods.GraphManagment.cuDeviceGraphMemTrim(this);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuDeviceGraphMemTrim", res));
            if (res != CUResult.Success) throw new CudaException(res);
        }

        /// <summary>
        /// Set asynchronous allocation attributes related to graphs<para/>
        /// Valid attributes are:<para/>
        /// - ::CU_GRAPH_MEM_ATTR_USED_MEM_HIGH: High watermark of memory, in bytes, associated with graphs since the last time it was reset.High watermark can only be reset to zero.<para/>
        /// - ::CU_GRAPH_MEM_ATTR_RESERVED_MEM_HIGH: High watermark of memory, in bytes, currently allocated for use by the CUDA graphs asynchronous allocator.
        /// </summary>
        public void SetGraphMemAttribute(CUgraphMem_attribute attr, ulong value)
        {

            CUResult res = DriverAPINativeMethods.GraphManagment.cuDeviceSetGraphMemAttribute(this, attr, ref value);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuDeviceSetGraphMemAttribute", res));
            if (res != CUResult.Success) throw new CudaException(res);
        }

        /// <summary>
        /// Query asynchronous allocation attributes related to graphs<para/>
        /// Valid attributes are:<para/>
        /// - ::CU_GRAPH_MEM_ATTR_USED_MEM_CURRENT: Amount of memory, in bytes, currently associated with graphs<para/>
        /// - ::CU_GRAPH_MEM_ATTR_USED_MEM_HIGH: High watermark of memory, in bytes, associated with graphs since the last time it was reset.High watermark can only be reset to zero.<para/>
        /// - ::CU_GRAPH_MEM_ATTR_RESERVED_MEM_CURRENT: Amount of memory, in bytes, currently allocated for use by the CUDA graphs asynchronous allocator.<para/>
        /// - ::CU_GRAPH_MEM_ATTR_RESERVED_MEM_HIGH: High watermark of memory, in bytes, currently allocated for use by the CUDA graphs asynchronous allocator.
        /// </summary>
        public ulong GetGraphMemAttribute(CUgraphMem_attribute attr)
        {
            ulong value = 0;
            CUResult res = DriverAPINativeMethods.GraphManagment.cuDeviceGetGraphMemAttribute(this, attr, ref value);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuDeviceGetGraphMemAttribute", res));
            if (res != CUResult.Success) throw new CudaException(res);
            return value;
        }


        #region operators
        /// <summary>
        /// 
        /// </summary>
        /// <param name="src"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool operator ==(CUdevice src, CUdevice value)
        {
            return src.Pointer == value.Pointer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="src"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool operator !=(CUdevice src, CUdevice value)
        {
            return src.Pointer != value.Pointer;
        }
        #endregion

        #region Override Methods
        /// <summary>
        /// Returns true if both objects are of type CUdevice and if both Pointer member are equal.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (!(obj is CUdevice)) return false;

            CUdevice value = (CUdevice)obj;

            return this.Pointer.Equals(value.Pointer);
        }

        /// <summary>
        /// Overrides object.GetHashCode()
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Pointer.GetHashCode();
        }

        /// <summary>
        /// override ToString()
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Pointer.ToString();
        }
        #endregion
    }

    /// <summary>
    /// Idea of a SizeT type from http://blogs.hoopoe-cloud.com/index.php/tag/cudanet/, entry from Tuesday, September 15th, 2009
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SizeT
    {
        private UIntPtr value;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public SizeT(int value)
        {
            this.value = new UIntPtr((uint)value);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public SizeT(uint value)
        {
            this.value = new UIntPtr(value);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public SizeT(long value)
        {
            this.value = new UIntPtr((ulong)value);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public SizeT(ulong value)
        {
            this.value = new UIntPtr(value);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public SizeT(UIntPtr value)
        {
            this.value = value;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public SizeT(IntPtr value)
        {
            this.value = new UIntPtr((ulong)value.ToInt64());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static implicit operator int(SizeT t)
        {
            return (int)t.value.ToUInt32();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static implicit operator uint(SizeT t)
        {
            return (t.value.ToUInt32());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static implicit operator long(SizeT t)
        {
            return (long)t.value.ToUInt64();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static implicit operator ulong(SizeT t)
        {
            return (t.value.ToUInt64());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static implicit operator UIntPtr(SizeT t)
        {
            return t.value;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static implicit operator IntPtr(SizeT t)
        {
            return new IntPtr((long)t.value.ToUInt64());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator SizeT(int value)
        {
            return new SizeT(value);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator SizeT(uint value)
        {
            return new SizeT(value);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator SizeT(long value)
        {
            return new SizeT(value);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator SizeT(ulong value)
        {
            return new SizeT(value);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator SizeT(IntPtr value)
        {
            return new SizeT(value);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator SizeT(UIntPtr value)
        {
            return new SizeT(value);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns></returns>
        public static bool operator !=(SizeT val1, SizeT val2)
        {
            return (val1.value != val2.value);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns></returns>
        public static bool operator ==(SizeT val1, SizeT val2)
        {
            return (val1.value == val2.value);
        }
        #region +
        /// <summary>
        /// Define operator + on converted to ulong values to avoid fall back to int
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns></returns>
        public static SizeT operator +(SizeT val1, SizeT val2)
        {
            return new SizeT(val1.value.ToUInt64() + val2.value.ToUInt64());
        }
        /// <summary>
        /// Define operator + on converted to ulong values to avoid fall back to int
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns></returns>
        public static SizeT operator +(SizeT val1, int val2)
        {
            return new SizeT(val1.value.ToUInt64() + (ulong)val2);
        }
        /// <summary>
        /// Define operator + on converted to ulong values to avoid fall back to int
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns></returns>
        public static SizeT operator +(int val1, SizeT val2)
        {
            return new SizeT((ulong)val1 + val2.value.ToUInt64());
        }
        /// <summary>
        /// Define operator + on converted to ulong values to avoid fall back to int
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns></returns>
        public static SizeT operator +(uint val1, SizeT val2)
        {
            return new SizeT((ulong)val1 + val2.value.ToUInt64());
        }
        /// <summary>
        /// Define operator + on converted to ulong values to avoid fall back to int
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns></returns>
        public static SizeT operator +(SizeT val1, uint val2)
        {
            return new SizeT(val1.value.ToUInt64() + (ulong)val2);
        }
        #endregion
        #region -
        /// <summary>
        /// Define operator - on converted to ulong values to avoid fall back to int
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns></returns>
        public static SizeT operator -(SizeT val1, SizeT val2)
        {
            return new SizeT(val1.value.ToUInt64() - val2.value.ToUInt64());
        }
        /// <summary>
        /// Define operator - on converted to ulong values to avoid fall back to int
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns></returns>
        public static SizeT operator -(SizeT val1, int val2)
        {
            return new SizeT(val1.value.ToUInt64() - (ulong)val2);
        }
        /// <summary>
        /// Define operator - on converted to ulong values to avoid fall back to int
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns></returns>
        public static SizeT operator -(int val1, SizeT val2)
        {
            return new SizeT((ulong)val1 - val2.value.ToUInt64());
        }
        /// <summary>
        /// Define operator - on converted to ulong values to avoid fall back to int
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns></returns>
        public static SizeT operator -(SizeT val1, uint val2)
        {
            return new SizeT(val1.value.ToUInt64() - (ulong)val2);
        }
        /// <summary>
        /// Define operator - on converted to ulong values to avoid fall back to int
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns></returns>
        public static SizeT operator -(uint val1, SizeT val2)
        {
            return new SizeT((ulong)val1 - val2.value.ToUInt64());
        }
        #endregion
        #region *
        /// <summary>
        /// Define operator * on converted to ulong values to avoid fall back to int
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns></returns>
        public static SizeT operator *(SizeT val1, SizeT val2)
        {
            return new SizeT(val1.value.ToUInt64() * val2.value.ToUInt64());
        }
        /// <summary>
        /// Define operator * on converted to ulong values to avoid fall back to int
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns></returns>
        public static SizeT operator *(SizeT val1, int val2)
        {
            return new SizeT(val1.value.ToUInt64() * (ulong)val2);
        }
        /// <summary>
        /// Define operator * on converted to ulong values to avoid fall back to int
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns></returns>
        public static SizeT operator *(int val1, SizeT val2)
        {
            return new SizeT((ulong)val1 * val2.value.ToUInt64());
        }
        /// <summary>
        /// Define operator * on converted to ulong values to avoid fall back to int
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns></returns>
        public static SizeT operator *(SizeT val1, uint val2)
        {
            return new SizeT(val1.value.ToUInt64() * (ulong)val2);
        }
        /// <summary>
        /// Define operator * on converted to ulong values to avoid fall back to int
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns></returns>
        public static SizeT operator *(uint val1, SizeT val2)
        {
            return new SizeT((ulong)val1 * val2.value.ToUInt64());
        }
        #endregion
        #region /
        /// <summary>
        /// Define operator / on converted to ulong values to avoid fall back to int
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns></returns>
        public static SizeT operator /(SizeT val1, SizeT val2)
        {
            return new SizeT(val1.value.ToUInt64() / val2.value.ToUInt64());
        }
        /// <summary>
        /// Define operator / on converted to ulong values to avoid fall back to int
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns></returns>
        public static SizeT operator /(SizeT val1, int val2)
        {
            return new SizeT(val1.value.ToUInt64() / (ulong)val2);
        }
        /// <summary>
        /// Define operator / on converted to ulong values to avoid fall back to int
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns></returns>
        public static SizeT operator /(int val1, SizeT val2)
        {
            return new SizeT((ulong)val1 / val2.value.ToUInt64());
        }
        /// <summary>
        /// Define operator / on converted to ulong values to avoid fall back to int
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns></returns>
        public static SizeT operator /(SizeT val1, uint val2)
        {
            return new SizeT(val1.value.ToUInt64() / (ulong)val2);
        }
        /// <summary>
        /// Define operator / on converted to ulong values to avoid fall back to int
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns></returns>
        public static SizeT operator /(uint val1, SizeT val2)
        {
            return new SizeT((ulong)val1 / val2.value.ToUInt64());
        }
        #endregion
        #region >
        /// <summary>
        /// Define operator &gt; on converted to ulong values to avoid fall back to int
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns></returns>
        public static bool operator >(SizeT val1, SizeT val2)
        {
            return val1.value.ToUInt64() > val2.value.ToUInt64();
        }
        /// <summary>
        /// Define operator &gt; on converted to ulong values to avoid fall back to int
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns></returns>
        public static bool operator >(SizeT val1, int val2)
        {
            return val1.value.ToUInt64() > (ulong)val2;
        }
        /// <summary>
        /// Define operator &gt; on converted to ulong values to avoid fall back to int
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns></returns>
        public static bool operator >(int val1, SizeT val2)
        {
            return (ulong)val1 > val2.value.ToUInt64();
        }
        /// <summary>
        /// Define operator &gt; on converted to ulong values to avoid fall back to int
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns></returns>
        public static bool operator >(SizeT val1, uint val2)
        {
            return val1.value.ToUInt64() > (ulong)val2;
        }
        /// <summary>
        /// Define operator &gt; on converted to ulong values to avoid fall back to int
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns></returns>
        public static bool operator >(uint val1, SizeT val2)
        {
            return (ulong)val1 > val2.value.ToUInt64();
        }
        #endregion
        #region <
        /// <summary>
        /// Define operator &lt; on converted to ulong values to avoid fall back to int
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns></returns>
        public static bool operator <(SizeT val1, SizeT val2)
        {
            return val1.value.ToUInt64() < val2.value.ToUInt64();
        }
        /// <summary>
        /// Define operator &lt; on converted to ulong values to avoid fall back to int
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns></returns>
        public static bool operator <(SizeT val1, int val2)
        {
            return val1.value.ToUInt64() < (ulong)val2;
        }
        /// <summary>
        /// Define operator &lt; on converted to ulong values to avoid fall back to int
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns></returns>
        public static bool operator <(int val1, SizeT val2)
        {
            return (ulong)val1 < val2.value.ToUInt64();
        }
        /// <summary>
        /// Define operator &lt; on converted to ulong values to avoid fall back to int
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns></returns>
        public static bool operator <(SizeT val1, uint val2)
        {
            return val1.value.ToUInt64() < (ulong)val2;
        }
        /// <summary>
        /// Define operator &lt; on converted to ulong values to avoid fall back to int
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns></returns>
        public static bool operator <(uint val1, SizeT val2)
        {
            return (ulong)val1 < val2.value.ToUInt64();
        }
        #endregion
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (!(obj is SizeT)) return false;
            SizeT o = (SizeT)obj;
            return this.value.Equals(o.value);
        }
        /// <summary>
        /// returns this.value.ToString()
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (IntPtr.Size == 4)
                return ((uint)this.value.ToUInt32()).ToString();
            else
                return ((ulong)this.value.ToUInt64()).ToString();
        }
        /// <summary>
        /// Returns this.value.GetHashCode()
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.value.GetHashCode();
        }
    }

    /// <summary>
    /// CUBLAS status type returns
    /// </summary>
    public enum CublasStatus
    {
        /// <summary>
        /// 
        /// </summary>
        Success = 0,
        /// <summary>
        /// 
        /// </summary>
        NotInitialized = 1,
        /// <summary>
        /// 
        /// </summary>
        AllocFailed = 3,
        /// <summary>
        /// 
        /// </summary>
        InvalidValue = 7,
        /// <summary>
        /// 
        /// </summary>
        ArchMismatch = 8,
        /// <summary>
        /// 
        /// </summary>
        MappingError = 11,
        /// <summary>
        /// 
        /// </summary>
        ExecutionFailed = 13,
        /// <summary>
        /// 
        /// </summary>
        InternalError = 14,
        /// <summary>
        /// 
        /// </summary>
        NotSupported = 15,
        /// <summary>
        /// 
        /// </summary>
        LicenseError = 16
    }

    /// <summary>
    /// Flags for specifying particular handle types
    /// </summary>
    [Flags]
    public enum CUmemAllocationHandleType
    {
        /// <summary>
        /// Does not allow any export mechanism.
        /// </summary>
        None = 0,
        /// <summary>
        /// Allows a file descriptor to be used for exporting. Permitted only on POSIX systems. (int)
        /// </summary>
        PosixFileDescriptor = 0x1,
        /// <summary>
        /// Allows a Win32 NT handle to be used for exporting. (HANDLE)
        /// </summary>
        Win32 = 0x2,
        /// <summary>
        /// Allows a Win32 KMT handle to be used for exporting. (D3DKMT_HANDLE)
        /// </summary>
        Win32KMT = 0x4
    }

    /// <summary>
    /// Cuda context
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CUcontext
    {
        /// <summary>
        /// 
        /// </summary>
        public IntPtr Pointer;
    }

    /// <summary>
    /// Memory types
    /// </summary>
    public enum CUMemoryType : uint
    {
        /// <summary>
        /// Host memory
        /// </summary>
        Host = 0x01,

        /// <summary>
        /// Device memory
        /// </summary>
        Device = 0x02,

        /// <summary>
        /// Array memory
        /// </summary>
        Array = 0x03,

        /// <summary>
        /// Unified device or host memory
        /// </summary>
        Unified = 4
    }

    /// <summary>
    /// GPU Direct v3 tokens
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CudaPointerAttributeP2PTokens
    {
        /// <summary>
        /// 
        /// </summary>
        ulong p2pToken;
        /// <summary>
        /// 
        /// </summary>
        uint vaSpaceToken;
    }

    /// <summary>
    /// Pointer to CUDA device memory
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CUdeviceptr
    {
        /// <summary>
        /// 
        /// </summary>
        public SizeT Pointer;

        #region operators
        /// <summary>
        /// 
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static implicit operator ulong(CUdeviceptr src)
        {
            return src.Pointer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static explicit operator CUdeviceptr(SizeT src)
        {
            CUdeviceptr udeviceptr = new CUdeviceptr();
            udeviceptr.Pointer = src;
            return udeviceptr;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="src"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static CUdeviceptr operator +(CUdeviceptr src, SizeT value)
        {
            CUdeviceptr udeviceptr = new CUdeviceptr();
            udeviceptr.Pointer = src.Pointer + value;
            return udeviceptr;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="src"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static CUdeviceptr operator -(CUdeviceptr src, SizeT value)
        {
            CUdeviceptr udeviceptr = new CUdeviceptr();
            udeviceptr.Pointer = src.Pointer - value;
            return udeviceptr;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="src"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool operator ==(CUdeviceptr src, CUdeviceptr value)
        {
            return src.Pointer == value.Pointer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="src"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool operator !=(CUdeviceptr src, CUdeviceptr value)
        {
            return src.Pointer != value.Pointer;
        }
        #endregion

        #region Override Methods
        /// <summary>
        /// Returns true if both objects are of type CUdeviceptr and if both Pointer member is equal.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (!(obj is CUdeviceptr)) return false;

            CUdeviceptr value = (CUdeviceptr)obj;

            return this.Pointer.Equals(value.Pointer);
        }

        /// <summary>
        /// Overrides object.GetHashCode()
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// override ToString()
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Pointer.ToString();
        }

        #endregion

        #region constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pointer"></param>
        public CUdeviceptr(SizeT pointer)
        {
            Pointer = pointer;
        }
        #endregion

        #region GetAttributeMethods
        /// <summary>
        /// The <see cref="CUcontext"/> on which a pointer was allocated or registered
        /// </summary>
        public CUcontext AttributeContext
        {
            get
            {
                CUcontext ret = new CUcontext();
                CUResult res = DriverAPINativeMethods.MemoryManagement.cuPointerGetAttribute(ref ret, CUPointerAttribute.Context, this);
                Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuPointerGetAttribute", res));
                if (res != CUResult.Success) throw new CudaException(res);
                return ret;
            }
        }

        /// <summary>
        /// The <see cref="CUMemoryType"/> describing the physical location of a pointer 
        /// </summary>
        public CUMemoryType AttributeMemoryType
        {
            get
            {
                CUMemoryType ret = new CUMemoryType();
                CUResult res = DriverAPINativeMethods.MemoryManagement.cuPointerGetAttribute(ref ret, CUPointerAttribute.MemoryType, this);
                Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuPointerGetAttribute", res));
                if (res != CUResult.Success) throw new CudaException(res);
                return ret;
            }
        }

        /// <summary>
        /// The address at which a pointer's memory may be accessed on the device <para/>
        /// Except in the exceptional disjoint addressing cases, the value returned will equal the input value.
        /// </summary>
        public CUdeviceptr AttributeDevicePointer
        {
            get
            {
                CUdeviceptr ret = new CUdeviceptr();
                CUResult res = DriverAPINativeMethods.MemoryManagement.cuPointerGetAttribute(ref ret, CUPointerAttribute.DevicePointer, this);
                Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuPointerGetAttribute", res));
                if (res != CUResult.Success) throw new CudaException(res);
                return ret;
            }
        }

        /// <summary>
        /// The address at which a pointer's memory may be accessed on the host 
        /// </summary>
        public IntPtr AttributeHostPointer
        {
            get
            {
                IntPtr ret = new IntPtr();
                CUResult res = DriverAPINativeMethods.MemoryManagement.cuPointerGetAttribute(ref ret, CUPointerAttribute.HostPointer, this);
                Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuPointerGetAttribute", res));
                if (res != CUResult.Success) throw new CudaException(res);
                return ret;
            }
        }

        /// <summary>
        /// A pair of tokens for use with the nv-p2p.h Linux kernel interface
        /// </summary>
        public CudaPointerAttributeP2PTokens AttributeP2PTokens
        {
            get
            {
                CudaPointerAttributeP2PTokens ret = new CudaPointerAttributeP2PTokens();
                CUResult res = DriverAPINativeMethods.MemoryManagement.cuPointerGetAttribute(ref ret, CUPointerAttribute.P2PTokens, this);
                Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuPointerGetAttribute", res));
                if (res != CUResult.Success) throw new CudaException(res);
                return ret;
            }
        }

        /// <summary>
        /// Synchronize every synchronous memory operation initiated on this region
        /// </summary>
        public bool AttributeSyncMemops
        {
            get
            {
                int ret = 0;
                CUResult res = DriverAPINativeMethods.MemoryManagement.cuPointerGetAttribute(ref ret, CUPointerAttribute.SyncMemops, this);
                Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuPointerGetAttribute", res));
                if (res != CUResult.Success) throw new CudaException(res);
                return ret != 0;
            }
            set
            {
                int val = value ? 1 : 0;
                CUResult res = DriverAPINativeMethods.MemoryManagement.cuPointerSetAttribute(ref val, CUPointerAttribute.SyncMemops, this);
                Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuPointerSetAttribute", res));
                if (res != CUResult.Success) throw new CudaException(res);
            }
        }

        /// <summary>
        /// A process-wide unique ID for an allocated memory region
        /// </summary>
        public ulong AttributeBufferID
        {
            get
            {
                ulong ret = 0;
                CUResult res = DriverAPINativeMethods.MemoryManagement.cuPointerGetAttribute(ref ret, CUPointerAttribute.BufferID, this);
                Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuPointerGetAttribute", res));
                if (res != CUResult.Success) throw new CudaException(res);
                return ret;
            }
        }

        /// <summary>
        /// Indicates if the pointer points to managed memory
        /// </summary>
        public bool AttributeIsManaged
        {
            get
            {
                int ret = 0;
                CUResult res = DriverAPINativeMethods.MemoryManagement.cuPointerGetAttribute(ref ret, CUPointerAttribute.IsManaged, this);
                Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuPointerGetAttribute", res));
                if (res != CUResult.Success) throw new CudaException(res);
                return ret != 0;
            }
        }

        /// <summary>
        /// A device ordinal of a device on which a pointer was allocated or registered
        /// </summary>
        public int AttributeDeviceOrdinal
        {
            get
            {
                int ret = 0;
                CUResult res = DriverAPINativeMethods.MemoryManagement.cuPointerGetAttribute(ref ret, CUPointerAttribute.DeviceOrdinal, this);
                Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuPointerGetAttribute", res));
                if (res != CUResult.Success) throw new CudaException(res);
                return ret;
            }
        }

        /// <summary>
        /// 1 if this pointer maps to an allocation that is suitable for ::cudaIpcGetMemHandle, 0 otherwise
        /// </summary>
        public bool AttributeIsLegacyCudaIPCCapable
        {
            get
            {
                int ret = 0;
                CUResult res = DriverAPINativeMethods.MemoryManagement.cuPointerGetAttribute(ref ret, CUPointerAttribute.IsLegacyCudaIPCCapable, this);
                Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuPointerGetAttribute", res));
                if (res != CUResult.Success) throw new CudaException(res);
                return ret != 0;
            }
        }

        /// <summary>
        /// Starting address for this requested pointer
        /// </summary>
        public CUdeviceptr AttributeRangeStartAddr
        {
            get
            {
                CUdeviceptr ret = new CUdeviceptr();
                CUResult res = DriverAPINativeMethods.MemoryManagement.cuPointerGetAttribute(ref ret, CUPointerAttribute.RangeStartAddr, this);
                Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuPointerGetAttribute", res));
                if (res != CUResult.Success) throw new CudaException(res);
                return ret;
            }
        }

        /// <summary>
        /// Size of the address range for this requested pointer
        /// </summary>
        public SizeT AttributeRangeSize
        {
            get
            {
                ulong ret = 0;
                CUResult res = DriverAPINativeMethods.MemoryManagement.cuPointerGetAttribute(ref ret, CUPointerAttribute.RangeSize, this);
                Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuPointerGetAttribute", res));
                if (res != CUResult.Success) throw new CudaException(res);
                return ret;
            }
        }

        /// <summary>
        /// 1 if this pointer is in a valid address range that is mapped to a backing allocation, 0 otherwise
        /// </summary>
        public bool AttributeMapped
        {
            get
            {
                int ret = 0;
                CUResult res = DriverAPINativeMethods.MemoryManagement.cuPointerGetAttribute(ref ret, CUPointerAttribute.Mapped, this);
                Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuPointerGetAttribute", res));
                if (res != CUResult.Success) throw new CudaException(res);
                return ret != 0;
            }
        }

        /// <summary>
        /// Bitmask of allowed ::CUmemAllocationHandleType for this allocation
        /// </summary>
        public CUmemAllocationHandleType AttributeAllowedHandleTypes
        {
            get
            {
                int ret = 0;
                CUResult res = DriverAPINativeMethods.MemoryManagement.cuPointerGetAttribute(ref ret, CUPointerAttribute.AllowedHandleTypes, this);
                Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuPointerGetAttribute", res));
                if (res != CUResult.Success) throw new CudaException(res);
                return (CUmemAllocationHandleType)ret;
            }
        }

        /// <summary>
        /// 1 if the memory this pointer is referencing can be used with the GPUDirect RDMA API
        /// </summary>
        public bool AttributeIsGPUDirectRDMACapable
        {
            get
            {
                int ret = 0;
                CUResult res = DriverAPINativeMethods.MemoryManagement.cuPointerGetAttribute(ref ret, CUPointerAttribute.IsGPUDirectRDMACapable, this);
                Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuPointerGetAttribute", res));
                if (res != CUResult.Success) throw new CudaException(res);
                return ret != 0;
            }
        }

        /// <summary>
        /// Returns the access flags the device associated with the current context has on the corresponding memory referenced by the pointer given
        /// </summary>
        public bool AttributeAccessFlags
        {
            get
            {
                int ret = 0;
                CUResult res = DriverAPINativeMethods.MemoryManagement.cuPointerGetAttribute(ref ret, CUPointerAttribute.AccessFlags, this);
                Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuPointerGetAttribute", res));
                if (res != CUResult.Success) throw new CudaException(res);
                return ret != 0;
            }
        }

        /// <summary>
        /// Returns the mempool handle for the allocation if it was allocated from a mempool. Otherwise returns NULL.
        /// </summary>
        public CUmemoryPool AttributeMempoolHandle
        {
            get
            {
                IntPtr temp = new IntPtr();
                CUmemoryPool ret = new CUmemoryPool();
                CUResult res = DriverAPINativeMethods.MemoryManagement.cuPointerGetAttribute(ref temp, CUPointerAttribute.MempoolHandle, this);
                Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuPointerGetAttribute", res));
                if (res != CUResult.Success) throw new CudaException(res);
                ret.Pointer = temp;
                return ret;
            }
        }

        /// <summary>
        /// Size of the actual underlying mapping that the pointer belongs to
        /// </summary>
        public SizeT AttributeMappingSize
        {
            get
            {
                ulong ret = 0;
                CUResult res = DriverAPINativeMethods.MemoryManagement.cuPointerGetAttribute(ref ret, CUPointerAttribute.MappingSize, this);
                Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuPointerGetAttribute", res));
                if (res != CUResult.Success) throw new CudaException(res);
                return ret;
            }
        }

        /// <summary>
        /// The start address of the mapping that the pointer belongs to
        /// </summary>
        public IntPtr AttributeBaseAddr
        {
            get
            {
                IntPtr ret = new IntPtr();
                CUResult res = DriverAPINativeMethods.MemoryManagement.cuPointerGetAttribute(ref ret, CUPointerAttribute.BaseAddr, this);
                Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuPointerGetAttribute", res));
                if (res != CUResult.Success) throw new CudaException(res);
                return ret;
            }
        }

        /// <summary>
        /// A process-wide unique id corresponding to the physical allocation the pointer belongs to
        /// </summary>
        public ulong AttributeMemoryBlockID
        {
            get
            {
                ulong ret = 0;
                CUResult res = DriverAPINativeMethods.MemoryManagement.cuPointerGetAttribute(ref ret, CUPointerAttribute.MemoryBlockID, this);
                Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuPointerGetAttribute", res));
                if (res != CUResult.Success) throw new CudaException(res);
                return ret;
            }
        }

        #endregion
    }

    /// <summary>
    /// CUDA memory pool
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CUmemoryPool
    {
        /// <summary>
        /// 
        /// </summary>
        public IntPtr Pointer;
    }

    /// <summary>
    /// Pointer information
    /// </summary>
    public enum CUPointerAttribute
    {
        /// <summary>
        /// The <see cref="CUcontext"/> on which a pointer was allocated or registered
        /// </summary>
        Context = 1,

        /// <summary>
        /// The <see cref="CUMemoryType"/> describing the physical location of a pointer 
        /// </summary>
        MemoryType = 2,

        /// <summary>
        /// The address at which a pointer's memory may be accessed on the device 
        /// </summary>
        DevicePointer = 3,

        /// <summary>
        /// The address at which a pointer's memory may be accessed on the host 
        /// </summary>
        HostPointer = 4,

        /// <summary>
        /// A pair of tokens for use with the nv-p2p.h Linux kernel interface
        /// </summary>
        P2PTokens = 5,

        /// <summary>
        /// Synchronize every synchronous memory operation initiated on this region
        /// </summary>
        SyncMemops = 6,

        /// <summary>
        /// A process-wide unique ID for an allocated memory region
        /// </summary>
        BufferID = 7,

        /// <summary>
        /// Indicates if the pointer points to managed memory
        /// </summary>
        IsManaged = 8,

        /// <summary>
        /// A device ordinal of a device on which a pointer was allocated or registered
        /// </summary>
        DeviceOrdinal = 9,

        /// <summary>
        /// 1 if this pointer maps to an allocation that is suitable for ::cudaIpcGetMemHandle, 0 otherwise
        /// </summary>
        IsLegacyCudaIPCCapable = 10,

        /// <summary>
        /// Starting address for this requested pointer
        /// </summary>
        RangeStartAddr = 11,

        /// <summary>
        /// Size of the address range for this requested pointer
        /// </summary>
        RangeSize = 12,

        /// <summary>
        /// 1 if this pointer is in a valid address range that is mapped to a backing allocation, 0 otherwise
        /// </summary>
        Mapped = 13,

        /// <summary>
        /// Bitmask of allowed ::CUmemAllocationHandleType for this allocation
        /// </summary>
        AllowedHandleTypes = 14,

        /// <summary>
        /// 1 if the memory this pointer is referencing can be used with the GPUDirect RDMA API
        /// </summary>
        IsGPUDirectRDMACapable = 15,

        /// <summary>
        /// Returns the access flags the device associated with the current context has on the corresponding memory referenced by the pointer given
        /// </summary>
        AccessFlags = 16,

        /// <summary>
        /// Returns the mempool handle for the allocation if it was allocated from a mempool. Otherwise returns NULL.
        /// </summary>
        MempoolHandle = 17,
        /// <summary>
        /// Size of the actual underlying mapping that the pointer belongs to
        /// </summary>
        MappingSize = 18,
        /// <summary>
        /// The start address of the mapping that the pointer belongs to
        /// </summary>
        BaseAddr = 19,
        /// <summary>
        /// A process-wide unique id corresponding to the physical allocation the pointer belongs to
        /// </summary>
        MemoryBlockID = 20

    }

    /// <summary>
    /// CUDA stream flags
    /// </summary>
    [Flags]
    public enum CUStreamFlags : uint
    {
        /// <summary>
        /// For compatibilty with pre Cuda 5.0, equal to Default
        /// </summary>
        None = 0,
        /// <summary>
        /// Default stream flag
        /// </summary>
        Default = 0x0,
        /// <summary>
        /// Stream does not synchronize with stream 0 (the NULL stream)
        /// </summary>
        NonBlocking = 0x1,
    }

    /// <summary>
    /// Cuda event
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CUevent
    {
        /// <summary>
        /// 
        /// </summary>
        public IntPtr Pointer;
    }

    /// <summary>
    /// CUDA stream callback
    /// </summary>
    /// <param name="hStream">The stream the callback was added to, as passed to ::cuStreamAddCallback.  May be NULL.</param>
    /// <param name="status">CUDA_SUCCESS or any persistent error on the stream.</param>
    /// <param name="userData">User parameter provided at registration.</param>
    public delegate void CUstreamCallback(CUstream hStream, CUResult status, IntPtr userData);

    /// <summary>
    /// Flag for cuStreamAddCallback()
    /// </summary>
    [Flags]
    public enum CUStreamAddCallbackFlags
    {
        /// <summary>
        /// No flags
        /// </summary>
        None = 0x0,
        ///// <summary>
        ///// The stream callback blocks the stream until it is done executing.
        ///// </summary>
        //Blocking = 0x01,
    }

    /// <summary>
    /// Flags for ::cuStreamWaitValue32
    /// </summary>
    [Flags]
    public enum CUstreamWaitValue_flags
    {
        /// <summary>
        /// Wait until (int32_t)(*addr - value) >= 0 (or int64_t for 64 bit values). Note this is a cyclic comparison which ignores wraparound. (Default behavior.) 
        /// </summary>
        GEQ = 0x0,
        /// <summary>
        /// Wait until *addr == value.
        /// </summary>
        EQ = 0x1,
        /// <summary>
        /// Wait until (*addr &amp; value) != 0.
        /// </summary>
        And = 0x2,
        /// <summary>
        /// Wait until ~(*addr | value) != 0. Support for this operation can be
        /// queried with ::cuDeviceGetAttribute() and ::CU_DEVICE_ATTRIBUTE_CAN_USE_STREAM_WAIT_VALUE_NOR. 
        /// Generally, this requires compute capability 7.0 or greater. 
        /// </summary>
        NOr = 0x3,
        /// <summary>
        /// Follow the wait operation with a flush of outstanding remote writes. This
        /// means that, if a remote write operation is guaranteed to have reached the
        /// device before the wait can be satisfied, that write is guaranteed to be
        /// visible to downstream device work. The device is permitted to reorder
        /// remote writes internally. For example, this flag would be required if
        /// two remote writes arrive in a defined order, the wait is satisfied by the
        /// second write, and downstream work needs to observe the first write.
        /// </summary>
        Flush = 1 << 30
    }

    /// <summary>
    /// Flags for ::cuStreamWriteValue32
    /// </summary>
    [Flags]
    public enum CUstreamWriteValue_flags
    {
        /// <summary>
        /// Default behavior
        /// </summary>
        Default = 0x0,
        /// <summary>
        /// Permits the write to be reordered with writes which were issued
        /// before it, as a performance optimization. Normally, ::cuStreamWriteValue32 will provide a memory fence before the
        /// write, which has similar semantics to __threadfence_system() but is scoped to the stream rather than a CUDA thread.
        /// </summary>
        NoMemoryBarrier = 0x1
    }

    /// <summary>
    /// 
    /// </summary>
    public enum CUstreamAttrID
    {
        /// <summary>
        /// Identifier for ::CUstreamAttrValue::accessPolicyWindow.
        /// </summary>
        AccessPolicyWindow = 1,
        /// <summary>
        /// ::CUsynchronizationPolicy for work queued up in this stream
        /// </summary>
        SynchronizationPolicy = 3
    }

    /// <summary>
    /// Stream attributes union, used with ::cuStreamSetAttribute/::cuStreamGetAttribute
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct CUstreamAttrValue
    {
        /// <summary>
        /// Attribute ::CUaccessPolicyWindow.
        /// </summary>
        [FieldOffset(0)]
        public CUaccessPolicyWindow accessPolicyWindow;
        /// <summary>
        /// Value for ::CU_STREAM_ATTRIBUTE_SYNCHRONIZATION_POLICY.
        /// </summary>
        [FieldOffset(0)]
        public CUsynchronizationPolicy syncPolicy;
    }

    /// <summary>
    /// Specifies an access policy for a window, a contiguous extent of memory
    /// beginning at base_ptr and ending at base_ptr + num_bytes.<para/>
    /// num_bytes is limited by CU_DEVICE_ATTRIBUTE_MAX_ACCESS_POLICY_WINDOW_SIZE.<para/>
    /// Partition into many segments and assign segments such that:<para/>
    /// sum of "hit segments" / window == approx.ratio.<para/>
    /// sum of "miss segments" / window == approx 1-ratio.<para/>
    /// Segments and ratio specifications are fitted to the capabilities of
    /// the architecture.<para/>
    /// Accesses in a hit segment apply the hitProp access policy.<para/>
    /// Accesses in a miss segment apply the missProp access policy.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CUaccessPolicyWindow
    {
        /// <summary>
        /// Starting address of the access policy window. CUDA driver may align it.
        /// </summary>
        public IntPtr base_ptr;
        /// <summary>
        /// Size in bytes of the window policy. CUDA driver may restrict the maximum size and alignment.
        /// </summary>
        public SizeT num_bytes;
        /// <summary>
        /// hitRatio specifies percentage of lines assigned hitProp, rest are assigned missProp.
        /// </summary>
        public float hitRatio;
        /// <summary>
        /// ::CUaccessProperty set for hit.
        /// </summary>
        public CUaccessProperty hitProp;
        /// <summary>
        /// ::CUaccessProperty set for miss. Must be either NORMAL or STREAMING
        /// </summary>
        public CUaccessProperty missProp;
    }

    /// <summary>
    /// 
    /// </summary>
    public enum CUsynchronizationPolicy
    {
        /// <summary/>
        Auto = 1,
        /// <summary/>
        Spin = 2,
        /// <summary/>
        Yield = 3,
        /// <summary/>
        BlockingSync = 4
    }

    /// <summary>
    /// Specifies performance hint with ::CUaccessPolicyWindow for hitProp and missProp members
    /// </summary>
    public enum CUaccessProperty
    {
        /// <summary>
        /// Normal cache persistence.
        /// </summary>
        Normal = 0,
        /// <summary>
        /// Streaming access is less likely to persit from cache.
        /// </summary>
        Streaming = 1,
        /// <summary>
        /// Persisting access is more likely to persist in cache.
        /// </summary>
        Persisting = 2
    }

    /// <summary>
    /// CUDA Lazy Loading status
    /// </summary>
    public enum CUmoduleLoadingMode
    {
        /// <summary>
        /// Lazy Kernel Loading is not enabled
        /// </summary>
        EagerLoading = 0x1,
        /// <summary>
        /// Lazy Kernel Loading is enabled
        /// </summary>
        LazyLoading = 0x2,
    }

    /// <summary>
    /// Cuda module
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CUmodule
    {
        /// <summary>
        /// 
        /// </summary>
        public IntPtr Pointer;

        /// <summary>
        /// Query lazy loading mode<para/>
        /// Returns lazy loading mode. Module loading mode is controlled by CUDA_MODULE_LOADING env variable
        /// </summary>
        public static CUmoduleLoadingMode GetLoadingMode
        {
            get
            {
                CUmoduleLoadingMode ret = new CUmoduleLoadingMode();
                CUResult res = DriverAPINativeMethods.ModuleManagement.cuModuleGetLoadingMode(ref ret);
                Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuModuleGetLoadingMode", res));
                if (res != CUResult.Success) throw new CudaException(res);
                return ret;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum CUmemPool_attribute
    {
        /// <summary/>
        ReuseFollowEventDependencies = 1,
        /// <summary/>
        ReuseAllowOpportunistic,
        /// <summary/>
        ReuseAllowInternalDependencies,
        /// <summary/>
        ReleaseThreshold,
        /// <summary/>
        ReservedMemCurrent,
        /// <summary/>
        ReservedMemHigh,
        /// <summary/>
        UsedMemCurrent,
        /// <summary/>
        UsedMemHigh
    }

    /// <summary>
    /// Execution Affinity Types 
    /// </summary>
    public enum CUexecAffinityType
    {
        /// <summary>
        /// Create a context with limited SMs.
        /// </summary>
        SMCount = 0,
        /// <summary/>
        Max
    }

    /// <summary/>
    public enum CUgraphMem_attribute
    {
        /// <summary>
        /// (value type = cuuint64_t)<para/>
        /// Amount of memory, in bytes, currently associated with graphs
        /// </summary>
        UsedMemCurrent,

        /// <summary>
        /// (value type = cuuint64_t)<para/>
        /// High watermark of memory, in bytes, associated with graphs since the last time it was reset.  High watermark can only be reset to zero.
        /// </summary>
        UsedMemHigh,

        /// <summary>
        /// (value type = cuuint64_t)<para/>
        /// Amount of memory, in bytes, currently allocated for use by the CUDA graphs asynchronous allocator.
        /// </summary>
        ReservedMemCurrent,

        /// <summary>
        /// (value type = cuuint64_t)<para/>
        /// High watermark of memory, in bytes, currently allocated for use by the CUDA graphs asynchronous allocator.
        /// </summary>
        ReservedMemHigh
    }

    /// <summary>
    /// CUDA definition of UUID
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CUuuid
    {
        /// <summary>
        /// 
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16, ArraySubType = UnmanagedType.I1)]
        public byte[] bytes;
    }

    /// <summary>
    /// 
    /// </summary>
    public enum CUlaunchAttributeID
    {
        /// <summary>
        /// Ignored entry, for convenient composition
        /// </summary>
        Ignore = 0,
        /// <summary>
        /// Valid for streams, graph nodes, launches.
        /// </summary>
        AccessPolicyWindow = 1,
        /// <summary>
        /// Valid for graph nodes, launches.
        /// </summary>
        Cooperative = 2,
        /// <summary>
        /// Valid for streams.
        /// </summary>
        SynchronizationPolicy = 3,
        /// <summary>
        /// Valid for graph nodes, launches.
        /// </summary>
        ClusterDimension = 4,
        /// <summary>
        /// Valid for graph nodes, launches.
        /// </summary>
        ClusterSchedulingPolicyPreference = 5,
        /// <summary>
        /// Valid for launches. Setting programmaticStreamSerializationAllowed to non-0
        /// signals that the kernel will use programmatic means to resolve its stream dependency, so that
        /// the CUDA runtime should opportunistically allow the grid's execution to overlap with the previous
        /// kernel in the stream, if that kernel requests the overlap. The dependent launches can choose to wait
        /// on the dependency using the programmatic sync (cudaGridDependencySynchronize() or equivalent PTX instructions).
        /// </summary>
        ProgrammaticStreamSerialization = 6,
        /// <summary>
        /// Valid for launches. Event recorded through this launch attribute is guaranteed to only trigger
        /// after all block in the associated kernel trigger the event. A block can trigger the event through
        /// PTX launchdep.release or CUDA builtin function cudaTriggerProgrammaticLaunchCompletion(). A trigger 
        /// can also be inserted at the beginning of each block's execution if triggerAtBlockStart is set to non-0. 
        /// The dependent launches can choose to wait on the dependency using the programmatic sync 
        /// (cudaGridDependencySynchronize() or equivalent PTX instructions). Note that dependents (including the
        /// CPU thread calling cuEventSynchronize()) are not guaranteed to observe the release precisely when it is 
        /// released.  For example, cuEventSynchronize() may only observe the event trigger long after the associated 
        /// kernel has completed. This recording type is primarily meant for establishing programmatic dependency 
        /// between device tasks. The event supplied must not be an interprocess or interop event. The event must 
        /// disable timing (i.e. created with ::CU_EVENT_DISABLE_TIMING flag set).
        /// </summary>
        ProgrammaticEvent = 7,
        /// <summary>
        /// Valid for streams, graph nodes, launches.
        /// </summary>
        Priority = 8,
        /// <summary>
        /// 
        /// </summary>
        MemSyncDomainMap = 9,
        /// <summary>
        /// 
        /// </summary>
        MemSyncDomain = 10
    }

    /// <summary>
    /// 
    /// </summary>
    public struct CUlaunchAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        public CUlaunchAttributeID id;
        /// <summary>
        /// 
        /// </summary>
        public int pad;
        /// <summary>
        /// 
        /// </summary>
        public CUlaunchAttributeValue value;
    }

    /// <summary>
    /// 
    /// </summary>
    public struct CUlaunchConfig
    {
        /// <summary>
        /// Width of grid in blocks
        /// </summary>
        public uint gridDimX;
        /// <summary>
        /// Height of grid in blocks
        /// </summary>
        public uint gridDimY;
        /// <summary>
        /// Depth of grid in blocks
        /// </summary>
        public uint gridDimZ;
        /// <summary>
        /// X dimension of each thread block
        /// </summary>
        public uint blockDimX;
        /// <summary>
        /// Y dimension of each thread block
        /// </summary>
        public uint blockDimY;
        /// <summary>
        /// Z dimension of each thread block
        /// </summary>
        public uint blockDimZ;
        /// <summary>
        /// Dynamic shared-memory size per thread block in bytes
        /// </summary>
        public uint sharedMemBytes;
        /// <summary>
        /// Stream identifier
        /// </summary>
        public CUstream hStream;
        /// <summary>
        /// nullable if numAttrs == 0
        /// </summary>
        public CUlaunchAttribute[] attrs;
    }

    /// <summary>
    /// 
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct CUlaunchAttributeValue
    {
        /// <summary>
        /// Cluster dimensions for the kernel node.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct ClusterDim
        {
            /// <summary/>
            public uint x;
            /// <summary/>
            public uint y;
            /// <summary/>
            public uint z;
        }

        /// <summary>
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct ProgrammaticEvent
        {
            /// <summary/>
            public CUevent event_;
            /// <summary>
            /// Does not accept ::CU_EVENT_RECORD_EXTERNAL
            /// </summary>
            public int flags;
            /// <summary/>
            public int triggerAtBlockStart;
        }

        /// <summary>
        /// Attribute ::CUaccessPolicyWindow.
        /// </summary>
        [FieldOffset(0)]
        CUaccessPolicyWindow accessPolicyWindow;
        /// <summary>
        /// Nonzero indicates a cooperative kernel (see ::cuLaunchCooperativeKernel).
        /// </summary>
        [FieldOffset(0)]
        int cooperative;
        /// <summary>
        /// ::CUsynchronizationPolicy for work queued up in this stream
        /// </summary>
        [FieldOffset(0)]
        CUsynchronizationPolicy syncPolicy;

        /// <summary>
        /// Cluster dimensions for the kernel node.
        /// </summary>
        [FieldOffset(0)]
        ClusterDim clusterDim;
        /// <summary>
        /// Cluster scheduling policy preference for the kernel node.
        /// </summary>
        [FieldOffset(0)]
        CUclusterSchedulingPolicy clusterSchedulingPolicyPreference;
        /// <summary>
        /// 
        /// </summary>
        [FieldOffset(0)]
        int programmaticStreamSerializationAllowed;
        /// <summary>
        /// 
        /// </summary>
        [FieldOffset(0)]
        ProgrammaticEvent programmaticEvent;
        /// <summary>
        /// Execution priority of the kernel.
        /// </summary>
        [FieldOffset(0)]
        int priority;
        /// <summary>
        /// 
        /// </summary>
        [FieldOffset(0)]
        CUlaunchMemSyncDomainMap memSyncDomainMap;
        /// <summary>
        /// 
        /// </summary>
        [FieldOffset(0)]
        CUlaunchMemSyncDomain memSyncDomain;

        /// <summary>
        /// Pad to 64 bytes
        /// </summary>
        [FieldOffset(60)]
        public int pad;
    }

    /// <summary>
    /// 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CUlaunchMemSyncDomainMap
    {
        /// <summary>
        /// 
        /// </summary>
        public byte default_;
        /// <summary>
        /// 
        /// </summary>
        public byte remote;
    }

    /// <summary>
    /// 
    /// </summary>
    public enum CUlaunchMemSyncDomain
    {
        /// <summary>
        /// 
        /// </summary>
        Default = 0,
        /// <summary>
        /// 
        /// </summary>
        Remote = 1
    }

    /// <summary>
    ///  Cluster scheduling policies. These may be passed to ::cuFuncSetAttribute or ::cuKernelSetAttribute
    /// </summary>
    public enum CUclusterSchedulingPolicy
    {
        /// <summary>
        /// the default policy
        /// </summary>
        Default = 0,
        /// <summary>
        /// spread the blocks within a cluster to the SMs
        /// </summary>
        Spread = 1,
        /// <summary>
        /// allow the hardware to load-balance the blocks in a cluster to the SMs
        /// </summary>
        LoadBalancing = 2
    }

    /// <summary>
    /// CUlibrary
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CUlibrary
    {
        /// <summary>
        /// 
        /// </summary>
        public IntPtr Pointer;
    }

    /// <summary>
    /// CUkernel
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CUkernel
    {
        /// <summary>
        /// 
        /// </summary>
        public IntPtr Pointer;

        /// <summary>
        /// Allows explicit casting from CUkernel to CUfunction to call context-less kernels
        /// </summary>
        public static explicit operator CUfunction(CUkernel cukernel)
        {
            CUfunction ret = new CUfunction();
            ret.Pointer = cukernel.Pointer;
            return ret;
        }

        /// <summary>
        /// Get the corresponding CUfunction handle using cuKernelGetFunction
        /// </summary>
        public CUfunction GetCUfunction()
        {
            CUfunction ret = new CUfunction();
            CUResult res = DriverAPINativeMethods.LibraryManagement.cuKernelGetFunction(ref ret, this);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuKernelGetFunction", res));
            if (res != CUResult.Success) throw new CudaException(res);
            return ret;
        }


        /// <summary>
        /// <para>The number of threads beyond which a launch of the function would fail.</para>
        /// <para>This number depends on both the function and the device on which the
        /// function is currently loaded.</para>
        /// </summary>
        public int GetMaxThreadsPerBlock(CUdevice device)
        {
            int temp = 0;
            CUResult res = DriverAPINativeMethods.LibraryManagement.cuKernelGetAttribute(ref temp, CUFunctionAttribute.MaxThreadsPerBlock, this, device);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuFuncGetAttribute", res));
            if (res != CUResult.Success) throw new CudaException(res);
            return temp;
        }

        /// <summary>
        /// <para>The size in bytes of statically-allocated shared memory required by
        /// this function. </para><para>This does not include dynamically-allocated shared
        /// memory requested by the user at runtime.</para>
        /// </summary>
        public int GetSharedMemory(CUdevice device)
        {
            int temp = 0;
            CUResult res = DriverAPINativeMethods.LibraryManagement.cuKernelGetAttribute(ref temp, CUFunctionAttribute.SharedSizeBytes, this, device);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuFuncGetAttribute", res));
            if (res != CUResult.Success) throw new CudaException(res);
            return temp;
        }

        /// <summary>
        /// <para>The size in bytes of statically-allocated shared memory required by
        /// this function. </para><para>This does not include dynamically-allocated shared
        /// memory requested by the user at runtime.</para>
        /// </summary>
        public int GetConstMemory(CUdevice device)
        {
            int temp = 0;
            CUResult res = DriverAPINativeMethods.LibraryManagement.cuKernelGetAttribute(ref temp, CUFunctionAttribute.ConstSizeBytes, this, device);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuFuncGetAttribute", res));
            if (res != CUResult.Success) throw new CudaException(res);
            return temp;
        }

        /// <summary>
        /// The size in bytes of thread local memory used by this function.
        /// </summary>
        public int GetLocalMemory(CUdevice device)
        {
            int temp = 0;
            CUResult res = DriverAPINativeMethods.LibraryManagement.cuKernelGetAttribute(ref temp, CUFunctionAttribute.LocalSizeBytes, this, device);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuFuncGetAttribute", res));
            if (res != CUResult.Success) throw new CudaException(res);
            return temp;
        }

        /// <summary>
        /// The number of registers used by each thread of this function.
        /// </summary>
        public int GetRegisters(CUdevice device)
        {
            int temp = 0;
            CUResult res = DriverAPINativeMethods.LibraryManagement.cuKernelGetAttribute(ref temp, CUFunctionAttribute.NumRegs, this, device);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuFuncGetAttribute", res));
            if (res != CUResult.Success) throw new CudaException(res);
            return temp;
        }

        /// <summary>
        /// The PTX virtual architecture version for which the function was
        /// compiled. This value is the major PTX version * 10 + the minor PTX version, so a PTX version 1.3 function
        /// would return the value 13. Note that this may return the undefined value of 0 for cubins compiled prior to CUDA
        /// 3.0.
        /// </summary>
        public Version GetPtxVersion(CUdevice device)
        {
            int temp = 0;
            CUResult res = DriverAPINativeMethods.LibraryManagement.cuKernelGetAttribute(ref temp, CUFunctionAttribute.PTXVersion, this, device);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuFuncGetAttribute", res));
            if (res != CUResult.Success) throw new CudaException(res);
            return new Version(temp / 10, temp % 10);
        }

        /// <summary>
        /// The binary version for which the function was compiled. This
        /// value is the major binary version * 10 + the minor binary version, so a binary version 1.3 function would return
        /// the value 13. Note that this will return a value of 10 for legacy cubins that do not have a properly-encoded binary
        /// architecture version.
        /// </summary>
        public Version GetBinaryVersion(CUdevice device)
        {
            int temp = 0;
            CUResult res = DriverAPINativeMethods.LibraryManagement.cuKernelGetAttribute(ref temp, CUFunctionAttribute.BinaryVersion, this, device);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuFuncGetAttribute", res));
            if (res != CUResult.Success) throw new CudaException(res);
            return new Version(temp / 10, temp % 10);
        }

        /// <summary>
        /// The attribute to indicate whether the function has been compiled with 
        /// user specified option "-Xptxas --dlcm=ca" set.
        /// </summary>
        public bool GetCacheModeCA(CUdevice device)
        {
            int temp = 0;
            CUResult res = DriverAPINativeMethods.LibraryManagement.cuKernelGetAttribute(ref temp, CUFunctionAttribute.CacheModeCA, this, device);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuFuncGetAttribute", res));
            if (res != CUResult.Success) throw new CudaException(res);
            return temp != 0;
        }

        /// <summary>
        /// This maximum size in bytes of
        /// dynamically-allocated shared memory.The value should contain the requested
        /// maximum size of dynamically-allocated shared memory.The sum of this value and
        /// the function attribute::CU_FUNC_ATTRIBUTE_SHARED_SIZE_BYTES cannot exceed the
        /// device attribute ::CU_DEVICE_ATTRIBUTE_MAX_SHARED_MEMORY_PER_BLOCK_OPTIN.
        /// The maximal size of requestable dynamic shared memory may differ by GPU
        /// architecture.
        /// </summary>
        public int GetMaxDynamicSharedSizeBytes(CUdevice device)
        {
            int temp = 0;
            CUResult res = DriverAPINativeMethods.LibraryManagement.cuKernelGetAttribute(ref temp, CUFunctionAttribute.MaxDynamicSharedSizeBytes, this, device);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuKernelGetAttribute", res));
            if (res != CUResult.Success) throw new CudaException(res);
            return temp;
        }

        /// <summary>
        /// This maximum size in bytes of
        /// dynamically-allocated shared memory.The value should contain the requested
        /// maximum size of dynamically-allocated shared memory.The sum of this value and
        /// the function attribute::CU_FUNC_ATTRIBUTE_SHARED_SIZE_BYTES cannot exceed the
        /// device attribute ::CU_DEVICE_ATTRIBUTE_MAX_SHARED_MEMORY_PER_BLOCK_OPTIN.
        /// The maximal size of requestable dynamic shared memory may differ by GPU
        /// architecture.
        /// </summary>
        public void SetMaxDynamicSharedSizeBytes(int size, CUdevice device)
        {
            CUResult res = DriverAPINativeMethods.LibraryManagement.cuKernelSetAttribute(CUFunctionAttribute.MaxDynamicSharedSizeBytes, size, this, device);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuKernelSetAttribute", res));
            if (res != CUResult.Success) throw new CudaException(res);
        }

        /// <summary>
        /// On devices where the L1
        /// cache and shared memory use the same hardware resources, this sets the shared memory
        /// carveout preference, in percent of the total resources.This is only a hint, and the
        /// driver can choose a different ratio if required to execute the function.
        /// </summary>
        public CUshared_carveout GetPreferredSharedMemoryCarveout(CUdevice device)
        {
            int temp = 0;
            CUResult res = DriverAPINativeMethods.LibraryManagement.cuKernelGetAttribute(ref temp, CUFunctionAttribute.PreferredSharedMemoryCarveout, this, device);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuKernelGetAttribute", res));
            if (res != CUResult.Success) throw new CudaException(res);
            return (CUshared_carveout)temp;
        }

        /// <summary>
        /// On devices where the L1
        /// cache and shared memory use the same hardware resources, this sets the shared memory
        /// carveout preference, in percent of the total resources.This is only a hint, and the
        /// driver can choose a different ratio if required to execute the function.
        /// </summary>
        public void SetPreferredSharedMemoryCarveout(CUshared_carveout value, CUdevice device)
        {
            CUResult res = DriverAPINativeMethods.LibraryManagement.cuKernelSetAttribute(CUFunctionAttribute.PreferredSharedMemoryCarveout, (int)value, this, device);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuKernelSetAttribute", res));
            if (res != CUResult.Success) throw new CudaException(res);
        }

        /// <summary>
        /// If this attribute is set, the kernel must launch with a valid cluster size specified.
        /// See ::cuFuncSetAttribute, ::cuKernelSetAttribute
        /// </summary>
        public bool GetClusterSizeMustBeSet(CUdevice device)
        {
            int temp = 0;
            CUResult res = DriverAPINativeMethods.LibraryManagement.cuKernelGetAttribute(ref temp, CUFunctionAttribute.ClusterSizeMustBeSet, this, device);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuKernelGetAttribute", res));
            if (res != CUResult.Success) throw new CudaException(res);
            return temp != 0;
        }

        /// <summary>
        /// The required cluster width in blocks. The values must either all be 0 or all be positive. 
        /// The validity of the cluster dimensions is otherwise checked at launch time.
        /// If the value is set during compile time, it cannot be set at runtime.
        /// Setting it at runtime will return CUDA_ERROR_NOT_PERMITTED. See ::cuFuncSetAttribute, ::cuKernelSetAttribute
        /// </summary>
        public int GetRequiredClusterWidth(CUdevice device)
        {
            int temp = 0;
            CUResult res = DriverAPINativeMethods.LibraryManagement.cuKernelGetAttribute(ref temp, CUFunctionAttribute.RequiredClusterWidth, this, device);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuKernelGetAttribute", res));
            if (res != CUResult.Success) throw new CudaException(res);
            return temp;
        }

        /// <summary>
        /// The required cluster width in blocks. The values must either all be 0 or all be positive. 
        /// The validity of the cluster dimensions is otherwise checked at launch time.
        /// If the value is set during compile time, it cannot be set at runtime.
        /// Setting it at runtime will return CUDA_ERROR_NOT_PERMITTED. See ::cuFuncSetAttribute, ::cuKernelSetAttribute
        /// </summary>
        public void SetRequiredClusterWidth(int value, CUdevice device)
        {
            CUResult res = DriverAPINativeMethods.LibraryManagement.cuKernelSetAttribute(CUFunctionAttribute.RequiredClusterWidth, value, this, device);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuKernelSetAttribute", res));
            if (res != CUResult.Success) throw new CudaException(res);
        }

        /// <summary>
        /// The required cluster height in blocks. The values must either all be 0 or
        /// all be positive. The validity of the cluster dimensions is otherwise
        /// checked at launch time.
        /// If the value is set during compile time, it cannot be set at runtime.
        /// Setting it at runtime should return CUDA_ERROR_NOT_PERMITTED. See ::cuFuncSetAttribute, ::cuKernelSetAttribute
        /// </summary>
        public int GetRequiredClusterHeight(CUdevice device)
        {
            int temp = 0;
            CUResult res = DriverAPINativeMethods.LibraryManagement.cuKernelGetAttribute(ref temp, CUFunctionAttribute.RequiredClusterHeight, this, device);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuKernelGetAttribute", res));
            if (res != CUResult.Success) throw new CudaException(res);
            return temp;
        }

        /// <summary>
        /// The required cluster height in blocks. The values must either all be 0 or
        /// all be positive. The validity of the cluster dimensions is otherwise
        /// checked at launch time.
        /// If the value is set during compile time, it cannot be set at runtime.
        /// Setting it at runtime should return CUDA_ERROR_NOT_PERMITTED. See ::cuFuncSetAttribute, ::cuKernelSetAttribute
        /// </summary>
        public void SetRequiredClusterHeight(int value, CUdevice device)
        {
            CUResult res = DriverAPINativeMethods.LibraryManagement.cuKernelSetAttribute(CUFunctionAttribute.RequiredClusterHeight, value, this, device);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuKernelSetAttribute", res));
            if (res != CUResult.Success) throw new CudaException(res);
        }

        /// <summary>
        /// The required cluster depth in blocks. The values must either all be 0 or
        /// all be positive. The validity of the cluster dimensions is otherwise
        /// checked at launch time.
        /// If the value is set during compile time, it cannot be set at runtime.
        /// Setting it at runtime should return CUDA_ERROR_NOT_PERMITTED. See ::cuFuncSetAttribute, ::cuKernelSetAttribute
        /// </summary>
        public int GetRequiredClusterDepth(CUdevice device)
        {
            int temp = 0;
            CUResult res = DriverAPINativeMethods.LibraryManagement.cuKernelGetAttribute(ref temp, CUFunctionAttribute.RequiredClusterDepth, this, device);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuKernelGetAttribute", res));
            if (res != CUResult.Success) throw new CudaException(res);
            return temp;
        }

        /// <summary>
        /// The required cluster depth in blocks. The values must either all be 0 or
        /// all be positive. The validity of the cluster dimensions is otherwise
        /// checked at launch time.
        /// If the value is set during compile time, it cannot be set at runtime.
        /// Setting it at runtime should return CUDA_ERROR_NOT_PERMITTED. See ::cuFuncSetAttribute, ::cuKernelSetAttribute
        /// </summary>
        public void SetRequiredClusterDepth(int value, CUdevice device)
        {
            CUResult res = DriverAPINativeMethods.LibraryManagement.cuKernelSetAttribute(CUFunctionAttribute.RequiredClusterDepth, value, this, device);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuKernelSetAttribute", res));
            if (res != CUResult.Success) throw new CudaException(res);
        }

        /// <summary>
        /// Whether the function can be launched with non-portable cluster size. 1 is
        /// allowed, 0 is disallowed. A non-portable cluster size may only function
        /// on the specific SKUs the program is tested on. The launch might fail if
        /// the program is run on a different hardware platform.<para/>
        /// CUDA API provides cudaOccupancyMaxActiveClusters to assist with checking
        /// whether the desired size can be launched on the current device.<para/>
        /// Portable Cluster Size<para/>
        /// A portable cluster size is guaranteed to be functional on all compute
        /// capabilities higher than the target compute capability. The portable
        /// cluster size for sm_90 is 8 blocks per cluster. This value may increase
        /// for future compute capabilities.<para/>
        /// The specific hardware unit may support higher cluster sizes that's not
        /// guaranteed to be portable.<para/>
        /// See ::cuFuncSetAttribute, ::cuKernelSetAttribute
        /// </summary>
        public bool GetNonPortableClusterSizeAllowed(CUdevice device)
        {
            int temp = 0;
            CUResult res = DriverAPINativeMethods.LibraryManagement.cuKernelGetAttribute(ref temp, CUFunctionAttribute.NonPortableClusterSizeAllowed, this, device);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuFuncGetAttribute", res));
            if (res != CUResult.Success) throw new CudaException(res);
            return temp != 0;
        }

        /// <summary>
        /// The block scheduling policy of a function. The value type is CUclusterSchedulingPolicy / cudaClusterSchedulingPolicy.
        /// See ::cuFuncSetAttribute, ::cuKernelSetAttribute
        /// </summary>
        public CUclusterSchedulingPolicy GetClusterSchedulingPolicyPreference(CUdevice device)
        {
            int temp = 0;
            CUResult res = DriverAPINativeMethods.LibraryManagement.cuKernelGetAttribute(ref temp, CUFunctionAttribute.ClusterSchedulingPolicyPreference, this, device);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuFuncGetAttribute", res));
            if (res != CUResult.Success) throw new CudaException(res);
            return (CUclusterSchedulingPolicy)temp;
        }

        /// <summary>
        /// The block scheduling policy of a function. The value type is CUclusterSchedulingPolicy / cudaClusterSchedulingPolicy.
        /// See ::cuFuncSetAttribute, ::cuKernelSetAttribute
        /// </summary>
        public void SetClusterSchedulingPolicyPreference(CUclusterSchedulingPolicy value, CUdevice device)
        {
            CUResult res = DriverAPINativeMethods.LibraryManagement.cuKernelSetAttribute(CUFunctionAttribute.ClusterSchedulingPolicyPreference, (int)value, this, device);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuKernelSetAttribute", res));
            if (res != CUResult.Success) throw new CudaException(res);
        }

        /// <summary>
        /// Sets the preferred cache configuration for a device kernel.<para/>
        /// On devices where the L1 cache and shared memory use the same hardware
        /// resources, this sets through \p config the preferred cache configuration for
        /// the device kernel \p kernel on the requested device \p dev. This is only a preference.
        /// The driver will use the requested configuration if possible, but it is free to choose a different
        /// configuration if required to execute \p kernel.  Any context-wide preference
        /// set via ::cuCtxSetCacheConfig() will be overridden by this per-kernel
        /// setting.<para/>
        /// Note that attributes set using ::cuFuncSetCacheConfig() will override the attribute
        /// set by this API irrespective of whether the call to ::cuFuncSetCacheConfig() is made
        /// before or after this API call.<para/>
        /// This setting does nothing on devices where the size of the L1 cache and
        /// shared memory are fixed.<para/>
        /// Launching a kernel with a different preference than the most recent
        /// preference setting may insert a device-side synchronization point.<para/>
        /// The supported cache configurations are:
        /// - ::CU_FUNC_CACHE_PREFER_NONE: no preference for shared memory or L1 (default)<para/>
        /// - ::CU_FUNC_CACHE_PREFER_SHARED: prefer larger shared memory and smaller L1 cache<para/>
        /// - ::CU_FUNC_CACHE_PREFER_L1: prefer larger L1 cache and smaller shared memory<para/>
        /// - ::CU_FUNC_CACHE_PREFER_EQUAL: prefer equal sized L1 cache and shared memory<para/>
        /// \note The API has stricter locking requirements in comparison to its legacy counterpart
        /// ::cuFuncSetCacheConfig() due to device-wide semantics. If multiple threads are trying to
        /// set a config on the same device simultaneously, the cache config setting will depend
        /// on the interleavings chosen by the OS scheduler and memory consistency.
        /// </summary>
        /// <param name="config">Requested cache configuration</param>
        /// <param name="device">Device to set attribute of</param>
        public void SetCacheConfig(CUFuncCache config, CUdevice device)
        {
            CUResult res;
            res = DriverAPINativeMethods.LibraryManagement.cuKernelSetCacheConfig(this, config, device);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuKernelSetCacheConfig", res));
            if (res != CUResult.Success)
                throw new CudaException(res);
        }
    }

    /// <summary>
    /// Function cache configurations
    /// </summary>
    public enum CUFuncCache
    {
        /// <summary>
        /// No preference for shared memory or L1 (default)
        /// </summary>
        PreferNone = 0x00,
        /// <summary>
        /// Function prefers larger shared memory and smaller L1 cache.
        /// </summary>
        PreferShared = 0x01,
        /// <summary>
        /// Function prefers larger L1 cache and smaller shared memory.
        /// </summary>
        PreferL1 = 0x02,
        /// <summary>
        /// Function prefers equal sized L1 cache and shared memory.
        /// </summary>
        PreferEqual = 0x03
    }

    /// <summary>
    /// Function properties
    /// </summary>
    public enum CUFunctionAttribute
    {
        /// <summary>
        /// <para>The number of threads beyond which a launch of the function would fail.</para>
        /// <para>This number depends on both the function and the device on which the
        /// function is currently loaded.</para>
        /// </summary>
        MaxThreadsPerBlock = 0,

        /// <summary>
        /// <para>The size in bytes of statically-allocated shared memory required by
        /// this function. </para><para>This does not include dynamically-allocated shared
        /// memory requested by the user at runtime.</para>
        /// </summary>
        SharedSizeBytes = 1,

        /// <summary>
        /// <para>The size in bytes of statically-allocated shared memory required by
        /// this function. </para><para>This does not include dynamically-allocated shared
        /// memory requested by the user at runtime.</para>
        /// </summary>
        ConstSizeBytes = 2,

        /// <summary>
        /// The size in bytes of thread local memory used by this function.
        /// </summary>
        LocalSizeBytes = 3,

        /// <summary>
        /// The number of registers used by each thread of this function.
        /// </summary>
        NumRegs = 4,

        /// <summary>
        /// The PTX virtual architecture version for which the function was
        /// compiled. This value is the major PTX version * 10 + the minor PTX version, so a PTX version 1.3 function
        /// would return the value 13. Note that this may return the undefined value of 0 for cubins compiled prior to CUDA
        /// 3.0.
        /// </summary>
        PTXVersion = 5,

        /// <summary>
        /// The binary version for which the function was compiled. This
        /// value is the major binary version * 10 + the minor binary version, so a binary version 1.3 function would return
        /// the value 13. Note that this will return a value of 10 for legacy cubins that do not have a properly-encoded binary
        /// architecture version.
        /// </summary>
        BinaryVersion = 6,

        /// <summary>
        /// The attribute to indicate whether the function has been compiled with 
        /// user specified option "-Xptxas --dlcm=ca" set.
        /// </summary>
        CacheModeCA = 7,

        /// <summary>
        /// The maximum size in bytes of dynamically-allocated shared memory that can be used by
        /// this function. If the user-specified dynamic shared memory size is larger than this
        /// value, the launch will fail.
        /// </summary>
        MaxDynamicSharedSizeBytes = 8,

        /// <summary>
        /// On devices where the L1 cache and shared memory use the same hardware resources, 
        /// this sets the shared memory carveout preference, in percent of the total resources. 
        /// This is only a hint, and the driver can choose a different ratio if required to execute the function.
        /// </summary>
        PreferredSharedMemoryCarveout = 9,

        /// <summary>
        /// If this attribute is set, the kernel must launch with a valid cluster size specified.
        /// See ::cuFuncSetAttribute, ::cuKernelSetAttribute
        /// </summary>
        ClusterSizeMustBeSet = 10,

        /// <summary>
        /// The required cluster width in blocks. The values must either all be 0 or all be positive. 
        /// The validity of the cluster dimensions is otherwise checked at launch time.
        /// If the value is set during compile time, it cannot be set at runtime.
        /// Setting it at runtime will return CUDA_ERROR_NOT_PERMITTED. See ::cuFuncSetAttribute, ::cuKernelSetAttribute
        /// </summary>
        RequiredClusterWidth = 11,

        /// <summary>
        /// The required cluster height in blocks. The values must either all be 0 or
        /// all be positive. The validity of the cluster dimensions is otherwise
        /// checked at launch time.
        /// If the value is set during compile time, it cannot be set at runtime.
        /// Setting it at runtime should return CUDA_ERROR_NOT_PERMITTED. See ::cuFuncSetAttribute, ::cuKernelSetAttribute
        /// </summary>
        RequiredClusterHeight = 12,

        /// <summary>
        /// The required cluster depth in blocks. The values must either all be 0 or
        /// all be positive. The validity of the cluster dimensions is otherwise
        /// checked at launch time.
        /// If the value is set during compile time, it cannot be set at runtime.
        /// Setting it at runtime should return CUDA_ERROR_NOT_PERMITTED. See ::cuFuncSetAttribute, ::cuKernelSetAttribute
        /// </summary>
        RequiredClusterDepth = 13,

        /// <summary>
        /// Whether the function can be launched with non-portable cluster size. 1 is
        /// allowed, 0 is disallowed. A non-portable cluster size may only function
        /// on the specific SKUs the program is tested on. The launch might fail if
        /// the program is run on a different hardware platform.<para/>
        /// CUDA API provides cudaOccupancyMaxActiveClusters to assist with checking
        /// whether the desired size can be launched on the current device.<para/>
        /// Portable Cluster Size<para/>
        /// A portable cluster size is guaranteed to be functional on all compute
        /// capabilities higher than the target compute capability. The portable
        /// cluster size for sm_90 is 8 blocks per cluster. This value may increase
        /// for future compute capabilities.<para/>
        /// The specific hardware unit may support higher cluster sizes that's not
        /// guaranteed to be portable.<para/>
        /// See ::cuFuncSetAttribute, ::cuKernelSetAttribute
        /// </summary>
        NonPortableClusterSizeAllowed = 14,

        /// <summary>
        /// The block scheduling policy of a function. The value type is CUclusterSchedulingPolicy / cudaClusterSchedulingPolicy.
        /// See ::cuFuncSetAttribute, ::cuKernelSetAttribute
        /// </summary>
        ClusterSchedulingPolicyPreference = 15,

        /// <summary>
        /// No descritption found...
        /// </summary>
        Max
    }

    /// <summary>
    /// Shared memory carveout configurations
    /// </summary>
    public enum CUshared_carveout
    {
        /// <summary>
        /// no preference for shared memory or L1 (default)
        /// </summary>
        Default = -1,
        /// <summary>
        /// prefer maximum available shared memory, minimum L1 cache
        /// </summary>
        MaxShared = 100,
        /// <summary>
        /// prefer maximum available L1 cache, minimum shared memory
        /// </summary>
        MaxL1 = 0
    }

    /// <summary>
    /// 2D memory copy parameters
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CUDAMemCpy2D
    {
        /// <summary>
        /// Source X in bytes
        /// </summary>
        public SizeT srcXInBytes;

        /// <summary>
        /// Source Y
        /// </summary>
        public SizeT srcY;

        /// <summary>
        /// Source memory type (host, device, array)
        /// </summary>
        public CUMemoryType srcMemoryType;

        /// <summary>
        /// Source host pointer
        /// </summary>
        public IntPtr srcHost;

        /// <summary>
        /// Source device pointer
        /// </summary>
        public CUdeviceptr srcDevice;

        /// <summary>
        /// Source array reference
        /// </summary>
        public CUarray srcArray;

        /// <summary>
        /// Source pitch (ignored when src is array)
        /// </summary>
        public SizeT srcPitch;

        /// <summary>
        /// Destination X in bytes
        /// </summary>
        public SizeT dstXInBytes;

        /// <summary>
        /// Destination Y
        /// </summary>
        public SizeT dstY;

        /// <summary>
        /// Destination memory type (host, device, array)
        /// </summary>
        public CUMemoryType dstMemoryType;

        /// <summary>
        /// Destination host pointer
        /// </summary>
        public IntPtr dstHost;

        /// <summary>
        /// Destination device pointer
        /// </summary>
        public CUdeviceptr dstDevice;

        /// <summary>
        /// Destination array reference
        /// </summary>
        public CUarray dstArray;

        /// <summary>
        /// Destination pitch (ignored when dst is array)
        /// </summary>
        public SizeT dstPitch;

        /// <summary>
        /// Width of 2D memory copy in bytes
        /// </summary>
        public SizeT WidthInBytes;

        /// <summary>
        /// Height of 2D memory copy
        /// </summary>
        public SizeT Height;
    }

    /// <summary>
    /// CUDA array
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CUarray
    {
        /// <summary>
        /// 
        /// </summary>
        public IntPtr Pointer;

        /// <summary>
        /// Returns the memory requirements of a CUDA array
        /// </summary>
        public CudaArrayMemoryRequirements GetMemoryRequirements(CUdevice device)
        {
            CudaArrayMemoryRequirements temp = new CudaArrayMemoryRequirements();
            CUResult res = DriverAPINativeMethods.ArrayManagement.cuArrayGetMemoryRequirements(ref temp, this, device);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuArrayGetMemoryRequirements", res));
            if (res != CUResult.Success) throw new CudaException(res);
            return temp;
        }
    }

    /// <summary>
    /// CUDA array memory requirements
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CudaArrayMemoryRequirements
    {
        /// <summary>
        /// Total required memory size
        /// </summary>
        public SizeT size;
        /// <summary>
        /// alignment requirement
        /// </summary>
        public SizeT alignment;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4, ArraySubType = UnmanagedType.U4)]
        private uint[] reserved;
    }

    /// <summary>
    /// 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CUmemPoolPtrExportData
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64, ArraySubType = UnmanagedType.U1)]
        byte[] reserved;
    }

    /// <summary>
    /// 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CUmemPoolProps
    {
        /// <summary/>
        public CUmemAllocationType allocType;
        /// <summary/>
        public CUmemAllocationHandleType handleTypes;
        /// <summary/>
        public CUmemLocation location;
        /// <summary/>
        public IntPtr win32SecurityAttributes;
        /// <summary/>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64, ArraySubType = UnmanagedType.U1)]
        byte[] reserved;
    }

    /// <summary>
    /// Defines the allocation types available
    /// </summary>
    public enum CUmemAllocationType
    {
        /// <summary>
        /// 
        /// </summary>
        Invalid = 0x0,
        /// <summary>
        /// This allocation type is 'pinned', i.e. cannot migrate from its current
        /// location while the application is actively using it
        /// </summary>
        Pinned = 0x1
    }

    /// <summary>
    /// Specifies a location for an allocation.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CUmemLocation
    {
        /// <summary>
        /// Specifies the location type, which modifies the meaning of id.
        /// </summary>
        public CUmemLocationType type;
        /// <summary>
        /// identifier for a given this location's ::CUmemLocationType.
        /// </summary>
        public int id;
    }

    /// <summary>
    /// Specifies the type of location
    /// </summary>
    public enum CUmemLocationType
    {
        /// <summary>
        /// 
        /// </summary>
        Invalid = 0x0,
        /// <summary>
        /// Location is a device location, thus id is a device ordinal
        /// </summary>
        Device = 0x1
    }

    /// <summary>
    /// Specifies the memory protection flags for mapping.
    /// </summary>
    [Flags]
    public enum CUmemAccess_flags
    {
        /// <summary>
        /// Default, make the address range not accessible
        /// </summary>
        ProtNone = 0x1,
        /// <summary>
        /// Make the address range read accessible
        /// </summary>
        ProtRead = 0x2,
        /// <summary>
        /// Make the address range read-write accessible
        /// </summary>
        ProtReadWrite = 0x3
    }

    /// <summary>
    ///  Memory access descriptor
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CUmemAccessDesc
    {
        /// <summary>
        /// Location on which the request is to change it's accessibility
        /// </summary>
        public CUmemLocation location;
        /// <summary>
        /// ::CUmemProt accessibility flags to set on the request
        /// </summary>
        public CUmemAccess_flags flags;
    }

    /// <summary>
    /// Online compiler options
    /// </summary>
    public enum CUJITOption
    {
        /// <summary>
        /// <para>Max number of registers that a thread may use.</para>
        /// <para>Option type: unsigned int</para>
        /// <para>Applies to: compiler only</para>
        /// </summary>
        MaxRegisters = 0,

        /// <summary>
        /// <para>IN: Specifies minimum number of threads per block to target compilation
        /// for</para>
        /// <para>OUT: Returns the number of threads the compiler actually targeted.
        /// This restricts the resource utilization fo the compiler (e.g. max
        /// registers) such that a block with the given number of threads should be
        /// able to launch based on register limitations. Note, this option does not
        /// currently take into account any other resource limitations, such as
        /// shared memory utilization.</para>
        /// <para>Option type: unsigned int</para>
        /// <para>Applies to: compiler only</para>
        /// </summary>
        ThreadsPerBlock = 1,

        /// <summary>
        /// Returns a float value in the option of the wall clock time, in
        /// milliseconds, spent creating the cubin<para/>
        /// Option type: float
        /// <para>Applies to: compiler and linker</para>
        /// </summary>
        WallTime = 2,

        /// <summary>
        /// <para>Pointer to a buffer in which to print any log messsages from PTXAS
        /// that are informational in nature (the buffer size is specified via
        /// option ::CU_JIT_INFO_LOG_BUFFER_SIZE_BYTES)</para>
        /// <para>Option type: char*</para>
        /// <para>Applies to: compiler and linker</para>
        /// </summary>
        InfoLogBuffer = 3,

        /// <summary>
        /// <para>IN: Log buffer size in bytes.  Log messages will be capped at this size
        /// (including null terminator)</para>
        /// <para>OUT: Amount of log buffer filled with messages</para>
        /// <para>Option type: unsigned int</para>
        /// <para>Applies to: compiler and linker</para>
        /// </summary>
        InfoLogBufferSizeBytes = 4,

        /// <summary>
        /// <para>Pointer to a buffer in which to print any log messages from PTXAS that
        /// reflect errors (the buffer size is specified via option
        /// ::CU_JIT_ERROR_LOG_BUFFER_SIZE_BYTES)</para>
        /// <para>Option type: char*</para>
        /// <para>Applies to: compiler and linker</para>
        /// </summary>
        ErrorLogBuffer = 5,

        /// <summary>
        /// <para>IN: Log buffer size in bytes.  Log messages will be capped at this size
        /// (including null terminator)</para>
        /// <para>OUT: Amount of log buffer filled with messages</para>
        /// <para>Option type: unsigned int</para>
        /// <para>Applies to: compiler and linker</para>
        /// </summary>
        ErrorLogBufferSizeBytes = 6,


        /// <summary>
        /// <para>Level of optimizations to apply to generated code (0 - 4), with 4
        /// being the default and highest level of optimizations.</para>
        /// <para>Option type: unsigned int</para>
        /// <para>Applies to: compiler only</para>
        /// </summary>
        OptimizationLevel = 7,

        /// <summary>
        /// <para>No option value required. Determines the target based on the current
        /// attached context (default)</para>
        /// <para>Option type: No option value needed</para>
        /// <para>Applies to: compiler and linker</para>
        /// </summary>
        TargetFromContext = 8,

        /// <summary>
        /// <para>Target is chosen based on supplied ::CUjit_target_enum. This option cannot be
        /// used with cuLink* APIs as the linker requires exact matches.</para>
        /// <para>Option type: unsigned int for enumerated type ::CUjit_target_enum</para>
        /// <para>Applies to: compiler and linker</para>
        /// </summary>
        Target = 9,

        /// <summary>
        /// <para>Specifies choice of fallback strategy if matching cubin is not found.
        /// Choice is based on supplied ::CUjit_fallback_enum.</para>
        /// <para>Option type: unsigned int for enumerated type ::CUjit_fallback_enum</para>
        /// <para>Applies to: compiler only</para>
        /// </summary>
        FallbackStrategy = 10,

        /// <summary>
        /// Specifies whether to create debug information in output (-g) <para/> (0: false, default)
        /// <para>Option type: int</para>
        /// <para>Applies to: compiler and linker</para>
        /// </summary>
        GenerateDebugInfo = 11,

        /// <summary>
        /// Generate verbose log messages <para/> (0: false, default)
        /// <para>Option type: int</para>
        /// <para>Applies to: compiler and linker</para>
        /// </summary>
        LogVerbose = 12,

        /// <summary>
        /// Generate line number information (-lineinfo) <para/> (0: false, default)
        /// <para>Option type: int</para>
        /// <para>Applies to: compiler only</para>
        /// </summary>
        GenerateLineInfo = 13,

        /// <summary>
        /// Specifies whether to enable caching explicitly (-dlcm)<para/>
        /// Choice is based on supplied ::CUjit_cacheMode_enum.
        /// <para>Option type: unsigned int for enumerated type ::CUjit_cacheMode_enum</para>
        /// <para>Applies to: compiler only</para>
        /// </summary>
        JITCacheMode = 14,

        /// <summary>
        /// The below jit options are used for internal purposes only, in this version of CUDA
        /// </summary>
        [Obsolete("This jit option is deprecated and should not be used.")]
        NewSM3XOpt = 15,

        /// <summary>
        /// This jit option is used for internal purpose only.
        /// </summary>
        FastCompile = 16,

        /// <summary>
        /// Array of device symbol names that will be relocated to the corresponing
        /// host addresses stored in ::CU_JIT_GLOBAL_SYMBOL_ADDRESSES.<para/>
        /// Must contain ::CU_JIT_GLOBAL_SYMBOL_COUNT entries.<para/>
        /// When loding a device module, driver will relocate all encountered
        /// unresolved symbols to the host addresses.<para/>
        /// It is only allowed to register symbols that correspond to unresolved
        /// global variables.<para/>
        /// It is illegal to register the same device symbol at multiple addresses.<para/>
        /// Option type: const char **<para/>
        /// Applies to: dynamic linker only
        /// </summary>
        GlobalSymbolNames = 17,

        /// <summary>
        /// Array of host addresses that will be used to relocate corresponding
        /// device symbols stored in ::CU_JIT_GLOBAL_SYMBOL_NAMES.<para/>
        /// Must contain ::CU_JIT_GLOBAL_SYMBOL_COUNT entries.<para/>
        /// Option type: void **<para/>
        /// Applies to: dynamic linker only
        /// </summary>
        GlobalSymbolAddresses = 18,

        /// <summary>
        /// Number of entries in ::CU_JIT_GLOBAL_SYMBOL_NAMES and
        /// ::CU_JIT_GLOBAL_SYMBOL_ADDRESSES arrays.<para/>
        /// Option type: unsigned int<para/>
        /// Applies to: dynamic linker only
        /// </summary>
        GlobalSymbolCount = 19,

        /// <summary>
        /// Enable link-time optimization (-dlto) for device code (0: false, default)<para/>
        /// Option type: int<para/>
        /// Applies to: compiler and linker
        /// </summary>
        [Obsolete("This jit option is deprecated and should not be used.")]
        Lto = 20,

        /// <summary>
        /// Control single-precision denormals (-ftz) support (0: false, default).<para/>
        /// 1 : flushes denormal values to zero<para/>
        /// 0 : preserves denormal values<para/>
        /// Option type: int<para/>
        /// Applies to: link-time optimization specified with CU_JIT_LTO
        /// </summary>
        [Obsolete("This jit option is deprecated and should not be used.")]
        Ftz = 21,

        /// <summary>
        /// Control single-precision floating-point division and reciprocals<para/>
        /// (-prec-div) support (1: true, default).<para/>
        /// 1 : Enables the IEEE round-to-nearest mode<para/>
        /// 0 : Enables the fast approximation mode<para/>
        /// Option type: int<para/>
        /// Applies to: link-time optimization specified with CU_JIT_LTO
        /// </summary>
        [Obsolete("This jit option is deprecated and should not be used.")]
        PrecDiv = 22,

        /// <summary>
        /// Control single-precision floating-point square root<para/>
        /// (-prec-sqrt) support (1: true, default).<para/>
        /// 1 : Enables the IEEE round-to-nearest mode<para/>
        /// 0 : Enables the fast approximation mode<para/>
        /// Option type: int\n<para/>
        /// Applies to: link-time optimization specified with CU_JIT_LTO
        /// </summary>
        [Obsolete("This jit option is deprecated and should not be used.")]
        PrecSqrt = 23,

        /// <summary>
        /// Enable/Disable the contraction of floating-point multiplies<para/>
        /// and adds/subtracts into floating-point multiply-add (-fma)<para/>
        /// operations (1: Enable, default; 0: Disable).<para/>
        /// Option type: int\n<para/>
        /// Applies to: link-time optimization specified with CU_JIT_LTO
        /// </summary>
        [Obsolete("This jit option is deprecated and should not be used.")]
        Fma = 24,

        /// <summary>
        /// Array of kernel names that should be preserved at link time while others
        /// can be removed.\n
        /// Must contain ::CU_JIT_REFERENCED_KERNEL_COUNT entries.\n
        /// Note that kernel names can be mangled by the compiler in which case the
        /// mangled name needs to be specified.\n
        /// Wildcard "*" can be used to represent zero or more characters instead of
        /// specifying the full or mangled name.\n
        /// It is important to note that the wildcard "*" is also added implicitly.
        /// For example, specifying "foo" will match "foobaz", "barfoo", "barfoobaz" and
        /// thus preserve all kernels with those names. This can be avoided by providing
        /// a more specific name like "barfoobaz".\n
        /// Option type: const char **\n
        /// Applies to: dynamic linker only
        ///
        /// Only valid with LTO-IR compiled with toolkits prior to CUDA 12.0
        /// </summary>
        [Obsolete("This jit option is deprecated and should not be used.")]
        ReferencedKernelNames = 25,

        /// <summary>
        /// Number of entries in ::CU_JIT_REFERENCED_KERNEL_NAMES array.\n
        /// Option type: unsigned int\n
        /// Applies to: dynamic linker only
        ///
        /// Only valid with LTO-IR compiled with toolkits prior to CUDA 12.0
        /// </summary>
        [Obsolete("This jit option is deprecated and should not be used.")]
        ReferencedKernelCount = 26,

        /// <summary>
        /// Array of variable names (__device__ and/or __constant__) that should be
        /// preserved at link time while others can be removed.\n
        /// Must contain ::CU_JIT_REFERENCED_VARIABLE_COUNT entries.\n
        /// Note that variable names can be mangled by the compiler in which case the
        /// mangled name needs to be specified.\n
        /// Wildcard "*" can be used to represent zero or more characters instead of
        /// specifying the full or mangled name.\n
        /// It is important to note that the wildcard "*" is also added implicitly.
        /// For example, specifying "foo" will match "foobaz", "barfoo", "barfoobaz" and
        /// thus preserve all variables with those names. This can be avoided by providing
        /// a more specific name like "barfoobaz".\n
        /// Option type: const char **\n
        /// Applies to: link-time optimization specified with CU_JIT_LTO
        ///
        /// Only valid with LTO-IR compiled with toolkits prior to CUDA 12.0
        /// </summary>
        [Obsolete("This jit option is deprecated and should not be used.")]
        ReferencedVariableNames = 27,

        /// <summary>
        /// Number of entries in ::CU_JIT_REFERENCED_VARIABLE_NAMES array.\n
        /// Option type: unsigned int\n
        /// Applies to: link-time optimization specified with CU_JIT_LTO
        ///
        /// Only valid with LTO-IR compiled with toolkits prior to CUDA 12.0
        /// </summary>
        [Obsolete("This jit option is deprecated and should not be used.")]
        ReferencedVariableCount = 28,

        /// <summary>
        /// This option serves as a hint to enable the JIT compiler/linker
        /// to remove constant (__constant__) and device (__device__) variables
        /// unreferenced in device code (Disabled by default).\n
        /// Note that host references to constant and device variables using APIs like
        /// ::cuModuleGetGlobal() with this option specified may result in undefined behavior unless
        /// the variables are explicitly specified using ::CU_JIT_REFERENCED_VARIABLE_NAMES.\n
        /// Option type: int\n
        /// Applies to: link-time optimization specified with CU_JIT_LTO
        ///
        /// Only valid with LTO-IR compiled with toolkits prior to CUDA 12.0
        /// </summary>
        [Obsolete("This jit option is deprecated and should not be used.")]
        OptimizeUnusedDeviceVariables = 29,

        /// <summary>
        /// Generate position independent code (0: false)\n
        /// Option type: int\n
        /// Applies to: compiler only
        /// </summary>
        PositionIndependentCode = 30,
    }

    /// <summary>
    /// Library options to be specified with ::cuLibraryLoadData() or ::cuLibraryLoadFromFile()
    /// </summary>
    public enum CUlibraryOption
    {
        /// <summary>
        /// 
        /// </summary>
        HostUniversalFunctionAndDataTable = 0,

        /// <summary>
        /// Specifes that the argument \p code passed to ::cuLibraryLoadData() will be preserved.
        /// Specifying this option will let the driver know that \p code can be accessed at any point
        /// until ::cuLibraryUnload(). The default behavior is for the driver to allocate and
        /// maintain its own copy of \p code. Note that this is only a memory usage optimization
        /// hint and the driver can choose to ignore it if required.
        /// Specifying this option with ::cuLibraryLoadFromFile() is invalid and
        /// will return ::CUDA_ERROR_INVALID_VALUE.
        /// </summary>
        BinaryIsPreserved = 1,
        /// <summary>
        /// 
        /// </summary>
        NumOptions
    }

    /// <summary>
    /// Event record flags
    /// </summary>
    [Flags]
    public enum CUEventRecordFlags : uint
    {
        /// <summary>
        /// Default event record flag
        /// </summary>
        Default = 0x0,
        /// <summary>
        /// When using stream capture, create an event record node
        /// instead of the default behavior. This flag is invalid
        /// when used outside of capture.
        /// </summary>
        External = 0x1
    }

    /// <summary>
    /// Event creation flags
    /// </summary>
    [Flags]
    public enum CUEventFlags
    {
        /// <summary>
        /// Default event creation flag.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Specifies that event should use blocking synchronization. A CPU thread
        /// that uses <see cref="DriverAPINativeMethods.Events.cuEventSynchronize"/> to wait on an event created with this flag will block until the event has actually
        /// been recorded.
        /// </summary>
        BlockingSync = 1,

        /// <summary>
        /// Event will not record timing data
        /// </summary>
        DisableTiming = 2,

        /// <summary>
        /// Event is suitable for interprocess use. CUEventFlags.DisableTiming must be set
        /// </summary>
        InterProcess = 4
    }

    /// <summary>
    /// Kernel launch parameters
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CudaLaunchParams
    {
        /// <summary>
        /// Kernel to launch
        /// </summary>
        public CUfunction function;
        /// <summary>
        /// Width of grid in blocks
        /// </summary>
        public uint gridDimX;
        /// <summary>
        /// Height of grid in blocks
        /// </summary>
        public uint gridDimY;
        /// <summary>
        /// Depth of grid in blocks
        /// </summary>
        public uint gridDimZ;
        /// <summary>
        /// X dimension of each thread block
        /// </summary>
        public uint blockDimX;
        /// <summary>
        /// Y dimension of each thread block
        /// </summary>
        public uint blockDimY;
        /// <summary>
        /// Z dimension of each thread block
        /// </summary>
        public uint blockDimZ;
        /// <summary>
        /// Dynamic shared-memory size per thread block in bytes
        /// </summary>
        public uint sharedMemBytes;
        /// <summary>
        /// Stream identifier
        /// </summary>
        public CUstream hStream;
        /// <summary>
        /// Array of pointers to kernel parameters
        /// </summary>
        public IntPtr kernelParams;
    }

    /// <summary>
    /// CudaCooperativeLaunchMultiDeviceFlags
    /// </summary>
    [Flags]
    public enum CudaCooperativeLaunchMultiDeviceFlags
    {
        /// <summary>
        /// No flags
        /// </summary>
        None = 0,

        /// <summary>
        /// If set, each kernel launched as part of ::cuLaunchCooperativeKernelMultiDevice only
        /// waits for prior work in the stream corresponding to that GPU to complete before the
        /// kernel begins execution.
        /// </summary>
        NoPreLaunchSync = 0x01,

        /// <summary>
        /// If set, any subsequent work pushed in a stream that participated in a call to
        /// ::cuLaunchCooperativeKernelMultiDevice will only wait for the kernel launched on
        /// the GPU corresponding to that stream to complete before it begins execution.
        /// </summary>
        NoPostLaunchSync = 0x02,
    }

    /// <summary>
    /// Shared memory configurations
    /// </summary>
    public enum CUsharedconfig
    {
        /// <summary>
        /// set default shared memory bank size 
        /// </summary>
        DefaultBankSize = 0x00,
        /// <summary>
        /// set shared memory bank width to four bytes
        /// </summary>
        FourByteBankSize = 0x01,
        /// <summary>
        /// set shared memory bank width to eight bytes
        /// </summary>
        EightByteBankSize = 0x02
    }

    /// <summary>
    /// Block size to per-block dynamic shared memory mapping for a certain
    /// kernel.<para/>
    /// e.g.:
    /// If no dynamic shared memory is used: x => 0<para/>
    /// If 4 bytes shared memory per thread is used: x = 4 * x
    /// </summary>
    /// <param name="aBlockSize">block size</param>
    /// <returns>The dynamic shared memory needed by a block</returns>
    public delegate SizeT del_CUoccupancyB2DSize(int aBlockSize);

    /// <summary>
    /// Occupancy calculator flag
    /// </summary>
    public enum CUoccupancy_flags
    {
        /// <summary>
        /// Default behavior
        /// </summary>
        Default = 0,

        /// <summary>
        /// Assume global caching is enabled and cannot be automatically turned off
        /// </summary>
        DisableCachingOverride = 1
    }

    /// <summary>
    /// The Operation type indicates which operation needs to be performed with the
    /// dense matrix. Its values correspond to Fortran characters ‘N’ or ‘n’ (non-transpose), ‘T’
    /// or ‘t’ (transpose) and ‘C’ or ‘c’ (conjugate transpose) that are often used as parameters
    /// to legacy BLAS implementations
    /// </summary>
    public enum Operation
    {
        /// <summary>
        /// the non-transpose operation is selected
        /// </summary>
        NonTranspose = 0,
        /// <summary>
        /// the transpose operation is selected
        /// </summary>
        Transpose = 1,
        /// <summary>
        /// the conjugate transpose operation is selected
        /// </summary>
        ConjugateTranspose = 2,
        /// <summary>
        /// synonym of ConjugateTranspose
        /// </summary>
        Hermitan = 2,
        /// <summary>
        /// the conjugate operation is selected
        /// </summary>
        Conjugate = 3

    }

    /// <summary>
    /// Opaque structure holding CUBLAS library context
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CudaBlasHandle
    {
        /// <summary>
        /// 
        /// </summary>
        public IntPtr Pointer;
    }

    //These library types do not really fit in here, but all libraries depend on managedCuda...
    /// <summary>
    /// cudaDataType
    /// </summary>
    public enum cudaDataType
    {
        /// <summary>
        /// 16 bit real 
        /// </summary>
        CUDA_R_16F = 2,

        /// <summary>
        /// 16 bit complex
        /// </summary>
        CUDA_C_16F = 6,

        /// <summary>
        /// 32 bit real
        /// </summary>
        CUDA_R_32F = 0,

        /// <summary>
        /// 32 bit complex
        /// </summary>
        CUDA_C_32F = 4,

        /// <summary>
        /// 64 bit real
        /// </summary>
        CUDA_R_64F = 1,

        /// <summary>
        /// 64 bit complex
        /// </summary>
        CUDA_C_64F = 5,

        /// <summary>
        /// 8 bit real as a signed integer 
        /// </summary>
        CUDA_R_8I = 3,

        /// <summary>
        /// 8 bit complex as a pair of signed integers
        /// </summary>
        CUDA_C_8I = 7,

        /// <summary>
        /// 8 bit real as a signed integer 
        /// </summary>
        CUDA_R_8U = 8,

        /// <summary>
        /// 8 bit complex as a pair of signed integers
        /// </summary>
        CUDA_C_8U = 9,

        /// <summary>
        /// real as a nv_bfloat16
        /// </summary>
        CUDA_R_16BF = 14,
        /// <summary>
        /// complex as a pair of nv_bfloat16 numbers
        /// </summary>
        CUDA_C_16BF = 15,
        /// <summary>
        /// real as a signed 4-bit int
        /// </summary>
        CUDA_R_4I = 16,
        /// <summary>
        /// complex as a pair of signed 4-bit int numbers
        /// </summary>
        CUDA_C_4I = 17,
        /// <summary>
        /// real as a unsigned 4-bit int
        /// </summary>
        CUDA_R_4U = 18,
        /// <summary>
        /// complex as a pair of unsigned 4-bit int numbers
        /// </summary>
        CUDA_C_4U = 19,
        /// <summary>
        /// real as a signed 16-bit int 
        /// </summary>
        CUDA_R_16I = 20,
        /// <summary>
        /// complex as a pair of signed 16-bit int numbers
        /// </summary>
        CUDA_C_16I = 21,
        /// <summary>
        /// real as a unsigned 16-bit int
        /// </summary>
        CUDA_R_16U = 22,
        /// <summary>
        /// complex as a pair of unsigned 16-bit int numbers
        /// </summary>
        CUDA_C_16U = 23,
        /// <summary>
        /// real as a signed 32-bit int
        /// </summary>
        CUDA_R_32I = 10,
        /// <summary>
        /// complex as a pair of signed 32-bit int numbers
        /// </summary>
        CUDA_C_32I = 11,
        /// <summary>
        /// real as a unsigned 32-bit int
        /// </summary>
        CUDA_R_32U = 12,
        /// <summary>
        /// complex as a pair of unsigned 32-bit int numbers
        /// </summary>
        CUDA_C_32U = 13,
        /// <summary>
        /// real as a signed 64-bit int
        /// </summary>
        CUDA_R_64I = 24,
        /// <summary>
        /// complex as a pair of signed 64-bit int numbers 
        /// </summary>
        CUDA_C_64I = 25,
        /// <summary>
        /// real as a unsigned 64-bit int
        /// </summary>
        CUDA_R_64U = 26,
        /// <summary>
        /// complex as a pair of unsigned 64-bit int numbers
        /// </summary>
        CUDA_C_64U = 27


    }

    

    /// <summary>
    /// The FillMode type indicates which part (lower or upper) of the dense matrix was
    /// filled and consequently should be used by the function. Its values correspond to Fortran
    /// characters ‘L’ or ‘l’ (lower) and ‘U’ or ‘u’ (upper) that are often used as parameters to
    /// legacy BLAS implementations.
    /// </summary>
    public enum FillMode
    {
        /// <summary>
        /// the lower part of the matrix is filled
        /// </summary>
        Lower = 0,
        /// <summary>
        /// the upper part of the matrix is filled
        /// </summary>
        Upper = 1,
        /// <summary>
        /// Full
        /// </summary>
        Full = 2
    }

    /// <summary>
    /// The DiagType type indicates whether the main diagonal of the dense matrix is
    /// unity and consequently should not be touched or modified by the function. Its values
    /// correspond to Fortran characters ‘N’ or ‘n’ (non-unit) and ‘U’ or ‘u’ (unit) that are
    /// often used as parameters to legacy BLAS implementations.
    /// </summary>
    public enum DiagType
    {
        /// <summary>
        /// the matrix diagonal has non-unit elements
        /// </summary>
        NonUnit = 0,
        /// <summary>
        /// the matrix diagonal has unit elements
        /// </summary>
        Unit = 1
    }

    /// <summary>
    /// The SideMode type indicates whether the dense matrix is on the left or right side
    /// in the matrix equation solved by a particular function. Its values correspond to Fortran
    /// characters ‘L’ or ‘l’ (left) and ‘R’ or ‘r’ (right) that are often used as parameters to
    /// legacy BLAS implementations.
    /// </summary>
    public enum SideMode
    {
        /// <summary>
        /// the matrix is on the left side in the equation
        /// </summary>
        Left = 0,
        /// <summary>
        /// the matrix is on the right side in the equation
        /// </summary>
        Right = 1
    }

    /// <summary>
    /// The PointerMode type indicates whether the scalar values are passed by
    /// reference on the host or device. It is important to point out that if several scalar values are
    /// present in the function call, all of them must conform to the same single pointer mode.
    /// The pointer mode can be set and retrieved using cublasSetPointerMode() and
    /// cublasGetPointerMode() routines, respectively.
    /// </summary>
    public enum PointerMode
    {
        /// <summary>
        /// the scalars are passed by reference on the host
        /// </summary>
        Host = 0,
        /// <summary>
        /// the scalars are passed by reference on the device
        /// </summary>
        Device = 1
    }

    /// <summary>
    /// The type indicates whether cuBLAS routines which has an alternate implementation
    /// using atomics can be used. The atomics mode can be set and queried using and routines,
    /// respectively.
    /// </summary>
    public enum AtomicsMode
    {
        /// <summary>
        /// the usage of atomics is not allowed
        /// </summary>
        NotAllowed = 0,
        /// <summary>
        /// the usage of atomics is allowed
        /// </summary>
        Allowed = 1
    }

    /// <summary>
    /// For different GEMM algorithm
    /// </summary>
    public enum GemmAlgo
    {
        /// <summary>
        /// </summary>
        Default = -1,
        /// <summary>
        /// </summary>
        Algo0 = 0,
        /// <summary>
        /// </summary>
        Algo1 = 1,
        /// <summary>
        /// </summary>
        Algo2 = 2,
        /// <summary>
        /// </summary>
        Algo3 = 3,
        /// <summary>
        /// </summary>
        Algo4 = 4,
        /// <summary>
        /// </summary>
        Algo5 = 5,
        /// <summary>
        /// </summary>
        Algo6 = 6,
        /// <summary>
        /// </summary>
        Algo7 = 7,
        /// <summary>
        /// </summary>
        Algo8 = 8,
        /// <summary>
        /// </summary>
        Algo9 = 9,
        /// <summary>
        /// </summary>
        Algo10 = 10,
        /// <summary>
        /// </summary>
        Algo11 = 11,
        /// <summary>
        /// </summary>
        Algo12 = 12,
        /// <summary>
        /// </summary>
        Algo13 = 13,
        /// <summary>
        /// </summary>
        Algo14 = 14,
        /// <summary>
        /// </summary>
        Algo15 = 15,
        /// <summary>
        /// </summary>
        Algo16 = 16,
        /// <summary>
        /// </summary>
        Algo17 = 17,
        /// <summary>
        /// sliced 32x32  
        /// </summary>
        Algo18 = 18,
        /// <summary>
        /// sliced 64x32
        /// </summary>  
        Algo19 = 19,
        /// <summary>
        /// sliced 128x32
        /// </summary>  
        Algo20 = 20,
        /// <summary>
        /// sliced 32x32  -splitK
        /// </summary>   
        Algo21 = 21,
        /// <summary>
        /// sliced 64x32  -splitK
        /// </summary>      
        Algo22 = 22,
        /// <summary>
        /// sliced 128x32 -splitK 
        /// </summary>    
        Algo23 = 23, //     
        /// <summary>
        /// </summary>
        DefaultTensorOp = 99,
        /// <summary>
        /// </summary>
        Algo0TensorOp = 100,
        /// <summary>
        /// </summary>
        Algo1TensorOp = 101,
        /// <summary>
        /// </summary>
        Algo2TensorOp = 102,
        /// <summary>
        /// </summary>
        Algo3TensorOp = 103,
        /// <summary>
        /// </summary>
        Algo4TensorOp = 104,
        /// <summary>
        /// </summary>
        Algo5TensorOp = 105,
        /// <summary>
        /// </summary>
        Algo6TensorOp = 106,
        /// <summary>
        /// </summary>
        Algo7TensorOp = 107,
        /// <summary>
        /// </summary>
        Algo8TensorOp = 108,
        /// <summary>
        /// </summary>
        Algo9TensorOp = 109,
        /// <summary>
        /// </summary>
        Algo10TensorOp = 110,
        /// <summary>
        /// </summary>
        Algo11TensorOp = 111,
        /// <summary>
        /// </summary>
        Algo12TensorOp = 112,
        /// <summary>
        /// </summary>
        Algo13TensorOp = 113,
        /// <summary>
        /// </summary>
        Algo14TensorOp = 114,
        /// <summary>
        /// </summary>
        Algo15TensorOp = 115
    }

    /// <summary>
    /// Enum for default math mode/tensor operation
    /// </summary>
    public enum Math
    {
        /// <summary>
        /// </summary>
        DefaultMath = 0,
        /// <summary>
        /// </summary>
		[Obsolete("deprecated, same effect as using CUBLAS_COMPUTE_32F_FAST_16F, will be removed in a future release")]
        TensorOpMath = 1,
        /// <summary>
        /// same as using matching _PEDANTIC compute type when using cublas routine calls or cublasEx() calls with cudaDataType as compute type
        /// </summary>
        PedanticMath = 2,
        /// <summary>
        /// allow accelerating single precision routines using TF32 tensor cores
        /// </summary>
        TF32TensorOpMath = 3,
        /// <summary>
        /// flag to force any reductons to use the accumulator type and not output type in case of mixed precision routines with lower size output type
        /// </summary>
        DisallowReducedPrecisionReduction = 16
    }

    /// <summary>
    /// Enum for compute type<para/>
    /// - default types provide best available performance using all available hardware features
    ///   and guarantee internal storage precision with at least the same precision and range;<para/>
    /// - _PEDANTIC types ensure standard arithmetic and exact specified internal storage format;<para/>
    /// - _FAST types allow for some loss of precision to enable higher throughput arithmetic.
    /// </summary>
    public enum ComputeType
    {
        /// <summary>
        /// half - default
        /// </summary>
        Compute16F = 64,
        /// <summary>
        /// half - pedantic
        /// </summary>
        Compute16FPedantic = 65,
        /// <summary>
        /// float - default
        /// </summary>
        Compute32F = 68,
        /// <summary>
        /// float - pedantic
        /// </summary>
        Compute32FPedantic = 69,
        /// <summary>
        /// float - fast, allows down-converting inputs to half or TF32
        /// </summary>
        Compute32FFast16F = 74,
        /// <summary>
        /// float - fast, allows down-converting inputs to bfloat16 or TF32
        /// </summary>
        Compute32FFast16BF = 75,
        /// <summary>
        /// float - fast, allows down-converting inputs to TF32
        /// </summary>
        Compute32FFastTF32 = 77,
        /// <summary>
        /// double - default
        /// </summary>
        Compute64F = 70,
        /// <summary>
        /// double - pedantic
        /// </summary>
        Compute64FPedantic = 71,
        /// <summary>
        /// signed 32-bit int - default
        /// </summary>
        Compute32I = 72,
        /// <summary>
        /// signed 32-bit int - pedantic
        /// </summary>
        Compute32IPedantic = 73,
    }

    /// <summary>
    /// The cublasDataType_t type is an enumerant to specify the data precision. It is used
    /// when the data reference does not carry the type itself (e.g void *).
    /// To mimic the typedef in cublas_api.h, we redefine the enum identically to cudaDataType
    /// </summary>
    public enum DataType
    {
        ///// <summary>
        ///// the data type is 32-bit floating-point
        ///// </summary>
        //Float = 0,
        ///// <summary>
        ///// the data type is 64-bit floating-point
        ///// </summary>
        //Double = 1,
        ///// <summary>
        ///// the data type is 16-bit floating-point
        ///// </summary>
        //Half = 2,
        ///// <summary>
        ///// the data type is 8-bit signed integer
        ///// </summary>
        //Int8 = 3

        /// <summary>
        /// 16 bit real 
        /// </summary>
        CUDA_R_16F = 2,

        /// <summary>
        /// 16 bit complex
        /// </summary>
        CUDA_C_16F = 6,

        /// <summary>
        /// 32 bit real
        /// </summary>
        CUDA_R_32F = 0,

        /// <summary>
        /// 32 bit complex
        /// </summary>
        CUDA_C_32F = 4,

        /// <summary>
        /// 64 bit real
        /// </summary>
        CUDA_R_64F = 1,

        /// <summary>
        /// 64 bit complex
        /// </summary>
        CUDA_C_64F = 5,

        /// <summary>
        /// 8 bit real as a signed integer 
        /// </summary>
        CUDA_R_8I = 3,

        /// <summary>
        /// 8 bit complex as a pair of signed integers
        /// </summary>
        CUDA_C_8I = 7,

        /// <summary>
        /// 8 bit real as a signed integer 
        /// </summary>
        CUDA_R_8U = 8,

        /// <summary>
        /// 8 bit complex as a pair of signed integers
        /// </summary>
        CUDA_C_8U = 9
    }

    /// <summary>
    /// half precission floating point
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. --> we want it to be the same as in C/C++/Cuda code
    public struct half
#pragma warning restore CS8981
    {
        ushort x;

        /// <summary>
        /// 
        /// </summary>
        public half(float f)
        {
            x = __float2half(f).x;
        }

        /// <summary>
        /// 
        /// </summary>
        public half(double d)
        {
            x = __double2half(d).x;
        }

        /// <summary>
        /// 
        /// </summary>
        public half(half h16)
        {
            x = h16.x;
        }

        private static ushort __internal_float2half(float f, ref uint sign, ref uint remainder)
        {
            float[] ftemp = new float[] { f };
            uint[] x = new uint[1];
            uint u = 0;
            uint result = 0;
            System.Buffer.BlockCopy(ftemp, 0, x, 0, sizeof(float));

            u = (x[0] & 0x7fffffffU);
            sign = ((x[0] >> 16) & 0x8000U);
            // NaN/+Inf/-Inf
            if (u >= 0x7f800000U)
            {
                remainder = 0U;
                result = ((u == 0x7f800000U) ? (sign | 0x7c00U) : 0x7fffU);
            }
            else if (u > 0x477fefffU)
            { // Overflows
                remainder = 0x80000000U;
                result = (sign | 0x7bffU);
            }
            else if (u >= 0x38800000U)
            { // Normal numbers
                remainder = u << 19;
                u -= 0x38000000U;
                result = (sign | (u >> 13));
            }
            else if (u < 0x33000001U)
            { // +0/-0
                remainder = u;
                result = sign;
            }
            else
            { // Denormal numbers
                uint exponent = u >> 23;
                uint shift = 0x7eU - exponent;
                uint mantissa = (u & 0x7fffffU);
                mantissa |= 0x800000U;
                remainder = mantissa << (32 - (int)shift);
                result = (sign | (mantissa >> (int)shift));
            }
            return (ushort)(result);
        }

        private static half __double2half(double x)
        {
            // Perform rounding to 11 bits of precision, convert value
            // to float and call existing float to half conversion.
            // By pre-rounding to 11 bits we avoid additional rounding
            // in float to half conversion.
            ulong absx;
            ulong[] ux = new ulong[1];
            double[] xa = new double[] { x };
            System.Buffer.BlockCopy(xa, 0, ux, 0, sizeof(double));

            absx = (ux[0] & 0x7fffffffffffffffUL);
            if ((absx >= 0x40f0000000000000UL) || (absx <= 0x3e60000000000000UL))
            {
                // |x| >= 2^16 or NaN or |x| <= 2^(-25)
                // double-rounding is not a problem
                return __float2half((float)x);
            }

            // here 2^(-25) < |x| < 2^16
            // prepare shifter value such that x + shifter
            // done in double precision performs round-to-nearest-even
            // and (x + shifter) - shifter results in x rounded to
            // 11 bits of precision. Shifter needs to have exponent of
            // x plus 53 - 11 = 42 and a leading bit in mantissa to guard
            // against negative values.
            // So need to have |x| capped to avoid overflow in exponent.
            // For inputs that are smaller than half precision minnorm
            // we prepare fixed shifter exponent.
            ulong shifterBits = ux[0] & 0x7ff0000000000000UL;
            if (absx >= 0x3f10000000000000UL)
            {   // |x| >= 2^(-14)
                // add 42 to exponent bits
                shifterBits += 42ul << 52;
            }

            else
            {   // 2^(-25) < |x| < 2^(-14), potentially results in denormal
                // set exponent bits to 42 - 14 + bias
                shifterBits = ((42ul - 14 + 1023) << 52);
            }
            // set leading mantissa bit to protect against negative inputs
            shifterBits |= 1ul << 51;
            ulong[] shifterBitsArr = new ulong[] { shifterBits };
            double[] shifter = new double[1];

            System.Buffer.BlockCopy(shifterBitsArr, 0, shifter, 0, sizeof(double));

            double xShiftRound = x + shifter[0];
            double[] xShiftRoundArr = new double[] { xShiftRound };

            // Prevent the compiler from optimizing away x + shifter - shifter
            // by doing intermediate memcopy and harmless bitwize operation
            ulong[] xShiftRoundBits = new ulong[1];

            System.Buffer.BlockCopy(xShiftRoundArr, 0, xShiftRoundBits, 0, sizeof(double));

            // the value is positive, so this operation doesn't change anything
            xShiftRoundBits[0] &= 0x7ffffffffffffffful;

            System.Buffer.BlockCopy(xShiftRoundBits, 0, xShiftRoundArr, 0, sizeof(double));

            double xRounded = xShiftRound - shifter[0];
            float xRndFlt = (float)xRounded;
            half res = __float2half(xRndFlt);
            return res;
        }

        private static half __float2half(float a)
        {
            half r = new half();
            uint sign = 0;
            uint remainder = 0;
            r.x = __internal_float2half(a, ref sign, ref remainder);
            if ((remainder > 0x80000000U) || ((remainder == 0x80000000U) && ((r.x & 0x1U) != 0U)))
            {
                r.x++;
            }

            return r;
        }

        /// <summary>
        /// 
        /// </summary>
        public override string ToString()
        {
            return x.ToString();
        }
    }

    /// <summary>
    /// Cublas logging
    /// </summary>
    public delegate void cublasLogCallback([MarshalAs(UnmanagedType.LPStr)] string msg);

    

    /// <summary>
    /// Opaque structure holding the matrix descriptor
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct cusparseMatDescr
    {
        /// <summary>
        /// 
        /// </summary>
        public IntPtr Handle;
    }

    /// <summary>
    /// CUDA driver API initialization flags
    /// </summary>
    [Flags]
    public enum CUInitializationFlags : uint
    {
        /// <summary>
        /// Currently no initialization flags are defined.
        /// </summary>
        None = 0
    }

    /// <summary>
    /// External semaphore wait parameters
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct CudaExternalSemaphoreWaitParams
    {
        /// <summary>
        /// Parameters for fence objects
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        public struct Parameters
        {
            /// <summary>
            /// Value of fence to be waited on
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct Fence
            {
                /// <summary>
                /// Value of fence to be waited on
                /// </summary>
                public ulong value;
            }
            /// <summary/>
            [FieldOffset(0)]
            public Fence fence;
            /// <summary>
            /// Pointer to NvSciSyncFence. Valid if CUexternalSemaphoreHandleType
            /// is of type CU_EXTERNAL_SEMAPHORE_HANDLE_TYPE_NVSCISYNC.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct NvSciSync
            {
                /// <summary>
                /// 
                /// </summary>
                public IntPtr fence;
            }
            /// <summary/>
            [FieldOffset(8)]
            public NvSciSync nvSciSync;
            /// <summary>
            /// Parameters for keyed mutex objects
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct KeyedMutex
            {
                /// <summary>
                /// Value of key to acquire the mutex with
                /// </summary>
                public ulong key;
                /// <summary>
                /// Timeout in milliseconds to wait to acquire the mutex
                /// </summary>
                public uint timeoutMs;
            }
            /// <summary/>
            [FieldOffset(16)]
            public KeyedMutex keyedMutex;
            [FieldOffset(20)]
            private uint reserved;
        }
        /// <summary/>
        [FieldOffset(0)]
        public Parameters parameters;
        /// <summary>
        /// Flags reserved for the future. Must be zero.
        /// </summary>
        [FieldOffset(72)]
        public uint flags;
        [FieldOffset(136)]
        uint reserved;
    }

    /// <summary>
    /// Memory allocation node parameters
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct CudaMemAllocNodeParamsInternal
    {
        /// <summary>
        /// in: location where the allocation should reside (specified in ::location).
        /// ::handleTypes must be::CU_MEM_HANDLE_TYPE_NONE.IPC is not supported.
        /// </summary>
        public CUmemPoolProps poolProps;
        /// <summary>
        /// in: array of memory access descriptors. Used to describe peer GPU access
        /// </summary>
        public IntPtr accessDescs;
        /// <summary>
        /// in: number of memory access descriptors.  Must not exceed the number of GPUs.
        /// </summary>
        public SizeT accessDescCount;
        /// <summary>
        /// in: size in bytes of the requested allocation
        /// </summary>
        public SizeT bytesize;
        /// <summary>
        /// out: address of the allocation returned by CUDA
        /// </summary>
        public CUdeviceptr dptr;
    }

    internal struct CUlaunchConfigInternal
    {
        /// <summary>
        /// Width of grid in blocks
        /// </summary>
        public uint gridDimX;
        /// <summary>
        /// Height of grid in blocks
        /// </summary>
        public uint gridDimY;
        /// <summary>
        /// Depth of grid in blocks
        /// </summary>
        public uint gridDimZ;
        /// <summary>
        /// X dimension of each thread block
        /// </summary>
        public uint blockDimX;
        /// <summary>
        /// Y dimension of each thread block
        /// </summary>
        public uint blockDimY;
        /// <summary>
        /// Z dimension of each thread block
        /// </summary>
        public uint blockDimZ;
        /// <summary>
        /// Dynamic shared-memory size per thread block in bytes
        /// </summary>
        public uint sharedMemBytes;
        /// <summary>
        /// Stream identifier
        /// </summary>
        public CUstream hStream;
        /// <summary>
        /// nullable if numAttrs == 0
        /// </summary>
        public IntPtr attrs;
        /// <summary>
        /// number of attributes populated in attrs
        /// </summary>
        public uint numAttrs;
    }

    /// <summary>
    /// CUDA graph node
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CUgraphNode
    {
        /// <summary>
        /// 
        /// </summary>
        public IntPtr Pointer;

        #region Properties
        /// <summary>
        /// Returns the type of the Node
        /// </summary>
        public CUgraphNodeType Type
        {
            get
            {
                CUgraphNodeType type = new CUgraphNodeType();
                CUResult res = DriverAPINativeMethods.GraphManagment.cuGraphNodeGetType(this, ref type);
                Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuGraphNodeGetType", res));
                if (res != CUResult.Success) throw new CudaException(res);

                return type;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Sets the parameters of host node nodeParams.
        /// </summary>
        /// <param name="nodeParams"></param>
        public void SetParameters(CudaHostNodeParams nodeParams)
        {
            CUResult res = DriverAPINativeMethods.GraphManagment.cuGraphHostNodeSetParams(this, ref nodeParams);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuGraphHostNodeSetParams", res));
            if (res != CUResult.Success) throw new CudaException(res);
        }

        /// <summary>
        /// Sets the parameters of kernel node nodeParams.
        /// </summary>
        /// <param name="nodeParams"></param>
        public void SetParameters(CudaKernelNodeParams nodeParams)
        {
            CUResult res = DriverAPINativeMethods.GraphManagment.cuGraphKernelNodeSetParams(this, ref nodeParams);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuGraphKernelNodeSetParams", res));
            if (res != CUResult.Success) throw new CudaException(res);
        }

        /// <summary>
        /// Sets the parameters of memcpy node nodeParams.
        /// </summary>
        /// <param name="nodeParams"></param>
        public void SetParameters(CUDAMemCpy3D nodeParams)
        {
            CUResult res = DriverAPINativeMethods.GraphManagment.cuGraphMemcpyNodeSetParams(this, ref nodeParams);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuGraphMemcpyNodeSetParams", res));
            if (res != CUResult.Success) throw new CudaException(res);
        }

        /// <summary>
        /// Sets the parameters of memset node nodeParams.
        /// </summary>
        /// <param name="nodeParams"></param>
        public void SetParameters(CudaMemsetNodeParams nodeParams)
        {
            CUResult res = DriverAPINativeMethods.GraphManagment.cuGraphMemsetNodeSetParams(this, ref nodeParams);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuGraphMemsetNodeSetParams", res));
            if (res != CUResult.Success) throw new CudaException(res);
        }

        /// <summary>
        /// Sets an external semaphore signal node's parameters.
        /// </summary>
        /// <param name="nodeParams"></param>
        public void SetParameters(CudaExtSemSignalNodeParams nodeParams)
        {
            CUResult res = DriverAPINativeMethods.GraphManagment.cuGraphExternalSemaphoresSignalNodeSetParams(this, nodeParams);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuGraphExternalSemaphoresSignalNodeSetParams", res));
            if (res != CUResult.Success) throw new CudaException(res);
        }

        /// <summary>
        /// Sets an external semaphore wait node's parameters.
        /// </summary>
        /// <param name="nodeParams"></param>
        public void SetParameters(CudaExtSemWaitNodeParams nodeParams)
        {
            CUResult res = DriverAPINativeMethods.GraphManagment.cuGraphExternalSemaphoresWaitNodeSetParams(this, nodeParams);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuGraphExternalSemaphoresWaitNodeSetParams", res));
            if (res != CUResult.Success) throw new CudaException(res);
        }

        /// <summary>
        /// Sets a batch mem op node's parameters
        /// </summary>
        /// <param name="nodeParams"></param>
        public void SetParameters(CudaBatchMemOpNodeParams nodeParams)
        {
            CUResult res = DriverAPINativeMethods.GraphManagment.cuGraphBatchMemOpNodeSetParams(this, ref nodeParams);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuGraphBatchMemOpNodeSetParams", res));
            if (res != CUResult.Success) throw new CudaException(res);
        }

        /// <summary>
        /// Gets the parameters of host node.
        /// </summary>
        /// <param name="nodeParams"></param>
        public void GetParameters(ref CudaHostNodeParams nodeParams)
        {
            CUResult res = DriverAPINativeMethods.GraphManagment.cuGraphHostNodeGetParams(this, ref nodeParams);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuGraphHostNodeGetParams", res));
            if (res != CUResult.Success) throw new CudaException(res);
        }

        /// <summary>
        /// Gets the parameters of kernel node.
        /// </summary>
        /// <param name="nodeParams"></param>
        public void GetParameters(ref CudaKernelNodeParams nodeParams)
        {
            CUResult res = DriverAPINativeMethods.GraphManagment.cuGraphKernelNodeGetParams(this, ref nodeParams);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuGraphKernelNodeGetParams", res));
            if (res != CUResult.Success) throw new CudaException(res);
        }

        /// <summary>
        /// Gets the parameters of memcpy node.
        /// </summary>
        /// <param name="nodeParams"></param>
        public void GetParameters(ref CUDAMemCpy3D nodeParams)
        {
            CUResult res = DriverAPINativeMethods.GraphManagment.cuGraphMemcpyNodeGetParams(this, ref nodeParams);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuGraphMemcpyNodeGetParams", res));
            if (res != CUResult.Success) throw new CudaException(res);
        }

        /// <summary>
        /// Gets the parameters of memset node.
        /// </summary>
        /// <param name="nodeParams"></param>
        public void GetParameters(ref CudaMemsetNodeParams nodeParams)
        {
            CUResult res = DriverAPINativeMethods.GraphManagment.cuGraphMemsetNodeGetParams(this, ref nodeParams);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuGraphMemsetNodeGetParams", res));
            if (res != CUResult.Success) throw new CudaException(res);
        }

        /// <summary>
        /// Gets the external semaphore signal node's parameters.
        /// </summary>
        /// <param name="nodeParams"></param>
        public void GetParameters(CudaExtSemSignalNodeParams nodeParams)
        {
            CUResult res = DriverAPINativeMethods.GraphManagment.cuGraphExternalSemaphoresSignalNodeGetParams(this, nodeParams);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuGraphExternalSemaphoresSignalNodeGetParams", res));
            if (res != CUResult.Success) throw new CudaException(res);
        }

        /// <summary>
        /// Gets the external semaphore wait node's parameters.
        /// </summary>
        /// <param name="nodeParams"></param>
        public void GetParameters(CudaExtSemWaitNodeParams nodeParams)
        {
            CUResult res = DriverAPINativeMethods.GraphManagment.cuGraphExternalSemaphoresWaitNodeGetParams(this, nodeParams);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuGraphExternalSemaphoresWaitNodeGetParams", res));
            if (res != CUResult.Success) throw new CudaException(res);
        }

        /// <summary>
        /// Returns a memory alloc node's parameters
        /// </summary>
        /// <param name="nodeParams"></param>
        public void GetParameters(ref CudaMemAllocNodeParams nodeParams)
        {
            CUResult res = DriverAPINativeMethods.GraphManagment.cuGraphMemAllocNodeGetParams(this, ref nodeParams);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuGraphMemAllocNodeGetParams", res));
            if (res != CUResult.Success) throw new CudaException(res);
        }

        /// <summary>
        /// Returns a memory free node's parameters
        /// </summary>
        /// <param name="nodeParams"></param>
        public void GetParameters(ref CUdeviceptr nodeParams)
        {
            CUResult res = DriverAPINativeMethods.GraphManagment.cuGraphMemFreeNodeGetParams(this, ref nodeParams);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuGraphMemFreeNodeGetParams", res));
            if (res != CUResult.Success) throw new CudaException(res);
        }

        /// <summary>
        /// Returns a batch mem op node's parameters
        /// </summary>
        /// <param name="nodeParams"></param>
        public void GetParameters(ref CudaBatchMemOpNodeParams nodeParams)
        {
            CUResult res = DriverAPINativeMethods.GraphManagment.cuGraphBatchMemOpNodeGetParams(this, ref nodeParams);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuGraphBatchMemOpNodeGetParams", res));
            if (res != CUResult.Success) throw new CudaException(res);
        }

        /// <summary>
        /// Returns a node's dependencies.
        /// </summary>
        /// <returns></returns>
        public CUgraphNode[] GetDependencies()
        {
            SizeT numNodes = new SizeT();
            CUResult res = DriverAPINativeMethods.GraphManagment.cuGraphNodeGetDependencies(this, null, ref numNodes);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuGraphNodeGetDependencies", res));
            if (res != CUResult.Success) throw new CudaException(res);

            if (numNodes > 0)
            {
                CUgraphNode[] nodes = new CUgraphNode[numNodes];
                res = DriverAPINativeMethods.GraphManagment.cuGraphNodeGetDependencies(this, nodes, ref numNodes);
                Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuGraphNodeGetDependencies", res));
                if (res != CUResult.Success) throw new CudaException(res);

                return nodes;
            }

            return null;
        }

        /// <summary>
        /// Returns a node's dependent nodes
        /// </summary>
        public CUgraphNode[] GetDependentNodes()
        {
            SizeT numNodes = new SizeT();
            CUResult res = DriverAPINativeMethods.GraphManagment.cuGraphNodeGetDependentNodes(this, null, ref numNodes);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuGraphNodeGetDependentNodes", res));
            if (res != CUResult.Success) throw new CudaException(res);

            if (numNodes > 0)
            {
                CUgraphNode[] nodes = new CUgraphNode[numNodes];
                res = DriverAPINativeMethods.GraphManagment.cuGraphNodeGetDependentNodes(this, nodes, ref numNodes);
                Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuGraphNodeGetDependentNodes", res));
                if (res != CUResult.Success) throw new CudaException(res);

                return nodes;
            }

            return null;
        }


        /// <summary>
        /// Copies attributes from source node to destination node.<para/>
        /// Copies attributes from source node \p src to destination node \p dst. Both node must have the same context.
        /// </summary>
        /// <param name="dst">Destination node</param>
        public void cuGraphKernelNodeCopyAttributes(CUgraphNode dst)
        {
            CUResult res = DriverAPINativeMethods.GraphManagment.cuGraphKernelNodeCopyAttributes(dst, this);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuGraphKernelNodeCopyAttributes", res));
            if (res != CUResult.Success) throw new CudaException(res);
        }

        /// <summary>
        /// Queries node attribute.<para/>
        /// Queries attribute \p attr from node \p hNode and stores it in corresponding member of \p value_out.
        /// </summary>
        /// <param name="attr"></param>
        public CUkernelNodeAttrValue GetAttribute(CUkernelNodeAttrID attr)
        {
            CUkernelNodeAttrValue value = new CUkernelNodeAttrValue();
            CUResult res = DriverAPINativeMethods.GraphManagment.cuGraphKernelNodeGetAttribute(this, attr, ref value);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuGraphKernelNodeGetAttribute", res));
            if (res != CUResult.Success) throw new CudaException(res);
            return value;
        }

        /// <summary>
        /// Sets node attribute.<para/>
        /// Sets attribute \p attr on node \p hNode from corresponding attribute of value.
        /// </summary>
        /// <param name="attr"></param>
        /// <param name="value"></param>
        public void SetAttribute(CUkernelNodeAttrID attr, CUkernelNodeAttrValue value)
        {
            CUResult res = DriverAPINativeMethods.GraphManagment.cuGraphKernelNodeSetAttribute(this, attr, ref value);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuGraphKernelNodeSetAttribute", res));
            if (res != CUResult.Success) throw new CudaException(res);
        }


        /// <summary>
        /// Returns the event associated with an event record node
        /// </summary>
        public CudaEvent GetRecordEvent()
        {
            CUevent event_out = new CUevent();
            CUResult res = DriverAPINativeMethods.GraphManagment.cuGraphEventRecordNodeGetEvent(this, ref event_out);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuGraphEventRecordNodeGetEvent", res));
            if (res != CUResult.Success) throw new CudaException(res);
            return new CudaEvent(event_out);
        }

        /// <summary>
        /// Sets an event record node's event
        /// </summary>
        public void SetRecordEvent(CudaEvent event_)
        {
            CUResult res = DriverAPINativeMethods.GraphManagment.cuGraphEventRecordNodeSetEvent(this, event_.Event);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuGraphEventRecordNodeSetEvent", res));
            if (res != CUResult.Success) throw new CudaException(res);
        }


        /// <summary>
        /// Returns the event associated with an event wait node
        /// </summary>
        public CudaEvent GetWaitEvent()
        {
            CUevent event_out = new CUevent();
            CUResult res = DriverAPINativeMethods.GraphManagment.cuGraphEventWaitNodeGetEvent(this, ref event_out);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuGraphEventWaitNodeGetEvent", res));
            if (res != CUResult.Success) throw new CudaException(res);
            return new CudaEvent(event_out);
        }

        /// <summary>
        /// Sets an event wait node's event
        /// </summary>
        public void SetWaitEvent(CudaEvent event_)
        {
            CUResult res = DriverAPINativeMethods.GraphManagment.cuGraphEventWaitNodeSetEvent(this, event_.Event);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuGraphEventWaitNodeSetEvent", res));
            if (res != CUResult.Success) throw new CudaException(res);
        }

        #endregion
    }

    /// <summary>
    /// CudaBatchMemOpNodeParams
    /// </summary>
    public class CudaBatchMemOpNodeParams
    {
        /// <summary/>
        public CUcontext ctx;
        /// <summary/>
        public CUstreamBatchMemOpParams[] paramArray;
        /// <summary/>
        public uint flags;
    }

    /// <summary/>
    [StructLayout(LayoutKind.Explicit)]
    public struct CUstreamBatchMemOpParams
    {
        /// <summary/>
        [FieldOffset(0)]
        public CUstreamBatchMemOpType operation;
        /// <summary/>
        [FieldOffset(0)]
        public CUstreamMemOpWaitValueParams waitValue;
        /// <summary/>
        [FieldOffset(0)]
        public CUstreamMemOpWriteValueParams writeValue;
        /// <summary/>
        [FieldOffset(0)]
        public CUstreamMemOpFlushRemoteWritesParams flushRemoteWrites;
        /// <summary/>
        [FieldOffset(0)]
        public CUstreamMemOpMemoryBarrierParams memoryBarrier;
        //In cuda header, an ulong[6] fixes the union size to 48 bytes, we
        //achieve the same in C# if we set at offset 40 an simple ulong
        [FieldOffset(5 * 8)]
        ulong pad;
    }

    /// <summary>
    /// Operations for ::cuStreamBatchMemOp
    /// </summary>
    public enum CUstreamBatchMemOpType
    {
        /// <summary>
        /// Represents a ::cuStreamWaitValue32 operation
        /// </summary>
        WaitValue32 = 1,
        /// <summary>
        /// Represents a ::cuStreamWriteValue32 operation
        /// </summary>
        WriteValue32 = 2,
        /// <summary>
        /// Represents a ::cuStreamWaitValue64 operation
        /// </summary>
        WaitValue64 = 4,
        /// <summary>
        /// Represents a ::cuStreamWriteValue64 operation
        /// </summary>
        WriteValue64 = 5,
        /// <summary>
        /// Insert a memory barrier of the specified type
        /// </summary>
        Barrier = 6,
        /// <summary>
        /// This has the same effect as ::CU_STREAM_WAIT_VALUE_FLUSH, but as a standalone operation.
        /// </summary>
        FlushRemoteWrites = 3
    }

    /// <summary/>
    [StructLayout(LayoutKind.Sequential)]
    public struct CUstreamMemOpWaitValueParams
    {
        /// <summary/>
        public CUstreamBatchMemOpType operation;
        /// <summary/>
        public CUdeviceptr address;
        /// <summary/>
        public cuuint3264_union value;
        /// <summary/>
        public uint flags;
        /// <summary>
        /// For driver internal use. Initial value is unimportant.
        /// </summary>
        public CUdeviceptr alias;
    }

    /// <summary>
    /// Per-operation parameters for ::cuStreamBatchMemOp
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct cuuint3264_union
    {
        /// <summary/>
        [FieldOffset(0)]
        public uint value;
        /// <summary/>
        [FieldOffset(0)]
        public ulong value64;
    }

    /// <summary/>
    [StructLayout(LayoutKind.Sequential)]
    public struct CUstreamMemOpWriteValueParams
    {
        /// <summary/>
        public CUstreamBatchMemOpType operation;
        /// <summary/>
        public CUdeviceptr address;
        /// <summary/>
        public cuuint3264_union value;
        /// <summary/>
        public uint flags;
        /// <summary>
        /// For driver internal use. Initial value is unimportant.
        /// </summary>
        public CUdeviceptr alias;
    }

    /// <summary>
    /// Memory allocation node parameters
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CudaMemAllocNodeParams
    {
        /// <summary>
        /// in: location where the allocation should reside (specified in ::location).
        /// ::handleTypes must be::CU_MEM_HANDLE_TYPE_NONE.IPC is not supported.
        /// </summary>
        public CUmemPoolProps poolProps;
        /// <summary>
        /// in: array of memory access descriptors. Used to describe peer GPU access
        /// </summary>
        public CUmemAccessDesc[] accessDescs;
        /// <summary>
        /// in: size in bytes of the requested allocation
        /// </summary>
        public SizeT bytesize;
        /// <summary>
        /// out: address of the allocation returned by CUDA
        /// </summary>
        public CUdeviceptr dptr;
    }

    /// <summary>
    /// Host node parameters
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CudaHostNodeParams
    {
        /// <summary>
        /// The function to call when the node executes
        /// </summary>
        public CUhostFn fn;
        /// <summary>
        /// Argument to pass to the function
        /// </summary>
        public IntPtr userData;
    }

    /// <summary>
    /// CUDA host function
    /// </summary>
    /// <param name="userData">Argument value passed to the function</param>
    public delegate void CUhostFn(IntPtr userData);

    /// <summary/>
    [StructLayout(LayoutKind.Sequential)]
    public struct CUstreamMemOpFlushRemoteWritesParams
    {
        /// <summary/>
        public CUstreamBatchMemOpType operation;
        /// <summary/>
        public uint flags;
    }

    /// <summary/>
    [StructLayout(LayoutKind.Sequential)]
    public struct CUstreamMemOpMemoryBarrierParams
    {
        /// <summary/>
        public CUstreamBatchMemOpType operation;
        /// <summary/>
        public uint flags;
    }

    /// <summary>
    /// Semaphore wait node parameters
    /// </summary>
    public class CudaExtSemWaitNodeParams
    {
        /// <summary/>
        public CUexternalSemaphore[] extSemArray;
        /// <summary/>
        public CudaExternalSemaphoreWaitParams[] paramsArray;
    }

    /// <summary>
    /// CUDA external semaphore
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CUexternalSemaphore
    {
        /// <summary>
        /// 
        /// </summary>
        public IntPtr Pointer;
    }

    /// <summary>
    /// Semaphore signal node parameters
    /// </summary>
    public class CudaExtSemSignalNodeParams
    {
        /// <summary/>
        public CUexternalSemaphore[] extSemArray;
        /// <summary/>
        public CudaExternalSemaphoreSignalParams[] paramsArray;
    }

    /// <summary>
    /// External semaphore signal parameters
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct CudaExternalSemaphoreSignalParams
    {
        /// <summary>
        /// Parameters for fence objects
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        public struct Parameters
        {
            /// <summary>
            /// Value of fence to be signaled
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct Fence
            {
                /// <summary>
                /// Value of fence to be signaled
                /// </summary>
                public ulong value;
            }
            /// <summary/>
            [FieldOffset(0)]
            public Fence fence;
            /// <summary>
            /// Pointer to NvSciSyncFence. Valid if CUexternalSemaphoreHandleType
            /// is of type CU_EXTERNAL_SEMAPHORE_HANDLE_TYPE_NVSCISYNC.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct NvSciSync
            {
                /// <summary>
                /// 
                /// </summary>
                public IntPtr fence;
            }
            /// <summary/>
            [FieldOffset(8)]
            public NvSciSync nvSciSync;

            /// <summary>
            /// Parameters for keyed mutex objects
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct KeyedMutex
            {
                /// <summary>
                /// Value of key to acquire the mutex with
                /// </summary>
                public ulong key;
            }
            /// <summary/>
            [FieldOffset(16)]
            public KeyedMutex keyedMutex;

            /// <summary/>
            [FieldOffset(68)] //params.reserved[9];
            private uint reserved;
        }
        /// <summary/>
        [FieldOffset(0)]
        public Parameters parameters;
        /// <summary>
        /// Flags reserved for the future. Must be zero.
        /// </summary>
        [FieldOffset(72)]
        public uint flags;
        [FieldOffset(136)] //offset of reserved[15]
        uint reserved;
    }

    /// <summary>
    /// 3D memory copy parameters
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CUDAMemCpy3D
    {
        /// <summary>
        /// Source X in bytes
        /// </summary>
        public SizeT srcXInBytes;

        /// <summary>
        /// Source Y
        /// </summary>
        public SizeT srcY;

        /// <summary>
        /// Source Z
        /// </summary>
        public SizeT srcZ;

        /// <summary>
        /// Source LOD
        /// </summary>
        public SizeT srcLOD;

        /// <summary>
        /// Source memory type (host, device, array)
        /// </summary>
        public CUMemoryType srcMemoryType;


        /// <summary>
        /// Source host pointer
        /// </summary>
        public IntPtr srcHost;


        /// <summary>
        /// Source device pointer
        /// </summary>
        public CUdeviceptr srcDevice;

        /// <summary>
        /// Source array reference
        /// </summary>
        public CUarray srcArray;

        /// <summary>
        /// Must be NULL
        /// </summary>
        public IntPtr reserved0;

        /// <summary>
        /// Source pitch (ignored when src is array)
        /// </summary>
        public SizeT srcPitch;

        /// <summary>
        /// Source height (ignored when src is array; may be 0 if Depth==1)
        /// </summary>
        public SizeT srcHeight;

        /// <summary>
        /// Destination X in bytes
        /// </summary>
        public SizeT dstXInBytes;

        /// <summary>
        /// Destination Y
        /// </summary>
        public SizeT dstY;

        /// <summary>
        /// Destination Z
        /// </summary>
        public SizeT dstZ;

        /// <summary>
        /// Destination LOD
        /// </summary>
        public SizeT dstLOD;

        /// <summary>
        /// Destination memory type (host, device, array)
        /// </summary>
        public CUMemoryType dstMemoryType;

        /// <summary>
        /// Destination host pointer
        /// </summary>
        public IntPtr dstHost;

        /// <summary>
        /// Destination device pointer
        /// </summary>
        public CUdeviceptr dstDevice;

        /// <summary>
        /// Destination array reference
        /// </summary>
        public CUarray dstArray;

        /// <summary>
        /// Must be NULL
        /// </summary>
        public IntPtr reserved1;

        /// <summary>
        /// Destination pitch (ignored when dst is array)
        /// </summary>
        public SizeT dstPitch;

        /// <summary>
        /// Destination height (ignored when dst is array; may be 0 if Depth==1)
        /// </summary>
        public SizeT dstHeight;

        /// <summary>
        /// Width of 3D memory copy in bytes
        /// </summary>
        public SizeT WidthInBytes;

        /// <summary>
        /// Height of 3D memory copy
        /// </summary>
        public SizeT Height;

        /// <summary>
        /// Depth of 3D memory copy
        /// </summary>
        public SizeT Depth;
    }

    /// <summary>
    /// Memset node parameters
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CudaMemsetNodeParams
    {
        /// <summary>
        /// Destination device pointer
        /// </summary>
        public CUdeviceptr dst;
        /// <summary>
        /// Pitch of destination device pointer. Unused if height is 1
        /// </summary>
        public SizeT pitch;
        /// <summary>
        /// Value to be set
        /// </summary>
        public uint value;
        /// <summary>
        /// Size of each element in bytes. Must be 1, 2, or 4.
        /// </summary>
        public uint elementSize;
        /// <summary>
        /// Width of the row in elements
        /// </summary>
        public SizeT width;
        /// <summary>
        /// Number of rows
        /// </summary>
        public SizeT height;

        /// <summary>
        /// Initialieses the struct
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="deviceVariable"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static CudaMemsetNodeParams init<T>(CudaDeviceVariable<T> deviceVariable, uint value) where T : struct
        {
            CudaMemsetNodeParams para = new CudaMemsetNodeParams();
            para.dst = deviceVariable.DevicePointer;
            para.pitch = deviceVariable.SizeInBytes;
            para.value = value;
            para.elementSize = deviceVariable.TypeSize;
            para.width = deviceVariable.SizeInBytes;
            para.height = 1;

            return para;
        }
    }

    /// <summary>
    /// Graph node types
    /// </summary>
    public enum CUgraphNodeType
    {
        /// <summary>
        /// GPU kernel node
        /// </summary>
        Kernel = 0,
        /// <summary>
        /// Memcpy node
        /// </summary>
        Memcpy = 1,
        /// <summary>
        /// Memset node
        /// </summary>
        Memset = 2,
        /// <summary>
        /// Host (executable) node
        /// </summary>
        Host = 3,
        /// <summary>
        /// Node which executes an embedded graph
        /// </summary>
        Graph = 4,
        /// <summary>
        /// Empty (no-op) node
        /// </summary>
        Empty = 5,
        /// <summary>
        /// External event wait node
        /// </summary>
        WaitEvent = 6,
        /// <summary>
        /// External event record node
        /// </summary>
        EventRecord = 7,
        /// <summary>
        /// External semaphore signal node
        /// </summary>
        ExtSemasSignal = 8,
        /// <summary>
        /// External semaphore wait node
        /// </summary>
        ExtSemasWait = 9,
        /// <summary>
        /// Memory Allocation Node
        /// </summary>
        MemAlloc = 10,
        /// <summary>
        /// Memory Free Node
        /// </summary>
        MemFree = 11,
        /// <summary>
        /// Batch MemOp Node
        /// </summary>
        BatchMemOp = 12
    }

    /// <summary>
    /// GPU kernel node parameters
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CudaKernelNodeParams
    {
        /// <summary>
        /// Kernel to launch
        /// </summary>
        public CUfunction func;
        /// <summary>
        /// Width of grid in blocks
        /// </summary>
        public uint gridDimX;
        /// <summary>
        /// Height of grid in blocks
        /// </summary>
        public uint gridDimY;
        /// <summary>
        /// Depth of grid in blocks
        /// </summary>
        public uint gridDimZ;
        /// <summary>
        /// X dimension of each thread block
        /// </summary>
        public uint blockDimX;
        /// <summary>
        /// Y dimension of each thread block
        /// </summary>
        public uint blockDimY;
        /// <summary>
        /// Z dimension of each thread block
        /// </summary>
        public uint blockDimZ;
        /// <summary>
        /// Dynamic shared-memory size per thread block in bytes
        /// </summary>
        public uint sharedMemBytes;
        /// <summary>
        /// Array of pointers to kernel parameters
        /// </summary>
        public IntPtr kernelParams;
        /// <summary>
        /// Extra options
        /// </summary>
        public IntPtr extra;
        /// <summary>
        /// Kernel to launch, will only be referenced if func is NULL
        /// </summary>
        CUkernel kern;
        /// <summary>
        /// Context for the kernel task to run in. The value NULL will indicate the current context should be used by the api. This field is ignored if func is set.
        /// </summary>
        CUcontext ctx;
    }

    /// <summary>
    /// Graph kernel node Attributes 
    /// </summary>
    public enum CUkernelNodeAttrID
    {
        /// <summary>
        /// Identifier for ::CUkernelNodeAttrValue::accessPolicyWindow.
        /// </summary>
        AccessPolicyWindow = 1,
        /// <summary>
        /// Allows a kernel node to be cooperative (see ::cuLaunchCooperativeKernel).
        /// </summary>
        Cooperative = 2
    }

    /// <summary>
    /// Graph attributes union, used with ::cuKernelNodeSetAttribute/::cuKernelNodeGetAttribute
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct CUkernelNodeAttrValue
    {
        /// <summary>
        /// Attribute ::CUaccessPolicyWindow.
        /// </summary>
        [FieldOffset(0)]
        public CUaccessPolicyWindow accessPolicyWindow;
        /// <summary>
        /// Nonzero indicates a cooperative kernel (see ::cuLaunchCooperativeKernel).
        /// </summary>
        [FieldOffset(0)]
        public int cooperative;
    }

    /// <summary>
    /// CUDA graph
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CUgraph
    {
        /// <summary>
        /// 
        /// </summary>
        public IntPtr Pointer;
    }

    /// <summary>
    /// CudaBatchMemOpNodeParams
    /// </summary>
    internal struct CudaBatchMemOpNodeParamsInternal
    {
        /// <summary/>
        public CUcontext ctx;
        /// <summary/>
        public uint count;
        /// <summary/>
        public IntPtr paramArray;
        /// <summary/>
        public uint flags;
    }

    /// <summary>
    /// CUDA user object for graphs
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CUuserObject
    {
        /// <summary>
        /// 
        /// </summary>
        public IntPtr Pointer;
    }

    /// <summary>
    /// 8-byte locally unique identifier. Value is undefined on TCC and non-Windows platforms 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Luid
    {
        /// <summary>
        /// 
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8, ArraySubType = UnmanagedType.I1)]
        public byte[] bytes;
    }

    /// <summary>
    /// Array formats
    /// </summary>
    public enum CUArrayFormat
    {
        /// <summary>
        /// Unsigned 8-bit integers
        /// </summary>
        UnsignedInt8 = 0x01,

        /// <summary>
        /// Unsigned 16-bit integers
        /// </summary>
        UnsignedInt16 = 0x02,

        /// <summary>
        /// Unsigned 32-bit integers
        /// </summary>
        UnsignedInt32 = 0x03,

        /// <summary>
        /// Signed 8-bit integers
        /// </summary>
        SignedInt8 = 0x08,

        /// <summary>
        /// Signed 16-bit integers
        /// </summary>
        SignedInt16 = 0x09,

        /// <summary>
        /// Signed 32-bit integers
        /// </summary>
        SignedInt32 = 0x0a,

        /// <summary>
        /// 16-bit floating point
        /// </summary>
        Half = 0x10,

        /// <summary>
        /// 32-bit floating point
        /// </summary>
        Float = 0x20,

        /// <summary>
        /// 8-bit YUV planar format, with 4:2:0 sampling
        /// </summary>
        NV12 = 0xb0,
        /// <summary>
        /// 1 channel unsigned 8-bit normalized integer
        /// </summary>
        UNormInt8X1 = 0xc0,
        /// <summary>
        /// 2 channel unsigned 8-bit normalized integer
        /// </summary>
        UNormInt8X2 = 0xc1,
        /// <summary>
        /// 4 channel unsigned 8-bit normalized integer
        /// </summary>
        UNormInt8X4 = 0xc2,
        /// <summary>
        /// 1 channel unsigned 16-bit normalized integer
        /// </summary>
        UNormInt16X1 = 0xc3,
        /// <summary>
        /// 2 channel unsigned 16-bit normalized integer
        /// </summary>
        UNormInt16X2 = 0xc4,
        /// <summary>
        /// 4 channel unsigned 16-bit normalized integer
        /// </summary>
        UNormInt16X4 = 0xc5,
        /// <summary>
        /// 1 channel signed 8-bit normalized integer
        /// </summary>
        SNormInt8X1 = 0xc6,
        /// <summary>
        /// 2 channel signed 8-bit normalized integer
        /// </summary>
        SNormInt8X2 = 0xc7,
        /// <summary>
        /// 4 channel signed 8-bit normalized integer
        /// </summary>
        SNormInt8X4 = 0xc8,
        /// <summary>
        /// 1 channel signed 16-bit normalized integer
        /// </summary>
        SNormInt16X1 = 0xc9,
        /// <summary>
        /// 2 channel signed 16-bit normalized integer
        /// </summary>
        SNormInt16X2 = 0xca,
        /// <summary>
        /// 4 channel signed 16-bit normalized integer 
        /// </summary>
        SNormInt16X4 = 0xcb,
        /// <summary>
        /// 4 channel unsigned normalized block-compressed (BC1 compression) format
        /// </summary>
        BC1UNorm = 0x91,
        /// <summary>
        /// 4 channel unsigned normalized block-compressed (BC1 compression) format with sRGB encoding
        /// </summary>
        BC1UNormSRGB = 0x92,
        /// <summary>
        /// 4 channel unsigned normalized block-compressed (BC2 compression) format
        /// </summary>
        BC2UNorm = 0x93,
        /// <summary>
        /// 4 channel unsigned normalized block-compressed (BC2 compression) format with sRGB encoding
        /// </summary>
        BC2UNormSRGB = 0x94,
        /// <summary>
        /// 4 channel unsigned normalized block-compressed (BC3 compression) format
        /// </summary>
        BC3UNorm = 0x95,
        /// <summary>
        /// 4 channel unsigned normalized block-compressed (BC3 compression) format with sRGB encoding
        /// </summary>
        BC3UNormSRGB = 0x96,
        /// <summary>
        /// 1 channel unsigned normalized block-compressed (BC4 compression) format
        /// </summary>
        BC4UNorm = 0x97,
        /// <summary>
        /// 1 channel signed normalized block-compressed (BC4 compression) format
        /// </summary>
        BC4SNorm = 0x98,
        /// <summary>
        /// 2 channel unsigned normalized block-compressed (BC5 compression) format
        /// </summary>
        BC5UNorm = 0x99,
        /// <summary>
        /// 2 channel signed normalized block-compressed (BC5 compression) format
        /// </summary>
        BC5SNorm = 0x9a,
        /// <summary>
        /// 3 channel unsigned half-float block-compressed (BC6H compression) format
        /// </summary>
        BC6HUF16 = 0x9b,
        /// <summary>
        /// 3 channel signed half-float block-compressed (BC6H compression) format
        /// </summary>
        BC6HSF16 = 0x9c,
        /// <summary>
        /// 4 channel unsigned normalized block-compressed (BC7 compression) format
        /// </summary>
        BC7UNorm = 0x9d,
        /// <summary>
        /// 4 channel unsigned normalized block-compressed (BC7 compression) format with sRGB encoding
        /// </summary>
        BC7UNormSRGB = 0x9e
    }

    /// <summary>
    /// Device properties
    /// </summary>
    public enum CUDeviceAttribute
    {
        /// <summary>
        /// Maximum number of threads per block
        /// </summary>
        MaxThreadsPerBlock = 1,

        /// <summary>
        /// Maximum block dimension X
        /// </summary>
        MaxBlockDimX = 2,

        /// <summary>
        /// Maximum block dimension Y
        /// </summary>
        MaxBlockDimY = 3,

        /// <summary>
        /// Maximum block dimension Z
        /// </summary>
        MaxBlockDimZ = 4,

        /// <summary>
        /// Maximum grid dimension X
        /// </summary>
        MaxGridDimX = 5,

        /// <summary>
        /// Maximum grid dimension Y
        /// </summary>
        MaxGridDimY = 6,

        /// <summary>
        /// Maximum grid dimension Z
        /// </summary>
        MaxGridDimZ = 7,

        /// <summary>
        /// Maximum amount of shared memory
        /// available to a thread block in bytes; this amount is shared by all thread blocks simultaneously resident on a
        /// multiprocessor
        /// </summary>
        MaxSharedMemoryPerBlock = 8,

        /// <summary>
        /// Deprecated, use MaxSharedMemoryPerBlock
        /// </summary>
        [Obsolete("Use MaxSharedMemoryPerBlock")]
        SharedMemoryPerBlock = 8,

        /// <summary>
        /// Memory available on device for __constant__ variables in a CUDA C kernel in bytes
        /// </summary>
        TotalConstantMemory = 9,

        /// <summary>
        /// Warp size in threads
        /// </summary>
        WarpSize = 10,

        /// <summary>
        /// Maximum pitch in bytes allowed by the memory copy functions
        /// that involve memory regions allocated through <see cref="DriverAPINativeMethods.MemoryManagement.cuMemAllocPitch_v2"/>
        /// </summary>
        MaxPitch = 11,

        /// <summary>
        /// Deprecated, use MaxRegistersPerBlock
        /// </summary>
        [Obsolete("Use MaxRegistersPerBlock")]
        RegistersPerBlock = 12,

        /// <summary>
        /// Maximum number of 32-bit registers available
        /// to a thread block; this number is shared by all thread blocks simultaneously resident on a multiprocessor
        /// </summary>
        MaxRegistersPerBlock = 12,

        /// <summary>
        /// Typical clock frequency in kilohertz
        /// </summary>
        ClockRate = 13,

        /// <summary>
        /// Alignment requirement; texture base addresses
        /// aligned to textureAlign bytes do not need an offset applied to texture fetches
        /// </summary>
        TextureAlignment = 14,

        /// <summary>
        /// 1 if the device can concurrently copy memory between host
        /// and device while executing a kernel, or 0 if not
        /// </summary>
        GPUOverlap = 15,

        /// <summary>
        /// Number of multiprocessors on device
        /// </summary>
        MultiProcessorCount = 0x10,

        /// <summary>
        /// Specifies whether there is a run time limit on kernels. <para/>
        /// 1 if there is a run time limit for kernels executed on the device, or 0 if not
        /// </summary>
        KernelExecTimeout = 0x11,

        /// <summary>
        /// Device is integrated with host memory. 1 if the device is integrated with the memory subsystem, or 0 if not
        /// </summary>
        Integrated = 0x12,

        /// <summary>
        /// Device can map host memory into CUDA address space. 1 if the device can map host memory into the
        /// CUDA address space, or 0 if not
        /// </summary>
        CanMapHostMemory = 0x13,

        /// <summary>
        /// Compute mode (See <see cref="CUComputeMode"/> for details)
        /// </summary>
        ComputeMode = 20,


        /// <summary>
        /// Maximum 1D texture width
        /// </summary>
        MaximumTexture1DWidth = 21,

        /// <summary>
        /// Maximum 2D texture width
        /// </summary>
        MaximumTexture2DWidth = 22,

        /// <summary>
        /// Maximum 2D texture height
        /// </summary>
        MaximumTexture2DHeight = 23,

        /// <summary>
        /// Maximum 3D texture width
        /// </summary>
        MaximumTexture3DWidth = 24,

        /// <summary>
        /// Maximum 3D texture height
        /// </summary>
        MaximumTexture3DHeight = 25,

        /// <summary>
        /// Maximum 3D texture depth
        /// </summary>
        MaximumTexture3DDepth = 26,

        /// <summary>
        /// Maximum texture array width
        /// </summary>
        MaximumTexture2DArray_Width = 27,

        /// <summary>
        /// Maximum texture array height
        /// </summary>
        MaximumTexture2DArray_Height = 28,

        /// <summary>
        /// Maximum slices in a texture array
        /// </summary>
        MaximumTexture2DArray_NumSlices = 29,

        /// <summary>
        /// Alignment requirement for surfaces
        /// </summary>
        SurfaceAllignment = 30,

        /// <summary>
        /// Device can possibly execute multiple kernels concurrently. <para/>
        /// 1 if the device supports executing multiple kernels
        /// within the same context simultaneously, or 0 if not. It is not guaranteed that multiple kernels will be resident on
        /// the device concurrently so this feature should not be relied upon for correctness.
        /// </summary>
        ConcurrentKernels = 31,

        /// <summary>
        /// Device has ECC support enabled. 1 if error correction is enabled on the device, 0 if error correction
        /// is disabled or not supported by the device.
        /// </summary>
        ECCEnabled = 32,

        /// <summary>
        /// PCI bus ID of the device
        /// </summary>
        PCIBusID = 33,

        /// <summary>
        /// PCI device ID of the device
        /// </summary>
        PCIDeviceID = 34,

        /// <summary>
        /// Device is using TCC driver model
        /// </summary>
        TCCDriver = 35,

        /// <summary>
        /// Peak memory clock frequency in kilohertz
        /// </summary>
        MemoryClockRate = 36,

        /// <summary>
        /// Global memory bus width in bits
        /// </summary>
        GlobalMemoryBusWidth = 37,

        /// <summary>
        /// Size of L2 cache in bytes
        /// </summary>
        L2CacheSize = 38,

        /// <summary>
        /// Maximum resident threads per multiprocessor
        /// </summary>
        MaxThreadsPerMultiProcessor = 39,

        /// <summary>
        /// Number of asynchronous engines
        /// </summary>
        AsyncEngineCount = 40,

        /// <summary>
        /// Device shares a unified address space with the host
        /// </summary>
        UnifiedAddressing = 41,

        /// <summary>
        /// Maximum 1D layered texture width
        /// </summary>
        MaximumTexture1DLayeredWidth = 42,

        /// <summary>
        /// Maximum layers in a 1D layered texture
        /// </summary>
        MaximumTexture1DLayeredLayers = 43,

        /// <summary>
        /// PCI domain ID of the device
        /// </summary>
        PCIDomainID = 50,

        /// <summary>
        /// Pitch alignment requirement for textures
        /// </summary>
        TexturePitchAlignment = 51,
        /// <summary>
        /// Maximum cubemap texture width/height
        /// </summary>
        MaximumTextureCubeMapWidth = 52,
        /// <summary>
        /// Maximum cubemap layered texture width/height
        /// </summary>
        MaximumTextureCubeMapLayeredWidth = 53,
        /// <summary>
        /// Maximum layers in a cubemap layered texture
        /// </summary>
        MaximumTextureCubeMapLayeredLayers = 54,
        /// <summary>
        /// Maximum 1D surface width
        /// </summary>
        MaximumSurface1DWidth = 55,
        /// <summary>
        /// Maximum 2D surface width
        /// </summary>
        MaximumSurface2DWidth = 56,
        /// <summary>
        /// Maximum 2D surface height
        /// </summary>
        MaximumSurface2DHeight = 57,
        /// <summary>
        /// Maximum 3D surface width
        /// </summary>
        MaximumSurface3DWidth = 58,
        /// <summary>
        /// Maximum 3D surface height
        /// </summary>
        MaximumSurface3DHeight = 59,
        /// <summary>
        /// Maximum 3D surface depth
        /// </summary>
        MaximumSurface3DDepth = 60,
        /// <summary>
        /// Maximum 1D layered surface width
        /// </summary>
        MaximumSurface1DLayeredWidth = 61,
        /// <summary>
        /// Maximum layers in a 1D layered surface
        /// </summary>
        MaximumSurface1DLayeredLayers = 62,
        /// <summary>
        /// Maximum 2D layered surface width
        /// </summary>
        MaximumSurface2DLayeredWidth = 63,
        /// <summary>
        /// Maximum 2D layered surface height
        /// </summary>
        MaximumSurface2DLayeredHeight = 64,
        /// <summary>
        /// Maximum layers in a 2D layered surface
        /// </summary>
        MaximumSurface2DLayeredLayers = 65,
        /// <summary>
        /// Maximum cubemap surface width
        /// </summary>
        MaximumSurfaceCubemapWidth = 66,
        /// <summary>
        /// Maximum cubemap layered surface width
        /// </summary>
        MaximumSurfaceCubemapLayeredWidth = 67,
        /// <summary>
        /// Maximum layers in a cubemap layered surface
        /// </summary>
        MaximumSurfaceCubemapLayeredLayers = 68,
        /// <summary>
        /// Maximum 1D linear texture width
        /// </summary>
        [Obsolete("Deprecated, do not use. Use cudaDeviceGetTexture1DLinearMaxWidth() or cuDeviceGetTexture1DLinearMaxWidth() instead.")]
        MaximumTexture1DLinearWidth = 69,
        /// <summary>
        /// Maximum 2D linear texture width
        /// </summary>
        MaximumTexture2DLinearWidth = 70,
        /// <summary>
        /// Maximum 2D linear texture height
        /// </summary>
        MaximumTexture2DLinearHeight = 71,
        /// <summary>
        /// Maximum 2D linear texture pitch in bytes
        /// </summary>
        MaximumTexture2DLinearPitch = 72,
        /// <summary>
        /// Maximum mipmapped 2D texture width
        /// </summary>
        MaximumTexture2DMipmappedWidth = 73,
        /// <summary>
        /// Maximum mipmapped 2D texture height
        /// </summary>
        MaximumTexture2DMipmappedHeight = 74,
        /// <summary>
        /// Major compute capability version number
        /// </summary>
        ComputeCapabilityMajor = 75,
        /// <summary>
        /// Minor compute capability version number
        /// </summary>
        ComputeCapabilityMinor = 76,
        /// <summary>
        /// Maximum mipmapped 1D texture width
        /// </summary>
        MaximumTexture1DMipmappedWidth = 77,
        /// <summary>
        /// Device supports stream priorities
        /// </summary>
        StreamPrioritiesSupported = 78,
        /// <summary>
        /// Device supports caching globals in L1
        /// </summary>
        GlobalL1CacheSupported = 79,
        /// <summary>
        /// Device supports caching locals in L1
        /// </summary>
        LocalL1CacheSupported = 80,
        /// <summary>
        /// Maximum shared memory available per multiprocessor in bytes
        /// </summary>
        MaxSharedMemoryPerMultiprocessor = 81,
        /// <summary>
        /// Maximum number of 32-bit registers available per multiprocessor
        /// </summary>
        MaxRegistersPerMultiprocessor = 82,
        /// <summary>
        /// Device can allocate managed memory on this system
        /// </summary>
        ManagedMemory = 83,
        /// <summary>
        /// Device is on a multi-GPU board
        /// </summary>
        MultiGpuBoard = 84,
        /// <summary>
        /// Unique id for a group of devices on the same multi-GPU board
        /// </summary>
        MultiGpuBoardGroupID = 85,
        /// <summary>
        /// Link between the device and the host supports native atomic operations (this is a placeholder attribute, and is not supported on any current hardware)
        /// </summary>
        HostNativeAtomicSupported = 86,
        /// <summary>
        /// Ratio of single precision performance (in floating-point operations per second) to double precision performance
        /// </summary>
        SingleToDoublePrecisionPerfRatio = 87,
        /// <summary>
        /// Device supports coherently accessing pageable memory without calling cudaHostRegister on it
        /// </summary>
        PageableMemoryAccess = 88,
        /// <summary>
        /// Device can coherently access managed memory concurrently with the CPU
        /// </summary>
        ConcurrentManagedAccess = 89,
        /// <summary>
        /// Device supports compute preemption.
        /// </summary>
        ComputePreemptionSupported = 90,
        /// <summary>
        /// Device can access host registered memory at the same virtual address as the CPU.
        /// </summary>
        CanUseHostPointerForRegisteredMem = 91,
        /// <summary>
        /// ::cuStreamBatchMemOp and related APIs are supported.
        /// </summary>
        [Obsolete("Deprecated, along with v1 MemOps API, ::cuStreamBatchMemOp and related APIs are supported.")]
        CanUseStreamMemOpsV1 = 92,
        /// <summary>
        /// 64-bit operations are supported in ::cuStreamBatchMemOp and related APIs.
        /// </summary>
        [Obsolete("Deprecated, along with v1 MemOps API, 64-bit operations are supported in ::cuStreamBatchMemOp and related APIs.")]
        CanUse64BitStreamMemOpsV1 = 93,
        /// <summary>
        /// ::CU_STREAM_WAIT_VALUE_NOR is supported.
        /// </summary>
        [Obsolete("Deprecated, along with v1 MemOps API, ::CU_STREAM_WAIT_VALUE_NOR is supported.")]
        CanUseStreamWaitValueNOrV1 = 94,
        /// <summary>
        /// Device supports launching cooperative kernels via ::cuLaunchCooperativeKernel
        /// </summary>
        CooperativeLaunch = 95,
        /// <summary>
        /// Device can participate in cooperative kernels launched via ::cuLaunchCooperativeKernelMultiDevice
        /// </summary>
        CooperativeMultiDeviceLaunch = 96,
        /// <summary>
        /// Maximum optin shared memory per block
        /// </summary>
        MaxSharedMemoryPerBlockOptin = 97,
        /// <summary>
        /// Both the ::CU_STREAM_WAIT_VALUE_FLUSH flag and the ::CU_STREAM_MEM_OP_FLUSH_REMOTE_WRITES MemOp are supported on the device. See \ref CUDA_MEMOP for additional details.
        /// </summary>
        CanFlushRemoteWrites = 98,
        /// <summary>
        /// Device supports host memory registration via ::cudaHostRegister.
        /// </summary>
        HostRegisterSupported = 99,
        /// <summary>
        /// Device accesses pageable memory via the host's page tables.
        /// </summary>
        PageableMemoryAccessUsesHostPageTables = 100,
        /// <summary>
        /// The host can directly access managed memory on the device without migration.
        /// </summary>
        DirectManagedMemoryAccessFromHost = 101,
        /// <summary>
        /// Deprecated, Use VirtualMemoryManagementSupported
        /// </summary>
        [Obsolete("Deprecated, Use VirtualMemoryManagementSupported")]
        VirtualAddressManagementSupported = 102,
        /// <summary>
        /// Device supports virtual memory management APIs like ::cuMemAddressReserve, ::cuMemCreate, ::cuMemMap and related APIs
        /// </summary>
        VirtualMemoryManagementSupported = 102,
        /// <summary>
        /// Device supports exporting memory to a posix file descriptor with ::cuMemExportToShareableHandle, if requested via ::cuMemCreate
        /// </summary>
        HandleTypePosixFileDescriptorSupported = 103,
        /// <summary>
        /// Device supports exporting memory to a Win32 NT handle with ::cuMemExportToShareableHandle, if requested via ::cuMemCreate
        /// </summary>
        HandleTypeWin32HandleSupported = 104,
        /// <summary>
        /// Device supports exporting memory to a Win32 KMT handle with ::cuMemExportToShareableHandle, if requested ::cuMemCreate
        /// </summary>
        HandleTypeWin32KMTHandleSupported = 105,
        /// <summary>
        /// Maximum number of blocks per multiprocessor
        /// </summary>
        MaxBlocksPerMultiProcessor = 106,
        /// <summary>
        /// Device supports compression of memory
        /// </summary>
        GenericCompressionSupported = 107,
        /// <summary>
        /// Device's maximum L2 persisting lines capacity setting in bytes
        /// </summary>
        MaxPersistingL2CacheSize = 108,
        /// <summary>
        /// The maximum value of CUaccessPolicyWindow::num_bytes.
        /// </summary>
        MaxAccessPolicyWindowSize = 109,
        /// <summary>
        /// Device supports specifying the GPUDirect RDMA flag with ::cuMemCreate
        /// </summary>
        GPUDirectRDMAWithCudaVMMSupported = 110,
        /// <summary>
        /// Shared memory reserved by CUDA driver per block in bytes
        /// </summary>
        ReservedSharedMemoryPerBlock = 111,
        /// <summary>
        /// Device supports sparse CUDA arrays and sparse CUDA mipmapped arrays
        /// </summary>
        SparseCudaArraySupported = 112,
        /// <summary>
        /// Device supports using the ::cuMemHostRegister flag CU_MEMHOSTERGISTER_READ_ONLY to register memory that must be mapped as read-only to the GPU
        /// </summary>
        ReadOnlyHostRegisterSupported = 113,
        /// <summary>
        /// External timeline semaphore interop is supported on the device
        /// </summary>
        TimelineSemaphoreInteropSupported = 114,
        /// <summary>
        /// Device supports using the ::cuMemAllocAsync and ::cuMemPool family of APIs
        /// </summary>
        MemoryPoolsSupported = 115,
        /// <summary>
        /// Device supports GPUDirect RDMA APIs, like nvidia_p2p_get_pages (see https://docs.nvidia.com/cuda/gpudirect-rdma for more information)
        /// </summary>
        GpuDirectRDMASupported = 116,
        /// <summary>
        /// The returned attribute shall be interpreted as a bitmask, where the individual bits are described by the ::CUflushGPUDirectRDMAWritesOptions enum
        /// </summary>
        GpuDirectRDMAFlushWritesOptions = 117,
        /// <summary>
        /// GPUDirect RDMA writes to the device do not need to be flushed for consumers within the scope indicated by the returned attribute. See ::CUGPUDirectRDMAWritesOrdering for the numerical values returned here.
        /// </summary>
        GpuDirectRDMAWritesOrdering = 118,
        /// <summary>
        /// Handle types supported with mempool based IPC
        /// </summary>
        MempoolSupportedHandleTypes = 119,


        /// <summary>
        /// Indicates device supports cluster launch
        /// </summary>
        ClusterLaunch = 120,
        /// <summary>
        /// Device supports deferred mapping CUDA arrays and CUDA mipmapped arrays
        /// </summary>
        DeferredMappingCudaArraySupported = 121,
        /// <summary>
        /// 64-bit operations are supported in ::cuStreamBatchMemOp and related MemOp APIs.
        /// </summary>
        CanUse64BitStreamMemOps = 122,
        /// <summary>
        /// ::CU_STREAM_WAIT_VALUE_NOR is supported by MemOp APIs.
        /// </summary>
        CanUseStreamWaitValueNOR = 123,
        /// <summary>
        /// Device supports buffer sharing with dma_buf mechanism.
        /// </summary>
        DmaBufSupported = 124,
        /// <summary>
        /// Device supports IPC Events.
        /// </summary>
        IPCEventSupported = 125,
        /// <summary>
        /// Number of memory domains the device supports.
        /// </summary>
        MemSyncDomainCount = 126,
        /// <summary>
        /// Device supports accessing memory using Tensor Map.
        /// </summary>
        TensorMapAccessSupported = 127,
        /// <summary>
        /// Device supports unified function pointers.
        /// </summary>
        UnifiedFunctionPointers = 129,

        /// <summary>
        /// Max elems...
        /// </summary>
        MAX
    }

    /// <summary>
    /// flags of ::cuDeviceGetNvSciSyncAttributes
    /// </summary>
    [Flags]
    public enum NvSciSyncAttr
    {
        /// <summary>
        /// When /p flags of ::cuDeviceGetNvSciSyncAttributes is set to this,
        /// it indicates that application needs signaler specific NvSciSyncAttr
        /// to be filled by ::cuDeviceGetNvSciSyncAttributes.
        /// </summary>
        Signal = 0x01,

        /// <summary>
        /// When /p flags of ::cuDeviceGetNvSciSyncAttributes is set to this,
        /// it indicates that application needs waiter specific NvSciSyncAttr
        /// to be filled by ::cuDeviceGetNvSciSyncAttributes.
        /// </summary>
        Wait = 0x02,
    }

    /// <summary>
    /// Interprocess Handle for Events
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CUipcEventHandle
    {
        /// <summary>
        /// 
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64, ArraySubType = UnmanagedType.I1)]
        public byte[] reserved;
    }

    /// <summary>
    /// Context creation flags. <para></para>
    /// The two LSBs of the flags parameter can be used to control how the OS thread, which owns the CUDA context at
    /// the time of an API call, interacts with the OS scheduler when waiting for results from the GPU.
    /// </summary>
    [Flags]
    public enum CUCtxFlags
    {
        /// <summary>
        /// The default value if the flags parameter is zero, uses a heuristic based on the
        /// number of active CUDA contexts in the process C and the number of logical processors in the system P. If C >
        /// P, then CUDA will yield to other OS threads when waiting for the GPU, otherwise CUDA will not yield while
        /// waiting for results and actively spin on the processor.
        /// </summary>
        SchedAuto = 0,

        /// <summary>
        /// Instruct CUDA to actively spin when waiting for results from the GPU. This can decrease
        /// latency when waiting for the GPU, but may lower the performance of CPU threads if they are performing
        /// work in parallel with the CUDA thread.
        /// </summary>
        SchedSpin = 1,

        /// <summary>
        /// Instruct CUDA to yield its thread when waiting for results from the GPU. This can
        /// increase latency when waiting for the GPU, but can increase the performance of CPU threads performing work
        /// in parallel with the GPU.
        /// </summary>
        SchedYield = 2,

        /// <summary>
        /// Instruct CUDA to block the CPU thread on a synchronization primitive when waiting for the GPU to finish work.
        /// </summary>
        BlockingSync = 4,

        /// <summary>
        /// No description found...
        /// </summary>
        SchedMask = 7,

        /// <summary>
        /// Instruct CUDA to support mapped pinned allocations. This flag must be set in order to allocate pinned host memory that is accessible to the GPU.
        /// </summary>
        [Obsolete("This flag was deprecated as of CUDA 11.0 and it no longer has any effect. All contexts as of CUDA 3.2 behave as though the flag is enabled.")]
        MapHost = 8,

        /// <summary>
        /// Instruct CUDA to not reduce local memory after resizing local memory
        /// for a kernel. This can prevent thrashing by local memory allocations when launching many kernels with high
        /// local memory usage at the cost of potentially increased memory usage.
        /// </summary>
        LMemResizeToMax = 16,

        /// <summary>
        /// No description found...
        /// </summary>
        FlagsMask = 0x1f
    }

    /// <summary>
    /// CUDA texture reference
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CUtexref
    {
        /// <summary>
        /// 
        /// </summary>
        public IntPtr Pointer;
    }

    /// <summary>
    /// CUDA mipmapped array
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CUmipmappedArray
    {
        /// <summary>
        /// 
        /// </summary>
        public IntPtr Pointer;

        /// <summary>
        /// Returns the memory requirements of a CUDA array
        /// </summary>
        public CudaArrayMemoryRequirements GetMemoryRequirements(CUdevice device)
        {
            CudaArrayMemoryRequirements temp = new CudaArrayMemoryRequirements();
            CUResult res = DriverAPINativeMethods.ArrayManagement.cuMipmappedArrayGetMemoryRequirements(ref temp, this, device);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuMipmappedArrayGetMemoryRequirements", res));
            if (res != CUResult.Success) throw new CudaException(res);
            return temp;
        }
    }

    /// <summary>
    /// CUDA graphics interop resource (DirectX / OpenGL)
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CUgraphicsResource
    {
        /// <summary>
        /// 
        /// </summary>
        public IntPtr Pointer;
    }

    /// <summary>
    /// Flags for mapping and unmapping graphics interop resources
    /// </summary>
    [Flags]
    public enum CUGraphicsMapResourceFlags
    {
        /// <summary>
        /// Specifies no hints about how this resource will be used.
        /// It is therefore assumed that this resource will be read from and written to by CUDA. This is the default value.
        /// </summary>
        None = 0,
        /// <summary>
        /// Specifies that CUDA will not write to this resource.
        /// </summary>
        ReadOnly = 1,
        /// <summary>
        /// Specifies that CUDA will not read from
        /// this resource and will write over the entire contents of the resource, so none of the data previously stored in the
        /// resource will be preserved.
        /// </summary>
        WriteDiscard = 2
    }

    /// <summary>
    /// CUDA texture object
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CUtexObject
    {
        /// <summary>
        /// 
        /// </summary>
        public ulong Pointer;
    }

    /// <summary>
    /// Possible stream capture statuses returned by ::cuStreamIsCapturing
    /// </summary>
    public enum CUstreamCaptureStatus
    {
        /// <summary>
        /// Stream is not capturing
        /// </summary>
        None = 0,
        /// <summary>
        /// Stream is actively capturing
        /// </summary>
        Active = 1,
        /// <summary>
        /// Stream is part of a capture sequence that has been invalidated, but not terminated
        /// </summary>
        Invalidated = 2
    }

    /// <summary>
    /// Limits
    /// </summary>
    public enum CULimit
    {
        /// <summary>
        /// GPU thread stack size
        /// </summary>
        StackSize = 0,

        /// <summary>
        /// GPU printf FIFO size
        /// </summary>
        PrintfFIFOSize = 1,

        /// <summary>
        /// GPU malloc heap size
        /// </summary>
        MallocHeapSize = 2,

        /// <summary>
        /// GPU device runtime launch synchronize depth
        /// </summary>
        DevRuntimeSyncDepth = 3,

        /// <summary>
        /// GPU device runtime pending launch count
        /// </summary>
        DevRuntimePendingLaunchCount = 4,

        /// <summary>
        /// A value between 0 and 128 that indicates the maximum fetch granularity of L2 (in Bytes). This is a hint
        /// </summary>
        MaxL2FetchGranularity = 0x05,

        /// <summary>
        /// A size in bytes for L2 persisting lines cache size
        /// </summary>
        PersistingL2CacheSize = 0x06
    }

    /// <summary>
    /// CUDA driver API Context Enable Peer Access flags
    /// </summary>
    [Flags]
    public enum CtxEnablePeerAccessFlags : uint
    {
        /// <summary>
        /// Currently no flags are defined.
        /// </summary>
        None = 0
    }

    /// <summary>
    /// Texture reference addressing modes
    /// </summary>
    public enum CUAddressMode
    {
        /// <summary>
        /// Wrapping address mode
        /// </summary>
        Wrap = 0,

        /// <summary>
        /// Clamp to edge address mode
        /// </summary>
        Clamp = 1,

        /// <summary>
        /// Mirror address mode
        /// </summary>
        Mirror = 2,

        /// <summary>
        /// Border address mode
        /// </summary>
        Border = 3
    }

    /// <summary>
    /// Texture reference filtering modes
    /// </summary>
    public enum CUFilterMode
    {
        /// <summary>
        /// Point filter mode
        /// </summary>
        Point = 0,

        /// <summary>
        /// Linear filter mode
        /// </summary>
        Linear = 1
    }

    /// <summary>
    /// CUMemHostRegisterFlags. All of these flags are orthogonal to one another: a developer may allocate memory that is portable or mapped
    /// with no restrictions.
    /// </summary>
    [Flags]
    public enum CUMemHostRegisterFlags
    {
        /// <summary>
        /// No flags
        /// </summary>
        None = 0,
        /// <summary>
        /// The memory returned by this call will be considered as pinned memory
        /// by all CUDA contexts, not just the one that performed the allocation.
        /// </summary>
        Portable = 1,

        /// <summary>
        /// Maps the allocation into the CUDA address space. The device pointer
        /// to the memory may be obtained by calling <see cref="DriverAPINativeMethods.MemoryManagement.cuMemHostGetDevicePointer_v2"/>. This feature is available only on
        /// GPUs with compute capability greater than or equal to 1.1.
        /// </summary>
        DeviceMap = 2,

        /// <summary>
        /// If set, the passed memory pointer is treated as pointing to some
        /// memory-mapped I/O space, e.g. belonging to a third-party PCIe device.<para/>
        /// On Windows the flag is a no-op.<para/>
        /// On Linux that memory is marked as non cache-coherent for the GPU and
        /// is expected to be physically contiguous.<para/>
        /// On all other platforms, it is not supported and CUDA_ERROR_INVALID_VALUE
        /// is returned.<para/>
        /// </summary>
        IOMemory = 0x04,

        /// <summary>
        /// If set, the passed memory pointer is treated as pointing to memory that is
        /// considered read-only by the device.  On platforms without
        /// CU_DEVICE_ATTRIBUTE_PAGEABLE_MEMORY_ACCESS_USES_HOST_PAGE_TABLES, this flag is
        /// required in order to register memory mapped to the CPU as read-only.  Support
        /// for the use of this flag can be queried from the device attribute
        /// CU_DEVICE_ATTRIBUTE_READ_ONLY_HOST_REGISTER_SUPPORTED.  Using this flag with
        /// a current context associated with a device that does not have this attribute
        /// set will cause ::cuMemHostRegister to error with CUDA_ERROR_NOT_SUPPORTED.
        /// </summary>
        ReadOnly = 0x08
    }

    /// <summary>
    /// CUDA surface reference
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CUsurfref
    {
        /// <summary>
        /// 
        /// </summary>
        public IntPtr Pointer;
    }

    /// <summary>
    /// Tensor map descriptor. Requires compiler support for aligning to 64 bytes.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 64)]
    public struct CUtensorMap
    {
        //CU_TENSOR_MAP_NUM_QWORDS = 16; Size of tensor map descriptor
        /// <summary/>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16, ArraySubType = UnmanagedType.U8)]
        public ulong[] opaque;
    }

    /// <summary>
    /// CUDA executable graph
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CUgraphExec
    {
        /// <summary>
        /// 
        /// </summary>
        public IntPtr Pointer;
    }

    /// <summary>
    /// Interprocess Handle for Memory
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CUipcMemHandle
    {
        /// <summary>
        /// 
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64, ArraySubType = UnmanagedType.I1)]
        public byte[] reserved;
    }

    /// <summary>
    /// Execution Affinity Parameters 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CUexecAffinityParam
    {
        CUexecAffinityType type;
        CUexecAffinityParamUnion param;
    }

    /// <summary>
    /// Value for ::CU_EXEC_AFFINITY_TYPE_SM_COUNT
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct CUexecAffinityParamUnion
    {
        /// <summary>
        /// The number of SMs the context is limited to use.
        /// </summary>
        [FieldOffset(0)]
        public CUexecAffinitySmCount smCount;
    }

    /// <summary>
    /// Value for ::CU_EXEC_AFFINITY_TYPE_SM_COUNT
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CUexecAffinitySmCount
    {
        /// <summary>
        /// The number of SMs the context is limited to use.
        /// </summary>
        public uint val;
    }

    /// <summary>
    /// CUDA linker
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CUlinkState
    {
        /// <summary>
        /// 
        /// </summary>
        public IntPtr Pointer;
    }

    /// <summary>
    /// Device code formats
    /// </summary>
    public enum CUJITInputType
    {
        /// <summary>
        /// Compiled device-class-specific device code
        /// <para>Applicable options: none</para>
        /// </summary>
        Cubin = 0,

        /// <summary>
        /// PTX source code
        /// <para>Applicable options: PTX compiler options</para>
        /// </summary>
        PTX,

        /// <summary>
        /// Bundle of multiple cubins and/or PTX of some device code
        /// <para>Applicable options: PTX compiler options, ::CU_JIT_FALLBACK_STRATEGY</para>
        /// </summary>
        FatBinary,

        /// <summary>
        /// Host object with embedded device code
        /// <para>Applicable options: PTX compiler options, ::CU_JIT_FALLBACK_STRATEGY</para>
        /// </summary>
        Object,

        /// <summary>
        /// Archive of host objects with embedded device code
        /// <para>Applicable options: PTX compiler options, ::CU_JIT_FALLBACK_STRATEGY</para>
        /// </summary>
        Library,

        /// <summary>
        /// High-level intermediate code for link-time optimization
        /// Applicable options: NVVM compiler options, PTX compiler options
        /// </summary>
        [Obsolete("Only valid with LTO-IR compiled with toolkits prior to CUDA 12.0")]
        NVVM
    }

    /// <summary>
    /// CUMemHostAllocFlags. All of these flags are orthogonal to one another: a developer may allocate memory that is portable, mapped and/or
    /// write-combined with no restrictions.
    /// </summary>
    [Flags]
    public enum CUMemHostAllocFlags
    {
        /// <summary>
        /// No flags
        /// </summary>
        None = 0,
        /// <summary>
        /// The memory returned by this call will be considered as pinned memory
        /// by all CUDA contexts, not just the one that performed the allocation.
        /// </summary>
        Portable = 1,

        /// <summary>
        /// Maps the allocation into the CUDA address space. The device pointer
        /// to the memory may be obtained by calling <see cref="DriverAPINativeMethods.MemoryManagement.cuMemHostGetDevicePointer_v2"/>. This feature is available only on
        /// GPUs with compute capability greater than or equal to 1.1.
        /// </summary>
        DeviceMap = 2,

        /// <summary>
        /// Allocates the memory as write-combined (WC). WC memory
        /// can be transferred across the PCI Express bus more quickly on some system configurations, but cannot be read
        /// efficiently by most CPUs. WC memory is a good option for buffers that will be written by the CPU and read by
        /// the GPU via mapped pinned memory or host->device transfers.<para/>
        /// If set, host memory is allocated as write-combined - fast to write,
        /// faster to DMA, slow to read except via SSE4 streaming load instruction
        /// (MOVNTDQA).
        /// </summary>
        WriteCombined = 4
    }

    /// <summary>
    /// Memory advise values
    /// </summary>
    public enum CUmemAdvise
    {
        /// <summary>
        /// Data will mostly be read and only occassionally be written to
        /// </summary>
        SetReadMostly = 1,
        /// <summary>
        /// Undo the effect of ::CU_MEM_ADVISE_SET_READ_MOSTLY
        /// </summary>
        UnsetReadMostly = 2,
        /// <summary>
        /// Set the preferred location for the data as the specified device
        /// </summary>
        SetPreferredLocation = 3,
        /// <summary>
        /// Clear the preferred location for the data
        /// </summary>
        UnsetPreferredLocation = 4,
        /// <summary>
        /// Data will be accessed by the specified device, so prevent page faults as much as possible
        /// </summary>
        SetAccessedBy = 5,
        /// <summary>
        /// Let the Unified Memory subsystem decide on the page faulting policy for the specified device
        /// </summary>
        UnsetAccessedBy = 6
    }

    /// <summary>
    /// 
    /// </summary>
    public enum CUmem_range_attribute
    {
        /// <summary>
        /// Whether the range will mostly be read and only occassionally be written to
        /// </summary>
        ReadMostly = 1,
        /// <summary>
        /// The preferred location of the range
        /// </summary>
        PreferredLocation = 2,
        /// <summary>
        /// Memory range has ::CU_MEM_ADVISE_SET_ACCESSED_BY set for specified device
        /// </summary>
        AccessedBy = 3,
        /// <summary>
        /// The last location to which the range was prefetched
        /// </summary>
        LastPrefetchLocation = 4
    }

    /// <summary>
    /// Specifies the allocation properties for a allocation.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CUmemAllocationProp
    {
        /// <summary>
        /// Allocation type
        /// </summary>
        public CUmemAllocationType type;
        /// <summary>
        /// requested ::CUmemAllocationHandleType
        /// </summary>
        public CUmemAllocationHandleType requestedHandleTypes;
        /// <summary>
        /// Location of allocation
        /// </summary>
        public CUmemLocation location;
        /// <summary>
        /// Windows-specific LPSECURITYATTRIBUTES required when
        /// ::CU_MEM_HANDLE_TYPE_WIN32 is specified.This security attribute defines
        /// the scope of which exported allocations may be tranferred to other
        /// processes.In all other cases, this field is required to be zero.
        /// </summary>
        public IntPtr win32HandleMetaData;
        /// <summary>
        /// allocFlags
        /// </summary>
        public allocFlags allocFlags;
    }

    /// <summary>
    /// Allocation hint for requesting compressible memory.
    /// On devices that support Compute Data Compression, compressible
    /// memory can be used to accelerate accesses to data with unstructured
    /// sparsity and other compressible data patterns.Applications are
    /// expected to query allocation property of the handle obtained with
    /// ::cuMemCreate using ::cuMemGetAllocationPropertiesFromHandle to
    /// validate if the obtained allocation is compressible or not.Note that
    /// compressed memory may not be mappable on all devices.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct allocFlags
    {
        /// <summary/>
        public CUmemAllocationCompType compressionType;
        /// <summary/>
        public byte gpuDirectRDMACapable;
        /// <summary>
        /// Bitmask indicating intended usage for this allocation
        /// </summary>
        public CUmemCreateUsage usage;
        byte reserved0;
        byte reserved1;
        byte reserved2;
        byte reserved3;
    }

    /// <summary>
    /// Specifies compression attribute for an allocation.
    /// </summary>
    public enum CUmemAllocationCompType : byte
    {
        /// <summary>
        /// Allocating non-compressible memory
        /// </summary>
        None = 0x0,
        /// <summary>
        /// Allocating  compressible memory
        /// </summary>
        Generic = 0x1
    }

    /// <summary>
    /// 
    /// </summary>
    public enum CUmemCreateUsage : ushort
    {
        /// <summary>
        /// 
        /// </summary>
        None = 0x0,
        /// <summary>
        /// This flag if set indicates that the memory will be used as a tile pool.
        /// </summary>
        TilePool = 0x1
    }

    /// <summary>
    /// 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CUmemGenericAllocationHandle
    {
        /// <summary>
        /// 
        /// </summary>
        public ulong Pointer;
    }

    /// <summary>
    /// Specifies the CUDA array or CUDA mipmapped array memory mapping information
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CUarrayMapInfo
    {

        /// <summary>
        /// 
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        public struct Resource
        {
            /// <summary>
            /// resource
            /// </summary>
            [FieldOffset(0)]

            public CUmipmappedArray mipmap;
            /// <summary>
            /// resource
            /// </summary>
            [FieldOffset(0)]
            public CUarray array;
        }

        /// <summary>
        /// 
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        public struct Subresource
        {
            /// <summary>
            /// 
            /// </summary>
            public struct SparseLevel
            {
                /// <summary>
                /// For CUDA mipmapped arrays must a valid mipmap level. For CUDA arrays must be zero 
                /// </summary>
                public uint level;
                /// <summary>
                /// For CUDA layered arrays must be a valid layer index. Otherwise, must be zero
                /// </summary>
                public uint layer;
                /// <summary>
                /// Starting X offset in elements
                /// </summary>
                public uint offsetX;
                /// <summary>
                /// Starting Y offset in elements
                /// </summary>
                public uint offsetY;
                /// <summary>
                /// Starting Z offset in elements
                /// </summary>
                public uint offsetZ;
                /// <summary>
                /// Width in elements
                /// </summary>
                public uint extentWidth;
                /// <summary>
                /// Height in elements
                /// </summary>
                public uint extentHeight;
                /// <summary>
                /// Depth in elements
                /// </summary>
                public uint extentDepth;
            }
            /// <summary>
            /// 
            /// </summary>
            public struct Miptail
            {
                /// <summary>
                /// For CUDA layered arrays must be a valid layer index. Otherwise, must be zero
                /// </summary>
                public uint layer;
                /// <summary>
                /// Offset within mip tail 
                /// </summary>
                public ulong offset;
                /// <summary>
                /// Extent in bytes
                /// </summary>
                public ulong size;
            }

            /// <summary>
            /// 
            /// </summary>
            [FieldOffset(0)]

            public SparseLevel sparseLevel;
            /// <summary>
            /// 
            /// </summary>
            [FieldOffset(0)]

            public Miptail miptail;
        }

        /// <summary>
        /// Resource type
        /// </summary>
        public CUResourceType resourceType;

        /// <summary>
        /// 
        /// </summary>
        public Resource resource;

        /// <summary>
        /// Sparse subresource type
        /// </summary>
        public CUarraySparseSubresourceType subresourceType;

        /// <summary>
        /// 
        /// </summary>
        public Subresource subresource;

        /// <summary>
        /// Memory operation type
        /// </summary>
        public CUmemOperationType memOperationType;
        /// <summary>
        /// Memory handle type
        /// </summary>
        public CUmemHandleType memHandleType;
        /// <summary>
        /// 
        /// </summary>

        public CUmemGenericAllocationHandle memHandle;

        /// <summary>
        /// Offset within the memory
        /// </summary>
        public ulong offset;
        /// <summary>
        /// Device ordinal bit mask
        /// </summary>
        public uint deviceBitMask;
        /// <summary>
        /// flags for future use, must be zero now.
        /// </summary>
        public uint flags;
        /// <summary>
        /// Reserved for future use, must be zero now.
        /// </summary>
        public uint reserved0;
        /// <summary>
        /// Reserved for future use, must be zero now.
        /// </summary>
        public uint reserved1;
    }

    /// <summary>
    /// Resource types
    /// </summary>
    public enum CUResourceType
    {
        /// <summary>
        /// Array resoure
        /// </summary>
        Array = 0x00,
        /// <summary>
        /// Mipmapped array resource
        /// </summary>
        MipmappedArray = 0x01,
        /// <summary>
        /// Linear resource
        /// </summary>
        Linear = 0x02,
        /// <summary>
        /// Pitch 2D resource
        /// </summary>
        Pitch2D = 0x03
    }

    /// <summary>
    /// Memory operation types
    /// </summary>
    public enum CUmemOperationType
    {
        /// <summary>
        /// 
        /// </summary>
        Map = 1,
        /// <summary>
        /// 
        /// </summary>
        Unmap = 2
    }

    /// <summary>
    /// Memory handle types
    /// </summary>
    public enum CUmemHandleType
    {
        /// <summary>
        /// 
        /// </summary>
        Generic = 0
    }

    /// <summary>
    /// Sparse subresource types
    /// </summary>
    public enum CUarraySparseSubresourceType
    {
        /// <summary>
        /// 
        /// </summary>
        SparseLevel = 0,
        /// <summary>
        /// 
        /// </summary>
        MipTail = 1
    }

    /// <summary>
    /// CUDA external memory
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CUexternalMemory
    {
        /// <summary>
        /// 
        /// </summary>
        public IntPtr Pointer;
    }

    /// <summary>
    /// Tensor map data type
    /// </summary>
    public enum CUtensorMapDataType
    {
        /// <summary/>
        UInt8 = 0,
        /// <summary/>
        UInt16,
        /// <summary/>
        UInt32,
        /// <summary/>
        Int32,
        /// <summary/>
        UInt64,
        /// <summary/>
        Int64,
        /// <summary/>
        Float16,
        /// <summary/>
        Float32,
        /// <summary/>
        Float64,
        /// <summary/>
        BFloat16,
        /// <summary/>
        Float32_FTZ,
        /// <summary/>
        TFloat32,
        /// <summary/>
        TFloat32_FTZ
    }

    /// <summary>
    /// 3D memory copy parameters
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CUDAMemCpy3DPeer
    {
        /// <summary>
        /// Source X in bytes
        /// </summary>
        public SizeT srcXInBytes;

        /// <summary>
        /// Source Y
        /// </summary>
        public SizeT srcY;

        /// <summary>
        /// Source Z
        /// </summary>
        public SizeT srcZ;

        /// <summary>
        /// Source LOD
        /// </summary>
        public SizeT srcLOD;

        /// <summary>
        /// Source memory type (host, device, array)
        /// </summary>
        public CUMemoryType srcMemoryType;


        /// <summary>
        /// Source host pointer
        /// </summary>
        public IntPtr srcHost;


        /// <summary>
        /// Source device pointer
        /// </summary>
        public CUdeviceptr srcDevice;

        /// <summary>
        /// Source array reference
        /// </summary>
        public CUarray srcArray;

        /// <summary>
        /// Source context (ignored with srcMemoryType is array)
        /// </summary>
        public CUcontext srcContext;

        /// <summary>
        /// Source pitch (ignored when src is array)
        /// </summary>
        public SizeT srcPitch;

        /// <summary>
        /// Source height (ignored when src is array; may be 0 if Depth==1)
        /// </summary>
        public SizeT srcHeight;

        /// <summary>
        /// Destination X in bytes
        /// </summary>
        public SizeT dstXInBytes;

        /// <summary>
        /// Destination Y
        /// </summary>
        public SizeT dstY;

        /// <summary>
        /// Destination Z
        /// </summary>
        public SizeT dstZ;

        /// <summary>
        /// Destination LOD
        /// </summary>
        public SizeT dstLOD;

        /// <summary>
        /// Destination memory type (host, device, array)
        /// </summary>
        public CUMemoryType dstMemoryType;

        /// <summary>
        /// Destination host pointer
        /// </summary>
        public IntPtr dstHost;

        /// <summary>
        /// Destination device pointer
        /// </summary>
        public CUdeviceptr dstDevice;

        /// <summary>
        /// Destination array reference
        /// </summary>
        public CUarray dstArray;

        /// <summary>
        /// Destination context (ignored with dstMemoryType is array)
        /// </summary>
        public CUcontext dstContext;

        /// <summary>
        /// Destination pitch (ignored when dst is array)
        /// </summary>
        public SizeT dstPitch;

        /// <summary>
        /// Destination height (ignored when dst is array; may be 0 if Depth==1)
        /// </summary>
        public SizeT dstHeight;

        /// <summary>
        /// Width of 3D memory copy in bytes
        /// </summary>
        public SizeT WidthInBytes;

        /// <summary>
        /// Height of 3D memory copy
        /// </summary>
        public SizeT Height;

        /// <summary>
        /// Depth of 3D memory copy
        /// </summary>
        public SizeT Depth;
    }

    /// <summary>
    /// Specifies the handle type for address range
    /// </summary>
    public enum CUmemRangeHandleType
    {
        /// <summary/>
        DMA_BUF_FD = 0x1,
        /// <summary/>
        MAX = 0x7FFFFFFF,
    }

    /// <summary>
    /// Context Attach flags
    /// </summary>
    public enum CUCtxAttachFlags
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0
    }

    /// <summary>
    /// CUDA Mem Attach Flags
    /// </summary>
    public enum CUmemAttach_flags
    {
        /// <summary>
        /// Memory can be accessed by any stream on any device
        /// </summary>
        Global = 1,

        /// <summary>
        /// Memory cannot be accessed by any stream on any device
        /// </summary>
        Host = 2,

        /// <summary>
        /// Memory can only be accessed by a single stream on the associated device
        /// </summary>
        Single = 4
    }

    /// <summary>
    /// Flag for requesting different optimal and required granularities for an allocation.
    /// </summary>
    [Flags]
    public enum CUmemAllocationGranularity_flags
    {
        /// <summary>
        /// Minimum required granularity for allocation
        /// </summary>
        Minimum = 0x0,
        /// <summary>
        /// Recommended granularity for allocation for best performance
        /// </summary>
        Recommended = 0x1
    }

    /// <summary>
    /// Array descriptor
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CUDAArrayDescriptor
    {
        /// <summary>
        /// Width of array
        /// </summary>
        public SizeT Width;

        /// <summary>
        /// Height of array
        /// </summary>
        public SizeT Height;

        /// <summary>
        /// Array format
        /// </summary>
        public CUArrayFormat Format;

        /// <summary>
        /// Channels per array element
        /// </summary>
        public uint NumChannels;
    }

    /// <summary>
    /// CUDA array sparse properties
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CudaArraySparseProperties
    {

        /// <summary>
        /// TileExtent
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct TileExtent
        {
            /// <summary>
            /// Width of sparse tile in elements
            /// </summary>
            public uint width;
            /// <summary>
            /// Height of sparse tile in elements
            /// </summary>
            public uint height;
            /// <summary>
            /// Depth of sparse tile in elements
            /// </summary>
            public uint depth;
        }
        /// <summary>
        /// TileExtent
        /// </summary>
        public TileExtent tileExtent;

        /// <summary>
        /// First mip level at which the mip tail begins.
        /// </summary>
        public uint miptailFirstLevel;
        /// <summary>
        /// Total size of the mip tail.
        /// </summary>
        public ulong miptailSize;
        /// <summary>
        /// Flags will either be zero or ::CU_ARRAY_SPARSE_PROPERTIES_SINGLE_MIPTAIL 
        /// </summary>
        public CUArraySparsePropertiesFlags flags;
        uint reserved0;
        uint reserved1;
        uint reserved2;
        uint reserved3;
    }

    /// <summary>
    /// 3D array descriptor
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CUDAArray3DDescriptor
    {
        /// <summary>
        /// Width of 3D array
        /// </summary>
        public SizeT Width;

        /// <summary>
        /// Height of 3D array
        /// </summary>
        public SizeT Height;

        /// <summary>
        /// Depth of 3D array
        /// </summary>
        public SizeT Depth;

        /// <summary>
        /// Array format
        /// </summary>
        public CUArrayFormat Format;

        /// <summary>
        /// Channels per array element
        /// </summary>
        public uint NumChannels;

        /// <summary>
        /// Flags
        /// </summary>
        public CUDAArray3DFlags Flags;
    }

    /// <summary>
    /// CUTexRefSetArrayFlags
    /// </summary>
    public enum CUTexRefSetArrayFlags
    {
        /// <summary>
        /// 
        /// </summary>
        None = 0,
        /// <summary>
        /// Override the texref format with a format inferred from the array.
        /// <para/>Flag for <see cref="DriverAPINativeMethods.TextureReferenceManagement.cuTexRefSetArray"/>.
        /// </summary>
        OverrideFormat = 1
    }

    /// <summary>
    /// CUTexRefSetFlags
    /// </summary>
    [Flags]
    public enum CUTexRefSetFlags
    {
        /// <summary>
        /// 
        /// </summary>
        None = 0,
        /// <summary>
        /// Read the texture as integers rather than promoting the values to floats in the range [0,1].
        /// <para/>Flag for <see cref="DriverAPINativeMethods.TextureReferenceManagement.cuTexRefSetFlags"/>
        /// </summary>
        ReadAsInteger = 1,

        /// <summary>
        /// Use normalized texture coordinates in the range [0,1) instead of [0,dim).
        /// <para/>Flag for <see cref="DriverAPINativeMethods.TextureReferenceManagement.cuTexRefSetFlags"/>
        /// </summary>
        NormalizedCoordinates = 2,

        /// <summary>
        /// Perform sRGB -> linear conversion during texture read.
        /// </summary>
        sRGB = 0x10,

        /// <summary>
        /// Disable any trilinear filtering optimizations.
        /// </summary>
        DisableTrilinearOptimization = 0x20,

        /// <summary>
        /// Enable seamless cube map filtering.
        /// </summary>
        SeamlessCubeMap = 0x40
    }

    /// <summary>
    /// CUSurfRefSetFlags
    /// </summary>
    public enum CUSurfRefSetFlags
    {
        /// <summary>
        /// Currently no CUSurfRefSetFlags flags are defined.
        /// </summary>
        None = 0
    }

    /// <summary>
    /// Possible modes for stream capture thread interactions. For more details see ::cuStreamBeginCapture and ::cuThreadExchangeStreamCaptureMode
    /// </summary>
    public enum CUstreamCaptureMode
    {
        /// <summary>
        /// 
        /// </summary>
        Global = 0,
        /// <summary>
        /// 
        /// </summary>
        Local = 1,
        /// <summary>
        /// 
        /// </summary>
        Relaxed = 2
    }

    /// <summary>
    /// P2P Attributes
    /// </summary>
    public enum CUdevice_P2PAttribute
    {
        /// <summary>
        /// A relative value indicating the performance of the link between two devices
        /// </summary>
        PerformanceRank = 0x01,
        /// <summary>
        /// P2P Access is enable
        /// </summary>
        AccessSupported = 0x02,
        /// <summary>
        /// Atomic operation over the link supported
        /// </summary>
        NativeAtomicSupported = 0x03,
        /// <summary>
        /// \deprecated use CudaArrayAccessAccessSupported instead
        /// </summary>
        [Obsolete("use CudaArrayAccessAccessSupported instead")]
        AccessAccessSupported = 0x04,
        /// <summary>
        /// Accessing CUDA arrays over the link supported
        /// </summary>
        CudaArrayAccessAccessSupported = 0x04

    }

    /// <summary>
    /// CUDA Resource descriptor
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CudaResourceDesc
    {
        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="var"></param>
        public CudaResourceDesc(CudaMipmappedArray var)
        {
            resType = CUResourceType.MipmappedArray;
            flags = 0;
            res = new CudaResourceDescUnion();
            res.hArray = new CUarray();
            res.linear = new CudaResourceDescLinear();
            res.pitch2D = new CudaResourceDescPitch2D();
            res.hMipmappedArray = var.CUMipmappedArray; ;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="var"></param>
        public CudaResourceDesc(CudaDeviceVariable<float> var)
        {
            resType = CUResourceType.Linear;
            flags = 0;
            res = new CudaResourceDescUnion();
            res.hArray = new CUarray();
            res.hMipmappedArray = new CUmipmappedArray();
            res.pitch2D = new CudaResourceDescPitch2D();
            res.linear = new CudaResourceDescLinear();

            res.linear.devPtr = var.DevicePointer;
            res.linear.format = CUArrayFormat.Float;
            res.linear.numChannels = 1;
            res.linear.sizeInBytes = var.SizeInBytes;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="var"></param>
        public CudaResourceDesc(CudaDeviceVariable<int> var)
        {
            resType = CUResourceType.Linear;
            flags = 0;
            res = new CudaResourceDescUnion();
            res.hArray = new CUarray();
            res.hMipmappedArray = new CUmipmappedArray();
            res.pitch2D = new CudaResourceDescPitch2D();
            res.linear = new CudaResourceDescLinear();

            res.linear.devPtr = var.DevicePointer;
            res.linear.format = CUArrayFormat.SignedInt32;
            res.linear.numChannels = 1;
            res.linear.sizeInBytes = var.SizeInBytes;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="var"></param>
        public CudaResourceDesc(CudaDeviceVariable<short> var)
        {
            resType = CUResourceType.Linear;
            flags = 0;
            res = new CudaResourceDescUnion();
            res.hArray = new CUarray();
            res.hMipmappedArray = new CUmipmappedArray();
            res.pitch2D = new CudaResourceDescPitch2D();
            res.linear = new CudaResourceDescLinear();

            res.linear.devPtr = var.DevicePointer;
            res.linear.format = CUArrayFormat.SignedInt16;
            res.linear.numChannels = 1;
            res.linear.sizeInBytes = var.SizeInBytes;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="var"></param>
        public CudaResourceDesc(CudaDeviceVariable<sbyte> var)
        {
            resType = CUResourceType.Linear;
            flags = 0;
            res = new CudaResourceDescUnion();
            res.hArray = new CUarray();
            res.hMipmappedArray = new CUmipmappedArray();
            res.pitch2D = new CudaResourceDescPitch2D();
            res.linear = new CudaResourceDescLinear();

            res.linear.devPtr = var.DevicePointer;
            res.linear.format = CUArrayFormat.SignedInt8;
            res.linear.numChannels = 1;
            res.linear.sizeInBytes = var.SizeInBytes;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="var"></param>
        public CudaResourceDesc(CudaDeviceVariable<byte> var)
        {
            resType = CUResourceType.Linear;
            flags = 0;
            res = new CudaResourceDescUnion();
            res.hArray = new CUarray();
            res.hMipmappedArray = new CUmipmappedArray();
            res.pitch2D = new CudaResourceDescPitch2D();
            res.linear = new CudaResourceDescLinear();

            res.linear.devPtr = var.DevicePointer;
            res.linear.format = CUArrayFormat.UnsignedInt8;
            res.linear.numChannels = 1;
            res.linear.sizeInBytes = var.SizeInBytes;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="var"></param>
        public CudaResourceDesc(CudaDeviceVariable<ushort> var)
        {
            resType = CUResourceType.Linear;
            flags = 0;
            res = new CudaResourceDescUnion();
            res.hArray = new CUarray();
            res.hMipmappedArray = new CUmipmappedArray();
            res.pitch2D = new CudaResourceDescPitch2D();
            res.linear = new CudaResourceDescLinear();

            res.linear.devPtr = var.DevicePointer;
            res.linear.format = CUArrayFormat.UnsignedInt16;
            res.linear.numChannels = 1;
            res.linear.sizeInBytes = var.SizeInBytes;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="var"></param>
        public CudaResourceDesc(CudaDeviceVariable<uint> var)
        {
            resType = CUResourceType.Linear;
            flags = 0;
            res = new CudaResourceDescUnion();
            res.hArray = new CUarray();
            res.hMipmappedArray = new CUmipmappedArray();
            res.pitch2D = new CudaResourceDescPitch2D();
            res.linear = new CudaResourceDescLinear();

            res.linear.devPtr = var.DevicePointer;
            res.linear.format = CUArrayFormat.UnsignedInt32;
            res.linear.numChannels = 1;
            res.linear.sizeInBytes = var.SizeInBytes;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="var"></param>
        public CudaResourceDesc(CudaResourceDescLinear var)
        {
            resType = CUResourceType.Linear;
            flags = 0;
            res = new CudaResourceDescUnion();
            res.hArray = new CUarray();
            res.hMipmappedArray = new CUmipmappedArray();
            res.pitch2D = new CudaResourceDescPitch2D();
            res.linear = var;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="var"></param>
        public CudaResourceDesc(CudaResourceDescPitch2D var)
        {
            resType = CUResourceType.Pitch2D;
            flags = 0;
            res = new CudaResourceDescUnion();
            res.hArray = new CUarray();
            res.hMipmappedArray = new CUmipmappedArray();
            res.linear = new CudaResourceDescLinear();
            res.pitch2D = var;
        }
        #endregion


        /// <summary>
        /// Resource type
        /// </summary>
        public CUResourceType resType;

        /// <summary>
        /// Mimics the union in C++
        /// </summary>
        public CudaResourceDescUnion res;

        /// <summary>
        /// Flags (must be zero)
        /// </summary>
        public uint flags;
    }

    /// <summary>
    /// Mimics the union "CUDA_RESOURCE_DESC.res" in cuda.h
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct CudaResourceDescUnion
    {
        /// <summary>
        /// CUDA array
        /// </summary>
        [FieldOffset(0)]
        public CUarray hArray;

        /// <summary>
        /// CUDA mipmapped array
        /// </summary>
        [FieldOffset(0)]
        public CUmipmappedArray hMipmappedArray;

        /// <summary>
        /// Linear memory
        /// </summary>
        [FieldOffset(0)]
        public CudaResourceDescLinear linear;

        /// <summary>
        /// Linear pitched 2D memory
        /// </summary>
        [FieldOffset(0)]
        public CudaResourceDescPitch2D pitch2D;

        //In cuda header, an int[32] fixes the union size to 128 bytes, we
        //achieve the same in C# if we set at offset 124 an simple int
        [FieldOffset(31 * 4)]
        private int reserved;
    }

    /// <summary>
    /// Inner struct for CudaResourceDesc
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CudaResourceDescLinear
    {
        /// <summary>
        /// Device pointer
        /// </summary>
        public CUdeviceptr devPtr;
        /// <summary>
        /// Array format
        /// </summary>
        public CUArrayFormat format;
        /// <summary>
        /// Channels per array element
        /// </summary>
        public uint numChannels;
        /// <summary>
        /// Size in bytes
        /// </summary>
        public SizeT sizeInBytes;
    }

    /// <summary>
    /// Inner struct for CudaResourceDesc
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CudaResourceDescPitch2D
    {
        /// <summary>
        /// Device pointer
        /// </summary>
        public CUdeviceptr devPtr;
        /// <summary>
        /// Array format
        /// </summary>
        public CUArrayFormat format;
        /// <summary>
        /// Channels per array element
        /// </summary>
        public uint numChannels;
        /// <summary>
        /// Width of the array in elements
        /// </summary>
        public SizeT width;
        /// <summary>
        /// Height of the array in elements
        /// </summary>
        public SizeT height;
        /// <summary>
        /// Pitch between two rows in bytes
        /// </summary>
        public SizeT pitchInBytes;
    }

    /// <summary>
    /// Texture descriptor
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CudaTextureDescriptor
    {
        /// <summary>
        /// Creates a new CudaTextureDescriptor
        /// </summary>
        /// <param name="aAddressMode">Address modes for all dimensions</param>
        /// <param name="aFilterMode">Filter mode</param>
        /// <param name="aFlags">Flags</param>
        public CudaTextureDescriptor(CUAddressMode aAddressMode, CUFilterMode aFilterMode, CUTexRefSetFlags aFlags)
        {
            addressMode = new CUAddressMode[3];
            addressMode[0] = aAddressMode;
            addressMode[1] = aAddressMode;
            addressMode[2] = aAddressMode;

            filterMode = aFilterMode;

            flags = aFlags;
            maxAnisotropy = 0;
            mipmapFilterMode = CUFilterMode.Point;
            mipmapLevelBias = 0;
            minMipmapLevelClamp = 0;
            maxMipmapLevelClamp = 0;
            borderColor = new float[4];
            _reserved = new int[12];
        }

        /// <summary>
        /// Creates a new CudaTextureDescriptor
        /// </summary>
        /// <param name="aAddressMode">Address modes for all dimensions</param>
        /// <param name="aFilterMode">Filter mode</param>
        /// <param name="aFlags">Flags</param>
        /// <param name="aBorderColor">borderColor (array of size 4)</param>
        public CudaTextureDescriptor(CUAddressMode aAddressMode, CUFilterMode aFilterMode, CUTexRefSetFlags aFlags, float[] aBorderColor)
        {
            addressMode = new CUAddressMode[3];
            addressMode[0] = aAddressMode;
            addressMode[1] = aAddressMode;
            addressMode[2] = aAddressMode;

            filterMode = aFilterMode;

            flags = aFlags;
            maxAnisotropy = 0;
            mipmapFilterMode = CUFilterMode.Point;
            mipmapLevelBias = 0;
            minMipmapLevelClamp = 0;
            maxMipmapLevelClamp = 0;
            borderColor = new float[4];
            borderColor[0] = aBorderColor[0];
            borderColor[1] = aBorderColor[1];
            borderColor[2] = aBorderColor[2];
            borderColor[3] = aBorderColor[3];
            _reserved = new int[12];
        }

        /// <summary>
        /// Creates a new CudaTextureDescriptor
        /// </summary>
        /// <param name="aAddressMode0">Address modes for dimension 0</param>
        /// <param name="aAddressMode1">Address modes for dimension 1</param>
        /// <param name="aAddressMode2">Address modes for dimension 2</param>
        /// <param name="aFilterMode">Filter mode</param>
        /// <param name="aFlags">Flags</param>
        public CudaTextureDescriptor(CUAddressMode aAddressMode0, CUAddressMode aAddressMode1, CUAddressMode aAddressMode2, CUFilterMode aFilterMode, CUTexRefSetFlags aFlags)
        {
            addressMode = new CUAddressMode[3];
            addressMode[0] = aAddressMode0;
            addressMode[1] = aAddressMode1;
            addressMode[2] = aAddressMode2;

            filterMode = aFilterMode;

            flags = aFlags;
            maxAnisotropy = 0;
            mipmapFilterMode = CUFilterMode.Point;
            mipmapLevelBias = 0;
            minMipmapLevelClamp = 0;
            maxMipmapLevelClamp = 0;
            borderColor = new float[4];
            _reserved = new int[12];
        }

        /// <summary>
        /// Creates a new CudaTextureDescriptor
        /// </summary>
        /// <param name="aAddressMode0">Address modes for dimension 0</param>
        /// <param name="aAddressMode1">Address modes for dimension 1</param>
        /// <param name="aAddressMode2">Address modes for dimension 2</param>
        /// <param name="aFilterMode">Filter mode</param>
        /// <param name="aFlags">Flags</param>
        /// <param name="aBorderColor">borderColor (array of size 4)</param>
        public CudaTextureDescriptor(CUAddressMode aAddressMode0, CUAddressMode aAddressMode1, CUAddressMode aAddressMode2, CUFilterMode aFilterMode, CUTexRefSetFlags aFlags, float[] aBorderColor)
        {
            addressMode = new CUAddressMode[3];
            addressMode[0] = aAddressMode0;
            addressMode[1] = aAddressMode1;
            addressMode[2] = aAddressMode2;

            filterMode = aFilterMode;

            flags = aFlags;
            maxAnisotropy = 0;
            mipmapFilterMode = CUFilterMode.Point;
            mipmapLevelBias = 0;
            minMipmapLevelClamp = 0;
            maxMipmapLevelClamp = 0;
            borderColor = new float[4];
            borderColor[0] = aBorderColor[0];
            borderColor[1] = aBorderColor[1];
            borderColor[2] = aBorderColor[2];
            borderColor[3] = aBorderColor[3];
            _reserved = new int[12];
        }

        /// <summary>
        /// Creates a new CudaTextureDescriptor
        /// </summary>
        /// <param name="aAddressMode">Address modes for all dimensions</param>
        /// <param name="aFilterMode">Filter mode</param>
        /// <param name="aFlags">Flags</param>
        /// <param name="aMaxAnisotropy">Maximum anisotropy ratio. Specifies the maximum anistropy ratio to be used when doing anisotropic
        /// filtering. This value will be clamped to the range [1,16].</param>
        /// <param name="aMipmapFilterMode">Mipmap filter mode. Specifies the filter mode when the calculated mipmap level lies between
        /// two defined mipmap levels.</param>
        /// <param name="aMipmapLevelBias">Mipmap level bias. Specifies the offset to be applied to the calculated mipmap level.</param>
        /// <param name="aMinMipmapLevelClamp">Mipmap minimum level clamp. Specifies the lower end of the mipmap level range to clamp access to.</param>
        /// <param name="aMaxMipmapLevelClamp">Mipmap maximum level clamp. Specifies the upper end of the mipmap level range to clamp access to.</param>
        public CudaTextureDescriptor(CUAddressMode aAddressMode, CUFilterMode aFilterMode, CUTexRefSetFlags aFlags, uint aMaxAnisotropy, CUFilterMode aMipmapFilterMode,
            float aMipmapLevelBias, float aMinMipmapLevelClamp, float aMaxMipmapLevelClamp)
        {
            addressMode = new CUAddressMode[3];
            addressMode[0] = aAddressMode;
            addressMode[1] = aAddressMode;
            addressMode[2] = aAddressMode;

            filterMode = aFilterMode;

            flags = aFlags;
            maxAnisotropy = aMaxAnisotropy;
            mipmapFilterMode = aMipmapFilterMode;
            mipmapLevelBias = aMipmapLevelBias;
            minMipmapLevelClamp = aMinMipmapLevelClamp;
            maxMipmapLevelClamp = aMaxMipmapLevelClamp;
            borderColor = new float[4];
            _reserved = new int[12];
        }

        /// <summary>
        /// Creates a new CudaTextureDescriptor
        /// </summary>
        /// <param name="aAddressMode">Address modes for all dimensions</param>
        /// <param name="aFilterMode">Filter mode</param>
        /// <param name="aFlags">Flags</param>
        /// <param name="aMaxAnisotropy">Maximum anisotropy ratio. Specifies the maximum anistropy ratio to be used when doing anisotropic
        /// filtering. This value will be clamped to the range [1,16].</param>
        /// <param name="aMipmapFilterMode">Mipmap filter mode. Specifies the filter mode when the calculated mipmap level lies between
        /// two defined mipmap levels.</param>
        /// <param name="aMipmapLevelBias">Mipmap level bias. Specifies the offset to be applied to the calculated mipmap level.</param>
        /// <param name="aMinMipmapLevelClamp">Mipmap minimum level clamp. Specifies the lower end of the mipmap level range to clamp access to.</param>
        /// <param name="aMaxMipmapLevelClamp">Mipmap maximum level clamp. Specifies the upper end of the mipmap level range to clamp access to.</param>
        /// <param name="aBorderColor">borderColor (array of size 4)</param>
        public CudaTextureDescriptor(CUAddressMode aAddressMode, CUFilterMode aFilterMode, CUTexRefSetFlags aFlags, uint aMaxAnisotropy, CUFilterMode aMipmapFilterMode,
            float aMipmapLevelBias, float aMinMipmapLevelClamp, float aMaxMipmapLevelClamp, float[] aBorderColor)
        {
            addressMode = new CUAddressMode[3];
            addressMode[0] = aAddressMode;
            addressMode[1] = aAddressMode;
            addressMode[2] = aAddressMode;

            filterMode = aFilterMode;

            flags = aFlags;
            maxAnisotropy = aMaxAnisotropy;
            mipmapFilterMode = aMipmapFilterMode;
            mipmapLevelBias = aMipmapLevelBias;
            minMipmapLevelClamp = aMinMipmapLevelClamp;
            maxMipmapLevelClamp = aMaxMipmapLevelClamp;
            borderColor = new float[4];
            borderColor[0] = aBorderColor[0];
            borderColor[1] = aBorderColor[1];
            borderColor[2] = aBorderColor[2];
            borderColor[3] = aBorderColor[3];
            _reserved = new int[12];
        }

        /// <summary>
        /// Creates a new CudaTextureDescriptor
        /// </summary>
        /// <param name="aAddressMode0">Address modes for dimension 0</param>
        /// <param name="aAddressMode1">Address modes for dimension 1</param>
        /// <param name="aAddressMode2">Address modes for dimension 2</param>
        /// <param name="aFilterMode">Filter mode</param>
        /// <param name="aFlags">Flags</param>
        /// <param name="aMaxAnisotropy">Maximum anisotropy ratio. Specifies the maximum anistropy ratio to be used when doing anisotropic
        /// filtering. This value will be clamped to the range [1,16].</param>
        /// <param name="aMipmapFilterMode">Mipmap filter mode. Specifies the filter mode when the calculated mipmap level lies between
        /// two defined mipmap levels.</param>
        /// <param name="aMipmapLevelBias">Mipmap level bias. Specifies the offset to be applied to the calculated mipmap level.</param>
        /// <param name="aMinMipmapLevelClamp">Mipmap minimum level clamp. Specifies the lower end of the mipmap level range to clamp access to.</param>
        /// <param name="aMaxMipmapLevelClamp">Mipmap maximum level clamp. Specifies the upper end of the mipmap level range to clamp access to.</param>
        public CudaTextureDescriptor(CUAddressMode aAddressMode0, CUAddressMode aAddressMode1, CUAddressMode aAddressMode2, CUFilterMode aFilterMode, CUTexRefSetFlags aFlags, uint aMaxAnisotropy, CUFilterMode aMipmapFilterMode,
            float aMipmapLevelBias, float aMinMipmapLevelClamp, float aMaxMipmapLevelClamp)
        {
            addressMode = new CUAddressMode[3];
            addressMode[0] = aAddressMode0;
            addressMode[1] = aAddressMode1;
            addressMode[2] = aAddressMode2;

            filterMode = aFilterMode;

            flags = aFlags;
            maxAnisotropy = aMaxAnisotropy;
            mipmapFilterMode = aMipmapFilterMode;
            mipmapLevelBias = aMipmapLevelBias;
            minMipmapLevelClamp = aMinMipmapLevelClamp;
            maxMipmapLevelClamp = aMaxMipmapLevelClamp;
            borderColor = new float[4];
            _reserved = new int[12];
        }

        /// <summary>
        /// Creates a new CudaTextureDescriptor
        /// </summary>
        /// <param name="aAddressMode0">Address modes for dimension 0</param>
        /// <param name="aAddressMode1">Address modes for dimension 1</param>
        /// <param name="aAddressMode2">Address modes for dimension 2</param>
        /// <param name="aFilterMode">Filter mode</param>
        /// <param name="aFlags">Flags</param>
        /// <param name="aMaxAnisotropy">Maximum anisotropy ratio. Specifies the maximum anistropy ratio to be used when doing anisotropic
        /// filtering. This value will be clamped to the range [1,16].</param>
        /// <param name="aMipmapFilterMode">Mipmap filter mode. Specifies the filter mode when the calculated mipmap level lies between
        /// two defined mipmap levels.</param>
        /// <param name="aMipmapLevelBias">Mipmap level bias. Specifies the offset to be applied to the calculated mipmap level.</param>
        /// <param name="aMinMipmapLevelClamp">Mipmap minimum level clamp. Specifies the lower end of the mipmap level range to clamp access to.</param>
        /// <param name="aMaxMipmapLevelClamp">Mipmap maximum level clamp. Specifies the upper end of the mipmap level range to clamp access to.</param>
        /// <param name="aBorderColor">borderColor (array of size 4)</param>
        public CudaTextureDescriptor(CUAddressMode aAddressMode0, CUAddressMode aAddressMode1, CUAddressMode aAddressMode2, CUFilterMode aFilterMode, CUTexRefSetFlags aFlags, uint aMaxAnisotropy, CUFilterMode aMipmapFilterMode,
            float aMipmapLevelBias, float aMinMipmapLevelClamp, float aMaxMipmapLevelClamp, float[] aBorderColor)
        {
            addressMode = new CUAddressMode[3];
            addressMode[0] = aAddressMode0;
            addressMode[1] = aAddressMode1;
            addressMode[2] = aAddressMode2;

            filterMode = aFilterMode;

            flags = aFlags;
            maxAnisotropy = aMaxAnisotropy;
            mipmapFilterMode = aMipmapFilterMode;
            mipmapLevelBias = aMipmapLevelBias;
            minMipmapLevelClamp = aMinMipmapLevelClamp;
            maxMipmapLevelClamp = aMaxMipmapLevelClamp;
            borderColor = new float[4];
            borderColor[0] = aBorderColor[0];
            borderColor[1] = aBorderColor[1];
            borderColor[2] = aBorderColor[2];
            borderColor[3] = aBorderColor[3];
            _reserved = new int[12];
        }

        /// <summary>
        /// Address modes
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3, ArraySubType = UnmanagedType.I4)]
        public CUAddressMode[] addressMode;
        /// <summary>
        /// Filter mode
        /// </summary>
        public CUFilterMode filterMode;
        /// <summary>
        /// Flags
        /// </summary>
        public CUTexRefSetFlags flags;
        /// <summary>
        /// Maximum anisotropy ratio. Specifies the maximum anistropy ratio to be used when doing anisotropic
        /// filtering. This value will be clamped to the range [1,16].
        /// </summary>
        public uint maxAnisotropy;
        /// <summary>
        /// Mipmap filter mode. Specifies the filter mode when the calculated mipmap level lies between
        /// two defined mipmap levels.
        /// </summary>
        public CUFilterMode mipmapFilterMode;
        /// <summary>
        /// Mipmap level bias. Specifies the offset to be applied to the calculated mipmap level.
        /// </summary>
        public float mipmapLevelBias;
        /// <summary>
        /// Mipmap minimum level clamp. Specifies the lower end of the mipmap level range to clamp access to.
        /// </summary>
        public float minMipmapLevelClamp;
        /// <summary>
        /// Mipmap maximum level clamp. Specifies the upper end of the mipmap level range to clamp access to.
        /// </summary>
        public float maxMipmapLevelClamp;

        /// <summary>
        /// Border Color
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4, ArraySubType = UnmanagedType.R4)]
        public float[] borderColor;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12, ArraySubType = UnmanagedType.I4)]
        private int[] _reserved;
    }

    /// <summary>
    /// Resource view descriptor
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CudaResourceViewDesc
    {
        /// <summary>
        /// Resource view format
        /// </summary>
        public CUresourceViewFormat format;
        /// <summary>
        /// Width of the resource view
        /// </summary>
        public SizeT width;
        /// <summary>
        /// Height of the resource view
        /// </summary>
        public SizeT height;
        /// <summary>
        /// Depth of the resource view
        /// </summary>
        public SizeT depth;
        /// <summary>
        /// First defined mipmap level
        /// </summary>
        public uint firstMipmapLevel;
        /// <summary>
        /// Last defined mipmap level
        /// </summary>
        public uint lastMipmapLevel;
        /// <summary>
        /// First layer index
        /// </summary>
        public uint firstLayer;
        /// <summary>
        /// Last layer index
        /// </summary>
        public uint lastLayer;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16, ArraySubType = UnmanagedType.I4)]
        private int[] _reserved;
    }

    /// <summary>
    /// CUDA surface object
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CUsurfObject
    {
        /// <summary>
        /// 
        /// </summary>
        public ulong Pointer;
    }

    /// <summary>
    /// Profiler Output Modes
    /// </summary>
    public enum CUoutputMode
    {
        /// <summary>
        /// Output mode Key-Value pair format.
        /// </summary>
        KeyValuePair = 0x00,
        /// <summary>
        /// Output mode Comma separated values format.
        /// </summary>
        CSV = 0x01
    }

    /// <summary>
    /// External memory handle descriptor
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct CudaExternalMemoryHandleDesc
    {
        /// <summary>
        /// Type of the handle
        /// </summary>
        [FieldOffset(0)]
        public CUexternalMemoryHandleType type;

        /// <summary>
        /// 
        /// </summary>
        [FieldOffset(8)]
        public HandleUnion handle;

        /// <summary>
        /// Size of the memory allocation
        /// </summary>
        [FieldOffset(24)]
        public ulong size;

        /// <summary>
        /// Flags must either be zero or ::CUDA_EXTERNAL_MEMORY_DEDICATED
        /// </summary>
        [FieldOffset(32)]
        public CudaExternalMemory flags;

        //Original struct definition in cuda-header sets a unsigned int[16] array at the end of the struct.
        //To get the same struct size (104 bytes), we simply put an uint at FieldOffset 100.
        [FieldOffset(100)]
        private uint reserved;
    }

    /// <summary>
    /// External memory buffer descriptor
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CudaExternalMemoryBufferDesc
    {
        /// <summary>
        /// Offset into the memory object where the buffer's base is
        /// </summary>
        //[FieldOffset(0)]
        public ulong offset;
        /// <summary>
        /// Size of the buffer
        /// </summary>
        //[FieldOffset(8)]
        public ulong size;
        /// <summary>
        /// Flags reserved for future use. Must be zero.
        /// </summary>
        //[FieldOffset(16)]
        public uint flags;

        //[FieldOffset(84)] //instead of uint[16]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16, ArraySubType = UnmanagedType.U4)]
        private uint[] reserved;
    }

    /// <summary>
    /// Indicates that the layered sparse CUDA array or CUDA mipmapped array has a single mip tail region for all layers
    /// </summary>
    [Flags]
    public enum CUArraySparsePropertiesFlags : uint
    {
        /// <summary>
        /// No flags
        /// </summary>
        None = 0,

        /// <summary>
        /// Indicates that the layered sparse CUDA array or CUDA mipmapped array has a single mip tail region for all layers
        /// </summary>
        SingleMiptail = 0x1
    }

    /// <summary>
    /// CUDAArray3DFlags
    /// </summary>
    [Flags]
    public enum CUDAArray3DFlags
    {
        /// <summary>
        /// No flags
        /// </summary>
        None = 0,

        /// <summary>
        /// if set, the CUDA array contains an array of 2D slices and
        /// the Depth member of CUDA_ARRAY3D_DESCRIPTOR specifies the
        /// number of slices, not the depth of a 3D array.
        /// </summary>
        [Obsolete("Since CUDA Version 4.0. Use <Layered> instead")]
        Array2D = 1,

        /// <summary>
        /// if set, the CUDA array contains an array of layers where each layer is either a 1D
        /// or a 2D array and the Depth member of CUDA_ARRAY3D_DESCRIPTOR specifies the number
        /// of layers, not the depth of a 3D array.
        /// </summary>
        Layered = 1,

        /// <summary>
        /// this flag must be set in order to bind a surface reference
        /// to the CUDA array
        /// </summary>
        SurfaceLDST = 2,

        /// <summary>
        /// If set, the CUDA array is a collection of six 2D arrays, representing faces of a cube. The
        /// width of such a CUDA array must be equal to its height, and Depth must be six.
        /// If ::CUDA_ARRAY3D_LAYERED flag is also set, then the CUDA array is a collection of cubemaps
        /// and Depth must be a multiple of six.
        /// </summary>
        Cubemap = 4,

        /// <summary>
        /// This flag must be set in order to perform texture gather operations on a CUDA array.
        /// </summary>
        TextureGather = 8,

        /// <summary>
        /// This flag if set indicates that the CUDA array is a DEPTH_TEXTURE.
        /// </summary>
        DepthTexture = 0x10,

        /// <summary>
        /// This flag indicates that the CUDA array may be bound as a color target in an external graphics API
        /// </summary>
        ColorAttachment = 0x20,

        /// <summary>
        /// This flag if set indicates that the CUDA array or CUDA mipmapped array
        /// is a sparse CUDA array or CUDA mipmapped array respectively
        /// </summary>
        Sparse = 0x40,

        /// <summary>
        /// This flag if set indicates that the CUDA array or CUDA mipmapped array will allow deferred memory mapping
        /// </summary>
        DeferredMapping = 0x80
    }

    /// <summary>
    /// Tensor map interleave layout type
    /// </summary>
    public enum CUtensorMapInterleave
    {
        /// <summary/>
        None = 0,
        /// <summary/>
        Interleave16B,
        /// <summary/>
        Interleave32B
    }

    /// <summary>
    /// Tensor map swizzling mode of shared memory banks
    /// </summary>
    public enum CUtensorMapSwizzle
    {
        /// <summary/>
        None = 0,
        /// <summary/>
        Swizzle32B,
        /// <summary/>
        Swizzle64B,
        /// <summary/>
        Swizzle128B
    }

    /// <summary>
    /// Tensor map L2 promotion type
    /// </summary>
    public enum CUtensorMapL2promotion
    {
        /// <summary/>
        NONE = 0,
        /// <summary/>
        L2_64B,
        /// <summary/>
        L2_128B,
        /// <summary/>
        L2_256B
    }

    /// <summary>
    /// A mipmapped Cuda array
    /// </summary>
    public class CudaMipmappedArray : IDisposable
    {
        private CUmipmappedArray _mipmappedArray;
        private CUDAArray3DDescriptor _arrayDescriptor;
        private CUResult res;
        private bool disposed;
        private bool _isOwner;

        /// <summary>
        /// Creates a CUDA mipmapped array according to <c>descriptor</c>. <para/>
        /// Width, Height, and Depth are the width, height, and depth of the CUDA array (in elements); the following
        /// types of CUDA arrays can be allocated:<para/>
        /// – A 1D mipmapped array is allocated if Height and Depth extents are both zero.<para/>
        /// – A 2D mipmapped array is allocated if only Depth extent is zero.<para/>
        /// – A 3D mipmapped array is allocated if all three extents are non-zero.<para/>
        /// – A 1D layered CUDA mipmapped array is allocated if only Height is zero and the <see cref="CUDAArray3DFlags.Layered"/> 
        ///   flag is set. Each layer is a 1D array. The number of layers is determined by the depth extent.<para/>
        /// – A 2D layered CUDA mipmapped array is allocated if all three extents are non-zero and the <see cref="CUDAArray3DFlags.Layered"/> 
        ///   flag is set. Each layer is a 2D array. The number of layers is determined by the depth extent.<para/>
        /// – A cubemap CUDA mipmapped array is allocated if all three extents are non-zero and the <see cref="CUDAArray3DFlags.Cubemap"/>
        ///   flag is set. Width must be equal to Height, and Depth must be six. A
        ///   cubemap is a special type of 2D layered CUDA array, where the six layers represent the six faces of a
        ///   cube. The order of the six layers in memory is the same as that listed in CUarray_cubemap_face.<para/>
        /// – A cubemap layered CUDA mipmapped array is allocated if all three extents are non-zero, and both,
        ///   <see cref="CUDAArray3DFlags.Cubemap"/> and <see cref="CUDAArray3DFlags.Layered"/> flags are set. Width must be equal
        ///   to Height, and Depth must be a multiple of six. A cubemap layered CUDA array is a special type of
        ///   2D layered CUDA array that consists of a collection of cubemaps. The first six layers represent the first
        ///   cubemap, the next six layers form the second cubemap, and so on.<para/>
        /// Flags may be set to:<para/>
        /// – <see cref="CUDAArray3DFlags.Layered"/> to enable creation of layered CUDA mipmapped arrays. If this flag is set,
        ///   Depth specifies the number of layers, not the depth of a 3D array.<para/>
        /// – <see cref="CUDAArray3DFlags.Cubemap"/> to enable creation of mipmapped cubemaps. If this flag is set, Width
        ///   must be equal to Height, and Depth must be six. If the CUDA_ARRAY3D_LAYERED flag is also set,
        ///   then Depth must be a multiple of six.<para/>
        /// – <see cref="CUDAArray3DFlags.TextureGather"/> to indicate that the CUDA mipmapped array will be used for
        ///   texture gather. Texture gather can only be performed on 2D CUDA mipmapped arrays.
        /// </summary>
        /// <param name="descriptor">mipmapped array descriptor</param>
        /// <param name="numMipmapLevels">Number of mipmap levels. This value is clamped to the range [1, 1 + floor(log2(max(width, height, depth)))]</param>
        public CudaMipmappedArray(CUDAArray3DDescriptor descriptor, uint numMipmapLevels)
        {
            _mipmappedArray = new CUmipmappedArray();
            _arrayDescriptor = descriptor;

            res = DriverAPINativeMethods.ArrayManagement.cuMipmappedArrayCreate(ref _mipmappedArray, ref _arrayDescriptor, numMipmapLevels);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuMipmappedArrayCreate", res));
            if (res != CUResult.Success) throw new CudaException(res);
            _isOwner = true;
        }

        /// <summary>
        /// Creates a CUDA mipmapped array according to <c>descriptor</c>. <para/>
        /// Width, Height, and Depth are the width, height, and depth of the CUDA array (in elements); the following
        /// types of CUDA arrays can be allocated:<para/>
        /// – A 1D mipmapped array is allocated if Height and Depth extents are both zero.<para/>
        /// – A 2D mipmapped array is allocated if only Depth extent is zero.<para/>
        /// – A 3D mipmapped array is allocated if all three extents are non-zero.<para/>
        /// – A 1D layered CUDA mipmapped array is allocated if only Height is zero and the <see cref="CUDAArray3DFlags.Layered"/> 
        ///   flag is set. Each layer is a 1D array. The number of layers is determined by the depth extent.
        /// – A 2D layered CUDA mipmapped array is allocated if all three extents are non-zero and the <see cref="CUDAArray3DFlags.Layered"/> 
        ///   flag is set. Each layer is a 2D array. The number of layers is determined by the depth extent.
        /// – A cubemap CUDA mipmapped array is allocated if all three extents are non-zero and the <see cref="CUDAArray3DFlags.Cubemap"/>
        ///   flag is set. Width must be equal to Height, and Depth must be six. A
        ///   cubemap is a special type of 2D layered CUDA array, where the six layers represent the six faces of a
        ///   cube. The order of the six layers in memory is the same as that listed in CUarray_cubemap_face.
        /// – A cubemap layered CUDA mipmapped array is allocated if all three extents are non-zero, and both,
        ///   <see cref="CUDAArray3DFlags.Cubemap"/> and <see cref="CUDAArray3DFlags.Layered"/> flags are set. Width must be equal
        ///   to Height, and Depth must be a multiple of six. A cubemap layered CUDA array is a special type of
        ///   2D layered CUDA array that consists of a collection of cubemaps. The first six layers represent the first
        ///   cubemap, the next six layers form the second cubemap, and so on.
        /// </summary>
        /// <param name="format">Array format</param>
        /// <param name="width">Array width. See general description.</param>
        /// <param name="height">Array height. See general description.</param>
        /// <param name="depth">Array depth or layer count. See general description.</param>
        /// <param name="numChannels">number of channels</param>
        /// <param name="flags">Flags may be set to:<para/>
        /// – <see cref="CUDAArray3DFlags.Layered"/> to enable creation of layered CUDA mipmapped arrays. If this flag is set,
        ///   Depth specifies the number of layers, not the depth of a 3D array.<para/>
        /// – <see cref="CUDAArray3DFlags.Cubemap"/> to enable creation of mipmapped cubemaps. If this flag is set, Width
        ///   must be equal to Height, and Depth must be six. If the CUDA_ARRAY3D_LAYERED flag is also set,
        ///   then Depth must be a multiple of six.<para/>
        /// – <see cref="CUDAArray3DFlags.TextureGather"/> to indicate that the CUDA mipmapped array will be used for
        ///   texture gather. Texture gather can only be performed on 2D CUDA mipmapped arrays.</param>
        /// <param name="numMipmapLevels">Number of mipmap levels. This value is clamped to the range [1, 1 + floor(log2(max(width, height, depth)))]</param>
        public CudaMipmappedArray(CUArrayFormat format, SizeT width, SizeT height, SizeT depth, CudaMipmappedArrayNumChannels numChannels, CUDAArray3DFlags flags, uint numMipmapLevels)
        {
            _mipmappedArray = new CUmipmappedArray();
            _arrayDescriptor = new CUDAArray3DDescriptor();
            _arrayDescriptor.Width = width;
            _arrayDescriptor.Height = height;
            _arrayDescriptor.Depth = depth;
            _arrayDescriptor.NumChannels = (uint)numChannels;
            _arrayDescriptor.Flags = flags;
            _arrayDescriptor.Format = format;


            res = DriverAPINativeMethods.ArrayManagement.cuMipmappedArrayCreate(ref _mipmappedArray, ref _arrayDescriptor, numMipmapLevels);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuMipmappedArrayCreate", res));
            if (res != CUResult.Success) throw new CudaException(res);
            _isOwner = true;
        }

        /// <summary>
        /// Creates a CUDA mipmapped array from an existing mipmap array handle.
        /// </summary>
        /// <param name="handle">handle to wrap</param>
        /// <param name="format">Array format of the wrapped array. Cannot be gathered through CUDA API.</param>
        /// <param name="numChannels">Number of channels of wrapped array.</param>
        public CudaMipmappedArray(CUmipmappedArray handle, CUArrayFormat format, CudaMipmappedArrayNumChannels numChannels)
        {
            _mipmappedArray = handle;
            _arrayDescriptor = new CUDAArray3DDescriptor();
            _arrayDescriptor.Format = format;
            _arrayDescriptor.NumChannels = (uint)numChannels;
            _isOwner = false;
        }

        #region Dispose
        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// For IDisposable
        /// </summary>
        /// <param name="fDisposing"></param>
        protected virtual void Dispose(bool fDisposing)
        {
            if (fDisposing && !disposed)
            {
                if (_isOwner)
                {
                    res = DriverAPINativeMethods.ArrayManagement.cuMipmappedArrayDestroy(_mipmappedArray);
                    Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuMipmappedArrayDestroy", res));
                }
                disposed = true;
            }
            if (!fDisposing && !disposed)
                Debug.WriteLine(String.Format("ManagedCUDA not-disposed warning: {0}", this.GetType()));
        }
        #endregion

        #region Methods

        /// <summary>
        /// Returns a CUDA array that represents a single mipmap level
        /// of the CUDA mipmapped array.
        /// </summary>
        /// <param name="level">Mipmap level</param>
        public CUarray GetLevelAsCUArray(uint level)
        {
            CUarray array = new CUarray();

            res = DriverAPINativeMethods.ArrayManagement.cuMipmappedArrayGetLevel(ref array, _mipmappedArray, level);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuMipmappedArrayGetLevel", res));
            if (res != CUResult.Success)
                throw new CudaException(res);

            return array;
        }

        /// <summary>
        /// Returns the layout properties of a sparse CUDA mipmapped array
        /// Returns the sparse array layout properties in \p sparseProperties
        /// If the CUDA mipmapped array is not allocated with flag ::CUDA_ARRAY3D_SPARSE
        /// ::CUDA_ERROR_INVALID_VALUE will be returned.
        /// For non-layered CUDA mipmapped arrays, ::CUDA_ARRAY_SPARSE_PROPERTIES::miptailSize returns the
        /// size of the mip tail region.The mip tail region includes all mip levels whose width, height or depth
        /// is less than that of the tile.
        /// For layered CUDA mipmapped arrays, if ::CUDA_ARRAY_SPARSE_PROPERTIES::flags contains ::CU_ARRAY_SPARSE_PROPERTIES_SINGLE_MIPTAIL,
        /// then ::CUDA_ARRAY_SPARSE_PROPERTIES::miptailSize specifies the size of the mip tail of all layers combined. 
        /// Otherwise, ::CUDA_ARRAY_SPARSE_PROPERTIES::miptailSize specifies mip tail size per layer.
        /// The returned value of::CUDA_ARRAY_SPARSE_PROPERTIES::miptailFirstLevel is valid only if ::CUDA_ARRAY_SPARSE_PROPERTIES::miptailSize is non-zero.
        /// </summary>
        public CudaArraySparseProperties GetSparseProperties()
        {
            CudaArraySparseProperties sparseProperties = new CudaArraySparseProperties();

            res = DriverAPINativeMethods.ArrayManagement.cuMipmappedArrayGetSparseProperties(ref sparseProperties, _mipmappedArray);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuMipmappedArrayGetSparseProperties", res));
            if (res != CUResult.Success)
                throw new CudaException(res);

            return sparseProperties;
        }

        /// <summary>
        /// Returns the memory requirements of a CUDA array
        /// </summary>
        public CudaArrayMemoryRequirements GetMemoryRequirements(CUdevice device)
        {
            return _mipmappedArray.GetMemoryRequirements(device);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Returns the wrapped CUmipmappedArray
        /// </summary>
        public CUmipmappedArray CUMipmappedArray
        {
            get { return _mipmappedArray; }
        }

        /// <summary>
        /// Returns the wrapped CUDAArray3DDescriptor
        /// </summary>
        public CUDAArray3DDescriptor Array3DDescriptor
        {
            get { return _arrayDescriptor; }
        }

        /// <summary>
        /// Returns the Depth of the array
        /// </summary>
        public SizeT Depth
        {
            get { return _arrayDescriptor.Depth; }
        }

        /// <summary>
        /// Returns the Height of the array
        /// </summary>
        public SizeT Height
        {
            get { return _arrayDescriptor.Height; }
        }

        /// <summary>
        /// Returns the array width in elements
        /// </summary>
        public SizeT Width
        {
            get { return _arrayDescriptor.Width; }
        }

        /// <summary>
        /// Returns the array creation flags
        /// </summary>
        public CUDAArray3DFlags Flags
        {
            get { return _arrayDescriptor.Flags; }
        }

        /// <summary>
        /// Returns the array format
        /// </summary>
        public CUArrayFormat Format
        {
            get { return _arrayDescriptor.Format; }
        }

        /// <summary>
        /// Returns number of channels
        /// </summary>
        public uint NumChannels
        {
            get { return _arrayDescriptor.NumChannels; }
        }

        /// <summary>
        /// If the wrapper class instance is the owner of a CUDA handle, it will be destroyed while disposing.
        /// </summary>
        public bool IsOwner
        {
            get { return _isOwner; }
        }
        #endregion
    }

    /// <summary>
    /// Tensor map out-of-bounds fill type
    /// </summary>
    public enum CUtensorMapFloatOOBfill
    {
        /// <summary/>
        None = 0,
        /// <summary/>
        NanRequestZeroFMA
    }

    /// <summary>
    /// Resource view format
    /// </summary>
    public enum CUresourceViewFormat
    {
        /// <summary>
        /// No resource view format (use underlying resource format)
        /// </summary>
        None = 0x00,
        /// <summary>
        /// 1 channel unsigned 8-bit integers
        /// </summary>
        Uint1X8 = 0x01,
        /// <summary>
        /// 2 channel unsigned 8-bit integers
        /// </summary>
        Uint2X8 = 0x02,
        /// <summary>
        /// 4 channel unsigned 8-bit integers
        /// </summary>
        Uint4X8 = 0x03,
        /// <summary>
        /// 1 channel signed 8-bit integers
        /// </summary>
        Sint1X8 = 0x04,
        /// <summary>
        /// 2 channel signed 8-bit integers
        /// </summary>
        Sint2X8 = 0x05,
        /// <summary>
        /// 4 channel signed 8-bit integers
        /// </summary>
        Sint4X8 = 0x06,
        /// <summary>
        /// 1 channel unsigned 16-bit integers
        /// </summary>
        Uint1X16 = 0x07,
        /// <summary>
        /// 2 channel unsigned 16-bit integers
        /// </summary>
        Uint2X16 = 0x08,
        /// <summary>
        /// 4 channel unsigned 16-bit integers
        /// </summary>
        Uint4X16 = 0x09,
        /// <summary>
        /// 1 channel signed 16-bit integers
        /// </summary>
        Sint1X16 = 0x0a,
        /// <summary>
        /// 2 channel signed 16-bit integers
        /// </summary>
        Sint2X16 = 0x0b,
        /// <summary>
        /// 4 channel signed 16-bit integers
        /// </summary>
        Sint4X16 = 0x0c,
        /// <summary>
        /// 1 channel unsigned 32-bit integers
        /// </summary>
        Uint1X32 = 0x0d,
        /// <summary>
        /// 2 channel unsigned 32-bit integers
        /// </summary>
        Uint2X32 = 0x0e,
        /// <summary>
        /// 4 channel unsigned 32-bit integers 
        /// </summary>
        Uint4X32 = 0x0f,
        /// <summary>
        /// 1 channel signed 32-bit integers
        /// </summary>
        Sint1X32 = 0x10,
        /// <summary>
        /// 2 channel signed 32-bit integers
        /// </summary>
        Sint2X32 = 0x11,
        /// <summary>
        /// 4 channel signed 32-bit integers
        /// </summary>
        Sint4X32 = 0x12,
        /// <summary>
        /// 1 channel 16-bit floating point
        /// </summary>
        Float1X16 = 0x13,
        /// <summary>
        /// 2 channel 16-bit floating point
        /// </summary>
        Float2X16 = 0x14,
        /// <summary>
        /// 4 channel 16-bit floating point
        /// </summary>
        Float4X16 = 0x15,
        /// <summary>
        /// 1 channel 32-bit floating point
        /// </summary>
        Float1X32 = 0x16,
        /// <summary>
        /// 2 channel 32-bit floating point
        /// </summary>
        Float2X32 = 0x17,
        /// <summary>
        /// 4 channel 32-bit floating point
        /// </summary>
        Float4X32 = 0x18,
        /// <summary>
        /// Block compressed 1 
        /// </summary>
        UnsignedBC1 = 0x19,
        /// <summary>
        /// Block compressed 2
        /// </summary>
        UnsignedBC2 = 0x1a,
        /// <summary>
        /// Block compressed 3 
        /// </summary>
        UnsignedBC3 = 0x1b,
        /// <summary>
        /// Block compressed 4 unsigned
        /// </summary>
        UnsignedBC4 = 0x1c,
        /// <summary>
        /// Block compressed 4 signed 
        /// </summary>
        SignedBC4 = 0x1d,
        /// <summary>
        /// Block compressed 5 unsigned
        /// </summary>
        UnsignedBC5 = 0x1e,
        /// <summary>
        /// Block compressed 5 signed
        /// </summary>
        SignedBC5 = 0x1f,
        /// <summary>
        /// Block compressed 6 unsigned half-float
        /// </summary>
        UnsignedBC6H = 0x20,
        /// <summary>
        /// Block compressed 6 signed half-float
        /// </summary>
        SignedBC6H = 0x21,
        /// <summary>
        /// Block compressed 7 
        /// </summary>
        UnsignedBC7 = 0x22
    }

    /// <summary>
    /// External memory handle types
    /// </summary>
    public enum CUexternalMemoryHandleType
    {
        /// <summary>
        /// Handle is an opaque file descriptor
        /// </summary>
        OpaqueFD = 1,
        /// <summary>
        /// Handle is an opaque shared NT handle
        /// </summary>
        OpaqueWin32 = 2,
        /// <summary>
        /// Handle is an opaque, globally shared handle
        /// </summary>
        OpaqueWin32KMT = 3,
        /// <summary>
        /// Handle is a D3D12 heap object
        /// </summary>
        D3D12Heap = 4,
        /// <summary>
        /// Handle is a D3D12 committed resource
        /// </summary>
        D3D12Resource = 5,
        /// <summary>
        /// Handle is a shared NT handle to a D3D11 resource
        /// </summary>
        D3D11Resource = 6,
        /// <summary>
        /// Handle is a globally shared handle to a D3D11 resource
        /// </summary>
        D3D11ResourceKMT = 7,
        /// <summary>
        /// Handle is an NvSciBuf object
        /// </summary>
        NvSciBuf = 8
    }

    /// <summary>
    /// </summary>
    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
    public struct HandleUnion
    {
        /// <summary>
        /// File descriptor referencing the memory object. Valid when type is CUDA_EXTERNAL_MEMORY_DEDICATED
        /// </summary>
        [FieldOffset(0)]
        public int fd;

        /// <summary>
        /// Win32 handle referencing the semaphore object.
        /// </summary>
        [FieldOffset(0)]
        public Win32Handle win32;

        /// <summary>
        /// A handle representing an NvSciBuf Object.Valid when type
        /// is ::CU_EXTERNAL_MEMORY_HANDLE_TYPE_NVSCIBUF
        /// </summary>
        [FieldOffset(0)]
        public IntPtr nvSciBufObject;
    }

    /// <summary>
    /// Indicates that the external memory object is a dedicated resource
    /// </summary>
    [Flags]
    public enum CudaExternalMemory
    {
        /// <summary>
        /// No flags
        /// </summary>
        Nothing = 0x0,
        /// <summary>
        /// Indicates that the external memory object is a dedicated resource
        /// </summary>
        Dedicated = 0x01,
    }

    /// <summary>
    /// Number of channels in array
    /// </summary>
    public enum CudaMipmappedArrayNumChannels
    {
        /// <summary>
        /// One channel, e.g. float1, int1, float, int
        /// </summary>
        One = 1,
        /// <summary>
        /// Two channels, e.g. float2, int2
        /// </summary>
        Two = 2,
        /// <summary>
        /// Four channels, e.g. float4, int4
        /// </summary>
        Four = 4
    }

    /// <summary>
    ///  Win32 handle referencing the semaphore object. Valid when
    ///  type is one of the following: <para/>
    ///  - ::CU_EXTERNAL_MEMORY_HANDLE_TYPE_OPAQUE_WIN32<para/>
    ///  - ::CU_EXTERNAL_MEMORY_HANDLE_TYPE_OPAQUE_WIN32_KMT<para/>
    ///  - ::CU_EXTERNAL_MEMORY_HANDLE_TYPE_D3D12_HEAP<para/>
    ///  - ::CU_EXTERNAL_MEMORY_HANDLE_TYPE_D3D12_RESOURCE<para/>
    ///  Exactly one of 'handle' and 'name' must be non-NULL. If
    ///  type is 
    ///  ::CU_EXTERNAL_SEMAPHORE_HANDLE_TYPE_OPAQUE_WIN32_KMT
    ///   then 'name' must be NULL.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
    public struct Win32Handle
    {
        /// <summary>
        /// Valid NT handle. Must be NULL if 'name' is non-NULL
        /// </summary>
        [FieldOffset(0)]
        public IntPtr handle;
        /// <summary>
        /// Name of a valid memory object. Must be NULL if 'handle' is non-NULL.
        /// </summary>
        [FieldOffset(8)]
        [MarshalAs(UnmanagedType.LPStr)]
        public string name;
    }

    /// <summary>
    /// External memory mipmap descriptor
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CudaExternalMemoryMipmappedArrayDesc
    {
        /// <summary>
        /// Offset into the memory object where the base level of the mipmap chain is.
        /// </summary>
        public ulong offset;
        /// <summary>
        /// Format, dimension and type of base level of the mipmap chain
        /// </summary>
        public CUDAArray3DDescriptor arrayDesc;
        /// <summary>
        /// Total number of levels in the mipmap chain
        /// </summary>
        public uint numLevels;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16, ArraySubType = UnmanagedType.U4)]
        private uint[] reserved;
    }

    /// <summary>
    /// External semaphore handle descriptor
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct CudaExternalSemaphoreHandleDesc
    {
        /// <summary>
        /// Type of the handle
        /// </summary>
        [FieldOffset(0)]
        public CUexternalSemaphoreHandleType type;

        /// <summary>
        /// 
        /// </summary>
        [FieldOffset(8)]
        public HandleUnion handle;
        /// <summary>
        /// Flags reserved for the future. Must be zero.
        /// </summary>
        [FieldOffset(32)]
        public uint flags;

        //Original struct definition in cuda-header sets a unsigned int[16] array at the end of the struct.
        //To get the same struct size (96 bytes), we simply put an uint at FieldOffset 92.
        [FieldOffset(92)]
        private uint reserved;
    }

    /// <summary>
    /// External semaphore handle types
    /// </summary>
    public enum CUexternalSemaphoreHandleType
    {
        /// <summary>
        /// Handle is an opaque file descriptor
        /// </summary>
        OpaqueFD = 1,
        /// <summary>
        /// Handle is an opaque shared NT handle
        /// </summary>
        OpaqueWin32 = 2,
        /// <summary>
        /// Handle is an opaque, globally shared handle
        /// </summary>
        OpaqueWin32KMT = 3,
        /// <summary>
        /// Handle is a shared NT handle referencing a D3D12 fence object
        /// </summary>
        D3D12DFence = 4,
        /// <summary>
        /// Handle is a shared NT handle referencing a D3D11 fence object
        /// </summary>
        D3D11Fence = 5,
        /// <summary>
        /// Opaque handle to NvSciSync Object
        /// </summary>
        NvSciSync = 6,
        /// <summary>
        /// Handle is a shared NT handle referencing a D3D11 keyed mutex object
        /// </summary>
        D3D11KeyedMutex = 7,
        /// <summary>
        /// Handle is a globally shared handle referencing a D3D11 keyed mutex object
        /// </summary>
        D3D11KeyedMutexKMT = 8,
        /// <summary>
        /// Handle is an opaque file descriptor referencing a timeline semaphore
        /// </summary>
        TimelineSemaphoreFD = 9,
        /// <summary>
        /// Handle is an opaque shared NT handle referencing a timeline semaphore
        /// </summary>
        TimelineSemaphoreWin32 = 10
    }

    /// <summary>
    /// Flags for instantiating a graph
    /// </summary>
    [Flags]
    public enum CUgraphInstantiate_flags : ulong
    {
        /// <summary/>
        None = 0,
        /// <summary>
        /// Automatically free memory allocated in a graph before relaunching.
        /// </summary>
        AutoFreeOnLaunch = 1,
        /// <summary>
        /// Automatically upload the graph after instantiaton.
        /// </summary>
        Upload = 2,
        /// <summary>
        /// Instantiate the graph to be launchable from the device.
        /// </summary>
        DeviceLaunch = 4,
        /// <summary>
        /// Run the graph using the per-node priority attributes rather than the priority of the stream it is launched into.
        /// </summary>
        UseNodePriority = 8
    }

    /// <summary>
    /// Graph instantiation parameters
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CudaGraphInstantiateParams
    {
        /// <summary>
        /// Instantiation flags
        /// </summary>
        public ulong flags;
        /// <summary>
        /// Upload stream
        /// </summary>
        public CUstream hUploadStream;
        /// <summary>
        /// The node which caused instantiation to fail, if any
        /// </summary>
        public CUgraphNode hErrNode_out;
        /// <summary>
        /// Whether instantiation was successful.  If it failed, the reason why
        /// </summary>
        public CUgraphInstantiateResult result_out;
    }

    /// <summary>
    /// Result information returned by cuGraphExecUpdate
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CUgraphExecUpdateResultInfo
    {
        /// <summary>
        /// Gives more specific detail when a cuda graph update fails.
        /// </summary>
        public CUgraphExecUpdateResult result;

        /// <summary>
        /// The "to node" of the error edge when the topologies do not match.
        /// The error node when the error is associated with a specific node.
        /// NULL when the error is generic.
        /// </summary>
        public CUgraphNode errorNode;

        /// <summary>
        /// The from node of error edge when the topologies do not match. Otherwise NULL.
        /// </summary>
        public CUgraphNode errorFromNode;
    }

    /// <summary>
    /// 
    /// </summary>
    public enum CUgraphExecUpdateResult
    {
        /// <summary>
        /// The update succeeded
        /// </summary>
        Success = 0x0,
        /// <summary>
        /// The update failed for an unexpected reason which is described in the return value of the function
        /// </summary>
        Error = 0x1,
        /// <summary>
        /// The update failed because the topology changed
        /// </summary>
        ErrorTopologyChanged = 0x2,
        /// <summary>
        /// The update failed because a node type changed
        /// </summary>
        ErrorNodeTypeChanged = 0x3,
        /// <summary>
        /// The update failed because the function of a kernel node changed
        /// </summary>
        ErrorFunctionChanged = 0x4,
        /// <summary>
        /// The update failed because the parameters changed in a way that is not supported
        /// </summary>
        ErrorParametersChanged = 0x5,
        /// <summary>
        /// The update failed because something about the node is not supported
        /// </summary>
        ErrorNotSupported = 0x6,
        /// <summary>
        /// The update failed because the function of a kernel node changed in an unsupported way
        /// </summary>
        ErrorUnsupportedFunctionChange = 0x7,
        /// <summary>
        /// The update failed because the node attributes changed in a way that is not supported
        /// </summary>
        ErrorAttributesChanged = 0x8
    }

    /// <summary>
    /// Graph instantiation results
    /// </summary>
    public enum CUgraphInstantiateResult
    {
        /// <summary>
        /// Instantiation succeeded
        /// </summary>
        Success = 0,
        /// <summary>
        /// Instantiation failed for an unexpected reason which is described in the return value of the function
        /// </summary>
        Error = 1,
        /// <summary>
        /// Instantiation failed due to invalid structure, such as cycles 
        /// </summary>
        InvalidStructure = 2,
        /// <summary>
        /// Instantiation for device launch failed because the graph contained an unsupported operation
        /// </summary>
        NodeOperationNotSupported = 3,
        /// <summary>
        /// Instantiation for device launch failed due to the nodes belonging to different contexts
        /// </summary>
        MultipleCtxsNotSupported = 4
    }

    /// <summary>
    /// The additional write options for ::cuGraphDebugDotPrint
    /// </summary>
    [Flags]
    public enum CUgraphDebugDot_flags
    {
        /// <summary/>
        None = 0,
        /// <summary>
        /// Output all debug data as if every debug flag is enabled 
        /// </summary>
        Verbose = 1 << 0,
        /// <summary>
        /// Use CUDA Runtime structures for output
        /// </summary>
        RuntimeTypes = 1 << 1,
        /// <summary>
        /// Adds CUDA_KERNEL_NODE_PARAMS values to output
        /// </summary>
        KernelNodeParams = 1 << 2,
        /// <summary>
        /// Adds CUDA_MEMCPY3D values to output
        /// </summary>
        MemcpyNodeParams = 1 << 3,
        /// <summary>
        /// Adds CUDA_MEMSET_NODE_PARAMS values to output 
        /// </summary>
        MemsetNodeParams = 1 << 4,
        /// <summary>
        /// Adds CUDA_HOST_NODE_PARAMS values to output
        /// </summary>
        HostNodeParams = 1 << 5,
        /// <summary>
        /// Adds CUevent handle from record and wait nodes to output
        /// </summary>
        EventNodeParams = 1 << 6,
        /// <summary>
        /// Adds CUDA_EXT_SEM_SIGNAL_NODE_PARAMS values to output
        /// </summary>
        ExtSemasSignalNodeParams = 1 << 7,
        /// <summary>
        /// Adds CUDA_EXT_SEM_WAIT_NODE_PARAMS values to output
        /// </summary>
        ExtSemasWaitNodeParams = 1 << 8,
        /// <summary>
        /// Adds CUkernelNodeAttrValue values to output
        /// </summary>
        KernelNodeAttributes = 1 << 9,
        /// <summary>
        /// Adds node handles and every kernel function handle to output
        /// </summary>
        Handles = 1 << 10,
        /// <summary>
        /// Adds memory alloc node parameters to output
        /// </summary>
        MemAllocNodeParams = 1 << 11,
        /// <summary>
        /// Adds memory free node parameters to output
        /// </summary>
        MemFreeNodeParams = 1 << 12,
        /// <summary>
        /// Adds batch mem op node parameters to output
        /// </summary>
        BatchMemOpNodeParams = 1 << 13,
        /// <summary>
        /// Adds edge numbering information
        /// </summary>
        ExtraTopoInfo = 1 << 14

    }

    /// <summary>
    /// Flags for user objects for graphs
    /// </summary>
    [Flags]
    public enum CUuserObject_flags
    {
        /// <summary/>
        None = 0,
        /// <summary>
        /// Indicates the destructor execution is not synchronized by any CUDA handle. 
        /// </summary>
        NoDestructorSync = 1
    }

    /// <summary>
    /// Flags for retaining user object references for graphs
    /// </summary>
    [Flags]
    public enum CUuserObjectRetain_flags
    {
        /// <summary/>
        None = 0,
        /// <summary>
        /// Transfer references from the caller rather than creating new references.
        /// </summary>
        Move = 1
    }

    /// <summary>
    /// The targets for ::cuFlushGPUDirectRDMAWrites
    /// </summary>
    public enum CUflushGPUDirectRDMAWritesTarget
    {
        /// <summary>
        /// Sets the target for ::cuFlushGPUDirectRDMAWrites() to the currently active CUDA device context.
        /// </summary>
        CurrentCtx = 0
    }

    /// <summary>
    /// The scopes for ::cuFlushGPUDirectRDMAWrites
    /// </summary>
    public enum CUflushGPUDirectRDMAWritesScope
    {
        /// <summary>
        /// Blocks until remote writes are visible to the CUDA device context owning the data.
        /// </summary>
        WritesToOwner = 100,
        /// <summary>
        /// Blocks until remote writes are visible to all CUDA device contexts.
        /// </summary>
        WritesToAllDevices = 200
    }

    /// <summary>
    /// Define a common interface for all CUDA vector types
    /// See http://blogs.msdn.com/b/ricom/archive/2006/09/07/745085.aspx why
    /// these vector types look like they are.
    /// </summary>
    public interface ICudaVectorType
    {
        /// <summary>
        /// Gives the size of this type in bytes. <para/>
        /// Is equal to <c>Marshal.SizeOf(this);</c>
        /// </summary>
        uint Size
        {
            get;
        }
    }

    // <summary>
    /// Define a common interface for all CUDA vector types supported by CudaArrays
    /// </summary>
    public interface ICudaVectorTypeForArray
    {
        /// <summary>
        /// Returns the Channel number from vector type, e.g. 3 for float3
        /// </summary>
        /// <returns></returns>
        uint GetChannelNumber();

        /// <summary>
        /// Returns a matching CUArrayFormat. If none is availabe a CudaException is thrown.
        /// </summary>
        /// <returns></returns>
        CUArrayFormat GetCUArrayFormat();
    }

    /// <summary>
    /// Platform native ordering for GPUDirect RDMA writes
    /// </summary>
    public enum CUGPUDirectRDMAWritesOrdering
    {
        /// <summary>
        /// The device does not natively support ordering of remote writes. ::cuFlushGPUDirectRDMAWrites() can be leveraged if supported.
        /// </summary>
        None = 0,
        /// <summary>
        /// Natively, the device can consistently consume remote writes, although other CUDA devices may not.
        /// </summary>
        Owner = 100,
        /// <summary>
        /// Any CUDA device in the system can consistently consume remote writes to this device.
        /// </summary>
        AllDevices = 200
    }

    /// <summary>
    /// Bitmasks for ::CU_DEVICE_ATTRIBUTE_GPU_DIRECT_RDMA_FLUSH_WRITES_OPTIONS
    /// </summary>
    [Flags]
    public enum CUflushGPUDirectRDMAWritesOptions
    {
        /// <summary/>
        None = 0,
        /// <summary>
        /// ::cuFlushGPUDirectRDMAWrites() and its CUDA Runtime API counterpart are supported on the device.
        /// </summary>
        Host = 1 << 0,
        /// <summary>
        /// The ::CU_STREAM_WAIT_VALUE_FLUSH flag and the ::CU_STREAM_MEM_OP_FLUSH_REMOTE_WRITES MemOp are supported on the device.
        /// </summary>
        Memops = 1 << 1
    }

    /// <summary>
    /// Compute mode that device is currently in.
    /// </summary>
    public enum CUComputeMode
    {
        /// <summary>
        /// Default mode - Device is not restricted and can have multiple CUDA
        /// contexts present at a single time.
        /// </summary>
        Default = 0,

        ///// <summary>
        ///// Compute-exclusive mode - Device can have only one CUDA context
        ///// present on it at a time.
        ///// </summary>
        //Exclusive = 1,

        /// <summary>
        /// Compute-prohibited mode - Device is prohibited from creating
        /// new CUDA contexts.
        /// </summary>
        Prohibited = 2,

        /// <summary>
        /// Compute-exclusive-process mode (Only one context used by a 
        /// single process can be present on this device at a time)
        /// </summary>
        ExclusiveProcess = 2
    }
}
