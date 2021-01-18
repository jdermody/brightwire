﻿namespace BrightData.Distributions
{
    internal class BernoulliDistribution : INonNegativeDiscreteDistribution
    {
        readonly IBrightDataContext _context;

        public BernoulliDistribution(IBrightDataContext context, float probability)
        {
            Probability = probability;
            _context = context;
        }

        public float Probability { get; }

        public uint Sample() => _context.NextFloat() < Probability ? (uint)1 : 0;
    }
}
