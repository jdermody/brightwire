using System;

namespace BrightData.Helper
{
    /// <summary>
    /// Helpers for double based math
    /// </summary>
    public static class DoubleMath
    {
        /// <summary>
        /// A number that is close to zero
        /// </summary>
        public const double AlmostZero = 1E-32f;

        /// <summary>
        /// True if the numbers are approximately equal
        /// </summary>
        /// <param name="value1">First value to compare</param>
        /// <param name="value2">Second value to compare</param>
        /// <param name="tolerance">How close to compare</param>
        /// <returns></returns>
        public static bool AreApproximatelyEqual(double value1, double value2, double tolerance = AlmostZero) => Math.Abs(value1 - value2) < tolerance;

        /// <summary>
        /// True if the numbers are approximately equal
        /// </summary>
        /// <param name="value1">First value to compare</param>
        /// <param name="value2">Second value to compare</param>
        /// <param name="tolerance">How close to compare</param>
        /// <returns></returns>
        public static bool AreApproximatelyEqual(double? value1, double? value2, double tolerance = AlmostZero) => value1.HasValue && value2.HasValue && AreApproximatelyEqual(value1.Value, value2.Value, tolerance);
    }
}
