using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using BrightData.Helper;
using ManagedCuda;
using ManagedCuda.BasicTypes;

namespace BrightData.Cuda.Helper
{
    /// <summary>
	/// Maintains a cache of available device memory
	/// </summary>
    public class DeviceMemoryBlock : IDeviceMemoryPtr
    {
        readonly MemoryPool? _memoryPool = null;
        readonly CudaDeviceVariable<float> _data;
        bool _disposed = false;
	    int _refCount = 0;

        static long _nextIndex = 0;

#if DEBUG
        static readonly long _badAlloc = -1;
        static readonly long _badDispose = -1;
        static ThreadSafeHashSet<DeviceMemoryBlock> AllocatedBlocks = new();
#endif

        public DeviceMemoryBlock(uint size) 
        {
            Index = Interlocked.Increment(ref _nextIndex);

	        var sizeInBytes = size * CudaProvider.FloatSize;
	        var ptr = new CUdeviceptr();
	        var result = DriverAPINativeMethods.MemoryManagement.cuMemAlloc_v2(ref ptr, sizeInBytes);
            CudaProvider.CheckForError(result);
            _data = new CudaDeviceVariable<float>(ptr, true, sizeInBytes);
            
#if DEBUG
            AllocatedBlocks.Add(this);
            if (Index == _badAlloc)
                Debugger.Break();
#endif
        }
        public DeviceMemoryBlock(MemoryPool? memoryPool, CudaDeviceVariable<float> data)
        {
            _data = data;
            _memoryPool = memoryPool;
            Index = Interlocked.Increment(ref _nextIndex);
#if DEBUG
            if (Index == _badAlloc)
                Debugger.Break();
            AllocatedBlocks.Add(this);
#endif
        }

#if DEBUG
        ~DeviceMemoryBlock()
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
        public void Dispose()
        {
            Release();
        }
        public override string ToString()
        {
            var valid = IsValid ? "" : " (invalid)";
            return $"{Index}, {_data.SizeInBytes} bytes {valid}";
        }

        public long Index { get; }
        public bool IsValid => !_disposed;

        public int AddRef()
	    {
		    return Interlocked.Increment(ref _refCount);
	    }
        public int Release()
        {
            var ret = Interlocked.Decrement(ref _refCount);
            if (ret <= 0 && !_disposed) {
                _memoryPool?.Recycle(_data.SizeInBytes, _data.DevicePointer);
                _data.Dispose();
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

        public CudaDeviceVariable<float> DeviceVariable => _data;
	    public CUdeviceptr DevicePointer => _data.DevicePointer;
	    public uint Size => _data.Size;

	    public void CopyToDevice(float[] source)
        {
            _data.CopyToDevice(source);
        }
        public void CopyToDevice(IDeviceMemoryPtr source)
        {
            _data.CopyToDevice(source.DeviceVariable);
        }
        public unsafe void CopyToDevice(ReadOnlySpan<float> span, uint offsetSource)
        {
            fixed (float* p = &MemoryMarshal.GetReference(span))
            {
                DeviceVariable.CopyToDevice((IntPtr)p, offsetSource, 0, (int)Size * sizeof(float));
            }
        }

        public unsafe void CopyToDevice(float* ptr, uint sourceOffset, uint targetOffset, uint size)
        {
            DeviceVariable.CopyToDevice((IntPtr)ptr, sourceOffset, targetOffset, (int)size * sizeof(float));
        }

        public void CopyToHost(float[] target)
        {
            _data.CopyToHost(target);
        }
        public void CopyToHost(ArraySegment<float> target)
        {
            _data.CopyToHost(target.Array!, 0, target.Offset, target.Count * sizeof(float));
        }
        public void Clear()
        {
            _data.Memset(0);
        }
    }
}
