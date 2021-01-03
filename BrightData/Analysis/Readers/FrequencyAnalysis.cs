using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightData.Analysis.Readers
{
    /// <summary>
    /// Frequency analysis results
    /// </summary>
    public class FrequencyAnalysis
    {
        internal FrequencyAnalysis(IMetaData metaData)
        {
            Total = metaData.Get<ulong>(Consts.Total);
            MostFrequent = metaData.Get<string>(Consts.MostFrequent);
            NumDistinct = metaData.GetNullable<uint>(Consts.NumDistinct);
            Frequency = metaData.GetStringsWithPrefix(Consts.FrequencyPrefix)
                .Select(k => (Label: k.Substring(Consts.FrequencyPrefix.Length), Value: metaData.Get<double>(k)))
                .ToArray()
            ;
        }

        /// <summary>
        /// Total number of items observed
        /// </summary>
        public ulong Total { get; }

        /// <summary>
        /// Most frequent item
        /// </summary>
        public string MostFrequent { get; }

        /// <summary>
        /// Number of distinct items
        /// </summary>
        public uint? NumDistinct { get; }

        /// <summary>
        /// Ranked histogram
        /// </summary>
        public (string Label, double Value)[] Frequency { get; }
    }
}
