using System;
using System.Numerics;
using BrightData.Helper;

namespace BrightData.Analysis
{
    /// <summary>
    /// Static methods to create analyzers
    /// </summary>
    public static class StaticAnalysers
    {
        /// <summary>
        /// Creates a date analyzer
        /// </summary>
        /// <returns></returns>
        public static IDataAnalyser<DateTime> CreateDateAnalyser() => new DateTimeAnalyser();

        /// <summary>
        /// Creates a numeric analyzer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="writeCount">Number of items to write in histogram</param>
        /// <returns></returns>
        public static IDataAnalyser<T> CreateNumericAnalyser<T>(uint writeCount = Consts.MaxWriteCount) where T: unmanaged, IMinMaxValue<T>, IBinaryFloatingPointIeee754<T>, IConvertible => new NumericAnalyser<T>(writeCount);

        public static IDataAnalyser<T> CreateNumericAnalyserCastToDouble<T>(uint writeCount = Consts.MaxWriteCount) where T: unmanaged, INumber<T> => new CastToDoubleNumericAnalysis<T>(writeCount);

        /// <summary>
        /// Creates an analyzer that will convert each item to a string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="writeCount"></param>
        /// <returns></returns>
        public static IDataAnalyser<T> CreateConvertToStringAnalyser<T>(uint writeCount = Consts.MaxWriteCount) where T : notnull => new ConvertToStringFrequencyAnalysis<T>(writeCount);

        /// <summary>
        /// Creates a dimension analyzer (to analyze the shape of tensors)
        /// </summary>
        /// <returns></returns>
        public static IDataAnalyser<IReadOnlyTensor<float>> CreateDimensionAnalyser() => new DimensionAnalyser();

        /// <summary>
        /// Creates an analyzer that tracks observed frequency of items
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="writeCount"></param>
        /// <returns></returns>
        public static IDataAnalyser<T> CreateFrequencyAnalyser<T>(uint writeCount = Consts.MaxWriteCount) where T: notnull => new FrequencyAnalyser<T>(writeCount);

        /// <summary>
        /// Creates an analyzer that tracks observed indices (for index lists and weighted index lists)
        /// </summary>
        /// <param name="writeCount"></param>
        /// <returns></returns>
        public static IDataAnalyser<IHaveIndices> CreateIndexAnalyser(uint writeCount = Consts.MaxWriteCount) => new IndexAnalyser(writeCount);

        /// <summary>
        /// Creates a numeric analyzer
        /// </summary>
        /// <param name="writeCount"></param>
        /// <returns></returns>
        public static IDataAnalyser<double> CreateNumericAnalyser(uint writeCount = Consts.MaxWriteCount) => new NumericAnalyser<double>(writeCount);

        /// <summary>
        /// Creates a string analyzer
        /// </summary>
        /// <param name="writeCount"></param>
        /// <returns></returns>
        public static IDataAnalyser<string> CreateStringAnalyser(uint writeCount = Consts.MaxWriteCount) => new StringAnalyser(writeCount);

        /// <summary>
        /// Creates a frequency analyzer (each item will be converted to a string)
        /// </summary>
        /// <param name="type"></param>
        /// <param name="writeCount"></param>
        /// <returns></returns>
        public static IDataAnalyser CreateFrequencyAnalyser(Type type, uint writeCount = Consts.MaxWriteCount) =>
            GenericTypeMapping.ConvertToStringFrequencyAnalysis(type, writeCount);
    }
}
