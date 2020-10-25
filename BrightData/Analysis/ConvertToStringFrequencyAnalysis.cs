using System;
using System.Collections.Generic;
using System.Text;

namespace BrightData.Analysis
{
    public class ConvertToStringFrequencyAnalysis<T> : IDataAnalyser<T>
    {
        public ConvertToStringFrequencyAnalysis(int writeCount = 100)
        {
            Analysis = new StringAnalyser(writeCount);
        }

        public StringAnalyser Analysis { get; }

        public void Add(T obj) => Analysis.Add(obj.ToString());
        public void WriteTo(IMetaData metadata) => Analysis.WriteTo(metadata);
        public void AddObject(object obj) => Analysis.AddObject(obj);
    }
}
