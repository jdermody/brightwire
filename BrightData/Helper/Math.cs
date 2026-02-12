using System;
using System.Numerics;

namespace BrightData.Helper
{
    /// <summary>
    /// Constrained typed math helpers - if the value is too small or too big it will be capped. Also, NaN values are replaced with zero.
    /// </summary>
    public static class Math<T>
        where T: unmanaged, IBinaryFloatingPointIeee754<T>
    {
#pragma warning disable 1591
        public static readonly T AlmostZero = T.CreateSaturating(1E-08f);
        public static readonly T TooSmall   = T.CreateSaturating(-1.0E20f);
        public static readonly T TooBig     = T.CreateSaturating(1.0E20f);
        public static readonly T Two        = T.One + T.One;

        public static T Log(T val) => Constrain(T.Log(val));
        public static T Exp(T val) => Constrain(T.Exp(val));
        public static T Sqrt(T val, T? adjustment = null) => Constrain(T.Sqrt(val + (adjustment ?? AlmostZero)));
        public static T Pow(T x, T y) => Constrain(T.Pow(x, y));
        public static bool IsZero(T value) => T.Abs(value) < AlmostZero;
        public static bool IsNotZero(T value) => !IsZero(value);
        public static bool AreEqual(T value1, T value2, T? tolerance = null) => T.Abs(value1 - value2) < (tolerance ?? AlmostZero);
        public static T Constrain(double val) => Constrain(T.CreateSaturating(val));
        public static T Constrain(T val)
        {
            if (T.IsNaN(val))
                return T.Zero;
            if (val < TooSmall || T.IsNegativeInfinity(val))
                return TooSmall;
            if (val > TooBig || T.IsPositiveInfinity(val))
                return TooBig;
            return val;
        }

        public static T Next(Random rand) => Constrain(T.CreateSaturating(rand.NextDouble()));

        public static T Max(T val1, T val2) => val1 > val2 ? val1 : val2;
        public static T Abs(T val) => T.Abs(val);
        public static T Sign(T val) => T.CreateSaturating(T.Sign(val));

        //class EqualityComparer(T tolerance) : IEqualityComparer<T>
        //{
        //    public bool Equals(T x, T y)
        //    {
        //        return T.Abs(x - y) < tolerance;
        //    }

        //    public int GetHashCode(T obj)
        //    {
        //        return obj.GetHashCode();
        //    }
        //}
        //public static IEqualityComparer<T> GetEqualityComparer(T? tolerance = null) => new EqualityComparer<T>(tolerance ?? AlmostZero);

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

        public static bool AreApproximatelyEqual<TT>(TT t1, TT t2, int maxDifference = 6)
            where TT: IReadOnlyNumericSegment<float>
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

        /// <summary>
        /// A number that is close to zero
        /// </summary>
        public const double AlmostZeroDouble = 1E-32f;

        /// <summary>
        /// True if the numbers are approximately equal
        /// </summary>
        /// <param name="value1">First value to compare</param>
        /// <param name="value2">Second value to compare</param>
        /// <param name="tolerance">How close to compare</param>
        /// <returns></returns>
        public static bool AreApproximatelyEqual(double value1, double value2, double tolerance = AlmostZeroDouble) => Math.Abs(value1 - value2) < tolerance;

        /// <summary>
        /// True if the numbers are approximately equal
        /// </summary>
        /// <param name="value1">First value to compare</param>
        /// <param name="value2">Second value to compare</param>
        /// <param name="tolerance">How close to compare</param>
        /// <returns></returns>
        public static bool AreApproximatelyEqual(double? value1, double? value2, double tolerance = AlmostZeroDouble) => value1.HasValue && value2.HasValue && AreApproximatelyEqual(value1.Value, value2.Value, tolerance);
    }
}
