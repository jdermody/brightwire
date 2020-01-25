using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightData;
using BrightTable.Buffers;
using BrightTable.Segments;

namespace BrightTable.Transformations.Conversions
{
    class ConvertColumnToBoolean : IConvertColumn
    {
        readonly HashSet<string> _true;

        public ConvertColumnToBoolean(params string[] trueValues)
        {
            _true = new HashSet<string>(trueValues);
        }

        public ISingleTypeTableSegment Convert(IBrightDataContext context, ISingleTypeTableSegment obj)
        {
            var ret = new DataSegmentBuffer<bool>(context, ColumnType.Boolean, obj.Size, obj.Enumerate().Select(o => _true.Contains(o.ToString())));
            ret.MetaData.Set(Consts.Type, ret.SingleType.ToString());
            ret.Finalise();
            return ret;
        }
    }
}
