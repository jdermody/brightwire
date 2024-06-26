﻿using System;
using System.Runtime.CompilerServices;

namespace BrightData.Converter
{
    /// <summary>
    /// Converts to long
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ConvertToLong<T> : ConverterBase<T>, ICanConvert<T, long>
        where T : notnull
    {
        readonly Func<T, long> _converter;

        public ConvertToLong(bool throwOnFailure = false) : base(throwOnFailure)
        {
            var typeCode = Type.GetTypeCode(typeof(T));
            _converter = typeCode switch
            {
                TypeCode.String  => FromString,
                TypeCode.Single  => FromSingle,
                TypeCode.Double  => FromDouble,
                TypeCode.SByte   => FromSByte,
                TypeCode.Byte    => FromByte,
                TypeCode.Int16   => FromInt16,
                TypeCode.UInt16  => FromUInt16,
                TypeCode.Int32   => FromInt32,
                TypeCode.UInt32  => FromUInt32,
                TypeCode.Int64   => GetInt64,
                TypeCode.UInt64  => FromUInt64,
                TypeCode.Decimal => FromDecimal,
                _                => ConvertGeneric,
            };
        }

        static long FromString(T str) => long.Parse(Unsafe.As<T, string>(ref str));
        static long FromSingle(T data) => System.Convert.ToInt64(GetSingle(data));
        static long FromDouble(T data) => System.Convert.ToInt64(GetDouble(data));
        static long FromDecimal(T data) => System.Convert.ToInt64(GetDecimal(data));
        static long FromSByte(T data) => GetSByte(data);
        static long FromByte(T data) => GetByte(data);
        static long FromInt16(T data) => GetInt16(data);
        static long FromUInt16(T data) => GetUInt16(data);
        static long FromInt32(T data) => GetInt32(data);
        static long FromUInt32(T data) => GetUInt32(data);
        static long FromUInt64(T data) => System.Convert.ToInt64(GetUInt64(data));
        long ConvertGeneric(T data)
        {
            var (ret, wasConverted) = (_genericConverter ??= new()).ConvertValue(data);
            if(!wasConverted && _throwOnFailure || ret == null)
                throw new ArgumentException($"Could not convert {data} to long");
            return (long)ret;
        }
        public long Convert(T data) => _converter(data);
        public Type To => typeof(long);
    }
}
