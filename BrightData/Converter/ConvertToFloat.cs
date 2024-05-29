using System;
using System.Runtime.CompilerServices;

namespace BrightData.Converter
{
    /// <summary>
    /// Converts to float
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ConvertToFloat<T> : ConverterBase<T>, ICanConvert<T, float>
        where T: notnull
    {
        readonly Func<T, float> _converter;

        public ConvertToFloat(bool throwOnFailure = false) : base(throwOnFailure)
        {
            var typeCode = Type.GetTypeCode(typeof(T));
            _converter = typeCode switch
            {
                TypeCode.String  => FromString,
                TypeCode.Single  => GetSingle,
                TypeCode.Double  => FromDouble,
                TypeCode.SByte   => FromSByte,
                TypeCode.Byte    => FromByte,
                TypeCode.Int16   => FromInt16,
                TypeCode.UInt16  => FromUInt16,
                TypeCode.Int32   => FromInt32,
                TypeCode.UInt32  => FromUInt32,
                TypeCode.Int64   => FromInt64,
                TypeCode.UInt64  => FromUInt64,
                TypeCode.Decimal => FromDecimal,
                _                => ConvertGeneric
            };
        }

        static float FromString(T str) => float.Parse(Unsafe.As<T, string>(ref str));
        static float FromDouble(T data) => System.Convert.ToSingle(GetDouble(data));
        static float FromDecimal(T data) => System.Convert.ToSingle(GetDecimal(data));
        static float FromSByte(T data) => GetSByte(data);
        static float FromByte(T data) => GetByte(data);
        static float FromInt16(T data) => GetInt16(data);
        static float FromUInt16(T data) => GetUInt16(data);
        static float FromInt32(T data) => GetInt32(data);
        static float FromUInt32(T data) => GetUInt32(data);
        static float FromInt64(T data) => GetInt64(data);
        static float FromUInt64(T data) => GetUInt64(data);
        float ConvertGeneric(T data)
        {
            var (ret, wasConverted) = (_genericConverter ??= new()).ConvertValue(data);
            if(!wasConverted && _throwOnFailure || ret == null)
                throw new ArgumentException($"Could not convert {data} to float");
            return (float)ret;
        }

        public float Convert(T data) => _converter(data);
        public Type To => typeof(float);
    }
}
