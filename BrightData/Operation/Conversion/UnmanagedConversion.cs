using BrightData.Converter;

namespace BrightData.Operation.Conversion
{
    internal class UnmanagedConversion<FT, T> : ConversionBase<FT, T>
        where FT : notnull
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
