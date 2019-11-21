using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightData;
using BrightTable.Segments;

namespace BrightTable.Transformations.Conversions
{
    class ConvertColumnToString : IConvertColumn
    {
        public ISingleTypeTableSegment Convert(IBrightDataContext context, ISingleTypeTableSegment segment)
        {
            var ret = new DataSegmentBuffer<string>(context, ColumnType.String, segment.Size, segment.Enumerate().Select(o => o.ToString()));
            ret.MetaData.Set(Consts.Type, ret.SingleType.ToString());
            ret.Finalise();
            return ret;
        }
    }
}
