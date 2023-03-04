using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace BrightData.Cuda.CudaToolkit
{
    [Serializable]
    internal class CudaException : Exception
    {
        CuResult _cudaError;
        Lazy<string?> _internalName, _internalDescription;

        public CudaException(CuResult error)
            : base(GetErrorMessageFromCuResult(error))
        {
            _cudaError = error;
            _internalDescription = new(() => GetInternalDescriptionFromCuResult(error));
            _internalName = new(() => GetInternalNameFromCuResult(error));
        }
        public CudaException(string message)
            : base(message)
        {
            _internalDescription = new("<none>");
            _internalName = new("<none>");
        }
        public CudaException(CuResult error, string message, Exception? exception)
            : base(message, exception)
        {
            this._cudaError = error;
            _internalDescription = new(() => GetInternalDescriptionFromCuResult(error));
            _internalName = new(() => GetInternalNameFromCuResult(error));
        }
        public override string ToString()
        {
            return CudaError.ToString();
        }
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("CudaError", this._cudaError);
        }

        static string GetErrorMessageFromCuResult(CuResult error)
        {
            var message = error switch {
                CuResult.Success                          => "No error.",
                CuResult.ErrorInvalidValue                => "This indicates that one or more of the parameters passed to the API call is not within an acceptable range of values.",
                CuResult.ErrorOutOfMemory                 => "The API call failed because it was unable to allocate enough memory to perform the requested operation.",
                CuResult.ErrorNotInitialized              => "The CUDA driver API is not yet initialized. Call cuInit(Flags) before any other driver API call.",
                CuResult.ErrorDeinitialized               => "This indicates that the CUDA driver is in the process of shutting down.",
                CuResult.ErrorProfilerDisabled            => "This indicates profiling APIs are called while application is running in visual profiler mode.",
                CuResult.ErrorStubLibrary                 => "This indicates that the CUDA driver that the application has loaded is a stub library. Applications that run with the stub rather than a real driver loaded will result in CUDA API returning this error.",
                CuResult.ErrorDeviceUnavailable           => "This indicates that requested CUDA device is unavailable at the current time. Devices are often unavailable due to use of ::CU_COMPUTEMODE_EXCLUSIVE_PROCESS or ::CU_COMPUTEMODE_PROHIBITED.",
                CuResult.ErrorNoDevice                    => "This indicates that no CUDA-capable devices were detected by the installed CUDA driver.",
                CuResult.ErrorInvalidDevice               => "This indicates that the device ordinal supplied by the user does not correspond to a valid CUDA device or that the action requested is invalid for the specified device.",
                CuResult.DeviceNotLicensed                => "This error indicates that the Grid license is not applied.",
                CuResult.ErrorInvalidImage                => "This indicates that the device kernel image is invalid. This can also indicate an invalid CUDA module.",
                CuResult.ErrorInvalidContext              => "This most frequently indicates that there is no context bound to the current thread. This can also be returned if the context passed to an API call is not a valid handle (such as a context that has had cuCtxDestroy() invoked on it). This can also be returned if a user mixes different API versions (i.e. 3010 context with 3020 API calls). See cuCtxGetApiVersion() for more details.",
                CuResult.ErrorMapFailed                   => "This indicates that a map or register operation has failed.",
                CuResult.ErrorUnmapFailed                 => "This indicates that an unmap or unregister operation has failed.",
                CuResult.ErrorArrayIsMapped               => "This indicates that the specified array is currently mapped and thus cannot be destroyed.",
                CuResult.ErrorAlreadyMapped               => "This indicates that the resource is already mapped.",
                CuResult.ErrorNoBinaryForGpu              => "This indicates that there is no kernel image available that is suitable for the device. This can occur when a user specifies code generation options for a particular CUDA source file that do not include the corresponding device configuration.",
                CuResult.ErrorAlreadyAcquired             => "This indicates that a resource has already been acquired.",
                CuResult.ErrorNotMapped                   => "This indicates that a resource is not mapped.",
                CuResult.ErrorNotMappedAsArray            => "This indicates that a mapped resource is not available for access as an array.",
                CuResult.ErrorNotMappedAsPointer          => "This indicates that a mapped resource is not available for access as a pointer.",
                CuResult.ErrorEccUncorrectable            => "This indicates that an uncorrectable ECC error was detected during execution.",
                CuResult.ErrorUnsupportedLimit            => "This indicates that the CUlimit passed to the API call is not supported by the active device.",
                CuResult.ErrorContextAlreadyInUse         => "This indicates that the ::CUcontext passed to the API call can only be bound to a single CPU thread at a time but is already bound to a CPU thread.",
                CuResult.ErrorPeerAccessUnsupported       => "This indicates that peer access is not supported across the given devices.",
                CuResult.ErrorInvalidPtx                  => "This indicates that a PTX JIT compilation failed.",
                CuResult.ErrorInvalidGraphicsContext      => "This indicates an error with OpenGL or DirectX context.",
                CuResult.NvLinkUncorrectable              => "This indicates that an uncorrectable NVLink error was detected during the execution.",
                CuResult.JitCompilerNotFound              => "This indicates that the PTX JIT compiler library was not found.",
                CuResult.UnsupportedPtxVersion            => "This indicates that the provided PTX was compiled with an unsupported toolchain.",
                CuResult.JitCompilationDisabled           => "This indicates that the PTX JIT compilation was disabled.",
                CuResult.UnsupportedExecAffinity          => "This indicates that the ::CUexecAffinityType passed to the API call is not supported by the active device.",
                CuResult.ErrorInvalidSource               => "This indicates that the device kernel source is invalid. This includes compilation/linker errors encountered in device code or user error.",
                CuResult.ErrorFileNotFound                => "This indicates that the file specified was not found.",
                CuResult.ErrorSharedObjectSymbolNotFound  => "This indicates that a link to a shared object failed to resolve.",
                CuResult.ErrorSharedObjectInitFailed      => "This indicates that initialization of a shared object failed.",
                CuResult.ErrorOperatingSystem             => "This indicates that an OS call failed.",
                CuResult.ErrorInvalidHandle               => "This indicates that a resource handle passed to the API call was not valid. Resource handles are opaque types like CUstream and CUevent.",
                CuResult.ErrorNotFound                    => "This indicates that a named symbol was not found. Examples of symbols are global/constant variable names, texture names, and surface names.",
                CuResult.ErrorNotReady                    => "This indicates that asynchronous operations issued previously have not completed yet. This result is not actually an error, but must be indicated differently than CUDA_SUCCESS (which indicates completion). Calls that may return this value include cuEventQuery() and cuStreamQuery().",
                CuResult.ErrorIllegalAddress              => "While executing a kernel, the device encountered a load or store instruction on an invalid memory address.\nThis leaves the process in an inconsistent state and any further CUDA work will return the same error.\nTo continue using CUDA, the process must be terminated and relaunched.",
                CuResult.ErrorLaunchOutOfResources        => "This indicates that a launch did not occur because it did not have appropriate resources. This error usually indicates that the user has attempted to pass too many arguments to the device kernel, or the kernel launch specifies too many threads for the kernel's register count. Passing arguments of the wrong size (i.e. a 64-bit pointer when a 32-bit int is expected) is equivalent to passing too many arguments and can also result in this error.",
                CuResult.ErrorLaunchTimeout               => "This indicates that the device kernel took too long to execute. This can only occur if timeouts are enabled - see the device attribute CU_DEVICE_ATTRIBUTE_KERNEL_EXEC_TIMEOUT for more information. This leaves the process in an inconsistent state and any further CUDA work will return the same error.\nTo continue using CUDA, the process must be terminated and relaunched.",
                CuResult.ErrorLaunchIncompatibleTexturing => "This error indicates a kernel launch that uses an incompatible texturing mode.",
                CuResult.ErrorPeerAccessAlreadyEnabled    => "This error indicates that a call to ::cuCtxEnablePeerAccess() is trying to re-enable peer access to a context which has already had peer access to it enabled.",
                CuResult.ErrorPeerAccessNotEnabled        => "This error indicates that ::cuCtxDisablePeerAccess() is trying to disable peer access which has not been enabled yet via ::cuCtxEnablePeerAccess().",
                CuResult.ErrorPrimaryContextActice        => "This error indicates that the primary context for the specified device has already been initialized.",
                CuResult.ErrorContextIsDestroyed          => "This error indicates that the context current to the calling thread has been destroyed using ::cuCtxDestroy, or is a primary context which has not yet been initialized.",
                CuResult.ErrorAssert                      => "A device-side assert triggered during kernel execution. This leaves the process in an inconsistent state and any further CUDA work will return the same error.\nTo continue using CUDA, the process must be terminated and relaunched.",
                CuResult.ErrorTooManyPeers                => "This error indicates that the hardware resources required to enable peer access have been exhausted for one or more of the devices passed to cuCtxEnablePeerAccess().",
                CuResult.ErrorHostMemoryAlreadyRegistered => "This error indicates that the memory range passed to cuMemHostRegister() has already been registered.",
                CuResult.ErrorHostMemoryNotRegistered     => "This error indicates that the pointer passed to cuMemHostUnregister() does not correspond to any currently registered memory region.",
                CuResult.ErrorHardwareStackError          => "While executing a kernel, the device encountered a stack error.\nThis can be due to stack corruption or exceeding the stack size limit.\nThis leaves the process in an inconsistent state and any further CUDA work will return the same error.\nTo continue using CUDA, the process must be terminated and relaunched.",
                CuResult.ErrorIllegalInstruction          => "While executing a kernel, the device encountered an illegal instruction.\nThis leaves the process in an inconsistent state and any further CUDA work will return the same error.\nTo continue using CUDA, the process must be terminated and relaunched.",
                CuResult.ErrorMisalignedAddress           => "While executing a kernel, the device encountered a load or store instruction on a memory address which is not aligned.\nThis leaves the process in an inconsistent state and any further CUDA work will return the same error.\nTo continue using CUDA, the process must be terminated and relaunched.",
                CuResult.ErrorInvalidAddressSpace         => "While executing a kernel, the device encountered an instruction which can only operate on memory locations in certain address spaces (global, shared, or local), but was supplied a memory address not belonging to an allowed address space.\nThis leaves the process in an inconsistent state and any further CUDA work will return the same error.\nTo continue using CUDA, the process must be terminated and relaunched.",
                CuResult.ErrorInvalidPc                   => "While executing a kernel, the device program counter wrapped its address space.\nThis leaves the process in an inconsistent state and any further CUDA work will return the same error.\nTo continue using CUDA, the process must be terminated and relaunched.",
                CuResult.ErrorLaunchFailed                => "An exception occurred on the device while executing a kernel. Common causes include dereferencing an invalid device pointer and accessing out of bounds shared memory.\nThis leaves the process in an inconsistent state and any further CUDA work will return the same error.\nTo continue using CUDA, the process must be terminated and relaunched.",
                CuResult.ErrorCooperativeLaunchTooLarge   => "This error indicates that the number of blocks launched per grid for a kernel that was launched via either ::cuLaunchCooperativeKernel or ::cuLaunchCooperativeKernelMultiDevice exceeds the maximum number of blocks as allowed by ::cuOccupancyMaxActiveBlocksPerMultiprocessor or ::cuOccupancyMaxActiveBlocksPerMultiprocessorWithFlags times the number of multiprocessors as specified by the device attribute ::CU_DEVICE_ATTRIBUTE_MULTIPROCESSOR_COUNT.",
                CuResult.ErrorNotPermitted                => "This error indicates that the attempted operation is not permitted.",
                CuResult.ErrorNotSupported                => "This error indicates that the attempted operation is not supported on the current system or device.",
                CuResult.ErrorSystemNotReady              => "This error indicates that the system is not yet ready to start any CUDA work.  To continue using CUDA, verify the system configuration is in a valid state and all required driver daemons are actively running.",
                CuResult.ErrorSystemDriverMismatch        => "This error indicates that there is a mismatch between the versions of the display driver and the CUDA driver. Refer to the compatibility documentation for supported versions.",
                CuResult.ErrorCompatNotSupportedOnDevice  => "This error indicates that the system was upgraded to run with forward compatibility but the visible hardware detected by CUDA does not support this configuration. Refer to the compatibility documentation for the supported hardware matrix or ensure that only supported hardware is visible during initialization via the CUDA_VISIBLE_DEVICES environment variable.",
                CuResult.MpsConnectionFailed              => "This error indicates that the MPS client failed to connect to the MPS control daemon or the MPS server.",
                CuResult.MpsRpcFailure                    => "This error indicates that the remote procedural call between the MPS server and the MPS client failed.",
                CuResult.MpsServerNotReady                => "This error indicates that the MPS server is not ready to accept new MPS client requests. This error can be returned when the MPS server is in the process of recovering from a fatal failure.",
                CuResult.MpsMaxClientsReached             => "This error indicates that the hardware resources required to create MPS client have been exhausted.",
                CuResult.MpsMaxConnectionsReached         => "This error indicates the the hardware resources required to support device connections have been exhausted.",
                CuResult.ErrorMpsClinetTerminated         => "This error indicates that the MPS client has been terminated by the server. To continue using CUDA, the process must be terminated and relaunched.",
                CuResult.ErrorCdpNotSupported             => "This error indicates that the module is using CUDA Dynamic Parallelism, but the current configuration, like MPS, does not support it.",
                CuResult.ErrorCdpVersionMismatch          => "This error indicates that a module contains an unsupported interaction between different versions of CUDA Dynamic Parallelism.",
                CuResult.ErrorStreamCaptureUnsupported    => "This error indicates that the operation is not permitted when the stream is capturing.",
                CuResult.ErrorStreamCaptureInvalidated    => "This error indicates that the current capture sequence on the stream has been invalidated due to a previous error.",
                CuResult.ErrorStreamCaptureMerge          => "This error indicates that the operation would have resulted in a merge of two independent capture sequences.",
                CuResult.ErrorStreamCaptureUnmatched      => "This error indicates that the capture was not initiated in this stream.",
                CuResult.ErrorStreamCaptureUnjoined       => "This error indicates that the capture sequence contains a fork that was not joined to the primary stream.",
                CuResult.ErrorStreamCaptureIsolation      => "This error indicates that a dependency would have been created which crosses the capture sequence boundary. Only implicit in-stream ordering dependencies are allowed to cross the boundary.",
                CuResult.ErrorStreamCaptureImplicit       => "This error indicates a disallowed implicit dependency on a current capture sequence from cudaStreamLegacy.",
                CuResult.ErrorCapturedEvent               => "This error indicates that the operation is not permitted on an event which was last recorded in a capturing stream.",
                CuResult.ErrorStreamCaptureWrongThread    => "A stream capture sequence not initiated with the ::CU_STREAM_CAPTURE_MODE_RELAXED argument to ::cuStreamBeginCapture was passed to ::cuStreamEndCapture in a different thread.",
                CuResult.ErrorTimeOut                     => "This error indicates that the timeout specified for the wait operation has lapsed.",
                CuResult.ErrorGraphExecUpdateFailure      => "This error indicates that the graph update was not performed because it included changes which violated constraints specific to instantiated graph update.",
                CuResult.ErrorExternalDevice              => "This indicates that an async error has occurred in a device outside of CUDA. If CUDA was waiting for an external device's signal before consuming shared data, the external device signaled an error indicating that the data is not valid for consumption. This leaves the process in an inconsistent state and any further CUDA work will return the same error. To continue using CUDA, the process must be terminated and relaunched.",
                CuResult.ErrorInvalidClusterSize          => "Indicates a kernel launch error due to cluster misconfiguration.",
                CuResult.ErrorUnknown                     => "This indicates that an unknown internal error has occurred.",
                _                                         => string.Empty
            };

            return error.ToString() + ": " + message;
        }

        static string? GetInternalNameFromCuResult(CuResult error)
        {
            var name = new nint();
            DriverApiNativeMethods.ErrorHandling.cuGetErrorName(error, ref name);
            var val = Marshal.PtrToStringAnsi(name);
            return val;
        }

        static string? GetInternalDescriptionFromCuResult(CuResult error)
        {
            var desc = new nint();
            DriverApiNativeMethods.ErrorHandling.cuGetErrorString(error, ref desc);
            var val = Marshal.PtrToStringAnsi(desc);
            return val;
        }
        public CuResult CudaError
        {
            get => _cudaError;
            set => _cudaError = value;
        }
        public string? CudaInternalErrorName => _internalName.Value;
        public string? CudaInternalErrorDescription => _internalDescription.Value;
    }
}