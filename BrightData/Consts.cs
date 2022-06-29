namespace BrightData
{
    /// <summary>
    /// Constants
    /// </summary>
    public class Consts
    {
        /// <summary>
        /// Data table version
        /// </summary>
        public const int DataTableVersion = 1;

        /// <summary>
        /// Default max distinct count
        /// </summary>
        public const uint MaxDistinct = 131072 * 4;

        /// <summary>
        /// Default max write count
        /// </summary>
        public const uint MaxWriteCount = 100;

        /// <summary>
        /// Default memory cache size
        /// </summary>
        //public const uint DefaultMemoryCacheSize = 1024 * 1048576;

        /// <summary>
        /// Number of rows to process before notifying about progress
        /// </summary>
        public const uint RowProcessingNotificationCadence = 1000;

#pragma warning disable 1591
        public const string ColumnIndex                         = "ColumnIndex";
        public const string Name                          = "Name";
        public const string Type                          = "Type";
        public const string IsNumeric                     = "IsNumeric";
        public const string IsTarget                      = "IsTarget";
        public const string IsCategorical                 = "IsCategorical";
        public const string IsSequential                  = "IsSequential";
        public const string HasBeenAnalysed               = "HasBeenAnalysed";
        public const string Mode                          = "Mode";
        public const string MostFrequent                  = "MostFrequent";
        public const string NumDistinct                   = "NumDistinct";
        public const string MinDate                       = "MinDate";
        public const string MaxDate                       = "MaxDate";
        public const string MinIndex                      = "MinIndex";
        public const string MaxIndex                      = "MaxIndex";
        public const string MinLength                     = "MinLength";
        public const string MaxLength                     = "MaxLength";
        public const string XDimension                    = "XDimension";
        public const string YDimension                    = "YDimension";
        public const string ZDimension                    = "ZDimension";
        public const string Size                          = "Size";
        public const string L1Norm                        = "L1Norm";
        public const string L2Norm                        = "L2Norm";
        public const string Min                           = "Min";
        public const string Max                           = "Max";
        public const string Mean                          = "Mean";
        public const string SampleVariance                = "SampleVariance";
        public const string SampleStdDev                  = "SampleStdDev";
        public const string PopulationVariance            = "PopulationVariance";
        public const string PopulationStdDev              = "PopulationStdDev";
        public const string Median                        = "Median";
        public const string Total                         = "Total";
        public const string NormalizationType             = "NormalizationType";
        public const string NormalizationP1               = "NormalizationP1";
        public const string NormalizationP2               = "NormalizationP2";
        public const string FrequencyPrefix               = "Frequency:";
        public const string FrequencyRangePrefix          = "FrequencyRange:";
        public const string CategoryPrefix                = "Category:";
        public const string LegacyFloatSerialisationInput = "LegacyFloatSerialisationInput";
        public const string Source                        = "Source";
        public const string FilePath                      = "FilePath";
        public const string BaseTempPath                  = "BaseTempPath";
        public const string ArrayBased                    = "array based";
#pragma warning restore 1591

        /// <summary>
        /// Standard metadata
        /// </summary>
        public static readonly string[] SimpleMetaData = { ColumnIndex, Name, IsTarget };

        /// <summary>
        /// Standard metadata
        /// </summary>
        public static readonly string[] StandardMetaData = { ColumnIndex, Name, Type, IsNumeric, IsTarget };
    }
}
