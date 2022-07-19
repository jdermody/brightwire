using System;
using System.Runtime.InteropServices;
using ManagedCuda;
using ManagedCuda.BasicTypes;

namespace BrightData.Cuda.Helper
{
    /// <summary>
    /// A pointer to a block of device memory (the block is owned by another pointer)
    /// </summary>
    internal class PtrToMemory : IDeviceMemoryPtr
    {
	    readonly IDeviceMemoryPtr          _rootBlock;
        readonly CudaDeviceVariable<float> _ptr;
        readonly CudaContext               _context;
        bool                               _addedReference = false;

        public PtrToMemory(CudaContext context, IDeviceMemoryPtr rootBlock, CUdeviceptr ptr, SizeT size)
        {
            _context   = context;
            _ptr       = new CudaDeviceVariable<float>(ptr, false, size);
	        _rootBlock = rootBlock;
        }

        public void Dispose()
        {
            if(_addedReference)
                Release();
        }

        public CudaDeviceVariable<float> DeviceVariable => _ptr;
        public CUdeviceptr DevicePointer => _ptr.DevicePointer;
        public uint Size => _ptr.Size;

        public void CopyToHost(ArraySegment<float> target) => _ptr.CopyToHost(target.Array, 0, target.Offset, target.Count * sizeof(float));

        public void Clear()
        {
            _context.ClearMemory(_ptr.DevicePointer, 0, _ptr.SizeInBytes);
        }

	    public void CopyToDevice(float[] source)
        {
	        _ptr.CopyToDevice(source);
        }

        public void CopyToDevice(IDeviceMemoryPtr source)
        {
	        _ptr.CopyToDevice(source.DeviceVariable);
        }
        public unsafe void CopyToDevice(ReadOnlySpan<float> span, uint offsetSource)
        {
            fixed (float* p = &MemoryMarshal.GetReference(span))
            {
                DeviceVariable.CopyToDevice((IntPtr)p, offsetSource, 0, (int)Size * sizeof(float));
            }
        }
        public void CopyToHost(float[] target)
        {
            _context.CopyToHost<float>(target, _ptr.DevicePointer);
        }

	    public int AddRef()
        {
            _addedReference = true;
		    return _rootBlock.AddRef();
	    }

	    public int Release()
        {
	        return _rootBlock.Release();
        }

        public bool IsValid => _rootBlock.IsValid;
    }
}
