namespace BrightData.Buffer.ReadOnly.Converter
{
    /// <summary>
    /// Converts to objects
    /// </summary>
    /// <typeparam name="FT"></typeparam>
    /// <param name="from"></param>
    internal class ToObjectConverter<FT>(IReadOnlyBuffer<FT> from) : ReadOnlyConverterBase<FT, object>(from)
        where FT : notnull
    {
        protected override object Convert(in FT from)
        {
            return from;
        }
    }
}
