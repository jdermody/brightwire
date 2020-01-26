using System;
using System.Collections.Generic;
using System.Text;

namespace BrightData.Converters
{
    class ConvertToShort<T> : ConverterBase<T>, ICanConvert<T, short>
        where T : struct
    {
        readonly Func<T, short> _converter;

        public ConvertToShort()
        {
            _converter = _GetConverter();
        }

        Func<T, short> _GetConverter()
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
                    return _GetInt16;
                case TypeCode.Int32:
                    return _FromInt32;
                case TypeCode.Int64:
                    return _FromInt64;
                case TypeCode.Decimal:
                    return _FromDecimal;
                default:
                    throw new NotImplementedException();
            }
        }

        short _FromSingle(T data) => System.Convert.ToInt16(_GetSingle(data));
        short _FromDouble(T data) => System.Convert.ToInt16(_GetDouble(data));
        short _FromDecimal(T data) => System.Convert.ToInt16(_GetDecimal(data));
        short _FromSByte(T data) => _GetSByte(data);
        short _FromInt32(T data) => System.Convert.ToInt16(_GetInt32(data));
        short _FromInt64(T data) => System.Convert.ToInt16(_GetInt64(data));

        public short Convert(T data) => _converter(data);
        public Type To => typeof(short);
    }
}
