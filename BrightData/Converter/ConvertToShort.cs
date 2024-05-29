using System;
using System.Runtime.CompilerServices;

namespace BrightData.Converter
{
    /// <summary>
    /// Converts to short
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ConvertToShort<T> : ConverterBase<T>, ICanConvert<T, short>
        where T : notnull
    {
        readonly Func<T, short> _converter;

        public ConvertToShort(bool throwOnFailure = false) : base(throwOnFailure)
        {
            var typeCode = Type.GetTypeCode(typeof(T));
            _converter = typeCode switch
            {
                TypeCode.String  => FromString,
                TypeCode.Single  => FromSingle,
                TypeCode.Double  => FromDouble,
                TypeCode.SByte   => FromSByte,
                TypeCode.Byte    => FromByte,
                TypeCode.Int16   => GetInt16,
                TypeCode.UInt16  => FromUInt16,
                TypeCode.Int32   => FromInt32,
                TypeCode.UInt32  => FromUInt32,
                TypeCode.Int64   => FromInt64,
                TypeCode.UInt64  => FromUInt64,
                TypeCode.Decimal => FromDecimal,
                _                => ConvertGeneric,
            };
        }

        static short FromString(T str) => short.Parse(Unsafe.As<T, string>(ref str));
        static short FromSingle(T data) => System.Convert.ToInt16(GetSingle(data));
        static short FromDouble(T data) => System.Convert.ToInt16(GetDouble(data));
        static short FromDecimal(T data) => System.Convert.ToInt16(GetDecimal(data));
        static short FromSByte(T data) => GetSByte(data);
        static short FromByte(T data) => GetByte(data);
        static short FromUInt16(T data) => System.Convert.ToInt16(GetUInt16(data));
        static short FromInt32(T data) => System.Convert.ToInt16(GetInt32(data));
        static short FromUInt32(T data) => System.Convert.ToInt16(GetUInt32(data));
        static short FromInt64(T data) => System.Convert.ToInt16(GetInt64(data));
        static short FromUInt64(T data) => System.Convert.ToInt16(GetUInt64(data));
        short ConvertGeneric(T data)
        {
            var (ret, wasConverted) = (_genericConverter ??= new()).ConvertValue(data);
            if(!wasConverted && _throwOnFailure || ret == null)
                throw new ArgumentException($"Could not convert {data} to short");
            return (short)ret;
        }
        public short Convert(T data) => _converter(data);
        public Type To => typeof(short);
    }
}
