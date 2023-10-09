namespace BrightData.Operations.Conversion
{
    internal class ToStringConversion<T> : ConversionBase<T, string>
        where T : notnull
    {
        public ToStringConversion(IReadOnlyBuffer<T> input, IAppendToBuffer<string> output) : base(input, output)
        {
        }

        protected override string Convert(T from) => from.ToString() ?? string.Empty;
    }
}
