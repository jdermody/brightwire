using System;

namespace BrightData.Analysis
{
    class CastToDoubleNumericAnalysis<T> : IDataAnalyser<T>
    {
        readonly TypeCode _typeCode;

        public CastToDoubleNumericAnalysis(uint writeCount = Consts.MaxWriteCount, uint maxCount = Consts.MaxDistinct)
        {
            _typeCode = Type.GetTypeCode(typeof(T));
            Analysis = new NumericAnalyser(writeCount, maxCount);
        }

        public NumericAnalyser Analysis { get; }

        public void Add(T obj)
        {
            if (_typeCode == TypeCode.SByte)
                Analysis.Add(__refvalue(__makeref(obj), sbyte));
            else if (_typeCode == TypeCode.Byte)
                Analysis.Add(__refvalue(__makeref(obj), byte));
            else if(_typeCode == TypeCode.Int16)
                Analysis.Add(__refvalue(__makeref(obj), short));
            else if (_typeCode == TypeCode.UInt16)
                Analysis.Add(__refvalue(__makeref(obj), ushort));
            else if(_typeCode == TypeCode.Int32)
                Analysis.Add(__refvalue(__makeref(obj), int));
            else if (_typeCode == TypeCode.UInt32)
                Analysis.Add(__refvalue(__makeref(obj), uint));
            else if (_typeCode == TypeCode.Int64)
                Analysis.Add(__refvalue(__makeref(obj), long));
            else if (_typeCode == TypeCode.UInt64)
                Analysis.Add(__refvalue(__makeref(obj), ulong));
            else if (_typeCode == TypeCode.Single)
                Analysis.Add(__refvalue(__makeref(obj), float));
            else if (_typeCode == TypeCode.Double)
                Analysis.Add(__refvalue(__makeref(obj), double));
            else if (_typeCode == TypeCode.Decimal)
                Analysis.Add((double)__refvalue(__makeref(obj), decimal));
            else
                throw new Exception("Data type was not numeric - double, float, int etc");
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
