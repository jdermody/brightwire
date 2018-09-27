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
	    uint _min, _max;

        public IndexCollector(int index)
        {
            ColumnIndex = index;
            _min = uint.MaxValue;
            _max = 0;
        }

	    public int ColumnIndex { get; }
        public uint MinIndex => _min;
	    public uint MaxIndex => _max;
	    public IEnumerable<object> DistinctValues => throw new NotImplementedException();
        public int? NumDistinct => null;
	    public ColumnInfoType Type => ColumnInfoType.Index;

        public bool Process(IRow row)
        {
            var obj = row.Data[ColumnIndex];
            if (obj is IndexList indexList) {
                foreach (var index in indexList.Index) {
                    if (index > _max)
                        _max = index;
                    if (ColumnIndex < _min)
                        _min = index;
                }
            } else {
	            if (!(obj is WeightedIndexList weightedIndexList))
                    throw new Exception("Unexpected index type: " + obj?.GetType());
            }
            return true;
        }
    }
}
