using System;

namespace BrightData.Buffer.ReadOnly.Converter
{
    internal class CustomConverter<FT, TT> : ReadOnlyConverterBase<FT, TT>
        where FT : notnull
        where TT: notnull
    {
        readonly Func<FT, TT> _converter;

        public CustomConverter(IReadOnlyBuffer<FT> from, Func<FT, TT> converter) : base(from)
        {
            _converter = converter;
        }

        protected override TT Convert(in FT from) => _converter(from);
    }
}
