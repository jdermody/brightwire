using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.Distribution
{
    internal class ExponentialDistribution(BrightDataContext context, float lambda) : IContinuousDistribution
    {
        public float Sample()
        {
            float r;
            do {
                r = context.NextRandomFloat();
            } while (r == 0f);
            return -MathF.Log(r) / lambda;
        }
    }
}
