namespace BrightData.Operation.Conversion
{
    internal class NopConversion<T> : ConversionBase<T, T> where T: notnull
    {
        public NopConversion(IReadOnlyBuffer<T> input, IAppendToBuffer<T> output) : base(input, output)
        {
        }

        protected override T Convert(T from) => from;
    }
}
