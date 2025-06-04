using System.Numerics;
using BrightData.Helper;

namespace BrightData.Distribution
{
    /// <summary>
    /// Normal distribution - https://en.wikipedia.org/wiki/Normal_distribution
    /// </summary>
    /// <param name="context"></param>
    /// <param name="mean"></param>
    /// <param name="stdDev"></param>
    internal class NormalDistribution<T>(BrightDataContext context, T? mean = null, T? stdDev = null) : IContinuousDistribution<T>
        where T: unmanaged, INumber<T>, IBinaryFloatingPointIeee754<T>
    {
        public T Mean { get; } = mean ?? T.Zero;
        public T StdDev { get; } = stdDev ?? T.One;

        public T Sample()
        {
            T x;
            while (!PolarTransform(context.NextRandom<T>(), context.NextRandom<T>(), out x, out _)) {
                // nop
            }

            return Mean + StdDev * x;
        }

        static bool PolarTransform(T a, T b, out T x, out T y)
        {
            var two = T.One + T.One;
            var v1 = (two * a) - T.One;
            var v2 = (two * b) - T.One;
            var r = (v1 * v1) + (v2 * v2);
            if (r >= T.One || T.Abs(r) < Math<T>.AlmostZero) {
                x = T.Zero;
                y = T.Zero;
                return false;
            }

            var fac = T.Sqrt(-two * T.Log(r) / r);
            x = v1 * fac;
            y = v2 * fac;
            return true;
        }
    }
}
