using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using BrightData.Cuda.CudaToolkit;
using BrightData.Cuda.CudaToolkit.Types;
using BrightData.LinearAlgebra;
using BrightData.LinearAlgebra.Segments;

namespace BrightData.Cuda
{
    /// <summary>
    /// Provides a GPU based linear algebra provider
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Creates a linear algebra provider that runs on the GPU
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cudaKernelPath">Path to .cubin or .ptx kernel file (defaults to .ptx file for forward compatibility)</param>
        public static CudaProvider CreateCudaProvider(this BrightDataContext context, string? cudaKernelPath = null)
        {
            if (cudaKernelPath != null && !File.Exists(cudaKernelPath))
                throw new FileNotFoundException($"Could not find cuda kernel at: {cudaKernelPath}");

            string? cudaDirectory = null;
            if (cudaKernelPath == null) {
                var assemblyLocation = AppContext.BaseDirectory;
                if (assemblyLocation == null)
                    throw new Exception("Unable to obtain current assembly location");

                cudaDirectory = Path.Combine(assemblyLocation, "cuda");
                if (!Directory.Exists(cudaDirectory) || !Directory.EnumerateFiles(cudaDirectory, "*.ptx").Any())
                    throw new Exception($"Could not find the default cuda kernel location ({cudaDirectory}). Are the ptx kernels in BrightData.Cuda/cuda set to 'Copy to Output Directory'?");
            }

            var ret = new CudaProvider(context, cudaKernelPath, cudaDirectory);
            return ret;
        }

        /// <summary>
        /// Use a CUDA linear algebra provider in this bright data context
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cudaKernelPath">Path to CUDA kernel (optional)</param>
        /// <returns></returns>
        public static CudaLinearAlgebraProvider UseCuda(this BrightDataContext context, string? cudaKernelPath = null)
        {
            var provider = CreateCudaProvider(context, cudaKernelPath);
            var ret = new CudaLinearAlgebraProvider(context, provider);
            var setLinearAlgebraProvider = (ISetLinearAlgebraProvider)context;
            setLinearAlgebraProvider.LinearAlgebraProvider = ret;
            setLinearAlgebraProvider.LinearAlgebraProviderFactory = () => new CudaLinearAlgebraProvider(context, provider);
            return ret;
        }

        /// <summary>
        /// Returns the CUDA linear algebra provider used by the bright data context (if any)
        /// </summary>
        /// <param name="lap"></param>
        /// <param name="cuda"></param>
        /// <returns></returns>
        public static bool IsCuda(this LinearAlgebraProvider lap, [NotNullWhen(true)]out CudaLinearAlgebraProvider? cuda)
        {
            if (lap.ProviderName == CudaLinearAlgebraProvider.Name) {
                cuda = (CudaLinearAlgebraProvider)lap;
                return true;
            }

            cuda = null;
            return false;
        }

        internal static void CheckResult(this CuResult result)
        {
            if (result != CuResult.Success)
                throw new CudaException(result);
        }

        internal static void CheckResult(this CuBlasStatus result)
        {
            if (result != CuBlasStatus.Success)
                throw new CudaBlasException(result);
        }

        internal static CudaDeviceVariable<float> GetDeviceVariable(this INumericSegment<float> segment) => GetDeviceMemoryPtr(segment).DeviceVariable;
        internal static IDeviceMemoryPtr GetDeviceMemoryPtr(this IReadOnlyNumericSegment<float> segment)
        {
            if (segment is not CudaTensorSegment cudaSegment) 
                throw new Exception("CUDA tensors can only be used with other CUDA tensors");
            if (!segment.IsValid)
                throw new Exception("CUDA tensor was not valid");
            return cudaSegment.DeviceMemory;
        }

        internal static CuDevicePtr GetDevicePointer(this IReadOnlyNumericSegment<float> segment) => GetDeviceMemoryPtr(segment).DevicePointer;

        internal static (IDeviceMemoryPtr Ptr, uint Stride) GetDeviceMemory(this IReadOnlyNumericSegment<float> segment)
        {
            var foundWrapper = false;
            uint offset = 0, stride = 0, size = uint.MaxValue;
            while (segment is MutableTensorSegmentWrapper<float> wrapper) {
                offset += wrapper.Offset;
                stride += wrapper.Stride;
                size = wrapper.Size;
                foundWrapper = true;
                segment = wrapper.UnderlyingSegment;
            }
            if (segment is CudaTensorSegment cudaSegment) {
                if (!segment.IsValid)
                    throw new Exception("CUDA tensor was not valid");

                var ptr = cudaSegment.DeviceMemory;
                if(offset > 0 || (foundWrapper && size != segment.Size))
                    ptr = ptr.Offset(offset, size);
                return (ptr, foundWrapper ? stride : 1);
            }

            throw new Exception("CUDA tensors can only be used with other CUDA tensors");
        }
    }
}
