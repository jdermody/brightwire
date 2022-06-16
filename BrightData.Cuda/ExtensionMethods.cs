using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Toolkit.HighPerformance;
using Microsoft.Toolkit.HighPerformance.Buffers;

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
        public static ILinearAlgebraProvider UseCudaLinearAlgebra(this IBrightDataContext context, string? cudaKernelPath = null, uint memoryCacheSize = 512 * 1048576)
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
            ((ISetLinearAlgebraProvider)context).LinearAlgebraProvider = ret;
            return ret;
        }

        public static unsafe void CopyToDevice(this IDeviceMemoryPtr ptr, ReadOnlySpan<float> span, float[]? sourceArray)
        {
            if (sourceArray is null) {
                //using var buffer = SpanOwner<float>.Allocate((int)ptr.Size);
                //span.CopyTo(buffer.Span);
                fixed (float* p = &MemoryMarshal.GetReference(span))
                {
                    ptr.DeviceVariable.CopyToDevice((IntPtr)p, 0, 0, (int)ptr.Size * sizeof(float));
                }
            }
            else {
                fixed (float* p = sourceArray)
                {
                    ptr.DeviceVariable.CopyToDevice((IntPtr)p, 0, 0, (int)ptr.Size * sizeof(float));
                }
            }
        }
    }
}
