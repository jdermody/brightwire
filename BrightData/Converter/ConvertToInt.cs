﻿using System;

namespace BrightData.Converter
{
    internal class ConvertToInt<T> : ConverterBase<T>, ICanConvert<T, int>
        where T : notnull
    {
        readonly Func<T, int> _converter;

        public ConvertToInt(bool throwOnFailure = false) : base(throwOnFailure)
        {
            var typeCode = Type.GetTypeCode(typeof(T));
            _converter = typeCode switch {
                TypeCode.String => FromString,
                TypeCode.Single => FromSingle,
                TypeCode.Double => FromDouble,
                TypeCode.SByte => FromSByte,
                TypeCode.Byte => FromByte,
                TypeCode.Int16 => FromInt16,
                TypeCode.UInt16 => FromUInt16,
                TypeCode.Int32 => GetInt32,
                TypeCode.UInt32 => FromUInt32,
                TypeCode.Int64 => FromInt64,
                TypeCode.UInt64 => FromUInt64,
                TypeCode.Decimal => FromDecimal,
                _ => ConvertGeneric,
            };
        }

        int FromString(T str) => int.Parse(__refvalue(__makeref(str), string));
        int FromSingle(T data) => System.Convert.ToInt32(GetSingle(data));
        int FromDouble(T data) => System.Convert.ToInt32(GetDouble(data));
        int FromDecimal(T data) => System.Convert.ToInt32(GetDecimal(data));
        int FromSByte(T data) => GetSByte(data);
        int FromByte(T data) => GetByte(data);
        int FromInt16(T data) => GetInt16(data);
        int FromUInt16(T data) => GetUInt16(data);
        int FromUInt32(T data) => System.Convert.ToInt32(GetUInt32(data));
        int FromInt64(T data) => System.Convert.ToInt32(GetInt64(data));
        int FromUInt64(T data) => System.Convert.ToInt32(GetUInt64(data));
        int ConvertGeneric(T data)
        {
            var (ret, wasConverted) = _genericConverter.Value.ConvertValue(data);
            if(!wasConverted && _throwOnFailure || ret == null)
                throw new ArgumentException($"Could not convert {data} to int");
            return (int)ret;
        }
        public int Convert(T data) => _converter(data);
        public Type To => typeof(int);
    }
}
