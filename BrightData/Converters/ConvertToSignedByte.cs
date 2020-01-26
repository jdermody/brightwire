using System;
using System.Collections.Generic;
using System.Text;

namespace BrightData.Converters
{
    class ConvertToSignedByte<T> : ConverterBase<T>, ICanConvert<T, sbyte>
        where T : struct
    {
        readonly Func<T, sbyte> _converter;

        public ConvertToSignedByte()
        {
            _converter = _GetConverter();
        }

        Func<T, sbyte> _GetConverter()
        {
            var typeCode = Type.GetTypeCode(typeof(T));
            switch (typeCode) {
                case TypeCode.Single:
                    return _FromSingle;
                case TypeCode.Double:
                    return _FromDouble;
                case TypeCode.SByte:
                    return _GetSByte;
                case TypeCode.Int16:
                    return _FromInt16;
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

        sbyte _FromSingle(T data) => System.Convert.ToSByte(_GetSingle(data));
        sbyte _FromDouble(T data) => System.Convert.ToSByte(_GetDouble(data));
        sbyte _FromDecimal(T data) => System.Convert.ToSByte(_GetDecimal(data));
        sbyte _FromInt16(T data) => System.Convert.ToSByte(_GetInt16(data));
        sbyte _FromInt32(T data) => System.Convert.ToSByte(_GetInt32(data));
        sbyte _FromInt64(T data) => System.Convert.ToSByte(_GetInt64(data));

        public sbyte Convert(T data) => _converter(data);
        public Type To => typeof(sbyte);
    }
}
