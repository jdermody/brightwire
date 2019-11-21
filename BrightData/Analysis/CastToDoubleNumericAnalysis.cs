using System;

namespace BrightData.Analysis
{
    public class CastToDoubleNumericAnalysis<T> : IDataAnalyser<T>
    {
        readonly TypeCode _typeCode;

        public CastToDoubleNumericAnalysis(int writeCount = 100)
        {
            _typeCode = Type.GetTypeCode(typeof(T));
            Analysis = new NumericAnalyser(writeCount);
        }

        public NumericAnalyser Analysis { get; }

        public void Add(T obj)
        {
            if (_typeCode == TypeCode.SByte)
                Analysis.Add(__refvalue(__makeref(obj), sbyte));
            else if(_typeCode == TypeCode.Int16)
                Analysis.Add(__refvalue(__makeref(obj), short));
            else if(_typeCode == TypeCode.Int32)
                Analysis.Add(__refvalue(__makeref(obj), int));
            else if (_typeCode == TypeCode.Int64)
                Analysis.Add(__refvalue(__makeref(obj), long));
            else if (_typeCode == TypeCode.Single)
                Analysis.Add(__refvalue(__makeref(obj), float));
            else if (_typeCode == TypeCode.Decimal)
                Analysis.Add((double)__refvalue(__makeref(obj), decimal));
            else
                throw new NotImplementedException();
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
