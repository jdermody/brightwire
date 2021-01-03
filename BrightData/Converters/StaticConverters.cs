using System;
using System.Collections.Generic;
using System.Text;

namespace BrightData.Converters
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
        public static ICanConvert<T, decimal> ConvertToDecimal<T>() where T: struct => new ConvertToDecimal<T>();

        /// <summary>
        /// Creates a converter to doubles
        /// </summary>
        /// <typeparam name="T">Type to convert from</typeparam>
        /// <returns></returns>
        public static ICanConvert<T, double> ConvertToDouble<T>() where T : struct => new ConvertToDouble<T>();

        /// <summary>
        /// Creates a converter to floats
        /// </summary>
        /// <typeparam name="T">Type to convert from</typeparam>
        /// <returns></returns>
        public static ICanConvert<T, float> ConvertToFloat<T>() where T : struct => new ConvertToFloat<T>();

        /// <summary>
        /// Creates a converter to ints
        /// </summary>
        /// <typeparam name="T">Type to convert from</typeparam>
        /// <returns></returns>
        public static ICanConvert<T, int> ConvertToInt<T>() where T : struct => new ConvertToInt<T>();

        /// <summary>
        /// Creates a converter to longs (Int64)
        /// </summary>
        /// <typeparam name="T">Type to convert from</typeparam>
        /// <returns></returns>
        public static ICanConvert<T, long> ConvertToLong<T>() where T : struct => new ConvertToLong<T>();

        /// <summary>
        /// Creates a converter to shorts (Int16)
        /// </summary>
        /// <typeparam name="T">Type to convert from</typeparam>
        /// <returns></returns>
        public static ICanConvert<T, short> ConvertToShort<T>() where T : struct => new ConvertToShort<T>();

        /// <summary>
        /// Creates a convert to signed bytes
        /// </summary>
        /// <typeparam name="T">Type to convert from</typeparam>
        /// <returns></returns>
        public static ICanConvert<T, sbyte> ConvertToSignedByte<T>() where T : struct => new ConvertToSignedByte<T>();
    }
}
