using System;
using BrightData.Helper;

namespace BrightData.Distribution
{
    internal class NormalDistribution : IContinuousDistribution
    {
        readonly IBrightDataContext _context;

        public NormalDistribution(IBrightDataContext context, float mean = 0f, float stdDev = 1f)
        {
            _context = context;
            Mean = mean;
            StdDev = stdDev;
        }

        public float Mean { get; }
        public float StdDev { get; }

        public float Sample()
        {
            float x, y;
            while (!PolarTransform(_context.NextRandomFloat(), _context.NextRandomFloat(), out x, out y)) {
                // nop
            }

            return Mean + (StdDev * x);
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
