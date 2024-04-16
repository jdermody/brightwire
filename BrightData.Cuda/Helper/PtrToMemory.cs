using System;
using BrightData.Cuda.CudaToolkit.Types;

namespace BrightData.Cuda.Helper
{
    /// <summary>
    /// A pointer to a block of device memory (the block is owned by another pointer)
    /// </summary>
    internal class PtrToMemory(IDeviceMemoryPtr rootBlock, CuDevicePtr ptr, SizeT size) : IDeviceMemoryPtr
    {
        readonly CudaDeviceVariable<float> _ptr = new(ptr, false, size);
        bool                               _addedReference = false;

        public void Dispose()
        {
            if(_addedReference)
                Release();
        }

        public CudaDeviceVariable<float> DeviceVariable => _ptr;
        public CuDevicePtr DevicePointer => _ptr.DevicePointer;
        public uint Size => _ptr.Size;

        public void CopyToHost(ArraySegment<float> target) => _ptr.CopyToHost(target.Array!, 0, target.Offset, target.Count * sizeof(float));

        public void Clear()
        {
            _ptr.Memset(0);
        }

        public IDeviceMemoryPtr Offset(uint offsetInFloats, uint size)
        {
            var offsetPtr = _ptr.DevicePointer.Pointer + (offsetInFloats * sizeof(float));
            return new PtrToMemory(rootBlock, new CuDevicePtr(offsetPtr), size * sizeof(float));
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
            fixed (float* p = span)                                                                             
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
            _ptr.CopyToHost(target);
        }

	    public int AddRef()
        {
            _addedReference = true;
		    return rootBlock.AddRef();
	    }

	    public int Release()
        {
	        return rootBlock.Release();
        }

        public bool IsValid => rootBlock.IsValid;
    }
}
