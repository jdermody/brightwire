using System.Numerics;
using BrightData.Converter;

namespace BrightData.Operations.Conversion
{
    internal class UnmanagedConversion<FT, T> : ConversionBase<FT, T>
        where FT : INumber<FT>
        where T : unmanaged
    {
        readonly ICanConvert<FT, T> _converter;

        public UnmanagedConversion(IReadOnlyBuffer<FT> input, IAppendToBuffer<T> output) : base(input, output)
        {
            _converter = StaticConverters.GetConverter<FT, T>();
        }

        protected override T Convert(FT from) => _converter.Convert(from);
    }
}
