using System;
using System.Collections.Generic;

namespace BrightData.Helper
{
    /// <summary>
    /// Constrained float math helpers - if the value is too small or too big it will be capped. Also NaN values are replaced with zero.
    /// </summary>
    public static class FloatMath
    {
        public const float AlmostZero = 1E-08f;
        public const float TooSmall = -1.0E20f;
        public const float TooBig = 1.0E20f;

        public static float Log(float val) => Constrain(MathF.Log(val));
        public static float Exp(float val) => Constrain(MathF.Exp(val));
        public static float Sqrt(float val, float adjustment = AlmostZero) => Constrain(MathF.Sqrt(val + adjustment));
        public static float Pow(float x, float y) => Constrain(MathF.Pow(x, y));
        public static bool IsZero(float value) => MathF.Abs(value) < AlmostZero;
        public static bool IsNotZero(float value) => !IsZero(value);
        public static bool AreEqual(float value1, float value2, float tolerance = AlmostZero) => MathF.Abs(value1 - value2) < tolerance;
        public static float Constrain(double val) => Constrain((float) val);
        public static float Constrain(float val)
        {
            if (float.IsNaN(val))
                return 0;
            if (val < TooSmall || float.IsNegativeInfinity(val))
                return TooSmall;
            if (val > TooBig || float.IsPositiveInfinity(val))
                return TooBig;
            return val;
        }

        public static float Next(Random rand) => Constrain(rand.NextDouble());

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
        public static IEqualityComparer<float> GetEqualityComparer(float tolerance = AlmostZero) => new EqualityComparer(tolerance);
    }
}
