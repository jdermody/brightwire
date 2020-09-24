using System;

namespace BrightData.Converters
{
    public class ConvertToFloat<T> : ConverterBase<T>, ICanConvert<T, float>
        where T: struct
    {
        readonly Func<T, float> _converter;

        public ConvertToFloat(bool throwOnFailure = false) : base(throwOnFailure)
        {
            _converter = _GetConverter();
        }

        Func<T, float> _GetConverter()
        {
            var typeCode = Type.GetTypeCode(typeof(T));
            return typeCode switch {
                TypeCode.Single => _GetSingle,
                TypeCode.Double => _FromDouble,
                TypeCode.SByte => _FromSByte,
                TypeCode.Int16 => _FromInt16,
                TypeCode.Int32 => _FromInt32,
                TypeCode.Int64 => _FromInt64,
                TypeCode.Decimal => _FromDecimal,
                _ => _ConvertGeneric
            };
        }

        float _FromDouble(T data) => System.Convert.ToSingle(_GetDouble(data));
        float _FromDecimal(T data) => System.Convert.ToSingle(_GetDecimal(data));
        float _FromSByte(T data) => _GetSByte(data);
        float _FromInt16(T data) => _GetInt16(data);
        float _FromInt32(T data) => _GetInt32(data);
        float _FromInt64(T data) => _GetInt64(data);
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
