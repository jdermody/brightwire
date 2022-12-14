using System;
using System.Runtime.InteropServices;
using System.Threading;
using ManagedCuda;
using ManagedCuda.BasicTypes;

namespace BrightData.Cuda.Helper
{
    /// <inheritdoc />
    public abstract unsafe class DeviceMemoryBlockBase : IDeviceMemoryPtr
    {
        /// <summary>
        /// CUDA device variable
        /// </summary>
        protected CudaDeviceVariable<float> _data;
        bool _disposed = false;
        int _refCount = 0;

        static long _nextIndex = 0;

#if DEBUG
        static readonly long _badAlloc = -1;
        static readonly long _badDispose = -1;
        static readonly ThreadSafeHashSet<DeviceMemoryBlockBase> AllocatedBlocks = new();
#endif
        /// <summary>
        /// Creates a wrapper from an existing CUDA device block
        /// </summary>
        /// <param name="data"></param>
        protected DeviceMemoryBlockBase(CudaDeviceVariable<float> data)
        {
            _data = data;
            Index = Interlocked.Increment(ref _nextIndex);
#if DEBUG
            AllocatedBlocks.Add(this);
            if (Index == _badAlloc)
                Debugger.Break();
#endif
        }

#if DEBUG
        ~DeviceMemoryBlockBase()
        {
            if (!_disposed)
                Debug.WriteLine($"\tMemory Block {Index} was not disposed - {_data.SizeInBytes} bytes leaked in the GPU !!");
        }

        public static void FindLeakedBlocks(HashSet<IDeviceMemoryPtr> exclude)
        {
            AllocatedBlocks.ForEach(b => {
                if(!exclude.Contains(b))
                    Debug.WriteLine($"Found leaked memory: block {b.Index} ({b.IsValid}): {b.Size:N0} bytes");
            });
        }
#endif

        /// <inheritdoc />
        public int AddRef() => Interlocked.Increment(ref _refCount);

        /// <summary>
        /// Block unique index
        /// </summary>
        public long Index { get; }

        /// <inheritdoc />
        public bool IsValid => !_disposed;

        /// <summary>
        /// Called when the block has been disposed
        /// </summary>
        protected abstract void OnDispose();

        /// <inheritdoc />
        public int Release()
        {
            var ret = Interlocked.Decrement(ref _refCount);
            if (ret <= 0 && !_disposed) {
                OnDispose();
                _disposed = true;
#if DEBUG
                GC.SuppressFinalize(this);
                if (Index == _badDispose)
                    Debugger.Break();
                AllocatedBlocks.Remove(this);
#endif
            }

            return ret;
        }

        /// <inheritdoc />
        public uint Size => _data.Size;

        /// <inheritdoc />
        public void Dispose() => Release();

        /// <inheritdoc />
        public CudaDeviceVariable<float> DeviceVariable => _data;

        /// <inheritdoc />
        public CUdeviceptr DevicePointer => _data.DevicePointer;

        /// <inheritdoc />
        public virtual void CopyToDevice(float[] source)
        {
            fixed (float* ptr = &source[0]) {
                DriverAPINativeMethods.SynchronousMemcpy_v2.cuMemcpyHtoD_v2(DevicePointer, (IntPtr)ptr, Math.Min(Size, source.Length) * sizeof(float)).CheckResult();
            }
            //_data.CopyToDevice(source);
        }

        /// <inheritdoc />
        public virtual void CopyToDevice(IDeviceMemoryPtr source)
        {
            DriverAPINativeMethods.SynchronousMemcpy_v2.cuMemcpyDtoD_v2(DevicePointer, source.DevicePointer, Math.Min(Size, source.Size) * sizeof(float)).CheckResult();
            //_data.CopyToDevice(source.DeviceVariable);
        }

        /// <inheritdoc />
        public virtual void CopyToDevice(ReadOnlySpan<float> span, uint offsetSource = 0)
        {
            fixed (float* p = &MemoryMarshal.GetReference(span))
            {
                var ptr = p + offsetSource * sizeof(float);
                DriverAPINativeMethods.SynchronousMemcpy_v2.cuMemcpyHtoD_v2(DevicePointer, (IntPtr)ptr, Math.Min(Size, span.Length) * sizeof(float)).CheckResult();
                //DeviceVariable.CopyToDevice((IntPtr)p, offsetSource, 0, (int)Size * sizeof(float));
            }
        }

        /// <inheritdoc />
        public virtual void CopyToDevice(float* ptr, uint sourceOffset, uint targetOffset, uint size)
        {
            DriverAPINativeMethods.SynchronousMemcpy_v2.cuMemcpyHtoD_v2(DevicePointer + targetOffset * sizeof(float), (IntPtr)(ptr + sourceOffset), size * sizeof(float)).CheckResult();
            //DeviceVariable.CopyToDevice((IntPtr)ptr, sourceOffset, targetOffset, (int)size * sizeof(float));
        }

        /// <inheritdoc />
        public virtual void CopyToHost(float[] target)
        {
            DriverAPINativeMethods.SynchronousMemcpy_v2.cuMemcpyDtoH_v2(target, DevicePointer, Math.Min(Size, target.Length) * sizeof(float));
            //_data.CopyToHost(target);
        }

        /// <inheritdoc />
        public virtual void CopyToHost(ArraySegment<float> target)
        {
            fixed (float* p = &target.Array![0]) {
                var ptr = p + target.Offset * sizeof(float);
                DriverAPINativeMethods.SynchronousMemcpy_v2.cuMemcpyDtoH_v2((IntPtr)ptr, DevicePointer, Math.Min(Size, target.Count) * sizeof(float));
            }
            //_data.CopyToHost(target.Array!, 0, target.Offset, target.Count * sizeof(float));
        }

        /// <inheritdoc />
        public virtual void Clear()
        {
            DriverAPINativeMethods.Memset.cuMemsetD8_v2(DevicePointer, 0, Size * sizeof(float));
            //if(Size % 2 == 0)
            //    DriverAPINativeMethods.Memset.cuMemsetD32_v2(DevicePointer, 0, Size / 2).CheckResult();
            //else
            //    DriverAPINativeMethods.Memset.cuMemsetD16_v2(DevicePointer, 0, Size).CheckResult();
        }

        /// <inheritdoc />
        public IDeviceMemoryPtr Offset(uint offsetInFloats, uint size)
        {
            var offsetPtr = _data.DevicePointer.Pointer + (offsetInFloats * sizeof(float));
            return new PtrToMemory(this, new CUdeviceptr(offsetPtr), size * sizeof(float));
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var valid = IsValid ? "" : " (invalid)";
            return $"{Index}, {_data.SizeInBytes} bytes {valid}";
        }
    }
}
