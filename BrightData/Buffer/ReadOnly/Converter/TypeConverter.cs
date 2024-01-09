namespace BrightData.Buffer.ReadOnly.Converter
{
    /// <summary>
    /// Converts via type converter
    /// </summary>
    /// <typeparam name="FT"></typeparam>
    /// <typeparam name="TT"></typeparam>
    /// <param name="from"></param>
    /// <param name="converter"></param>
    internal class TypeConverter<FT, TT>(IReadOnlyBuffer<FT> from, ICanConvert<FT, TT> converter) : ReadOnlyConverterBase<FT, TT>(from)
        where FT : notnull
        where TT : notnull
    {
        protected override TT Convert(in FT from) => converter.Convert(from);
    }
}
