using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightData;
using BrightTable.Buffers;
using BrightTable.Segments;

namespace BrightTable.Transformations.Conversions
{
    class ConvertColumnToVector : IConvertColumn
    {
        static Vector<float> _Convert(object obj)
        {
            if (obj is IndexList indexList)
                return indexList.ToDense();
            if (obj is WeightedIndexList weightedIndexList)
                return weightedIndexList.ToDense();
            if (obj is Vector<float> vector)
                return vector;

            throw new InvalidOperationException();
        }

        public ISingleTypeTableSegment Convert(IBrightDataContext context, ISingleTypeTableSegment segment)
        {
            var ret = new DataSegmentBuffer<Vector<float>>(context, ColumnType.IndexList, segment.Size, segment.Enumerate().Select(_Convert));
            ret.MetaData.Set(Consts.Type, ret.SingleType.ToString());
            ret.Finalise();
            return ret;
        }
    }
}
