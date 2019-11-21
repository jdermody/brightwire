using System;
using System.Collections.Generic;
using System.Text;

namespace BrightData.Distance
{
    class CosineDistance
    {
        public static float Calculate(float[] v1, float[] v2)
        {
            double aa = 0, bb = 0, ab = 0;
            for (int i = 0, len = v1.Length; i < len; i++) {
                var a = v1[i];
                var b = v2[i];
                ab += a * b;
                aa += a * a;
                bb += b * b;
            }
            return Convert.ToSingle(1 - ab / (Math.Sqrt(aa) * Math.Sqrt(bb)));
        }
    }
}
