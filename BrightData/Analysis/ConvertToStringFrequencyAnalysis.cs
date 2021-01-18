namespace BrightData.Analysis
{
    internal class ConvertToStringFrequencyAnalysis<T> : IDataAnalyser<T>
    {
        public ConvertToStringFrequencyAnalysis(uint writeCount = Consts.MaxWriteCount, uint maxCount = Consts.MaxDistinct)
        {
            Analysis = new StringAnalyser(writeCount, maxCount);
        }

        public StringAnalyser Analysis { get; }

        public void Add(T obj) => Analysis.Add(obj.ToString());
        public void WriteTo(IMetaData metadata) => Analysis.WriteTo(metadata);
        public void AddObject(object obj) => Analysis.AddObject(obj.ToString());
    }
}
