using System;
using System.Collections.Generic;
using System.Linq;

namespace BrightData.Analysis
{
    public class LinearBinnedFrequencyAnalysis
    {
        readonly double _min, _max, _step;
        readonly ulong[] _bins;
        ulong _belowRange = 0, _aboveRange = 0;

        public LinearBinnedFrequencyAnalysis(double min, double max, uint numBins)
        {
            _step = (max - min) / numBins;
            _min = min;
            _max = max;

            _bins = new ulong[numBins];
        }

        public void Add(double val)
        {
            if (val < _min)
                _belowRange++;
            else if (val > _max)
                _aboveRange++;
            else {
                var binIndex = Convert.ToInt32((val - _min) / _step);
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
                    yield return (double.NegativeInfinity, _min, _belowRange);
                var index = 0;
                foreach (var c in _bins) {
                    yield return (
                        _min + (index * _step),
                        _min + (index + 1) * _step,
                        c
                    );
                    ++index;
                }
                if(_aboveRange > 0)
                    yield return (_max, double.PositiveInfinity, _aboveRange);
            }
        }
    }
}
