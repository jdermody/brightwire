using System;

namespace BrightData.Distributions
{
    class ContinuousDistribution : IContinuousDistribution
    {
        private readonly IBrightDataContext _context;

        public ContinuousDistribution(IBrightDataContext context, float inclusiveLowerBound = 0f, float exclusiveUpperBound = 1f)
        {
            if (inclusiveLowerBound > exclusiveUpperBound)
                throw new ArgumentException("Lower bound was higher than upper bound");

            _context = context;
            From = inclusiveLowerBound;
            Size = exclusiveUpperBound - inclusiveLowerBound;
        }

        public float From { get; }
        public float Size { get; }

        public float Sample() => From + _context.NextFloat() * Size;
    }
}
