using System;

namespace BrightData.Converter
{
    internal class ConvertToDouble<T> : ConverterBase<T>, ICanConvert<T, double>
        where T : struct
    {
        readonly Func<T, double> _converter;

        public ConvertToDouble(bool throwOnFailure = false) : base(throwOnFailure)
        {
            var typeCode = Type.GetTypeCode(typeof(T));
            _converter = typeCode switch
            {
                TypeCode.Single => FromSingle,
                TypeCode.Double => GetDouble,
                TypeCode.SByte => FromSByte,
                TypeCode.Byte => FromByte,
                TypeCode.Int16 => FromInt16,
                TypeCode.UInt16 => FromUInt16,
                TypeCode.Int32 => FromInt32,
                TypeCode.UInt32 => FromUInt32,
                TypeCode.Int64 => FromInt64,
                TypeCode.UInt64 => FromUInt64,
                TypeCode.Decimal => FromDecimal,
                _ => ConvertGeneric,
            };
        }

        double FromSingle(T data) => System.Convert.ToDouble(GetSingle(data));
        double FromDecimal(T data) => System.Convert.ToDouble(GetDecimal(data));
        double FromSByte(T data) => GetSByte(data);
        double FromByte(T data) => GetByte(data);
        double FromInt16(T data) => GetInt16(data);
        double FromUInt16(T data) => GetUInt16(data);
        double FromInt32(T data) => GetInt32(data);
        double FromUInt32(T data) => GetUInt32(data);
        double FromInt64(T data) => GetInt64(data);
        double FromUInt64(T data) => GetUInt64(data);
        double ConvertGeneric(T data)
        {
            var (ret, wasConverted) = _genericConverter.Value.ConvertValue(data);
            if(!wasConverted && _throwOnFailure || ret == null)
                throw new ArgumentException($"Could not convert {data} to double");
            return (double)ret;
        }

        public double Convert(T data) => _converter(data);
        public Type To => typeof(double);
    }
}
