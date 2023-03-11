
using BrightData.Cuda.CudaToolkit;
using BrightData.Cuda.CudaToolkit.Types;

namespace BrightData.Cuda.Helper
{
    /// <summary>
	/// Maintains a cache of available device memory
	/// </summary>
    internal class DeviceMemoryBlock : DeviceMemoryBlockBase
    {
        readonly MemoryPool? _memoryPool;

        /// <inheritdoc />
        public DeviceMemoryBlock(uint size) : base(Create(size))
        {
        }

        static CudaDeviceVariable<float> Create(uint size)
        {
            var sizeInBytes = size * CudaProvider.FloatSize;
            var ptr = new CuDevicePtr();
            var result = DriverApiNativeMethods.MemoryManagement.cuMemAlloc_v2(ref ptr, sizeInBytes);
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
