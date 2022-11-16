using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData.LinearAlgebra;

namespace BrightData.MKL
{
    public static class ExtensionMethods
    {
        public static MklLinearAlgebraProvider UseMkl(this BrightDataContext context)
        {
            var ret = new MklLinearAlgebraProvider(context);
            ((ISetLinearAlgebraProvider) context).LinearAlgebraProvider = ret;
            return ret;
        }
    }
}
