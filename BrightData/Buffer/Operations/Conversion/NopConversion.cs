namespace BrightData.Buffer.Operations.Conversion
{
    /// <summary>
    /// Nop conversion (does nothing)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="input"></param>
    /// <param name="output"></param>
    internal class NopConversion<T>(IReadOnlyBuffer<T> input, IAppendToBuffer<T> output) : ConversionBase<T, T>(input, output)
        where T : notnull
    {
        protected override T Convert(T from) => from;
    }
}
