using System;
using System.Collections.Generic;
using System.Text;

namespace BrightData.Analysis.Readers
{
    public class StringAnalysis : FrequencyAnalysis
    {
        public StringAnalysis(IMetaData metadata) : base(metadata)
        {
            MinLength = metadata.Get<uint>(Consts.MinLength);
            MaxLength = metadata.Get<uint>(Consts.MaxLength);
        }

        public uint MinLength { get; }
        public uint MaxLength { get; }
    }
}
