using System.Collections.Generic;
using System.Linq;

namespace BrightData.Analysis
{
    public class FrequencyAnalyser<T> : IDataAnalyser<T>
    {
        readonly int _writeCount;
        readonly Dictionary<string, ulong> _valueCount = new Dictionary<string, ulong>();

        ulong _highestCount = 0;
        string _mostFrequent = null;

        public FrequencyAnalyser(int writeCount = 100)
        {
            _writeCount = writeCount;
        }

        public uint? NumDistinct => _valueCount.Count < Consts.MaxDistinct ? (uint?)_valueCount.Count : null;
        public string MostFrequent => _valueCount.Count < Consts.MaxDistinct ? _mostFrequent : null;
        public ulong Total { get; private set; } = 0;
        public virtual void Add(T obj)
        {
            _Add(obj.ToString());
        }
        public IEnumerable<KeyValuePair<string, ulong>> ItemFrequency => _valueCount;

        protected void _Add(string str)
        {
            if (_valueCount.Count < Consts.MaxDistinct) {
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

        public void AddObject(object obj)
        {
            Add((T)obj);
        }

        public virtual void WriteTo(IMetaData metadata)
        {
            metadata.Set(Consts.HasBeenAnalysed, true);
            metadata.SetIfNotNull(Consts.Mode, MostFrequent);
            if (metadata.SetIfNotNull(Consts.NumDistinct, NumDistinct)) {
                var total = (double)Total;
                foreach (var item in _valueCount.OrderByDescending(kv => kv.Value).Take(_writeCount))
                    metadata.Set($"{Consts.FrequencyPrefix}{item.Key}", item.Value / total);
            }
        }
    }
}
