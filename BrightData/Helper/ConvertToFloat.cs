using System;
using System.Collections.Generic;
using System.Text;

namespace BrightData.Helper
{
    public class ConvertToFloat<T>
        where T: struct
    {
        readonly TypeCode _from;

        public ConvertToFloat()
        {
            _from = Type.GetTypeCode(typeof(T));

            // TODO: check that the type is numeric
        }

        public float Convert(T data)
        {
            if (_from == TypeCode.Single)
                return __refvalue(__makeref(data), float);

            if (_from == TypeCode.SByte)
                return __refvalue(__makeref(data), sbyte);
            else if(_from == TypeCode.Int16)
                return __refvalue(__makeref(data), short);
            else if(_from == TypeCode.Int32)
                return __refvalue(__makeref(data), int);
            else if (_from == TypeCode.Int64)
                return __refvalue(__makeref(data), long);
            else if (_from == TypeCode.Decimal)
                return (float)__refvalue(__makeref(data), decimal);

            throw new NotImplementedException();
        }
    }
}
