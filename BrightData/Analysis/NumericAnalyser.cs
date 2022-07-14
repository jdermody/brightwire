using System;
using System.Collections.Generic;
using System.Linq;

namespace BrightData.Analysis
{
    internal class NumericAnalyser : IDataAnalyser<double>
	{
		readonly uint _writeCount, _maxCount;
		readonly SortedDictionary<double, ulong> _distinct = new();

		double _mean, _m2, _min, _max, _mode, _l1, _l2;
		ulong _total, _highestCount;

		public NumericAnalyser(uint writeCount = Consts.MaxWriteCount, uint maxCount = Consts.MaxDistinct)
		{
            _max = double.MinValue;
            _min = double.MaxValue;
            _maxCount = maxCount;
            _writeCount = writeCount;
		}

		public virtual void Add(double val)
		{
			++_total;

			// online std deviation and mean 
			// https://en.wikipedia.org/wiki/Algorithms_for_calculating_variance#Online_algorithm
			var delta = val - _mean;
			_mean += (delta / _total);
			_m2 += delta * (val - _mean);

			// find the min and the max
			if (val < _min)
				_min = val;
			if (val > _max)
				_max = val;

			// add to distinct values
			if (_distinct.Count < _maxCount) {
                if (_distinct.TryGetValue(val, out var count))
					_distinct[val] = ++count;
				else
					_distinct.Add(val, count = 1);

				if (count > _highestCount) {
					_highestCount = count;
					_mode = val;
				}
			}

			// calculate norms
			_l1 += Math.Abs(val);
			_l2 += val * val;
		}

		public double L1Norm => _l1;
	    public double L2Norm => Math.Sqrt(_l2);
	    public double Min => _min;
	    public double Max => _max;
	    public double Mean => _mean;
	    public double? SampleVariance => _total > 1 ? _m2 / (_total - 1) : null;
        public double? PopulationVariance => _total > 0 ? _m2 / _total : null;
        public uint? NumDistinct => _distinct.Count < _maxCount ? (uint?)_distinct.Count : null;

	    public double? SampleStdDev {
            get
            {
                var variance = SampleVariance;
                if (variance.HasValue)
                    return Math.Sqrt(variance.Value);
                return null;
            }
        }

        public double? PopulationStdDev
        {
            get
            {
                var variance = PopulationVariance;
                if (variance.HasValue)
                    return Math.Sqrt(variance.Value);
                return null;
            }
        }

        public double? Median
        {
            get
            {
                double? ret = null;
                if (_distinct.Count < _maxCount && _distinct.Any()) {
                    if (_total % 2 == 1)
                        return SortedValues.Skip((int) (_total / 2)).First();
                    return SortedValues.Skip((int) (_total / 2 - 1)).Take(2).Average();
                }
                return ret;
            }
        }

        IEnumerable<double> SortedValues
        {
            get
            {
                foreach (var item in _distinct) {
                    for (ulong i = 0; i < item.Value; i++)
                        yield return item.Key;
                }
            }
        }

        public double? Mode
        {
            get
            {
                if (_distinct.Count < _maxCount && _distinct.Any())
                    return _mode;
                return null;
            }
        }

        public void AddObject(object obj)
        {
            var str = obj.ToString();
            if (str != null) {
                var val = double.Parse(str);
                Add(val);
            }
        }

        public void WriteTo(MetaData metadata)
        {
            metadata.Set(Consts.HasBeenAnalysed, true);
            metadata.Set(Consts.IsNumeric, true);
            metadata.Set(Consts.L1Norm, L1Norm);
			metadata.Set(Consts.L2Norm, L2Norm);
			metadata.Set(Consts.Min, Min);
			metadata.Set(Consts.Max, Max);
			metadata.Set(Consts.Mean, Mean);
            metadata.Set(Consts.Total, _total);
			metadata.SetIfNotNull(Consts.SampleVariance, SampleVariance);
			metadata.SetIfNotNull(Consts.SampleStdDev, SampleStdDev);
            metadata.SetIfNotNull(Consts.PopulationVariance, PopulationVariance);
            metadata.SetIfNotNull(Consts.PopulationStdDev, PopulationStdDev);
            metadata.SetIfNotNull(Consts.Median, Median);
			metadata.SetIfNotNull(Consts.Mode, Mode);
			if (metadata.SetIfNotNull(Consts.NumDistinct, NumDistinct)) {
				var total = (double) _total;
                var range = Max - Min;
                if (range > 0) {
                    var bin = new LinearBinnedFrequencyAnalysis(Min, Max, 10);
                    var index = 0;
                    foreach (var item in _distinct.OrderByDescending(kv => kv.Value)) {
                        if (index++ < _writeCount)
                            metadata.Set($"{Consts.FrequencyPrefix}{item.Key}", item.Value / total);
                        for (ulong i = 0; i < item.Value; i++)
                            bin.Add(item.Key);
                    }

                    foreach (var (s, e, c) in bin.ContinuousFrequency) {
                        if (c == 0 && (double.IsNegativeInfinity(s) || double.IsPositiveInfinity(e)))
                            continue;
                        var start = double.IsNegativeInfinity(s) ? "-∞" : s.ToString("G17");
                        var end = double.IsPositiveInfinity(e) ? "∞" : e.ToString("G17");
                        metadata.Set($"{Consts.FrequencyRangePrefix}{start}/{end}", c / total);
                    }
                }
            }
		}
	}
}
