﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightData.Analysis.Readers
{
    /// <summary>
    /// Index analysis results
    /// </summary>
    public class IndexAnalysis
    {
        internal IndexAnalysis(IMetaData metaData)
        {
            MinIndex = metaData.Get<uint>(Consts.MinIndex);
            MaxIndex = metaData.Get<uint>(Consts.MaxIndex);
            NumDistinct = metaData.GetNullable<uint>(Consts.NumDistinct);
            Frequency = metaData.GetStringsWithPrefix(Consts.FrequencyPrefix)
                .Select(k => (Label: k.Substring(Consts.FrequencyPrefix.Length), Value: metaData.Get<double>(k)))
                .ToArray()
            ;
        }

        /// <summary>
        /// Lowest observed index
        /// </summary>
        public uint MinIndex { get; }

        /// <summary>
        /// Highest observed index
        /// </summary>
        public uint MaxIndex { get; }

        /// <summary>
        /// Number of distinct items
        /// </summary>
        public uint? NumDistinct { get; }

        /// <summary>
        /// Ranked histogram
        /// </summary>
        public (string Label, double value)[] Frequency { get; }
    }
}
