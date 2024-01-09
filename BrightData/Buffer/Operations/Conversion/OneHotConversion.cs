using BrightData.LinearAlgebra.ReadOnly;

namespace BrightData.Buffer.Operations.Conversion
{
    /// <summary>
    /// One hot conversion
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="input"></param>
    /// <param name="indexer"></param>
    /// <param name="output"></param>
    internal class OneHotConversion<T>(IReadOnlyBuffer<T> input, ICanIndex<T> indexer, IAppendToBuffer<ReadOnlyVector> output)
        : ConversionBase<T, ReadOnlyVector>(input, output)
        where T : notnull
    {
        protected override ReadOnlyVector Convert(T from)
        {
            var index = indexer.GetIndex(from);
            return new ReadOnlyVector(indexer.Size, x => x == index ? 1f : 0f);
        }
    }
}
