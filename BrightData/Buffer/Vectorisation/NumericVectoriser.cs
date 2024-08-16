using System;
using BrightData.Helper;

namespace BrightData.Buffer.Vectorisation
{
    /// <summary>
    /// Numeric vectorisation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class NumericVectoriser<T>() : VectorisationBase<T>(1)
        where T : notnull
    {
        readonly ICanConvert<T, float> _converter = (ICanConvert<T, float>)GenericTypeMapping.ConvertToFloat(typeof(T));

        protected override void Vectorise(in T item, Span<float> buffer)
        {
            buffer[0] = _converter.Convert(item);
        }

        public override VectorisationType Type => VectorisationType.Numeric;
    }
}
