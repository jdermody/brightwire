using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightData.Distributions
{
    public class BinomialDistribution : INonNegativeDiscreteDistribution
    {
        private readonly BernoulliDistribution _bernoulli;

        public BinomialDistribution(IBrightDataContext context, float probability, uint numTrials)
        {
            NumTrials = numTrials;
            _bernoulli = new BernoulliDistribution(context, probability);
        }

        public BernoulliDistribution BaseDistribution { get; }
        public uint NumTrials { get; }

        public uint Sample()
        {
            uint ret = 0;
            for (uint i = 0; i < NumTrials; i++)
                ret += BaseDistribution.Sample();
            return ret;
        }
    }
}
