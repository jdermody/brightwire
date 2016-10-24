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
    public static partial class Provider
    {
        /// <summary>
        /// Creates a linear alebra provider that runs on the GPU
        /// </summary>
        /// <param name="stochastic">False to disable random number generation</param>
        /// <param name="cudaKernelPath">Path to .cubin or .ptx kernel file (defaults to .ptx file for forward compatability)</param>
        public static ILinearAlgebraProvider CreateGPULinearAlgebra(bool stochastic = true, string cudaKernelPath = null)
        {
            var path = cudaKernelPath ?? GetCudaKernelPath(true);
            if (!File.Exists(path))
                throw new FileNotFoundException("Could not find cuda kernel at: " + path);
            return new CudaProvider(cudaKernelPath ?? GetCudaKernelPath(true), stochastic);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="usePtx"></param>
        /// <returns></returns>
        public static string GetCudaKernelPath(bool usePtx)
        {
            var assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return assemblyLocation + @"\LinearAlgebra\cuda\kernel." + (usePtx ? "ptx" : "cubin");
        }
    }
}
