namespace BrightData.Analysis.Readers
{
    /// <summary>
    /// String analysis results
    /// </summary>
    public class StringAnalysis : FrequencyAnalysis
    {
        internal StringAnalysis(MetaData metadata) : base(metadata)
        {
            MinLength = metadata.GetNullable<uint>(Consts.MinLength);
            MaxLength = metadata.GetNullable<uint>(Consts.MaxLength);
        }

        /// <summary>
        /// Length of smallest observed string
        /// </summary>
        public uint? MinLength { get; }

        /// <summary>
        /// Length of largest observed string
        /// </summary>
        public uint? MaxLength { get; }
    }
}
