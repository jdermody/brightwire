using System;
using BrightData.Converter;

namespace BrightData.Operation.Vectorisation
{
    internal class NumericVectoriser<T> : VectorisationBase<T> where T : notnull
    {
        readonly ICanConvert<T, float> _converter;

        public NumericVectoriser() : base(1)
        {
            _converter = StaticConverters.GetConverterToFloat<T>();
        }

        protected override void Vectorise(in T item, Span<float> buffer)
        {
            buffer[0] = _converter.Convert(item);
        }

        public override VectorisationType Type => VectorisationType.Numeric;
    }
}
