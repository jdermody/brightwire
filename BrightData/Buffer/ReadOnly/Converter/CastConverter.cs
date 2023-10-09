namespace BrightData.Buffer.ReadOnly.Converter
{
    internal class CastConverter<FT, TT> : ReadOnlyConverterBase<FT, TT> 
        where FT : notnull 
        where TT : notnull
    {
        public CastConverter(IReadOnlyBuffer<FT> from) : base(from)
        {
        }

        protected override TT Convert(in FT from)
        {
            return (TT)(object)from;
        }
    }
}
