using System;
using System.Collections.Generic;
using System.Text;

namespace BrightData.Converters
{
    class ConvertToInt<T> : ConverterBase<T>, ICanConvert<T, int>
        where T : struct
    {
        readonly Func<T, int> _converter;

        public ConvertToInt()
        {
            _converter = _GetConverter();
        }

        Func<T, int> _GetConverter()
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
                    return _GetInt32;
                case TypeCode.Int64:
                    return _FromInt64;
                case TypeCode.Decimal:
                    return _FromDecimal;
                default:
                    throw new NotImplementedException();
            }
        }

        int _FromSingle(T data) => System.Convert.ToInt32(_GetSingle(data));
        int _FromDouble(T data) => System.Convert.ToInt32(_GetDouble(data));
        int _FromDecimal(T data) => System.Convert.ToInt32(_GetDecimal(data));
        int _FromSByte(T data) => _GetSByte(data);
        int _FromInt16(T data) => _GetInt16(data);
        int _FromInt32(T data) => _GetInt32(data);
        int _FromInt64(T data) => System.Convert.ToInt32(_GetInt64(data));

        public int Convert(T data) => _converter(data);
        public Type To => typeof(int);
    }
}
