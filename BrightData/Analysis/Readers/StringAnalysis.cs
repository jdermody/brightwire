using System;
using System.Collections.Generic;
using System.Text;

namespace BrightData.Analysis.Readers
{
    /// <summary>
    /// String analysis results
    /// </summary>
    public class StringAnalysis : FrequencyAnalysis
    {
        internal StringAnalysis(IMetaData metadata) : base(metadata)
        {
            MinLength = metadata.Get<uint>(Consts.MinLength);
            MaxLength = metadata.Get<uint>(Consts.MaxLength);
        }

        /// <summary>
        /// Length of smallest observed string
        /// </summary>
        public uint MinLength { get; }

        /// <summary>
        /// Length of largest observed string
        /// </summary>
        public uint MaxLength { get; }
    }
}
