using System;
using System.Numerics;
using BrightData.Helper;

namespace BrightData.Buffer.ReadOnly.Converter
{
    internal class NormalizationConverter<T> : ReadOnlyConverterBase<T, T> where T: unmanaged, INumber<T>
    {
        readonly T _divide, _subtract;
        readonly bool _divideByZero;

        public NormalizationConverter(IReadOnlyBuffer<T> buffer, INormalize normalization) : base(buffer)
        {
            _divide = T.CreateSaturating(normalization.Divide);
            _subtract = T.CreateSaturating(normalization.Subtract);
            _divideByZero = Math.Abs(normalization.Divide) <= FloatMath.AlmostZero;
            Type = normalization.NormalizationType;
        }

        public NormalizationType Type { get; }

        protected override T Convert(in T from) => (from - _subtract) / (_divideByZero ? T.One : _divide);
    }
}
