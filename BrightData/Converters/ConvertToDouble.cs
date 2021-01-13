using System;

namespace BrightData.Converters
{
    class ConvertToDouble<T> : ConverterBase<T>, ICanConvert<T, double>
        where T : struct
    {
        readonly Func<T, double> _converter;

        public ConvertToDouble(bool throwOnFailure = false) : base(throwOnFailure)
        {
            var typeCode = Type.GetTypeCode(typeof(T));
            _converter = typeCode switch
            {
                TypeCode.Single => _FromSingle,
                TypeCode.Double => _GetDouble,
                TypeCode.SByte => _FromSByte,
                TypeCode.Byte => _FromByte,
                TypeCode.Int16 => _FromInt16,
                TypeCode.UInt16 => _FromUInt16,
                TypeCode.Int32 => _FromInt32,
                TypeCode.UInt32 => _FromUInt32,
                TypeCode.Int64 => _FromInt64,
                TypeCode.UInt64 => _FromUInt64,
                TypeCode.Decimal => _FromDecimal,
                _ => _ConvertGeneric,
            };
        }

        double _FromSingle(T data) => System.Convert.ToDouble(_GetSingle(data));
        double _FromDecimal(T data) => System.Convert.ToDouble(_GetDecimal(data));
        double _FromSByte(T data) => _GetSByte(data);
        double _FromByte(T data) => _GetByte(data);
        double _FromInt16(T data) => _GetInt16(data);
        double _FromUInt16(T data) => _GetUInt16(data);
        double _FromInt32(T data) => _GetInt32(data);
        double _FromUInt32(T data) => _GetUInt32(data);
        double _FromInt64(T data) => _GetInt64(data);
        double _FromUInt64(T data) => _GetUInt64(data);
        double _ConvertGeneric(T data)
        {
            var (ret, wasConverted) = _genericConverter.Value.ConvertValue(data);
            if(!wasConverted && _throwOnFailure)
                throw new ArgumentException($"Could not convert {data} to double");
            return (double)ret;
        }

        public double Convert(T data) => _converter(data);
        public Type To => typeof(double);
    }
}
