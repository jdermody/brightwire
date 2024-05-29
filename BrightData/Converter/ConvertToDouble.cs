using System;
using System.Runtime.CompilerServices;

namespace BrightData.Converter
{
    /// <summary>
    /// Converts to double
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ConvertToDouble<T> : ConverterBase<T>, ICanConvert<T, double>
        where T : notnull
    {
        readonly Func<T, double> _converter;

        public ConvertToDouble(bool throwOnFailure = false) : base(throwOnFailure)
        {
            var typeCode = Type.GetTypeCode(typeof(T));
            _converter = typeCode switch
            {
                TypeCode.String  => FromString,
                TypeCode.Single  => FromSingle,
                TypeCode.Double  => GetDouble,
                TypeCode.SByte   => FromSByte,
                TypeCode.Byte    => FromByte,
                TypeCode.Int16   => FromInt16,
                TypeCode.UInt16  => FromUInt16,
                TypeCode.Int32   => FromInt32,
                TypeCode.UInt32  => FromUInt32,
                TypeCode.Int64   => FromInt64,
                TypeCode.UInt64  => FromUInt64,
                TypeCode.Decimal => FromDecimal,
                _                => ConvertGeneric,
            };
        }

        static double FromString(T str) => double.Parse(Unsafe.As<T, string>(ref str));
        static double FromSingle(T data) => System.Convert.ToDouble(GetSingle(data));
        static double FromDecimal(T data) => System.Convert.ToDouble(GetDecimal(data));
        static double FromSByte(T data) => GetSByte(data);
        static double FromByte(T data) => GetByte(data);
        static double FromInt16(T data) => GetInt16(data);
        static double FromUInt16(T data) => GetUInt16(data);
        static double FromInt32(T data) => GetInt32(data);
        static double FromUInt32(T data) => GetUInt32(data);
        static double FromInt64(T data) => GetInt64(data);
        static double FromUInt64(T data) => GetUInt64(data);
        double ConvertGeneric(T data)
        {
            var (ret, wasConverted) = (_genericConverter ??= new()).ConvertValue(data);
            if(!wasConverted && _throwOnFailure || ret == null)
                throw new ArgumentException($"Could not convert {data} to double");
            return (double)ret;
        }

        public double Convert(T data) => _converter(data);
        public Type To => typeof(double);
    }
}
