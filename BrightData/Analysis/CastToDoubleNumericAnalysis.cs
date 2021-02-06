using BrightData.Converter;

namespace BrightData.Analysis
{
    internal class CastToDoubleNumericAnalysis<T> : IDataAnalyser<T> where T : struct
    {
        readonly ConvertToDouble<T> _converter = new ConvertToDouble<T>();

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

        public void WriteTo(IMetaData metadata)
        {
            Analysis.WriteTo(metadata);
        }
    }
}
