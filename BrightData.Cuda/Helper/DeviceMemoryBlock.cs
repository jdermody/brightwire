
using BrightData.Cuda.CudaToolkit;

namespace BrightData.Cuda.Helper
{
    /// <summary>
	/// Maintains a cache of available device memory
	/// </summary>
    public class DeviceMemoryBlock : DeviceMemoryBlockBase
    {
        readonly MemoryPool? _memoryPool = null;

        /// <inheritdoc />
        public DeviceMemoryBlock(uint size) : base(Create(size))
        {
        }

        static CudaDeviceVariable<float> Create(uint size)
        {
            var sizeInBytes = size * CudaProvider.FloatSize;
            var ptr = new CUdeviceptr();
            var result = DriverAPINativeMethods.MemoryManagement.cuMemAlloc_v2(ref ptr, sizeInBytes);
            CudaProvider.CheckForError(result);
            return new CudaDeviceVariable<float>(ptr, true, sizeInBytes);
        }

        /// <inheritdoc />
        public DeviceMemoryBlock(MemoryPool? memoryPool, CudaDeviceVariable<float> data) : base(data)
        {
            _data = data;
            _memoryPool = memoryPool;
        }

        /// <inheritdoc />
        protected override void OnDispose()
        {
            _memoryPool?.Recycle(_data.SizeInBytes, _data.DevicePointer);
            _data.Dispose();
        }
    }
}
