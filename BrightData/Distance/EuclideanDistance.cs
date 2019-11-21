using System;
using System.Collections.Generic;
using System.Text;
using BrightData.Helper;

namespace BrightData.Distance
{
    class EuclideanDistance
    {
        public static float Calculate(float[] v1, float[] v2)
        {
            float ret = 0f;
            for (int i = 0, len = v1.Length; i < len; i++) {
                var distance = v1[i] - v2[i];
                ret += distance * distance;
            }
            return FloatMath.Sqrt(ret);
        }
    }
}
