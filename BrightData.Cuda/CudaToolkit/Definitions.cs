using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace BrightData.Cuda.CudaToolkit
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CUfunction
    {
        public IntPtr Pointer;
        public CUmodule GetModule()
        {
            var temp = new CUmodule();
            var res = DriverApiNativeMethods.FunctionManagement.cuFuncGetModule(ref temp, this);
            
            if (res != CuResult.Success) throw new CudaException(res);
            return temp;
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CUstream
    {
        public IntPtr Pointer;
        public static CUstream NullStream
        {
            get
            {
                var s = new CUstream();
                s.Pointer = 0;
                return s;
            }
        }
        public static CUstream LegacyStream
        {
            get
            {
                var s = new CUstream();
                s.Pointer = 1;
                return s;
            }
        }
        public static CUstream StreamPerThread
        {
            get
            {
                var s = new CUstream();
                s.Pointer = 2;
                return s;
            }
        }
        public ulong Id
        {
            get
            {
                ulong ret = 0;
                var res = DriverApiNativeMethods.Streams.cuStreamGetId(this, ref ret);
                
                if (res != CuResult.Success) throw new CudaException(res);
                return ret;
            }
        }
    }
    public enum CuResult
    {
        Success = 0,
        ErrorInvalidValue = 1,
        ErrorOutOfMemory = 2,
        ErrorNotInitialized = 3,
        ErrorDeinitialized = 4,
        ErrorProfilerDisabled = 5,
        [Obsolete("deprecated as of CUDA 5.0")]
        ErrorProfilerNotInitialized = 6,
        [Obsolete("deprecated as of CUDA 5.0")]
        ErrorProfilerAlreadyStarted = 7,
        [Obsolete("deprecated as of CUDA 5.0")]
        ErrorProfilerAlreadyStopped = 8,
        ErrorStubLibrary = 34,
        ErrorDeviceUnavailable = 46,
        ErrorNoDevice = 100,
        ErrorInvalidDevice = 101,
        DeviceNotLicensed = 102,
        ErrorInvalidImage = 200,
        ErrorInvalidContext = 201,
        [Obsolete("This error return is deprecated as of CUDA 3.2. It is no longer an error to attempt to push the active context via cuCtxPushCurrent().")]
        ErrorContextAlreadyCurrent = 202,
        ErrorMapFailed = 205,
        ErrorUnmapFailed = 206,
        ErrorArrayIsMapped = 207,
        ErrorAlreadyMapped = 208,
        ErrorNoBinaryForGpu = 209,
        ErrorAlreadyAcquired = 210,
        ErrorNotMapped = 211,
        ErrorNotMappedAsArray = 212,
        ErrorNotMappedAsPointer = 213,
        ErrorEccUncorrectable = 214,
        ErrorUnsupportedLimit = 215,
        ErrorContextAlreadyInUse = 216,
        ErrorPeerAccessUnsupported = 217,
        ErrorInvalidPtx = 218,
        ErrorInvalidGraphicsContext = 219,
        NvLinkUncorrectable = 220,
        JitCompilerNotFound = 221,
        UnsupportedPtxVersion = 222,
        JitCompilationDisabled = 223,
        UnsupportedExecAffinity = 224,
        ErrorInvalidSource = 300,
        ErrorFileNotFound = 301,
        ErrorSharedObjectSymbolNotFound = 302,
        ErrorSharedObjectInitFailed = 303,
        ErrorOperatingSystem = 304,
        ErrorInvalidHandle = 400,
        ErrorIllegalState = 401,
        ErrorNotFound = 500,
        ErrorNotReady = 600,
        ErrorIllegalAddress = 700,
        ErrorLaunchOutOfResources = 701,
        ErrorLaunchTimeout = 702,
        ErrorLaunchIncompatibleTexturing = 703,
        ErrorPeerAccessAlreadyEnabled = 704,
        ErrorPeerAccessNotEnabled = 705,
        ErrorPrimaryContextActice = 708,
        ErrorContextIsDestroyed = 709,
        ErrorAssert = 710,
        ErrorTooManyPeers = 711,
        ErrorHostMemoryAlreadyRegistered = 712,
        ErrorHostMemoryNotRegistered = 713,
        ErrorHardwareStackError = 714,
        ErrorIllegalInstruction = 715,
        ErrorMisalignedAddress = 716,
        ErrorInvalidAddressSpace = 717,
        ErrorInvalidPc = 718,
        ErrorLaunchFailed = 719,
        ErrorCooperativeLaunchTooLarge = 720,
        ErrorNotPermitted = 800,
        ErrorNotSupported = 801,
        ErrorSystemNotReady = 802,
        ErrorSystemDriverMismatch = 803,
        ErrorCompatNotSupportedOnDevice = 804,
        MpsConnectionFailed = 805,
        MpsRpcFailure = 806,
        MpsServerNotReady = 807,
        MpsMaxClientsReached = 808,
        MpsMaxConnectionsReached = 809,
        ErrorMpsClinetTerminated = 810,
        ErrorCdpNotSupported = 811,
        ErrorCdpVersionMismatch = 812,
        ErrorStreamCaptureUnsupported = 900,
        ErrorStreamCaptureInvalidated = 901,
        ErrorStreamCaptureMerge = 902,
        ErrorStreamCaptureUnmatched = 903,
        ErrorStreamCaptureUnjoined = 904,
        ErrorStreamCaptureIsolation = 905,
        ErrorStreamCaptureImplicit = 906,
        ErrorCapturedEvent = 907,
        ErrorStreamCaptureWrongThread = 908,
        ErrorTimeOut = 909,
        ErrorGraphExecUpdateFailure = 910,
        ErrorExternalDevice = 911,
        ErrorInvalidClusterSize = 912,
        ErrorUnknown = 999,
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CUdevice
    {
        public int Pointer;
        public void SetMemoryPool(CUmemoryPool memPool)
        {
            var res = DriverApiNativeMethods.DeviceManagement.cuDeviceSetMemPool(this, memPool);
            
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public CUmemoryPool GetMemoryPool()
        {
            var ret = new CUmemoryPool();
            DriverApiNativeMethods.DeviceManagement.cuDeviceGetDefaultMemPool(ref ret, this);
            return ret;
        }
        public CUmemoryPool GetDefaultMemoryPool()
        {
            var ret = new CUmemoryPool();
            DriverApiNativeMethods.DeviceManagement.cuDeviceGetDefaultMemPool(ref ret, this);
            return ret;
        }
        public CUuuid Uuid
        {
            get
            {
                var uuid = new CUuuid();
                var res = DriverApiNativeMethods.DeviceManagement.cuDeviceGetUuid_v2(ref uuid, this);
                
                if (res != CuResult.Success) throw new CudaException(res);
                return uuid;
            }
        }
        public bool GetExecAffinitySupport(CUexecAffinityType type)
        {
            var pi = 0;
            var res = DriverApiNativeMethods.DeviceManagement.cuDeviceGetExecAffinitySupport(ref pi, type, this);
            
            if (res != CuResult.Success) throw new CudaException(res);
            return pi > 0;
        }
        public void GraphMemTrim()
        {
            var res = DriverApiNativeMethods.GraphManagment.cuDeviceGraphMemTrim(this);
            
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public void SetGraphMemAttribute(CUgraphMemAttribute attr, ulong value)
        {
            var res = DriverApiNativeMethods.GraphManagment.cuDeviceSetGraphMemAttribute(this, attr, ref value);
            
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public ulong GetGraphMemAttribute(CUgraphMemAttribute attr)
        {
            ulong value = 0;
            var res = DriverApiNativeMethods.GraphManagment.cuDeviceGetGraphMemAttribute(this, attr, ref value);
            
            if (res != CuResult.Success) throw new CudaException(res);
            return value;
        }
        public static bool operator ==(CUdevice src, CUdevice value)
        {
            return src.Pointer == value.Pointer;
        }
        public static bool operator !=(CUdevice src, CUdevice value)
        {
            return src.Pointer != value.Pointer;
        }
        public override bool Equals(object? obj)
        {
            if (!(obj is CUdevice)) return false;

            var value = (CUdevice)obj;

            return this.Pointer.Equals(value.Pointer);
        }
        public override int GetHashCode()
        {
            return Pointer.GetHashCode();
        }
        public override string ToString()
        {
            return Pointer.ToString();
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct SizeT
    {
        UIntPtr value;
        public SizeT(int value)
        {
            this.value = new UIntPtr((uint)value);
        }
        public SizeT(uint value)
        {
            this.value = new UIntPtr(value);
        }
        public SizeT(long value)
        {
            this.value = new UIntPtr((ulong)value);
        }
        public SizeT(ulong value)
        {
            this.value = new UIntPtr(value);
        }
        public SizeT(UIntPtr value)
        {
            this.value = value;
        }
        public SizeT(IntPtr value)
        {
            this.value = new UIntPtr((ulong)value.ToInt64());
        }
        public static implicit operator int(SizeT t)
        {
            return (int)t.value.ToUInt32();
        }
        public static implicit operator uint(SizeT t)
        {
            return (t.value.ToUInt32());
        }
        public static implicit operator long(SizeT t)
        {
            return (long)t.value.ToUInt64();
        }
        public static implicit operator ulong(SizeT t)
        {
            return (t.value.ToUInt64());
        }
        public static implicit operator UIntPtr(SizeT t)
        {
            return t.value;
        }
        public static implicit operator IntPtr(SizeT t)
        {
            return new IntPtr((long)t.value.ToUInt64());
        }
        public static implicit operator SizeT(int value)
        {
            return new SizeT(value);
        }
        public static implicit operator SizeT(uint value)
        {
            return new SizeT(value);
        }
        public static implicit operator SizeT(long value)
        {
            return new SizeT(value);
        }
        public static implicit operator SizeT(ulong value)
        {
            return new SizeT(value);
        }
        public static implicit operator SizeT(IntPtr value)
        {
            return new SizeT(value);
        }
        public static implicit operator SizeT(UIntPtr value)
        {
            return new SizeT(value);
        }
        public static bool operator !=(SizeT val1, SizeT val2)
        {
            return (val1.value != val2.value);
        }
        public static bool operator ==(SizeT val1, SizeT val2)
        {
            return (val1.value == val2.value);
        }
        public static SizeT operator +(SizeT val1, SizeT val2)
        {
            return new SizeT(val1.value.ToUInt64() + val2.value.ToUInt64());
        }
        public static SizeT operator +(SizeT val1, int val2)
        {
            return new SizeT(val1.value.ToUInt64() + (ulong)val2);
        }
        public static SizeT operator +(int val1, SizeT val2)
        {
            return new SizeT((ulong)val1 + val2.value.ToUInt64());
        }
        public static SizeT operator +(uint val1, SizeT val2)
        {
            return new SizeT(val1 + val2.value.ToUInt64());
        }
        public static SizeT operator +(SizeT val1, uint val2)
        {
            return new SizeT(val1.value.ToUInt64() + val2);
        }
        public static SizeT operator -(SizeT val1, SizeT val2)
        {
            return new SizeT(val1.value.ToUInt64() - val2.value.ToUInt64());
        }
        public static SizeT operator -(SizeT val1, int val2)
        {
            return new SizeT(val1.value.ToUInt64() - (ulong)val2);
        }
        public static SizeT operator -(int val1, SizeT val2)
        {
            return new SizeT((ulong)val1 - val2.value.ToUInt64());
        }
        public static SizeT operator -(SizeT val1, uint val2)
        {
            return new SizeT(val1.value.ToUInt64() - val2);
        }
        public static SizeT operator -(uint val1, SizeT val2)
        {
            return new SizeT(val1 - val2.value.ToUInt64());
        }
        public static SizeT operator *(SizeT val1, SizeT val2)
        {
            return new SizeT(val1.value.ToUInt64() * val2.value.ToUInt64());
        }
        public static SizeT operator *(SizeT val1, int val2)
        {
            return new SizeT(val1.value.ToUInt64() * (ulong)val2);
        }
        public static SizeT operator *(int val1, SizeT val2)
        {
            return new SizeT((ulong)val1 * val2.value.ToUInt64());
        }
        public static SizeT operator *(SizeT val1, uint val2)
        {
            return new SizeT(val1.value.ToUInt64() * val2);
        }
        public static SizeT operator *(uint val1, SizeT val2)
        {
            return new SizeT(val1 * val2.value.ToUInt64());
        }
        public static SizeT operator /(SizeT val1, SizeT val2)
        {
            return new SizeT(val1.value.ToUInt64() / val2.value.ToUInt64());
        }
        public static SizeT operator /(SizeT val1, int val2)
        {
            return new SizeT(val1.value.ToUInt64() / (ulong)val2);
        }
        public static SizeT operator /(int val1, SizeT val2)
        {
            return new SizeT((ulong)val1 / val2.value.ToUInt64());
        }
        public static SizeT operator /(SizeT val1, uint val2)
        {
            return new SizeT(val1.value.ToUInt64() / val2);
        }
        public static SizeT operator /(uint val1, SizeT val2)
        {
            return new SizeT(val1 / val2.value.ToUInt64());
        }
        public static bool operator >(SizeT val1, SizeT val2)
        {
            return val1.value.ToUInt64() > val2.value.ToUInt64();
        }
        public static bool operator >(SizeT val1, int val2)
        {
            return val1.value.ToUInt64() > (ulong)val2;
        }
        public static bool operator >(int val1, SizeT val2)
        {
            return (ulong)val1 > val2.value.ToUInt64();
        }
        public static bool operator >(SizeT val1, uint val2)
        {
            return val1.value.ToUInt64() > val2;
        }
        public static bool operator >(uint val1, SizeT val2)
        {
            return val1 > val2.value.ToUInt64();
        }
        public static bool operator <(SizeT val1, SizeT val2)
        {
            return val1.value.ToUInt64() < val2.value.ToUInt64();
        }
        public static bool operator <(SizeT val1, int val2)
        {
            return val1.value.ToUInt64() < (ulong)val2;
        }
        public static bool operator <(int val1, SizeT val2)
        {
            return (ulong)val1 < val2.value.ToUInt64();
        }
        public static bool operator <(SizeT val1, uint val2)
        {
            return val1.value.ToUInt64() < val2;
        }
        public static bool operator <(uint val1, SizeT val2)
        {
            return val1 < val2.value.ToUInt64();
        }
        public override bool Equals(object? obj) => obj is SizeT sizeT && value.Equals(sizeT.value);
        public override string ToString()
        {
            if (IntPtr.Size == 4)
                return this.value.ToUInt32().ToString();
            else
                return this.value.ToUInt64().ToString();
        }
        public override int GetHashCode()
        {
            return this.value.GetHashCode();
        }
    }
    public enum CublasStatus
    {
        Success = 0,
        NotInitialized = 1,
        AllocFailed = 3,
        InvalidValue = 7,
        ArchMismatch = 8,
        MappingError = 11,
        ExecutionFailed = 13,
        InternalError = 14,
        NotSupported = 15,
        LicenseError = 16
    }
    [Flags]
    public enum CUmemAllocationHandleType
    {
        None = 0,
        PosixFileDescriptor = 0x1,
        Win32 = 0x2,
        Win32Kmt = 0x4
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CUcontext
    {
        public IntPtr Pointer;
    }
    public enum CuMemoryType : uint
    {
        Host = 0x01,
        Device = 0x02,
        Array = 0x03,
        Unified = 4
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CudaPointerAttributeP2PTokens
    {
        ulong p2pToken;
        uint vaSpaceToken;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CUdeviceptr
    {
        public SizeT Pointer;
        public static implicit operator ulong(CUdeviceptr src)
        {
            return src.Pointer;
        }
        public static explicit operator CUdeviceptr(SizeT src)
        {
            var udeviceptr = new CUdeviceptr();
            udeviceptr.Pointer = src;
            return udeviceptr;
        }
        public static CUdeviceptr operator +(CUdeviceptr src, SizeT value)
        {
            var udeviceptr = new CUdeviceptr();
            udeviceptr.Pointer = src.Pointer + value;
            return udeviceptr;
        }
        public static CUdeviceptr operator -(CUdeviceptr src, SizeT value)
        {
            var udeviceptr = new CUdeviceptr();
            udeviceptr.Pointer = src.Pointer - value;
            return udeviceptr;
        }
        public static bool operator ==(CUdeviceptr src, CUdeviceptr value)
        {
            return src.Pointer == value.Pointer;
        }
        public static bool operator !=(CUdeviceptr src, CUdeviceptr value)
        {
            return src.Pointer != value.Pointer;
        }

        public override bool Equals(object? obj) => obj is CUdeviceptr value && Pointer.Equals(value.Pointer);
        public override int GetHashCode() => base.GetHashCode();
        public override string ToString()
        {
            return Pointer.ToString();
        }
        public CUdeviceptr(SizeT pointer)
        {
            Pointer = pointer;
        }
        public CUcontext AttributeContext
        {
            get
            {
                var ret = new CUcontext();
                var res = DriverApiNativeMethods.MemoryManagement.cuPointerGetAttribute(ref ret, CuPointerAttribute.Context, this);
                
                if (res != CuResult.Success) throw new CudaException(res);
                return ret;
            }
        }
        public CuMemoryType AttributeMemoryType
        {
            get
            {
                var ret = new CuMemoryType();
                var res = DriverApiNativeMethods.MemoryManagement.cuPointerGetAttribute(ref ret, CuPointerAttribute.MemoryType, this);
                
                if (res != CuResult.Success) throw new CudaException(res);
                return ret;
            }
        }
        public CUdeviceptr AttributeDevicePointer
        {
            get
            {
                var ret = new CUdeviceptr();
                var res = DriverApiNativeMethods.MemoryManagement.cuPointerGetAttribute(ref ret, CuPointerAttribute.DevicePointer, this);
                
                if (res != CuResult.Success) throw new CudaException(res);
                return ret;
            }
        }
        public IntPtr AttributeHostPointer
        {
            get
            {
                var ret = new IntPtr();
                var res = DriverApiNativeMethods.MemoryManagement.cuPointerGetAttribute(ref ret, CuPointerAttribute.HostPointer, this);
                
                if (res != CuResult.Success) throw new CudaException(res);
                return ret;
            }
        }
        public CudaPointerAttributeP2PTokens AttributeP2PTokens
        {
            get
            {
                var ret = new CudaPointerAttributeP2PTokens();
                var res = DriverApiNativeMethods.MemoryManagement.cuPointerGetAttribute(ref ret, CuPointerAttribute.P2PTokens, this);
                
                if (res != CuResult.Success) throw new CudaException(res);
                return ret;
            }
        }
        public bool AttributeSyncMemops
        {
            get
            {
                var ret = 0;
                var res = DriverApiNativeMethods.MemoryManagement.cuPointerGetAttribute(ref ret, CuPointerAttribute.SyncMemops, this);
                
                if (res != CuResult.Success) throw new CudaException(res);
                return ret != 0;
            }
            set
            {
                var val = value ? 1 : 0;
                var res = DriverApiNativeMethods.MemoryManagement.cuPointerSetAttribute(ref val, CuPointerAttribute.SyncMemops, this);
                
                if (res != CuResult.Success) throw new CudaException(res);
            }
        }
        public ulong AttributeBufferId
        {
            get
            {
                ulong ret = 0;
                var res = DriverApiNativeMethods.MemoryManagement.cuPointerGetAttribute(ref ret, CuPointerAttribute.BufferId, this);
                
                if (res != CuResult.Success) throw new CudaException(res);
                return ret;
            }
        }
        public bool AttributeIsManaged
        {
            get
            {
                var ret = 0;
                var res = DriverApiNativeMethods.MemoryManagement.cuPointerGetAttribute(ref ret, CuPointerAttribute.IsManaged, this);
                
                if (res != CuResult.Success) throw new CudaException(res);
                return ret != 0;
            }
        }
        public int AttributeDeviceOrdinal
        {
            get
            {
                var ret = 0;
                var res = DriverApiNativeMethods.MemoryManagement.cuPointerGetAttribute(ref ret, CuPointerAttribute.DeviceOrdinal, this);
                
                if (res != CuResult.Success) throw new CudaException(res);
                return ret;
            }
        }
        public bool AttributeIsLegacyCudaIpcCapable
        {
            get
            {
                var ret = 0;
                var res = DriverApiNativeMethods.MemoryManagement.cuPointerGetAttribute(ref ret, CuPointerAttribute.IsLegacyCudaIpcCapable, this);
                
                if (res != CuResult.Success) throw new CudaException(res);
                return ret != 0;
            }
        }
        public CUdeviceptr AttributeRangeStartAddr
        {
            get
            {
                var ret = new CUdeviceptr();
                var res = DriverApiNativeMethods.MemoryManagement.cuPointerGetAttribute(ref ret, CuPointerAttribute.RangeStartAddr, this);
                
                if (res != CuResult.Success) throw new CudaException(res);
                return ret;
            }
        }
        public SizeT AttributeRangeSize
        {
            get
            {
                ulong ret = 0;
                var res = DriverApiNativeMethods.MemoryManagement.cuPointerGetAttribute(ref ret, CuPointerAttribute.RangeSize, this);
                
                if (res != CuResult.Success) throw new CudaException(res);
                return ret;
            }
        }
        public bool AttributeMapped
        {
            get
            {
                var ret = 0;
                var res = DriverApiNativeMethods.MemoryManagement.cuPointerGetAttribute(ref ret, CuPointerAttribute.Mapped, this);
                
                if (res != CuResult.Success) throw new CudaException(res);
                return ret != 0;
            }
        }
        public CUmemAllocationHandleType AttributeAllowedHandleTypes
        {
            get
            {
                var ret = 0;
                var res = DriverApiNativeMethods.MemoryManagement.cuPointerGetAttribute(ref ret, CuPointerAttribute.AllowedHandleTypes, this);
                
                if (res != CuResult.Success) throw new CudaException(res);
                return (CUmemAllocationHandleType)ret;
            }
        }
        public bool AttributeIsGpuDirectRdmaCapable
        {
            get
            {
                var ret = 0;
                var res = DriverApiNativeMethods.MemoryManagement.cuPointerGetAttribute(ref ret, CuPointerAttribute.IsGpuDirectRdmaCapable, this);
                
                if (res != CuResult.Success) throw new CudaException(res);
                return ret != 0;
            }
        }
        public bool AttributeAccessFlags
        {
            get
            {
                var ret = 0;
                var res = DriverApiNativeMethods.MemoryManagement.cuPointerGetAttribute(ref ret, CuPointerAttribute.AccessFlags, this);
                
                if (res != CuResult.Success) throw new CudaException(res);
                return ret != 0;
            }
        }
        public CUmemoryPool AttributeMempoolHandle
        {
            get
            {
                var temp = new IntPtr();
                var ret = new CUmemoryPool();
                var res = DriverApiNativeMethods.MemoryManagement.cuPointerGetAttribute(ref temp, CuPointerAttribute.MempoolHandle, this);
                
                if (res != CuResult.Success) throw new CudaException(res);
                ret.Pointer = temp;
                return ret;
            }
        }
        public SizeT AttributeMappingSize
        {
            get
            {
                ulong ret = 0;
                var res = DriverApiNativeMethods.MemoryManagement.cuPointerGetAttribute(ref ret, CuPointerAttribute.MappingSize, this);
                
                if (res != CuResult.Success) throw new CudaException(res);
                return ret;
            }
        }
        public IntPtr AttributeBaseAddr
        {
            get
            {
                var ret = new IntPtr();
                var res = DriverApiNativeMethods.MemoryManagement.cuPointerGetAttribute(ref ret, CuPointerAttribute.BaseAddr, this);
                
                if (res != CuResult.Success) throw new CudaException(res);
                return ret;
            }
        }
        public ulong AttributeMemoryBlockId
        {
            get
            {
                ulong ret = 0;
                var res = DriverApiNativeMethods.MemoryManagement.cuPointerGetAttribute(ref ret, CuPointerAttribute.MemoryBlockId, this);
                
                if (res != CuResult.Success) throw new CudaException(res);
                return ret;
            }
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CUmemoryPool
    {
        public IntPtr Pointer;
    }
    public enum CuPointerAttribute
    {
        Context = 1,
        MemoryType = 2,
        DevicePointer = 3,
        HostPointer = 4,
        P2PTokens = 5,
        SyncMemops = 6,
        BufferId = 7,
        IsManaged = 8,
        DeviceOrdinal = 9,
        IsLegacyCudaIpcCapable = 10,
        RangeStartAddr = 11,
        RangeSize = 12,
        Mapped = 13,
        AllowedHandleTypes = 14,
        IsGpuDirectRdmaCapable = 15,
        AccessFlags = 16,
        MempoolHandle = 17,
        MappingSize = 18,
        BaseAddr = 19,
        MemoryBlockId = 20
    }
    [Flags]
    public enum CuStreamFlags : uint
    {
        None = 0,
        Default = 0x0,
        NonBlocking = 0x1,
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CUevent
    {
        public IntPtr Pointer;
    }
    public delegate void CUstreamCallback(CUstream hStream, CuResult status, IntPtr userData);
    [Flags]
    public enum CuStreamAddCallbackFlags
    {
        None = 0x0,
    }
    [Flags]
    public enum CUstreamWaitValueFlags
    {
        Geq = 0x0,
        Eq = 0x1,
        And = 0x2,
        NOr = 0x3,
        Flush = 1 << 30
    }
    [Flags]
    public enum CUstreamWriteValueFlags
    {
        Default = 0x0,
        NoMemoryBarrier = 0x1
    }
    public enum CUstreamAttrId
    {
        AccessPolicyWindow = 1,
        SynchronizationPolicy = 3
    }
    [StructLayout(LayoutKind.Explicit)]
    public struct CUstreamAttrValue
    {
        [FieldOffset(0)] public CUaccessPolicyWindow accessPolicyWindow;
        [FieldOffset(0)] public CUsynchronizationPolicy syncPolicy;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CUaccessPolicyWindow
    {
        public IntPtr base_ptr;
        public SizeT num_bytes;
        public float hitRatio;
        public CUaccessProperty hitProp;
        public CUaccessProperty missProp;
    }
    public enum CUsynchronizationPolicy
    {
        Auto = 1,
        Spin = 2,
        Yield = 3,
        BlockingSync = 4
    }
    public enum CUaccessProperty
    {
        Normal = 0,
        Streaming = 1,
        Persisting = 2
    }
    public enum CUmoduleLoadingMode
    {
        EagerLoading = 0x1,
        LazyLoading = 0x2,
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CUmodule
    {
        public IntPtr Pointer;
        public static CUmoduleLoadingMode GetLoadingMode
        {
            get
            {
                var ret = new CUmoduleLoadingMode();
                var res = DriverApiNativeMethods.ModuleManagement.cuModuleGetLoadingMode(ref ret);
                
                if (res != CuResult.Success) throw new CudaException(res);
                return ret;
            }
        }
    }
    public enum CUmemPoolAttribute
    {
        ReuseFollowEventDependencies = 1,
        ReuseAllowOpportunistic,
        ReuseAllowInternalDependencies,
        ReleaseThreshold,
        ReservedMemCurrent,
        ReservedMemHigh,
        UsedMemCurrent,
        UsedMemHigh
    }
    public enum CUexecAffinityType
    {
        SmCount = 0,
        Max
    }
    public enum CUgraphMemAttribute
    {
        UsedMemCurrent,
        UsedMemHigh,
        ReservedMemCurrent,
        ReservedMemHigh
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CUuuid
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16, ArraySubType = UnmanagedType.I1)]
        public byte[] bytes;
    }
    public enum CUlaunchAttributeId
    {
        Ignore = 0,
        AccessPolicyWindow = 1,
        Cooperative = 2,
        SynchronizationPolicy = 3,
        ClusterDimension = 4,
        ClusterSchedulingPolicyPreference = 5,
        ProgrammaticStreamSerialization = 6,
        ProgrammaticEvent = 7,
        Priority = 8,
        MemSyncDomainMap = 9,
        MemSyncDomain = 10
    }
    public struct CUlaunchAttribute
    {
        public CUlaunchAttributeId Id;
        public int Pad;
        public CUlaunchAttributeValue Value;
    }
    public struct CUlaunchConfig
    {
        public uint GridDimX;
        public uint GridDimY;
        public uint GridDimZ;
        public uint BlockDimX;
        public uint BlockDimY;
        public uint BlockDimZ;
        public uint SharedMemBytes;
        public CUstream HStream;
        public CUlaunchAttribute[]? Attrs;
    }
    [StructLayout(LayoutKind.Explicit)]
    public struct CUlaunchAttributeValue
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct ClusterDim
        {
            public uint x;
            public uint y;
            public uint z;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct ProgrammaticEvent
        {
            public CUevent event_;
            public int flags;
            public int triggerAtBlockStart;
        }
        [FieldOffset(0)] CUaccessPolicyWindow accessPolicyWindow;
        [FieldOffset(0)] int cooperative;
        [FieldOffset(0)] CUsynchronizationPolicy syncPolicy;
        [FieldOffset(0)] ClusterDim clusterDim;
        [FieldOffset(0)] CUclusterSchedulingPolicy clusterSchedulingPolicyPreference;
        [FieldOffset(0)] int programmaticStreamSerializationAllowed;
        [FieldOffset(0)] ProgrammaticEvent programmaticEvent;
        [FieldOffset(0)] int priority;
        [FieldOffset(0)] CUlaunchMemSyncDomainMap memSyncDomainMap;
        [FieldOffset(0)] CUlaunchMemSyncDomain memSyncDomain;
        [FieldOffset(60)] public int pad;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CUlaunchMemSyncDomainMap
    {
        public byte default_;
        public byte remote;
    }
    public enum CUlaunchMemSyncDomain
    {
        Default = 0,
        Remote = 1
    }
    public enum CUclusterSchedulingPolicy
    {
        Default = 0,
        Spread = 1,
        LoadBalancing = 2
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CUlibrary
    {
        public IntPtr Pointer;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CUkernel
    {
        public IntPtr Pointer;
        public static explicit operator CUfunction(CUkernel cukernel)
        {
            var ret = new CUfunction();
            ret.Pointer = cukernel.Pointer;
            return ret;
        }
        public CUfunction GetCUfunction()
        {
            var ret = new CUfunction();
            var res = DriverApiNativeMethods.LibraryManagement.cuKernelGetFunction(ref ret, this);
            
            if (res != CuResult.Success) throw new CudaException(res);
            return ret;
        }
        public int GetMaxThreadsPerBlock(CUdevice device)
        {
            var temp = 0;
            var res = DriverApiNativeMethods.LibraryManagement.cuKernelGetAttribute(ref temp, CuFunctionAttribute.MaxThreadsPerBlock, this, device);
            
            if (res != CuResult.Success) throw new CudaException(res);
            return temp;
        }
        public int GetSharedMemory(CUdevice device)
        {
            var temp = 0;
            var res = DriverApiNativeMethods.LibraryManagement.cuKernelGetAttribute(ref temp, CuFunctionAttribute.SharedSizeBytes, this, device);
            
            if (res != CuResult.Success) throw new CudaException(res);
            return temp;
        }
        public int GetConstMemory(CUdevice device)
        {
            var temp = 0;
            var res = DriverApiNativeMethods.LibraryManagement.cuKernelGetAttribute(ref temp, CuFunctionAttribute.ConstSizeBytes, this, device);
            
            if (res != CuResult.Success) throw new CudaException(res);
            return temp;
        }
        public int GetLocalMemory(CUdevice device)
        {
            var temp = 0;
            var res = DriverApiNativeMethods.LibraryManagement.cuKernelGetAttribute(ref temp, CuFunctionAttribute.LocalSizeBytes, this, device);
            
            if (res != CuResult.Success) throw new CudaException(res);
            return temp;
        }
        public int GetRegisters(CUdevice device)
        {
            var temp = 0;
            var res = DriverApiNativeMethods.LibraryManagement.cuKernelGetAttribute(ref temp, CuFunctionAttribute.NumRegs, this, device);
            
            if (res != CuResult.Success) throw new CudaException(res);
            return temp;
        }
        public Version GetPtxVersion(CUdevice device)
        {
            var temp = 0;
            var res = DriverApiNativeMethods.LibraryManagement.cuKernelGetAttribute(ref temp, CuFunctionAttribute.PtxVersion, this, device);
            
            if (res != CuResult.Success) throw new CudaException(res);
            return new Version(temp / 10, temp % 10);
        }
        public Version GetBinaryVersion(CUdevice device)
        {
            var temp = 0;
            var res = DriverApiNativeMethods.LibraryManagement.cuKernelGetAttribute(ref temp, CuFunctionAttribute.BinaryVersion, this, device);
            
            if (res != CuResult.Success) throw new CudaException(res);
            return new Version(temp / 10, temp % 10);
        }
        public bool GetCacheModeCa(CUdevice device)
        {
            var temp = 0;
            var res = DriverApiNativeMethods.LibraryManagement.cuKernelGetAttribute(ref temp, CuFunctionAttribute.CacheModeCa, this, device);
            
            if (res != CuResult.Success) throw new CudaException(res);
            return temp != 0;
        }
        public int GetMaxDynamicSharedSizeBytes(CUdevice device)
        {
            var temp = 0;
            var res = DriverApiNativeMethods.LibraryManagement.cuKernelGetAttribute(ref temp, CuFunctionAttribute.MaxDynamicSharedSizeBytes, this, device);
            
            if (res != CuResult.Success) throw new CudaException(res);
            return temp;
        }
        public void SetMaxDynamicSharedSizeBytes(int size, CUdevice device)
        {
            var res = DriverApiNativeMethods.LibraryManagement.cuKernelSetAttribute(CuFunctionAttribute.MaxDynamicSharedSizeBytes, size, this, device);
            
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public CUsharedCarveout GetPreferredSharedMemoryCarveout(CUdevice device)
        {
            var temp = 0;
            var res = DriverApiNativeMethods.LibraryManagement.cuKernelGetAttribute(ref temp, CuFunctionAttribute.PreferredSharedMemoryCarveout, this, device);
            
            if (res != CuResult.Success) throw new CudaException(res);
            return (CUsharedCarveout)temp;
        }
        public void SetPreferredSharedMemoryCarveout(CUsharedCarveout value, CUdevice device)
        {
            var res = DriverApiNativeMethods.LibraryManagement.cuKernelSetAttribute(CuFunctionAttribute.PreferredSharedMemoryCarveout, (int)value, this, device);
            
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public bool GetClusterSizeMustBeSet(CUdevice device)
        {
            var temp = 0;
            var res = DriverApiNativeMethods.LibraryManagement.cuKernelGetAttribute(ref temp, CuFunctionAttribute.ClusterSizeMustBeSet, this, device);
            
            if (res != CuResult.Success) throw new CudaException(res);
            return temp != 0;
        }
        public int GetRequiredClusterWidth(CUdevice device)
        {
            var temp = 0;
            var res = DriverApiNativeMethods.LibraryManagement.cuKernelGetAttribute(ref temp, CuFunctionAttribute.RequiredClusterWidth, this, device);
            
            if (res != CuResult.Success) throw new CudaException(res);
            return temp;
        }
        public void SetRequiredClusterWidth(int value, CUdevice device)
        {
            var res = DriverApiNativeMethods.LibraryManagement.cuKernelSetAttribute(CuFunctionAttribute.RequiredClusterWidth, value, this, device);
            
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public int GetRequiredClusterHeight(CUdevice device)
        {
            var temp = 0;
            var res = DriverApiNativeMethods.LibraryManagement.cuKernelGetAttribute(ref temp, CuFunctionAttribute.RequiredClusterHeight, this, device);
            
            if (res != CuResult.Success) throw new CudaException(res);
            return temp;
        }
        public void SetRequiredClusterHeight(int value, CUdevice device)
        {
            var res = DriverApiNativeMethods.LibraryManagement.cuKernelSetAttribute(CuFunctionAttribute.RequiredClusterHeight, value, this, device);
            
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public int GetRequiredClusterDepth(CUdevice device)
        {
            var temp = 0;
            var res = DriverApiNativeMethods.LibraryManagement.cuKernelGetAttribute(ref temp, CuFunctionAttribute.RequiredClusterDepth, this, device);
            
            if (res != CuResult.Success) throw new CudaException(res);
            return temp;
        }
        public void SetRequiredClusterDepth(int value, CUdevice device)
        {
            var res = DriverApiNativeMethods.LibraryManagement.cuKernelSetAttribute(CuFunctionAttribute.RequiredClusterDepth, value, this, device);
            
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public bool GetNonPortableClusterSizeAllowed(CUdevice device)
        {
            var temp = 0;
            var res = DriverApiNativeMethods.LibraryManagement.cuKernelGetAttribute(ref temp, CuFunctionAttribute.NonPortableClusterSizeAllowed, this, device);
            
            if (res != CuResult.Success) throw new CudaException(res);
            return temp != 0;
        }
        public CUclusterSchedulingPolicy GetClusterSchedulingPolicyPreference(CUdevice device)
        {
            var temp = 0;
            var res = DriverApiNativeMethods.LibraryManagement.cuKernelGetAttribute(ref temp, CuFunctionAttribute.ClusterSchedulingPolicyPreference, this, device);
            
            if (res != CuResult.Success) throw new CudaException(res);
            return (CUclusterSchedulingPolicy)temp;
        }
        public void SetClusterSchedulingPolicyPreference(CUclusterSchedulingPolicy value, CUdevice device)
        {
            var res = DriverApiNativeMethods.LibraryManagement.cuKernelSetAttribute(CuFunctionAttribute.ClusterSchedulingPolicyPreference, (int)value, this, device);
            
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public void SetCacheConfig(CuFuncCache config, CUdevice device)
        {
            CuResult res;
            res = DriverApiNativeMethods.LibraryManagement.cuKernelSetCacheConfig(this, config, device);
            
            if (res != CuResult.Success)
                throw new CudaException(res);
        }
    }
    public enum CuFuncCache
    {
        PreferNone = 0x00,
        PreferShared = 0x01,
        PreferL1 = 0x02,
        PreferEqual = 0x03
    }
    public enum CuFunctionAttribute
    {
        MaxThreadsPerBlock = 0,
        SharedSizeBytes = 1,
        ConstSizeBytes = 2,
        LocalSizeBytes = 3,
        NumRegs = 4,
        PtxVersion = 5,
        BinaryVersion = 6,
        CacheModeCa = 7,
        MaxDynamicSharedSizeBytes = 8,
        PreferredSharedMemoryCarveout = 9,
        ClusterSizeMustBeSet = 10,
        RequiredClusterWidth = 11,
        RequiredClusterHeight = 12,
        RequiredClusterDepth = 13,
        NonPortableClusterSizeAllowed = 14,
        ClusterSchedulingPolicyPreference = 15,
        Max
    }
    public enum CUsharedCarveout
    {
        Default = -1,
        MaxShared = 100,
        MaxL1 = 0
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CudaMemCpy2D
    {
        public SizeT srcXInBytes;
        public SizeT srcY;
        public CuMemoryType srcMemoryType;
        public IntPtr srcHost;
        public CUdeviceptr srcDevice;
        public CUarray srcArray;
        public SizeT srcPitch;
        public SizeT dstXInBytes;
        public SizeT dstY;
        public CuMemoryType dstMemoryType;
        public IntPtr dstHost;
        public CUdeviceptr dstDevice;
        public CUarray dstArray;
        public SizeT dstPitch;
        public SizeT WidthInBytes;
        public SizeT Height;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CUarray
    {
        public IntPtr Pointer;
        public CudaArrayMemoryRequirements GetMemoryRequirements(CUdevice device)
        {
            var temp = new CudaArrayMemoryRequirements();
            var res = DriverApiNativeMethods.ArrayManagement.cuArrayGetMemoryRequirements(ref temp, this, device);
            
            if (res != CuResult.Success) throw new CudaException(res);
            return temp;
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CudaArrayMemoryRequirements
    {
        public SizeT size;
        public SizeT alignment;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4, ArraySubType = UnmanagedType.U4)]
        uint[] reserved;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CUmemPoolPtrExportData
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64, ArraySubType = UnmanagedType.U1)]
        byte[] reserved;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CUmemPoolProps
    {
        public CUmemAllocationType allocType;
        public CUmemAllocationHandleType handleTypes;
        public CUmemLocation location;
        public IntPtr win32SecurityAttributes;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64, ArraySubType = UnmanagedType.U1)]
        byte[] reserved;
    }
    public enum CUmemAllocationType
    {
        Invalid = 0x0,
        Pinned = 0x1
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CUmemLocation
    {
        public CUmemLocationType type;
        public int id;
    }
    public enum CUmemLocationType
    {
        Invalid = 0x0,
        Device = 0x1
    }
    [Flags]
    public enum CUmemAccessFlags
    {
        ProtNone = 0x1,
        ProtRead = 0x2,
        ProtReadWrite = 0x3
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CUmemAccessDesc
    {
        public CUmemLocation location;
        public CUmemAccessFlags flags;
    }
    public enum CujitOption
    {
        MaxRegisters = 0,
        ThreadsPerBlock = 1,
        WallTime = 2,
        InfoLogBuffer = 3,
        InfoLogBufferSizeBytes = 4,
        ErrorLogBuffer = 5,
        ErrorLogBufferSizeBytes = 6,
        OptimizationLevel = 7,
        TargetFromContext = 8,
        Target = 9,
        FallbackStrategy = 10,
        GenerateDebugInfo = 11,
        LogVerbose = 12,
        GenerateLineInfo = 13,
        JitCacheMode = 14,
        [Obsolete("This jit option is deprecated and should not be used.")]
        NewSm3XOpt = 15,
        FastCompile = 16,
        GlobalSymbolNames = 17,
        GlobalSymbolAddresses = 18,
        GlobalSymbolCount = 19,
        [Obsolete("This jit option is deprecated and should not be used.")]
        Lto = 20,
        [Obsolete("This jit option is deprecated and should not be used.")]
        Ftz = 21,
        [Obsolete("This jit option is deprecated and should not be used.")]
        PrecDiv = 22,
        [Obsolete("This jit option is deprecated and should not be used.")]
        PrecSqrt = 23,
        [Obsolete("This jit option is deprecated and should not be used.")]
        Fma = 24,
        [Obsolete("This jit option is deprecated and should not be used.")]
        ReferencedKernelNames = 25,
        [Obsolete("This jit option is deprecated and should not be used.")]
        ReferencedKernelCount = 26,
        [Obsolete("This jit option is deprecated and should not be used.")]
        ReferencedVariableNames = 27,
        [Obsolete("This jit option is deprecated and should not be used.")]
        ReferencedVariableCount = 28,
        [Obsolete("This jit option is deprecated and should not be used.")]
        OptimizeUnusedDeviceVariables = 29,
        PositionIndependentCode = 30,
    }
    public enum CUlibraryOption
    {
        HostUniversalFunctionAndDataTable = 0,
        BinaryIsPreserved = 1,
        NumOptions
    }
    [Flags]
    public enum CuEventRecordFlags : uint
    {
        Default = 0x0,
        External = 0x1
    }
    [Flags]
    public enum CuEventFlags
    {
        Default = 0,
        BlockingSync = 1,
        DisableTiming = 2,
        InterProcess = 4
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CudaLaunchParams
    {
        public CUfunction function;
        public uint gridDimX;
        public uint gridDimY;
        public uint gridDimZ;
        public uint blockDimX;
        public uint blockDimY;
        public uint blockDimZ;
        public uint sharedMemBytes;
        public CUstream hStream;
        public IntPtr kernelParams;
    }
    [Flags]
    public enum CudaCooperativeLaunchMultiDeviceFlags
    {
        None = 0,
        NoPreLaunchSync = 0x01,
        NoPostLaunchSync = 0x02,
    }
    public enum CUsharedconfig
    {
        DefaultBankSize = 0x00,
        FourByteBankSize = 0x01,
        EightByteBankSize = 0x02
    }
    public delegate SizeT DelCUoccupancyB2DSize(int aBlockSize);
    public enum CUoccupancyFlags
    {
        Default = 0,
        DisableCachingOverride = 1
    }
    public enum Operation
    {
        NonTranspose = 0,
        Transpose = 1,
        ConjugateTranspose = 2,
        Hermitan = 2,
        Conjugate = 3
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CudaBlasHandle
    {
        public IntPtr Pointer;
    }
    public enum CudaDataType
    {
        CudaR16F = 2,
        CudaC16F = 6,
        CudaR32F = 0,
        CudaC32F = 4,
        CudaR64F = 1,
        CudaC64F = 5,
        CudaR8I = 3,
        CudaC8I = 7,
        CudaR8U = 8,
        CudaC8U = 9,
        CudaR16Bf = 14,
        CudaC16Bf = 15,
        CudaR4I = 16,
        CudaC4I = 17,
        CudaR4U = 18,
        CudaC4U = 19,
        CudaR16I = 20,
        CudaC16I = 21,
        CudaR16U = 22,
        CudaC16U = 23,
        CudaR32I = 10,
        CudaC32I = 11,
        CudaR32U = 12,
        CudaC32U = 13,
        CudaR64I = 24,
        CudaC64I = 25,
        CudaR64U = 26,
        CudaC64U = 27
    }
    public enum FillMode
    {
        Lower = 0,
        Upper = 1,
        Full = 2
    }
    public enum DiagType
    {
        NonUnit = 0,
        Unit = 1
    }
    public enum SideMode
    {
        Left = 0,
        Right = 1
    }
    public enum PointerMode
    {
        Host = 0,
        Device = 1
    }
    public enum AtomicsMode
    {
        NotAllowed = 0,
        Allowed = 1
    }
    public enum GemmAlgo
    {
        Default = -1,
        Algo0 = 0,
        Algo1 = 1,
        Algo2 = 2,
        Algo3 = 3,
        Algo4 = 4,
        Algo5 = 5,
        Algo6 = 6,
        Algo7 = 7,
        Algo8 = 8,
        Algo9 = 9,
        Algo10 = 10,
        Algo11 = 11,
        Algo12 = 12,
        Algo13 = 13,
        Algo14 = 14,
        Algo15 = 15,
        Algo16 = 16,
        Algo17 = 17,
        Algo18 = 18,
        Algo19 = 19,
        Algo20 = 20,
        Algo21 = 21,
        Algo22 = 22,
        Algo23 = 23,
        DefaultTensorOp = 99,
        Algo0TensorOp = 100,
        Algo1TensorOp = 101,
        Algo2TensorOp = 102,
        Algo3TensorOp = 103,
        Algo4TensorOp = 104,
        Algo5TensorOp = 105,
        Algo6TensorOp = 106,
        Algo7TensorOp = 107,
        Algo8TensorOp = 108,
        Algo9TensorOp = 109,
        Algo10TensorOp = 110,
        Algo11TensorOp = 111,
        Algo12TensorOp = 112,
        Algo13TensorOp = 113,
        Algo14TensorOp = 114,
        Algo15TensorOp = 115
    }
    public enum Math
    {
        DefaultMath = 0,
        [Obsolete("deprecated, same effect as using CUBLAS_COMPUTE_32F_FAST_16F, will be removed in a future release")]
        TensorOpMath = 1,
        PedanticMath = 2,
        Tf32TensorOpMath = 3,
        DisallowReducedPrecisionReduction = 16
    }
    public enum ComputeType
    {
        Compute16F = 64,
        Compute16FPedantic = 65,
        Compute32F = 68,
        Compute32FPedantic = 69,
        Compute32FFast16F = 74,
        Compute32FFast16Bf = 75,
        Compute32FFastTf32 = 77,
        Compute64F = 70,
        Compute64FPedantic = 71,
        Compute32I = 72,
        Compute32IPedantic = 73,
    }
    public enum DataType
    {
        CudaR16F = 2,
        CudaC16F = 6,
        CudaR32F = 0,
        CudaC32F = 4,
        CudaR64F = 1,
        CudaC64F = 5,
        CudaR8I = 3,
        CudaC8I = 7,
        CudaR8U = 8,
        CudaC8U = 9
    }
    [StructLayout(LayoutKind.Sequential)]
#pragma warning disable CS8981
    public struct Half
#pragma warning restore CS8981
    {
        ushort x;
        public Half(float f)
        {
            x = __float2half(f).x;
        }
        public Half(double d)
        {
            x = __double2half(d).x;
        }
        public Half(Half h16)
        {
            x = h16.x;
        }

        static ushort __internal_float2half(float f, out uint sign, out uint remainder)
        {
            var ftemp = new[] { f };
            var x = new uint[1];
            uint u = 0;
            uint result = 0;
            System.Buffer.BlockCopy(ftemp, 0, x, 0, sizeof(float));

            u = (x[0] & 0x7fffffffU);
            sign = ((x[0] >> 16) & 0x8000U);
            if (u >= 0x7f800000U) {
                remainder = 0U;
                result = ((u == 0x7f800000U) ? (sign | 0x7c00U) : 0x7fffU);
            }
            else if (u > 0x477fefffU) {
                remainder = 0x80000000U;
                result = (sign | 0x7bffU);
            }
            else if (u >= 0x38800000U) {
                remainder = u << 19;
                u -= 0x38000000U;
                result = (sign | (u >> 13));
            }
            else if (u < 0x33000001U) {
                remainder = u;
                result = sign;
            }
            else {
                var exponent = u >> 23;
                var shift = 0x7eU - exponent;
                var mantissa = (u & 0x7fffffU);
                mantissa |= 0x800000U;
                remainder = mantissa << (32 - (int)shift);
                result = (sign | (mantissa >> (int)shift));
            }

            return (ushort)(result);
        }

        static Half __double2half(double x)
        {
            ulong absx;
            var ux = new ulong[1];
            var xa = new[] { x };
            System.Buffer.BlockCopy(xa, 0, ux, 0, sizeof(double));

            absx = (ux[0] & 0x7fffffffffffffffUL);
            if ((absx >= 0x40f0000000000000UL) || (absx <= 0x3e60000000000000UL)) {
                return __float2half((float)x);
            }
            var shifterBits = ux[0] & 0x7ff0000000000000UL;
            if (absx >= 0x3f10000000000000UL) {
                shifterBits += 42ul << 52;
            }

            else {
                shifterBits = ((42ul - 14 + 1023) << 52);
            }
            shifterBits |= 1ul << 51;
            var shifterBitsArr = new[] { shifterBits };
            var shifter = new double[1];

            System.Buffer.BlockCopy(shifterBitsArr, 0, shifter, 0, sizeof(double));

            var xShiftRound = x + shifter[0];
            var xShiftRoundArr = new[] { xShiftRound };
            var xShiftRoundBits = new ulong[1];

            System.Buffer.BlockCopy(xShiftRoundArr, 0, xShiftRoundBits, 0, sizeof(double));
            xShiftRoundBits[0] &= 0x7ffffffffffffffful;

            System.Buffer.BlockCopy(xShiftRoundBits, 0, xShiftRoundArr, 0, sizeof(double));

            var xRounded = xShiftRound - shifter[0];
            var xRndFlt = (float)xRounded;
            var res = __float2half(xRndFlt);
            return res;
        }

        static Half __float2half(float a)
        {
            var r = new Half {
                x = __internal_float2half(a, out _, out var remainder)
            };
            if ((remainder > 0x80000000U) || ((remainder == 0x80000000U) && ((r.x & 0x1U) != 0U))) {
                r.x++;
            }

            return r;
        }
        public override string ToString()
        {
            return x.ToString();
        }
    }
    public delegate void CublasLogCallback([MarshalAs(UnmanagedType.LPStr)] string msg);
    [StructLayout(LayoutKind.Sequential)]
    public struct CusparseMatDescr
    {
        public IntPtr Handle;
    }
    [Flags]
    public enum CuInitializationFlags : uint
    {
        None = 0
    }
    [StructLayout(LayoutKind.Explicit)]
    public struct CudaExternalSemaphoreWaitParams
    {
        [StructLayout(LayoutKind.Explicit)]
        public struct Parameters
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct Fence
            {
                public ulong value;
            }
            [FieldOffset(0)] public Fence fence;
            [StructLayout(LayoutKind.Sequential)]
            public struct NvSciSync
            {
                public IntPtr fence;
            }
            [FieldOffset(8)] public NvSciSync nvSciSync;
            [StructLayout(LayoutKind.Sequential)]
            public struct KeyedMutex
            {
                public ulong key;
                public uint timeoutMs;
            }
            [FieldOffset(16)] public KeyedMutex keyedMutex;

            [FieldOffset(20)] uint reserved;
        }
        [FieldOffset(0)] public Parameters parameters;
        [FieldOffset(72)] public uint flags;

        [FieldOffset(136)] uint reserved;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct CudaMemAllocNodeParamsInternal
    {
        public CUmemPoolProps poolProps;
        public IntPtr accessDescs;
        public SizeT accessDescCount;
        public SizeT bytesize;
        public CUdeviceptr dptr;
    }

    internal struct CUlaunchConfigInternal
    {
        public uint GridDimX;
        public uint GridDimY;
        public uint GridDimZ;
        public uint BlockDimX;
        public uint BlockDimY;
        public uint BlockDimZ;
        public uint SharedMemBytes;
        public CUstream HStream;
        public IntPtr Attrs;
        public uint NumAttrs;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct CUgraphNode
    {
        public IntPtr Pointer;
        public CUgraphNodeType Type
        {
            get
            {
                var type = new CUgraphNodeType();
                var res = DriverApiNativeMethods.GraphManagment.cuGraphNodeGetType(this, ref type);
                if (res != CuResult.Success) throw new CudaException(res);
                return type;
            }
        }
        public void SetParameters(CudaHostNodeParams nodeParams)
        {
            var res = DriverApiNativeMethods.GraphManagment.cuGraphHostNodeSetParams(this, ref nodeParams);
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public void SetParameters(CudaKernelNodeParams nodeParams)
        {
            var res = DriverApiNativeMethods.GraphManagment.cuGraphKernelNodeSetParams(this, ref nodeParams);
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public void SetParameters(CudaMemCpy3D nodeParams)
        {
            var res = DriverApiNativeMethods.GraphManagment.cuGraphMemcpyNodeSetParams(this, ref nodeParams);
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public void SetParameters(CudaMemsetNodeParams nodeParams)
        {
            var res = DriverApiNativeMethods.GraphManagment.cuGraphMemsetNodeSetParams(this, ref nodeParams);
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public void SetParameters(CudaExtSemSignalNodeParams nodeParams)
        {
            var res = DriverApiNativeMethods.GraphManagment.CuGraphExternalSemaphoresSignalNodeSetParams(this, nodeParams);
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public void SetParameters(CudaExtSemWaitNodeParams nodeParams)
        {
            var res = DriverApiNativeMethods.GraphManagment.CuGraphExternalSemaphoresWaitNodeSetParams(this, nodeParams);
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public void SetParameters(CudaBatchMemOpNodeParams nodeParams)
        {
            var res = DriverApiNativeMethods.GraphManagment.CuGraphBatchMemOpNodeSetParams(this, ref nodeParams);
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public void GetParameters(ref CudaHostNodeParams nodeParams)
        {
            var res = DriverApiNativeMethods.GraphManagment.cuGraphHostNodeGetParams(this, ref nodeParams);
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public void GetParameters(ref CudaKernelNodeParams nodeParams)
        {
            var res = DriverApiNativeMethods.GraphManagment.cuGraphKernelNodeGetParams(this, ref nodeParams);
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public void GetParameters(ref CudaMemCpy3D nodeParams)
        {
            var res = DriverApiNativeMethods.GraphManagment.cuGraphMemcpyNodeGetParams(this, ref nodeParams);
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public void GetParameters(ref CudaMemsetNodeParams nodeParams)
        {
            var res = DriverApiNativeMethods.GraphManagment.cuGraphMemsetNodeGetParams(this, ref nodeParams);
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public void GetParameters(CudaExtSemSignalNodeParams nodeParams)
        {
            var res = DriverApiNativeMethods.GraphManagment.CuGraphExternalSemaphoresSignalNodeGetParams(this, nodeParams);
            
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public void GetParameters(CudaExtSemWaitNodeParams nodeParams)
        {
            var res = DriverApiNativeMethods.GraphManagment.CuGraphExternalSemaphoresWaitNodeGetParams(this, nodeParams);
            
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public void GetParameters(ref CudaMemAllocNodeParams nodeParams)
        {
            var res = DriverApiNativeMethods.GraphManagment.CuGraphMemAllocNodeGetParams(this, ref nodeParams);
            
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public void GetParameters(ref CUdeviceptr nodeParams)
        {
            var res = DriverApiNativeMethods.GraphManagment.cuGraphMemFreeNodeGetParams(this, ref nodeParams);
            
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public void GetParameters(ref CudaBatchMemOpNodeParams nodeParams)
        {
            var res = DriverApiNativeMethods.GraphManagment.CuGraphBatchMemOpNodeGetParams(this, ref nodeParams);
            
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public CUgraphNode[]? GetDependencies()
        {
            var numNodes = new SizeT();
            var res = DriverApiNativeMethods.GraphManagment.cuGraphNodeGetDependencies(this, null, ref numNodes);
            
            if (res != CuResult.Success) throw new CudaException(res);

            if (numNodes > 0) {
                var nodes = new CUgraphNode[numNodes];
                res = DriverApiNativeMethods.GraphManagment.cuGraphNodeGetDependencies(this, nodes, ref numNodes);
                
                if (res != CuResult.Success) throw new CudaException(res);

                return nodes;
            }

            return null;
        }
        public CUgraphNode[]? GetDependentNodes()
        {
            var numNodes = new SizeT();
            var res = DriverApiNativeMethods.GraphManagment.cuGraphNodeGetDependentNodes(this, null, ref numNodes);
            
            if (res != CuResult.Success) throw new CudaException(res);

            if (numNodes > 0) {
                var nodes = new CUgraphNode[numNodes];
                res = DriverApiNativeMethods.GraphManagment.cuGraphNodeGetDependentNodes(this, nodes, ref numNodes);
                
                if (res != CuResult.Success) throw new CudaException(res);

                return nodes;
            }

            return null;
        }
        public void CuGraphKernelNodeCopyAttributes(CUgraphNode dst)
        {
            var res = DriverApiNativeMethods.GraphManagment.cuGraphKernelNodeCopyAttributes(dst, this);
            
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public CUkernelNodeAttrValue GetAttribute(CUkernelNodeAttrId attr)
        {
            var value = new CUkernelNodeAttrValue();
            var res = DriverApiNativeMethods.GraphManagment.cuGraphKernelNodeGetAttribute(this, attr, ref value);
            
            if (res != CuResult.Success) throw new CudaException(res);
            return value;
        }
        public void SetAttribute(CUkernelNodeAttrId attr, CUkernelNodeAttrValue value)
        {
            var res = DriverApiNativeMethods.GraphManagment.cuGraphKernelNodeSetAttribute(this, attr, ref value);
            
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public CUevent GetRecordEvent()
        {
            var eventOut = new CUevent();
            var res = DriverApiNativeMethods.GraphManagment.cuGraphEventRecordNodeGetEvent(this, ref eventOut);
            
            if (res != CuResult.Success) throw new CudaException(res);
            return eventOut;
        }
        public void SetRecordEvent(CUevent @event)
        {
            var res = DriverApiNativeMethods.GraphManagment.cuGraphEventRecordNodeSetEvent(this, @event);
            
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public CUevent GetWaitEvent()
        {
            var eventOut = new CUevent();
            var res = DriverApiNativeMethods.GraphManagment.cuGraphEventWaitNodeGetEvent(this, ref eventOut);
            
            if (res != CuResult.Success) throw new CudaException(res);
            return eventOut;
        }
        public void SetWaitEvent(CUevent @event)
        {
            var res = DriverApiNativeMethods.GraphManagment.cuGraphEventWaitNodeSetEvent(this, @event);
            
            if (res != CuResult.Success) throw new CudaException(res);
        }
    }
    public class CudaBatchMemOpNodeParams
    {
        public CUcontext Ctx;
        public CUstreamBatchMemOpParams[]? ParamArray;
        public uint Flags;
    }
    [StructLayout(LayoutKind.Explicit)]
    public struct CUstreamBatchMemOpParams
    {
        [FieldOffset(0)] public CUstreamBatchMemOpType operation;
        [FieldOffset(0)] public CUstreamMemOpWaitValueParams waitValue;
        [FieldOffset(0)] public CUstreamMemOpWriteValueParams writeValue;
        [FieldOffset(0)] public CUstreamMemOpFlushRemoteWritesParams flushRemoteWrites;
        [FieldOffset(0)] public CUstreamMemOpMemoryBarrierParams memoryBarrier;
        [FieldOffset(5 * 8)] ulong pad;
    }
    public enum CUstreamBatchMemOpType
    {
        WaitValue32 = 1,
        WriteValue32 = 2,
        WaitValue64 = 4,
        WriteValue64 = 5,
        Barrier = 6,
        FlushRemoteWrites = 3
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CUstreamMemOpWaitValueParams
    {
        public CUstreamBatchMemOpType operation;
        public CUdeviceptr address;
        public Cuuint3264Union value;
        public uint flags;
        public CUdeviceptr alias;
    }
    [StructLayout(LayoutKind.Explicit)]
    public struct Cuuint3264Union
    {
        [FieldOffset(0)] public uint value;
        [FieldOffset(0)] public ulong value64;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CUstreamMemOpWriteValueParams
    {
        public CUstreamBatchMemOpType operation;
        public CUdeviceptr address;
        public Cuuint3264Union value;
        public uint flags;
        public CUdeviceptr alias;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CudaMemAllocNodeParams
    {
        public CUmemPoolProps poolProps;
        public CUmemAccessDesc[] accessDescs;
        public SizeT bytesize;
        public CUdeviceptr dptr;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CudaHostNodeParams
    {
        public CUhostFn fn;
        public IntPtr userData;
    }
    public delegate void CUhostFn(IntPtr userData);
    [StructLayout(LayoutKind.Sequential)]
    public struct CUstreamMemOpFlushRemoteWritesParams
    {
        public CUstreamBatchMemOpType operation;
        public uint flags;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CUstreamMemOpMemoryBarrierParams
    {
        public CUstreamBatchMemOpType operation;
        public uint flags;
    }
    public class CudaExtSemWaitNodeParams
    {
        public CUexternalSemaphore[]? ExtSemArray;
        public CudaExternalSemaphoreWaitParams[]? ParamsArray;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CUexternalSemaphore
    {
        public IntPtr Pointer;
    }
    public class CudaExtSemSignalNodeParams
    {
        public CUexternalSemaphore[]? ExtSemArray;
        public CudaExternalSemaphoreSignalParams[]? ParamsArray;
    }
    [StructLayout(LayoutKind.Explicit)]
    public struct CudaExternalSemaphoreSignalParams
    {
        [StructLayout(LayoutKind.Explicit)]
        public struct Parameters
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct Fence
            {
                public ulong value;
            }
            [FieldOffset(0)] public Fence fence;
            [StructLayout(LayoutKind.Sequential)]
            public struct NvSciSync
            {
                public IntPtr fence;
            }
            [FieldOffset(8)] public NvSciSync nvSciSync;
            [StructLayout(LayoutKind.Sequential)]
            public struct KeyedMutex
            {
                public ulong key;
            }
            [FieldOffset(16)] public KeyedMutex keyedMutex;
            [FieldOffset(68)]
            uint reserved;
        }
        [FieldOffset(0)] public Parameters parameters;
        [FieldOffset(72)] public uint flags;

        [FieldOffset(136)]
        uint reserved;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CudaMemCpy3D
    {
        public SizeT srcXInBytes;
        public SizeT srcY;
        public SizeT srcZ;
        public SizeT srcLOD;
        public CuMemoryType srcMemoryType;
        public IntPtr srcHost;
        public CUdeviceptr srcDevice;
        public CUarray srcArray;
        public IntPtr reserved0;
        public SizeT srcPitch;
        public SizeT srcHeight;
        public SizeT dstXInBytes;
        public SizeT dstY;
        public SizeT dstZ;
        public SizeT dstLOD;
        public CuMemoryType dstMemoryType;
        public IntPtr dstHost;
        public CUdeviceptr dstDevice;
        public CUarray dstArray;
        public IntPtr reserved1;
        public SizeT dstPitch;
        public SizeT dstHeight;
        public SizeT WidthInBytes;
        public SizeT Height;
        public SizeT Depth;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct CudaMemsetNodeParams
    {
        public CUdeviceptr dst;
        public SizeT pitch;
        public uint value;
        public uint elementSize;
        public SizeT width;
        public SizeT height;
        public static CudaMemsetNodeParams Init<T>(CudaDeviceVariable<T> deviceVariable, uint value) where T : struct
        {
            var para = new CudaMemsetNodeParams();
            para.dst = deviceVariable.DevicePointer;
            para.pitch = deviceVariable.SizeInBytes;
            para.value = value;
            para.elementSize = deviceVariable.TypeSize;
            para.width = deviceVariable.SizeInBytes;
            para.height = 1;

            return para;
        }
    }
    public enum CUgraphNodeType
    {
        Kernel = 0,
        Memcpy = 1,
        Memset = 2,
        Host = 3,
        Graph = 4,
        Empty = 5,
        WaitEvent = 6,
        EventRecord = 7,
        ExtSemasSignal = 8,
        ExtSemasWait = 9,
        MemAlloc = 10,
        MemFree = 11,
        BatchMemOp = 12
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CudaKernelNodeParams
    {
        public CUfunction func;
        public uint gridDimX;
        public uint gridDimY;
        public uint gridDimZ;
        public uint blockDimX;
        public uint blockDimY;
        public uint blockDimZ;
        public uint sharedMemBytes;
        public IntPtr kernelParams;
        public IntPtr extra;
        CUkernel kern;
        CUcontext ctx;
    }
    public enum CUkernelNodeAttrId
    {
        AccessPolicyWindow = 1,
        Cooperative = 2
    }
    [StructLayout(LayoutKind.Explicit)]
    public struct CUkernelNodeAttrValue
    {
        [FieldOffset(0)] public CUaccessPolicyWindow accessPolicyWindow;
        [FieldOffset(0)] public int cooperative;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CUgraph
    {
        public IntPtr Pointer;
    }
    internal struct CudaBatchMemOpNodeParamsInternal
    {
        public CUcontext Ctx;
        public uint Count;
        public IntPtr ParamArray;
        public uint Flags;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CUuserObject
    {
        public IntPtr Pointer;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct Luid
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8, ArraySubType = UnmanagedType.I1)]
        public byte[] bytes;
    }
    public enum CuArrayFormat
    {
        UnsignedInt8 = 0x01,
        UnsignedInt16 = 0x02,
        UnsignedInt32 = 0x03,
        SignedInt8 = 0x08,
        SignedInt16 = 0x09,
        SignedInt32 = 0x0a,
        Half = 0x10,
        Float = 0x20,
        Nv12 = 0xb0,
        UNormInt8X1 = 0xc0,
        UNormInt8X2 = 0xc1,
        UNormInt8X4 = 0xc2,
        UNormInt16X1 = 0xc3,
        UNormInt16X2 = 0xc4,
        UNormInt16X4 = 0xc5,
        SNormInt8X1 = 0xc6,
        SNormInt8X2 = 0xc7,
        SNormInt8X4 = 0xc8,
        SNormInt16X1 = 0xc9,
        SNormInt16X2 = 0xca,
        SNormInt16X4 = 0xcb,
        Bc1UNorm = 0x91,
        Bc1UNormSrgb = 0x92,
        Bc2UNorm = 0x93,
        Bc2UNormSrgb = 0x94,
        Bc3UNorm = 0x95,
        Bc3UNormSrgb = 0x96,
        Bc4UNorm = 0x97,
        Bc4SNorm = 0x98,
        Bc5UNorm = 0x99,
        Bc5SNorm = 0x9a,
        Bc6Huf16 = 0x9b,
        Bc6Hsf16 = 0x9c,
        Bc7UNorm = 0x9d,
        Bc7UNormSrgb = 0x9e
    }
    public enum CuDeviceAttribute
    {
        MaxThreadsPerBlock = 1,
        MaxBlockDimX = 2,
        MaxBlockDimY = 3,
        MaxBlockDimZ = 4,
        MaxGridDimX = 5,
        MaxGridDimY = 6,
        MaxGridDimZ = 7,
        MaxSharedMemoryPerBlock = 8,
        [Obsolete("Use MaxSharedMemoryPerBlock")]
        SharedMemoryPerBlock = 8,
        TotalConstantMemory = 9,
        WarpSize = 10,
        MaxPitch = 11,
        [Obsolete("Use MaxRegistersPerBlock")] RegistersPerBlock = 12,
        MaxRegistersPerBlock = 12,
        ClockRate = 13,
        TextureAlignment = 14,
        GpuOverlap = 15,
        MultiProcessorCount = 0x10,
        KernelExecTimeout = 0x11,
        Integrated = 0x12,
        CanMapHostMemory = 0x13,
        ComputeMode = 20,
        MaximumTexture1DWidth = 21,
        MaximumTexture2DWidth = 22,
        MaximumTexture2DHeight = 23,
        MaximumTexture3DWidth = 24,
        MaximumTexture3DHeight = 25,
        MaximumTexture3DDepth = 26,
        MaximumTexture2DArrayWidth = 27,
        MaximumTexture2DArrayHeight = 28,
        MaximumTexture2DArrayNumSlices = 29,
        SurfaceAllignment = 30,
        ConcurrentKernels = 31,
        EccEnabled = 32,
        PciBusId = 33,
        PciDeviceId = 34,
        TccDriver = 35,
        MemoryClockRate = 36,
        GlobalMemoryBusWidth = 37,
        L2CacheSize = 38,
        MaxThreadsPerMultiProcessor = 39,
        AsyncEngineCount = 40,
        UnifiedAddressing = 41,
        MaximumTexture1DLayeredWidth = 42,
        MaximumTexture1DLayeredLayers = 43,
        PciDomainId = 50,
        TexturePitchAlignment = 51,
        MaximumTextureCubeMapWidth = 52,
        MaximumTextureCubeMapLayeredWidth = 53,
        MaximumTextureCubeMapLayeredLayers = 54,
        MaximumSurface1DWidth = 55,
        MaximumSurface2DWidth = 56,
        MaximumSurface2DHeight = 57,
        MaximumSurface3DWidth = 58,
        MaximumSurface3DHeight = 59,
        MaximumSurface3DDepth = 60,
        MaximumSurface1DLayeredWidth = 61,
        MaximumSurface1DLayeredLayers = 62,
        MaximumSurface2DLayeredWidth = 63,
        MaximumSurface2DLayeredHeight = 64,
        MaximumSurface2DLayeredLayers = 65,
        MaximumSurfaceCubemapWidth = 66,
        MaximumSurfaceCubemapLayeredWidth = 67,
        MaximumSurfaceCubemapLayeredLayers = 68,
        [Obsolete("Deprecated, do not use. Use cudaDeviceGetTexture1DLinearMaxWidth() or cuDeviceGetTexture1DLinearMaxWidth() instead.")]
        MaximumTexture1DLinearWidth = 69,
        MaximumTexture2DLinearWidth = 70,
        MaximumTexture2DLinearHeight = 71,
        MaximumTexture2DLinearPitch = 72,
        MaximumTexture2DMipmappedWidth = 73,
        MaximumTexture2DMipmappedHeight = 74,
        ComputeCapabilityMajor = 75,
        ComputeCapabilityMinor = 76,
        MaximumTexture1DMipmappedWidth = 77,
        StreamPrioritiesSupported = 78,
        GlobalL1CacheSupported = 79,
        LocalL1CacheSupported = 80,
        MaxSharedMemoryPerMultiprocessor = 81,
        MaxRegistersPerMultiprocessor = 82,
        ManagedMemory = 83,
        MultiGpuBoard = 84,
        MultiGpuBoardGroupId = 85,
        HostNativeAtomicSupported = 86,
        SingleToDoublePrecisionPerfRatio = 87,
        PageableMemoryAccess = 88,
        ConcurrentManagedAccess = 89,
        ComputePreemptionSupported = 90,
        CanUseHostPointerForRegisteredMem = 91,
        [Obsolete("Deprecated, along with v1 MemOps API, ::cuStreamBatchMemOp and related APIs are supported.")]
        CanUseStreamMemOpsV1 = 92,
        [Obsolete("Deprecated, along with v1 MemOps API, 64-bit operations are supported in ::cuStreamBatchMemOp and related APIs.")]
        CanUse64BitStreamMemOpsV1 = 93,
        [Obsolete("Deprecated, along with v1 MemOps API, ::CU_STREAM_WAIT_VALUE_NOR is supported.")]
        CanUseStreamWaitValueNOrV1 = 94,
        CooperativeLaunch = 95,
        CooperativeMultiDeviceLaunch = 96,
        MaxSharedMemoryPerBlockOptin = 97,
        CanFlushRemoteWrites = 98,
        HostRegisterSupported = 99,
        PageableMemoryAccessUsesHostPageTables = 100,
        DirectManagedMemoryAccessFromHost = 101,
        [Obsolete("Deprecated, Use VirtualMemoryManagementSupported")]
        VirtualAddressManagementSupported = 102,
        VirtualMemoryManagementSupported = 102,
        HandleTypePosixFileDescriptorSupported = 103,
        HandleTypeWin32HandleSupported = 104,
        HandleTypeWin32KmtHandleSupported = 105,
        MaxBlocksPerMultiProcessor = 106,
        GenericCompressionSupported = 107,
        MaxPersistingL2CacheSize = 108,
        MaxAccessPolicyWindowSize = 109,
        GpuDirectRdmaWithCudaVmmSupported = 110,
        ReservedSharedMemoryPerBlock = 111,
        SparseCudaArraySupported = 112,
        ReadOnlyHostRegisterSupported = 113,
        TimelineSemaphoreInteropSupported = 114,
        MemoryPoolsSupported = 115,
        GpuDirectRdmaSupported = 116,
        GpuDirectRdmaFlushWritesOptions = 117,
        GpuDirectRdmaWritesOrdering = 118,
        MempoolSupportedHandleTypes = 119,
        ClusterLaunch = 120,
        DeferredMappingCudaArraySupported = 121,
        CanUse64BitStreamMemOps = 122,
        CanUseStreamWaitValueNor = 123,
        DmaBufSupported = 124,
        IpcEventSupported = 125,
        MemSyncDomainCount = 126,
        TensorMapAccessSupported = 127,
        UnifiedFunctionPointers = 129,
        Max
    }
    [Flags]
    public enum NvSciSyncAttr
    {
        Signal = 0x01,
        Wait = 0x02,
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CUipcEventHandle
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64, ArraySubType = UnmanagedType.I1)]
        public byte[] reserved;
    }
    [Flags]
    public enum CuCtxFlags
    {
        SchedAuto = 0,
        SchedSpin = 1,
        SchedYield = 2,
        BlockingSync = 4,
        SchedMask = 7,
        [Obsolete("This flag was deprecated as of CUDA 11.0 and it no longer has any effect. All contexts as of CUDA 3.2 behave as though the flag is enabled.")]
        MapHost = 8,
        LMemResizeToMax = 16,
        FlagsMask = 0x1f
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CUtexref
    {
        public IntPtr Pointer;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CUmipmappedArray
    {
        public IntPtr Pointer;
        public CudaArrayMemoryRequirements GetMemoryRequirements(CUdevice device)
        {
            var temp = new CudaArrayMemoryRequirements();
            var res = DriverApiNativeMethods.ArrayManagement.cuMipmappedArrayGetMemoryRequirements(ref temp, this, device);
            
            if (res != CuResult.Success) throw new CudaException(res);
            return temp;
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CUgraphicsResource
    {
        public IntPtr Pointer;
    }
    [Flags]
    public enum CuGraphicsMapResourceFlags
    {
        None = 0,
        ReadOnly = 1,
        WriteDiscard = 2
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CUtexObject
    {
        public ulong Pointer;
    }
    public enum CUstreamCaptureStatus
    {
        None = 0,
        Active = 1,
        Invalidated = 2
    }
    public enum CuLimit
    {
        StackSize = 0,
        PrintfFifoSize = 1,
        MallocHeapSize = 2,
        DevRuntimeSyncDepth = 3,
        DevRuntimePendingLaunchCount = 4,
        MaxL2FetchGranularity = 0x05,
        PersistingL2CacheSize = 0x06
    }
    [Flags]
    public enum CtxEnablePeerAccessFlags : uint
    {
        None = 0
    }
    public enum CuAddressMode
    {
        Wrap = 0,
        Clamp = 1,
        Mirror = 2,
        Border = 3
    }
    public enum CuFilterMode
    {
        Point = 0,
        Linear = 1
    }
    [Flags]
    public enum CuMemHostRegisterFlags
    {
        None = 0,
        Portable = 1,
        DeviceMap = 2,
        IoMemory = 0x04,
        ReadOnly = 0x08
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CUsurfref
    {
        public IntPtr Pointer;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 64)]
    public struct CUtensorMap
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16, ArraySubType = UnmanagedType.U8)]
        public ulong[] opaque;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CUgraphExec
    {
        public IntPtr Pointer;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CUipcMemHandle
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64, ArraySubType = UnmanagedType.I1)]
        public byte[] reserved;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CUexecAffinityParam
    {
        CUexecAffinityType type;
        CUexecAffinityParamUnion param;
    }
    [StructLayout(LayoutKind.Explicit)]
    public struct CUexecAffinityParamUnion
    {
        [FieldOffset(0)] public CUexecAffinitySmCount smCount;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CUexecAffinitySmCount
    {
        public uint val;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CUlinkState
    {
        public IntPtr Pointer;
    }
    public enum CujitInputType
    {
        Cubin = 0,
        Ptx,
        FatBinary,
        Object,
        Library,
        [Obsolete("Only valid with LTO-IR compiled with toolkits prior to CUDA 12.0")]
        Nvvm
    }
    [Flags]
    public enum CuMemHostAllocFlags
    {
        None = 0,
        Portable = 1,
        DeviceMap = 2,
        WriteCombined = 4
    }
    public enum CUmemAdvise
    {
        SetReadMostly = 1,
        UnsetReadMostly = 2,
        SetPreferredLocation = 3,
        UnsetPreferredLocation = 4,
        SetAccessedBy = 5,
        UnsetAccessedBy = 6
    }
    public enum CUmemRangeAttribute
    {
        ReadMostly = 1,
        PreferredLocation = 2,
        AccessedBy = 3,
        LastPrefetchLocation = 4
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CUmemAllocationProp
    {
        public CUmemAllocationType type;
        public CUmemAllocationHandleType requestedHandleTypes;
        public CUmemLocation location;
        public IntPtr win32HandleMetaData;
        public AllocFlags allocFlags;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct AllocFlags
    {
        public CUmemAllocationCompType compressionType;
        public byte gpuDirectRDMACapable;
        public CUmemCreateUsage usage;

        byte reserved0;
        byte reserved1;
        byte reserved2;
        byte reserved3;
    }
    public enum CUmemAllocationCompType : byte
    {
        None = 0x0,
        Generic = 0x1
    }
    public enum CUmemCreateUsage : ushort
    {
        None = 0x0,
        TilePool = 0x1
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CUmemGenericAllocationHandle
    {
        public ulong Pointer;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CUarrayMapInfo
    {
        [StructLayout(LayoutKind.Explicit)]
        public struct Resource
        {
            [FieldOffset(0)] public CUmipmappedArray mipmap;
            [FieldOffset(0)] public CUarray array;
        }
        [StructLayout(LayoutKind.Explicit)]
        public struct Subresource
        {
            public struct SparseLevel
            {
                public uint Level;
                public uint Layer;
                public uint OffsetX;
                public uint OffsetY;
                public uint OffsetZ;
                public uint ExtentWidth;
                public uint ExtentHeight;
                public uint ExtentDepth;
            }
            public struct Miptail
            {
                public uint Layer;
                public ulong Offset;
                public ulong Size;
            }
            [FieldOffset(0)] public SparseLevel sparseLevel;
            [FieldOffset(0)] public Miptail miptail;
        }
        public CuResourceType resourceType;
        public Resource resource;
        public CUarraySparseSubresourceType subresourceType;
        public Subresource subresource;
        public CUmemOperationType memOperationType;
        public CUmemHandleType memHandleType;
        public CUmemGenericAllocationHandle memHandle;
        public ulong offset;
        public uint deviceBitMask;
        public uint flags;
        public uint reserved0;
        public uint reserved1;
    }
    public enum CuResourceType
    {
        Array = 0x00,
        MipmappedArray = 0x01,
        Linear = 0x02,
        Pitch2D = 0x03
    }
    public enum CUmemOperationType
    {
        Map = 1,
        Unmap = 2
    }
    public enum CUmemHandleType
    {
        Generic = 0
    }
    public enum CUarraySparseSubresourceType
    {
        SparseLevel = 0,
        MipTail = 1
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CUexternalMemory
    {
        public IntPtr Pointer;
    }
    public enum CUtensorMapDataType
    {
        UInt8 = 0,
        UInt16,
        UInt32,
        Int32,
        UInt64,
        Int64,
        Float16,
        Float32,
        Float64,
        BFloat16,
        Float32Ftz,
        TFloat32,
        TFloat32Ftz
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CudaMemCpy3DPeer
    {
        public SizeT srcXInBytes;
        public SizeT srcY;
        public SizeT srcZ;
        public SizeT srcLOD;
        public CuMemoryType srcMemoryType;
        public IntPtr srcHost;
        public CUdeviceptr srcDevice;
        public CUarray srcArray;
        public CUcontext srcContext;
        public SizeT srcPitch;
        public SizeT srcHeight;
        public SizeT dstXInBytes;
        public SizeT dstY;
        public SizeT dstZ;
        public SizeT dstLOD;
        public CuMemoryType dstMemoryType;
        public IntPtr dstHost;
        public CUdeviceptr dstDevice;
        public CUarray dstArray;
        public CUcontext dstContext;
        public SizeT dstPitch;
        public SizeT dstHeight;
        public SizeT WidthInBytes;
        public SizeT Height;
        public SizeT Depth;
    }
    public enum CUmemRangeHandleType
    {
        DmaBufFd = 0x1,
        Max = 0x7FFFFFFF,
    }
    public enum CuCtxAttachFlags
    {
        None = 0
    }
    public enum CUmemAttachFlags
    {
        Global = 1,
        Host = 2,
        Single = 4
    }
    [Flags]
    public enum CUmemAllocationGranularityFlags
    {
        Minimum = 0x0,
        Recommended = 0x1
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CudaArrayDescriptor
    {
        public SizeT Width;
        public SizeT Height;
        public CuArrayFormat Format;
        public uint NumChannels;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CudaArraySparseProperties
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct TileExtent
        {
            public uint width;
            public uint height;
            public uint depth;
        }
        public TileExtent tileExtent;
        public uint miptailFirstLevel;
        public ulong miptailSize;
        public CuArraySparsePropertiesFlags flags;

        uint reserved0;
        uint reserved1;
        uint reserved2;
        uint reserved3;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CudaArray3DDescriptor
    {
        public SizeT Width;
        public SizeT Height;
        public SizeT Depth;
        public CuArrayFormat Format;
        public uint NumChannels;
        public CudaArray3DFlags Flags;
    }
    public enum CuTexRefSetArrayFlags
    {
        None = 0,
        OverrideFormat = 1
    }
    [Flags]
    public enum CuTexRefSetFlags
    {
        None = 0,
        ReadAsInteger = 1,
        NormalizedCoordinates = 2,
        SRgb = 0x10,
        DisableTrilinearOptimization = 0x20,
        SeamlessCubeMap = 0x40
    }
    public enum CuSurfRefSetFlags
    {
        None = 0
    }
    public enum CUstreamCaptureMode
    {
        Global = 0,
        Local = 1,
        Relaxed = 2
    }
    public enum CUdeviceP2PAttribute
    {
        PerformanceRank = 0x01,
        AccessSupported = 0x02,
        NativeAtomicSupported = 0x03,
        [Obsolete("use CudaArrayAccessAccessSupported instead")]
        AccessAccessSupported = 0x04,
        CudaArrayAccessAccessSupported = 0x04
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct CudaResourceDesc
    {
        public CudaResourceDesc(CudaMipmappedArray var)
        {
            resType = CuResourceType.MipmappedArray;
            flags = 0;
            res = new CudaResourceDescUnion();
            res.hArray = new CUarray();
            res.linear = new CudaResourceDescLinear();
            res.pitch2D = new CudaResourceDescPitch2D();
            res.hMipmappedArray = var.CuMipmappedArray;
        }
        public CudaResourceDesc(CudaDeviceVariable<float> var)
        {
            resType = CuResourceType.Linear;
            flags = 0;
            res = new CudaResourceDescUnion();
            res.hArray = new CUarray();
            res.hMipmappedArray = new CUmipmappedArray();
            res.pitch2D = new CudaResourceDescPitch2D();
            res.linear = new CudaResourceDescLinear();

            res.linear.devPtr = var.DevicePointer;
            res.linear.format = CuArrayFormat.Float;
            res.linear.numChannels = 1;
            res.linear.sizeInBytes = var.SizeInBytes;
        }
        public CudaResourceDesc(CudaDeviceVariable<int> var)
        {
            resType = CuResourceType.Linear;
            flags = 0;
            res = new CudaResourceDescUnion();
            res.hArray = new CUarray();
            res.hMipmappedArray = new CUmipmappedArray();
            res.pitch2D = new CudaResourceDescPitch2D();
            res.linear = new CudaResourceDescLinear();

            res.linear.devPtr = var.DevicePointer;
            res.linear.format = CuArrayFormat.SignedInt32;
            res.linear.numChannels = 1;
            res.linear.sizeInBytes = var.SizeInBytes;
        }
        public CudaResourceDesc(CudaDeviceVariable<short> var)
        {
            resType = CuResourceType.Linear;
            flags = 0;
            res = new CudaResourceDescUnion();
            res.hArray = new CUarray();
            res.hMipmappedArray = new CUmipmappedArray();
            res.pitch2D = new CudaResourceDescPitch2D();
            res.linear = new CudaResourceDescLinear();

            res.linear.devPtr = var.DevicePointer;
            res.linear.format = CuArrayFormat.SignedInt16;
            res.linear.numChannels = 1;
            res.linear.sizeInBytes = var.SizeInBytes;
        }
        public CudaResourceDesc(CudaDeviceVariable<sbyte> var)
        {
            resType = CuResourceType.Linear;
            flags = 0;
            res = new CudaResourceDescUnion();
            res.hArray = new CUarray();
            res.hMipmappedArray = new CUmipmappedArray();
            res.pitch2D = new CudaResourceDescPitch2D();
            res.linear = new CudaResourceDescLinear();

            res.linear.devPtr = var.DevicePointer;
            res.linear.format = CuArrayFormat.SignedInt8;
            res.linear.numChannels = 1;
            res.linear.sizeInBytes = var.SizeInBytes;
        }
        public CudaResourceDesc(CudaDeviceVariable<byte> var)
        {
            resType = CuResourceType.Linear;
            flags = 0;
            res = new CudaResourceDescUnion();
            res.hArray = new CUarray();
            res.hMipmappedArray = new CUmipmappedArray();
            res.pitch2D = new CudaResourceDescPitch2D();
            res.linear = new CudaResourceDescLinear();

            res.linear.devPtr = var.DevicePointer;
            res.linear.format = CuArrayFormat.UnsignedInt8;
            res.linear.numChannels = 1;
            res.linear.sizeInBytes = var.SizeInBytes;
        }
        public CudaResourceDesc(CudaDeviceVariable<ushort> var)
        {
            resType = CuResourceType.Linear;
            flags = 0;
            res = new CudaResourceDescUnion();
            res.hArray = new CUarray();
            res.hMipmappedArray = new CUmipmappedArray();
            res.pitch2D = new CudaResourceDescPitch2D();
            res.linear = new CudaResourceDescLinear();

            res.linear.devPtr = var.DevicePointer;
            res.linear.format = CuArrayFormat.UnsignedInt16;
            res.linear.numChannels = 1;
            res.linear.sizeInBytes = var.SizeInBytes;
        }
        public CudaResourceDesc(CudaDeviceVariable<uint> var)
        {
            resType = CuResourceType.Linear;
            flags = 0;
            res = new CudaResourceDescUnion();
            res.hArray = new CUarray();
            res.hMipmappedArray = new CUmipmappedArray();
            res.pitch2D = new CudaResourceDescPitch2D();
            res.linear = new CudaResourceDescLinear();

            res.linear.devPtr = var.DevicePointer;
            res.linear.format = CuArrayFormat.UnsignedInt32;
            res.linear.numChannels = 1;
            res.linear.sizeInBytes = var.SizeInBytes;
        }
        public CudaResourceDesc(CudaResourceDescLinear var)
        {
            resType = CuResourceType.Linear;
            flags = 0;
            res = new CudaResourceDescUnion();
            res.hArray = new CUarray();
            res.hMipmappedArray = new CUmipmappedArray();
            res.pitch2D = new CudaResourceDescPitch2D();
            res.linear = var;
        }
        public CudaResourceDesc(CudaResourceDescPitch2D var)
        {
            resType = CuResourceType.Pitch2D;
            flags = 0;
            res = new CudaResourceDescUnion();
            res.hArray = new CUarray();
            res.hMipmappedArray = new CUmipmappedArray();
            res.linear = new CudaResourceDescLinear();
            res.pitch2D = var;
        }
        public CuResourceType resType;
        public CudaResourceDescUnion res;
        public uint flags;
    }
    [StructLayout(LayoutKind.Explicit)]
    public struct CudaResourceDescUnion
    {
        [FieldOffset(0)] public CUarray hArray;
        [FieldOffset(0)] public CUmipmappedArray hMipmappedArray;
        [FieldOffset(0)] public CudaResourceDescLinear linear;
        [FieldOffset(0)] public CudaResourceDescPitch2D pitch2D;
        [FieldOffset(31 * 4)] int reserved;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CudaResourceDescLinear
    {
        public CUdeviceptr devPtr;
        public CuArrayFormat format;
        public uint numChannels;
        public SizeT sizeInBytes;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CudaResourceDescPitch2D
    {
        public CUdeviceptr devPtr;
        public CuArrayFormat format;
        public uint numChannels;
        public SizeT width;
        public SizeT height;
        public SizeT pitchInBytes;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CudaResourceViewDesc
    {
        public CUresourceViewFormat format;
        public SizeT width;
        public SizeT height;
        public SizeT depth;
        public uint firstMipmapLevel;
        public uint lastMipmapLevel;
        public uint firstLayer;
        public uint lastLayer;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16, ArraySubType = UnmanagedType.I4)]
        int[] _reserved;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CUsurfObject
    {
        public ulong Pointer;
    }
    public enum CUoutputMode
    {
        KeyValuePair = 0x00,
        Csv = 0x01
    }
    [StructLayout(LayoutKind.Explicit)]
    public struct CudaExternalMemoryHandleDesc
    {
        [FieldOffset(0)] public CUexternalMemoryHandleType type;
        [FieldOffset(8)] public HandleUnion handle;
        [FieldOffset(24)] public ulong size;
        [FieldOffset(32)] public CudaExternalMemory flags;
        [FieldOffset(100)] uint reserved;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CudaExternalMemoryBufferDesc
    {
        public ulong offset;
        public ulong size;
        public uint flags;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16, ArraySubType = UnmanagedType.U4)]
        uint[] reserved;
    }
    [Flags]
    public enum CuArraySparsePropertiesFlags : uint
    {
        None = 0,
        SingleMiptail = 0x1
    }
    [Flags]
    public enum CudaArray3DFlags
    {
        None = 0,
        [Obsolete("Since CUDA Version 4.0. Use <Layered> instead")]
        Array2D = 1,
        Layered = 1,
        SurfaceLdst = 2,
        Cubemap = 4,
        TextureGather = 8,
        DepthTexture = 0x10,
        ColorAttachment = 0x20,
        Sparse = 0x40,
        DeferredMapping = 0x80
    }
    public enum CUtensorMapInterleave
    {
        None = 0,
        Interleave16B,
        Interleave32B
    }
    public enum CUtensorMapSwizzle
    {
        None = 0,
        Swizzle32B,
        Swizzle64B,
        Swizzle128B
    }
    public enum CUtensorMapL2Promotion
    {
        None = 0,
        L264B,
        L2128B,
        L2256B
    }
    public class CudaMipmappedArray : IDisposable
    {
        CUmipmappedArray _mipmappedArray;
        CudaArray3DDescriptor _arrayDescriptor;
        CuResult _res;
        bool _disposed;
        bool _isOwner;
        public CudaMipmappedArray(CudaArray3DDescriptor descriptor, uint numMipmapLevels)
        {
            _mipmappedArray = new CUmipmappedArray();
            _arrayDescriptor = descriptor;

            _res = DriverApiNativeMethods.ArrayManagement.cuMipmappedArrayCreate(ref _mipmappedArray, ref _arrayDescriptor, numMipmapLevels);
            
            if (_res != CuResult.Success) throw new CudaException(_res);
            _isOwner = true;
        }
        public CudaMipmappedArray(CuArrayFormat format, SizeT width, SizeT height, SizeT depth, CudaMipmappedArrayNumChannels numChannels, CudaArray3DFlags flags, uint numMipmapLevels)
        {
            _mipmappedArray = new CUmipmappedArray();
            _arrayDescriptor = new CudaArray3DDescriptor();
            _arrayDescriptor.Width = width;
            _arrayDescriptor.Height = height;
            _arrayDescriptor.Depth = depth;
            _arrayDescriptor.NumChannels = (uint)numChannels;
            _arrayDescriptor.Flags = flags;
            _arrayDescriptor.Format = format;


            _res = DriverApiNativeMethods.ArrayManagement.cuMipmappedArrayCreate(ref _mipmappedArray, ref _arrayDescriptor, numMipmapLevels);
            
            if (_res != CuResult.Success) throw new CudaException(_res);
            _isOwner = true;
        }
        public CudaMipmappedArray(CUmipmappedArray handle, CuArrayFormat format, CudaMipmappedArrayNumChannels numChannels)
        {
            _mipmappedArray = handle;
            _arrayDescriptor = new CudaArray3DDescriptor();
            _arrayDescriptor.Format = format;
            _arrayDescriptor.NumChannels = (uint)numChannels;
            _isOwner = false;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool fDisposing)
        {
            if (fDisposing && !_disposed) {
                if (_isOwner) {
                    _res = DriverApiNativeMethods.ArrayManagement.cuMipmappedArrayDestroy(_mipmappedArray);
                }

                _disposed = true;
            }
        }
        public CUarray GetLevelAsCuArray(uint level)
        {
            var array = new CUarray();

            _res = DriverApiNativeMethods.ArrayManagement.cuMipmappedArrayGetLevel(ref array, _mipmappedArray, level);
            
            if (_res != CuResult.Success)
                throw new CudaException(_res);

            return array;
        }
        public CudaArraySparseProperties GetSparseProperties()
        {
            var sparseProperties = new CudaArraySparseProperties();

            _res = DriverApiNativeMethods.ArrayManagement.cuMipmappedArrayGetSparseProperties(ref sparseProperties, _mipmappedArray);
            
            if (_res != CuResult.Success)
                throw new CudaException(_res);

            return sparseProperties;
        }
        public CudaArrayMemoryRequirements GetMemoryRequirements(CUdevice device)
        {
            return _mipmappedArray.GetMemoryRequirements(device);
        }
        public CUmipmappedArray CuMipmappedArray => _mipmappedArray;
        public CudaArray3DDescriptor Array3DDescriptor => _arrayDescriptor;
        public SizeT Depth => _arrayDescriptor.Depth;
        public SizeT Height => _arrayDescriptor.Height;
        public SizeT Width => _arrayDescriptor.Width;
        public CudaArray3DFlags Flags => _arrayDescriptor.Flags;
        public CuArrayFormat Format => _arrayDescriptor.Format;
        public uint NumChannels => _arrayDescriptor.NumChannels;
        public bool IsOwner => _isOwner;
    }
    public enum CUtensorMapFloatOoBfill
    {
        None = 0,
        NanRequestZeroFma
    }
    public enum CUresourceViewFormat
    {
        None = 0x00,
        Uint1X8 = 0x01,
        Uint2X8 = 0x02,
        Uint4X8 = 0x03,
        Sint1X8 = 0x04,
        Sint2X8 = 0x05,
        Sint4X8 = 0x06,
        Uint1X16 = 0x07,
        Uint2X16 = 0x08,
        Uint4X16 = 0x09,
        Sint1X16 = 0x0a,
        Sint2X16 = 0x0b,
        Sint4X16 = 0x0c,
        Uint1X32 = 0x0d,
        Uint2X32 = 0x0e,
        Uint4X32 = 0x0f,
        Sint1X32 = 0x10,
        Sint2X32 = 0x11,
        Sint4X32 = 0x12,
        Float1X16 = 0x13,
        Float2X16 = 0x14,
        Float4X16 = 0x15,
        Float1X32 = 0x16,
        Float2X32 = 0x17,
        Float4X32 = 0x18,
        UnsignedBc1 = 0x19,
        UnsignedBc2 = 0x1a,
        UnsignedBc3 = 0x1b,
        UnsignedBc4 = 0x1c,
        SignedBc4 = 0x1d,
        UnsignedBc5 = 0x1e,
        SignedBc5 = 0x1f,
        UnsignedBc6H = 0x20,
        SignedBc6H = 0x21,
        UnsignedBc7 = 0x22
    }
    public enum CUexternalMemoryHandleType
    {
        OpaqueFd = 1,
        OpaqueWin32 = 2,
        OpaqueWin32Kmt = 3,
        D3D12Heap = 4,
        D3D12Resource = 5,
        D3D11Resource = 6,
        D3D11ResourceKmt = 7,
        NvSciBuf = 8
    }
    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
    public struct HandleUnion
    {
        [FieldOffset(0)] public int fd;
        [FieldOffset(0)] public Win32Handle win32;
        [FieldOffset(0)] public IntPtr nvSciBufObject;
    }
    [Flags]
    public enum CudaExternalMemory
    {
        Nothing = 0x0,
        Dedicated = 0x01,
    }
    public enum CudaMipmappedArrayNumChannels
    {
        One = 1,
        Two = 2,
        Four = 4
    }
    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
    public struct Win32Handle
    {
        [FieldOffset(0)] public IntPtr handle;
        [FieldOffset(8)] [MarshalAs(UnmanagedType.LPStr)]
        public string name;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CudaExternalMemoryMipmappedArrayDesc
    {
        public ulong offset;
        public CudaArray3DDescriptor arrayDesc;
        public uint numLevels;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16, ArraySubType = UnmanagedType.U4)]
        uint[] reserved;
    }
    [StructLayout(LayoutKind.Explicit)]
    public struct CudaExternalSemaphoreHandleDesc
    {
        [FieldOffset(0)] public CUexternalSemaphoreHandleType type;
        [FieldOffset(8)] public HandleUnion handle;
        [FieldOffset(32)] public uint flags;
        [FieldOffset(92)] uint reserved;
    }
    public enum CUexternalSemaphoreHandleType
    {
        OpaqueFd = 1,
        OpaqueWin32 = 2,
        OpaqueWin32Kmt = 3,
        D3D12DFence = 4,
        D3D11Fence = 5,
        NvSciSync = 6,
        D3D11KeyedMutex = 7,
        D3D11KeyedMutexKmt = 8,
        TimelineSemaphoreFd = 9,
        TimelineSemaphoreWin32 = 10
    }
    [Flags]
    public enum CUgraphInstantiateFlags : ulong
    {
        None = 0,
        AutoFreeOnLaunch = 1,
        Upload = 2,
        DeviceLaunch = 4,
        UseNodePriority = 8
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct CudaGraphInstantiateParams
    {
        public ulong flags;
        public CUstream hUploadStream;
        public CUgraphNode hErrNode_out;
        public CUgraphInstantiateResult result_out;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct CUgraphExecUpdateResultInfo
    {
        public CUgraphExecUpdateResult result;
        public CUgraphNode errorNode;
        public CUgraphNode errorFromNode;
    }
    public enum CUgraphExecUpdateResult
    {
        Success = 0x0,
        Error = 0x1,
        ErrorTopologyChanged = 0x2,
        ErrorNodeTypeChanged = 0x3,
        ErrorFunctionChanged = 0x4,
        ErrorParametersChanged = 0x5,
        ErrorNotSupported = 0x6,
        ErrorUnsupportedFunctionChange = 0x7,
        ErrorAttributesChanged = 0x8
    }
    public enum CUgraphInstantiateResult
    {
        Success = 0,
        Error = 1,
        InvalidStructure = 2,
        NodeOperationNotSupported = 3,
        MultipleCtxsNotSupported = 4
    }
    [Flags]
    public enum CUgraphDebugDotFlags
    {
        None = 0,
        Verbose = 1 << 0,
        RuntimeTypes = 1 << 1,
        KernelNodeParams = 1 << 2,
        MemcpyNodeParams = 1 << 3,
        MemsetNodeParams = 1 << 4,
        HostNodeParams = 1 << 5,
        EventNodeParams = 1 << 6,
        ExtSemasSignalNodeParams = 1 << 7,
        ExtSemasWaitNodeParams = 1 << 8,
        KernelNodeAttributes = 1 << 9,
        Handles = 1 << 10,
        MemAllocNodeParams = 1 << 11,
        MemFreeNodeParams = 1 << 12,
        BatchMemOpNodeParams = 1 << 13,
        ExtraTopoInfo = 1 << 14
    }
    [Flags]
    public enum CUuserObjectFlags
    {
        None = 0,
        NoDestructorSync = 1
    }
    [Flags]
    public enum CUuserObjectRetainFlags
    {
        None = 0,
        Move = 1
    }
    public enum CUflushGpuDirectRdmaWritesTarget
    {
        CurrentCtx = 0
    }
    public enum CUflushGpuDirectRdmaWritesScope
    {
        WritesToOwner = 100,
        WritesToAllDevices = 200
    }
    public interface ICudaVectorType
    {
        uint Size { get; }
    }
    public interface ICudaVectorTypeForArray
    {
        uint GetChannelNumber();
        CuArrayFormat GetCuArrayFormat();
    }
    public enum CugpuDirectRdmaWritesOrdering
    {
        None = 0,
        Owner = 100,
        AllDevices = 200
    }
    [Flags]
    public enum CUflushGpuDirectRdmaWritesOptions
    {
        None = 0,
        Host = 1 << 0,
        Memops = 1 << 1
    }
    public enum CuComputeMode
    {
        Default = 0,
        Prohibited = 2,
        ExclusiveProcess = 2
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct Dim3
    {
        public uint x;
        public uint y;
        public uint z;
        public static Dim3 Add(Dim3 src, Dim3 value)
        {
            var ret = new Dim3(src.x + value.x, src.y + value.y, src.z + value.z);
            return ret;
        }
        public static Dim3 Add(Dim3 src, uint value)
        {
            var ret = new Dim3(src.x + value, src.y + value, src.z + value);
            return ret;
        }
        public static Dim3 Add(uint src, Dim3 value)
        {
            var ret = new Dim3(src + value.x, src + value.y, src + value.z);
            return ret;
        }
        public static Dim3 Subtract(Dim3 src, Dim3 value)
        {
            var ret = new Dim3(src.x - value.x, src.y - value.y, src.z - value.z);
            return ret;
        }
        public static Dim3 Subtract(Dim3 src, uint value)
        {
            var ret = new Dim3(src.x - value, src.y - value, src.z - value);
            return ret;
        }
        public static Dim3 Subtract(uint src, Dim3 value)
        {
            var ret = new Dim3(src - value.x, src - value.y, src - value.z);
            return ret;
        }
        public static Dim3 Multiply(Dim3 src, Dim3 value)
        {
            var ret = new Dim3(src.x * value.x, src.y * value.y, src.z * value.z);
            return ret;
        }
        public static Dim3 Multiply(Dim3 src, uint value)
        {
            var ret = new Dim3(src.x * value, src.y * value, src.z * value);
            return ret;
        }
        public static Dim3 Multiply(uint src, Dim3 value)
        {
            var ret = new Dim3(src * value.x, src * value.y, src * value.z);
            return ret;
        }
        public static Dim3 Divide(Dim3 src, Dim3 value)
        {
            var ret = new Dim3(src.x / value.x, src.y / value.y, src.z / value.z);
            return ret;
        }
        public static Dim3 Divide(Dim3 src, uint value)
        {
            var ret = new Dim3(src.x / value, src.y / value, src.z / value);
            return ret;
        }
        public static Dim3 Divide(uint src, Dim3 value)
        {
            var ret = new Dim3(src / value.x, src / value.y, src / value.z);
            return ret;
        }
        public static Dim3 operator +(Dim3 src, Dim3 value)
        {
            return Add(src, value);
        }
        public static Dim3 operator +(Dim3 src, uint value)
        {
            return Add(src, value);
        }
        public static Dim3 operator +(uint src, Dim3 value)
        {
            return Add(src, value);
        }
        public static Dim3 operator -(Dim3 src, Dim3 value)
        {
            return Subtract(src, value);
        }
        public static Dim3 operator -(Dim3 src, uint value)
        {
            return Subtract(src, value);
        }
        public static Dim3 operator -(uint src, Dim3 value)
        {
            return Subtract(src, value);
        }
        public static Dim3 operator *(Dim3 src, Dim3 value)
        {
            return Multiply(src, value);
        }
        public static Dim3 operator *(Dim3 src, uint value)
        {
            return Multiply(src, value);
        }
        public static Dim3 operator *(uint src, Dim3 value)
        {
            return Multiply(src, value);
        }
        public static Dim3 operator /(Dim3 src, Dim3 value)
        {
            return Divide(src, value);
        }
        public static Dim3 operator /(Dim3 src, uint value)
        {
            return Divide(src, value);
        }
        public static Dim3 operator /(uint src, Dim3 value)
        {
            return Divide(src, value);
        }
        public static bool operator ==(Dim3 src, Dim3 value)
        {
            return src.Equals(value);
        }
        public static bool operator !=(Dim3 src, Dim3 value)
        {
            return !(src == value);
        }
        public static implicit operator Dim3(int value)
        {
            return new Dim3(value);
        }
        public static implicit operator Dim3(uint value)
        {
            return new Dim3(value);
        }
        public override bool Equals(object? obj)
        {
            if (obj is not Dim3 value) 
                return false;

            var ret = true;
            ret &= this.x == value.x;
            ret &= this.y == value.y;
            ret &= this.z == value.z;
            return ret;
        }
        public bool Equals(Dim3 value)
        {
            var ret = true;
            ret &= this.x == value.x;
            ret &= this.y == value.y;
            ret &= this.z == value.z;
            return ret;
        }
        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode();
        }
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "({0}; {1}; {2})", this.x, this.y, this.z);
        }
        public Dim3(uint xValue, uint yValue, uint zValue)
        {
            this.x = xValue;
            this.y = yValue;
            this.z = zValue;
        }
        public Dim3(uint xValue, uint yValue)
        {
            this.x = xValue;
            this.y = yValue;
            this.z = 1;
        }
        public Dim3(uint val)
        {
            this.x = val;
            this.y = 1;
            this.z = 1;
        }
        public Dim3(int xValue, int yValue, int zValue)
        {
            this.x = (uint)xValue;
            this.y = (uint)yValue;
            this.z = (uint)zValue;
        }
        public Dim3(int xValue, int yValue)
        {
            this.x = (uint)xValue;
            this.y = (uint)yValue;
            this.z = 1;
        }
        public Dim3(int val)
        {
            this.x = (uint)val;
            this.y = 1;
            this.z = 1;
        }
        public static Dim3 Min(Dim3 aValue, Dim3 bValue)
        {
            return new Dim3(System.Math.Min(aValue.x, bValue.x), System.Math.Min(aValue.y, bValue.y), System.Math.Min(aValue.z, bValue.z));
        }
        public static Dim3 Max(Dim3 aValue, Dim3 bValue)
        {
            return new Dim3(System.Math.Max(aValue.x, bValue.x), System.Math.Max(aValue.y, bValue.y), System.Math.Max(aValue.z, bValue.z));
        }
        public static uint SizeOf => (uint)Marshal.SizeOf(typeof(Dim3));
        public uint Size => (uint)Marshal.SizeOf(this);
    }

    public enum CusolverStatus
    {
        Success = 0,
        NotInititialized = 1,
        AllocFailed = 2,
        InvalidValue = 3,
        ArchMismatch = 4,
        MappingError = 5,
        ExecutionFailed = 6,
        InternalError = 7,
        MatrixTypeNotSupported = 8,
        NotSupported = 9,
        ZeroPivot = 10,
        InvalidLicense = 11,
        CusolverStatusIrsParamsNotInitialized = 12,
        CusolverStatusIrsParamsInvalid = 13,

        CusolverStatusIrsParamsInvalidPrec = 14,
        CusolverStatusIrsParamsInvalidRefine = 15,
        CusolverStatusIrsParamsInvalidMaxiter = 16,
        CusolverStatusIrsInternalError = 20,
        CusolverStatusIrsNotSupported = 21,
        CusolverStatusIrsOutOfRange = 22,
        CusolverStatusIrsNrhsNotSupportedForRefineGmres = 23,
        CusolverStatusIrsInfosNotInitialized = 25,
        CusolverStatusIrsInfosNotDestroyed = 26,
        CusolverStatusIrsMatrixSingular = 30,
        CusolverStatusInvalidWorkspace = 31
    }
    public enum CusolverEigType
    {
        Type1 = 1,
        Type2 = 2,
        Type3 = 3
    }
    public enum CusolverEigMode
    {
        NoVector = 0,
        Vector = 1
    }
    public enum CusolverEigRange
    {
        All = 1001,
        I = 1002,
        V = 1003,
    }
    public enum CusolverIrsRefinement
    {
        NotSet = 1100,
        None = 1101,
        Classical = 1102,
        ClassicalGmres = 1103,
        Gmres = 1104,
        GmresGmres = 1105,
        GmresNopcond = 1106,

        PrecDd = 1150,
        PrecSs = 1151,
        PrecSht = 1152,
    }

    public enum CusolverPrecType
    {
        CusolverR8I = 1201,
        CusolverR8U = 1202,
        CusolverR64F = 1203,
        CusolverR32F = 1204,
        CusolverR16F = 1205,
        CusolverR16Bf = 1206,
        CusolverRTf32 = 1207,
        CusolverRAp = 1208,
        CusolverC8I = 1211,
        CusolverC8U = 1212,
        CusolverC64F = 1213,
        CusolverC32F = 1214,
        CusolverC16F = 1215,
        CusolverC16Bf = 1216,
        CusolverCTf32 = 1217,
        CusolverCAp = 1218,
    }

    public enum CusolverAlgMode
    {
        CusolverAlg0 = 0,
        CusolverAlg1 = 1,
        CusolverAlg2 = 2
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CusolverDnHandle
    {
        public IntPtr Pointer;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct SyevjInfo
    {
        public IntPtr Pointer;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct GesvdjInfo
    {
        public IntPtr Pointer;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CusolverDnIrsParams
    {
        public IntPtr Pointer;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CusolverDnIrsInfos
    {
        public IntPtr Pointer;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CusolverDnParams
    {
        public IntPtr Pointer;
    }
    public enum CusolverDnFunction
    {
        CusolverdnGetrf = 0,
        CusolverdnPotrf = 1
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CusolverSpHandle
    {
        public IntPtr Pointer;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CusolverRfHandle
    {
        public IntPtr Pointer;
    }
    public enum ResetValuesFastMode
    {
        Off = 0,
        On = 1
    }
    public enum MatrixFormat
    {
        Csr = 0,
        Csc = 1
    }
    public enum UnitDiagonal
    {
        StoredL = 0,
        StoredU = 1,
        AssumedL = 2,
        AssumedU = 3
    }
    public enum Factorization
    {
        Alg0 = 0,
        Alg1 = 1,
        Alg2 = 2,
    }
    public enum TriangularSolve
    {
        Alg1 = 1,
        Alg2 = 2,
        Alg3 = 3
    }
    public enum NumericBoostReport
    {
        NotUsed = 0,
        Used = 1
    }
}