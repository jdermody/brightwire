using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Models.Simple
{
    public class MinMax
    {
        public float Min { get; private set; }
        public float Max { get; private set; }

        public MinMax(float min, float max)
        {
            Min = min;
            Max = max;
        }

        public static MinMax Empty { get; } = new MinMax(0f, 0f);
    }
}
