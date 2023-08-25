using System.Linq;

namespace BrightData.Analysis.Readers
{
    /// <summary>
    /// Frequency analysis results
    /// </summary>
    public class FrequencyAnalysis
    {
        internal FrequencyAnalysis(MetaData metaData)
        {
            Total = metaData.GetOrThrow<ulong>(Consts.Total);
            MostFrequent = metaData.Get(Consts.MostFrequent) as string;
            NumDistinct = metaData.GetNullable<uint>(Consts.NumDistinct);
            Frequency = metaData.GetStringsWithPrefix(Consts.FrequencyPrefix)
                .Select(k => (Label: k[Consts.FrequencyPrefix.Length..], Value: metaData.GetOrThrow<double>(k)))
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
        public string? MostFrequent { get; }

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
