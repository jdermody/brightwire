using System;
using System.Collections.Generic;
using System.Text;

namespace BrightData.Analysis.Readers
{
    /// <summary>
    /// Date analysis results
    /// </summary>
    public class DateAnalysis
    {
        internal DateAnalysis(IMetaData metaData)
        {
            MinDate = metaData.Get<DateTime>(Consts.MinDate);
            MaxDate = metaData.Get<DateTime>(Consts.MaxDate);
            NumDistinct = metaData.GetNullable<uint>(Consts.NumDistinct);
        }

        /// <summary>
        /// Minimum date (null if none)
        /// </summary>
        public DateTime? MinDate { get; }

        /// <summary>
        /// Maximum date (null if none)
        /// </summary>
        public DateTime? MaxDate { get; }

        /// <summary>
        /// Number of distinct date (null if max count exceeded)
        /// </summary>
        public uint? NumDistinct { get; }
    }
}
