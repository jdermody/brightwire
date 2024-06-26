﻿using System;
using BrightData.Converter;
using BrightData.Types;

namespace BrightData.Analysis
{
    /// <summary>
    /// Used to cast other numeric types to doubles for numeric analysis
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class CastToDoubleNumericAnalysis<T>(uint writeCount = Consts.MaxWriteCount) : IDataAnalyser<T>
        where T : struct
    {
        readonly ConvertToDouble<T> _converter = new();

        public NumericAnalyser Analysis { get; } = new(writeCount);

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
    }
}
