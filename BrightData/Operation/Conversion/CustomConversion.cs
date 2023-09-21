using System;

namespace BrightData.Operation.Conversion
{
    internal class CustomConversion<FT, T> : ConversionBase<FT, T>
        where FT : notnull
        where T : notnull
    {
        readonly Func<FT, T> _converter;

        public CustomConversion(Func<FT, T> converter, IReadOnlyBuffer<FT> input, IAppendToBuffer<T> output) : base(input, output)
        {
            _converter = converter;
        }

        protected override T Convert(FT from) => _converter(from);
    }
}
