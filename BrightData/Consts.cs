namespace BrightData
{
    /// <summary>
    /// Constants
    /// </summary>
    public class Consts
    {
        /// <summary>
        /// Default max distinct count
        /// </summary>
        public const uint MaxDistinct = 131072 * 4;

        /// <summary>
        /// Default max write count
        /// </summary>
        public const uint MaxWriteCount = 100;

        /// <summary>
        /// Default number of items to preview
        /// </summary>
        public const int DefaultPreviewSize = 16;

        /// <summary>
        /// Minimum size to use parallel processing
        /// </summary>
        public const int MinimumSizeForParallel = 8192;

        /// <summary>
        /// Minimum size to use SIMD processing
        /// </summary>
        public const int MinimumSizeForVectorised = 64;

        /// <summary>
        /// Default initial size of a block buffer
        /// </summary>
        public const int DefaultInitialBlockSize = 1_024;

        /// <summary>
        /// Default max size of a block buffer
        /// </summary>
        public const int DefaultMaxBlockSize = 32_768;

        /// <summary>
        /// Default maximum number of blocks to keep in memory
        /// </summary>
        public const int DefaultMaxBlocksInMemory = 4096;

        /// <summary>
        /// Default max write count
        /// </summary>
        public const uint MaxMetaDataWriteCount = 100;

        /// <summary>
        /// Maximum size in bytes to allocate data on the stack (via stackalloc)
        /// </summary>
        public const int MaxStackAllocSizeInBytes = 512;

#pragma warning disable 1591
        public const string ColumnIndex                      = "ColumnIndex";
        public const string Name                             = "Name";
        public const string Type                             = "Type";
        public const string IsNumeric                        = "IsNumeric";
        public const string IsTarget                         = "IsTarget";
        public const string IsCategorical                    = "IsCategorical";
        public const string IsOneHot                         = "IsOneHot";
        public const string IsSequential                     = "IsSequential";
        public const string HasBeenAnalysed                  = "HasBeenAnalysed";
        public const string Mode                             = "Mode";
        public const string MostFrequent                     = "MostFrequent";
        public const string NumDistinct                      = "NumDistinct";
        public const string MinDate                          = "MinDate";
        public const string MaxDate                          = "MaxDate";
        public const string MinIndex                         = "MinIndex";
        public const string MaxIndex                         = "MaxIndex";
        public const string MinLength                        = "MinLength";
        public const string MaxLength                        = "MaxLength";
        public const string XDimension                       = "XDimension";
        public const string YDimension                       = "YDimension";
        public const string ZDimension                       = "ZDimension";
        public const string CDimension                       = "CDimension";
        public const string Size                             = "Size";
        public const string L1Norm                           = "L1Norm";
        public const string L2Norm                           = "L2Norm";
        public const string Min                              = "Min";
        public const string Max                              = "Max";
        public const string Mean                             = "Mean";
        public const string SampleVariance                   = "SampleVariance";
        public const string SampleStdDev                     = "SampleStdDev";
        public const string PopulationVariance               = "PopulationVariance";
        public const string PopulationStdDev                 = "PopulationStdDev";
        public const string Median                           = "Median";
        public const string Total                            = "Total";
        public const string NormalizationType                = "NormalizationType";
        public const string NormalizationP1                  = "NormalizationP1";
        public const string NormalizationP2                  = "NormalizationP2";
        public const string FrequencyPrefix                  = "Frequency:";
        public const string FrequencyRangePrefix             = "FrequencyRange:";
        public const string CategoryPrefix                   = "Category:";
        public const string LegacyFloatSerialisationInput    = "LegacyFloatSerialisationInput";
        public const string Source                           = "Source";
        public const string FilePath                         = "FilePath";
        public const string BaseTempPath                     = "BaseTempPath";
        public const string ArrayBased                       = "ArrayBased";
        public const string MemoryOwnerBased                 = "MemoryOwnerBased";
        public const string MemoryBased                      = "MemoryBased";
        public const string DateTimeCreated                  = "DateTimeCreated";
        public const string CustomColumnReaders              = "CustomColumnReaders";
        public const string DefaultLinearAlgebraProviderName = "default";
        public const string VectorisationType                = "VectorisationType";
        public const string VectorisationSize                = "VectorisationSize";
        public const string VectorisationSourceColumnIndex   = "VectorisationSourceColumnIndex";
#pragma warning restore 1591
    }
}
