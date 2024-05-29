using System;
using System.Runtime.CompilerServices;

namespace BrightData.Converter
{
    /// <summary>
    /// Converter base class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="throwOnFailure"></param>
    internal abstract class ConverterBase<T>(bool throwOnFailure)
        where T : notnull
    {
        protected GenericConverter<float>? _genericConverter = null;
        protected readonly bool _throwOnFailure = throwOnFailure;

        protected static float GetSingle(T data)    => Unsafe.As<T, float>(ref data);
        protected static double GetDouble(T data)   => Unsafe.As<T, double>(ref data);
        protected static sbyte GetSByte(T data)     => Unsafe.As<T, sbyte>(ref data);
        protected static byte GetByte(T data)       => Unsafe.As<T, byte>(ref data);
        protected static short GetInt16(T data)     => Unsafe.As<T, short>(ref data);
        protected static ushort GetUInt16(T data)   => Unsafe.As<T, ushort>(ref data);
        protected static int GetInt32(T data)       => Unsafe.As<T, int>(ref data);
        protected static uint GetUInt32(T data)     => Unsafe.As<T, uint>(ref data);
        protected static long GetInt64(T data)      => Unsafe.As<T, long>(ref data);
        protected static ulong GetUInt64(T data)    => Unsafe.As<T, ulong>(ref data);
        protected static decimal GetDecimal(T data) => Unsafe.As<T, decimal>(ref data);

        public Type From => typeof(T);
    }
}
