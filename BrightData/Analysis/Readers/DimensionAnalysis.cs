namespace BrightData.Analysis.Readers
{
    /// <summary>
    /// Dimension analysis results
    /// </summary>
    public class DimensionAnalysis
    {
        internal DimensionAnalysis(IMetaData metaData)
        {
            XDimension = metaData.GetNullable<uint>(Consts.XDimension);
            YDimension = metaData.GetNullable<uint>(Consts.YDimension);
            ZDimension = metaData.GetNullable<uint>(Consts.ZDimension);
            NumDistinct = metaData.GetNullable<uint>(Consts.NumDistinct);
            Size = metaData.Get<uint>(Consts.Size);
        }

        /// <summary>
        /// Max size of the x dimension
        /// </summary>
        public uint? XDimension { get; }

        /// <summary>
        /// Max size of the y dimension
        /// </summary>
        public uint? YDimension { get; }

        /// <summary>
        /// Max size of the z dimension
        /// </summary>
        public uint? ZDimension { get; }

        /// <summary>
        /// Number of distinct size combinations
        /// </summary>
        public uint? NumDistinct { get; }

        /// <summary>
        /// Total size across all dimensions
        /// </summary>
        public uint Size { get; }
    }
}
