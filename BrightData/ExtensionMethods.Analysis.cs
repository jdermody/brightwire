using System;
using System.Collections.Generic;
using BrightData.Analysis;
using BrightData.Analysis.Readers;
using BrightData.DataTable;
using BrightData.Transformation;

namespace BrightData
{
    /// <summary>
    /// Extension methods to attach analyser creation to the bright data context
    /// </summary>
    public static partial class ExtensionMethods
    {
        /// <summary>
        /// Gets the date analysis that was stored in meta data
        /// </summary>
        /// <param name="metaData">Meta data store</param>
        /// <returns></returns>
        public static DateAnalysis GetDateAnalysis(this MetaData metaData) => new(metaData);

        /// <summary>
        /// Gets the dimension analysis that was stored in meta data
        /// </summary>
        /// <param name="metaData">Meta data store</param>
        /// <returns></returns>
        public static DimensionAnalysis GetDimensionAnalysis(this MetaData metaData) => new(metaData);

        /// <summary>
        /// Gets the frequency analysis that was stored in meta data
        /// </summary>
        /// <param name="metaData">Meta data store</param>
        /// <returns></returns>
        public static FrequencyAnalysis GetFrequencyAnalysis(this MetaData metaData) => new(metaData);

        /// <summary>
        /// Gets the index analysis that was stored in meta data
        /// </summary>
        /// <param name="metaData">Meta data store</param>
        /// <returns></returns>
        public static IndexAnalysis GetIndexAnalysis(this MetaData metaData) => new(metaData);

        /// <summary>
        /// Gets the numeric analysis that was stored in meta data
        /// </summary>
        /// <param name="metaData">Meta data store</param>
        /// <returns></returns>
        public static NumericAnalysis GetNumericAnalysis(this MetaData metaData) => new(metaData);

        /// <summary>
        /// Gets the string analysis that was stored in meta data
        /// </summary>
        /// <param name="metaData">Meta data store</param>
        /// <returns></returns>
        public static StringAnalysis GetStringAnalysis(this MetaData metaData) => new(metaData);

        /// <summary>
        /// Gets the categories that were stored in meta data
        /// </summary>
        /// <param name="metaData">Meta data store</param>
        /// <returns></returns>
        public static DictionaryValues GetDictionaryValues(this MetaData metaData) => new(metaData);

        /// <summary>
        /// Returns a normalization that was previously stored in the metadata
        /// </summary>
        /// <param name="metaData">Meta data store</param>
        /// <returns></returns>
        public static NormalizeTransformation GetNormalization(this MetaData metaData) => new(metaData);

        /// <summary>
        /// Returns the normalization that was applied to the specified data table column
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="columnIndex">Column index to retrieve</param>
        /// <returns></returns>
        public static NormalizeTransformation GetColumnNormalization(this BrightDataTable dataTable, uint columnIndex) => dataTable.ColumnMetaData[columnIndex].GetNormalization();

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
            var analysis = new DateTimeAnalyser();
            foreach (var item in dates)
                analysis.Add(item);
            return analysis.GetMetaData().GetDateAnalysis();
        }

        /// <summary>
        /// Analyzes tensors in a sequence
        /// </summary>
        /// <param name="tensors"></param>
        /// <returns></returns>
        public static DimensionAnalysis Analyze(this IEnumerable<ITensor> tensors)
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
        /// <returns></returns>
        public static StringAnalysis Analyze(this IEnumerable<string> items)
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
        public static FrequencyAnalysis AnalyzeFrequency<T>(this IEnumerable<T> items) where T : notnull
        {
            var analysis = new FrequencyAnalyser<T>();
            foreach (var item in items)
                analysis.Add(item);
            return analysis.GetMetaData().GetFrequencyAnalysis();
        }
    }
}
