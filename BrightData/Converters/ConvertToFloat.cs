using System;
using System.Collections.Generic;
using System.Text;
using BrightData.Converters;

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
            switch (typeCode) {
                case TypeCode.Single:
                    return _GetSingle;
                case TypeCode.Double:
                    return _FromDouble;
                case TypeCode.SByte:
                    return _FromSByte;
                case TypeCode.Int16:
                    return _FromInt16;
                case TypeCode.Int32:
                    return _FromInt32;
                case TypeCode.Int64:
                    return _FromInt64;
                case TypeCode.Decimal:
                    return _FromDecimal;
                default:
                    return _ConvertGeneric;
            }
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
