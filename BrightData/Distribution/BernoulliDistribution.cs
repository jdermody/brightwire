namespace BrightData.Distribution
{
    internal class BernoulliDistribution : INonNegativeDiscreteDistribution
    {
        readonly BrightDataContext _context;

        public BernoulliDistribution(BrightDataContext context, float probability)
        {
            Probability = probability;
            _context = context;
        }

        public float Probability { get; }

        public uint Sample() => _context.NextRandomFloat() < Probability ? (uint)1 : 0;
    }
}
