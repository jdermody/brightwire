using BrightWire.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire
{
    /// <summary>
    /// Creates linear algebra providers
    /// </summary>
    public static partial class LinearAlgebraProvider
    {
        /// <summary>
        /// Creates a linear alebra provider that runs on the GPU
        /// </summary>
        /// <param name="stochastic">False to disable random number generation</param>
        public static ILinearAlgebraProvider CreateGPU(bool stochastic = true)
        {
            return new CudaProvider(stochastic);
        }
    }
}
