using System;
using BrightData.Converter;

namespace BrightData.Buffer.Operations.Vectorisation
{
    /// <summary>
    /// Numeric vectorisation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="isOutput"></param>
    internal class NumericVectoriser<T>(bool isOutput) : VectorisationBase<T>(isOutput, 1)
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
