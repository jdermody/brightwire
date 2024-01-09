namespace BrightData.Buffer.Operations.Conversion
{
    /// <summary>
    /// Converts to a string
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="input"></param>
    /// <param name="output"></param>
    internal class ToStringConversion<T>(IReadOnlyBuffer<T> input, IAppendToBuffer<string> output) : ConversionBase<T, string>(input, output)
        where T : notnull
    {
        protected override string Convert(T from) => from.ToString() ?? string.Empty;
    }
}
