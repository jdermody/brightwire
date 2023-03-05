using System;

namespace BrightData.Cuda.CudaToolkit.Types
{
    internal class CudaContext : IDisposable
    {
        readonly CuDevice _device;
        readonly bool _isContextOwner;
        CuContext _context;
        bool _isDisposed;

        public CudaContext() : this(0, CuCtxFlags.SchedAuto, true)
        {
        }

        public CudaContext(int deviceId, CuCtxFlags flags, bool createNew)
        {
            var deviceCount = 0;
            var res = DriverApiNativeMethods.DeviceManagement.cuDeviceGetCount(ref deviceCount);

            if (res == CuResult.ErrorNotInitialized)
            {
                res = DriverApiNativeMethods.cuInit(CuInitializationFlags.None);
                if (res != CuResult.Success)
                    throw new CudaException(res);

                res = DriverApiNativeMethods.DeviceManagement.cuDeviceGetCount(ref deviceCount);
                if (res != CuResult.Success)
                    throw new CudaException(res);
            }
            else if (res != CuResult.Success)
                throw new CudaException(res);

            if (deviceCount == 0)
            {
                throw new CudaException(CuResult.ErrorNoDevice, "Cuda initialization error: There is no device supporting CUDA", null);
            }

            DeviceId = deviceId;
            res = DriverApiNativeMethods.DeviceManagement.cuDeviceGet(ref _device, deviceId);

            if (res != CuResult.Success)
                throw new CudaException(res);

            if (!createNew)
            {
                res = DriverApiNativeMethods.ContextManagement.cuCtxGetCurrent(ref _context);
                if (res != CuResult.Success)
                    throw new CudaException(res);

                if (_context.Pointer == IntPtr.Zero)
                {
                    createNew = true;
                }
                else
                {
                    var deviceCheck = new CuDevice();
                    res = DriverApiNativeMethods.ContextManagement.cuCtxGetDevice(ref deviceCheck);

                    if (res != CuResult.Success)
                        throw new CudaException(res);

                    if (deviceCheck != _device)
                    {
                        createNew = true;
                    }
                    else
                    {
                        _isContextOwner = false;
                    }
                }
            }

            if (createNew)
            {
                res = DriverApiNativeMethods.ContextManagement.cuCtxCreate_v2(ref _context, flags, _device);
                if (res != CuResult.Success)
                    throw new CudaException(res);
                _isContextOwner = true;
            }
        }

        ~CudaContext()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool fDisposing)
        {
            if (fDisposing && !_isDisposed)
            {
                if (_isContextOwner)
                {
                    DriverApiNativeMethods.ContextManagement.cuCtxDestroy_v2(_context);
                }

                _isDisposed = true;
            }
        }

        public void Synchronize()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(ToString());
            var res = DriverApiNativeMethods.ContextManagement.cuCtxSynchronize();
            if (res != CuResult.Success)
                throw new CudaException(res);
        }

        public void PushContext()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(ToString());
            var res = DriverApiNativeMethods.ContextManagement.cuCtxPushCurrent_v2(_context);
            if (res != CuResult.Success)
                throw new CudaException(res);
        }

        public void PopContext()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(ToString());
            var res = DriverApiNativeMethods.ContextManagement.cuCtxPopCurrent_v2(ref _context);
            if (res != CuResult.Success)
                throw new CudaException(res);
        }

        public void SetCurrent()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(ToString());
            var res = DriverApiNativeMethods.ContextManagement.cuCtxSetCurrent(_context);
            if (res != CuResult.Success)
                throw new CudaException(res);
        }

        public void SetSharedMemConfig(CuSharedConfig config)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(ToString());
            var res = DriverApiNativeMethods.ContextManagement.cuCtxSetSharedMemConfig(config);
            if (res != CuResult.Success)
                throw new CudaException(res);
        }

        public CuSharedConfig GetSharedMemConfig()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(ToString());
            var config = new CuSharedConfig();
            var res = DriverApiNativeMethods.ContextManagement.cuCtxGetSharedMemConfig(ref config);
            if (res != CuResult.Success)
                throw new CudaException(res);
            return config;
        }

        public CuModule LoadModule(string modulePath)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(ToString());
            var hcuModule = new CuModule();
            var res = DriverApiNativeMethods.ModuleManagement.cuModuleLoad(ref hcuModule, modulePath);
            if (res != CuResult.Success)
                throw new CudaException(res);
            return hcuModule;
        }

        public SizeT GetTotalDeviceMemorySize()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(ToString());
            SizeT size = 0, free = 0;
            var res = DriverApiNativeMethods.MemoryManagement.cuMemGetInfo_v2(ref free, ref size);
            if (res != CuResult.Success)
                throw new CudaException(res);
            return size;
        }

        public SizeT GetFreeDeviceMemorySize()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(ToString());
            SizeT size = 0, free = 0;
            var res = DriverApiNativeMethods.MemoryManagement.cuMemGetInfo_v2(ref free, ref size);

            if (res != CuResult.Success)
                throw new CudaException(res);
            return free;
        }

        public bool DeviceCanAccessPeer(CudaContext peerContext)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(ToString());
            var canAccessPeer = 0;
            var res = DriverApiNativeMethods.CudaPeerAccess.cuDeviceCanAccessPeer(ref canAccessPeer, _device, peerContext.Device);
            if (res != CuResult.Success)
                throw new CudaException(res);
            return canAccessPeer != 0;
        }

        public CuFuncCache GetCacheConfig()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(ToString());
            var cache = new CuFuncCache();
            var res = DriverApiNativeMethods.ContextManagement.cuCtxGetCacheConfig(ref cache);
            if (res != CuResult.Success)
                throw new CudaException(res);
            return cache;
        }

        public void SetCacheConfig(CuFuncCache cacheConfig)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(ToString());
            var res = DriverApiNativeMethods.ContextManagement.cuCtxSetCacheConfig(cacheConfig);
            if (res != CuResult.Success)
                throw new CudaException(res);
        }

        public Version GetDeviceComputeCapability()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(ToString());
            int major = 0, minor = 0;
            var res = DriverApiNativeMethods.DeviceManagement.cuDeviceGetAttribute(ref minor, CuDeviceAttribute.ComputeCapabilityMinor, _device);
            if (res != CuResult.Success)
                throw new CudaException(res);

            res = DriverApiNativeMethods.DeviceManagement.cuDeviceGetAttribute(ref major, CuDeviceAttribute.ComputeCapabilityMajor, _device);
            if (res != CuResult.Success)
                throw new CudaException(res);
            return new Version(major, minor);
        }

        public void GetStreamPriorityRange(ref int leastPriority, ref int greatestPriority)
        {
            var res = DriverApiNativeMethods.ContextManagement.cuCtxGetStreamPriorityRange(ref leastPriority, ref greatestPriority);
            if (res != CuResult.Success)
                throw new CudaException(res);
        }

        public SizeT GetLimit(CuLimit limit)
        {
            var ret = new SizeT();
            var res = DriverApiNativeMethods.Limits.cuCtxGetLimit(ref ret, limit);
            if (res != CuResult.Success)
                throw new CudaException(res);
            return ret;
        }

        public void SetLimit(CuLimit limit, SizeT value)
        {
            var res = DriverApiNativeMethods.Limits.cuCtxSetLimit(limit, value);
            if (res != CuResult.Success)
                throw new CudaException(res);
        }

        public static Version GetDriverVersion()
        {
            var driverVersion = 0;
            var res = DriverApiNativeMethods.cuDriverGetVersion(ref driverVersion);

            if (res == CuResult.ErrorNotInitialized)
            {
                res = DriverApiNativeMethods.cuInit(CuInitializationFlags.None);
                if (res != CuResult.Success)
                    throw new CudaException(res);

                res = DriverApiNativeMethods.cuDriverGetVersion(ref driverVersion);
                if (res != CuResult.Success)
                    throw new CudaException(res);
            }
            else if (res != CuResult.Success)
                throw new CudaException(res);

            return new Version(driverVersion / 1000, driverVersion % 100);
        }

        public static int GetDeviceCount()
        {
            var deviceCount = 0;
            var res = DriverApiNativeMethods.DeviceManagement.cuDeviceGetCount(ref deviceCount);
            if (res == CuResult.ErrorNotInitialized)
            {
                res = DriverApiNativeMethods.cuInit(CuInitializationFlags.None);
                if (res != CuResult.Success)
                    throw new CudaException(res);

                res = DriverApiNativeMethods.DeviceManagement.cuDeviceGetCount(ref deviceCount);
                if (res != CuResult.Success)
                    throw new CudaException(res);
            }
            else if (res != CuResult.Success)
                throw new CudaException(res);

            return deviceCount;
        }

        public static void EnablePeerAccess(CudaContext peerContext)
        {
            var res = DriverApiNativeMethods.CudaPeerAccess.cuCtxEnablePeerAccess(peerContext.Context, 0);
            if (res != CuResult.Success)
                throw new CudaException(res);
        }

        public static void DisablePeerAccess(CudaContext peerContext)
        {
            var res = DriverApiNativeMethods.CudaPeerAccess.cuCtxDisablePeerAccess(peerContext.Context);
            if (res != CuResult.Success)
                throw new CudaException(res);
        }

        public static CuDevice GetCUdevice(int deviceId)
        {
            var deviceCount = GetDeviceCount();
            if (deviceCount == 0)
            {
                throw new CudaException(CuResult.ErrorNoDevice, "Cuda initialization error: There is no device supporting CUDA", null);
            }

            if (deviceId < 0 || deviceId > deviceCount - 1)
                throw new ArgumentOutOfRangeException(nameof(deviceId), deviceId, "The device ID is not in the range [0.." + (deviceCount - 1).ToString() + "]");

            var device = new CuDevice();
            var res = DriverApiNativeMethods.DeviceManagement.cuDeviceGet(ref device, deviceId);
            if (res != CuResult.Success)
                throw new CudaException(res);

            return device;
        }

        public static void ProfilerInitialize(string configFile, string outputFile, CuOutputMode outputMode)
        {
            var res = DriverApiNativeMethods.Profiling.cuProfilerInitialize(configFile, outputFile, outputMode);
            if (res != CuResult.Success)
                throw new CudaException(res);
        }

        public static void ProfilerStart()
        {
            var res = DriverApiNativeMethods.Profiling.cuProfilerStart();
            if (res != CuResult.Success)
                throw new CudaException(res);
        }

        public static void ProfilerStop()
        {
            var res = DriverApiNativeMethods.Profiling.cuProfilerStop();
            if (res != CuResult.Success)
                throw new CudaException(res);
        }

        public static void CtxResetPersistingL2Cache()
        {
            var res = DriverApiNativeMethods.ContextManagement.cuCtxResetPersistingL2Cache();
            if (res != CuResult.Success)
                throw new CudaException(res);
        }

        public CuContext Context => _context;
        public CuDevice Device => _device;
        public int DeviceId { get; }

        public CuCtxFlags Flags
        {
            get
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(ToString());
                var flags = new CuCtxFlags();
                var res = DriverApiNativeMethods.ContextManagement.cuCtxGetFlags(ref flags);

                if (res != CuResult.Success) throw new CudaException(res);
                return flags;
            }
        }

        public ulong Id
        {
            get
            {
                if (_isDisposed) throw new ObjectDisposedException(ToString());
                ulong id = 0;
                var res = DriverApiNativeMethods.ContextManagement.cuCtxGetId(_context, ref id);

                if (res != CuResult.Success) throw new CudaException(res);
                return id;
            }
        }
    }
}