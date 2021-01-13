using System.Linq;

namespace BrightData.Analysis.Readers
{
    /// <summary>
    /// Numeric analysis results
    /// </summary>
    public class NumericAnalysis
    {
        internal NumericAnalysis(IMetaData metaData)
        {
            L1Norm = metaData.Get<double>(Consts.L1Norm);
            L2Norm = metaData.Get<double>(Consts.L2Norm);
            Min = metaData.Get<double>(Consts.Min);
            Max = metaData.Get<double>(Consts.Max);
            Mean = metaData.Get<double>(Consts.Mean);
            Variance = metaData.GetNullable<double>(Consts.Variance);
            StdDev = metaData.GetNullable<double>(Consts.StdDev);
            Median = metaData.GetNullable<double>(Consts.Median);
            Mode = metaData.GetNullable<double>(Consts.Mode);
            Total = metaData.Get<ulong>(Consts.Total);
            NumDistinct = metaData.GetNullable<uint>(Consts.NumDistinct);
            Frequency = _Get(Consts.FrequencyPrefix, metaData);
            FrequencyRange = _Get(Consts.FrequencyRangePrefix, metaData);
        }

        (string Label, double value)[] _Get(string prefix, IMetaData metaData)
        {
            return metaData.GetStringsWithPrefix(prefix)
                .Select(k => (Label: k.Substring(prefix.Length), Value: metaData.Get<double>(k)))
                .ToArray();
        }

        /// <summary>
        /// L1 Norm
        /// </summary>
        public double L1Norm { get; }

        /// <summary>
        /// L2 Norm
        /// </summary>
        public double L2Norm { get; }

        /// <summary>
        /// Minimum observed value
        /// </summary>
        public double Min { get; }

        /// <summary>
        /// Maximum observed value
        /// </summary>
        public double Max { get; }

        /// <summary>
        /// Mean of values
        /// </summary>
        public double Mean { get; }

        /// <summary>
        /// Sample variance of values
        /// </summary>
        public double? Variance { get; }

        /// <summary>
        /// Sample standard deviation
        /// </summary>
        public double? StdDev { get; }

        /// <summary>
        /// Median value
        /// </summary>
        public double? Median { get; }

        /// <summary>
        /// Mode (most frequent value)
        /// </summary>
        public double? Mode { get; }

        /// <summary>
        /// Number of distinct values
        /// </summary>
        public uint? NumDistinct { get; }

        /// <summary>
        /// Total count of items
        /// </summary>
        public ulong Total { get; }

        /// <summary>
        /// Ranked histogram
        /// </summary>
        public (string Label, double value)[] Frequency { get; }

        /// <summary>
        /// Bucketed histogram
        /// </summary>
        public (string Label, double value)[] FrequencyRange { get; }
    }
}
