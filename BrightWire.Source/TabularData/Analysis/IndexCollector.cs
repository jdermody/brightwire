using System;
using System.Collections.Generic;
using BrightWire.Models;

namespace BrightWire.TabularData.Analysis
{
    /// <summary>
    /// Collects min and max values from the index or weighted index lists of a single column in a data table
    /// </summary>
    class IndexCollector : IRowProcessor, IIndexColumnInfo
    {
        readonly int _index;
        uint _min, _max;

        public IndexCollector(int index)
        {
            _index = index;
            _min = uint.MaxValue;
            _max = 0;
        }

        public uint MinIndex => _min;
        public uint MaxIndex => _max;
        public int ColumnIndex => _index;
        public IEnumerable<object> DistinctValues => throw new NotImplementedException();
        public int? NumDistinct => null;

        public bool Process(IRow row)
        {
            var obj = row.Data[_index];
            if (obj is IndexList indexList) {
                foreach (var index in indexList.Index) {
                    if (index > _max)
                        _max = index;
                    if (_index < _min)
                        _min = index;
                }
            } else {
                var weightedIndexList = obj as WeightedIndexList;
                if (weightedIndexList == null)
                    throw new Exception("Unexpected index type: " + obj?.GetType());
            }
            return true;
        }
    }
}
