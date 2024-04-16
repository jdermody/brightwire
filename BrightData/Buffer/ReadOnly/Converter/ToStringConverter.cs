namespace BrightData.Buffer.ReadOnly.Converter
{
    /// <summary>
    /// Converts to strings
    /// </summary>
    /// <typeparam name="FT"></typeparam>
    /// <param name="from"></param>
    internal class ToStringConverter<FT>(IReadOnlyBuffer<FT> from) : ReadOnlyConverterBase<FT, string>(from)
        where FT : notnull
    {
        protected override string Convert(in FT from)
        {
            return from.ToString() ?? string.Empty;
        }
    }
}
