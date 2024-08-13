namespace BrightData
{
    /// <summary>
    /// Typed data analyser
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDataAnalyser<T> : IAppendBlocks<T>, IDataAnalyser where T : notnull
    {
        /// <summary>
        /// Adds a typed object
        /// </summary>
        /// <param name="obj"></param>
        void Add(T obj);
    }

    /// <summary>
    /// Standard deviation analysis
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IStandardDeviationAnalysis<T> where T : unmanaged
    {
        /// <summary>
        /// Numerical mean
        /// </summary>
        T Mean { get; }

        /// <summary>
        /// Sample variance
        /// </summary>
        T? SampleVariance { get; }

        /// <summary>
        /// Population variance
        /// </summary>
        T? PopulationVariance { get; }

        /// <summary>
        /// Sample standard deviation
        /// </summary>
        T? SampleStdDev { get; }

        /// <summary>
        /// Population standard deviation
        /// </summary>
        T? PopulationStdDev { get; }

        /// <summary>
        /// Count of items that were analysed
        /// </summary>
        ulong Count { get; }
    }

    /// <summary>
    /// Numerical analysis
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface INumericAnalysis<T> : IStandardDeviationAnalysis<T>
        where T: unmanaged
    {
        /// <summary>
        /// L1 norm
        /// </summary>
        T L1Norm { get; }

        /// <summary>
        /// L2 norm
        /// </summary>
        T L2Norm { get; }

        /// <summary>
        /// Minimum number
        /// </summary>
        T Min { get; }

        /// <summary>
        /// Maximum number
        /// </summary>
        T Max { get; }

        /// <summary>
        /// Number of unique numbers
        /// </summary>
        uint NumDistinct { get; }

        /// <summary>
        /// Median
        /// </summary>
        T? Median { get; }

        /// <summary>
        /// Mode
        /// </summary>
        T? Mode { get; }
    }

    internal interface ISimpleNumericAnalysis : IOperation
    {
        public bool IsInteger { get; }
        public uint NanCount { get; }
        public uint InfinityCount { get; }
        public double MinValue { get; }
        public double MaxValue { get; }
    }
}
