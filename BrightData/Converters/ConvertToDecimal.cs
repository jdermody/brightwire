using System;
using System.Collections.Generic;
using System.Text;

namespace BrightData.Converters
{
    class ConvertToDecimal<T> : ConverterBase<T>, ICanConvert<T, decimal>
        where T : struct
    {
        readonly Func<T, decimal> _converter;

        public ConvertToDecimal()
        {
            _converter = _GetConverter();
        }

        Func<T, decimal> _GetConverter()
        {
            var typeCode = Type.GetTypeCode(typeof(T));
            switch (typeCode) {
                case TypeCode.Single:
                    return _FromSingle;
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
                    return _GetDecimal;
                default:
                    throw new NotImplementedException();
            }
        }

        decimal _FromSingle(T data) => System.Convert.ToDecimal(_GetSingle(data));
        decimal _FromDouble(T data) => System.Convert.ToDecimal(_GetDouble(data));
        decimal _FromSByte(T data) => _GetSByte(data);
        decimal _FromInt16(T data) => _GetInt16(data);
        decimal _FromInt32(T data) => _GetInt32(data);
        decimal _FromInt64(T data) => _GetInt64(data);

        public decimal Convert(T data) => _converter(data);
        public Type To => typeof(decimal);
    }
}
