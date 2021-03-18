using System;

namespace BrightData.Converter
{
    internal class ConvertToShort<T> : ConverterBase<T>, ICanConvert<T, short>
        where T : notnull
    {
        readonly Func<T, short> _converter;

        public ConvertToShort(bool throwOnFailure = false) : base(throwOnFailure)
        {
            var typeCode = Type.GetTypeCode(typeof(T));
            _converter = typeCode switch
            {
                TypeCode.String => FromString,
                TypeCode.Single => FromSingle,
                TypeCode.Double => FromDouble,
                TypeCode.SByte => FromSByte,
                TypeCode.Byte => FromByte,
                TypeCode.Int16 => GetInt16,
                TypeCode.UInt16 => FromUInt16,
                TypeCode.Int32 => FromInt32,
                TypeCode.UInt32 => FromUInt32,
                TypeCode.Int64 => FromInt64,
                TypeCode.UInt64 => FromUInt64,
                TypeCode.Decimal => FromDecimal,
                _ => ConvertGeneric,
            };
        }

        short FromString(T str) => short.Parse(__refvalue(__makeref(str), string));
        short FromSingle(T data) => System.Convert.ToInt16(GetSingle(data));
        short FromDouble(T data) => System.Convert.ToInt16(GetDouble(data));
        short FromDecimal(T data) => System.Convert.ToInt16(GetDecimal(data));
        short FromSByte(T data) => GetSByte(data);
        short FromByte(T data) => GetByte(data);
        short FromUInt16(T data) => System.Convert.ToInt16(GetUInt16(data));
        short FromInt32(T data) => System.Convert.ToInt16(GetInt32(data));
        short FromUInt32(T data) => System.Convert.ToInt16(GetUInt32(data));
        short FromInt64(T data) => System.Convert.ToInt16(GetInt64(data));
        short FromUInt64(T data) => System.Convert.ToInt16(GetUInt64(data));
        short ConvertGeneric(T data)
        {
            var (ret, wasConverted) = _genericConverter.Value.ConvertValue(data);
            if(!wasConverted && _throwOnFailure || ret == null)
                throw new ArgumentException($"Could not convert {data} to short");
            return (short)ret;
        }
        public short Convert(T data) => _converter(data);
        public Type To => typeof(short);
    }
}
