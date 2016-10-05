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
        /// Creates a linear algebra provider that runs on the CPU
        /// </summary>
        /// <param name="stochastic">False to disable random number generation</param>
        public static ILinearAlgebraProvider CreateCPU(bool stochastic = true)
        {
            return new NumericsProvider(stochastic);
        }
    }
}
