using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Models.Output
{
    /// <summary>
    /// The minimum and maxmimum values of a sequence of floats
    /// </summary>
    public class MinMax
    {
        /// <summary>
        /// The minimum value
        /// </summary>
        public float Min { get; private set; }

        /// <summary>
        /// The maximum value
        /// </summary>
        public float Max { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public MinMax(float min, float max)
        {
            Min = min;
            Max = max;
        }

        /// <summary>
        /// Default empty item
        /// </summary>
        public static MinMax Empty { get; } = new MinMax(0f, 0f);
    }
}
