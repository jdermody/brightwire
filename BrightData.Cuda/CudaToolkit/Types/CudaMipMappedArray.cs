using System;

namespace BrightData.Cuda.CudaToolkit.Types
{
    internal class CudaMipMappedArray : IDisposable
    {
        CuMipMappedArray _mipMappedArray;
        readonly CudaArray3DDescriptor _arrayDescriptor;
        CuResult _res;
        bool _disposed;
        readonly bool _isOwner;

        public CudaMipMappedArray(CudaArray3DDescriptor descriptor, uint numMipmapLevels)
        {
            _mipMappedArray = new CuMipMappedArray();
            _arrayDescriptor = descriptor;

            _res = DriverApiNativeMethods.ArrayManagement.cuMipmappedArrayCreate(ref _mipMappedArray, ref _arrayDescriptor, numMipmapLevels);
            
            if (_res != CuResult.Success) throw new CudaException(_res);
            _isOwner = true;
        }
        public CudaMipMappedArray(CuArrayFormat format, SizeT width, SizeT height, SizeT depth, CudaMipMappedArrayNumChannels numChannels, CudaArray3DFlags flags, uint numMipmapLevels)
        {
            _mipMappedArray = new CuMipMappedArray();
            _arrayDescriptor = new CudaArray3DDescriptor {
                Width = width,
                Height = height,
                Depth = depth,
                NumChannels = (uint)numChannels,
                Flags = flags,
                Format = format
            };

            _res = DriverApiNativeMethods.ArrayManagement.cuMipmappedArrayCreate(ref _mipMappedArray, ref _arrayDescriptor, numMipmapLevels);
            if (_res != CuResult.Success) throw new CudaException(_res);
            _isOwner = true;
        }
        public CudaMipMappedArray(CuMipMappedArray handle, CuArrayFormat format, CudaMipMappedArrayNumChannels numChannels)
        {
            _mipMappedArray = handle;
            _arrayDescriptor = new CudaArray3DDescriptor {
                Format = format,
                NumChannels = (uint)numChannels
            };
            _isOwner = false;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool fDisposing)
        {
            if (fDisposing && !_disposed) {
                if (_isOwner) {
                    _res = DriverApiNativeMethods.ArrayManagement.cuMipmappedArrayDestroy(_mipMappedArray);
                }
                _disposed = true;
            }
        }
        public CuArray GetLevelAsCuArray(uint level)
        {
            var array = new CuArray();

            _res = DriverApiNativeMethods.ArrayManagement.cuMipmappedArrayGetLevel(ref array, _mipMappedArray, level);
            if (_res != CuResult.Success)
                throw new CudaException(_res);

            return array;
        }
        public CudaArraySparseProperties GetSparseProperties()
        {
            var sparseProperties = new CudaArraySparseProperties();

            _res = DriverApiNativeMethods.ArrayManagement.cuMipmappedArrayGetSparseProperties(ref sparseProperties, _mipMappedArray);
            if (_res != CuResult.Success)
                throw new CudaException(_res);

            return sparseProperties;
        }
        public CudaArrayMemoryRequirements GetMemoryRequirements(CuDevice device)
        {
            return _mipMappedArray.GetMemoryRequirements(device);
        }
        public CuMipMappedArray CuMipMappedArray => _mipMappedArray;
        public CudaArray3DDescriptor Array3DDescriptor => _arrayDescriptor;
        public SizeT Depth => _arrayDescriptor.Depth;
        public SizeT Height => _arrayDescriptor.Height;
        public SizeT Width => _arrayDescriptor.Width;
        public CudaArray3DFlags Flags => _arrayDescriptor.Flags;
        public CuArrayFormat Format => _arrayDescriptor.Format;
        public uint NumChannels => _arrayDescriptor.NumChannels;
        public bool IsOwner => _isOwner;
    }
}
