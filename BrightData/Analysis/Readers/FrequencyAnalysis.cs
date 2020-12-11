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
            MostFrequent = metaData.Get<string>(Consts.MostFrequent);
            NumDistinct = metaData.GetNullable<uint>(Consts.NumDistinct);
            Frequency = metaData.GetStringsWithPrefix(Consts.FrequencyPrefix)
                .Select(k => (Label: k.Substring(Consts.FrequencyPrefix.Length), Value: metaData.Get<double>(k)))
                .ToArray()
            ;
        }

        public string MostFrequent { get; }
        public uint? NumDistinct { get; }
        public (string Label, double value)[] Frequency { get; }
    }
}
