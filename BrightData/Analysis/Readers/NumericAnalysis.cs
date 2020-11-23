using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightData.Analysis.Readers
{
    public class NumericAnalysis
    {
        public NumericAnalysis(IMetaData metaData)
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

        public double L1Norm { get; }
        public double L2Norm { get; }
        public double Min { get; }
        public double Max { get; }
        public double Mean { get; }
        public double? Variance { get; }
        public double? StdDev { get; }
        public double? Median { get; }
        public double? Mode { get; }
        public uint? NumDistinct { get; }
        public (string Label, double value)[] Frequency { get; }
        public (string Label, double value)[] FrequencyRange { get; }
    }
}
