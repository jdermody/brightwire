using BrightData.Converter;

namespace BrightData.Analysis
{
    /// <summary>
    /// Used to cast other numeric types to doubles for numeric analysis
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class CastToDoubleNumericAnalysis<T> : IDataAnalyser<T> where T : struct
    {
        readonly ConvertToDouble<T> _converter = new();

        public CastToDoubleNumericAnalysis(uint writeCount = Consts.MaxWriteCount, uint maxCount = Consts.MaxDistinct)
        {
            Analysis = new NumericAnalyser(writeCount, maxCount);
        }

        public NumericAnalyser Analysis { get; }

        public void Add(T val)
        {
            Analysis.Add(_converter.Convert(val));
        }

        public void AddObject(object obj)
        {
            Add((T)obj);
        }

        public void WriteTo(MetaData metadata)
        {
            Analysis.WriteTo(metadata);
        }
    }
}
