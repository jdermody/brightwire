using System;

namespace BrightData.Converters
{
    class ConvertToSignedByte<T> : ConverterBase<T>, ICanConvert<T, sbyte>
        where T : struct
    {
        readonly Func<T, sbyte> _converter;

        public ConvertToSignedByte(bool throwOnFailure = false) : base(throwOnFailure)
        {
            var typeCode = Type.GetTypeCode(typeof(T));
            _converter = typeCode switch
            {
                TypeCode.Single => _FromSingle,
                TypeCode.Double => _FromDouble,
                TypeCode.SByte => _GetSByte,
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

        sbyte _FromSingle(T data) => System.Convert.ToSByte(_GetSingle(data));
        sbyte _FromDouble(T data) => System.Convert.ToSByte(_GetDouble(data));
        sbyte _FromDecimal(T data) => System.Convert.ToSByte(_GetDecimal(data));
        sbyte _FromByte(T data) => System.Convert.ToSByte(_GetByte(data));
        sbyte _FromInt16(T data) => System.Convert.ToSByte(_GetInt16(data));
        sbyte _FromUInt16(T data) => System.Convert.ToSByte(_GetUInt16(data));
        sbyte _FromInt32(T data) => System.Convert.ToSByte(_GetInt32(data));
        sbyte _FromUInt32(T data) => System.Convert.ToSByte(_GetUInt32(data));
        sbyte _FromInt64(T data) => System.Convert.ToSByte(_GetInt64(data));
        sbyte _FromUInt64(T data) => System.Convert.ToSByte(_GetUInt64(data));
        sbyte _ConvertGeneric(T data)
        {
            var (ret, wasConverted) = _genericConverter.Value.ConvertValue(data);
            if(!wasConverted && _throwOnFailure)
                throw new ArgumentException($"Could not convert {data} to sbyte");
            return (sbyte)ret;
        }
        public sbyte Convert(T data) => _converter(data);
        public Type To => typeof(sbyte);
    }
}
