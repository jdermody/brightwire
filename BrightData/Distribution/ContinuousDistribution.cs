using System;
using System.Numerics;

namespace BrightData.Distribution
{
    /// <summary>
    /// Continuous distribution - https://en.wikipedia.org/wiki/Probability_distribution#Absolutely_continuous_probability_distribution
    /// </summary>
    /// <remarks>
    /// A null lower bound defaults to zero and a null upper bound defaults to one, so omitting both bounds
    /// yields a uniform distribution over [0, 1). Note that a null upper bound is NOT "unbounded" - it simply
    /// defaults to one.
    /// </remarks>
    internal class ContinuousDistribution<T> : IContinuousDistribution<T>
        where T : unmanaged, INumber<T>, IBinaryFloatingPointIeee754<T>
    {
        readonly BrightDataContext _context;

        /// <summary>
        /// Creates a continuous (uniform) distribution over [inclusiveLowerBound, exclusiveUpperBound)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="inclusiveLowerBound">Inclusive lower bound (defaults to zero if null)</param>
        /// <param name="exclusiveUpperBound">Exclusive upper bound (defaults to one if null - not "unbounded")</param>
        public ContinuousDistribution(BrightDataContext context, T? inclusiveLowerBound = null, T? exclusiveUpperBound = null)
        {
            _context = context;
            // null-coalesce both bounds first, then validate - comparing the raw nullables would
            // silently treat a missing bound as "not greater" and defer the error (or produce a
            // negative size) to sample time
            From = inclusiveLowerBound ?? T.Zero;
            Size = (exclusiveUpperBound ?? T.One) - From;
            if (Size < T.Zero)
                throw new ArgumentException("Lower bound was higher than upper bound");
        }

        public T From { get; }
        public T Size { get; }

        public T Sample() => From + _context.NextRandom<T>() * Size;
    }
}
