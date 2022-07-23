using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BrightData.Helper;
using ManagedCuda;
using ManagedCuda.BasicTypes;

namespace BrightData.Cuda.Helper
{
    public abstract unsafe class DeviceMemoryBlockBase : IDeviceMemoryPtr
    {
        protected CudaDeviceVariable<float> _data;
        bool _disposed = false;
        int _refCount = 0;

        static long _nextIndex = 0;

#if DEBUG
        static readonly long _badAlloc = -1;
        static readonly long _badDispose = -1;
        static readonly ThreadSafeHashSet<DeviceMemoryBlockBase> AllocatedBlocks = new();
#endif
        protected DeviceMemoryBlockBase()
        {
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

        public int AddRef() => Interlocked.Increment(ref _refCount);
        public long Index { get; }
        public bool IsValid => !_disposed;
        protected abstract void OnDispose();
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

        public uint Size => _data.Size;
        public void Dispose() => Release();

        public CudaDeviceVariable<float> DeviceVariable => _data;
        public CUdeviceptr DevicePointer => _data.DevicePointer;
        public virtual void CopyToDevice(float[] source)
        {
            fixed (float* ptr = &source[0]) {
                DriverAPINativeMethods.SynchronousMemcpy_v2.cuMemcpyHtoD_v2(DevicePointer, (IntPtr)ptr, source.Length * sizeof(float)).CheckResult();
            }
            //_data.CopyToDevice(source);
        }

        public virtual void CopyToDevice(IDeviceMemoryPtr source)
        {
            DriverAPINativeMethods.SynchronousMemcpy_v2.cuMemcpyDtoD_v2(DevicePointer, source.DevicePointer, source.Size * sizeof(float)).CheckResult();
            //_data.CopyToDevice(source.DeviceVariable);
        }

        public virtual void CopyToDevice(ReadOnlySpan<float> span, uint offsetSource = 0)
        {
            fixed (float* p = &MemoryMarshal.GetReference(span))
            {
                var ptr = p + offsetSource * sizeof(float);
                DriverAPINativeMethods.SynchronousMemcpy_v2.cuMemcpyHtoD_v2(DevicePointer, (IntPtr)ptr, Size * sizeof(float)).CheckResult();
                //DeviceVariable.CopyToDevice((IntPtr)p, offsetSource, 0, (int)Size * sizeof(float));
            }
        }

        public virtual void CopyToDevice(float* ptr, uint sourceOffset, uint targetOffset, uint size)
        {
            DriverAPINativeMethods.SynchronousMemcpy_v2.cuMemcpyHtoD_v2(DevicePointer + targetOffset * sizeof(float), (IntPtr)(ptr + sourceOffset), size * sizeof(float)).CheckResult();
            //DeviceVariable.CopyToDevice((IntPtr)ptr, sourceOffset, targetOffset, (int)size * sizeof(float));
        }

        public virtual void CopyToHost(float[] target)
        {
            DriverAPINativeMethods.SynchronousMemcpy_v2.cuMemcpyDtoH_v2(target, DevicePointer, target.Length * sizeof(float));
            //_data.CopyToHost(target);
        }

        public virtual void CopyToHost(ArraySegment<float> target)
        {
            fixed (float* p = &target.Array![0]) {
                var ptr = p + target.Offset * sizeof(float);
                DriverAPINativeMethods.SynchronousMemcpy_v2.cuMemcpyDtoH_v2((IntPtr)ptr, DevicePointer, target.Count * sizeof(float));
            }
            //_data.CopyToHost(target.Array!, 0, target.Offset, target.Count * sizeof(float));
        }

        public void Clear()
        {
            _data.Memset(0);
        }

        public IDeviceMemoryPtr Offset(uint offsetInFloats, uint size)
        {
            var offsetPtr = _data.DevicePointer.Pointer + (offsetInFloats * sizeof(float));
            return new PtrToMemory(this, new CUdeviceptr(offsetPtr), size * sizeof(float));
        }

        public override string ToString()
        {
            var valid = IsValid ? "" : " (invalid)";
            return $"{Index}, {_data.SizeInBytes} bytes {valid}";
        }
    }
}
