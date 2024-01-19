using System;
using BrightData.Converter;

namespace BrightData.Buffer.Operations.Vectorisation
{
    /// <summary>
    /// Numeric vectorisation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class NumericVectoriser<T>() : VectorisationBase<T>(1)
        where T : notnull
    {
        readonly ICanConvert<T, float> _converter = StaticConverters.GetConverterToFloat<T>();

        protected override void Vectorise(in T item, Span<float> buffer)
        {
            buffer[0] = _converter.Convert(item);
        }

        public override VectorisationType Type => VectorisationType.Numeric;
    }
}
