using System;

namespace BrightData.Converters
{
    internal class ConvertToLong<T> : ConverterBase<T>, ICanConvert<T, long>
        where T : struct
    {
        readonly Func<T, long> _converter;

        public ConvertToLong(bool throwOnFailure = false) : base(throwOnFailure)
        {
            var typeCode = Type.GetTypeCode(typeof(T));
            _converter = typeCode switch
            {
                TypeCode.Single => _FromSingle,
                TypeCode.Double => _FromDouble,
                TypeCode.SByte => _FromSByte,
                TypeCode.Byte => _FromByte,
                TypeCode.Int16 => _FromInt16,
                TypeCode.UInt16 => _FromUInt16,
                TypeCode.Int32 => _FromInt32,
                TypeCode.UInt32 => _FromUInt32,
                TypeCode.Int64 => _GetInt64,
                TypeCode.UInt64 => _FromUInt64,
                TypeCode.Decimal => _FromDecimal,
                _ => _ConvertGeneric,
            };
        }

        long _FromSingle(T data) => System.Convert.ToInt64(_GetSingle(data));
        long _FromDouble(T data) => System.Convert.ToInt64(_GetDouble(data));
        long _FromDecimal(T data) => System.Convert.ToInt64(_GetDecimal(data));
        long _FromSByte(T data) => _GetSByte(data);
        long _FromByte(T data) => _GetByte(data);
        long _FromInt16(T data) => _GetInt16(data);
        long _FromUInt16(T data) => _GetUInt16(data);
        long _FromInt32(T data) => _GetInt32(data);
        long _FromUInt32(T data) => _GetUInt32(data);
        long _FromUInt64(T data) => System.Convert.ToInt64(_GetUInt64(data));
        long _ConvertGeneric(T data)
        {
            var (ret, wasConverted) = _genericConverter.Value.ConvertValue(data);
            if(!wasConverted && _throwOnFailure)
                throw new ArgumentException($"Could not convert {data} to long");
            return (long)ret;
        }
        public long Convert(T data) => _converter(data);
        public Type To => typeof(long);
    }
}
