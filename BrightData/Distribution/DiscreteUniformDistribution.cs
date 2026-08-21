using System;

namespace BrightData.Distribution
{
    /// <summary>
    /// Discrete uniform distribution - https://en.wikipedia.org/wiki/Discrete_uniform_distribution
    /// </summary>
    internal class DiscreteUniformDistribution : IDiscreteDistribution
    {
        readonly BrightDataContext _context;

        public DiscreteUniformDistribution(BrightDataContext context, int inclusiveLowerBound, int exclusiveUpperBound)
        {
            // an empty range (lower >= upper) is invalid - reject it here so the error surfaces at construction
            // time rather than as an ArgumentOutOfRangeException deep inside Sample()
            if (inclusiveLowerBound >= exclusiveUpperBound)
                throw new ArgumentException("Lower bound was greater than or equal to upper bound");
            From = inclusiveLowerBound;
            Size = exclusiveUpperBound - inclusiveLowerBound;
            _context = context;
        }

        public int From { get; }
        public int Size { get; }

        public int Sample() => From + (int)_context.RandomIndex(Size);
    }
}
