using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icbld.BrightWire.Helper
{
    public sealed class BoundMath
    {
        public const float TOO_SMALL = -1.0E20f;
        public const float TOO_BIG = 1.0E20f;

        private BoundMath() { }

        public static float Constrain(float d)
        {
            if (float.IsNaN(d))
                return 0;
            else if (d < TOO_SMALL || float.IsNegativeInfinity(d))
                return TOO_SMALL;
            else if (d > TOO_BIG || float.IsPositiveInfinity(d))
                return TOO_BIG;
            else
                return d;
        }

        public static float Exp(float d)
        {
            return Constrain((float)Math.Exp(d));
        }

        public static float Log(float d)
        {
            return Constrain((float)Math.Log(d));
        }

        public static float Pow(float x, float y)
        {
            return Constrain((float)Math.Pow(x, y));
        }
    }
}
