using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.Converter
{
    internal class ConvertToBoolean<T> : ConverterBase<T>, ICanConvert<T, bool> where T: INumber<T>
    {
        public ConvertToBoolean(bool throwOnFailure = false) : base(throwOnFailure)
        {
        }

        public bool Convert(T data)
        {
            return data != T.Zero;
        }

        public Type To => typeof(T);
    }
}
