﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BrightData.Distributions
{
    public class BernoulliDistribution : IPositiveDiscreteDistribution
    {
        private readonly IBrightDataContext _context;

        public BernoulliDistribution(IBrightDataContext context, float probability)
        {
            Probability = probability;
            _context = context;
        }

        public float Probability { get; }

        public uint Sample() => _context.NextFloat() < Probability ? (uint)1 : 0;
    }
}
