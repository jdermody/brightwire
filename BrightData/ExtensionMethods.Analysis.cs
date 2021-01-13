using System;
using System.Collections.Generic;
using System.Text;
using BrightData.Analysis;
using BrightData.Analysis.Readers;

namespace BrightData
{
    /// <summary>
    /// Extension methods to attach analyser creation to the bright data context
    /// </summary>
    public static partial class ExtensionMethods
    {
        public static IDataAnalyser<DateTime> GetDateAnalyser(this IBrightDataContext context, uint maxCount = Consts.MaxDistinct) =>
            StaticAnalysers.CreateDateAnalyser(maxCount);
        public static IDataAnalyser<T> GetNumericAnalyser<T>(this IBrightDataContext context, uint maxCount = Consts.MaxDistinct, uint writeCount = Consts.MaxWriteCount) where T: struct =>
            StaticAnalysers.CreateNumericAnalyser<T>(maxCount, writeCount);
        public static IDataAnalyser<T> GetConvertToStringAnalyser<T>(this IBrightDataContext context, uint maxCount = Consts.MaxDistinct, uint writeCount = Consts.MaxWriteCount) =>
            StaticAnalysers.CreateConvertToStringAnalyser<T>(maxCount, writeCount);
        public static IDataAnalyser<ITensor<float>> GetDimensionAnalyser(this IBrightDataContext context, uint maxCount = Consts.MaxDistinct) =>
            StaticAnalysers.CreateDimensionAnalyser(maxCount);
        public static IDataAnalyser<T> GetFrequencyAnalyser<T>(this IBrightDataContext context, uint maxCount = Consts.MaxDistinct, uint writeCount = Consts.MaxWriteCount) =>
            StaticAnalysers.CreateFrequencyAnalyser<T>(maxCount);
        public static IDataAnalyser<IHaveIndices> GetIndexAnalyser(this IBrightDataContext context, uint maxCount = Consts.MaxDistinct, uint writeCount = Consts.MaxWriteCount) =>
            StaticAnalysers.CreateIndexAnalyser(maxCount);
        public static IDataAnalyser<double> GetNumericAnalyser(this IBrightDataContext context, uint maxCount = Consts.MaxDistinct, uint writeCount = Consts.MaxWriteCount) =>
            StaticAnalysers.CreateNumericAnalyser(writeCount, maxCount);
        public static IDataAnalyser<string> GetStringAnalyser(this IBrightDataContext context, uint maxCount = Consts.MaxDistinct, uint writeCount = Consts.MaxWriteCount) =>
            StaticAnalysers.CreateStringAnalyser(writeCount, maxCount);
        public static IDataAnalyser GetFrequencyAnalyser(this IBrightDataContext context, Type type, uint maxCount = Consts.MaxDistinct, uint writeCount = Consts.MaxWriteCount) =>
            StaticAnalysers.CreateFrequencyAnalyser(type, maxCount, writeCount);
    }
}
