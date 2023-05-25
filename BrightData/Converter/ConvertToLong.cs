using System;

namespace BrightData.Converter
{
    internal class ConvertToLong<T> : ConverterBase<T>, ICanConvert<T, long>
        where T : notnull
    {
        readonly Func<T, long> _converter;

        public ConvertToLong(bool throwOnFailure = false) : base(throwOnFailure)
        {
            var typeCode = Type.GetTypeCode(typeof(T));
            _converter = typeCode switch
            {
                TypeCode.String  => FromString,
                TypeCode.Single  => FromSingle,
                TypeCode.Double  => FromDouble,
                TypeCode.SByte   => FromSByte,
                TypeCode.Byte    => FromByte,
                TypeCode.Int16   => FromInt16,
                TypeCode.UInt16  => FromUInt16,
                TypeCode.Int32   => FromInt32,
                TypeCode.UInt32  => FromUInt32,
                TypeCode.Int64   => GetInt64,
                TypeCode.UInt64  => FromUInt64,
                TypeCode.Decimal => FromDecimal,
                _                => ConvertGeneric,
            };
        }

        static long FromString(T str) => long.Parse(__refvalue(__makeref(str), string));
        long FromSingle(T data) => System.Convert.ToInt64(GetSingle(data));
        long FromDouble(T data) => System.Convert.ToInt64(GetDouble(data));
        long FromDecimal(T data) => System.Convert.ToInt64(GetDecimal(data));
        long FromSByte(T data) => GetSByte(data);
        long FromByte(T data) => GetByte(data);
        long FromInt16(T data) => GetInt16(data);
        long FromUInt16(T data) => GetUInt16(data);
        long FromInt32(T data) => GetInt32(data);
        long FromUInt32(T data) => GetUInt32(data);
        long FromUInt64(T data) => System.Convert.ToInt64(GetUInt64(data));
        long ConvertGeneric(T data)
        {
            var (ret, wasConverted) = _genericConverter.Value.ConvertValue(data);
            if(!wasConverted && _throwOnFailure || ret == null)
                throw new ArgumentException($"Could not convert {data} to long");
            return (long)ret;
        }
        public long Convert(T data) => _converter(data);
        public Type To => typeof(long);
    }
}
