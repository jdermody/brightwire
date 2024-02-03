using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.Converter
{
    /// <summary>
    /// Converter that does not actually convert
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class NopConverter<T> : ICanConvert<T, T> where T : notnull
    {
        public T Convert(T data) => data;
        public Type From => typeof(T);
        public Type To => typeof(T);
    }
}
