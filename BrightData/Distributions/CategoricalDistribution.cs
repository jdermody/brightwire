using System.Collections.Generic;
using System.Linq;

namespace BrightData.Distributions
{
    internal class CategoricalDistribution : INonNegativeDiscreteDistribution
    {
        readonly IBrightDataContext _context;

        public CategoricalDistribution(IBrightDataContext context, IEnumerable<float> categoricalValues)
        {
            _context = context;

            float cumulativeTotal = 0;
            CumulativeValues = categoricalValues.Select(v => cumulativeTotal += v).ToArray();
            CumulativeTotal = cumulativeTotal;
        }

        public float[] CumulativeValues { get; }

        public float CumulativeTotal { get; }

        public uint Sample()
        {
            var total = _context.NextFloat() * CumulativeTotal;
            uint index = 0;

            while (total > CumulativeValues[index])
                index++;

            return index;
        }
    }
}
