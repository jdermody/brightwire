using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightData;
using BrightTable.Buffers;
using BrightTable.Segments;

namespace BrightTable.Transformations.Conversions
{
    class ConvertColumnToCategoricalIndex : IConvertColumn
    {
        readonly Dictionary<string, int> _categoryIndex = new Dictionary<string, int>();

        public ISingleTypeTableSegment Convert(IBrightDataContext context, ISingleTypeTableSegment segment)
        {
            var data = segment.Enumerate().Select(o => _GetIndex(o.ToString()));
            var ret = new DataSegmentBuffer<int>(context, ColumnType.Int, segment.Size, data);
            var metaData = ret.MetaData;
            metaData.Set(Consts.Type, ret.SingleType.ToString());
            metaData.Set(Consts.IsNumeric, true);
            foreach (var category in _categoryIndex.OrderBy(d => d.Value))
                metaData.Set("category:" + category.Value, category.Key);
            ret.Finalise();
            return ret;
        }

        int _GetIndex(string str)
        {
            if(_categoryIndex.TryGetValue(str, out var index))
                return index;
            _categoryIndex.Add(str, index = _categoryIndex.Count);
            return index;
        }
    }
}
