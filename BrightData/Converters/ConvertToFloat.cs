using System;

namespace BrightData.Converters
{
    internal class ConvertToFloat<T> : ConverterBase<T>, ICanConvert<T, float>
        where T: struct
    {
        readonly Func<T, float> _converter;

        public ConvertToFloat(bool throwOnFailure = false) : base(throwOnFailure)
        {
            var typeCode = Type.GetTypeCode(typeof(T));
            _converter = typeCode switch
            {
                TypeCode.Single => _GetSingle,
                TypeCode.Double => _FromDouble,
                TypeCode.SByte => _FromSByte,
                TypeCode.Byte => _FromByte,
                TypeCode.Int16 => _FromInt16,
                TypeCode.UInt16 => _FromUInt16,
                TypeCode.Int32 => _FromInt32,
                TypeCode.UInt32 => _FromUInt32,
                TypeCode.Int64 => _FromInt64,
                TypeCode.UInt64 => _FromUInt64,
                TypeCode.Decimal => _FromDecimal,
                _ => _ConvertGeneric
            };
        }

        float _FromDouble(T data) => System.Convert.ToSingle(_GetDouble(data));
        float _FromDecimal(T data) => System.Convert.ToSingle(_GetDecimal(data));
        float _FromSByte(T data) => _GetSByte(data);
        float _FromByte(T data) => _GetByte(data);
        float _FromInt16(T data) => _GetInt16(data);
        float _FromUInt16(T data) => _GetUInt16(data);
        float _FromInt32(T data) => _GetInt32(data);
        float _FromUInt32(T data) => _GetUInt32(data);
        float _FromInt64(T data) => _GetInt64(data);
        float _FromUInt64(T data) => _GetUInt64(data);
        float _ConvertGeneric(T data)
        {
            var (ret, wasConverted) = _genericConverter.Value.ConvertValue(data);
            if(!wasConverted && _throwOnFailure)
                throw new ArgumentException($"Could not convert {data} to float");
            return (float)ret;
        }

        public float Convert(T data) => _converter(data);
        public Type To => typeof(float);
    }
}
