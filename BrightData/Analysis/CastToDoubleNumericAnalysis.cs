using System;
using System.Numerics;
using BrightData.Converter;
using BrightData.Types;

namespace BrightData.Analysis
{
    /// <summary>
    /// Used to convert other numeric types to doubles for numeric analysis
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ConvertToDoubleNumericAnalysis<T>(uint writeCount = Consts.MaxWriteCount) : IDataAnalyser<T>, INumericAnalysis<T>
        where T : unmanaged, INumber<T>
    {
        readonly ConvertToDouble<T> _converter = new();

        public NumericAnalyser<double> Analysis { get; } = new(writeCount);

        public void Add(T val)
        {
            Analysis.Add(_converter.Convert(val));
        }

        public void AddObject(object obj)
        {
            Add((T)obj);
        }

        public void Append(ReadOnlySpan<T> block)
        {
            foreach(ref readonly var item in block)
                Analysis.Add(_converter.Convert(item));
        }

        public void WriteTo(MetaData metadata)
        {
            Analysis.WriteTo(metadata);
        }

        public T L1Norm => T.CreateSaturating(Analysis.L1Norm);
        public T L2Norm => T.CreateSaturating(Analysis.L2Norm);
        public T Min => T.CreateSaturating(Analysis.Min);
        public T Max => T.CreateSaturating(Analysis.Max);
        public T Mean => T.CreateSaturating(Analysis.Mean);
        public T? SampleVariance => Analysis.SampleVariance.HasValue ? T.CreateSaturating(Analysis.SampleVariance.Value) : null;
        public T? PopulationVariance => Analysis.PopulationVariance.HasValue ? T.CreateSaturating(Analysis.PopulationVariance.Value) : null;
        public uint NumDistinct => Analysis.NumDistinct;
        public T? SampleStdDev => Analysis.SampleStdDev.HasValue ? T.CreateSaturating(Analysis.SampleStdDev.Value) : null;
        public T? PopulationStdDev => Analysis.PopulationStdDev.HasValue ? T.CreateSaturating(Analysis.PopulationStdDev.Value) : null;
        public ulong Count => Analysis.Count;
        public T? Median => Analysis.Median.HasValue ? T.CreateSaturating(Analysis.Median.Value) : null;
        public T? Mode => Analysis.Mode.HasValue ? T.CreateSaturating(Analysis.Mode.Value) : null;
    }
}
