using System;
using System.Collections.Generic;
using System.Text;

namespace BrightData.Analysis.Readers
{
    public class DimensionAnalysis
    {
        public DimensionAnalysis(IMetaData metaData)
        {
            XDimension = metaData.GetNullable<uint>(Consts.XDimension);
            YDimension = metaData.GetNullable<uint>(Consts.YDimension);
            ZDimension = metaData.GetNullable<uint>(Consts.ZDimension);
            NumDistinct = metaData.GetNullable<uint>(Consts.NumDistinct);
            Size = metaData.Get<uint>(Consts.Size);
        }

        public uint? XDimension { get; }
        public uint? YDimension { get; }
        public uint? ZDimension { get; }
        public uint? NumDistinct { get; }
        public uint Size { get; }
    }
}
