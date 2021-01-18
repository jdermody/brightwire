using System;

namespace BrightData.Converters
{
    internal abstract class ConverterBase<T> where T: struct
    {
        protected readonly Lazy<GenericConverter<float>> _genericConverter = new Lazy<GenericConverter<float>>();
        protected readonly bool _throwOnFailure;

        protected ConverterBase(bool throwOnFailure)
        {
            _throwOnFailure = throwOnFailure;
        }

        protected float _GetSingle(T data) => __refvalue(__makeref(data), float);
        protected double _GetDouble(T data) => __refvalue(__makeref(data), double);
        protected sbyte _GetSByte(T data) => __refvalue(__makeref(data), sbyte);
        protected byte _GetByte(T data) => __refvalue(__makeref(data), byte);
        protected short _GetInt16(T data) => __refvalue(__makeref(data), short);
        protected ushort _GetUInt16(T data) => __refvalue(__makeref(data), ushort);
        protected int _GetInt32(T data) => __refvalue(__makeref(data), int);
        protected uint _GetUInt32(T data) => __refvalue(__makeref(data), uint);
        protected long _GetInt64(T data) => __refvalue(__makeref(data), long);
        protected ulong _GetUInt64(T data) => __refvalue(__makeref(data), ulong);
        protected decimal _GetDecimal(T data) => __refvalue(__makeref(data), decimal);

        public Type From => typeof(T);
    }
}
