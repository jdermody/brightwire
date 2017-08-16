using System;

namespace BrightWire.LinearAlgebra.Helper
{
    /// <summary>
    /// Floating point helper that prevents values from getting too big or small
    /// </summary>
    public class BoundMath
    {
        /// <summary>
        /// Minimum value
        /// </summary>
        public const float TOO_SMALL = -1.0E20f;

        /// <summary>
        /// Maximum value
        /// </summary>
        public const float TOO_BIG = 1.0E20f;

        private BoundMath() { }

        /// <summary>
        /// Forces the value to lie within the valid range
        /// </summary>
        /// <param name="val">Value to check</param>
        public static float Constrain(float val)
        {
            if (float.IsNaN(val))
                return 0;
            else if (val < TOO_SMALL || float.IsNegativeInfinity(val))
                return TOO_SMALL;
            else if (val > TOO_BIG || float.IsPositiveInfinity(val))
                return TOO_BIG;
            else
                return val;
        }

        /// <summary>
        /// Bounded exponent
        /// </summary>
        /// <param name="val"></param>
        public static float Exp(float val)
        {
            return Constrain((float)Math.Exp(val));
        }

        /// <summary>
        /// Bounded natural log
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static float Log(float d)
        {
            return Constrain((float)Math.Log(d));
        }

        /// <summary>
        /// Bounded power
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static float Pow(float x, float y)
        {
            return Constrain((float)Math.Pow(x, y));
        }
    }
}
