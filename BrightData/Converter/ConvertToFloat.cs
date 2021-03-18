using System;

namespace BrightData.Converter
{
    internal class ConvertToFloat<T> : ConverterBase<T>, ICanConvert<T, float>
        where T: notnull
    {
        readonly Func<T, float> _converter;

        public ConvertToFloat(bool throwOnFailure = false) : base(throwOnFailure)
        {
            var typeCode = Type.GetTypeCode(typeof(T));
            _converter = typeCode switch
            {
                TypeCode.String => FromString,
                TypeCode.Single => GetSingle,
                TypeCode.Double => FromDouble,
                TypeCode.SByte => FromSByte,
                TypeCode.Byte => FromByte,
                TypeCode.Int16 => FromInt16,
                TypeCode.UInt16 => FromUInt16,
                TypeCode.Int32 => FromInt32,
                TypeCode.UInt32 => FromUInt32,
                TypeCode.Int64 => FromInt64,
                TypeCode.UInt64 => FromUInt64,
                TypeCode.Decimal => FromDecimal,
                _ => ConvertGeneric
            };
        }

        float FromString(T str) => float.Parse(__refvalue(__makeref(str), string));
        float FromDouble(T data) => System.Convert.ToSingle(GetDouble(data));
        float FromDecimal(T data) => System.Convert.ToSingle(GetDecimal(data));
        float FromSByte(T data) => GetSByte(data);
        float FromByte(T data) => GetByte(data);
        float FromInt16(T data) => GetInt16(data);
        float FromUInt16(T data) => GetUInt16(data);
        float FromInt32(T data) => GetInt32(data);
        float FromUInt32(T data) => GetUInt32(data);
        float FromInt64(T data) => GetInt64(data);
        float FromUInt64(T data) => GetUInt64(data);
        float ConvertGeneric(T data)
        {
            var (ret, wasConverted) = _genericConverter.Value.ConvertValue(data);
            if(!wasConverted && _throwOnFailure || ret == null)
                throw new ArgumentException($"Could not convert {data} to float");
            return (float)ret;
        }

        public float Convert(T data) => _converter(data);
        public Type To => typeof(float);
    }
}
