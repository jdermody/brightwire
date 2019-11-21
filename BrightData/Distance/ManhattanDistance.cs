using System;
using System.Collections.Generic;
using System.Text;

namespace BrightData.Distance
{
    class ManhattanDistance
    {
        public static float Calculate(float[] v1, float[] v2)
        {
            float ret = 0f;
            for (int i = 0, len = v1.Length; i < len; i++)
                ret += Math.Abs(v1[i] - v2[1]);
            return ret;
        }
    }
}
