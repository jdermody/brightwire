namespace BrightData.Buffer.ReadOnly.Converter
{
    internal class TypeConverter<FT, TT> : ReadOnlyConverterBase<FT, TT>
        where FT: notnull
        where TT: notnull
    {
        readonly ICanConvert<FT, TT> _converter;

        public TypeConverter(IReadOnlyBuffer<FT> from, ICanConvert<FT, TT> converter) : base(from)
        {
            _converter = converter;
        }

        protected override TT Convert(in FT from) => _converter.Convert(from);
    }

    internal class TypeConverterWithMetaData<FT, TT> : ReadOnlyConverterBase<FT, TT>
        where FT: notnull
        where TT: notnull
    {
        readonly ICanConvert<FT, TT> _converter;

        public TypeConverterWithMetaData(IReadOnlyBufferWithMetaData<FT> from, ICanConvert<FT, TT> converter) : base(from)
        {
            _converter = converter;
        }

        protected override TT Convert(in FT from) => _converter.Convert(from);
    }
}
