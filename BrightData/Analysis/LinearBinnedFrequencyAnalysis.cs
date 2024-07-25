using System;
using System.Collections.Generic;
using System.Numerics;

namespace BrightData.Analysis
{
    /// <summary>
    /// Binned frequency analysis
    /// </summary>
    internal class LinearBinnedFrequencyAnalysis<T>(T min, T max, uint numBins)
        where T : unmanaged, INumber<T>, IBinaryFloatingPointIeee754<T>
    {
        readonly T _step = (max - min) / T.CreateTruncating(numBins);
        readonly ulong[] _bins = new ulong[numBins];
        ulong _belowRange = 0, _aboveRange = 0;

        public void Add(T val)
        {
            if (T.IsNaN(val))
                return;

            if (val < min)
                _belowRange++;
            else if (val > max)
                _aboveRange++;
            else {
                var binIndex = Convert.ToInt32((val - min) / _step);
                if (binIndex >= _bins.Length)
                    --binIndex;
                _bins[binIndex]++;
            }
        }

        public IEnumerable<(T Start, T End, ulong Count)> ContinuousFrequency
        {
            get 
            {
                if (_belowRange > 0)
                    yield return (T.NegativeInfinity, min, _belowRange);
                var index = 0;
                foreach (var c in _bins) {
                    var val = T.CreateTruncating(index);
                    yield return (
                        min + (val * _step),
                        min + (val + T.One) * _step,
                        c
                    );
                    ++index;
                }
                if(_aboveRange > 0)
                    yield return (max, T.PositiveInfinity, _aboveRange);
            }
        }
    }
}
