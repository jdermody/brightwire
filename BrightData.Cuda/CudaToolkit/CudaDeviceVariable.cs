using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace BrightData.Cuda.CudaToolkit
{
    internal class CudaDeviceVariable<T> : IDisposable where T : struct
    {
        readonly CUdeviceptr _devPtr;
        readonly SizeT _size = 0;
        readonly SizeT _typeSize;
        CuResult _res;
        bool _disposed;
        bool _isOwner;

        public CudaDeviceVariable(SizeT size)
        {
            _devPtr = new CUdeviceptr();
            _size = size;
            _typeSize = (uint)Marshal.SizeOf(typeof(T));

            _res = DriverApiNativeMethods.MemoryManagement.cuMemAlloc_v2(ref _devPtr, _typeSize * size);
            
            if (_res != CuResult.Success) throw new CudaException(_res);
            _isOwner = true;
        }
        public CudaDeviceVariable(SizeT size, CUstream stream)
        {
            _devPtr = new CUdeviceptr();
            _size = size;
            _typeSize = (uint)Marshal.SizeOf(typeof(T));

            _res = DriverApiNativeMethods.MemoryManagement.cuMemAllocAsync(ref _devPtr, _typeSize * size, stream);
            
            if (_res != CuResult.Success) throw new CudaException(_res);
            _isOwner = true;
        }
        public CudaDeviceVariable(CUdeviceptr devPtr)
            : this(devPtr, false)
        {
        }
        public CudaDeviceVariable(CUdeviceptr devPtr, bool isOwner)
        {
            _devPtr = devPtr;
            var nullPtr = new CUdeviceptr();
            _res = DriverApiNativeMethods.MemoryManagement.cuMemGetAddressRange_v2(ref nullPtr, ref _size, devPtr);
            
            if (_res != CuResult.Success) throw new CudaException(_res);
            _typeSize = (uint)Marshal.SizeOf(typeof(T));
            var sizeInBytes = _size;
            _size = sizeInBytes / _typeSize;
            if (sizeInBytes != _size * _typeSize)
                throw new CudaException("Variable size is not a multiple of its type size.");
            _isOwner = isOwner;
        }
        public CudaDeviceVariable(CUdeviceptr devPtr, SizeT size)
            : this(devPtr, false, size)
        {
        }
        public CudaDeviceVariable(CUdeviceptr devPtr, bool isOwner, SizeT size)
        {
            _devPtr = devPtr;
            _typeSize = (uint)Marshal.SizeOf(typeof(T));
            _size = size / _typeSize;
            if (size != _size * _typeSize)
                throw new CudaException("Variable size is not a multiple of its type size.");
            _isOwner = isOwner;
        }
        public CudaDeviceVariable(CUmodule module, string name)
        {
            _devPtr = new CUdeviceptr();
            var sizeInBytes = new SizeT();
            _res = DriverApiNativeMethods.ModuleManagement.cuModuleGetGlobal_v2(ref _devPtr, ref sizeInBytes, module, name);
            
            if (_res != CuResult.Success) throw new CudaException(_res);

            _typeSize = Marshal.SizeOf(typeof(T));
            _size = sizeInBytes / _typeSize;

            if (sizeInBytes != _size * _typeSize)
                throw new CudaException("Variable size is not a multiple of its type size.");
            _isOwner = false;
        }
        public CudaDeviceVariable(CUlibrary library, string name)
        {
            _devPtr = new CUdeviceptr();
            var sizeInBytes = new SizeT();
            _res = DriverApiNativeMethods.LibraryManagement.cuLibraryGetGlobal(ref _devPtr, ref sizeInBytes, library, name);
            
            if (_res != CuResult.Success) throw new CudaException(_res);

            _typeSize = Marshal.SizeOf(typeof(T));
            _size = sizeInBytes / _typeSize;

            if (sizeInBytes != _size * _typeSize)
                throw new CudaException("Variable size is not a multiple of its type size.");
            _isOwner = false;
        }
        ~CudaDeviceVariable()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        public void Dispose(CUstream stream)
        {
            _res = DriverApiNativeMethods.MemoryManagement.cuMemFreeAsync(_devPtr, stream);
            
            _isOwner = false;
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool fDisposing)
        {
            if (fDisposing && !_disposed) {
                if (_isOwner) {
                    _res = DriverApiNativeMethods.MemoryManagement.cuMemFree_v2(_devPtr);
                }

                _disposed = true;
            }
        }
        SizeT MinSize(SizeT value)
        {
            return (ulong)value < (ulong)_size ? value : _size;
        }
        public void CopyToDevice(CUdeviceptr source)
        {
            if (_disposed) throw new ObjectDisposedException(this.ToString());
            var aSizeInBytes = _size * _typeSize;
            CuResult res;
            res = DriverApiNativeMethods.SynchronousMemcpyV2.cuMemcpyDtoD_v2(_devPtr, source, aSizeInBytes);
            
            if (res != CuResult.Success)
                throw new CudaException(res);
        }
        public void CopyToDevice(CUdeviceptr source, SizeT offsetSrc, SizeT offsetDest, SizeT sizeInBytes)
        {
            if (_disposed) throw new ObjectDisposedException(this.ToString());
            CuResult res;
            res = DriverApiNativeMethods.SynchronousMemcpyV2.cuMemcpyDtoD_v2(_devPtr + offsetDest, source + offsetSrc, sizeInBytes);
            
            if (res != CuResult.Success)
                throw new CudaException(res);
        }
        public void CopyToDevice(CudaDeviceVariable<T> source)
        {
            if (_disposed) throw new ObjectDisposedException(this.ToString());
            var aSizeInBytes = MinSize(source.Size) * _typeSize;
            CuResult res;
            res = DriverApiNativeMethods.SynchronousMemcpyV2.cuMemcpyDtoD_v2(_devPtr, source.DevicePointer, aSizeInBytes);
            
            if (res != CuResult.Success)
                throw new CudaException(res);
        }
        public void CopyToDevice(CudaDeviceVariable<T> source, SizeT offsetSrc, SizeT offsetDest, SizeT sizeInBytes)
        {
            if (_disposed) throw new ObjectDisposedException(this.ToString());
            CuResult res;
            res = DriverApiNativeMethods.SynchronousMemcpyV2.cuMemcpyDtoD_v2(_devPtr + offsetDest, source.DevicePointer + offsetSrc, sizeInBytes);
            
            if (res != CuResult.Success)
                throw new CudaException(res);
        }
        public void CopyToDevice(T[] source)
        {
            if (_disposed) throw new ObjectDisposedException(this.ToString());
            var aSizeInBytes = MinSize(source.LongLength) * _typeSize;
            var handle = GCHandle.Alloc(source, GCHandleType.Pinned);
            CuResult res;
            try {
                var ptr = handle.AddrOfPinnedObject();
                res = DriverApiNativeMethods.SynchronousMemcpyV2.cuMemcpyHtoD_v2(_devPtr, ptr, aSizeInBytes);
                
            }
            finally {
                handle.Free();
            }

            if (res != CuResult.Success)
                throw new CudaException(res);
        }
        public void CopyToDevice(T[] source, SizeT offsetSrc, SizeT offsetDest, SizeT sizeInBytes)
        {
            if (_disposed) throw new ObjectDisposedException(this.ToString());
            var handle = GCHandle.Alloc(source, GCHandleType.Pinned);
            CuResult res;
            try {
                var ptr = new IntPtr(handle.AddrOfPinnedObject().ToInt64() + (long)offsetSrc);
                res = DriverApiNativeMethods.SynchronousMemcpyV2.cuMemcpyHtoD_v2(_devPtr + offsetDest, ptr, sizeInBytes);
                
            }
            finally {
                handle.Free();
            }

            if (res != CuResult.Success)
                throw new CudaException(res);
        }
        public void CopyToDevice(T source)
        {
            if (_disposed) throw new ObjectDisposedException(this.ToString());
            var aSizeInBytes = _typeSize;
            var handle = GCHandle.Alloc(source, GCHandleType.Pinned);
            CuResult res;
            try {
                var ptr = handle.AddrOfPinnedObject();
                res = DriverApiNativeMethods.SynchronousMemcpyV2.cuMemcpyHtoD_v2(_devPtr, ptr, aSizeInBytes);
                
            }
            finally {
                handle.Free();
            }

            if (res != CuResult.Success)
                throw new CudaException(res);
        }
        public void CopyToDevice(T source, SizeT offsetDest)
        {
            if (_disposed) throw new ObjectDisposedException(this.ToString());
            var aSizeInBytes = _typeSize;
            var handle = GCHandle.Alloc(source, GCHandleType.Pinned);
            CuResult res;
            try {
                var ptr = handle.AddrOfPinnedObject();
                res = DriverApiNativeMethods.SynchronousMemcpyV2.cuMemcpyHtoD_v2(_devPtr + offsetDest, ptr, aSizeInBytes);
                
            }
            finally {
                handle.Free();
            }

            if (res != CuResult.Success)
                throw new CudaException(res);
        }
        public void CopyToDevice(IntPtr source, SizeT offsetSrc, SizeT offsetDest, SizeT sizeInBytes)
        {
            if (_disposed) throw new ObjectDisposedException(this.ToString());

            CuResult res;
            var ptr = new IntPtr(source.ToInt64() + (long)offsetSrc);
            res = DriverApiNativeMethods.SynchronousMemcpyV2.cuMemcpyHtoD_v2(_devPtr + offsetDest, ptr, sizeInBytes);
            

            if (res != CuResult.Success)
                throw new CudaException(res);
        }
        public void CopyToDevice(IntPtr source, SizeT offsetDest)
        {
            if (_disposed) throw new ObjectDisposedException(this.ToString());

            CuResult res;
            res = DriverApiNativeMethods.SynchronousMemcpyV2.cuMemcpyHtoD_v2(_devPtr + offsetDest, source, _size * _typeSize);
            

            if (res != CuResult.Success)
                throw new CudaException(res);
        }
        public void CopyToDevice(IntPtr source)
        {
            if (_disposed) throw new ObjectDisposedException(this.ToString());

            CuResult res;
            res = DriverApiNativeMethods.SynchronousMemcpyV2.cuMemcpyHtoD_v2(_devPtr, source, _size * _typeSize);
            

            if (res != CuResult.Success)
                throw new CudaException(res);
        }
        public void CopyToDevice(Array hostSrc)
        {
            if (_disposed) throw new ObjectDisposedException(this.ToString());
            var aSizeInBytes = _size * _typeSize;
            var handle = GCHandle.Alloc(hostSrc, GCHandleType.Pinned);
            try {
                var ptr = handle.AddrOfPinnedObject();
                _res = DriverApiNativeMethods.SynchronousMemcpyV2.cuMemcpyHtoD_v2(_devPtr, ptr, aSizeInBytes);
                
            }
            finally {
                handle.Free();
            }

            if (_res != CuResult.Success)
                throw new CudaException(_res);
        }
        public void CopyToHost(T[] dest)
        {
            if (_disposed) throw new ObjectDisposedException(this.ToString());
            var aSizeInBytes = MinSize(dest.LongLength) * _typeSize;
            var handle = GCHandle.Alloc(dest, GCHandleType.Pinned);
            CuResult res;
            try {
                var ptr = handle.AddrOfPinnedObject();
                res = DriverApiNativeMethods.SynchronousMemcpyV2.cuMemcpyDtoH_v2(ptr, _devPtr, aSizeInBytes);
                
            }
            finally {
                handle.Free();
            }

            if (res != CuResult.Success)
                throw new CudaException(res);
        }
        public void CopyToHost(T[] dest, SizeT offsetSrc, SizeT offsetDest, SizeT sizeInBytes)
        {
            if (_disposed) throw new ObjectDisposedException(this.ToString());
            var handle = GCHandle.Alloc(dest, GCHandleType.Pinned);
            CuResult res;
            try {
                var ptr = new IntPtr(handle.AddrOfPinnedObject().ToInt64() + (long)offsetDest);
                res = DriverApiNativeMethods.SynchronousMemcpyV2.cuMemcpyDtoH_v2(ptr, _devPtr + offsetSrc, sizeInBytes);
                
            }
            finally {
                handle.Free();
            }

            if (res != CuResult.Success)
                throw new CudaException(res);
        }
        public void CopyToHost(ref T dest)
        {
            if (_disposed) throw new ObjectDisposedException(this.ToString());
            var aSizeInBytes = _typeSize;
            var handle = GCHandle.Alloc(dest, GCHandleType.Pinned);
            CuResult res;
            try {
                var ptr = handle.AddrOfPinnedObject();
                res = DriverApiNativeMethods.SynchronousMemcpyV2.cuMemcpyDtoH_v2(ptr, _devPtr, aSizeInBytes);
                
                dest = (T)(Marshal.PtrToStructure(ptr, typeof(T)) ?? throw new InvalidOperationException());
            }
            finally {
                handle.Free();
            }

            if (res != CuResult.Success)
                throw new CudaException(res);
        }
        public void CopyToHost(ref T dest, SizeT offsetSrc)
        {
            if (_disposed) throw new ObjectDisposedException(this.ToString());
            var aSizeInBytes = _typeSize;
            var handle = GCHandle.Alloc(dest, GCHandleType.Pinned);
            CuResult res;
            try {
                var ptr = handle.AddrOfPinnedObject();
                res = DriverApiNativeMethods.SynchronousMemcpyV2.cuMemcpyDtoH_v2(ptr, _devPtr + offsetSrc, aSizeInBytes);
                
                dest = (T)(Marshal.PtrToStructure(ptr, typeof(T)) ?? throw new InvalidOperationException());
            }
            finally {
                handle.Free();
            }

            if (res != CuResult.Success)
                throw new CudaException(res);
        }
        public void CopyToHost(IntPtr dest)
        {
            if (_disposed) throw new ObjectDisposedException(this.ToString());

            CuResult res;
            res = DriverApiNativeMethods.SynchronousMemcpyV2.cuMemcpyDtoH_v2(dest, _devPtr, _size * _typeSize);
            

            if (res != CuResult.Success)
                throw new CudaException(res);
        }
        public void CopyToHost(IntPtr dest, SizeT offsetSrc)
        {
            if (_disposed) throw new ObjectDisposedException(this.ToString());

            CuResult res;
            res = DriverApiNativeMethods.SynchronousMemcpyV2.cuMemcpyDtoH_v2(dest, _devPtr + offsetSrc, _size * _typeSize);
            
            if (res != CuResult.Success)
                throw new CudaException(res);
        }
        public void CopyToHost(IntPtr dest, SizeT offsetSrc, SizeT offsetDest, SizeT sizeInBytes)
        {
            if (_disposed) throw new ObjectDisposedException(this.ToString());

            CuResult res;
            var ptr = new IntPtr(dest.ToInt64() + (long)offsetDest);
            res = DriverApiNativeMethods.SynchronousMemcpyV2.cuMemcpyDtoH_v2(ptr, _devPtr + offsetSrc, sizeInBytes);
            

            if (res != CuResult.Success)
                throw new CudaException(res);
        }
        public void CopyToHost(Array hostDest)
        {
            if (_disposed) throw new ObjectDisposedException(this.ToString());
            var aSizeInBytes = _size * _typeSize;
            var handle = GCHandle.Alloc(hostDest, GCHandleType.Pinned);
            try {
                var ptr = handle.AddrOfPinnedObject();
                _res = DriverApiNativeMethods.SynchronousMemcpyV2.cuMemcpyDtoH_v2(ptr, _devPtr, aSizeInBytes);
                
            }
            finally {
                handle.Free();
            }

            if (_res != CuResult.Success)
                throw new CudaException(_res);
        }
        public void AsyncCopyToDevice(CUdeviceptr source, CUstream stream)
        {
            if (_disposed) throw new ObjectDisposedException(this.ToString());
            var aSizeInBytes = _size * _typeSize;
            CuResult res;
            res = DriverApiNativeMethods.AsynchronousMemcpyV2.cuMemcpyDtoDAsync_v2(_devPtr, source, aSizeInBytes, stream);
            
            if (res != CuResult.Success)
                throw new CudaException(res);
        }
        public void AsyncCopyToDevice(CudaDeviceVariable<T> source, CUstream stream)
        {
            if (_disposed) throw new ObjectDisposedException(this.ToString());
            var aSizeInBytes = MinSize(source.Size) * _typeSize;
            CuResult res;
            res = DriverApiNativeMethods.AsynchronousMemcpyV2.cuMemcpyDtoDAsync_v2(_devPtr, source.DevicePointer, aSizeInBytes, stream);
            
            if (res != CuResult.Success)
                throw new CudaException(res);
        }
        public void AsyncCopyToDevice(CUdeviceptr source, SizeT offsetSrc, SizeT offsetDest, SizeT sizeInBytes, CUstream stream)
        {
            if (_disposed) throw new ObjectDisposedException(this.ToString());
            CuResult res;
            res = DriverApiNativeMethods.AsynchronousMemcpyV2.cuMemcpyDtoDAsync_v2(_devPtr + offsetDest, source + offsetSrc, sizeInBytes, stream);
            
            if (res != CuResult.Success)
                throw new CudaException(res);
        }
        public void AsyncCopyToDevice(CudaDeviceVariable<T> source, SizeT offsetSrc, SizeT offsetDest, SizeT sizeInBytes, CUstream stream)
        {
            if (_disposed) throw new ObjectDisposedException(this.ToString());
            CuResult res;
            res = DriverApiNativeMethods.AsynchronousMemcpyV2.cuMemcpyDtoDAsync_v2(_devPtr + offsetDest, source.DevicePointer + offsetSrc, sizeInBytes, stream);
            
            if (res != CuResult.Success)
                throw new CudaException(res);
        }
        public void Memset(byte aValue)
        {
            if (_disposed) throw new ObjectDisposedException(this.ToString());
            CuResult res;
            res = DriverApiNativeMethods.Memset.cuMemsetD8_v2(_devPtr, aValue, _size * _typeSize);
            
            if (res != CuResult.Success)
                throw new CudaException(res);
        }
        public void PeerCopyToDevice(CudaContext destCtx, CUdeviceptr source, CudaContext sourceCtx)
        {
            if (_disposed) throw new ObjectDisposedException(this.ToString());
            var aSizeInBytes = _size * _typeSize;
            CuResult res;
            res = DriverApiNativeMethods.SynchronousMemcpyV2.cuMemcpyPeer(_devPtr, destCtx.Context, source, sourceCtx.Context, aSizeInBytes);
            
            if (res != CuResult.Success)
                throw new CudaException(res);
        }
        public void PeerCopyToDevice(CudaContext destCtx, CudaDeviceVariable<T> source, CudaContext sourceCtx)
        {
            if (_disposed) throw new ObjectDisposedException(this.ToString());
            var aSizeInBytes = _size * _typeSize;
            CuResult res;
            res = DriverApiNativeMethods.SynchronousMemcpyV2.cuMemcpyPeer(_devPtr, destCtx.Context, source.DevicePointer, sourceCtx.Context, aSizeInBytes);
            
            if (res != CuResult.Success)
                throw new CudaException(res);
        }
        public void PeerCopyToDevice(CudaContext destCtx, CUdeviceptr source, CudaContext sourceCtx, CUstream stream)
        {
            if (_disposed) throw new ObjectDisposedException(this.ToString());
            var aSizeInBytes = _size * _typeSize;
            CuResult res;
            res = DriverApiNativeMethods.AsynchronousMemcpyV2.cuMemcpyPeerAsync(_devPtr, destCtx.Context, source, sourceCtx.Context, aSizeInBytes, stream);
            
            if (res != CuResult.Success)
                throw new CudaException(res);
        }
        public void PeerCopyToDevice(CudaContext destCtx, CudaDeviceVariable<T> source, CudaContext sourceCtx, CUstream stream)
        {
            if (_disposed) throw new ObjectDisposedException(this.ToString());
            var aSizeInBytes = _size * _typeSize;
            CuResult res;
            res = DriverApiNativeMethods.AsynchronousMemcpyV2.cuMemcpyPeerAsync(_devPtr, destCtx.Context, source.DevicePointer, sourceCtx.Context, aSizeInBytes, stream);
            
            if (res != CuResult.Success)
                throw new CudaException(res);
        }
        public CUmemPoolPtrExportData MemPoolExportPointer()
        {
            if (_disposed) throw new ObjectDisposedException(this.ToString());
            var exportData = new CUmemPoolPtrExportData();
            _res = DriverApiNativeMethods.MemoryManagement.cuMemPoolExportPointer(ref exportData, _devPtr);
            
            if (_res != CuResult.Success)
                throw new CudaException(_res);
            return exportData;
        }
        public T this[SizeT index]
        {
            get
            {
                if (_disposed) throw new ObjectDisposedException(this.ToString());

                var position = _devPtr + index * _typeSize;
                var dest = new T();

                var aSizeInBytes = _typeSize;
                var handle = GCHandle.Alloc(dest, GCHandleType.Pinned);
                CuResult res;
                try {
                    var ptr = handle.AddrOfPinnedObject();
                    res = DriverApiNativeMethods.SynchronousMemcpyV2.cuMemcpyDtoH_v2(ptr, position, aSizeInBytes);
                    
                    dest = (T)(Marshal.PtrToStructure(ptr, typeof(T)) ?? throw new InvalidOperationException());
                }
                finally {
                    handle.Free();
                }

                if (res != CuResult.Success)
                    throw new CudaException(res);
                return dest;
            }

            set
            {
                if (_disposed) throw new ObjectDisposedException(this.ToString());
                var position = _devPtr + index * _typeSize;

                var aSizeInBytes = _typeSize;
                var handle = GCHandle.Alloc(value, GCHandleType.Pinned);
                CuResult res;
                try {
                    var ptr = handle.AddrOfPinnedObject();
                    res = DriverApiNativeMethods.SynchronousMemcpyV2.cuMemcpyHtoD_v2(position, ptr, aSizeInBytes);
                    
                }
                finally {
                    handle.Free();
                }

                if (res != CuResult.Success)
                    throw new CudaException(res);
            }
        }
        public CUdeviceptr DevicePointer
        {
            get { return _devPtr; }
        }
        public SizeT SizeInBytes
        {
            get { return _size * _typeSize; }
        }
        public SizeT TypeSize
        {
            get { return _typeSize; }
        }
        public SizeT Size
        {
            get { return _size; }
        }
    }
}