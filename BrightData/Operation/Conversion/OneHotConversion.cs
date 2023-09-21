using BrightData.LinearAlgebra.ReadOnly;

namespace BrightData.Operation.Conversion
{
    internal class OneHotConversion<T> : ConversionBase<T, ReadOnlyVector> where T: notnull
    {
        readonly ICanIndex<T> _indexer;

        public OneHotConversion(IReadOnlyBuffer<T> input, ICanIndex<T> indexer, IAppendToBuffer<ReadOnlyVector> output) : base(input, output)
        {
            _indexer = indexer;
        }

        protected override ReadOnlyVector Convert(T from)
        {
            var index = _indexer.GetIndex(from);
            return new ReadOnlyVector(_indexer.Size, x => x == index ? 1f : 0f);
        }
    }
}
