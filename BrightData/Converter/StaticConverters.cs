using System;
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
            return Type.GetTypeCode(typeTo) switch {
                TypeCode.Boolean => ConvertToBoolean(typeFrom),
                TypeCode.SByte   => GenericTypeMapping.ConvertToSignedByte(typeFrom),
                TypeCode.Int16   => GenericTypeMapping.ConvertToShort(typeFrom),
                TypeCode.Int32   => GenericTypeMapping.ConvertToInt(typeFrom),
                TypeCode.Int64   => GenericTypeMapping.ConvertToLong(typeFrom),
                TypeCode.Single  => GenericTypeMapping.ConvertToFloat(typeFrom),
                TypeCode.Double  => GenericTypeMapping.ConvertToDouble(typeFrom),
                TypeCode.Decimal => GenericTypeMapping.ConvertToDecimal(typeFrom),
                _                => throw new NotImplementedException($"Could not create type mapping for {typeFrom} to {typeTo}")
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
