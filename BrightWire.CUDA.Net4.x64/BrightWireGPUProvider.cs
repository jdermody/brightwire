using BrightWire.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire
{
    /// <summary>
    /// Provides a GPU based linear algebra provider
    /// </summary>
    public static class BrightWireGpuProvider
    {
        /// <summary>
        /// Creates a linear alebra provider that runs on the GPU
        /// </summary>
        /// <param name="stochastic">False to disable random number generation</param>
        /// <param name="cudaKernelPath">Path to .cubin or .ptx kernel file (defaults to .ptx file for forward compatability)</param>
        public static ILinearAlgebraProvider CreateLinearAlgebra(bool stochastic = true, string cudaKernelPath = null)
        {
            var path = cudaKernelPath ?? GetKernelPath();
            if (!File.Exists(path))
                throw new FileNotFoundException($"Could not find cuda kernel at: {path}. Is the \\LinearAlgebra\\cuda\\kernel.ptx file set to 'Copy to Output Directory'?");
            return new CudaProvider(path, stochastic);
        }

        /// <summary>
        /// Returns the default cuda kernel path
        /// </summary>
        /// <returns></returns>
        public static string GetKernelPath()
        {
            var assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            
            // This is the universal separator, returns '/' on Linux, '\' on Windows, although Windows doesn't really care
            var s = Path.DirectorySeparatorChar;

            return assemblyLocation + $"{s}cuda{s}kernel.ptx";        
        }
    }
}
