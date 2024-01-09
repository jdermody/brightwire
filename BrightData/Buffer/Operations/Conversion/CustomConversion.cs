using System;

namespace BrightData.Buffer.Operations.Conversion
{
    /// <summary>
    /// Custom buffer conversion
    /// </summary>
    /// <typeparam name="FT"></typeparam>
    /// <typeparam name="T"></typeparam>
    /// <param name="converter"></param>
    /// <param name="input"></param>
    /// <param name="output"></param>
    internal class CustomConversion<FT, T>(Func<FT, T> converter, IReadOnlyBuffer<FT> input, IAppendToBuffer<T> output)
        : ConversionBase<FT, T>(input, output)
        where FT : notnull
        where T : notnull
    {
        protected override T Convert(FT from) => converter(from);
    }
}
