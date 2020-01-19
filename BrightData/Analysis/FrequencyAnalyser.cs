using System.Collections.Generic;
using System.Linq;

namespace BrightData.Analysis
{
    public class FrequencyAnalyser<T> : IDataAnalyser<T>
    {
        readonly int _writeCount;
        readonly Dictionary<string, ulong> _valueCount = new Dictionary<string, ulong>();

        ulong _total = 0, _highestCount = 0;
        string _mostFrequent = null;

        public FrequencyAnalyser(int writeCount = 100)
        {
            _writeCount = writeCount;
        }

        public int? NumDistinct => _valueCount.Count < Consts.MaxDistinct ? _valueCount.Count : (int?)null;
        public string MostFrequent => _valueCount.Count < Consts.MaxDistinct ? _mostFrequent : null;

        public virtual void Add(T obj)
        {
            _Add(obj.ToString());
        }

        protected void _Add(string str)
        {
            if (_valueCount.Count < Consts.MaxDistinct) {
                if (_valueCount.TryGetValue(str, out ulong count))
                    _valueCount[str] = count + 1;
                else
                    _valueCount.Add(str, 1);
                ++_total;
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
            metadata.WriteIfNotNull(Consts.Mode, MostFrequent);
            if (metadata.WriteIfNotNull(Consts.NumDistinct, NumDistinct)) {
                var total = (double) _total;
                foreach (var item in _valueCount.OrderByDescending(kv => kv.Value).Take(_writeCount))
                    metadata.Set($"{Consts.FrequencyPrefix}{item.Key}", item.Value / total);
            }
        }
    }
}
