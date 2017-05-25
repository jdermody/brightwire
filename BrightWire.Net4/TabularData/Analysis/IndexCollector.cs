using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightWire.Models;

namespace BrightWire.TabularData.Analysis
{
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
            var indexList = obj as IndexList;
            if(indexList != null) {
                foreach(var index in indexList.Index) {
                    if (index > _max)
                        _max = index;
                    if (_index < _min)
                        _min = index;
                }
            }else {
                var weightedIndexList = obj as WeightedIndexList;
                if (weightedIndexList == null)
                    throw new Exception("Unexpected index type: " + obj?.GetType()?.ToString() ?? "(null)");
            }
            return true;
        }
    }
}
