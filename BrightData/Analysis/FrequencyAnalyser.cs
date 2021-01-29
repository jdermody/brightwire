using System.Collections.Generic;
using System.Linq;

namespace BrightData.Analysis
{
    internal class FrequencyAnalyser<T> : IDataAnalyser<T>
        where T: notnull
    {
        readonly uint _writeCount, _maxCount;
        readonly Dictionary<string, ulong> _valueCount = new Dictionary<string, ulong>();

        ulong _highestCount = 0;
        string? _mostFrequent = null;

        public FrequencyAnalyser(uint writeCount = Consts.MaxWriteCount, uint maxCount = Consts.MaxDistinct)
        {
            _writeCount = writeCount;
            _maxCount = maxCount;
        }

        public uint? NumDistinct => _valueCount.Count < _maxCount ? (uint?)_valueCount.Count : null;
        public string? MostFrequent => _valueCount.Count < _maxCount ? _mostFrequent : null;
        public ulong Total { get; private set; } = 0;
        public virtual void Add(T obj) => _Add(obj.ToString());
        public IEnumerable<KeyValuePair<string, ulong>> ItemFrequency => _valueCount;

        protected void _Add(string? str)
        {
            if (str != null && _valueCount.Count < _maxCount) {
                if (_valueCount.TryGetValue(str, out ulong count))
                    _valueCount[str] = count + 1;
                else
                    _valueCount.Add(str, 1);
                ++Total;
                if (count > _highestCount) {
                    _highestCount = count;
                    _mostFrequent = str;
                }
            }
        }

        public void AddObject(object obj) => Add((T)obj);

        public virtual void WriteTo(IMetaData metadata)
        {
            metadata.Set(Consts.HasBeenAnalysed, true);
            metadata.Set(Consts.Total, Total);
            metadata.SetIfNotNull(Consts.MostFrequent, MostFrequent);
            if (metadata.SetIfNotNull(Consts.NumDistinct, NumDistinct)) {
                var total = (double)Total;
                foreach (var item in _valueCount.OrderByDescending(kv => kv.Value).Take((int)_writeCount))
                    metadata.Set($"{Consts.FrequencyPrefix}{item.Key}", item.Value / total);
            }
        }
    }
}
