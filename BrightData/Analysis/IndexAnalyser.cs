﻿using System;
using System.Collections.Generic;
using System.Linq;
using BrightData.Types;

namespace BrightData.Analysis
{
    /// <summary>
    /// Index based type analysis
    /// </summary>
    internal class IndexAnalyser(uint writeCount = Consts.MaxWriteCount) : IDataAnalyser<IHaveIndices>
    {
        readonly Dictionary<uint, uint> _indexFrequency = [];
        uint _min = uint.MaxValue, _max = uint.MinValue;

        public void Add(IHaveIndices obj)
        {
            foreach(var index in obj.Indices)
                Add(index);
        }

        public void Append(ReadOnlySpan<IHaveIndices> block)
        {
            foreach(ref readonly var item in block)
                Add(item);
        }

        void Add(uint index)
        {
            if(index < _min)
                _min = index;
            if(index > _max)
                _max = index;

            if (_indexFrequency.TryGetValue(index, out var count))
                _indexFrequency[index] = count + 1;
            else
                _indexFrequency.Add(index, 1);
        }

        public void AddObject(object obj)
        {
            if (obj is IHaveIndices indexed)
                Add(indexed);
        }

        public void WriteTo(MetaData metadata)
        {
            metadata.Set(Consts.HasBeenAnalysed, true);
            if (_min != uint.MaxValue)
                metadata.Set(Consts.MinIndex, _min);
            if(_max != uint.MinValue)
                metadata.Set(Consts.MaxIndex, _max);

            metadata.Set(Consts.NumDistinct, (uint)_indexFrequency.Count);
            var total = (double) _indexFrequency.Count;
            foreach (var item in _indexFrequency.OrderByDescending(kv => kv.Value).Take((int)writeCount))
                metadata.Set($"{Consts.FrequencyPrefix}{item.Key}", item.Value / total);
        }
    }
}
