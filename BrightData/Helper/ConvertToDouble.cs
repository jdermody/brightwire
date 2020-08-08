using System;
using System.Collections.Generic;
using System.Text;

namespace BrightData.Helper
{
    public class ConvertToDouble<T>
        where T: struct
    {
        readonly TypeCode _from;
        readonly Lazy<GenericConverter<double>> _genericConverter = new Lazy<GenericConverter<double>>();
        readonly bool _throwOnFailure;

        public ConvertToDouble(bool throwOnFailure = false)
        {
            _from = Type.GetTypeCode(typeof(T));
            _throwOnFailure = throwOnFailure;

            // TODO: check that the type is numeric
        }

        public double Convert(T data)
        {
            if (_from == TypeCode.Double)
                return __refvalue(__makeref(data), double);
            else if (_from == TypeCode.Single)
                return __refvalue(__makeref(data), float);
            else if (_from == TypeCode.SByte)
                return __refvalue(__makeref(data), sbyte);
            else if(_from == TypeCode.Int16)
                return __refvalue(__makeref(data), short);
            else if(_from == TypeCode.Int32)
                return __refvalue(__makeref(data), int);
            else if (_from == TypeCode.Int64)
                return __refvalue(__makeref(data), long);
            else if (_from == TypeCode.Decimal)
                return (float)__refvalue(__makeref(data), decimal);

            var (ret, wasConverted) = _genericConverter.Value.ConvertValue(data);
            if(!wasConverted && _throwOnFailure)
                throw new ArgumentException($"Could not convert {data} to float");
            return (double)ret;
        }
    }
}
