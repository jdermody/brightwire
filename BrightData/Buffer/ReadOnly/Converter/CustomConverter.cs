namespace BrightData.Buffer.ReadOnly.Converter
{
    internal class CustomConverter<FT, TT> : ReadOnlyConverterBase<FT, TT>
        where FT : notnull
        where TT: notnull
    {
        readonly ConvertInPlaceDelegate _inPlace;
        readonly ConvertDelegate _convert;

        public delegate void ConvertInPlaceDelegate(in FT from, ref TT to);
        public delegate TT ConvertDelegate(in FT from);

        public CustomConverter(IReadOnlyBuffer<FT> from, ConvertInPlaceDelegate inPlace, ConvertDelegate convert) : base(from)
        {
            _inPlace = inPlace;
            _convert = convert;
        }

        protected override void Convert(in FT from, ref TT to) => _inPlace(from, ref to);
        protected override TT Convert(in FT from) => _convert(from);
    }
}
