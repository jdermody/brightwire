using System;
using System.ComponentModel.DataAnnotations;
using System.Numerics;
using System.Reflection;

namespace BrightData.Converter
{
    /// <summary>
    /// Static methods to create converters
    /// </summary>
    public class StaticConverters
    {
        /// <summary>
        /// Creates a converter to decimals
        /// </summary>
        /// <typeparam name="T">Type to convert from</typeparam>
        /// <returns></returns>
        public static ICanConvert<T, decimal> GetConverterToDecimal<T>() where T: notnull => new ConvertToDecimal<T>();

        /// <summary>
        /// Creates a converter to doubles
        /// </summary>
        /// <typeparam name="T">Type to convert from</typeparam>
        /// <returns></returns>
        public static ICanConvert<T, double> GetConverterToDouble<T>() where T : notnull => new ConvertToDouble<T>();

        /// <summary>
        /// Creates a converter to floats
        /// </summary>
        /// <typeparam name="T">Type to convert from</typeparam>
        /// <returns></returns>
        public static ICanConvert<T, float> GetConverterToFloat<T>() where T : notnull => new ConvertToFloat<T>();

        /// <summary>
        /// Creates a converter to integers
        /// </summary>
        /// <typeparam name="T">Type to convert from</typeparam>
        /// <returns></returns>
        public static ICanConvert<T, int> GetConverterToInt<T>() where T : notnull => new ConvertToInt<T>();

        /// <summary>
        /// Creates a converter to longs (Int64)
        /// </summary>
        /// <typeparam name="T">Type to convert from</typeparam>
        /// <returns></returns>
        public static ICanConvert<T, long> GetConverterToLong<T>() where T : notnull => new ConvertToLong<T>();

        /// <summary>
        /// Creates a converter to shorts (Int16)
        /// </summary>
        /// <typeparam name="T">Type to convert from</typeparam>
        /// <returns></returns>
        public static ICanConvert<T, short> GetConverterToShort<T>() where T : notnull => new ConvertToShort<T>();

        /// <summary>
        /// Creates a convert to signed bytes
        /// </summary>
        /// <typeparam name="T">Type to convert from</typeparam>
        /// <returns></returns>
        public static ICanConvert<T, sbyte> GetConverterToSignedByte<T>() where T : notnull => new ConvertToSignedByte<T>();

        /// <summary>
        /// Creates a converter from FT to TT
        /// </summary>
        /// <typeparam name="TF">Type to convert from</typeparam>
        /// <typeparam name="TT">Type to convert to</typeparam>
        /// <returns></returns>
        public static ICanConvert<TF, TT> GetNumericConverter<TF, TT>() where TF: INumber<TF> where TT : notnull
        {
            return Type.GetTypeCode(typeof(TT)) switch {
                TypeCode.Boolean => (ICanConvert<TF, TT>) new ConvertToBoolean<TF>(),
                TypeCode.SByte   => (ICanConvert<TF, TT>) GetConverterToSignedByte<TF>(),
                TypeCode.Int16   => (ICanConvert<TF, TT>) GetConverterToShort<TF>(),
                TypeCode.Int32   => (ICanConvert<TF, TT>) GetConverterToInt<TF>(),
                TypeCode.Int64   => (ICanConvert<TF, TT>) GetConverterToLong<TF>(),
                TypeCode.Single  => (ICanConvert<TF, TT>) GetConverterToFloat<TF>(),
                TypeCode.Double  => (ICanConvert<TF, TT>) GetConverterToDouble<TF>(),
                TypeCode.Decimal => (ICanConvert<TF, TT>) GetConverterToDecimal<TF>(),
                _                => throw new NotImplementedException()
            };
        }

        public static ICanConvert<TF, TT> GetConverter<TF, TT>() where TF : notnull where TT : notnull
        {
            if (Type.GetTypeCode(typeof(TF)) is TypeCode.String) {
                return Type.GetTypeCode(typeof(TT)) switch {
                    TypeCode.SByte   => (ICanConvert<TF, TT>) GetConverterToSignedByte<TF>(),
                    TypeCode.Int16   => (ICanConvert<TF, TT>) GetConverterToShort<TF>(),
                    TypeCode.Int32   => (ICanConvert<TF, TT>) GetConverterToInt<TF>(),
                    TypeCode.Int64   => (ICanConvert<TF, TT>) GetConverterToLong<TF>(),
                    TypeCode.Single  => (ICanConvert<TF, TT>) GetConverterToFloat<TF>(),
                    TypeCode.Double  => (ICanConvert<TF, TT>) GetConverterToDouble<TF>(),
                    TypeCode.Decimal => (ICanConvert<TF, TT>) GetConverterToDecimal<TF>(),
                    _                => throw new NotImplementedException()
                };
            }
            return (ICanConvert<TF, TT>) StaticConverters.GetNumericConverterMethodInfo.MakeGenericMethod(typeof(TF), typeof(TT)).Invoke(null, null)!;
        }

        public static MethodInfo GetNumericConverterMethodInfo = typeof(StaticConverters).GetMethod(nameof(GetNumericConverter), BindingFlags.Static | BindingFlags.Public)!;
        public static MethodInfo GetConverterMethodInfo = typeof(StaticConverters).GetMethod(nameof(GetConverter), BindingFlags.Static | BindingFlags.Public)!;
    }
}
