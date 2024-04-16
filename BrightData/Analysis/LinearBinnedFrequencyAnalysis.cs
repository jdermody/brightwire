using System;
using System.Collections.Generic;

namespace BrightData.Analysis
{
    /// <summary>
    /// Binned frequency analysis
    /// </summary>
    internal class LinearBinnedFrequencyAnalysis(double min, double max, uint numBins)
    {
        readonly double _step = (max - min) / numBins;
        readonly ulong[] _bins = new ulong[numBins];
        ulong _belowRange = 0, _aboveRange = 0;

        public void Add(double val)
        {
            if (double.IsNaN(val))
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

        public IEnumerable<(double Start, double End, ulong Count)> ContinuousFrequency
        {
            get 
            {
                if (_belowRange > 0)
                    yield return (double.NegativeInfinity, min, _belowRange);
                var index = 0;
                foreach (var c in _bins) {
                    yield return (
                        min + (index * _step),
                        min + (index + 1) * _step,
                        c
                    );
                    ++index;
                }
                if(_aboveRange > 0)
                    yield return (max, double.PositiveInfinity, _aboveRange);
            }
        }
    }
}
