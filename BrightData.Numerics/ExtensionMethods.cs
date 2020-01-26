using System;
using System.Collections.Generic;
using System.Text;

namespace BrightData.Numerics
{
    public static class ExtensionMethods
    {
        public static void UseNumericsComputation(this IBrightDataContext context)
        {
            context.ComputableFactory = new NumericsComputableFactory(context);
        }
    }
}
