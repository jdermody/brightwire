namespace BrightData.Distribution
{
    /// <summary>
    /// Binomial distribution - https://en.wikipedia.org/wiki/Binomial_distribution
    /// </summary>
    /// <param name="context"></param>
    /// <param name="probability"></param>
    /// <param name="numTrials"></param>
    internal class BinomialDistribution(BrightDataContext context, float probability, uint numTrials)
        : INonNegativeDiscreteDistribution
    {
        public BernoulliDistribution BaseDistribution { get; } = new(context, probability);
        public uint NumTrials { get; } = numTrials;

        public uint Sample()
        {
            uint ret = 0;
            for (uint i = 0; i < NumTrials; i++)
                ret += BaseDistribution.Sample();
            return ret;
        }
    }
}
