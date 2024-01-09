using System;

namespace BrightData.Buffer.ReadOnly.Converter
{
    /// <summary>
    /// Converts via a user supplied function
    /// </summary>
    /// <typeparam name="FT"></typeparam>
    /// <typeparam name="TT"></typeparam>
    /// <param name="from"></param>
    /// <param name="converter"></param>
    internal class CustomConverter<FT, TT>(IReadOnlyBuffer<FT> from, Func<FT, TT> converter) : ReadOnlyConverterBase<FT, TT>(from)
        where FT : notnull
        where TT : notnull
    {
        protected override TT Convert(in FT from) => converter(from);
    }
}
