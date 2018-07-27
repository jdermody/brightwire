using System.Collections.Generic;
using System.Linq;

namespace BrightWire.TabularData.Analysis
{
    /// <summary>
    /// A collector that collects the frequency from a single column of a data table
    /// </summary>
    class FrequencyCollector : IRowProcessor, IFrequencyColumnInfo
    {
	    readonly Dictionary<string, ulong> _valueCount = new Dictionary<string, ulong>();

	    public FrequencyCollector(int index)
        {
            ColumnIndex = index;
        }

        public int ColumnIndex { get; }
	    public int? NumDistinct => _valueCount.Count;
        public IEnumerable<object> DistinctValues => _valueCount.Select(kv => kv.Key);
        public IEnumerable<KeyValuePair<string, ulong>> Frequency => _valueCount;
	    public ulong Total { get; set; } = 0;

	    public bool Process(IRow row)
        {
            var val = row.GetField<string>(ColumnIndex);
            if (_valueCount.TryGetValue(val, out ulong count))
                _valueCount[val] = count + 1;
            else
                _valueCount.Add(val, 1);
            ++Total;
            return true;
        }
    }
}
