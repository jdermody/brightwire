using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace BrightData.Cuda
{
    /// <summary>
    /// Provides a GPU based linear algebra provider
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Creates a linear alebra provider that runs on the GPU
        /// </summary>
        /// <param name="context"></param>
        /// <param name="memoryCacheSize">The amount of device memory to use an application memory cache</param>
        /// <param name="cudaKernelPath">Path to .cubin or .ptx kernel file (defaults to .ptx file for forward compatability)</param>
        public static CudaProvider CreateCudaProvider(this BrightDataContext context, string? cudaKernelPath = null, uint? memoryCacheSize = null)
        {
            if (cudaKernelPath != null && !File.Exists(cudaKernelPath))
                throw new FileNotFoundException($"Could not find cuda kernel at: {cudaKernelPath}");

            string? cudaDirectory = null;
            if (cudaKernelPath == null) {
                var assemblyLocation = Path.GetDirectoryName((Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).Location);
                if (assemblyLocation == null)
                    throw new Exception("Unable to obtain current assembly location");

                cudaDirectory = Path.Combine(assemblyLocation, "cuda");
                if (!Directory.Exists(cudaDirectory) || !Directory.EnumerateFiles(cudaDirectory, "*.ptx").Any())
                    throw new Exception($"Could not find the default cuda kernel location ({cudaDirectory}). Are the ptx kernels in BrightData.Cuda/cuda set to 'Copy to Output Directory'?");
            }

            var ret = new CudaProvider(context, cudaKernelPath, cudaDirectory, memoryCacheSize);
            return ret;
        }

        public static CudaLinearAlgebraProvider UseCuda(this BrightDataContext context, string? cudaKernelPath = null, uint? cudaMemoryCacheSize = null)
        {
            var provider = CreateCudaProvider(context, cudaKernelPath, cudaMemoryCacheSize);
            var ret = new CudaLinearAlgebraProvider(context, provider);
            ((ISetLinearAlgebraProvider) context).LinearAlgebraProvider = ret;
            return ret;
        }
    }
}
