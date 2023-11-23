using System;
using System.Collections.Generic;
using System.Linq;
using BrightData.Types;

namespace BrightData.Analysis
{
    /// <summary>
    /// Frequency analysis
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class FrequencyAnalyser<T> : IDataAnalyser<T>
        where T: notnull
    {
        readonly uint _writeCount;
        readonly Dictionary<string, ulong> _valueCount = new();

        ulong _highestCount = 0;
        string? _mostFrequent = null;

        public FrequencyAnalyser(uint writeCount = Consts.MaxWriteCount)
        {
            _writeCount = writeCount;
        }

        public uint NumDistinct => (uint)_valueCount.Count;
        public string? MostFrequent => _valueCount.Count > 0 ? _mostFrequent : null;
        public ulong Total { get; private set; } = 0;
        public virtual void Add(T obj) => AddString(obj.ToString());
        public IEnumerable<KeyValuePair<string, ulong>> ItemFrequency => _valueCount;

        protected void AddString(string? str)
        {
            if (str != null) {
                if (_valueCount.TryGetValue(str, out var count))
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

        public void AddObject(object obj) => AddString(obj.ToString());

        public void Add(ReadOnlySpan<T> block)
        {
            foreach(ref readonly var item in block)
                Add(item);
        }

        public virtual void WriteTo(MetaData metadata)
        {
            metadata.Set(Consts.HasBeenAnalysed, true);
            metadata.Set(Consts.Total, Total);
            metadata.SetIfNotNull(Consts.MostFrequent, MostFrequent);
            metadata.Set(Consts.NumDistinct, NumDistinct);
            var total = (double)Total;
            foreach (var item in _valueCount.OrderByDescending(kv => kv.Value).Take((int)_writeCount))
                metadata.Set($"{Consts.FrequencyPrefix}{item.Key}", item.Value / total);
        }
    }
}
