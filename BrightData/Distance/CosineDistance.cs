using System;

namespace BrightData.Distance
{
    internal class CosineDistance
    {
        public static float Calculate(float[] v1, float[] v2)
        {
            float aa = 0, bb = 0, ab = 0;
            for (int i = 0, len = v1.Length; i < len; i++) {
                var a = v1[i];
                var b = v2[i];
                ab += a * b;
                aa += a * a;
                bb += b * b;
            }
            return 1f - ab / (MathF.Sqrt(aa) * MathF.Sqrt(bb));
        }
    }
}
