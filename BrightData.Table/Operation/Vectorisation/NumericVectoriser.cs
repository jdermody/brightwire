using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData.Converter;

namespace BrightData.Table.Operation.Vectorisation
{
    internal class NumericVectoriser<T> : VectorisationBase<T> where T : notnull
    {
        readonly ICanConvert<T, float> _converter;

        public NumericVectoriser(IReadOnlyBuffer<T> buffer) : base(buffer, 1)
        {
            _converter = StaticConverters.GetConverterToFloat<T>();
        }

        protected override void Vectorise(in T item, Span<float> buffer)
        {
            buffer[0] = _converter.Convert(item);
        }
    }
}
