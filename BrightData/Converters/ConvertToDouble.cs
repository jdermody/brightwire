using System;
using System.Collections.Generic;
using System.Text;

namespace BrightData.Converters
{
    class ConvertToDouble<T> : ConverterBase<T>, ICanConvert<T, double>
        where T : struct
    {
        readonly Func<T, double> _converter;

        public ConvertToDouble()
        {
            _converter = _GetConverter();
        }

        Func<T, double> _GetConverter()
        {
            var typeCode = Type.GetTypeCode(typeof(T));
            switch (typeCode) {
                case TypeCode.Single:
                    return _FromSingle;
                case TypeCode.Double:
                    return _GetDouble;
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
                    throw new NotImplementedException();
            }
        }

        double _FromSingle(T data) => System.Convert.ToDouble(_GetSingle(data));
        double _FromDecimal(T data) => System.Convert.ToDouble(_GetDecimal(data));
        double _FromSByte(T data) => _GetSByte(data);
        double _FromInt16(T data) => _GetInt16(data);
        double _FromInt32(T data) => _GetInt32(data);
        double _FromInt64(T data) => _GetInt64(data);

        public double Convert(T data) => _converter(data);
        public Type To => typeof(double);
    }
}
