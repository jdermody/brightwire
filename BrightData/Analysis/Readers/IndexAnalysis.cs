using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightData.Analysis.Readers
{
    public class IndexAnalysis
    {
        public IndexAnalysis(IMetaData metaData)
        {
            MinIndex = metaData.Get<uint>(Consts.MinIndex);
            MaxIndex = metaData.Get<uint>(Consts.MaxIndex);
            NumDistinct = metaData.GetNullable<uint>(Consts.NumDistinct);
            Frequency = metaData.GetStringsWithPrefix(Consts.FrequencyPrefix)
                .Select(k => (Label: k.Substring(Consts.FrequencyPrefix.Length), Value: metaData.Get<double>(k)))
                .ToArray()
            ;
        }

        public uint MinIndex { get; }
        public uint MaxIndex { get; }
        public uint? NumDistinct { get; }
        public (string Label, double value)[] Frequency { get; }
    }
}
