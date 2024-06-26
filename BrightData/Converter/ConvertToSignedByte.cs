﻿using System;
using System.Runtime.CompilerServices;

namespace BrightData.Converter
{
    /// <summary>
    /// Converts to signed bytes
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ConvertToSignedByte<T> : ConverterBase<T>, ICanConvert<T, sbyte>
        where T : notnull
    {
        readonly Func<T, sbyte> _converter;

        public ConvertToSignedByte(bool throwOnFailure = false) : base(throwOnFailure)
        {
            var typeCode = Type.GetTypeCode(typeof(T));
            _converter = typeCode switch
            {
                TypeCode.String  => FromString,
                TypeCode.Single  => FromSingle,
                TypeCode.Double  => FromDouble,
                TypeCode.SByte   => GetSByte,
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

        static sbyte FromString(T str) => sbyte.Parse(Unsafe.As<T, string>(ref str));
        static sbyte FromSingle(T data) => System.Convert.ToSByte(GetSingle(data));
        static sbyte FromDouble(T data) => System.Convert.ToSByte(GetDouble(data));
        static sbyte FromDecimal(T data) => System.Convert.ToSByte(GetDecimal(data));
        static sbyte FromByte(T data) => System.Convert.ToSByte(GetByte(data));
        static sbyte FromInt16(T data) => System.Convert.ToSByte(GetInt16(data));
        static sbyte FromUInt16(T data) => System.Convert.ToSByte(GetUInt16(data));
        static sbyte FromInt32(T data) => System.Convert.ToSByte(GetInt32(data));
        static sbyte FromUInt32(T data) => System.Convert.ToSByte(GetUInt32(data));
        static sbyte FromInt64(T data) => System.Convert.ToSByte(GetInt64(data));
        static sbyte FromUInt64(T data) => System.Convert.ToSByte(GetUInt64(data));
        sbyte ConvertGeneric(T data)
        {
            var (ret, wasConverted) = (_genericConverter ??= new()).ConvertValue(data);
            if(!wasConverted && _throwOnFailure || ret == null)
                throw new ArgumentException($"Could not convert {data} to sbyte");
            return (sbyte)ret;
        }
        public sbyte Convert(T data) => _converter(data);
        public Type To => typeof(sbyte);
    }
}
