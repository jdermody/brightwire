using System;
using System.Numerics;

namespace BrightData.Distribution
{
    /// <summary>
    /// Continuous distribution - https://en.wikipedia.org/wiki/Probability_distribution#Absolutely_continuous_probability_distribution
    /// </summary>
    internal class ContinuousDistribution<T> : IContinuousDistribution<T>
        where T : unmanaged, INumber<T>, IBinaryFloatingPointIeee754<T>
    {
        readonly BrightDataContext _context;

        public ContinuousDistribution(BrightDataContext context, T? inclusiveLowerBound = null, T? exclusiveUpperBound = null)
        {
            if (inclusiveLowerBound > exclusiveUpperBound)
                throw new ArgumentException("Lower bound was higher than upper bound");

            _context = context;
            From = inclusiveLowerBound ?? T.Zero;
            Size = (exclusiveUpperBound ?? T.One) - From;
        }

        public T From { get; }
        public T Size { get; }

        public T Sample() => From + _context.NextRandom<T>() * Size;
    }
}
