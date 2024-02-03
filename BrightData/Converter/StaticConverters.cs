using System;
using System.Numerics;
using System.Reflection;
using BrightData.Helper;

namespace BrightData.Converter
{
    /// <summary>
    /// Static methods to create converters
    /// </summary>
    public class StaticConverters
    {
        internal static ICanConvert ConvertToBoolean(Type type, bool throwOnFailure = false)
        {
            var typeCode = Type.GetTypeCode(type);
            if(typeCode == TypeCode.Boolean)
                return new NopConverter<bool>();
            if(typeCode == TypeCode.SByte)
                return new ConvertToBoolean<sbyte>(throwOnFailure);
            if(typeCode == TypeCode.Single)
                return new ConvertToBoolean<float>(throwOnFailure);
            if(typeCode == TypeCode.Double)
                return new ConvertToBoolean<double>(throwOnFailure);
            if(typeCode == TypeCode.Decimal)
                return new ConvertToBoolean<decimal>(throwOnFailure);
            if(typeCode == TypeCode.Int16)
                return new ConvertToBoolean<short>(throwOnFailure);
            if(typeCode == TypeCode.Int32)
                return new ConvertToBoolean<int>(throwOnFailure);
            if(typeCode == TypeCode.Int64)
                return new ConvertToBoolean<long>(throwOnFailure);
            throw new NotImplementedException($"Could not create ConvertToBoolean for type {type}");

        }

        /// <summary>
        /// Creates a numeric converter from TF to TT
        /// </summary>
        /// <returns></returns>
        public static ICanConvert GetConverter(Type typeFrom, Type typeTo)
        {
            return Type.GetTypeCode(typeFrom) switch {
                TypeCode.Boolean => ConvertToBoolean(typeTo),
                TypeCode.SByte   => GenericTypeMapping.ConvertToSignedByte(typeTo),
                TypeCode.Int16   => GenericTypeMapping.ConvertToShort(typeTo),
                TypeCode.Int32   => GenericTypeMapping.ConvertToInt(typeTo),
                TypeCode.Int64   => GenericTypeMapping.ConvertToLong(typeTo),
                TypeCode.Single  => GenericTypeMapping.ConvertToFloat(typeTo),
                TypeCode.Double  => GenericTypeMapping.ConvertToDouble(typeTo),
                TypeCode.Decimal => GenericTypeMapping.ConvertToDecimal(typeTo),
                _                => throw new NotImplementedException()
            };
        }

        /// <summary>
        /// Creates a converter from TF to TT
        /// </summary>
        /// <typeparam name="TF"></typeparam>
        /// <typeparam name="TT"></typeparam>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static ICanConvert<TF, TT> GetConverter<TF, TT>() where TF : notnull where TT : notnull => (ICanConvert<TF, TT>)GetConverter(typeof(TF), typeof(TT));
    }
}
