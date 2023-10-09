namespace BrightData.Buffer.ReadOnly.Converter
{
    internal class ToStringConverter<FT> : ReadOnlyConverterBase<FT, string> where FT: notnull
    {
        public ToStringConverter(IReadOnlyBuffer<FT> from) : base(from)
        {
        }

        protected override string Convert(in FT from)
        {
            return from.ToString() ?? string.Empty;
        }
    }
}
