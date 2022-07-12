using System;
using System.Collections.Generic;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.Helper
{
    /// <summary>
    /// Constrained float math helpers - if the value is too small or too big it will be capped. Also NaN values are replaced with zero.
    /// </summary>
    public static class FloatMath
    {
#pragma warning disable 1591
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

        public static unsafe int FloatToInt32Bits(float f)
        {
            return *((int*)&f);
        }

        public static bool AlmostEqual2SComplement(float a, float b, int maxDeltaBits)
        {
            var aInt = FloatToInt32Bits(a);
            if (aInt < 0)
                aInt = Int32.MinValue - aInt;

            var bInt = FloatToInt32Bits(b);
            if (bInt < 0)
                bInt = Int32.MinValue - bInt;

            var intDiff = Math.Abs(aInt - bInt);
            return intDiff <= (1 << maxDeltaBits);
        }

        public static bool AreApproximatelyEqual(float v1, float v2, int maxDifference = 6)
        {
            if (float.IsNaN(v1) && float.IsNaN(v2))
                return true;
            return AlmostEqual2SComplement(v1, v2, maxDifference);
        }

        public static bool AreApproximatelyEqual<T>(T t1, T t2, int maxDifference = 6)
            where T: IHaveSize, IHaveSpan
        {
            var len = t1.Size;
            if (len != t2.Size)
                return false;

            SpanOwner<float> temp1 = SpanOwner<float>.Empty, temp2 = SpanOwner<float>.Empty;
            var p1 = t1.GetSpan(ref temp1, out var wasTemp1Used);
            var p2 = t2.GetSpan(ref temp2, out var wasTemp2Used);

            try {
                for (var i = 0; i < len; i++) {
                    if (!AreApproximatelyEqual(p1[i], p2[i], maxDifference))
                        return false;
                }

                return true;
            }
            finally {
                if(wasTemp1Used)
                    temp1.Dispose();
                if(wasTemp2Used)
                    temp2.Dispose();
            }
        }
#pragma warning restore 1591
    }
}
