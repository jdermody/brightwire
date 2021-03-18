using System;

namespace BrightData.Converter
{
    internal abstract class ConverterBase<T> where T: notnull
    {
        protected readonly Lazy<GenericConverter<float>> _genericConverter = new Lazy<GenericConverter<float>>();
        protected readonly bool _throwOnFailure;

        protected ConverterBase(bool throwOnFailure)
        {
            _throwOnFailure = throwOnFailure;
        }

        protected float GetSingle(T data) => __refvalue(__makeref(data), float);
        protected double GetDouble(T data) => __refvalue(__makeref(data), double);
        protected sbyte GetSByte(T data) => __refvalue(__makeref(data), sbyte);
        protected byte GetByte(T data) => __refvalue(__makeref(data), byte);
        protected short GetInt16(T data) => __refvalue(__makeref(data), short);
        protected ushort GetUInt16(T data) => __refvalue(__makeref(data), ushort);
        protected int GetInt32(T data) => __refvalue(__makeref(data), int);
        protected uint GetUInt32(T data) => __refvalue(__makeref(data), uint);
        protected long GetInt64(T data) => __refvalue(__makeref(data), long);
        protected ulong GetUInt64(T data) => __refvalue(__makeref(data), ulong);
        protected decimal GetDecimal(T data) => __refvalue(__makeref(data), decimal);

        public Type From => typeof(T);
    }
}
