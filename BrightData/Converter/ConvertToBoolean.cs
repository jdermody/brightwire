using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.Converter
{
    /// <summary>
    /// Convert to boolean
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="throwOnFailure"></param>
    internal class ConvertToBoolean<T>(bool throwOnFailure = false) : ConverterBase<T>(throwOnFailure), ICanConvert<T, bool>
        where T : INumber<T>
    {
        public bool Convert(T data)
        {
            return data != T.Zero;
        }

        public Type To => typeof(T);
    }
}
