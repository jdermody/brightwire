using System;
using System.Collections.Generic;
using System.Text;

namespace BrightData.Distributions
{
    class DiscreteUniformDistribution : IDiscreteDistribution
    {
        private readonly IBrightDataContext _context;

        public DiscreteUniformDistribution(IBrightDataContext context, int inclusiveLowerBound, int exclusiveUpperBound)
        {
            if(inclusiveLowerBound > exclusiveUpperBound)
                throw new ArgumentException("Lower bound was greater than upper bound");
            From = inclusiveLowerBound;
            Size = exclusiveUpperBound - inclusiveLowerBound;
            _context = context;
        }

        public int From { get; }
        public int Size { get; }

        public int Sample() => From + (int)_context.RandomIndex(Size);
    }
}
