using System;

namespace BrightData.Converters
{
    class ConvertToShort<T> : ConverterBase<T>, ICanConvert<T, short>
        where T : struct
    {
        readonly Func<T, short> _converter;

        public ConvertToShort(bool throwOnFailure = false) : base(throwOnFailure)
        {
            var typeCode = Type.GetTypeCode(typeof(T));
            _converter = typeCode switch
            {
                TypeCode.Single => _FromSingle,
                TypeCode.Double => _FromDouble,
                TypeCode.SByte => _FromSByte,
                TypeCode.Byte => _FromByte,
                TypeCode.Int16 => _GetInt16,
                TypeCode.UInt16 => _FromUInt16,
                TypeCode.Int32 => _FromInt32,
                TypeCode.UInt32 => _FromUInt32,
                TypeCode.Int64 => _FromInt64,
                TypeCode.UInt64 => _FromUInt64,
                TypeCode.Decimal => _FromDecimal,
                _ => _ConvertGeneric,
            };
        }

        short _FromSingle(T data) => System.Convert.ToInt16(_GetSingle(data));
        short _FromDouble(T data) => System.Convert.ToInt16(_GetDouble(data));
        short _FromDecimal(T data) => System.Convert.ToInt16(_GetDecimal(data));
        short _FromSByte(T data) => _GetSByte(data);
        short _FromByte(T data) => _GetByte(data);
        short _FromUInt16(T data) => System.Convert.ToInt16(_GetUInt16(data));
        short _FromInt32(T data) => System.Convert.ToInt16(_GetInt32(data));
        short _FromUInt32(T data) => System.Convert.ToInt16(_GetUInt32(data));
        short _FromInt64(T data) => System.Convert.ToInt16(_GetInt64(data));
        short _FromUInt64(T data) => System.Convert.ToInt16(_GetUInt64(data));
        short _ConvertGeneric(T data)
        {
            var (ret, wasConverted) = _genericConverter.Value.ConvertValue(data);
            if(!wasConverted && _throwOnFailure)
                throw new ArgumentException($"Could not convert {data} to short");
            return (short)ret;
        }
        public short Convert(T data) => _converter(data);
        public Type To => typeof(short);
    }
}
