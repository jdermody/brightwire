using System.Collections.Generic;
using System.Linq;

namespace BrightData.Analysis
{
    public class IndexAnalyser : IDataAnalyser<IHaveIndices>
    {
        readonly int _writeCount;
        readonly Dictionary<uint, uint> _indexFrequency = new Dictionary<uint, uint>();
        uint _min = uint.MaxValue, _max = uint.MinValue;

        public IndexAnalyser(int writeCount = 100)
        {
            _writeCount = writeCount;
        }

        public void Add(IHaveIndices obj)
        {
            if (obj is IndexList indexList) {
                foreach (var index in indexList.Indices)
                    _Add(index);
            }else if (obj is WeightedIndexList weightedIndexList) {
                foreach(var index in weightedIndexList.Indices)
                    _Add(index.Index);
            }
        }

        void _Add(uint index)
        {
            if(index < _min)
                _min = index;
            if(index > _max)
                _max = index;

            if (_indexFrequency.Count < Consts.MaxDistinct) {
                if (_indexFrequency.TryGetValue(index, out var count))
                    _indexFrequency[index] = count + 1;
                else
                    _indexFrequency.Add(index, 1);
            }
        }

        public void AddObject(object obj)
        {
            if (obj is IHaveIndices indexed)
                Add(indexed);
        }

        public void WriteTo(IMetaData metadata)
        {
            if(_min != uint.MaxValue)
                metadata.Set(Consts.MinIndex, _min);
            if(_max != uint.MinValue)
                metadata.Set(Consts.MaxIndex, _max);

            if (_indexFrequency.Count < Consts.MaxDistinct) {
                metadata.Set(Consts.NumDistinct, _indexFrequency.Count);
                var total = (double) _indexFrequency.Count;
                foreach (var item in _indexFrequency.OrderByDescending(kv => kv.Value).Take(_writeCount))
                    metadata.Set($"{Consts.FrequencyPrefix}{item.Key}", item.Value / total);
            }
        }
    }
}
