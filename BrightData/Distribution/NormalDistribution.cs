using System;
using BrightData.Helper;

namespace BrightData.Distribution
{
    /// <summary>
    /// Normal distribution - https://en.wikipedia.org/wiki/Normal_distribution
    /// </summary>
    /// <param name="context"></param>
    /// <param name="mean"></param>
    /// <param name="stdDev"></param>
    internal class NormalDistribution(BrightDataContext context, float mean = 0f, float stdDev = 1f)
        : IContinuousDistribution
    {
        public float Mean { get; } = mean;
        public float StdDev { get; } = stdDev;

        public float Sample()
        {
            float x;
            while (!PolarTransform(context.NextRandomFloat(), context.NextRandomFloat(), out x, out _)) {
                // nop
            }

            return Mean + StdDev * x;
        }

        static bool PolarTransform(float a, float b, out float x, out float y)
        {
            var v1 = (2.0f * a) - 1.0f;
            var v2 = (2.0f * b) - 1.0f;
            var r = (v1 * v1) + (v2 * v2);
            if (r >= 1.0 || MathF.Abs(r) < FloatMath.AlmostZero) {
                x = 0;
                y = 0;
                return false;
            }

            var fac = MathF.Sqrt(-2.0f * MathF.Log(r) / r);
            x = v1 * fac;
            y = v2 * fac;
            return true;
        }
    }
}
