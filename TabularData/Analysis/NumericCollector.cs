using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.TabularData.Analysis
{
    class NumberCollector : IRowProcessor, INumericColumnInfo
    {
        readonly int _index, _maxDistinct;
        readonly Dictionary<double, ulong> _distinct = new Dictionary<double, ulong>();

        double _mean = 0, _m2 = 0, _min = double.MaxValue, _max = double.MinValue, _mode = 0;
        ulong _total = 0, _highestCount = 0;

        public NumberCollector(int index, int maxDistinct = 131072 * 4)
        {
            _index = index;
            _maxDistinct = maxDistinct;
        }

        public bool Process(IRow row)
        {
            var val = row.GetField<double>(_index);
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
                ulong temp, count = 0;
                if (_distinct.TryGetValue(val, out temp))
                    _distinct[val] = count = temp + 1;
                else
                    _distinct.Add(val, count = 1);
                if (count > _highestCount) {
                    _highestCount = count;
                    _mode = val;
                }
            }
            return true;
        }

        public int ColumnIndex { get { return _index; } }
        public double Min { get { return _min; } }
        public double Max { get { return _max; } }
        public double Mean { get { return _mean; } }
        public double? Variance { get { return _total > 1 ? (_m2 / (_total - 1)) : (double?)null; } }
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
        public int? NumDistinct { get { return _distinct.Count < _maxDistinct ? _distinct.Count : (int?)null; } }
        public IEnumerable<object> DistinctValues { get { return _distinct.Count < _maxDistinct ? _distinct.Select(kv => (object)kv.Key) : null; } }
    }
}
