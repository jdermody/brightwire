﻿using System;
using BrightData.Types;

namespace BrightData.Analysis.Readers
{
    /// <summary>
    /// Date analysis results
    /// </summary>
    public class DateAnalysis : FrequencyAnalysis
    {
        internal DateAnalysis(MetaData metaData) : base(metaData)
        {
            MinDate = metaData.GetNullable<DateTime>(Consts.MinDate);
            MaxDate = metaData.GetNullable<DateTime>(Consts.MaxDate);
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
