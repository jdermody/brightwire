using BrightData.LinearAlgebra.ReadOnly;

namespace BrightData.Buffer.ReadOnly.Converter
{
    /// <summary>
    /// Converts via a one hot encoder
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="from"></param>
    /// <param name="indexer"></param>
    internal class OneHotConverter<T>(IReadOnlyBuffer<T> from, ICanIndex<T> indexer) : ReadOnlyConverterBase<T, ReadOnlyVector<float>>(from)
        where T : notnull
    {
        protected override ReadOnlyVector<float> Convert(in T from)
        {
            var index = indexer.GetIndex(from);
            return new ReadOnlyVector<float>(indexer.Size, x => x == index ? 1f : 0f);
        }
    }
}
