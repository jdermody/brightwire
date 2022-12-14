using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData.LinearAlgebra;

namespace BrightData.MKL
{
    /// <summary>
    /// MKL Extension methods
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Configures the bright data context to use MKL linear algebra provider
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static MklLinearAlgebraProvider UseMkl(this BrightDataContext context)
        {
            var ret = new MklLinearAlgebraProvider(context);
            ((ISetLinearAlgebraProvider) context).LinearAlgebraProvider = ret;
            return ret;
        }
    }
}
