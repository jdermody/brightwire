using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightData.Analysis.Readers
{
    public class FrequencyAnalysis
    {
        public FrequencyAnalysis(IMetaData metaData)
        {
            Mode = metaData.Get<string>(Consts.Mode);
            NumDistinct = metaData.GetNullable<uint>(Consts.NumDistinct);
            Frequency = metaData.GetStringsWithPrefix(Consts.FrequencyPrefix)
                .Select(k => (Label: k.Substring(Consts.FrequencyPrefix.Length), Value: metaData.Get<double>(k)))
                .ToArray()
            ;
        }

        public string Mode { get; }
        public uint? NumDistinct { get; }
        public (string Label, double value)[] Frequency { get; }
    }
}
