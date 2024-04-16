using System;

namespace BrightData.Distribution
{
    /// <summary>
    /// Continuous distribution - https://en.wikipedia.org/wiki/Probability_distribution#Absolutely_continuous_probability_distribution
    /// </summary>
    internal class ContinuousDistribution : IContinuousDistribution
    {
        readonly BrightDataContext _context;

        public ContinuousDistribution(BrightDataContext context, float inclusiveLowerBound = 0f, float exclusiveUpperBound = 1f)
        {
            if (inclusiveLowerBound > exclusiveUpperBound)
                throw new ArgumentException("Lower bound was higher than upper bound");

            _context = context;
            From = inclusiveLowerBound;
            Size = exclusiveUpperBound - inclusiveLowerBound;
        }

        public float From { get; }
        public float Size { get; }

        public float Sample() => From + _context.NextRandomFloat() * Size;
    }
}
