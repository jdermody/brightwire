namespace BrightData.Buffer.ReadOnly.Converter
{
    /// <summary>
    /// Converts the values in the buffer to a single categorical index
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="from"></param>
    /// <param name="indexer"></param>
    internal class CategoricalIndexConverter<T>(IReadOnlyBuffer<T> from, ICanIndex<T> indexer) : ReadOnlyConverterBase<T, int>(from)
        where T : notnull
    {
        protected override int Convert(in T from) => (int)indexer.GetIndex(in from);
    }
}
