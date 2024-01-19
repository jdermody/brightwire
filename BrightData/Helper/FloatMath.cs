using System;
using System.Collections.Generic;

namespace BrightData.Helper
{
    /// <summary>
    /// Constrained float math helpers - if the value is too small or too big it will be capped. Also, NaN values are replaced with zero.
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

        class EqualityComparer(float tolerance) : IEqualityComparer<float>
        {
            public bool Equals(float x, float y)
            {
                return Math.Abs(Math.Abs(x) - Math.Abs(y)) < tolerance;
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
            where T: IReadOnlyNumericSegment<float>
        {
            var len = t1.Size;
            if (len != t2.Size)
                return false;

            using var a1 = t1.Values.GetEnumerator();
            using var a2 = t2.Values.GetEnumerator();
            while(a1.MoveNext() && a2.MoveNext()) {
                if (!AreApproximatelyEqual(a1.Current, a2.Current, maxDifference))
                    return false;
            }
            return true;
        }

        public static bool AreApproximatelyEqual(float[] t1, float[] t2, int maxDifference = 6)
        {
            var len = t1.Length;
            if (len != t2.Length)
                return false;

            for (var i = 0; i < len; i++) {
                if (!AreApproximatelyEqual(t1[i], t2[i], maxDifference))
                    return false;
            }

            return true;
        }

        public static bool AreApproximatelyEqual(INumericSegment<float> t1, INumericSegment<float> t2, int maxDifference = 6)
        {
            var len = t1.Size;
            if (len != t2.Size)
                return false;

            for (var i = 0; i < len; i++) {
                if (!AreApproximatelyEqual(t1[i], t2[i], maxDifference))
                    return false;
            }

            return true;
        }

        public static bool AreApproximatelyEqual(IHaveReadOnlyTensorSegment<float> t1, IHaveReadOnlyTensorSegment<float> t2, int maxDifference = 6) => AreApproximatelyEqual(t1.ReadOnlySegment, t2.ReadOnlySegment, maxDifference);
#pragma warning restore 1591
    }
}
