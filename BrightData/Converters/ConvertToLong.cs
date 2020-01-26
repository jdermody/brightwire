using System;
using System.Collections.Generic;
using System.Text;

namespace BrightData.Converters
{
    class ConvertToLong<T> : ConverterBase<T>, ICanConvert<T, long>
        where T : struct
    {
        readonly Func<T, long> _converter;

        public ConvertToLong()
        {
            _converter = _GetConverter();
        }

        Func<T, long> _GetConverter()
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
                    return _GetInt64;
                case TypeCode.Decimal:
                    return _FromDecimal;
                default:
                    throw new NotImplementedException();
            }
        }

        long _FromSingle(T data) => System.Convert.ToInt64(_GetSingle(data));
        long _FromDouble(T data) => System.Convert.ToInt64(_GetDouble(data));
        long _FromDecimal(T data) => System.Convert.ToInt64(_GetDecimal(data));
        long _FromSByte(T data) => _GetSByte(data);
        long _FromInt16(T data) => _GetInt16(data);
        long _FromInt32(T data) => _GetInt32(data);

        public long Convert(T data) => _converter(data);
        public Type To => typeof(long);
    }
}
