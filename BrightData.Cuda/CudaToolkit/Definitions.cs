using System;
using System.Globalization;
using System.Runtime.InteropServices;
using BrightData.Cuda.CudaToolkit.Types;

namespace BrightData.Cuda.CudaToolkit
{
    internal enum CuResult
    {
        Success = 0,
        ErrorInvalidValue = 1,
        ErrorOutOfMemory = 2,
        ErrorNotInitialized = 3,
        ErrorUninitialized = 4,
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
    
    internal enum CuBlasStatus
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
    public enum CuMemAllocationHandleType
    {
        None = 0,
        PosixFileDescriptor = 0x1,
        Win32 = 0x2,
        Win32Kmt = 0x4
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CuContext
    {
        public nint Pointer;
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
        readonly ulong p2pToken;
        readonly uint vaSpaceToken;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct CuMemoryPool
    {
        public nint Pointer;
    }
    internal enum CuPointerAttribute
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
    internal enum CuStreamFlags : uint
    {
        None = 0,
        Default = 0x0,
        NonBlocking = 0x1,
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct CuEvent
    {
        public nint Pointer;
    }
    internal delegate void CuStreamCallback(CuStream hStream, CuResult status, nint userData);
    [Flags]
    internal enum CuStreamAddCallbackFlags
    {
        None = 0x0,
    }
    [Flags]
    internal enum CuStreamWaitValueFlags
    {
        Geq = 0x0,
        Eq = 0x1,
        And = 0x2,
        NOr = 0x3,
        Flush = 1 << 30
    }
    [Flags]
    internal enum CuStreamWriteValueFlags
    {
        Default = 0x0,
        NoMemoryBarrier = 0x1
    }
    internal enum CuStreamAttrId
    {
        AccessPolicyWindow = 1,
        SynchronizationPolicy = 3
    }
    [StructLayout(LayoutKind.Explicit)]
    internal struct CuStreamAttrValue
    {
        [FieldOffset(0)] public CuAccessPolicyWindow accessPolicyWindow;
        [FieldOffset(0)] public CuSynchronizationPolicy syncPolicy;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct CuAccessPolicyWindow
    {
        public nint base_ptr;
        public SizeT num_bytes;
        public float hitRatio;
        public CuAccessProperty hitProp;
        public CuAccessProperty missProp;
    }
    internal enum CuSynchronizationPolicy
    {
        Auto = 1,
        Spin = 2,
        Yield = 3,
        BlockingSync = 4
    }
    internal enum CuAccessProperty
    {
        Normal = 0,
        Streaming = 1,
        Persisting = 2
    }
    internal enum CuModuleLoadingMode
    {
        EagerLoading = 0x1,
        LazyLoading = 0x2,
    }
    internal enum CuMemPoolAttribute
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
    internal enum CuExecAffinityType
    {
        SmCount = 0,
        Max
    }
    internal enum CuGraphMemAttribute
    {
        UsedMemCurrent,
        UsedMemHigh,
        ReservedMemCurrent,
        ReservedMemHigh
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct CuUuid
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16, ArraySubType = UnmanagedType.I1)]
        public byte[] bytes;
    }
    internal enum CuLaunchAttributeId
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
    internal struct CuLaunchAttribute
    {
        public CuLaunchAttributeId Id;
        public int Pad;
        public CuLaunchAttributeValue Value;
    }
    internal struct CuLaunchConfig
    {
        public uint GridDimX;
        public uint GridDimY;
        public uint GridDimZ;
        public uint BlockDimX;
        public uint BlockDimY;
        public uint BlockDimZ;
        public uint SharedMemBytes;
        public CuStream HStream;
        public CuLaunchAttribute[]? Attrs;
    }
    [StructLayout(LayoutKind.Explicit)]
    internal struct CuLaunchAttributeValue
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct ClusterDim
        {
            public uint x;
            public uint y;
            public uint z;
        }
        [StructLayout(LayoutKind.Sequential)]
        internal struct ProgrammaticEvent
        {
            public CuEvent event_;
            public int flags;
            public int triggerAtBlockStart;
        }
        [FieldOffset(0)] readonly CuAccessPolicyWindow accessPolicyWindow;
        [FieldOffset(0)] readonly int cooperative;
        [FieldOffset(0)] readonly CuSynchronizationPolicy syncPolicy;
        [FieldOffset(0)] readonly ClusterDim clusterDim;
        [FieldOffset(0)] readonly CuClusterSchedulingPolicy clusterSchedulingPolicyPreference;
        [FieldOffset(0)] readonly int programmaticStreamSerializationAllowed;
        [FieldOffset(0)] readonly ProgrammaticEvent programmaticEvent;
        [FieldOffset(0)] readonly int priority;
        [FieldOffset(0)] readonly CuLaunchMemSyncDomainMap memSyncDomainMap;
        [FieldOffset(0)] readonly CuLaunchMemSyncDomain memSyncDomain;
        [FieldOffset(60)] public int pad;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct CuLaunchMemSyncDomainMap
    {
        public byte default_;
        public byte remote;
    }
    internal enum CuLaunchMemSyncDomain
    {
        Default = 0,
        Remote = 1
    }
    internal enum CuClusterSchedulingPolicy
    {
        Default = 0,
        Spread = 1,
        LoadBalancing = 2
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct CuLibrary
    {
        public nint Pointer;
    }
    
    internal enum CuFuncCache
    {
        PreferNone = 0x00,
        PreferShared = 0x01,
        PreferL1 = 0x02,
        PreferEqual = 0x03
    }
    internal enum CuFunctionAttribute
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
    internal enum CuSharedCarveout
    {
        Default = -1,
        MaxShared = 100,
        MaxL1 = 0
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct CudaMemCpy2D
    {
        public SizeT srcXInBytes;
        public SizeT srcY;
        public CuMemoryType srcMemoryType;
        public nint srcHost;
        public CuDevicePtr srcDevice;
        public CuArray srcArray;
        public SizeT srcPitch;
        public SizeT dstXInBytes;
        public SizeT dstY;
        public CuMemoryType dstMemoryType;
        public nint dstHost;
        public CuDevicePtr dstDevice;
        public CuArray dstArray;
        public SizeT dstPitch;
        public SizeT WidthInBytes;
        public SizeT Height;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct CuArray
    {
        public nint Pointer;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct CudaArrayMemoryRequirements
    {
        public SizeT size;
        public SizeT alignment;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4, ArraySubType = UnmanagedType.U4)]
        readonly uint[] reserved;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct CuMemPoolPtrExportData
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64, ArraySubType = UnmanagedType.U1)]
        readonly byte[] reserved;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct CuMemPoolProps
    {
        public CuMemAllocationType allocType;
        public CuMemAllocationHandleType handleTypes;
        public CuMemLocation location;
        public nint win32SecurityAttributes;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64, ArraySubType = UnmanagedType.U1)]
        readonly byte[] reserved;
    }
    internal enum CuMemAllocationType
    {
        Invalid = 0x0,
        Pinned = 0x1
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct CuMemLocation
    {
        public CuMemLocationType type;
        public int id;
    }
    internal enum CuMemLocationType
    {
        Invalid = 0x0,
        Device = 0x1
    }
    [Flags]
    internal enum CuMemAccessFlags
    {
        ProtNone = 0x1,
        ProtRead = 0x2,
        ProtReadWrite = 0x3
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct CuMemAccessDesc
    {
        public CuMemLocation location;
        public CuMemAccessFlags flags;
    }
    internal enum CuJitOption
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
    internal enum CuLibraryOption
    {
        HostUniversalFunctionAndDataTable = 0,
        BinaryIsPreserved = 1,
        NumOptions
    }
    [Flags]
    internal enum CuEventRecordFlags : uint
    {
        Default = 0x0,
        External = 0x1
    }
    [Flags]
    internal enum CuEventFlags
    {
        Default = 0,
        BlockingSync = 1,
        DisableTiming = 2,
        InterProcess = 4
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct CudaLaunchParams
    {
        public CuFunction function;
        public uint gridDimX;
        public uint gridDimY;
        public uint gridDimZ;
        public uint blockDimX;
        public uint blockDimY;
        public uint blockDimZ;
        public uint sharedMemBytes;
        public CuStream hStream;
        public nint kernelParams;
    }
    [Flags]
    internal enum CudaCooperativeLaunchMultiDeviceFlags
    {
        None = 0,
        NoPreLaunchSync = 0x01,
        NoPostLaunchSync = 0x02,
    }
    internal enum CuSharedConfig
    {
        DefaultBankSize = 0x00,
        FourByteBankSize = 0x01,
        EightByteBankSize = 0x02
    }
    internal delegate SizeT DelCuOccupancyB2DSize(int aBlockSize);
    internal enum CuOccupancyFlags
    {
        Default = 0,
        DisableCachingOverride = 1
    }
    internal enum Operation
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
        public nint Pointer;
    }
    internal enum CudaDataType
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
    internal enum FillMode
    {
        Lower = 0,
        Upper = 1,
        Full = 2
    }
    internal enum DiagType
    {
        NonUnit = 0,
        Unit = 1
    }
    internal enum SideMode
    {
        Left = 0,
        Right = 1
    }
    internal enum PointerMode
    {
        Host = 0,
        Device = 1
    }
    internal enum AtomicsMode
    {
        NotAllowed = 0,
        Allowed = 1
    }
    internal enum GemmAlgo
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
    internal enum Math
    {
        DefaultMath = 0,
        [Obsolete("deprecated, same effect as using CUBLAS_COMPUTE_32F_FAST_16F, will be removed in a future release")]
        TensorOpMath = 1,
        PedanticMath = 2,
        Tf32TensorOpMath = 3,
        DisallowReducedPrecisionReduction = 16
    }
    internal enum ComputeType
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
    internal enum DataType
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
    internal delegate void CublasLogCallback([MarshalAs(UnmanagedType.LPStr)] string msg);
    [StructLayout(LayoutKind.Sequential)]
    internal struct CuSparseMatDescriptor
    {
        public nint Handle;
    }
    [Flags]
    internal enum CuInitializationFlags : uint
    {
        None = 0
    }
    [StructLayout(LayoutKind.Explicit)]
    internal struct CudaExternalSemaphoreWaitParams
    {
        [StructLayout(LayoutKind.Explicit)]
        internal struct Parameters
        {
            [StructLayout(LayoutKind.Sequential)]
            internal struct Fence
            {
                public ulong value;
            }
            [FieldOffset(0)] public Fence fence;
            [StructLayout(LayoutKind.Sequential)]
            internal struct NvSciSync
            {
                public nint fence;
            }
            [FieldOffset(8)] public NvSciSync nvSciSync;
            [StructLayout(LayoutKind.Sequential)]
            internal struct KeyedMutex
            {
                public ulong key;
                public uint timeoutMs;
            }
            [FieldOffset(16)] public KeyedMutex keyedMutex;

            [FieldOffset(20)] readonly uint reserved;
        }
        [FieldOffset(0)] public Parameters parameters;
        [FieldOffset(72)] public uint flags;

        [FieldOffset(136)] readonly uint reserved;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct CudaMemAllocNodeParamsInternal
    {
        public CuMemPoolProps poolProps;
        public nint accessDescs;
        public SizeT accessDescCount;
        public SizeT bytesize;
        public CuDevicePtr dptr;
    }

    internal struct CuLaunchConfigInternal
    {
        public uint GridDimX;
        public uint GridDimY;
        public uint GridDimZ;
        public uint BlockDimX;
        public uint BlockDimY;
        public uint BlockDimZ;
        public uint SharedMemBytes;
        public CuStream HStream;
        public nint Attrs;
        public uint NumAttrs;
    }
    internal class CudaBatchMemOpNodeParams
    {
        public CuContext Ctx;
        public CuStreamBatchMemOpParams[]? ParamArray;
        public uint Flags;
    }
    [StructLayout(LayoutKind.Explicit)]
    internal struct CuStreamBatchMemOpParams
    {
        [FieldOffset(0)] public CuStreamBatchMemOpType operation;
        [FieldOffset(0)] public CuStreamMemOpWaitValueParams waitValue;
        [FieldOffset(0)] public CuStreamMemOpWriteValueParams writeValue;
        [FieldOffset(0)] public CuStreamMemOpFlushRemoteWritesParams flushRemoteWrites;
        [FieldOffset(0)] public CuStreamMemOpMemoryBarrierParams memoryBarrier;
        [FieldOffset(5 * 8)] readonly ulong pad;
    }
    internal enum CuStreamBatchMemOpType
    {
        WaitValue32 = 1,
        WriteValue32 = 2,
        WaitValue64 = 4,
        WriteValue64 = 5,
        Barrier = 6,
        FlushRemoteWrites = 3
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct CuStreamMemOpWaitValueParams
    {
        public CuStreamBatchMemOpType operation;
        public CuDevicePtr address;
        public CuUint3264Union value;
        public uint flags;
        public CuDevicePtr alias;
    }
    [StructLayout(LayoutKind.Explicit)]
    internal struct CuUint3264Union
    {
        [FieldOffset(0)] public uint value;
        [FieldOffset(0)] public ulong value64;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct CuStreamMemOpWriteValueParams
    {
        public CuStreamBatchMemOpType operation;
        public CuDevicePtr address;
        public CuUint3264Union value;
        public uint flags;
        public CuDevicePtr alias;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct CudaMemAllocNodeParams
    {
        public CuMemPoolProps poolProps;
        public CuMemAccessDesc[] accessDescs;
        public SizeT bytesize;
        public CuDevicePtr dptr;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct CudaHostNodeParams
    {
        public CUhostFn fn;
        public nint userData;
    }
    internal delegate void CUhostFn(nint userData);
    [StructLayout(LayoutKind.Sequential)]
    internal struct CuStreamMemOpFlushRemoteWritesParams
    {
        public CuStreamBatchMemOpType operation;
        public uint flags;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct CuStreamMemOpMemoryBarrierParams
    {
        public CuStreamBatchMemOpType operation;
        public uint flags;
    }
    internal class CudaExtSemWaitNodeParams
    {
        public CuExternalSemaphore[]? ExtSemArray;
        public CudaExternalSemaphoreWaitParams[]? ParamsArray;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct CuExternalSemaphore
    {
        public nint Pointer;
    }
    internal class CudaExtSemSignalNodeParams
    {
        public CuExternalSemaphore[]? ExtSemArray;
        public CudaExternalSemaphoreSignalParams[]? ParamsArray;
    }
    [StructLayout(LayoutKind.Explicit)]
    internal struct CudaExternalSemaphoreSignalParams
    {
        [StructLayout(LayoutKind.Explicit)]
        internal struct Parameters
        {
            [StructLayout(LayoutKind.Sequential)]
            internal struct Fence
            {
                public ulong value;
            }
            [FieldOffset(0)] public Fence fence;
            [StructLayout(LayoutKind.Sequential)]
            internal struct NvSciSync
            {
                public nint fence;
            }
            [FieldOffset(8)] public NvSciSync nvSciSync;
            [StructLayout(LayoutKind.Sequential)]
            internal struct KeyedMutex
            {
                public ulong key;
            }
            [FieldOffset(16)] public KeyedMutex keyedMutex;
            [FieldOffset(68)] readonly uint reserved;
        }
        [FieldOffset(0)] public Parameters parameters;
        [FieldOffset(72)] public uint flags;

        [FieldOffset(136)] readonly uint reserved;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct CudaMemCpy3D
    {
        public SizeT srcXInBytes;
        public SizeT srcY;
        public SizeT srcZ;
        public SizeT srcLOD;
        public CuMemoryType srcMemoryType;
        public nint srcHost;
        public CuDevicePtr srcDevice;
        public CuArray srcArray;
        public nint reserved0;
        public SizeT srcPitch;
        public SizeT srcHeight;
        public SizeT dstXInBytes;
        public SizeT dstY;
        public SizeT dstZ;
        public SizeT dstLOD;
        public CuMemoryType dstMemoryType;
        public nint dstHost;
        public CuDevicePtr dstDevice;
        public CuArray dstArray;
        public nint reserved1;
        public SizeT dstPitch;
        public SizeT dstHeight;
        public SizeT WidthInBytes;
        public SizeT Height;
        public SizeT Depth;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct CudaMemsetNodeParams
    {
        public CuDevicePtr dst;
        public SizeT pitch;
        public uint value;
        public uint elementSize;
        public SizeT width;
        public SizeT height;
        public static CudaMemsetNodeParams Init<T>(CudaDeviceVariable<T> deviceVariable, uint value) where T : struct
        {
            var para = new CudaMemsetNodeParams {
                dst = deviceVariable.DevicePointer,
                pitch = deviceVariable.SizeInBytes,
                value = value,
                elementSize = deviceVariable.TypeSize,
                width = deviceVariable.SizeInBytes,
                height = 1
            };

            return para;
        }
    }
    internal enum CuGraphNodeType
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
    internal struct CudaKernelNodeParams
    {
        public CuFunction func;
        public uint gridDimX;
        public uint gridDimY;
        public uint gridDimZ;
        public uint blockDimX;
        public uint blockDimY;
        public uint blockDimZ;
        public uint sharedMemBytes;
        public nint kernelParams;
        public nint extra;
        readonly CuKernel kern;
        readonly CuContext ctx;
    }
    internal enum CuKernelNodeAttrId
    {
        AccessPolicyWindow = 1,
        Cooperative = 2
    }
    [StructLayout(LayoutKind.Explicit)]
    internal struct CuKernelNodeAttrValue
    {
        [FieldOffset(0)] public CuAccessPolicyWindow accessPolicyWindow;
        [FieldOffset(0)] public int cooperative;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct CuGraph
    {
        public nint Pointer;
    }
    internal struct CudaBatchMemOpNodeParamsInternal
    {
        public CuContext Ctx;
        public uint Count;
        public nint ParamArray;
        public uint Flags;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct CuUserObject
    {
        public nint Pointer;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct Luid
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8, ArraySubType = UnmanagedType.I1)]
        public byte[] bytes;
    }
    internal enum CuArrayFormat
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
    internal enum CuDeviceAttribute
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
    internal enum NvSciSyncAttr
    {
        Signal = 0x01,
        Wait = 0x02,
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct CuIpcEventHandle
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64, ArraySubType = UnmanagedType.I1)]
        public byte[] reserved;
    }
    [Flags]
    internal enum CuCtxFlags
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
    internal struct CuTexRef
    {
        public nint Pointer;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct CuMipMappedArray
    {
        public nint Pointer;
        public CudaArrayMemoryRequirements GetMemoryRequirements(CuDevice device)
        {
            var temp = new CudaArrayMemoryRequirements();
            var res = DriverApiNativeMethods.ArrayManagement.cuMipmappedArrayGetMemoryRequirements(ref temp, this, device);
            
            if (res != CuResult.Success) throw new CudaException(res);
            return temp;
        }
    }
    internal enum CuStreamCaptureStatus
    {
        None = 0,
        Active = 1,
        Invalidated = 2
    }
    internal enum CuLimit
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
    internal enum CtxEnablePeerAccessFlags : uint
    {
        None = 0
    }
    [Flags]
    internal enum CuMemHostRegisterFlags
    {
        None = 0,
        Portable = 1,
        DeviceMap = 2,
        IoMemory = 0x04,
        ReadOnly = 0x08
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct CuSurfRef
    {
        public nint Pointer;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 64)]
    internal struct CuTensorMap
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16, ArraySubType = UnmanagedType.U8)]
        public ulong[] opaque;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct CuGraphExec
    {
        public nint Pointer;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct CuIpcMemHandle
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64, ArraySubType = UnmanagedType.I1)]
        public byte[] reserved;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct CuExecAffinityParam
    {
        readonly CuExecAffinityType type;
        readonly CuExecAffinityParamUnion param;
    }
    [StructLayout(LayoutKind.Explicit)]
    internal struct CuExecAffinityParamUnion
    {
        [FieldOffset(0)] public CuExecAffinitySmCount smCount;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct CuExecAffinitySmCount
    {
        public uint val;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct CulinkState
    {
        public nint Pointer;
    }
    internal enum CuJitInputType
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
    internal enum CuMemHostAllocFlags
    {
        None = 0,
        Portable = 1,
        DeviceMap = 2,
        WriteCombined = 4
    }
    internal enum CuMemAdvise
    {
        SetReadMostly = 1,
        UnsetReadMostly = 2,
        SetPreferredLocation = 3,
        UnsetPreferredLocation = 4,
        SetAccessedBy = 5,
        UnsetAccessedBy = 6
    }
    internal enum CuMemRangeAttribute
    {
        ReadMostly = 1,
        PreferredLocation = 2,
        AccessedBy = 3,
        LastPrefetchLocation = 4
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct CuMemAllocationProp
    {
        public CuMemAllocationType type;
        public CuMemAllocationHandleType requestedHandleTypes;
        public CuMemLocation location;
        public nint win32HandleMetaData;
        public AllocFlags allocFlags;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct AllocFlags
    {
        public CuMemAllocationCompType compressionType;
        public byte gpuDirectRDMACapable;
        public CuMemCreateUsage usage;

        readonly byte reserved0;
        readonly byte reserved1;
        readonly byte reserved2;
        readonly byte reserved3;
    }
    internal enum CuMemAllocationCompType : byte
    {
        None = 0x0,
        Generic = 0x1
    }
    internal enum CuMemCreateUsage : ushort
    {
        None = 0x0,
        TilePool = 0x1
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct CuMemGenericAllocationHandle
    {
        public ulong Pointer;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct CuArrayMapInfo
    {
        [StructLayout(LayoutKind.Explicit)]
        internal struct Resource
        {
            [FieldOffset(0)] public CuMipMappedArray mipmap;
            [FieldOffset(0)] public CuArray array;
        }
        [StructLayout(LayoutKind.Explicit)]
        internal struct SubResource
        {
            internal struct SparseLevel
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
            internal struct MipTail
            {
                public uint Layer;
                public ulong Offset;
                public ulong Size;
            }
            [FieldOffset(0)] public SparseLevel sparseLevel;
            [FieldOffset(0)] public MipTail mipTail;
        }
        public CuResourceType resourceType;
        public Resource resource;
        public CuArraySparseSubResourceType subResourceType;
        public SubResource subResource;
        public CuMemOperationType memOperationType;
        public CuMemHandleType memHandleType;
        public CuMemGenericAllocationHandle memHandle;
        public ulong offset;
        public uint deviceBitMask;
        public uint flags;
        public uint reserved0;
        public uint reserved1;
    }
    internal enum CuResourceType
    {
        Array = 0x00,
        MipMappedArray = 0x01,
        Linear = 0x02,
        Pitch2D = 0x03
    }
    internal enum CuMemOperationType
    {
        Map = 1,
        Unmap = 2
    }
    internal enum CuMemHandleType
    {
        Generic = 0
    }
    internal enum CuArraySparseSubResourceType
    {
        SparseLevel = 0,
        MipTail = 1
    }
    internal enum CuTensorMapDataType
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
    internal struct CudaMemCpy3DPeer
    {
        public SizeT srcXInBytes;
        public SizeT srcY;
        public SizeT srcZ;
        public SizeT srcLOD;
        public CuMemoryType srcMemoryType;
        public nint srcHost;
        public CuDevicePtr srcDevice;
        public CuArray srcArray;
        public CuContext srcContext;
        public SizeT srcPitch;
        public SizeT srcHeight;
        public SizeT dstXInBytes;
        public SizeT dstY;
        public SizeT dstZ;
        public SizeT dstLOD;
        public CuMemoryType dstMemoryType;
        public nint dstHost;
        public CuDevicePtr dstDevice;
        public CuArray dstArray;
        public CuContext dstContext;
        public SizeT dstPitch;
        public SizeT dstHeight;
        public SizeT WidthInBytes;
        public SizeT Height;
        public SizeT Depth;
    }
    internal enum CuMemRangeHandleType
    {
        DmaBufFd = 0x1,
        Max = 0x7FFFFFFF,
    }
    internal enum CuCtxAttachFlags
    {
        None = 0
    }
    internal enum CuMemAttachFlags
    {
        Global = 1,
        Host = 2,
        Single = 4
    }
    [Flags]
    internal enum CuMemAllocationGranularityFlags
    {
        Minimum = 0x0,
        Recommended = 0x1
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct CudaArrayDescriptor
    {
        public SizeT Width;
        public SizeT Height;
        public CuArrayFormat Format;
        public uint NumChannels;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct CudaArraySparseProperties
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct TileExtent
        {
            public uint width;
            public uint height;
            public uint depth;
        }
        public TileExtent tileExtent;
        public uint mipTailFirstLevel;
        public ulong mipTailSize;
        public CuArraySparsePropertiesFlags flags;

        readonly uint reserved0;
        readonly uint reserved1;
        readonly uint reserved2;
        readonly uint reserved3;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct CudaArray3DDescriptor
    {
        public SizeT Width;
        public SizeT Height;
        public SizeT Depth;
        public CuArrayFormat Format;
        public uint NumChannels;
        public CudaArray3DFlags Flags;
    }
    internal enum CuStreamCaptureMode
    {
        Global = 0,
        Local = 1,
        Relaxed = 2
    }
    internal enum CuDeviceP2PAttribute
    {
        PerformanceRank = 0x01,
        AccessSupported = 0x02,
        NativeAtomicSupported = 0x03,
        [Obsolete("use CudaArrayAccessAccessSupported instead")]
        AccessAccessSupported = 0x04,
        CudaArrayAccessAccessSupported = 0x04
    }
    internal enum CuOutputMode
    {
        KeyValuePair = 0x00,
        Csv = 0x01
    }
    [Flags]
    internal enum CuArraySparsePropertiesFlags : uint
    {
        None = 0,
        SingleMipTail = 0x1
    }
    [Flags]
    internal enum CudaArray3DFlags
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
    internal enum CuTensorMapInterleave
    {
        None = 0,
        Interleave16B,
        Interleave32B
    }
    internal enum CuTensorMapSwizzle
    {
        None = 0,
        Swizzle32B,
        Swizzle64B,
        Swizzle128B
    }
    internal enum CuTensorMapL2Promotion
    {
        None = 0,
        L264B,
        L2128B,
        L2256B
    }
    
    internal enum CuTensorMapFloatOoBfill
    {
        None = 0,
        NanRequestZeroFma
    }
    internal enum CuResourceViewFormat
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
    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
    internal struct HandleUnion
    {
        [FieldOffset(0)] public int fd;
        [FieldOffset(0)] public Win32Handle win32;
        [FieldOffset(0)] public nint nvSciBufObject;
    }
    [Flags]
    internal enum CudaExternalMemory
    {
        Nothing = 0x0,
        Dedicated = 0x01,
    }
    internal enum CudaMipMappedArrayNumChannels
    {
        One = 1,
        Two = 2,
        Four = 4
    }
    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
    internal struct Win32Handle
    {
        [FieldOffset(0)] public nint handle;
        [FieldOffset(8)] [MarshalAs(UnmanagedType.LPStr)]
        public string name;
    }
    internal enum CuExternalSemaphoreHandleType
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
    internal enum CuGraphInstantiateFlags : ulong
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
        public CuStream hUploadStream;
        public CuGraphNode hErrNode_out;
        public CuGraphInstantiateResult result_out;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct CuGraphExecUpdateResultInfo
    {
        public CuGraphExecUpdateResult result;
        public CuGraphNode errorNode;
        public CuGraphNode errorFromNode;
    }
    internal enum CuGraphExecUpdateResult
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
    internal enum CuGraphInstantiateResult
    {
        Success = 0,
        Error = 1,
        InvalidStructure = 2,
        NodeOperationNotSupported = 3,
        MultipleCtxsNotSupported = 4
    }
    [Flags]
    internal enum CuGraphDebugDotFlags
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
    internal enum CuUserObjectFlags
    {
        None = 0,
        NoDestructorSync = 1
    }
    [Flags]
    internal enum CuUserObjectRetainFlags
    {
        None = 0,
        Move = 1
    }
    internal enum CuSolverStatus
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
        CuSolverStatusIrsParamsNotInitialized = 12,
        CuSolverStatusIrsParamsInvalid = 13,
        CuSolverStatusIrsParamsInvalidPrec = 14,
        CuSolverStatusIrsParamsInvalidRefine = 15,
        CuSolverStatusIrsParamsInvalidMaxiter = 16,
        CuSolverStatusIrsInternalError = 20,
        CuSolverStatusIrsNotSupported = 21,
        CuSolverStatusIrsOutOfRange = 22,
        CuSolverStatusIrsNrhsNotSupportedForRefineGmres = 23,
        CuSolverStatusIrsInfosNotInitialized = 25,
        CuSolverStatusIrsInfosNotDestroyed = 26,
        CuSolverStatusIrsMatrixSingular = 30,
        CuSolverStatusInvalidWorkspace = 31
    }
    internal enum CuSolverEigType
    {
        Type1 = 1,
        Type2 = 2,
        Type3 = 3
    }
    internal enum CuSolverEigMode
    {
        NoVector = 0,
        Vector = 1
    }
    internal enum CuSolverEigRange
    {
        All = 1001,
        I = 1002,
        V = 1003,
    }
    internal enum CuSolverIrsRefinement
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

    internal enum CuSolverPrecType
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

    internal enum CuSolverAlgMode
    {
        CusolverAlg0 = 0,
        CusolverAlg1 = 1,
        CusolverAlg2 = 2
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CuSolverDnHandle
    {
        public nint Pointer;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct SyevjInfo
    {
        public nint Pointer;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct GesvdjInfo
    {
        public nint Pointer;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct CuSolverDnIrsParams
    {
        public nint Pointer;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct CuSolverDnIrsInfos
    {
        public nint Pointer;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct CuSolverDnParams
    {
        public nint Pointer;
    }
    internal enum CuSolverDnFunction
    {
        CuSolverdnGetrf = 0,
        CuSolverdnPotrf = 1
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct CuSolverSpHandle
    {
        public nint Pointer;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct CuSolverRfHandle
    {
        public nint Pointer;
    }
    internal enum ResetValuesFastMode
    {
        Off = 0,
        On = 1
    }
    internal enum MatrixFormat
    {
        Csr = 0,
        Csc = 1
    }
    internal enum UnitDiagonal
    {
        StoredL = 0,
        StoredU = 1,
        AssumedL = 2,
        AssumedU = 3
    }
    internal enum Factorization
    {
        Alg0 = 0,
        Alg1 = 1,
        Alg2 = 2,
    }
    internal enum TriangularSolve
    {
        Alg1 = 1,
        Alg2 = 2,
        Alg3 = 3
    }
    internal enum NumericBoostReport
    {
        NotUsed = 0,
        Used = 1
    }
}