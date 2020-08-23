using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightData.Analysis
{
    public class NumericAnalyser : IDataAnalyser<double>
	{
		readonly int _writeCount;
		readonly Dictionary<double, ulong> _distinct = new Dictionary<double, ulong>();

		double _mean, _m2, _min, _max, _mode, _l1, _l2;
		ulong _total, _highestCount;

		public NumericAnalyser(int writeCount = 100)
		{
            _max = double.MinValue;
            _min = double.MaxValue;

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
			if (_distinct.Count < Consts.MaxDistinct) {
                if (_distinct.TryGetValue(val, out var count))
					_distinct[val] = count + 1;
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
	    public double? Variance => _total > 1 ? _m2 / (_total - 1) : (double?)null;
	    public int? NumDistinct => _distinct.Count < Consts.MaxDistinct ? _distinct.Count : (int?)null;

	    public double? StdDev {
            get
            {
                var variance = Variance;
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
                if (_distinct.Count < Consts.MaxDistinct && _distinct.Any()) {
                    ulong middle = _total / 2, count = 0;
                    foreach (var item in _distinct.OrderBy(kv => kv.Key)) {
                        top:
                        if (count + item.Value >= middle) {
                            if (ret.HasValue) {
                                ret = (ret.Value + item.Key) / 2;
                                break;
                            }

                            ret = item.Key;
                            if (_total % 2 == 0)
	                            break;
                            middle++;
                            goto top;
                        }
                        count += item.Value;
                    }
                }
                return ret;
            }
        }

        public double? Mode
        {
            get
            {
                if (_distinct.Count < Consts.MaxDistinct && _distinct.Any())
                    return _mode;
                return null;
            }
        }

        public void AddObject(object obj)
        {
	        var val = double.Parse(obj.ToString());
	        Add(val);
        }

        public void WriteTo(IMetaData metadata)
        {
            metadata.Set(Consts.HasBeenAnalysed, true);
            metadata.Set(Consts.IsNumeric, true);
            metadata.Set(Consts.L1Norm, L1Norm);
			metadata.Set(Consts.L2Norm, L2Norm);
			metadata.Set(Consts.Min, Min);
			metadata.Set(Consts.Max, Max);
			metadata.Set(Consts.Mean, Mean);
			metadata.SetIfNotNull(Consts.Variance, Variance);
			metadata.SetIfNotNull(Consts.StdDev, StdDev);
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

                    foreach (var item in bin.ContinuousFrequency) {
                        if (item.Count == 0 && (double.IsNegativeInfinity(item.Start) || double.IsPositiveInfinity(item.End)))
                            continue;
                        var start = double.IsNegativeInfinity(item.Start) ? "-∞" : item.Start.ToString("G17");
                        var end = double.IsPositiveInfinity(item.End) ? "∞" : item.End.ToString("G17");
                        metadata.Set($"{Consts.FrequencyRangePrefix}{start}/{end}", item.Count / total);
                    }
                }
            }
		}
	}
}
