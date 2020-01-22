using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightData;
using BrightTable.Segments;

namespace BrightTable.Transformations.Conversions
{
    class ConvertColumnToWeightedIndexList : IConvertColumn
    {
        static WeightedIndexList _Convert(object obj)
        {
            if (obj is IndexList indexList)
                return WeightedIndexList.Create(indexList.Context, indexList.Indices.Select(ind => new WeightedIndexList.Item(ind, 1f)).ToArray());
            if (obj is WeightedIndexList weightedIndexList)
                return weightedIndexList;
            if (obj is Vector<float> vector)
                return vector.Data.ToSparse();

            throw new InvalidOperationException();
        }

        public ISingleTypeTableSegment Convert(IBrightDataContext context, ISingleTypeTableSegment segment)
        {
            var ret = new DataSegmentBuffer<WeightedIndexList>(context, ColumnType.WeightedIndexList, segment.Size, segment.Enumerate().Select(_Convert));
            ret.MetaData.Set(Consts.Type, ret.SingleType.ToString());
            ret.Finalise();
            return ret;
        }
    }
}
