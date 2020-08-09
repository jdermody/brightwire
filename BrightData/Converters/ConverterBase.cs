using BrightData.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightData.Converters
{
    public abstract class ConverterBase<T> where T: struct
    {
        protected readonly Lazy<GenericConverter<float>> _genericConverter = new Lazy<GenericConverter<float>>();
        protected readonly bool _throwOnFailure;

        public ConverterBase(bool throwOnFailure)
        {
            _throwOnFailure = throwOnFailure;
        }

        protected float _GetSingle(T data) => __refvalue(__makeref(data), float);
        protected double _GetDouble(T data) => __refvalue(__makeref(data), double);
        protected sbyte _GetSByte(T data) => __refvalue(__makeref(data), sbyte);
        protected short _GetInt16(T data) => __refvalue(__makeref(data), short);
        protected int _GetInt32(T data) => __refvalue(__makeref(data), int);
        protected long _GetInt64(T data) => __refvalue(__makeref(data), long);
        protected decimal _GetDecimal(T data) => __refvalue(__makeref(data), decimal);

        public Type From => typeof(T);
    }
}
