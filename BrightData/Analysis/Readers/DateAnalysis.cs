using System;

namespace BrightData.Analysis.Readers
{
    /// <summary>
    /// Date analysis results
    /// </summary>
    public class DateAnalysis : FrequencyAnalysis
    {
        internal DateAnalysis(IMetaData metaData) : base(metaData)
        {
            MinDate = metaData.Get<DateTime>(Consts.MinDate);
            MaxDate = metaData.Get<DateTime>(Consts.MaxDate);
        }

        /// <summary>
        /// Minimum date (null if none)
        /// </summary>
        public DateTime? MinDate { get; }

        /// <summary>
        /// Maximum date (null if none)
        /// </summary>
        public DateTime? MaxDate { get; }
    }
}
