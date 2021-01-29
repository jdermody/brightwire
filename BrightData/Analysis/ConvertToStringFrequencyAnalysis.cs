namespace BrightData.Analysis
{
    internal class ConvertToStringFrequencyAnalysis<T> : IDataAnalyser<T> 
        where T: notnull
    {
        public ConvertToStringFrequencyAnalysis(uint writeCount = Consts.MaxWriteCount, uint maxCount = Consts.MaxDistinct)
        {
            Analysis = new StringAnalyser(writeCount, maxCount);
        }

        public StringAnalyser Analysis { get; }

        public void Add(T obj) => _Add(obj.ToString());
        public void WriteTo(IMetaData metadata) => Analysis.WriteTo(metadata);
        public void AddObject(object obj) => _Add(obj.ToString());

        void _Add(string? str)
        {
            if(str != null)
                Analysis.Add(str);
        }
    }
}
