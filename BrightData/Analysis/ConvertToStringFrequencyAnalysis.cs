using System;
using BrightData.Types;

namespace BrightData.Analysis
{
    /// <summary>
    /// Used to convert other types to strings for frequency analysis
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ConvertToStringFrequencyAnalysis<T>(uint writeCount = Consts.MaxWriteCount) : IDataAnalyser<T>
        where T : notnull
    {
        public StringAnalyser Analysis { get; } = new(writeCount);

        public void Add(T obj) => Add(obj.ToString());
        public void WriteTo(MetaData metadata) => Analysis.WriteTo(metadata);
        public void AddObject(object obj) => Add(obj.ToString());

        public void Append(ReadOnlySpan<T> block)
        {
            foreach(ref readonly var item in block)
                Analysis.Add(item.ToString());
        }

        void Add(string? str)
        {
            if(str != null)
                Analysis.Add(str);
        }
    }
}
