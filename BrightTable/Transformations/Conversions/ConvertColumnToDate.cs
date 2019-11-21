using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightData;
using BrightTable.Segments;

namespace BrightTable.Transformations.Conversions
{
    class ConvertColumnToDate : IConvertColumn
    {
        public ISingleTypeTableSegment Convert(IBrightDataContext context, ISingleTypeTableSegment obj)
        {
            var ret = new DataSegmentBuffer<DateTime>(context, ColumnType.Date, obj.Size, obj.Enumerate().Select(o => DateTime.Parse(o.ToString())));
            ret.MetaData.Set(Consts.Type, ret.SingleType.ToString());
            ret.Finalise();
            return ret;
        }
    }
}
