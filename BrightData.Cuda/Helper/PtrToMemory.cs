using System.Threading;
using ManagedCuda;
using ManagedCuda.BasicTypes;

namespace BrightData.Cuda.Helper
{
    /// <summary>
    /// A pointer to a block of device memory (the block is owned by another pointer)
    /// </summary>
    internal class PtrToMemory : IDeviceMemoryPtr
    {
	    readonly IDeviceMemoryPtr _rootBlock;
        readonly CudaDeviceVariable<float> _ptr;
        readonly CudaContext _context;
	    int _refCount = 1;

        public PtrToMemory(CudaContext context, IDeviceMemoryPtr rootBlock, CUdeviceptr ptr, SizeT size)
        {
            _context = context;
            _ptr = new CudaDeviceVariable<float>(ptr, size);
	        _rootBlock = rootBlock;
	        rootBlock.AddRef();
        }

        public CudaDeviceVariable<float> DeviceVariable => _ptr;
        public CUdeviceptr DevicePointer => _ptr.DevicePointer;
        public uint Size => _ptr.Size;

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

        public void CopyToHost(float[] target)
        {
            _context.CopyToHost<float>(target, _ptr.DevicePointer);
        }

	    public int AddRef()
	    {
		    return Interlocked.Increment(ref _refCount) + _rootBlock.AddRef();
	    }

	    public void Free()
        {
	        _rootBlock.Free();
			if(Interlocked.Decrement(ref _refCount) <= 0)
				_ptr.Dispose();
        }
    }
}
