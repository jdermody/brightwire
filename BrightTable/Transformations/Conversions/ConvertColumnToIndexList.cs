using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightData;
using BrightTable.Segments;

namespace BrightTable.Transformations.Conversions
{
    class ConvertColumnToIndexList : IConvertColumn
    {
        public ISingleTypeTableSegment Convert(IBrightDataContext context, ISingleTypeTableSegment obj)
        {
            var ret = new DataSegmentBuffer<IndexList>(context, ColumnType.IndexList, obj.Size, obj.Enumerate().Select(_Convert));
            ret.MetaData.Set(Consts.Type, ret.SingleType.ToString());
            ret.Finalise();
            return ret;
        }

        IndexList _Convert(object obj)
        {
            if (obj is IndexList indexList)
                return indexList;
            if (obj is WeightedIndexList weightedIndexList)
                return weightedIndexList.AsIndexList();
            if (obj is Vector<float> vector)
                return vector.Data.ToSparse().AsIndexList();

            throw new InvalidOperationException();
        }
    }
}
