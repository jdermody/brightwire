using System;

namespace BrightData.Helper
{
    public class ConvertToFloat<T>
        where T: struct
    {
        readonly Lazy<GenericConverter<float>> _genericConverter = new Lazy<GenericConverter<float>>();
        readonly bool _throwOnFailure;
        readonly Func<T, float> _converter;

        public ConvertToFloat(bool throwOnFailure = false)
        {
            var from = Type.GetTypeCode(typeof(T));
            _throwOnFailure = throwOnFailure;

            _converter = from switch
            {
                TypeCode.Single => _ConvertFromSingle,
                TypeCode.Double => _ConvertFromDouble,
                TypeCode.SByte => _ConvertFromSByte,
                TypeCode.Int16 => _ConvertFromInt16,
                TypeCode.Int32 => _ConvertFromInt32,
                TypeCode.Int64 => _ConvertFromInt64,
                TypeCode.Decimal => _ConvertFromDecimal,
                _ => _ConvertGeneric
            };
        }

        public float Convert(T data) => _converter(data);

        static float _ConvertFromSingle(T data) => __refvalue(__makeref(data), float);
        static float _ConvertFromDouble(T data) => System.Convert.ToSingle(__refvalue(__makeref(data), double));
        static float _ConvertFromSByte(T data) => __refvalue(__makeref(data), sbyte);
        static float _ConvertFromInt16(T data) => __refvalue(__makeref(data), short);
        static float _ConvertFromInt32(T data) => __refvalue(__makeref(data), int);
        static float _ConvertFromInt64(T data) => __refvalue(__makeref(data), long);
        static float _ConvertFromDecimal(T data) => System.Convert.ToSingle(__refvalue(__makeref(data), decimal));

        float _ConvertGeneric(T data)
        {
            var (ret, wasConverted) = _genericConverter.Value.ConvertValue(data);
            if(!wasConverted && _throwOnFailure)
                throw new ArgumentException($"Could not convert {data} to float");
            return (float)ret;
        }
    }
}
