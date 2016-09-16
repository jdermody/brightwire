using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.TabularData.Analysis
{
    class StringCollector : IRowProcessor, IStringColumnInfo
    {
        readonly int _index, _maxDistinct;
        readonly Dictionary<string, ulong> _distinct = new Dictionary<string, ulong>();

        int _minLength = int.MaxValue, _maxLength = int.MinValue;
        ulong _total = 0, _highestCount = 0;
        string _mode = null;

        public StringCollector(int index, int maxDistinct = 131072 * 4)
        {
            _index = index;
            _maxDistinct = maxDistinct;
        }

        public bool Process(IRow row)
        {
            var val = row.GetField<string>(_index);
            var len = val.Length;
            if (len < _minLength)
                _minLength = len;
            if (len > _maxLength)
                _maxLength = len;
            ++_total;

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
        public int MinLength { get { return _minLength; } }
        public int MaxLength { get { return _maxLength; } }
        public string MostCommonString { get { return _distinct.Count < _maxDistinct ? _mode : null; } }
        public int? NumDistinct { get { return _distinct.Count < _maxDistinct ? _distinct.Count : (int?)null; } }
        public IEnumerable<object> DistinctValues { get { return _distinct.Count < _maxDistinct ? _distinct.Select(kv => kv.Key) : null; } }
    }
}
