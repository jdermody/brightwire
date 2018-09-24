using System;
using System.Collections.Generic;

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

		/// <summary>
		/// Tolerance for comparison to zero
		/// </summary>
	    public const float ZERO_LIKE = 0.000000001f;

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

		/// <summary>
		/// Returns true if the value approximates zero
		/// </summary>
		/// <param name="value">Value to test</param>
	    public static bool IsZero(float value)
	    {
		    return Math.Abs(value) < ZERO_LIKE;
	    }

		/// <summary>
		/// Returns true if the value is greater than zero
		/// </summary>
		/// <param name="value">Value to test</param>
		/// <returns></returns>
	    public static bool IsNotZero(float value)
	    {
		    return !IsZero(value);
	    }

	    /// <summary>
	    /// Checks if the two floating point numbers are equal (with a degree of tolerance)
	    /// </summary>
	    /// <param name="value1">First value to compare</param>
	    /// <param name="value2">Second value to compare</param>
	    /// <param name="tolerance">Tolerance allowed between the numbers</param>
	    /// <returns></returns>
	    public static bool AreEqual(float value1, float value2, float tolerance = ZERO_LIKE)
	    {
		    return Math.Abs(value1 - value2) < tolerance;
	    }

	    class EqualityComparer : IEqualityComparer<float>
	    {
		    readonly float _tolerance;

		    public EqualityComparer(float tolerance)
		    {
			    _tolerance = tolerance;
		    }

		    public bool Equals(float x, float y)
		    {
			    return Math.Abs(Math.Abs(x) - Math.Abs(y)) < _tolerance;
		    }

		    public int GetHashCode(float obj)
		    {
			    return obj.GetHashCode();
		    }
	    }

	    /// <summary>
	    /// Used for comparing floating point numbers (if they are within the tolerance they are considered equal)
	    /// </summary>
	    /// <param name="tolerance">Tolerance to consider if two floating point numbers are the same</param>
	    /// <returns></returns>
	    public static IEqualityComparer<float> GetEqualityComparer(float tolerance = ZERO_LIKE) => new EqualityComparer(tolerance);
    }
}
