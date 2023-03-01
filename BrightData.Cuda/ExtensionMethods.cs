using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using BrightData.Cuda.CudaToolkit;
using BrightData.LinearAlgebra;

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
        /// <param name="cudaKernelPath">Path to .cubin or .ptx kernel file (defaults to .ptx file for forward compatability)</param>
        public static CudaProvider CreateCudaProvider(this BrightDataContext context, string? cudaKernelPath = null)
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
            ((ISetLinearAlgebraProvider) context).LinearAlgebraProvider = ret;
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

        internal static void CheckResult(this CUResult result)
        {
            if (result != CUResult.Success)
                throw new CudaException(result);
        }

        internal static void CheckResult(this CublasStatus result)
        {
            if (result != CublasStatus.Success)
                throw new CudaBlasException(result);
        }
    }
}
