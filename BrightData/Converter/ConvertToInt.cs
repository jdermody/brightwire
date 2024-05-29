using System;
using System.Runtime.CompilerServices;

namespace BrightData.Converter
{
    /// <summary>
    /// Converts to int
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ConvertToInt<T> : ConverterBase<T>, ICanConvert<T, int>
        where T : notnull
    {
        readonly Func<T, int> _converter;

        public ConvertToInt(bool throwOnFailure = false) : base(throwOnFailure)
        {
            var typeCode = Type.GetTypeCode(typeof(T));
            _converter = typeCode switch {
                TypeCode.String  => FromString,
                TypeCode.Single  => FromSingle,
                TypeCode.Double  => FromDouble,
                TypeCode.SByte   => FromSByte,
                TypeCode.Byte    => FromByte,
                TypeCode.Int16   => FromInt16,
                TypeCode.UInt16  => FromUInt16,
                TypeCode.Int32   => GetInt32,
                TypeCode.UInt32  => FromUInt32,
                TypeCode.Int64   => FromInt64,
                TypeCode.UInt64  => FromUInt64,
                TypeCode.Decimal => FromDecimal,
                _                => ConvertGeneric,
            };
        }

        static int FromString(T str) => int.Parse(Unsafe.As<T, string>(ref str));
        static int FromSingle(T data) => System.Convert.ToInt32(GetSingle(data));
        static int FromDouble(T data) => System.Convert.ToInt32(GetDouble(data));
        static int FromDecimal(T data) => System.Convert.ToInt32(GetDecimal(data));
        static int FromSByte(T data) => GetSByte(data);
        static int FromByte(T data) => GetByte(data);
        static int FromInt16(T data) => GetInt16(data);
        static int FromUInt16(T data) => GetUInt16(data);
        static int FromUInt32(T data) => System.Convert.ToInt32(GetUInt32(data));
        static int FromInt64(T data) => System.Convert.ToInt32(GetInt64(data));
        static int FromUInt64(T data) => System.Convert.ToInt32(GetUInt64(data));
        int ConvertGeneric(T data)
        {
            var (ret, wasConverted) = (_genericConverter ??= new()).ConvertValue(data);
            if(!wasConverted && _throwOnFailure || ret == null)
                throw new ArgumentException($"Could not convert {data} to int");
            return (int)ret;
        }
        public int Convert(T data) => _converter(data);
        public Type To => typeof(int);
    }
}
