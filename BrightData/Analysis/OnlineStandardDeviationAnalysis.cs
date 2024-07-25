using System;
using System.Numerics;

namespace BrightData.Analysis
{
    internal class OnlineStandardDeviationAnalysis<T> : IStandardDeviationAnalysis<T>
        where T: unmanaged, INumber<T>, IMinMaxValue<T>, IBinaryFloatingPointIeee754<T>, IConvertible
    {
        T _m2;

        public virtual void Add(T value)
        {
            ++Count;

            // online std deviation and mean 
            // https://en.wikipedia.org/wiki/Algorithms_for_calculating_variance#Online_algorithm
            var delta = value - Mean;
            Mean += (delta / T.CreateTruncating(Count));
            _m2 += delta * (value - Mean);
        }

        public ulong Count { get; private set; } = 0;
        public T Mean { get; private set; }
        public T? SampleVariance => Count > 1 ? _m2 / T.CreateTruncating(Count - 1) : null;
        public T? PopulationVariance => Count > 0 ? _m2 / T.CreateTruncating(Count) : null;

        public T? SampleStdDev {
            get
            {
                var variance = SampleVariance;
                if (variance.HasValue)
                    return T.Sqrt(variance.Value);
                return null;
            }
        }

        public T? PopulationStdDev
        {
            get
            {
                var variance = PopulationVariance;
                if (variance.HasValue)
                    return T.Sqrt(variance.Value);
                return null;
            }
        }
    }
}
