﻿using System;
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
        /// <param name="maxCount">Maximum number of distinct items to track</param>
        /// <returns></returns>
        public static IDataAnalyser<DateTime> CreateDateAnalyser(uint maxCount = Consts.MaxDistinct) => new DateAnalyser(maxCount);

        /// <summary>
        /// Creates a numeric analyzer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="maxCount">Maximum number of distinct items to track</param>
        /// <param name="writeCount">Number of items to write in histogram</param>
        /// <returns></returns>
        public static IDataAnalyser<T> CreateNumericAnalyser<T>(uint maxCount = Consts.MaxDistinct, uint writeCount = Consts.MaxWriteCount) where T:struct => new CastToDoubleNumericAnalysis<T>(writeCount, maxCount);

        /// <summary>
        /// Creates an analyzer that will convert each item to a string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="maxCount"></param>
        /// <param name="writeCount"></param>
        /// <returns></returns>
        public static IDataAnalyser<T> CreateConvertToStringAnalyser<T>(uint maxCount = Consts.MaxDistinct, uint writeCount = Consts.MaxWriteCount) where T: notnull => new ConvertToStringFrequencyAnalysis<T>(writeCount, maxCount);

        /// <summary>
        /// Creates a dimension analyzer (to analyze the shape of tensors)
        /// </summary>
        /// <param name="maxCount"></param>
        /// <returns></returns>
        public static IDataAnalyser<ITensor<float>> CreateDimensionAnalyser(uint maxCount = Consts.MaxDistinct) => new DimensionAnalyser(maxCount);

        /// <summary>
        /// Creates an analyzer that tracks observed frequency of items
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="maxCount"></param>
        /// <param name="writeCount"></param>
        /// <returns></returns>
        public static IDataAnalyser<T> CreateFrequencyAnalyser<T>(uint maxCount = Consts.MaxDistinct, uint writeCount = Consts.MaxWriteCount) where T: notnull => new FrequencyAnalyser<T>(writeCount, maxCount);

        /// <summary>
        /// Creates an analyzer that tracks observed indices (for index lists and weighted index lists)
        /// </summary>
        /// <param name="maxCount"></param>
        /// <param name="writeCount"></param>
        /// <returns></returns>
        public static IDataAnalyser<IHaveIndices> CreateIndexAnalyser(uint maxCount = Consts.MaxDistinct, uint writeCount = Consts.MaxWriteCount) => new IndexAnalyser(writeCount, maxCount);

        /// <summary>
        /// Creates a numeric analyzer
        /// </summary>
        /// <param name="maxCount"></param>
        /// <param name="writeCount"></param>
        /// <returns></returns>
        public static IDataAnalyser<double> CreateNumericAnalyser(uint maxCount = Consts.MaxDistinct, uint writeCount = Consts.MaxWriteCount) => new NumericAnalyser(writeCount, maxCount);

        /// <summary>
        /// Creates a string analyzer
        /// </summary>
        /// <param name="maxCount"></param>
        /// <param name="writeCount"></param>
        /// <returns></returns>
        public static IDataAnalyser<string> CreateStringAnalyser(uint maxCount = Consts.MaxDistinct, uint writeCount = Consts.MaxWriteCount) => new StringAnalyser(writeCount, maxCount);

        /// <summary>
        /// Creates a frequency analyzer (each item will be converted to a string)
        /// </summary>
        /// <param name="type"></param>
        /// <param name="maxCount"></param>
        /// <param name="writeCount"></param>
        /// <returns></returns>
        public static IDataAnalyser CreateFrequencyAnalyser(Type type, uint maxCount = Consts.MaxDistinct, uint writeCount = Consts.MaxWriteCount) =>
            GenericActivator.Create<IDataAnalyser>(typeof(ConvertToStringFrequencyAnalysis<>).MakeGenericType(type), writeCount, maxCount);
    }
}
