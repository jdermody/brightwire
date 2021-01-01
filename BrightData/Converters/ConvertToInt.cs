﻿using System;

namespace BrightData.Converters
{
    class ConvertToInt<T> : ConverterBase<T>, ICanConvert<T, int>
        where T : struct
    {
        readonly Func<T, int> _converter;

        public ConvertToInt(bool throwOnFailure = false) : base(throwOnFailure)
        {
            _converter = _GetConverter();
        }

        Func<T, int> _GetConverter()
        {
            var typeCode = Type.GetTypeCode(typeof(T));
            return typeCode switch
            {
                TypeCode.Single => _FromSingle,
                TypeCode.Double => _FromDouble,
                TypeCode.SByte => _FromSByte,
                TypeCode.Int16 => _FromInt16,
                TypeCode.Int32 => _GetInt32,
                TypeCode.Int64 => _FromInt64,
                TypeCode.Decimal => _FromDecimal,
                _ => _ConvertGeneric,
            };
        }

        int _FromSingle(T data) => System.Convert.ToInt32(_GetSingle(data));
        int _FromDouble(T data) => System.Convert.ToInt32(_GetDouble(data));
        int _FromDecimal(T data) => System.Convert.ToInt32(_GetDecimal(data));
        int _FromSByte(T data) => _GetSByte(data);
        int _FromInt16(T data) => _GetInt16(data);
        int _FromInt32(T data) => _GetInt32(data);
        int _FromInt64(T data) => System.Convert.ToInt32(_GetInt64(data));
        int _ConvertGeneric(T data)
        {
            var (ret, wasConverted) = _genericConverter.Value.ConvertValue(data);
            if(!wasConverted && _throwOnFailure)
                throw new ArgumentException($"Could not convert {data} to int");
            return (int)ret;
        }
        public int Convert(T data) => _converter(data);
        public Type To => typeof(int);
    }
}