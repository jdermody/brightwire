using System;
using System.Collections.Generic;
using System.Linq;

namespace BrightWire.TabularData.Analysis
{
    /// <summary>
    /// Collects standard deviation, mean, mode etc from a single numeric column in a data table
    /// </summary>
    internal class NumberCollector : IRowProcessor, INumericColumnInfo
    {
        readonly int _maxDistinct;
        readonly Dictionary<double, ulong> _distinct = new Dictionary<double, ulong>();

        double _mean = 0, _m2 = 0, _min = double.MaxValue, _max = double.MinValue, _mode = 0, _l1 = 0, _l2 = 0;
        ulong _total = 0, _highestCount = 0;

        public NumberCollector(int index, int maxDistinct = 131072 * 4)
        {
            ColumnIndex = index;
            _maxDistinct = maxDistinct;
        }

        public bool Process(IRow row)
        {
            var val = row.GetField<double>(ColumnIndex);
            ++_total;

            // online std deviation and mean 
            // https://en.wikipedia.org/wiki/Algorithms_for_calculating_variance#Online_algorithm
            var delta = val - _mean;
            _mean += delta / _total;
            _m2 += delta * (val - _mean);

            // find the min and the max
            if (val < _min)
                _min = val;
            if (val > _max)
                _max = val;

            // add to distinct values
            if (_distinct.Count < _maxDistinct) {
                ulong count = 0;
                if (_distinct.TryGetValue(val, out ulong temp))
                    _distinct[val] = count = temp + 1;
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

            return true;
        }

        public double L1Norm => _l1;
	    public double L2Norm => Math.Sqrt(_l2);
	    public int ColumnIndex { get; }
	    public double Min => _min;
	    public double Max => _max;
	    public double Mean => _mean;
	    public double? Variance => _total > 1 ? (_m2 / (_total - 1)) : (double?)null;
	    public ColumnInfoType Type => ColumnInfoType.Numeric;

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
                if (_distinct.Count < _maxDistinct && _distinct.Any()) {
                    ulong middle = _total / 2, count = 0;
                    foreach (var item in _distinct.OrderBy(kv => kv.Key)) {
                        top:
                        if (count + item.Value >= middle) {
                            if (ret.HasValue) {
                                ret = (ret.Value + item.Key) / 2;
                                break;
                            }
                            else {
                                ret = item.Key;
                                if (_total % 2 == 0)
                                    break;
                                middle = middle + 1;
                                goto top;
                            }
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
                if (_distinct.Count < _maxDistinct && _distinct.Any())
                    return _mode;
                return null;
            }
        }
        public int? NumDistinct => _distinct.Count < _maxDistinct ? _distinct.Count : (int?)null;
	    public IEnumerable<object> DistinctValues { get { return _distinct.Count < _maxDistinct ? _distinct.Select(kv => (object)kv.Key) : null; } }
    }
}
