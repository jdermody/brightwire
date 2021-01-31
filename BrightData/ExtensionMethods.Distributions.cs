using System.Collections.Generic;
using BrightData.Distribution;

namespace BrightData
{
    public partial class ExtensionMethods
    {
        /// <summary>
        /// Returns a randomly initialized float
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static float NextRandomFloat(this IBrightDataContext context) => (float)context.Random.NextDouble();

        /// <summary>
        /// Returns a randomly initialized positive number
        /// </summary>
        /// <param name="context"></param>
        /// <param name="length">Exclusive upper bound</param>
        /// <returns></returns>
        public static uint RandomIndex(this IBrightDataContext context, int length) => (uint)context.Random.Next(length);

        /// <summary>
        /// Returns a randomly initialized positive number
        /// </summary>
        /// <param name="context"></param>
        /// <param name="length">Exclusive upper bound</param>
        /// <returns></returns>
        public static uint RandomIndex(this IBrightDataContext context, uint length) => (uint)context.Random.Next((int)length);

        /// <summary>
        /// Create a bernoulli distribution
        /// </summary>
        /// <param name="context"></param>
        /// <param name="probability"></param>
        /// <returns></returns>
        public static INonNegativeDiscreteDistribution CreateBernoulliDistribution(this IBrightDataContext context, float probability) => new BernoulliDistribution(context, probability);

        /// <summary>
        /// Create a binomial distribution
        /// </summary>
        /// <param name="context"></param>
        /// <param name="probability"></param>
        /// <param name="numTrials"></param>
        /// <returns></returns>
        public static INonNegativeDiscreteDistribution CreateBinomialDistribution(this IBrightDataContext context, float probability, uint numTrials) => new BinomialDistribution(context, probability, numTrials);

        /// <summary>
        /// Create a categorical distribution
        /// </summary>
        /// <param name="context"></param>
        /// <param name="categoricalValues"></param>
        /// <returns></returns>
        public static INonNegativeDiscreteDistribution CreateCategoricalDistribution(this IBrightDataContext context, IEnumerable<float> categoricalValues) => new CategoricalDistribution(context, categoricalValues);

        /// <summary>
        /// Create a continuous distribution
        /// </summary>
        /// <param name="context"></param>
        /// <param name="inclusiveLowerBound"></param>
        /// <param name="exclusiveUpperBound"></param>
        /// <returns></returns>
        public static IContinuousDistribution CreateContinuousDistribution(this IBrightDataContext context, float inclusiveLowerBound = 0f, float exclusiveUpperBound = 1f) => new ContinuousDistribution(context, inclusiveLowerBound, exclusiveUpperBound);

        /// <summary>
        /// Create a discrete uniform distribution
        /// </summary>
        /// <param name="context"></param>
        /// <param name="inclusiveLowerBound"></param>
        /// <param name="exclusiveUpperBound"></param>
        /// <returns></returns>
        public static IDiscreteDistribution CreateDiscreteUniformDistribution(this IBrightDataContext context, int inclusiveLowerBound, int exclusiveUpperBound) => new DiscreteUniformDistribution(context, inclusiveLowerBound, exclusiveUpperBound);

        /// <summary>
        /// Create a normal distribution
        /// </summary>
        /// <param name="context"></param>
        /// <param name="mean"></param>
        /// <param name="stdDev">Standard deviation</param>
        /// <returns></returns>
        public static IContinuousDistribution CreateNormalDistribution(this IBrightDataContext context, float mean = 0f, float stdDev = 1f) => new NormalDistribution(context, mean, stdDev);
    }
}
