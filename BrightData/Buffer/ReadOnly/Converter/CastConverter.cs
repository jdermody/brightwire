namespace BrightData.Buffer.ReadOnly.Converter
{
    /// <summary>
    /// Converts via a cast
    /// </summary>
    /// <typeparam name="FT"></typeparam>
    /// <typeparam name="TT"></typeparam>
    /// <param name="from"></param>
    internal class CastConverter<FT, TT>(IReadOnlyBuffer<FT> from) : ReadOnlyConverterBase<FT, TT>(from)
        where FT : notnull
        where TT : notnull
    {
        protected override TT Convert(in FT from)
        {
            return (TT)(object)from;
        }
    }
}
