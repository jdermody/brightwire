namespace BrightData.Distribution
{
    /// <summary>
    /// Bernoulli distribution - https://en.wikipedia.org/wiki/Bernoulli_distribution
    /// </summary>
    /// <param name="context"></param>
    /// <param name="probability"></param>
    internal class BernoulliDistribution(BrightDataContext context, float probability) : INonNegativeDiscreteDistribution
    {
        public float Probability { get; } = probability;

        public uint Sample() => context.NextRandomFloat() < Probability ? (uint)1 : 0;
    }
}
