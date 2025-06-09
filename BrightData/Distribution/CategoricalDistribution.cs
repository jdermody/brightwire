using System.Collections.Generic;
using System.Linq;

namespace BrightData.Distribution
{
    /// <summary>
    /// Categorical distribution - https://en.wikipedia.org/wiki/Categorical_distribution
    /// </summary>
    internal class CategoricalDistribution : INonNegativeDiscreteDistribution
    {
        readonly BrightDataContext _context;

        public CategoricalDistribution(BrightDataContext context, IEnumerable<float> categoricalValues)
        {
            _context = context;

            float cumulativeTotal = 0;
            CumulativeValues = [.. categoricalValues.Select(v => cumulativeTotal += v)];
            CumulativeTotal = cumulativeTotal;
        }

        public float[] CumulativeValues { get; }

        public float CumulativeTotal { get; }

        public uint Sample()
        {
            var total = _context.NextRandomFloat() * CumulativeTotal;
            uint index = 0;

            while (total > CumulativeValues[index])
                index++;

            return index;
        }
    }
}
