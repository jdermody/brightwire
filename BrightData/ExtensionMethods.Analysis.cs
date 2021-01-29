using System;
using System.Collections.Generic;
using System.Text;
using BrightData.Analysis;
using BrightData.Analysis.Readers;
using BrightData.Transformation;

namespace BrightData
{
    /// <summary>
    /// Extension methods to attach analyser creation to the bright data context
    /// </summary>
    public static partial class ExtensionMethods
    {
        /// <summary>
        /// Creates a date analyzer
        /// </summary>
        /// <param name="context"></param>
        /// <param name="maxCount">Max distinct items</param>
        /// <returns></returns>
        public static IDataAnalyser<DateTime> GetDateAnalyser(this IBrightDataContext context, uint maxCount = Consts.MaxDistinct) =>
            StaticAnalysers.CreateDateAnalyser(maxCount);


        /// <summary>
        /// Creates a numeric analyzer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="maxCount">Max distinct items</param>
        /// <param name="writeCount">Maximum count to write to meta data</param>
        /// <returns></returns>
        public static IDataAnalyser<T> GetNumericAnalyser<T>(this IBrightDataContext context, uint maxCount = Consts.MaxDistinct, uint writeCount = Consts.MaxWriteCount) where T: struct =>
            StaticAnalysers.CreateNumericAnalyser<T>(maxCount, writeCount);

        /// <summary>
        /// Creates a frequency analyzer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="maxCount">Max distinct items</param>
        /// <param name="writeCount">Maximum count to write to meta data</param>
        /// <returns></returns>
        public static IDataAnalyser<T> GetConvertToStringAnalyser<T>(this IBrightDataContext context, uint maxCount = Consts.MaxDistinct, uint writeCount = Consts.MaxWriteCount) where T : notnull =>
            StaticAnalysers.CreateConvertToStringAnalyser<T>(maxCount, writeCount);

        /// <summary>
        /// Creates a dimension analyzer
        /// </summary>
        /// <param name="context"></param>
        /// <param name="maxCount">Max distinct items</param>
        /// <returns></returns>
        public static IDataAnalyser<ITensor<float>> GetDimensionAnalyser(this IBrightDataContext context, uint maxCount = Consts.MaxDistinct) =>
            StaticAnalysers.CreateDimensionAnalyser(maxCount);

        /// <summary>
        /// Creates a frequency analyzer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="maxCount">Max distinct items</param>
        /// <param name="writeCount">Maximum count to write to meta data</param>
        /// <returns></returns>
        public static IDataAnalyser<T> GetFrequencyAnalyser<T>(this IBrightDataContext context, uint maxCount = Consts.MaxDistinct, uint writeCount = Consts.MaxWriteCount) where T: notnull =>
            StaticAnalysers.CreateFrequencyAnalyser<T>(maxCount);

        /// <summary>
        /// Creates an index analyzer
        /// </summary>
        /// <param name="context"></param>
        /// <param name="maxCount">Max distinct items</param>
        /// <param name="writeCount">Maximum count to write to meta data</param>
        /// <returns></returns>
        public static IDataAnalyser<IHaveIndices> GetIndexAnalyser(this IBrightDataContext context, uint maxCount = Consts.MaxDistinct, uint writeCount = Consts.MaxWriteCount) =>
            StaticAnalysers.CreateIndexAnalyser(maxCount);

        /// <summary>
        /// Creates a numeric analyzer
        /// </summary>
        /// <param name="context"></param>
        /// <param name="maxCount">Max distinct items</param>
        /// <param name="writeCount">Maximum count to write to meta data</param>
        /// <returns></returns>
        public static IDataAnalyser<double> GetNumericAnalyser(this IBrightDataContext context, uint maxCount = Consts.MaxDistinct, uint writeCount = Consts.MaxWriteCount) =>
            StaticAnalysers.CreateNumericAnalyser(writeCount, maxCount);

        /// <summary>
        /// Creates a string analyzer
        /// </summary>
        /// <param name="context"></param>
        /// <param name="maxCount">Max distinct items</param>
        /// <param name="writeCount">Maximum count to write to meta data</param>
        /// <returns></returns>
        public static IDataAnalyser<string> GetStringAnalyser(this IBrightDataContext context, uint maxCount = Consts.MaxDistinct, uint writeCount = Consts.MaxWriteCount) =>
            StaticAnalysers.CreateStringAnalyser(writeCount, maxCount);

        /// <summary>
        /// Creates a frequency analyzer
        /// </summary>
        /// <param name="context"></param>
        /// <param name="type"></param>
        /// <param name="maxCount">Max distinct items</param>
        /// <param name="writeCount">Maximum count to write to meta data</param>
        /// <returns></returns>
        public static IDataAnalyser GetFrequencyAnalyser(this IBrightDataContext context, Type type, uint maxCount = Consts.MaxDistinct, uint writeCount = Consts.MaxWriteCount) =>
            StaticAnalysers.CreateFrequencyAnalyser(type, maxCount, writeCount);

        /// <summary>
        /// Gets the date analysis that was stored in meta data
        /// </summary>
        /// <param name="metaData">Meta data store</param>
        /// <returns></returns>
        public static DateAnalysis GetDateAnalysis(this IMetaData metaData) => new DateAnalysis(metaData);

        /// <summary>
        /// Gets the dimension analysis that was stored in meta data
        /// </summary>
        /// <param name="metaData">Meta data store</param>
        /// <returns></returns>
        public static DimensionAnalysis GetDimensionAnalysis(this IMetaData metaData) => new DimensionAnalysis(metaData);

        /// <summary>
        /// Gets the frequency analysis that was stored in meta data
        /// </summary>
        /// <param name="metaData">Meta data store</param>
        /// <returns></returns>
        public static FrequencyAnalysis GetFrequencyAnalysis(this IMetaData metaData) => new FrequencyAnalysis(metaData);

        /// <summary>
        /// Gets the index analysis that was stored in meta data
        /// </summary>
        /// <param name="metaData">Meta data store</param>
        /// <returns></returns>
        public static IndexAnalysis GetIndexAnalysis(this IMetaData metaData) => new IndexAnalysis(metaData);

        /// <summary>
        /// Gets the numeric analysis that was stored in meta data
        /// </summary>
        /// <param name="metaData">Meta data store</param>
        /// <returns></returns>
        public static NumericAnalysis GetNumericAnalysis(this IMetaData metaData) => new NumericAnalysis(metaData);

        /// <summary>
        /// Gets the string analysis that was stored in meta data
        /// </summary>
        /// <param name="metaData">Meta data store</param>
        /// <returns></returns>
        public static StringAnalysis GetStringAnalysis(this IMetaData metaData) => new StringAnalysis(metaData);

        /// <summary>
        /// Gets the categories that was stored in meta data
        /// </summary>
        /// <param name="metaData">Meta data store</param>
        /// <returns></returns>
        public static DictionaryValues GetDictionaryValues(this IMetaData metaData) => new DictionaryValues(metaData);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="metaData">Meta data store</param>
        /// <returns></returns>
        public static NormalizeTransformation GetNormalization(this IMetaData metaData) => new NormalizeTransformation(metaData);

        /// <summary>
        /// Analyzes numbers in a sequence
        /// </summary>
        /// <param name="data"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static NumericAnalysis Analyze<T>(this IEnumerable<T> data)
            where T : struct
        {
            var analysis = new CastToDoubleNumericAnalysis<T>();
            foreach (var item in data)
                analysis.Add(item);
            return analysis.GetMetaData().GetNumericAnalysis();
        }

        /// <summary>
        /// Analyzes dates in a sequence
        /// </summary>
        /// <param name="dates"></param>
        /// <returns></returns>
        public static DateAnalysis Analyze(this IEnumerable<DateTime> dates)
        {
            var analysis = new DateAnalyser();
            foreach (var item in dates)
                analysis.Add(item);
            return analysis.GetMetaData().GetDateAnalysis();
        }

        /// <summary>
        /// Analyzes tensors in a sequence
        /// </summary>
        /// <param name="tensors"></param>
        /// <returns></returns>
        public static DimensionAnalysis Analyze(this IEnumerable<ITensor<float>> tensors)
        {
            var analysis = new DimensionAnalyser();
            foreach (var item in tensors)
                analysis.Add(item);
            return analysis.GetMetaData().GetDimensionAnalysis();
        }

        /// <summary>
        /// Analyzes indices in a sequence
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public static IndexAnalysis Analyze(this IEnumerable<IHaveIndices> items)
        {
            var analysis = new IndexAnalyser();
            foreach (var item in items)
                analysis.Add(item);
            return analysis.GetMetaData().GetIndexAnalysis();
        }

        /// <summary>
        /// Analyzes a sequence of strings
        /// </summary>
        /// <param name="items"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static StringAnalysis Analyze<T>(this IEnumerable<string> items)
        {
            var analysis = new StringAnalyser();
            foreach (var item in items)
                analysis.Add(item);
            return analysis.GetMetaData().GetStringAnalysis();
        }

        /// <summary>
        /// Analyzes the frequency of items
        /// </summary>
        /// <param name="items"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static FrequencyAnalysis AnalyzeFrequency<T>(this IEnumerable<T> items)
        {
            var analysis = new FrequencyAnalyser<T>();
            foreach (var item in items)
                analysis.Add(item);
            return analysis.GetMetaData().GetFrequencyAnalysis();
        }
    }
}
