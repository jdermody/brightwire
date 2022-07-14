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

        public void Add(T obj) => Add(obj.ToString());
        public void WriteTo(MetaData metadata) => Analysis.WriteTo(metadata);
        public void AddObject(object obj) => Add(obj.ToString());

        void Add(string? str)
        {
            if(str != null)
                Analysis.Add(str);
        }
    }
}
