﻿using System.Linq;
using BrightData.Types;

namespace BrightData.Analysis.Readers
{
    /// <summary>
    /// Numeric analysis results
    /// </summary>
    public class NumericAnalysis : INumericAnalysis<double>
    {
        internal NumericAnalysis(MetaData metaData)
        {
            L1Norm             = metaData.GetOrThrow<double>(Consts.L1Norm);
            L2Norm             = metaData.GetOrThrow<double>(Consts.L2Norm);
            Min                = metaData.GetOrThrow<double>(Consts.Min);
            Max                = metaData.GetOrThrow<double>(Consts.Max);
            Mean               = metaData.GetOrThrow<double>(Consts.Mean);
            SampleVariance     = metaData.GetNullable<double>(Consts.SampleVariance);
            SampleStdDev       = metaData.GetNullable<double>(Consts.SampleStdDev);
            PopulationVariance = metaData.GetNullable<double>(Consts.PopulationVariance);
            PopulationStdDev   = metaData.GetNullable<double>(Consts.PopulationStdDev);
            Median             = metaData.GetNullable<double>(Consts.Median);
            Mode               = metaData.GetNullable<double>(Consts.Mode);
            Total              = metaData.GetOrThrow<ulong>(Consts.Total);
            NumDistinct        = metaData.GetNullable<uint>(Consts.NumDistinct);
            Frequency          = Get(Consts.FrequencyPrefix, metaData);
            FrequencyRange     = Get(Consts.FrequencyRangePrefix, metaData);
        }

        static (string Label, double value)[] Get(string prefix, MetaData metaData)
        {
            return [
                ..metaData.GetStringsWithPrefix(prefix)
                .Select(k => (Label: k[prefix.Length..], Value: metaData.GetOrThrow<double>(k)))
            ];
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
        public double? SampleVariance { get; }

        /// <summary>
        /// Sample standard deviation
        /// </summary>
        public double? SampleStdDev { get; }

        /// <summary>
        /// Population variance of values
        /// </summary>
        public double? PopulationVariance { get; }

        /// <summary>
        /// Population standard deviation
        /// </summary>
        public double? PopulationStdDev { get; }

        /// <inheritdoc />
        public ulong Count => Total;

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
        public uint NumDistinct { get; }

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
