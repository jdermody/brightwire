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
            var index = 0;

            // linear scan is deliberate: the real call sites have only a handful of categories
            // (e.g. the 3-way random-projection distribution), where a scan beats a binary search.
            // The bound is clamped to the last entry so that float rounding of the product can
            // never push the index past the end (which would throw from inside Sample()).
            while (index < CumulativeValues.Length - 1 && total > CumulativeValues[index])
                index++;

            return (uint)index;
        }
    }
}
