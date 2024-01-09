using System.Numerics;
using BrightData.Converter;

namespace BrightData.Buffer.Operations.Conversion
{
    /// <summary>
    /// Converts numbers to unmanaged types
    /// </summary>
    /// <typeparam name="FT"></typeparam>
    /// <typeparam name="T"></typeparam>
    /// <param name="input"></param>
    /// <param name="output"></param>
    internal class NumericUnmanagedConversion<FT, T>(IReadOnlyBuffer<FT> input, IAppendToBuffer<T> output) : ConversionBase<FT, T>(input, output)
        where FT : INumber<FT>
        where T : unmanaged
    {
        readonly ICanConvert<FT, T> _converter = StaticConverters.GetNumericConverter<FT, T>();

        protected override T Convert(FT from) => _converter.Convert(from);
    }
}
