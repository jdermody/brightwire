using System;
using System.Collections.Generic;
using System.Text;

namespace BrightData.Analysis.Readers
{
    public class DateAnalysis
    {
        public DateAnalysis(IMetaData metaData)
        {
            MinDate = metaData.Get<DateTime>(Consts.MinDate);
            MaxDate = metaData.Get<DateTime>(Consts.MaxDate);
            NumDistinct = metaData.GetNullable<uint>(Consts.NumDistinct);
        }

        public DateTime? MinDate { get; }
        public DateTime? MaxDate { get; }
        public uint? NumDistinct { get; }
    }
}
