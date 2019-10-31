using System;
using System.Collections.Generic;
using System.Text;

namespace BrightData.Helper
{
    public static class FloatMath
    {
        public const float ALMOST_ZERO = 1E-08f;
        public const float TOO_SMALL = -1.0E20f;
        public const float TOO_BIG = 1.0E20f;

        public static float Log(float val) => Constrain(MathF.Log(val));
        public static float Exp(float val) => Constrain(MathF.Exp(val));
        public static float Sqrt(float val, float adjustment = ALMOST_ZERO) => Constrain(MathF.Sqrt(val + adjustment));
        public static float Pow(float x, float y) => Constrain(MathF.Pow(x, y));
        public static bool IsZero(float value) => MathF.Abs(value) < ALMOST_ZERO;
        public static bool IsNotZero(float value) => !IsZero(value);
        public static bool AreEqual(float value1, float value2, float tolerance = ALMOST_ZERO) => MathF.Abs(value1 - value2) < tolerance;
        public static float Constrain(double val) => Constrain((float) val);
        public static float Constrain(float val)
        {
            if (float.IsNaN(val))
                return 0;
            if (val < TOO_SMALL || float.IsNegativeInfinity(val))
                return TOO_SMALL;
            if (val > TOO_BIG || float.IsPositiveInfinity(val))
                return TOO_BIG;
            return val;
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
        public static IEqualityComparer<float> GetEqualityComparer(float tolerance = ALMOST_ZERO) => new EqualityComparer(tolerance);
    }
}
