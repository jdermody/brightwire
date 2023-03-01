using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace BrightData.Cuda.CudaToolkit
{
    /// <summary>
    /// An abstraction layer for the CUDA driver API
    /// </summary>
    public class CudaContext : IDisposable
    {
        /// <summary/>
        protected CUcontext _context;
        /// <summary/>
        protected CUdevice _device;
        /// <summary/>
        protected int _deviceID;
        /// <summary/>
        protected bool disposed;
        readonly bool _contextOwner; //Indicates if this CudaContext instance created the wrapped cuda context and should be destroyed while disposing.

        /// <summary>
        /// Create a new instace of managed Cuda. Creates a new cuda context.
        /// Using device with ID 0 and <see cref="CUCtxFlags.SchedAuto"/>
        /// </summary>
        public CudaContext()
            : this(0, CUCtxFlags.SchedAuto, true)
        {

        }

        /// <summary>
        /// Create a new instace of managed Cuda
        /// </summary>
        /// <param name="deviceId">DeviceID.</param>
        /// <param name="flags">Context creation flags.</param>
        /// <param name="createNew">Create a new CUDA context or use an exiting context for the calling thread. Creates a new context if no context exists.</param>
        public CudaContext(int deviceId, CUCtxFlags flags, bool createNew)
        {
            CUResult res;
            int deviceCount = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetCount(ref deviceCount);
            Debug.WriteLine($"{DateTime.Now:G}, cuDeviceGetCount: {res}");

            if (res == CUResult.ErrorNotInitialized)
            {
                res = DriverAPINativeMethods.cuInit(CUInitializationFlags.None);
                Debug.WriteLine($"{DateTime.Now:G}, cuInit: {res}");
                if (res != CUResult.Success)
                    throw new CudaException(res);

                res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetCount(ref deviceCount);
                Debug.WriteLine($"{DateTime.Now:G}, cuDeviceGetCount: {res}");
                if (res != CUResult.Success)
                    throw new CudaException(res);
            }
            else if (res != CUResult.Success)
                throw new CudaException(res);

            if (deviceCount == 0)
            {
                throw new CudaException(CUResult.ErrorNoDevice, "Cuda initialization error: There is no device supporting CUDA", null);
            }

            _deviceID = deviceId;

            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGet(ref _device, deviceId);
            Debug.WriteLine($"{DateTime.Now:G}, cuDeviceGet: {res}");
            if (res != CUResult.Success)
                throw new CudaException(res);

            if (!createNew) //bind to existing cuda context
            {
                res = DriverAPINativeMethods.ContextManagement.cuCtxGetCurrent(ref _context);
                Debug.WriteLine($"{DateTime.Now:G}, cuCtxGetCurrent: {res}");
                if (res != CUResult.Success)
                    throw new CudaException(res);

                if (_context.Pointer == IntPtr.Zero) //No cuda context exists to bind to
                {
                    createNew = true; //create new context
                }
                else
                {
                    CUdevice deviceCheck = new CUdevice();
                    res = DriverAPINativeMethods.ContextManagement.cuCtxGetDevice(ref deviceCheck);
                    Debug.WriteLine($"{DateTime.Now:G}, cuCtxGetDevice: {res}");
                    if (res != CUResult.Success)
                        throw new CudaException(res);

                    if (deviceCheck != _device) //the current context is bound to another device, we don't want this context
                    {
                        createNew = true; //create new context on that device
                    }
                    else
                    {
                        _contextOwner = false;
                    }
                }
            }

            if (createNew)
            {
                res = DriverAPINativeMethods.ContextManagement.cuCtxCreate_v2(ref _context, flags, _device);
                Debug.WriteLine($"{DateTime.Now:G}, cuCtxCreate: {res}");
                if (res != CUResult.Success)
                    throw new CudaException(res);
                _contextOwner = true;
            }
        }

        /// <summary>
        /// For dispose
        /// </summary>
        ~CudaContext()
        {
            Dispose(false);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// For IDisposable. <para/>
        /// Note: If this instance created the wrapped CUcontext, it will be destroyed and can't be accessed by other threads anymore. <para/>
        /// If this instance only was bound to an existing CUcontext, the wrapped CUcontext won't be destroyed.
        /// </summary>
        /// <param name="fDisposing"></param>
        protected virtual void Dispose(bool fDisposing)
        {
            if (fDisposing && !disposed)
            {
                if (_contextOwner)
                {
                    //Ignore if failing
                    CUResult res;
                    res = DriverAPINativeMethods.ContextManagement.cuCtxDestroy_v2(_context);
                    Debug.WriteLine($"{DateTime.Now:G}, cuCtxDestroy: {res}");
                }
                disposed = true;
            }
            if (!fDisposing && !disposed)
                Debug.WriteLine($"ManagedCUDA not-disposed warning: {this.GetType()}");
        }

        /// <summary>
        /// Make sure the kernel image arrays are zero terminated by appending a zero
        /// </summary>
        protected byte[] AddZeroToArray(byte[] image)
        {
            byte[] retArr = new byte[image.LongLength + 1];
            Array.Copy(image, retArr, image.LongLength);
            retArr[image.LongLength] = 0;
            return retArr;
        }

        /// <summary>
        /// Gets the context's API version
        /// </summary>
        /// <returns>Version</returns>
        public Version GetAPIVersionOfCurrentContext()
        {
            if (disposed) throw new ObjectDisposedException(this.ToString());
            uint version = 0;
            CUResult res;
            res = DriverAPINativeMethods.ContextManagement.cuCtxGetApiVersion(_context, ref version);
            Debug.WriteLine($"{DateTime.Now:G}, cuCtxGetApiVersion: {res}");
            if (res != CUResult.Success)
                throw new CudaException(res);
            return new Version((int)version / 1000, (int)version % 100);
        }

        /// <summary>
        /// Blocks until the device has completed all preceding requested tasks. Throws a <see cref="CudaException"/> if one of the
        /// preceding tasks failed. If the context was created with the <see cref="CUCtxFlags.SchedAuto"/> flag, the CPU thread will
        /// block until the GPU context has finished its work.
        /// </summary>
        public void Synchronize()
        {
            if (disposed) throw new ObjectDisposedException(this.ToString());
            CUResult res;
            res = DriverAPINativeMethods.ContextManagement.cuCtxSynchronize();
            Debug.WriteLine($"{DateTime.Now:G}, cuCtxSynchronize: {res}");
            if (res != CUResult.Success)
                throw new CudaException(res);
        }

        /// <summary>
        /// Push the CUDA context
        /// </summary>
        public void PushContext()
        {
            if (disposed) throw new ObjectDisposedException(this.ToString());
            CUResult res;
            res = DriverAPINativeMethods.ContextManagement.cuCtxPushCurrent_v2(_context);
            Debug.WriteLine($"{DateTime.Now:G}, cuCtxPushCurrent: {res}");
            if (res != CUResult.Success)
                throw new CudaException(res);
        }

        /// <summary>
        /// Pop the CUDA context
        /// </summary>
        public void PopContext()
        {
            if (disposed) throw new ObjectDisposedException(this.ToString());
            CUResult res;
            res = DriverAPINativeMethods.ContextManagement.cuCtxPopCurrent_v2(ref _context);
            Debug.WriteLine($"{DateTime.Now:G}, cuCtxPopCurrent: {res}");
            if (res != CUResult.Success)
                throw new CudaException(res);
        }

        /// <summary>
        /// Binds this CUDA context to the calling CPU thread
        /// </summary>
        public void SetCurrent()
        {
            if (disposed) throw new ObjectDisposedException(this.ToString());
            CUResult res;
            res = DriverAPINativeMethods.ContextManagement.cuCtxSetCurrent(_context);
            Debug.WriteLine($"{DateTime.Now:G}, cuCtxSetCurrent: {res}");
            if (res != CUResult.Success)
                throw new CudaException(res);
        }

        /// <summary>
        /// Sets the shared memory configuration for the current context.<para/>
        /// On devices with configurable shared memory banks, this function will set
        /// the context's shared memory bank size which is used for subsequent kernel 
        /// launches. <para/> 
        /// Changed the shared memory configuration between launches may insert a device
        /// side synchronization point between those launches.<para/>
        /// Changing the shared memory bank size will not increase shared memory usage
        /// or affect occupancy of kernels, but may have major effects on performance. 
        /// Larger bank sizes will allow for greater potential bandwidth to shared memory,
        /// but will change what kinds of accesses to shared memory will result in bank 
        /// conflicts.<para/>
        /// This function will do nothing on devices with fixed shared memory bank size.
        /// <para/>
        /// The supported bank configurations are:
        /// - <see cref="CUsharedconfig.DefaultBankSize"/>: set bank width to the default initial
        ///   setting (currently, four bytes).
        /// - <see cref="CUsharedconfig.FourByteBankSize"/>: set shared memory bank width to
        ///   be natively four bytes.
        /// - <see cref="CUsharedconfig.EightByteBankSize"/>: set shared memory bank width to
        ///   be natively eight bytes.
        /// </summary>
        public void SetSharedMemConfig(CUsharedconfig config)
        {
            if (disposed) throw new ObjectDisposedException(this.ToString());
            CUResult res;
            res = DriverAPINativeMethods.ContextManagement.cuCtxSetSharedMemConfig(config);
            Debug.WriteLine($"{DateTime.Now:G}, cuCtxSetSharedMemConfig: {res}");
            if (res != CUResult.Success)
                throw new CudaException(res);
        }

        /// <summary>
        /// Returns the current shared memory configuration for the current context.
        /// </summary>
        public CUsharedconfig GetSharedMemConfig()
        {
            if (disposed) throw new ObjectDisposedException(this.ToString());
            CUResult res;
            CUsharedconfig config = new CUsharedconfig();
            res = DriverAPINativeMethods.ContextManagement.cuCtxGetSharedMemConfig(ref config);
            Debug.WriteLine($"{DateTime.Now:G}, cuCtxGetSharedMemConfig: {res}");
            if (res != CUResult.Success)
                throw new CudaException(res);
            return config;
        }

        /// <summary>
        /// Load a CUBIN-module from file
        /// </summary>
        /// <param name="modulePath"></param>
        /// <returns></returns>
        public CUmodule LoadModule(string modulePath)
        {
            if (disposed) throw new ObjectDisposedException(this.ToString());
            CUResult res;
            CUmodule hcuModule = new CUmodule();

            res = DriverAPINativeMethods.ModuleManagement.cuModuleLoad(ref hcuModule, modulePath);
            Debug.WriteLine($"{DateTime.Now:G}, cuModuleLoad: {res}");
            if (res != CUResult.Success)
                throw new CudaException(res);

            return hcuModule;
        }

        /// <summary>
        /// Load a PTX module from file
        /// </summary>
        /// <param name="modulePath"></param>
        /// <param name="options"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public CUmodule LoadModulePTX(string modulePath, CUJITOption[] options, object[] values)
        {
            if (disposed) throw new ObjectDisposedException(this.ToString());
            byte[] image;
            System.IO.FileInfo fi = new System.IO.FileInfo(modulePath);
            if (!fi.Exists) throw new System.IO.FileNotFoundException("Cannot read from module/kernel file.", modulePath);

            using (System.IO.BinaryReader br = new System.IO.BinaryReader(fi.OpenRead()))
            {
                image = br.ReadBytes((int)br.BaseStream.Length);
            }
            return LoadModulePTX(image, options, values);
        }

        /// <summary>
        /// Load a PTX module from file
        /// </summary>
        /// <param name="modulePath"></param>
        /// <returns></returns>
        public CUmodule LoadModulePTX(string modulePath)
        {
            return LoadModulePTX(modulePath, null, null);
        }

        /// <summary>
        /// Load a ptx module from image as byte[]
        /// </summary>
        /// <param name="moduleImage"></param>
        /// <param name="options"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public CUmodule LoadModulePTX(byte[] moduleImage, CUJITOption[] options, object[] values)
        {
            if (disposed) throw new ObjectDisposedException(this.ToString());
            bool noOptions = options == null || values == null;

            if (!noOptions)
                if (options.Length != values.Length)
                    throw new ArgumentException("options array and values array must have same size.");

            CUResult res;
            CUmodule hcuModule = new CUmodule();

            //Append zero to make image always zero terminated:
            moduleImage = AddZeroToArray(moduleImage);

            if (noOptions)
            {
                res = DriverAPINativeMethods.ModuleManagement.cuModuleLoadDataEx(ref hcuModule, moduleImage, 0, null, null);
                Debug.WriteLine($"{DateTime.Now:G}, cuModuleLoadDataEx: {res}");
            }
            else
            {

                IntPtr[] valuesIntPtr = new IntPtr[values.Length];
                GCHandle[] gcHandles = new GCHandle[values.Length];

                for (int i = 0; i < values.Length; i++)
                {
                    switch (options[i])
                    {
                        case CUJITOption.MaxRegisters:
                            valuesIntPtr[i] = (IntPtr)(Convert.ToUInt32(values[i], System.Globalization.CultureInfo.InvariantCulture));
                            break;
                        case CUJITOption.ThreadsPerBlock:
                            valuesIntPtr[i] = (IntPtr)(Convert.ToUInt32(values[i], System.Globalization.CultureInfo.InvariantCulture));
                            break;
                        case CUJITOption.WallTime:
                            valuesIntPtr[i] = new IntPtr();
                            break;
                        case CUJITOption.InfoLogBuffer:
                            gcHandles[i] = GCHandle.Alloc(values[i], GCHandleType.Pinned);
                            valuesIntPtr[i] = gcHandles[i].AddrOfPinnedObject();
                            break;
                        case CUJITOption.InfoLogBufferSizeBytes:
                            valuesIntPtr[i] = (IntPtr)(Convert.ToUInt32(values[i], System.Globalization.CultureInfo.InvariantCulture));
                            break;
                        case CUJITOption.ErrorLogBuffer:
                            gcHandles[i] = GCHandle.Alloc(values[i], GCHandleType.Pinned);
                            valuesIntPtr[i] = gcHandles[i].AddrOfPinnedObject();
                            break;
                        case CUJITOption.ErrorLogBufferSizeBytes:
                            valuesIntPtr[i] = (IntPtr)(Convert.ToUInt32(values[i], System.Globalization.CultureInfo.InvariantCulture));
                            break;
                        case CUJITOption.OptimizationLevel:
                            valuesIntPtr[i] = (IntPtr)(Convert.ToUInt32(values[i], System.Globalization.CultureInfo.InvariantCulture));
                            break;
                        case CUJITOption.TargetFromContext:
                            valuesIntPtr[i] = (IntPtr)(Convert.ToUInt32(values[i], System.Globalization.CultureInfo.InvariantCulture));
                            break;
                        case CUJITOption.Target:
                            valuesIntPtr[i] = (IntPtr)(Convert.ToUInt32(values[i], System.Globalization.CultureInfo.InvariantCulture));
                            break;
                        case CUJITOption.FallbackStrategy:
                            valuesIntPtr[i] = (IntPtr)(Convert.ToUInt32(values[i], System.Globalization.CultureInfo.InvariantCulture));
                            break;
                        case CUJITOption.GenerateDebugInfo:
                            valuesIntPtr[i] = (IntPtr)(Convert.ToUInt32(values[i], System.Globalization.CultureInfo.InvariantCulture));
                            break;
                        case CUJITOption.LogVerbose:
                            valuesIntPtr[i] = (IntPtr)(Convert.ToUInt32(values[i], System.Globalization.CultureInfo.InvariantCulture));
                            break;
                        case CUJITOption.GenerateLineInfo:
                            valuesIntPtr[i] = (IntPtr)(Convert.ToUInt32(values[i], System.Globalization.CultureInfo.InvariantCulture));
                            break;
                        case CUJITOption.JITCacheMode:
                            valuesIntPtr[i] = (IntPtr)(Convert.ToUInt32(values[i], System.Globalization.CultureInfo.InvariantCulture));
                            break;
                        case CUJITOption.FastCompile:
                            valuesIntPtr[i] = (IntPtr)(Convert.ToUInt32(values[i], System.Globalization.CultureInfo.InvariantCulture));
                            break;
                        case CUJITOption.GlobalSymbolNames:
                            valuesIntPtr[i] = (IntPtr)(Convert.ToInt64(values[i], System.Globalization.CultureInfo.InvariantCulture));
                            break;
                        case CUJITOption.GlobalSymbolAddresses:
                            valuesIntPtr[i] = (IntPtr)(Convert.ToInt64(values[i], System.Globalization.CultureInfo.InvariantCulture));
                            break;
                        case CUJITOption.GlobalSymbolCount:
                            valuesIntPtr[i] = (IntPtr)(Convert.ToUInt32(values[i], System.Globalization.CultureInfo.InvariantCulture));
                            break;
                        case CUJITOption.PositionIndependentCode:
                            valuesIntPtr[i] = (IntPtr)(Convert.ToUInt32(values[i], System.Globalization.CultureInfo.InvariantCulture));
                            break;
                        default:
                            valuesIntPtr[i] = (IntPtr)(Convert.ToInt64(values[i], System.Globalization.CultureInfo.InvariantCulture));
                            break;
                    }
                }

                res = DriverAPINativeMethods.ModuleManagement.cuModuleLoadDataEx(ref hcuModule, moduleImage, (uint)options.Length, options, valuesIntPtr);
                Debug.WriteLine($"{DateTime.Now:G}, cuModuleLoadDataEx: {res}");

                for (int i = 0; i < values.Length; i++)
                {

                    switch (options[i])
                    {
                        case CUJITOption.ErrorLogBuffer:
                            gcHandles[i].Free();
                            break;
                        case CUJITOption.InfoLogBuffer:
                            gcHandles[i].Free();
                            break;
                        case CUJITOption.ErrorLogBufferSizeBytes:
                            values[i] = (uint)valuesIntPtr[i];
                            break;
                        case CUJITOption.InfoLogBufferSizeBytes:
                            values[i] = (uint)valuesIntPtr[i];
                            break;
                        case CUJITOption.ThreadsPerBlock:
                            values[i] = (uint)valuesIntPtr[i];
                            break;
                        case CUJITOption.WallTime:
                            uint test = (uint)valuesIntPtr[i];
                            byte[] bytes = BitConverter.GetBytes(test);
                            values[i] = BitConverter.ToSingle(bytes, 0);
                            break;

                    }
                }
            }

            if (res != CUResult.Success)
                throw new CudaException(res);
            return hcuModule;
        }

        /// <summary>
        /// Load a ptx module from image as stream
        /// </summary>
        /// <param name="moduleImage"></param>
        /// <param name="options"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public CUmodule LoadModulePTX(System.IO.Stream moduleImage, CUJITOption[] options, object[] values)
        {
            if (moduleImage == null) throw new ArgumentNullException(nameof(moduleImage));
            byte[] kernel = new byte[moduleImage.Length];

            int bytesToRead = (int)moduleImage.Length;
            moduleImage.Position = 0;
            while (bytesToRead > 0)
            {
                bytesToRead -= moduleImage.Read(kernel, (int)moduleImage.Position, bytesToRead);
            }
            moduleImage.Position = 0;
            return LoadModulePTX(kernel, options, values);
        }

        /// <summary>
        /// Load a ptx module from image as byte[]
        /// </summary>
        /// <param name="moduleImage"></param>
        /// <returns></returns>
        public CUmodule LoadModulePTX(byte[] moduleImage)
        {
            return LoadModulePTX(moduleImage, null, null);
        }

        /// <summary>
        /// Load a ptx module from image as stream
        /// </summary>
        /// <param name="moduleImage"></param>
        /// <returns></returns>
        public CUmodule LoadModulePTX(System.IO.Stream moduleImage)
        {
            if (moduleImage == null) throw new ArgumentNullException(nameof(moduleImage));
            byte[] kernel = new byte[moduleImage.Length];

            int bytesToRead = (int)moduleImage.Length;
            moduleImage.Position = 0;
            while (bytesToRead > 0)
            {
                bytesToRead -= moduleImage.Read(kernel, (int)moduleImage.Position, bytesToRead);
            }
            moduleImage.Position = 0;
            return LoadModulePTX(kernel, null, null);
        }

        /// <summary>
        /// Load a FatBinary module from image as byte[]
        /// </summary>
        /// <param name="moduleImage"></param>
        /// <returns></returns>
        public CUmodule LoadModuleFatBin(byte[] moduleImage)
        {
            if (disposed) throw new ObjectDisposedException(this.ToString());
            CUResult res;
            CUmodule hcuModule = new CUmodule();

            //Append zero to make image always zero terminated:
            moduleImage = AddZeroToArray(moduleImage);

            res = DriverAPINativeMethods.ModuleManagement.cuModuleLoadFatBinary(ref hcuModule, moduleImage);
            Debug.WriteLine($"{DateTime.Now:G}, cuModuleLoadDataEx: {res}");

            if (res != CUResult.Success)
                throw new CudaException(res);
            return hcuModule;
        }

        /// <summary>
        /// Load a FatBinary module from image as stream
        /// </summary>
        /// <param name="moduleImage"></param>
        /// <returns></returns>
        public CUmodule LoadModuleFatBin(System.IO.Stream moduleImage)
        {

            if (moduleImage == null) throw new ArgumentNullException(nameof(moduleImage));
            byte[] kernel = new byte[moduleImage.Length];

            int bytesToRead = (int)moduleImage.Length;
            moduleImage.Position = 0;
            while (bytesToRead > 0)
            {
                bytesToRead -= moduleImage.Read(kernel, (int)moduleImage.Position, bytesToRead);
            }
            moduleImage.Position = 0;
            return LoadModuleFatBin(kernel);
        }

        /// <summary>
        /// unload module
        /// </summary>
        /// <param name="aModule"></param>
        public void UnloadModule(CUmodule aModule)
        {
            if (disposed) throw new ObjectDisposedException(this.ToString());
            CUResult res;
            res = DriverAPINativeMethods.ModuleManagement.cuModuleUnload(aModule);
            Debug.WriteLine($"{DateTime.Now:G}, cuModuleUnload: {res}");
            if (res != CUResult.Success)
                throw new CudaException(res);
        }

        /// <summary>
        /// Allocate memory on the device
        /// </summary>
        /// <param name="aSizeInBytes"></param>
        /// <returns></returns>
        public CUdeviceptr AllocateMemory(SizeT aSizeInBytes)
        {
            if (disposed) throw new ObjectDisposedException(this.ToString());
            CUResult res;
            CUdeviceptr dBuffer = new CUdeviceptr();

            res = DriverAPINativeMethods.MemoryManagement.cuMemAlloc_v2(ref dBuffer, aSizeInBytes);
            Debug.WriteLine($"{DateTime.Now:G}, cuMemAlloc: {res}");
            if (res != CUResult.Success)
                throw new CudaException(res);

            return dBuffer;
        }

        /// <summary>
        /// SetMemory (cuMemsetD8)
        /// </summary>
        /// <param name="aPtr"></param>
        /// <param name="aValue"></param>
        /// <param name="aSizeInBytes"></param>
        public void ClearMemory(CUdeviceptr aPtr, byte aValue, SizeT aSizeInBytes)
        {
            if (disposed) throw new ObjectDisposedException(this.ToString());
            CUResult res;
            res = DriverAPINativeMethods.Memset.cuMemsetD8_v2(aPtr, aValue, aSizeInBytes);
            Debug.WriteLine($"{DateTime.Now:G}, cuMemsetD8: {res}");
            if (res != CUResult.Success)
                throw new CudaException(res);
        }

        /// <summary>
        /// SetMemory (cuMemsetD16)
        /// </summary>
        /// <param name="aPtr"></param>
        /// <param name="aValue"></param>
        /// <param name="aSizeInBytes"></param>
        public void ClearMemory(CUdeviceptr aPtr, ushort aValue, SizeT aSizeInBytes)
        {
            if (disposed) throw new ObjectDisposedException(this.ToString());
            CUResult res;
            res = DriverAPINativeMethods.Memset.cuMemsetD16_v2(aPtr, aValue, aSizeInBytes / sizeof(ushort));
            Debug.WriteLine($"{DateTime.Now:G}, cuMemsetD16: {res}");
            if (res != CUResult.Success)
                throw new CudaException(res);
        }

        /// <summary>
        /// SetMemory (cuMemsetD32)
        /// </summary>
        /// <param name="aPtr"></param>
        /// <param name="aValue"></param>
        /// <param name="aSizeInBytes"></param>
        public void ClearMemory(CUdeviceptr aPtr, uint aValue, SizeT aSizeInBytes)
        {
            if (disposed) throw new ObjectDisposedException(this.ToString());
            CUResult res;
            res = DriverAPINativeMethods.Memset.cuMemsetD32_v2(aPtr, aValue, aSizeInBytes / sizeof(uint));
            Debug.WriteLine($"{DateTime.Now:G}, cuMemsetD32: {res}");
            if (res != CUResult.Success)
                throw new CudaException(res);
        }

        /// <summary>
        /// SetMemory (cuMemset2DD8)
        /// </summary>
        /// <param name="aPtr"></param>
        /// <param name="aValue"></param>
        /// <param name="aPitch"></param>
        /// <param name="aHeight"></param>
        /// <param name="aWidth"></param>
        public void ClearMemory(CUdeviceptr aPtr, byte aValue, SizeT aPitch, SizeT aWidth, SizeT aHeight)
        {
            if (disposed) throw new ObjectDisposedException(this.ToString());
            CUResult res;
            res = DriverAPINativeMethods.Memset.cuMemsetD2D8_v2(aPtr, aPitch, aValue, aWidth, aHeight);
            Debug.WriteLine($"{DateTime.Now:G}, cuMemsetD2D8: {res}");
            if (res != CUResult.Success)
                throw new CudaException(res);
        }

        /// <summary>
        /// SetMemory (cuMemset2DD16)
        /// </summary>
        /// <param name="aPtr"></param>
        /// <param name="aValue"></param>
        /// <param name="aPitch"></param>
        /// <param name="aHeight"></param>
        /// <param name="aWidth"></param>
        public void ClearMemory(CUdeviceptr aPtr, ushort aValue, SizeT aPitch, SizeT aWidth, SizeT aHeight)
        {
            if (disposed) throw new ObjectDisposedException(this.ToString());
            CUResult res;
            res = DriverAPINativeMethods.Memset.cuMemsetD2D16_v2(aPtr, aPitch, aValue, aWidth, aHeight);
            Debug.WriteLine($"{DateTime.Now:G}, cuMemsetD2D16: {res}");
            if (res != CUResult.Success)
                throw new CudaException(res);
        }

        /// <summary>
        /// SetMemory (cuMemset2DD32)
        /// </summary>
        /// <param name="aPtr"></param>
        /// <param name="aValue"></param>
        /// <param name="aPitch"></param>
        /// <param name="aHeight"></param>
        /// <param name="aWidth"></param>
        public void ClearMemory(CUdeviceptr aPtr, uint aValue, SizeT aPitch, SizeT aWidth, SizeT aHeight)
        {
            if (disposed) throw new ObjectDisposedException(this.ToString());
            CUResult res;
            res = DriverAPINativeMethods.Memset.cuMemsetD2D32_v2(aPtr, aPitch, aValue, aWidth, aHeight);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuMemsetD2D32"}: {res}");
            if (res != CUResult.Success)
                throw new CudaException(res);
        }

        /// <summary>
        /// SetMemory (cuMemsetD8)
        /// </summary>
        /// <param name="aPtr"></param>
        /// <param name="aValue"></param>
        /// <param name="aSizeInBytes"></param>
        /// <param name="stream"></param>
        public void ClearMemoryAsync(CUdeviceptr aPtr, byte aValue, SizeT aSizeInBytes, CUstream stream)
        {
            if (disposed) throw new ObjectDisposedException(this.ToString());
            CUResult res;
            res = DriverAPINativeMethods.MemsetAsync.cuMemsetD8Async(aPtr, aValue, aSizeInBytes, stream);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuMemsetD8Async"}: {res}");
            if (res != CUResult.Success)
                throw new CudaException(res);
        }

        /// <summary>
        /// SetMemory (cuMemsetD16)
        /// </summary>
        /// <param name="aPtr"></param>
        /// <param name="aValue"></param>
        /// <param name="aSizeInBytes"></param>
        /// <param name="stream"></param>
        public void ClearMemoryAsync(CUdeviceptr aPtr, ushort aValue, SizeT aSizeInBytes, CUstream stream)
        {
            if (disposed) throw new ObjectDisposedException(this.ToString());
            CUResult res;
            res = DriverAPINativeMethods.MemsetAsync.cuMemsetD16Async(aPtr, aValue, aSizeInBytes / sizeof(ushort), stream);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuMemsetD16Async"}: {res}");
            if (res != CUResult.Success)
                throw new CudaException(res);
        }

        /// <summary>
        /// SetMemory (cuMemsetD32)
        /// </summary>
        /// <param name="aPtr"></param>
        /// <param name="aValue"></param>
        /// <param name="aSizeInBytes"></param>
        /// <param name="stream"></param>
        public void ClearMemoryAsync(CUdeviceptr aPtr, uint aValue, SizeT aSizeInBytes, CUstream stream)
        {
            if (disposed) throw new ObjectDisposedException(this.ToString());
            CUResult res;
            res = DriverAPINativeMethods.MemsetAsync.cuMemsetD32Async(aPtr, aValue, aSizeInBytes / sizeof(uint), stream);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuMemsetD32Async"}: {res}");
            if (res != CUResult.Success)
                throw new CudaException(res);
        }

        /// <summary>
        /// SetMemory (cuMemset2DD8)
        /// </summary>
        /// <param name="aPtr"></param>
        /// <param name="aValue"></param>
        /// <param name="aPitch"></param>
        /// <param name="aHeight"></param>
        /// <param name="aWidth"></param>
        /// <param name="stream"></param>
        public void ClearMemoryAsync(CUdeviceptr aPtr, byte aValue, SizeT aPitch, SizeT aWidth, SizeT aHeight, CUstream stream)
        {
            if (disposed) throw new ObjectDisposedException(this.ToString());
            CUResult res;
            res = DriverAPINativeMethods.MemsetAsync.cuMemsetD2D8Async(aPtr, aPitch, aValue, aWidth, aHeight, stream);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuMemsetD2D8Async"}: {res}");
            if (res != CUResult.Success)
                throw new CudaException(res);
        }

        /// <summary>
        /// SetMemory (cuMemset2DD16)
        /// </summary>
        /// <param name="aPtr"></param>
        /// <param name="aValue"></param>
        /// <param name="aPitch"></param>
        /// <param name="aHeight"></param>
        /// <param name="aWidth"></param>
        /// <param name="stream"></param>
        public void ClearMemoryAsync(CUdeviceptr aPtr, ushort aValue, SizeT aPitch, SizeT aWidth, SizeT aHeight, CUstream stream)
        {
            if (disposed) throw new ObjectDisposedException(this.ToString());
            CUResult res;
            res = DriverAPINativeMethods.MemsetAsync.cuMemsetD2D16Async(aPtr, aPitch, aValue, aWidth, aHeight, stream);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuMemsetD2D16Async"}: {res}");
            if (res != CUResult.Success)
                throw new CudaException(res);
        }

        /// <summary>
        /// SetMemory (cuMemset2DD32)
        /// </summary>
        /// <param name="aPtr"></param>
        /// <param name="aValue"></param>
        /// <param name="aPitch"></param>
        /// <param name="aHeight"></param>
        /// <param name="aWidth"></param>
        /// <param name="stream"></param>
        public void ClearMemoryAsync(CUdeviceptr aPtr, uint aValue, SizeT aPitch, SizeT aWidth, SizeT aHeight, CUstream stream)
        {
            if (disposed) throw new ObjectDisposedException(this.ToString());
            CUResult res;
            res = DriverAPINativeMethods.MemsetAsync.cuMemsetD2D32Async(aPtr, aPitch, aValue, aWidth, aHeight, stream);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuMemsetD2D32Async"}: {res}");
            if (res != CUResult.Success)
                throw new CudaException(res);
        }

        /// <summary>
        /// Free device memory
        /// </summary>
        /// <param name="dBuffer"></param>
        public void FreeMemory(CUdeviceptr dBuffer)
        {
            if (disposed) throw new ObjectDisposedException(this.ToString());
            CUResult res;
            res = DriverAPINativeMethods.MemoryManagement.cuMemFree_v2(dBuffer);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuMemFree"}: {res}");
            if (res != CUResult.Success)
                throw new CudaException(res);
        }

        /// <summary>
        /// Returns the total device memory in bytes
        /// </summary>
        /// <returns></returns>
        public SizeT GetTotalDeviceMemorySize()
        {
            if (disposed) throw new ObjectDisposedException(this.ToString());
            CUResult res;
            SizeT size = 0, free = 0;
            res = DriverAPINativeMethods.MemoryManagement.cuMemGetInfo_v2(ref free, ref size);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuMemGetInfo"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            return size;
        }

        /// <summary>
        /// Returns the free available device memory in bytes
        /// </summary>
        /// <returns></returns>
        public SizeT GetFreeDeviceMemorySize()
        {
            if (disposed) throw new ObjectDisposedException(this.ToString());
            CUResult res;
            SizeT size = 0, free = 0;
            res = DriverAPINativeMethods.MemoryManagement.cuMemGetInfo_v2(ref free, ref size);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuMemGetInfo"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            return free;
        }

        /// <summary>
        /// Queries if a device may directly access a peer device's memory
        /// </summary>
        /// <returns></returns>
        public bool DeviceCanAccessPeer(CudaContext peerContext)
        {
            if (disposed) throw new ObjectDisposedException(this.ToString());
            CUResult res;
            int canAccessPeer = 0;
            res = DriverAPINativeMethods.CudaPeerAccess.cuDeviceCanAccessPeer(ref canAccessPeer, _device, peerContext.Device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceCanAccessPeer"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            return canAccessPeer != 0;
        }

        /// <summary>
        /// On devices where the L1 cache and shared memory use the same hardware
        /// resources, this returns the preferred cache configuration
        /// for the current context. This is only a preference. The driver will use
        /// the requested configuration if possible, but it is free to choose a different
        /// configuration if required to execute functions.<para/>
        /// This will return <see cref="CUFuncCache.PreferNone"/> on devices
        /// where the size of the L1 cache and shared memory are fixed.
        /// </summary>
        /// <returns></returns>
        public CUFuncCache GetCacheConfig()
        {
            if (disposed) throw new ObjectDisposedException(this.ToString());
            CUResult res;
            CUFuncCache cache = new CUFuncCache();
            res = DriverAPINativeMethods.ContextManagement.cuCtxGetCacheConfig(ref cache);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuCtxGetCacheConfig"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            return cache;
        }

        /// <summary>
        /// On devices where the L1 cache and shared memory use the same hardware
        /// resources, this sets through <c>cacheConfig</c> the preferred cache configuration for
        /// the current context. This is only a preference. The driver will use
        /// the requested configuration if possible, but it is free to choose a different
        /// configuration if required to execute the function. Any function preference
        /// set via <see cref="SetCacheConfig"/> will be preferred over this context-wide
        /// setting. Setting the context-wide cache configuration to
        /// <see cref="CUFuncCache.PreferNone"/> will cause subsequent kernel launches to prefer
        /// to not change the cache configuration unless required to launch the kernel.<para/>
        /// This setting does nothing on devices where the size of the L1 cache and
        /// shared memory are fixed.<para/>
        /// Launching a kernel with a different preference than the most recent
        /// preference setting may insert a device-side synchronization point.
        /// </summary>
        /// <param name="cacheConfig"></param>
        public void SetCacheConfig(CUFuncCache cacheConfig)
        {
            if (disposed) throw new ObjectDisposedException(this.ToString());
            CUResult res;
            res = DriverAPINativeMethods.ContextManagement.cuCtxSetCacheConfig(cacheConfig);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuCtxSetCacheConfig"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
        }

        /// <summary>
        /// Returns the device name of the device bound to the actual context
        /// </summary>
        /// <returns>Device Name</returns>
        public string GetDeviceName()
        {
            if (disposed) throw new ObjectDisposedException(this.ToString());
            byte[] devName = new byte[256];

            CUResult res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetName(devName, 256, _device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetName"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);

            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            return enc.GetString(devName).Replace("\0", "");
        }

        /// <summary>
        /// Returns the device's compute capability of the device bound to the actual context
        /// </summary>
        /// <returns>Device compute capability</returns>
        public Version GetDeviceComputeCapability()
        {
            if (disposed) throw new ObjectDisposedException(this.ToString());
            int major = 0, minor = 0;

            CUResult res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref minor, CUDeviceAttribute.ComputeCapabilityMinor, _device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success)
                throw new CudaException(res);

            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref major, CUDeviceAttribute.ComputeCapabilityMajor, _device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success)
                throw new CudaException(res);
            return new Version(major, minor);
        }

        /// <summary>
        /// Retrieve device properties
        /// </summary>
        /// <returns>DeviceProperties</returns>
        public CudaDeviceProperties GetDeviceInfo()
        {
            if (disposed) throw new ObjectDisposedException(this.ToString());
            return CudaContext.GetDeviceInfo(_device);
        }

        /// <summary>
        /// Returns numerical values that correspond to the least and greatest stream priorities.<para/>
        /// Returns in <c>leastPriority</c> and <c>greatestPriority</c> the numerical values that correspond
        /// to the least and greatest stream priorities respectively. Stream priorities
        /// follow a convention where lower numbers imply greater priorities. The range of
        /// meaningful stream priorities is given by [<c>greatestPriority</c>, <c>leastPriority</c>].
        /// If the user attempts to create a stream with a priority value that is
        /// outside the meaningful range as specified by this API, the priority is
        /// automatically clamped down or up to either <c>leastPriority</c> or <c>greatestPriority</c>
        /// respectively. See ::cuStreamCreateWithPriority for details on creating a
        /// priority stream.
        /// A NULL may be passed in for <c>leastPriority</c> or <c>greatestPriority</c> if the value
        /// is not desired.
        /// This function will return '0' in both <c>leastPriority</c> and <c>greatestPriority</c> if
        /// the current context's device does not support stream priorities
        /// (see ::cuDeviceGetAttribute).
        /// </summary>
        /// <param name="leastPriority">Pointer to an int in which the numerical value for least
        /// stream priority is returned</param>
        /// <param name="greatestPriority">Pointer to an int in which the numerical value for greatest stream priority is returned</param>
        public void GetStreamPriorityRange(ref int leastPriority, ref int greatestPriority)
        {
            CUResult res = DriverAPINativeMethods.ContextManagement.cuCtxGetStreamPriorityRange(ref leastPriority, ref greatestPriority);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuCtxGetStreamPriorityRange"}: {res}");
            if (res != CUResult.Success)
                throw new CudaException(res);
        }

        /// <summary>
        /// Returns the current size of limit. See <see cref="CULimit"/>
        /// </summary>
        /// <param name="limit">Limit to query</param>
        /// <returns>Returned size in bytes of limit</returns>
        public SizeT GetLimit(CULimit limit)
        {
            SizeT ret = new SizeT();
            CUResult res = DriverAPINativeMethods.Limits.cuCtxGetLimit(ref ret, limit);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuCtxGetLimit"}: {res}");
            if (res != CUResult.Success)
                throw new CudaException(res);
            return ret;
        }

        /// <summary>
        /// Setting <c>limit</c> to <c>value</c> is a request by the application to update the current limit maintained by the context. The
        /// driver is free to modify the requested value to meet h/w requirements (this could be clamping to minimum or maximum
        /// values, rounding up to nearest element size, etc). The application can use <see cref="GetLimit"/> to find out exactly what
        /// the limit has been set to.<para/>
        /// Setting each <see cref="CULimit"/> has its own specific restrictions, so each is discussed here:
        /// <list type="table">  
        /// <listheader><term>Value</term><description>Restriction</description></listheader>  
        /// <item><term><see cref="CULimit.StackSize"/></term><description>
        /// <see cref="CULimit.StackSize"/> controls the stack size of each GPU thread. This limit is only applicable to devices
        /// of compute capability 2.0 and higher. Attempting to set this limit on devices of compute capability less than 2.0
        /// will result in the error <see cref="CUResult.ErrorUnsupportedLimit"/> being returned.
        /// </description></item>  
        /// <item><term><see cref="CULimit.PrintfFIFOSize"/></term><description>
        /// <see cref="CULimit.PrintfFIFOSize"/> controls the size of the FIFO used by the <c>printf()</c> device system call. Setting
        /// <see cref="CULimit.PrintfFIFOSize"/> must be performed before loading any module that uses the printf() device
        /// system call, otherwise <see cref="CUResult.ErrorInvalidValue"/> will be returned. This limit is only applicable to
        /// devices of compute capability 2.0 and higher. Attempting to set this limit on devices of compute capability less
        /// than 2.0 will result in the error <see cref="CUResult.ErrorUnsupportedLimit"/> being returned.
        /// </description></item> 
        /// <item><term><see cref="CULimit.MallocHeapSize"/></term><description>
        /// <see cref="CULimit.MallocHeapSize"/> controls the size in bytes of the heap used by the ::malloc() and ::free() device system calls. Setting
        /// <see cref="CULimit.MallocHeapSize"/> must be performed before launching any kernel that uses the ::malloc() or ::free() device system calls, otherwise
        /// <see cref="CUResult.ErrorInvalidValue"/> will be returned. This limit is only applicable to
        /// devices of compute capability 2.0 and higher. Attempting to set this limit on devices of compute capability less
        /// than 2.0 will result in the error <see cref="CUResult.ErrorUnsupportedLimit"/> being returned.
        /// </description></item> 
        /// <item><term><see cref="CULimit.DevRuntimeSyncDepth"/></term><description>
        /// <see cref="CULimit.DevRuntimeSyncDepth"/> controls the maximum nesting depth of a grid at which a thread can safely call ::cudaDeviceSynchronize(). Setting
        /// this limit must be performed before any launch of a kernel that uses the
        /// device runtime and calls ::cudaDeviceSynchronize() above the default sync
        /// depth, two levels of grids. Calls to ::cudaDeviceSynchronize() will fail 
        /// with error code ::cudaErrorSyncDepthExceeded if the limitation is 
        /// violated. This limit can be set smaller than the default or up the maximum
        /// launch depth of 24. When setting this limit, keep in mind that additional
        /// levels of sync depth require the driver to reserve large amounts of device
        /// memory which can no longer be used for user allocations. If these 
        /// reservations of device memory fail, ::cuCtxSetLimit will return 
        /// <see cref="CUResult.ErrorOutOfMemory"/>, and the limit can be reset to a lower value.
        /// This limit is only applicable to devices of compute capability 3.5 and
        /// higher. Attempting to set this limit on devices of compute capability less
        /// than 3.5 will result in the error <see cref="CUResult.ErrorUnsupportedLimit"/> being 
        /// returned.
        /// </description></item> 
        /// <item><term><see cref="CULimit.DevRuntimePendingLaunchCount"/></term><description>
        /// <see cref="CULimit.DevRuntimePendingLaunchCount"/> controls the maximum number of 
        /// outstanding device runtime launches that can be made from the current
        /// context. A grid is outstanding from the point of launch up until the grid
        /// is known to have been completed. Device runtime launches which violate 
        /// this limitation fail and return ::cudaErrorLaunchPendingCountExceeded when
        /// ::cudaGetLastError() is called after launch. If more pending launches than
        /// the default (2048 launches) are needed for a module using the device
        /// runtime, this limit can be increased. Keep in mind that being able to
        /// sustain additional pending launches will require the driver to reserve
        /// larger amounts of device memory upfront which can no longer be used for
        /// allocations. If these reservations fail, ::cuCtxSetLimit will return
        /// <see cref="CUResult.ErrorOutOfMemory"/>, and the limit can be reset to a lower value.
        /// This limit is only applicable to devices of compute capability 3.5 and
        /// higher. Attempting to set this limit on devices of compute capability less
        /// than 3.5 will result in the error <see cref="CUResult.ErrorUnsupportedLimit"/> being
        /// returned. 
        /// </description></item> 
        /// </list>   
        /// </summary>
        /// <param name="limit">Limit to set</param>
        /// <param name="value">Size in bytes of limit</param>
        public void SetLimit(CULimit limit, SizeT value)
        {
            CUResult res = DriverAPINativeMethods.Limits.cuCtxSetLimit(limit, value);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuCtxSetLimit"}: {res}");
            if (res != CUResult.Success)
                throw new CudaException(res);
        }

        /// <summary>
        /// Returns the device name of the device with ID <c>deviceID</c>
        /// </summary>
        /// <param name="deviceID"></param>
        /// <returns>Device Name</returns>
        public static string GetDeviceName(int deviceID)
        {
            byte[] devName = new byte[256];

            CUResult res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetName(devName, 256, GetCUdevice(deviceID));
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetName"}: {res}");
            if (res == CUResult.ErrorNotInitialized)
            {
                res = DriverAPINativeMethods.cuInit(CUInitializationFlags.None);
                Debug.WriteLine($"{DateTime.Now:G}, {"cuInit"}: {res}");
                if (res != CUResult.Success)
                    throw new CudaException(res);

                res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetName(devName, 256, GetCUdevice(deviceID));
                Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetName"}: {res}");
                if (res != CUResult.Success)
                    throw new CudaException(res);
            }
            else
                if (res != CUResult.Success) throw new CudaException(res);

            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            return enc.GetString(devName).Replace("\0", "");
        }

        /// <summary>
        /// Returns the device's compute capability of the device with ID <c>deviceID</c>
        /// </summary>
        /// <param name="deviceID"></param>
        /// <returns>Device compute capability</returns>
        public static Version GetDeviceComputeCapability(int deviceID)
        {
            int major = 0, minor = 0;

            CUResult res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref minor, CUDeviceAttribute.ComputeCapabilityMinor, GetCUdevice(deviceID));
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");


            if (res == CUResult.ErrorNotInitialized)
            {
                res = DriverAPINativeMethods.cuInit(CUInitializationFlags.None);
                Debug.WriteLine($"{DateTime.Now:G}, {"cuInit"}: {res}");
                if (res != CUResult.Success)
                    throw new CudaException(res);

                res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref minor, CUDeviceAttribute.ComputeCapabilityMinor, GetCUdevice(deviceID));
                Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
                if (res != CUResult.Success)
                    throw new CudaException(res);

                res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref major, CUDeviceAttribute.ComputeCapabilityMajor, GetCUdevice(deviceID));
                Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
                if (res != CUResult.Success)
                    throw new CudaException(res);
            }
            else
            {
                if (res != CUResult.Success)
                    throw new CudaException(res);
                else
                {
                    res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref major, CUDeviceAttribute.ComputeCapabilityMajor, GetCUdevice(deviceID));
                    Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
                    if (res != CUResult.Success)
                        throw new CudaException(res);
                }
            }
            return new Version(major, minor);
        }

        /// <summary>
        /// Returns the version number of the installed cuda driver
        /// </summary>
        /// <returns>CUDA driver version</returns>
        public static Version GetDriverVersion()
        {
            int driverVersion = 0;

            CUResult res = DriverAPINativeMethods.cuDriverGetVersion(ref driverVersion);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDriverGetVersion"}: {res}");
            if (res == CUResult.ErrorNotInitialized)
            {
                res = DriverAPINativeMethods.cuInit(CUInitializationFlags.None);
                Debug.WriteLine($"{DateTime.Now:G}, {"cuInit"}: {res}");
                if (res != CUResult.Success)
                    throw new CudaException(res);

                res = DriverAPINativeMethods.cuDriverGetVersion(ref driverVersion);
                Debug.WriteLine($"{DateTime.Now:G}, {"cuDriverGetVersion"}: {res}");
                if (res != CUResult.Success)
                    throw new CudaException(res);
            }
            else if (res != CUResult.Success)
                throw new CudaException(res);

            return new Version(driverVersion / 1000, driverVersion % 100);
        }
        /// <summary>
        /// Retrieve device properties
        /// </summary>
        /// <param name="deviceId">Device ID</param>
        /// <returns>DeviceProperties</returns>
        public static CudaDeviceProperties GetDeviceInfo(int deviceId)
        {
            return GetDeviceInfo(GetCUdevice(deviceId));
        }

        /// <summary>
        /// Get the number of CUDA capable devices
        /// </summary>
        /// <returns></returns>
        public static int GetDeviceCount()
        {
            CUResult res;
            int deviceCount = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetCount(ref deviceCount);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetCount"}: {res}");
            if (res == CUResult.ErrorNotInitialized)
            {
                res = DriverAPINativeMethods.cuInit(CUInitializationFlags.None);
                Debug.WriteLine($"{DateTime.Now:G}, {"cuInit"}: {res}");
                if (res != CUResult.Success)
                    throw new CudaException(res);

                res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetCount(ref deviceCount);
                Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetCount"}: {res}");
                if (res != CUResult.Success)
                    throw new CudaException(res);
            }
            else if (res != CUResult.Success)
                throw new CudaException(res);

            return deviceCount;
        }

        /// <summary>
        /// If both the current context (current to the calling thread) and <c>peerContext</c> are on devices which support unified 
        /// addressing (as may be queried using GetDeviceInfo), then
        /// on success all allocations from <c>peerContext</c> will immediately be accessible
        /// by the current context.  See \ref CUDA_UNIFIED for additional
        /// details. <para/>
        /// Note that access granted by this call is unidirectional and that in order to access
        /// memory from the current context in <c>peerContext</c>, a separate symmetric call 
        /// to ::cuCtxEnablePeerAccess() is required. <para/>
        /// Returns <see cref="CUResult.ErrorInvalidDevice"/> if <see cref="DeviceCanAccessPeer"/> indicates
        /// that the CUdevice of the current context cannot directly access memory
        /// from the CUdevice of <c>peerContext</c>. <para/>
        /// Throws <see cref="CUResult.ErrorPeerAccessAlreadyEnabled"/> if direct access of
        /// <c>peerContext</c> from the current context has already been enabled. <para/>
        /// Throws <see cref="CUResult.ErrorInvalidContext"/> if there is no current context, <c>peerContext</c>
        /// is not a valid context, or if the current context is <c>peerContext</c>. <para/>
        /// </summary>
        /// <param name="peerContext">Peer context to enable direct access to from the current context</param>
        /// <returns></returns>
        public static void EnablePeerAccess(CudaContext peerContext)
        {
            CUResult res;
            res = DriverAPINativeMethods.CudaPeerAccess.cuCtxEnablePeerAccess(peerContext.Context, 0);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuCtxEnablePeerAccess"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
        }

        /// <summary>
        /// Disables direct access to memory allocations in a peer context and unregisters any registered allocations.
        /// </summary>
        /// <param name="peerContext">Peer context to disable direct access to</param>
        /// <returns></returns>
        public static void DisablePeerAccess(CudaContext peerContext)
        {
            CUResult res;
            res = DriverAPINativeMethods.CudaPeerAccess.cuCtxDisablePeerAccess(peerContext.Context);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuCtxEnablePeerAccess"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
        }

        /// <summary>
        /// Fills the CudaDeviceProperties structure
        /// </summary>
        protected static CudaDeviceProperties GetDeviceInfo(CUdevice device)
        {
            CUResult res;

            //Read out all properties
            CudaDeviceProperties props = new CudaDeviceProperties();
            byte[] devName = new byte[256];
            int major = 0, minor = 0;

            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetName(devName, 256, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetName"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);

            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            props.DeviceName = enc.GetString(devName).Replace("\0", "");

            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref minor, CUDeviceAttribute.ComputeCapabilityMinor, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.ComputeCapabilityMinor = minor;

            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref major, CUDeviceAttribute.ComputeCapabilityMajor, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.ComputeCapabilityMajor = major;

            int driverVersion = 0;
            res = DriverAPINativeMethods.cuDriverGetVersion(ref driverVersion);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDriverGetVersion"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.DriverVersion = new Version(driverVersion / 1000, driverVersion % 100);



            SizeT totalGlobalMem = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceTotalMem_v2(ref totalGlobalMem, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceTotalMem"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.TotalGlobalMemory = totalGlobalMem;


            int multiProcessorCount = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref multiProcessorCount, CUDeviceAttribute.MultiProcessorCount, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.MultiProcessorCount = multiProcessorCount;


            int totalConstantMemory = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref totalConstantMemory, CUDeviceAttribute.TotalConstantMemory, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.TotalConstantMemory = totalConstantMemory;

            int sharedMemPerBlock = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref sharedMemPerBlock, CUDeviceAttribute.MaxSharedMemoryPerBlock, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.SharedMemoryPerBlock = sharedMemPerBlock;

            int regsPerBlock = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref regsPerBlock, CUDeviceAttribute.MaxRegistersPerBlock, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.RegistersPerBlock = regsPerBlock;

            int warpSize = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref warpSize, CUDeviceAttribute.WarpSize, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.WarpSize = warpSize;

            int maxThreadsPerBlock = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref maxThreadsPerBlock, CUDeviceAttribute.MaxThreadsPerBlock, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.MaxThreadsPerBlock = maxThreadsPerBlock;

            int x = 0, y = 0, z = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref x, CUDeviceAttribute.MaxBlockDimX, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref y, CUDeviceAttribute.MaxBlockDimY, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref z, CUDeviceAttribute.MaxBlockDimZ, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.MaxBlockDim = new dim3((uint)x, (uint)y, (uint)z);

            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref x, CUDeviceAttribute.MaxGridDimX, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref y, CUDeviceAttribute.MaxGridDimY, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref z, CUDeviceAttribute.MaxGridDimZ, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.MaxGridDim = new dim3((uint)x, (uint)y, (uint)z);

            int memPitch = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref memPitch, CUDeviceAttribute.MaxPitch, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.MemoryPitch = memPitch;

            int textureAlign = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref textureAlign, CUDeviceAttribute.TextureAlignment, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.TextureAlign = textureAlign;

            int clockRate = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref clockRate, CUDeviceAttribute.ClockRate, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.ClockRate = clockRate;



            int gpuOverlap = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref gpuOverlap, CUDeviceAttribute.GPUOverlap, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.GpuOverlap = gpuOverlap > 0;


            int kernelExecTimeoutEnabled = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref kernelExecTimeoutEnabled, CUDeviceAttribute.KernelExecTimeout, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.KernelExecTimeoutEnabled = kernelExecTimeoutEnabled > 0;

            int integrated = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref integrated, CUDeviceAttribute.Integrated, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.Integrated = integrated > 0;

            int canMapHostMemory = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref canMapHostMemory, CUDeviceAttribute.CanMapHostMemory, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.CanMapHostMemory = canMapHostMemory > 0;

            int computeMode = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref computeMode, CUDeviceAttribute.ComputeMode, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.ComputeMode = (CUComputeMode)computeMode;

            int maxtexture1DWidth = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref maxtexture1DWidth, CUDeviceAttribute.MaximumTexture1DWidth, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.MaximumTexture1DWidth = maxtexture1DWidth;

            int maxtexture2DWidth = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref maxtexture2DWidth, CUDeviceAttribute.MaximumTexture2DWidth, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.MaximumTexture2DWidth = maxtexture2DWidth;

            int maxtexture2DHeight = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref maxtexture2DHeight, CUDeviceAttribute.MaximumTexture2DHeight, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.MaximumTexture2DHeight = maxtexture2DHeight;

            int maxtexture3DWidth = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref maxtexture3DWidth, CUDeviceAttribute.MaximumTexture3DWidth, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.MaximumTexture3DWidth = maxtexture2DWidth;

            int maxtexture3DHeight = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref maxtexture3DHeight, CUDeviceAttribute.MaximumTexture3DHeight, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.MaximumTexture3DHeight = maxtexture2DHeight;

            int maxtexture3DDepth = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref maxtexture3DDepth, CUDeviceAttribute.MaximumTexture3DDepth, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.MaximumTexture3DDepth = maxtexture2DHeight;

            int maxtexture2DArray_Width = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref maxtexture2DArray_Width, CUDeviceAttribute.MaximumTexture2DArray_Width, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.MaximumTexture2DArrayWidth = maxtexture2DArray_Width;

            int maxtexture2DArray_Height = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref maxtexture2DArray_Height, CUDeviceAttribute.MaximumTexture2DArray_Height, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.MaximumTexture2DArrayHeight = maxtexture2DArray_Height;

            int maxtexture2DArray_NumSlices = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref maxtexture2DArray_NumSlices, CUDeviceAttribute.MaximumTexture2DArray_NumSlices, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.MaximumTexture2DArrayNumSlices = maxtexture2DArray_NumSlices;

            int surfaceAllignment = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref surfaceAllignment, CUDeviceAttribute.SurfaceAllignment, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.SurfaceAllignment = surfaceAllignment;

            int concurrentKernels = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref concurrentKernels, CUDeviceAttribute.ConcurrentKernels, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.ConcurrentKernels = concurrentKernels > 0;

            int ECCEnabled = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref ECCEnabled, CUDeviceAttribute.ECCEnabled, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.EccEnabled = ECCEnabled > 0;

            int PCIBusID = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref PCIBusID, CUDeviceAttribute.PCIBusID, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.PciBusId = PCIBusID;

            int PCIDeviceID = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref PCIDeviceID, CUDeviceAttribute.PCIDeviceID, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.PciDeviceId = PCIDeviceID;

            int TCCDriver = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref TCCDriver, CUDeviceAttribute.TCCDriver, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.TccDrivelModel = TCCDriver > 0;

            int MemoryClockRate = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref MemoryClockRate, CUDeviceAttribute.MemoryClockRate, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.MemoryClockRate = MemoryClockRate;

            int GlobalMemoryBusWidth = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref GlobalMemoryBusWidth, CUDeviceAttribute.GlobalMemoryBusWidth, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.GlobalMemoryBusWidth = GlobalMemoryBusWidth;

            int L2CacheSize = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref L2CacheSize, CUDeviceAttribute.L2CacheSize, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.L2CacheSize = L2CacheSize;

            int MaxThreadsPerMultiProcessor = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref MaxThreadsPerMultiProcessor, CUDeviceAttribute.MaxThreadsPerMultiProcessor, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.MaxThreadsPerMultiProcessor = MaxThreadsPerMultiProcessor;

            int AsyncEngineCount = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref AsyncEngineCount, CUDeviceAttribute.AsyncEngineCount, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.AsyncEngineCount = AsyncEngineCount;

            int UnifiedAddressing = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref UnifiedAddressing, CUDeviceAttribute.UnifiedAddressing, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.UnifiedAddressing = UnifiedAddressing > 0;

            int MaximumTexture1DLayeredWidth = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref MaximumTexture1DLayeredWidth, CUDeviceAttribute.MaximumTexture1DLayeredWidth, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.MaximumTexture1DLayeredWidth = MaximumTexture1DLayeredWidth;

            int MaximumTexture1DLayeredLayers = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref MaximumTexture1DLayeredLayers, CUDeviceAttribute.MaximumTexture1DLayeredLayers, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.MaximumTexture1DLayeredLayers = MaximumTexture1DLayeredLayers;

            int PCIDomainID = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref PCIDomainID, CUDeviceAttribute.PCIDomainID, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.PCIDomainID = PCIDomainID;


            int texturePitchAlignment = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref texturePitchAlignment, CUDeviceAttribute.TexturePitchAlignment, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.TexturePitchAlignment = texturePitchAlignment;

            int maximumTextureCubeMapWidth = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref maximumTextureCubeMapWidth, CUDeviceAttribute.MaximumTextureCubeMapWidth, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.MaximumTextureCubeMapWidth = maximumTextureCubeMapWidth;

            int maximumTextureCubeMapLayeredWidth = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref maximumTextureCubeMapLayeredWidth, CUDeviceAttribute.MaximumTextureCubeMapLayeredWidth, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.MaximumTextureCubeMapLayeredWidth = maximumTextureCubeMapLayeredWidth;

            int maximumTextureCubeMapLayeredLayers = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref maximumTextureCubeMapLayeredLayers, CUDeviceAttribute.MaximumTextureCubeMapLayeredLayers, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.MaximumTextureCubeMapLayeredLayers = maximumTextureCubeMapLayeredLayers;

            int maximumSurface1DWidth = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref maximumSurface1DWidth, CUDeviceAttribute.MaximumSurface1DWidth, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.MaximumSurface1DWidth = maximumSurface1DWidth;

            int maximumSurface2DWidth = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref maximumSurface2DWidth, CUDeviceAttribute.MaximumSurface2DWidth, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.MaximumSurface2DWidth = maximumSurface2DWidth;

            int maximumSurface2DHeight = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref maximumSurface2DHeight, CUDeviceAttribute.MaximumSurface2DHeight, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.MaximumSurface2DHeight = maximumSurface2DHeight;

            int maximumSurface3DWidth = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref maximumSurface3DWidth, CUDeviceAttribute.MaximumSurface3DWidth, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.MaximumSurface3DWidth = maximumSurface3DWidth;

            int maximumSurface3DHeight = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref maximumSurface3DHeight, CUDeviceAttribute.MaximumSurface3DHeight, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.MaximumSurface3DHeight = maximumSurface3DHeight;

            int maximumSurface3DDepth = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref maximumSurface3DDepth, CUDeviceAttribute.MaximumSurface3DDepth, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.MaximumSurface3DDepth = maximumSurface3DDepth;

            int maximumSurface1DLayeredWidth = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref maximumSurface1DLayeredWidth, CUDeviceAttribute.MaximumSurface1DLayeredWidth, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.MaximumSurface1DLayeredWidth = maximumSurface1DLayeredWidth;

            int maximumSurface1DLayeredLayers = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref maximumSurface1DLayeredLayers, CUDeviceAttribute.MaximumSurface1DLayeredLayers, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.MaximumSurface1DLayeredLayers = maximumSurface1DLayeredLayers;

            int maximumSurface2DLayeredWidth = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref maximumSurface2DLayeredWidth, CUDeviceAttribute.MaximumSurface2DLayeredWidth, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.MaximumSurface2DLayeredWidth = maximumSurface2DLayeredWidth;

            int maximumSurface2DLayeredHeight = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref maximumSurface2DLayeredHeight, CUDeviceAttribute.MaximumSurface2DLayeredHeight, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.MaximumSurface2DLayeredHeight = maximumSurface2DLayeredHeight;

            int maximumSurface2DLayeredLayers = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref maximumSurface2DLayeredLayers, CUDeviceAttribute.MaximumSurface2DLayeredLayers, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.MaximumSurface2DLayeredLayers = maximumSurface2DLayeredLayers;

            int maximumSurfaceCubemapWidth = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref maximumSurfaceCubemapWidth, CUDeviceAttribute.MaximumSurfaceCubemapWidth, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.MaximumSurfaceCubemapWidth = maximumSurfaceCubemapWidth;

            int maximumSurfaceCubemapLayeredWidth = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref maximumSurfaceCubemapLayeredWidth, CUDeviceAttribute.MaximumSurfaceCubemapLayeredWidth, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.MaximumSurfaceCubemapLayeredWidth = maximumSurfaceCubemapLayeredWidth;

            int maximumSurfaceCubemapLayeredLayers = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref maximumSurfaceCubemapLayeredLayers, CUDeviceAttribute.MaximumSurfaceCubemapLayeredLayers, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.MaximumSurfaceCubemapLayeredLayers = maximumSurfaceCubemapLayeredLayers;

#pragma warning disable 0618 //warning that deprecated
            int maximumTexture1DLinearWidth = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref maximumTexture1DLinearWidth, CUDeviceAttribute.MaximumTexture1DLinearWidth, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.MaximumTexture1DLinearWidth = maximumTexture1DLinearWidth;
#pragma warning restore 0618


            int maximumTexture2DLinearWidth = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref maximumTexture2DLinearWidth, CUDeviceAttribute.MaximumTexture2DLinearWidth, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.MaximumTexture2DLinearWidth = maximumTexture2DLinearWidth;

            int maximumTexture2DLinearHeight = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref maximumTexture2DLinearHeight, CUDeviceAttribute.MaximumTexture2DLinearHeight, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.MaximumTexture2DLinearHeight = maximumTexture2DLinearHeight;

            int maximumTexture2DLinearPitch = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref maximumTexture2DLinearPitch, CUDeviceAttribute.MaximumTexture2DLinearPitch, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.MaximumTexture2DLinearPitch = maximumTexture2DLinearPitch;


            int maximumTexture2DMipmappedWidth = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref maximumTexture2DMipmappedWidth, CUDeviceAttribute.MaximumTexture2DMipmappedWidth, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.MaximumTexture2DMipmappedWidth = maximumTexture2DMipmappedWidth;

            int maximumTexture2DMipmappedHeight = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref maximumTexture2DMipmappedHeight, CUDeviceAttribute.MaximumTexture2DMipmappedHeight, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.MaximumTexture2DMipmappedHeight = maximumTexture2DMipmappedHeight;

            int maximumTexture1DMipmappedWidth = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref maximumTexture1DMipmappedWidth, CUDeviceAttribute.MaximumTexture1DMipmappedWidth, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.MaximumTexture1DMipmappedWidth = maximumTexture1DMipmappedWidth;

            int streamPriority = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref streamPriority, CUDeviceAttribute.StreamPrioritiesSupported, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.SupportsStreamPriorities = streamPriority > 0;

            int globalL1CacheSupported = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref globalL1CacheSupported, CUDeviceAttribute.GlobalL1CacheSupported, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.GlobalL1CacheSupported = globalL1CacheSupported > 0;

            int localL1CacheSupported = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref localL1CacheSupported, CUDeviceAttribute.LocalL1CacheSupported, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.LocalL1CacheSupported = localL1CacheSupported > 0;

            int maxSharedMemoryPerMultiprocessor = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref maxSharedMemoryPerMultiprocessor, CUDeviceAttribute.MaxSharedMemoryPerMultiprocessor, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.MaxSharedMemoryPerMultiprocessor = maxSharedMemoryPerMultiprocessor;

            int maxRegistersPerMultiprocessor = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref maxRegistersPerMultiprocessor, CUDeviceAttribute.MaxRegistersPerMultiprocessor, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.MaxRegistersPerMultiprocessor = maxRegistersPerMultiprocessor;

            int managedMemory = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref managedMemory, CUDeviceAttribute.ManagedMemory, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.ManagedMemory = managedMemory > 0;

            int multiGPUBoard = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref multiGPUBoard, CUDeviceAttribute.MultiGpuBoard, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.MultiGPUBoard = multiGPUBoard > 0;

            int multiGPUBoardGroupID = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref multiGPUBoardGroupID, CUDeviceAttribute.MultiGpuBoardGroupID, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.MultiGPUBoardGroupID = multiGPUBoardGroupID;


            int hostNativeAtomicSupported = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref hostNativeAtomicSupported, CUDeviceAttribute.HostNativeAtomicSupported, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.HostNativeAtomicSupported = hostNativeAtomicSupported == 1;

            int singleToDoublePrecisionPerfRatio = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref singleToDoublePrecisionPerfRatio, CUDeviceAttribute.SingleToDoublePrecisionPerfRatio, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.SingleToDoublePrecisionPerfRatio = singleToDoublePrecisionPerfRatio;

            int pageableMemoryAccess = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref pageableMemoryAccess, CUDeviceAttribute.PageableMemoryAccess, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.PageableMemoryAccess = pageableMemoryAccess > 0;

            int concurrentManagedAccess = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref concurrentManagedAccess, CUDeviceAttribute.ConcurrentManagedAccess, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.ConcurrentManagedAccess = concurrentManagedAccess > 0;

            int computePreemptionSupported = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref computePreemptionSupported, CUDeviceAttribute.ComputePreemptionSupported, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.ComputePreemptionSupported = computePreemptionSupported > 0;

            int canUseHostPointerForRegisteredMem = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref canUseHostPointerForRegisteredMem, CUDeviceAttribute.CanUseHostPointerForRegisteredMem, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.CanUseHostPointerForRegisteredMem = canUseHostPointerForRegisteredMem > 0;


            //int canUseStreamMemOps = 0;
            //res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref canUseStreamMemOps, CUDeviceAttribute.CanUseStreamMemOpsV1, device);
            //Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuDeviceGetAttribute", res));
            //if (res != CUResult.Success) throw new CudaException(res);
            //props.CanUseStreamMemOps = canUseStreamMemOps > 0;

            int canUse64BitStreamMemOps = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref canUse64BitStreamMemOps, CUDeviceAttribute.CanUse64BitStreamMemOps, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.CanUse64BitStreamMemOps = canUse64BitStreamMemOps > 0;

            int cooperativeLaunch = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref cooperativeLaunch, CUDeviceAttribute.CooperativeLaunch, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.CooperativeLaunch = cooperativeLaunch > 0;

            int cooperativeMultiDeviceLaunch = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref cooperativeMultiDeviceLaunch, CUDeviceAttribute.CooperativeMultiDeviceLaunch, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.CooperativeMultiDeviceLaunch = cooperativeMultiDeviceLaunch > 0;

            int maxSharedMemoryPerBlockOptin = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref maxSharedMemoryPerBlockOptin, CUDeviceAttribute.MaxSharedMemoryPerBlockOptin, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.MaxSharedMemoryPerBlockOptin = maxSharedMemoryPerBlockOptin;

            int canFlushRemoteWrites = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref canFlushRemoteWrites, CUDeviceAttribute.CanFlushRemoteWrites, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.CanFlushRemoteWrites = canFlushRemoteWrites > 0;

            int hostRegisterSupported = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref hostRegisterSupported, CUDeviceAttribute.HostRegisterSupported, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.HostRegisterSupported = hostRegisterSupported > 0;

            int pageableMemoryAccessUsesHostPageTables = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref pageableMemoryAccessUsesHostPageTables, CUDeviceAttribute.PageableMemoryAccessUsesHostPageTables, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.PageableMemoryAccessUsesHostPageTables = pageableMemoryAccessUsesHostPageTables > 0;

            int directManagedMemoryAccessFromHost = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref directManagedMemoryAccessFromHost, CUDeviceAttribute.DirectManagedMemoryAccessFromHost, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.DirectManagedMemoryAccessFromHost = directManagedMemoryAccessFromHost > 0;

            int virtualMemoryManagementSupported = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref virtualMemoryManagementSupported, CUDeviceAttribute.VirtualMemoryManagementSupported, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.VirtualMemoryManagementSupported = virtualMemoryManagementSupported > 0;

            int handleTypePosixFileDescriptorSupported = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref handleTypePosixFileDescriptorSupported, CUDeviceAttribute.HandleTypePosixFileDescriptorSupported, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.HandleTypePosixFileDescriptorSupported = handleTypePosixFileDescriptorSupported > 0;

            int handleTypeWin32HandleSupported = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref handleTypeWin32HandleSupported, CUDeviceAttribute.HandleTypeWin32HandleSupported, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.HandleTypeWin32HandleSupported = handleTypeWin32HandleSupported > 0;

            int handleTypeWin32KMTHandleSupported = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref handleTypeWin32KMTHandleSupported, CUDeviceAttribute.HandleTypeWin32KMTHandleSupported, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.HandleTypeWin32KMTHandleSupported = handleTypeWin32KMTHandleSupported > 0;

            int maxBlocksPerMultiProcessor = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref maxBlocksPerMultiProcessor, CUDeviceAttribute.MaxBlocksPerMultiProcessor, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.MaxBlocksPerMultiProcessor = maxBlocksPerMultiProcessor;

            int genericCompressionSupported = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref genericCompressionSupported, CUDeviceAttribute.GenericCompressionSupported, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.GenericCompressionSupported = genericCompressionSupported > 0;

            int maxPersistingL2CacheSize = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref maxPersistingL2CacheSize, CUDeviceAttribute.MaxPersistingL2CacheSize, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.MaxPersistingL2CacheSize = maxPersistingL2CacheSize;

            int maxAccessPolicyWindowSize = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref maxAccessPolicyWindowSize, CUDeviceAttribute.MaxAccessPolicyWindowSize, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.MaxAccessPolicyWindowSize = maxAccessPolicyWindowSize;

            int gPUDirectRDMAWithCudaVMMSupported = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref gPUDirectRDMAWithCudaVMMSupported, CUDeviceAttribute.GPUDirectRDMAWithCudaVMMSupported, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.GPUDirectRDMAWithCudaVMMSupported = gPUDirectRDMAWithCudaVMMSupported > 0;

            int reservedSharedMemoryPerBlock = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref reservedSharedMemoryPerBlock, CUDeviceAttribute.ReservedSharedMemoryPerBlock, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.ReservedSharedMemoryPerBlock = reservedSharedMemoryPerBlock;

            int sparseCudaArraySupported = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref sparseCudaArraySupported, CUDeviceAttribute.SparseCudaArraySupported, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.SparseCudaArraySupported = sparseCudaArraySupported > 0;

            int readOnlyHostRegisterSupported = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref readOnlyHostRegisterSupported, CUDeviceAttribute.ReadOnlyHostRegisterSupported, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.ReadOnlyHostRegisterSupported = readOnlyHostRegisterSupported > 0;


            int gpuDirectRDMASupported = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref gpuDirectRDMASupported, CUDeviceAttribute.GpuDirectRDMASupported, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.GpuDirectRDMASupported = gpuDirectRDMASupported > 0;

            int gpuDirectRDMAFlushWritesOptions = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref gpuDirectRDMAFlushWritesOptions, CUDeviceAttribute.GpuDirectRDMAFlushWritesOptions, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.GpuDirectRDMAFlushWritesOptions = (CUflushGPUDirectRDMAWritesOptions)gpuDirectRDMAFlushWritesOptions;

            int gpuDirectRDMAWritesOrdering = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref gpuDirectRDMAWritesOrdering, CUDeviceAttribute.GpuDirectRDMAWritesOrdering, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.GpuDirectRDMAWritesOrdering = (CUGPUDirectRDMAWritesOrdering)gpuDirectRDMAWritesOrdering;

            int mempoolSupportedHandleTypes = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref mempoolSupportedHandleTypes, CUDeviceAttribute.MempoolSupportedHandleTypes, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.MempoolSupportedHandleTypes = (CUmemAllocationHandleType)mempoolSupportedHandleTypes;


            int clusterLaunch = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref clusterLaunch, CUDeviceAttribute.ClusterLaunch, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.ClusterLaunch = clusterLaunch != 0;

            int deferredMappingCudaArraySupported = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref deferredMappingCudaArraySupported, CUDeviceAttribute.DeferredMappingCudaArraySupported, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.DeferredMappingCudaArraySupported = deferredMappingCudaArraySupported != 0;

            int dmaBufSupported = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref dmaBufSupported, CUDeviceAttribute.DmaBufSupported, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.DmaBufSupported = dmaBufSupported != 0;

            int ipcEventSupported = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref ipcEventSupported, CUDeviceAttribute.IPCEventSupported, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.IPCEventSupported = ipcEventSupported != 0;

            int memSyncDomainCount = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref memSyncDomainCount, CUDeviceAttribute.MemSyncDomainCount, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.MemSyncDomainCount = memSyncDomainCount;

            int tensorMapAccessSupported = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref tensorMapAccessSupported, CUDeviceAttribute.TensorMapAccessSupported, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.TensorMapAccessSupported = tensorMapAccessSupported != 0;

            int unifiedFunctionPointers = 0;
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetAttribute(ref unifiedFunctionPointers, CUDeviceAttribute.UnifiedFunctionPointers, device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetAttribute"}: {res}");
            if (res != CUResult.Success) throw new CudaException(res);
            props.UnifiedFunctionPointers = unifiedFunctionPointers != 0;

            return props;
        }

        /// <summary>
        /// Gets the CUdevice for a given device ordinal number
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public static CUdevice GetCUdevice(int deviceId)
        {
            CUResult res;
            int deviceCount = GetDeviceCount();

            if (deviceCount == 0)
            {
                throw new CudaException(CUResult.ErrorNoDevice, "Cuda initialization error: There is no device supporting CUDA", null);
            }

            if (deviceId < 0 || deviceId > deviceCount - 1)
                throw new ArgumentOutOfRangeException("deviceID", deviceId, "The device ID is not in the range [0.." + (deviceCount - 1).ToString() + "]");

            CUdevice device = new CUdevice();
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGet(ref device, deviceId);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGet"}: {res}");
            if (res != CUResult.Success)
                throw new CudaException(res);

            return device;
        }

        /// <summary>
        /// Initialize the profiling.<para/>
        /// Using this API user can initialize the CUDA profiler by specifying
        /// the configuration file, output file and output file format. This
        /// API is generally used to profile different set of counters by
        /// looping the kernel launch. The <c>configFile</c> parameter can be used
        /// to select profiling options including profiler counters. Refer to
        /// the "Compute Command Line Profiler User Guide" for supported
        /// profiler options and counters.<para/>
        /// Limitation: The CUDA profiler cannot be initialized with this API
        /// if another profiling tool is already active, as indicated by the
        /// exception <see cref="CUResult.ErrorProfilerDisabled"/>.
        /// </summary>
        /// <param name="configFile">Name of the config file that lists the counters/options for profiling.</param>
        /// <param name="outputFile">Name of the outputFile where the profiling results will be stored.</param>
        /// <param name="outputMode">outputMode</param>
        public static void ProfilerInitialize(string configFile, string outputFile, CUoutputMode outputMode)
        {
            CUResult res;
            res = DriverAPINativeMethods.Profiling.cuProfilerInitialize(configFile, outputFile, outputMode);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuProfilerInitialize"}: {res}");
            if (res != CUResult.Success)
                throw new CudaException(res);
        }

        /// <summary>
        /// Enable profiling.<para/>
        /// Enables profile collection by the active profiling tool for the
        /// current context. If profiling is already enabled, then
        /// cuProfilerStart() has no effect.<para/>
        /// cuProfilerStart and cuProfilerStop APIs are used to
        /// programmatically control the profiling granularity by allowing
        /// profiling to be done only on selective pieces of code.
        /// </summary>
        public static void ProfilerStart()
        {
            CUResult res;
            res = DriverAPINativeMethods.Profiling.cuProfilerStart();
            Debug.WriteLine($"{DateTime.Now:G}, {"cuProfilerStart"}: {res}");
            if (res != CUResult.Success)
                throw new CudaException(res);
        }

        /// <summary>
        /// Disables profile collection by the active profiling tool for the
        /// current context. If profiling is already disabled, then
        /// cuProfilerStop() has no effect.<para/>
        /// cuProfilerStart and cuProfilerStop APIs are used to
        /// programmatically control the profiling granularity by allowing
        /// profiling to be done only on selective pieces of code.
        /// </summary>
        public static void ProfilerStop()
        {
            CUResult res;
            res = DriverAPINativeMethods.Profiling.cuProfilerStop();
            Debug.WriteLine($"{DateTime.Now:G}, {"cuProfilerStop"}: {res}");
            if (res != CUResult.Success)
                throw new CudaException(res);
        }

        /// <summary>
        /// Resets all persisting lines in cache to normal status.<para/>
        /// CtxResetPersistingL2Cache Resets all persisting lines in cache to normal
        /// status. Takes effect on function return.
        /// </summary>
        public static void CtxResetPersistingL2Cache()
        {
            CUResult res;
            res = DriverAPINativeMethods.ContextManagement.cuCtxResetPersistingL2Cache();
            Debug.WriteLine($"{DateTime.Now:G}, {"cuCtxResetPersistingL2Cache"}: {res}");
            if (res != CUResult.Success)
                throw new CudaException(res);
        }

        /// <summary>
        /// Returns the execution affinity setting for the current context.
        /// </summary>
        public static CUexecAffinityParam GetExecAffinity(CUexecAffinityType type)
        {
            CUResult res;
            CUexecAffinityParam param = new CUexecAffinityParam();
            res = DriverAPINativeMethods.ContextManagement.cuCtxGetExecAffinity(ref param, type);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuCtxGetExecAffinity"}: {res}");
            if (res != CUResult.Success)
                throw new CudaException(res);
            return param;
        }

        /// <summary>
        /// Returns the maximum number of elements allocatable in a 1D linear texture for a given texture element size.
        /// Returns in \p maxWidthInElements the maximum number of texture elements allocatable in a 1D linear texture
        /// for given \p format and \p numChannels.
        /// </summary>
        /// <param name="format">Texture format.</param>
        /// <param name="numChannels">Number of channels per texture element.</param>
        /// <returns>Returned maximum number of texture elements allocatable for given \p format and \p numChannels.</returns>
        public SizeT GetTexture1DLinearMaxWidth(CUArrayFormat format, uint numChannels)
        {
            CUResult res;
            SizeT maxWidthInElements = new SizeT();
            res = DriverAPINativeMethods.DeviceManagement.cuDeviceGetTexture1DLinearMaxWidth(ref maxWidthInElements, format, numChannels, _device);
            Debug.WriteLine($"{DateTime.Now:G}, {"cuDeviceGetTexture1DLinearMaxWidth"}: {res}");
            if (res != CUResult.Success)
                throw new CudaException(res);
            return maxWidthInElements;
        }

        /// <summary>
        /// Gets the Cuda context bound to this managed Cuda object
        /// </summary>
        public CUcontext Context
        {
            get { return _context; }
        }

        /// <summary>
        /// Gets the Cuda device allocated to the Cuda Context
        /// </summary>
        public CUdevice Device
        {
            get { return _device; }
        }

        /// <summary>
        /// Gets the Id of the Cuda device.
        /// </summary>
        public int DeviceId
        {
            get { return _deviceID; }
        }

        /// <summary>
        /// Indicates if the CudaContext instance created the wrapped cuda context (return = true) or if the CudaContext instance was bound to an existing cuda context.
        /// </summary>
        public bool IsContextOwner
        {
            get { return _contextOwner; }
        }

        /// <summary>
        /// Returns the flags for the current context.
        /// </summary>
        public CUCtxFlags Flags
        {
            get
            {
                if (disposed) throw new ObjectDisposedException(this.ToString());
                CUResult res;
                CUCtxFlags flags = new CUCtxFlags();
                res = DriverAPINativeMethods.ContextManagement.cuCtxGetFlags(ref flags);
                Debug.WriteLine($"{DateTime.Now:G}, {"cuCtxGetFlags"}: {res}");
                if (res != CUResult.Success) throw new CudaException(res);
                return flags;
            }
        }

        /// <summary>
        /// Returns the unique Id associated with the context supplied<para/>
        /// The Id is unique for the life of the program for this instance of CUDA.
        /// </summary>
        public ulong ID
        {
            get
            {
                if (disposed) throw new ObjectDisposedException(this.ToString());
                CUResult res;
                ulong id = 0;
                res = DriverAPINativeMethods.ContextManagement.cuCtxGetId(_context, ref id);
                Debug.WriteLine($"{DateTime.Now:G}, {"cuCtxGetId"}: {res}");
                if (res != CUResult.Success) throw new CudaException(res);
                return id;
            }
        }
    }
}
