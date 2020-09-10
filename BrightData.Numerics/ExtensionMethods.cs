using System;
using System.Collections.Generic;
using System.Text;
using BrightWire.LinearAlgebra;

namespace BrightData.Numerics
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Creates a linear algebra provider that runs on the CPU
        /// </summary>
        /// <param name="context"></param>
        /// <param name="stochastic">False to use the same random number generation each time</param>
        public static ILinearAlgebraProvider CreateLinearAlgebra(this IBrightDataContext context, bool stochastic = true)
        {
            var ret = new CpuProvider(context, stochastic);
            ((ISetLinearAlgebraProvider) context).LinearAlgebraProvider = ret;
            return ret;
        }
    }
}
