namespace BrightData.Distributions
{
    internal class BinomialDistribution : INonNegativeDiscreteDistribution
    {
        public BinomialDistribution(IBrightDataContext context, float probability, uint numTrials)
        {
            NumTrials = numTrials;
            BaseDistribution = new BernoulliDistribution(context, probability);
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
