using System;
using System.Collections.Generic;
using System.Text;

namespace BrightData.Converters
{
    class ConvertToDecimal<T> : ConverterBase<T>, ICanConvert<T, decimal>
        where T : struct
    {
        readonly Func<T, decimal> _converter;

        public ConvertToDecimal(bool throwOnFailure = false) : base(throwOnFailure)
        {
            _converter = _GetConverter();
        }

        Func<T, decimal> _GetConverter()
        {
            var typeCode = Type.GetTypeCode(typeof(T));
            return typeCode switch
            {
                TypeCode.Single => _FromSingle,
                TypeCode.Double => _FromDouble,
                TypeCode.SByte => _FromSByte,
                TypeCode.Int16 => _FromInt16,
                TypeCode.Int32 => _FromInt32,
                TypeCode.Int64 => _FromInt64,
                TypeCode.Decimal => _GetDecimal,
                _ => _ConvertGeneric,
            };
        }

        decimal _FromSingle(T data) => System.Convert.ToDecimal(_GetSingle(data));
        decimal _FromDouble(T data) => System.Convert.ToDecimal(_GetDouble(data));
        decimal _FromSByte(T data) => _GetSByte(data);
        decimal _FromInt16(T data) => _GetInt16(data);
        decimal _FromInt32(T data) => _GetInt32(data);
        decimal _FromInt64(T data) => _GetInt64(data);
        decimal _ConvertGeneric(T data)
        {
            var (ret, wasConverted) = _genericConverter.Value.ConvertValue(data);
            if(!wasConverted && _throwOnFailure)
                throw new ArgumentException($"Could not convert {data} to decimal");
            return (decimal)ret;
        }

        public decimal Convert(T data) => _converter(data);
        public Type To => typeof(decimal);
    }
}
