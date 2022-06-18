using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData.LinearAlegbra2;

namespace BrightData.MKL
{
    public static class ExtensionMethods
    {
        public static LinearAlgebraProvider UseMKL(this BrightDataContext context)
        {
            var ret = new MklLinearAlgebraProvider(context);;
            ((ISetLinearAlgebraProvider) context).LinearAlgebraProvider2 = ret;
            return ret;
        }
    }
}
