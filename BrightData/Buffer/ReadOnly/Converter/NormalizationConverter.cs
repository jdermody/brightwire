using System;
using System.Numerics;
using BrightData.Helper;

namespace BrightData.Buffer.ReadOnly.Converter
{
    /// <summary>
    /// Converts via a normalisation model
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="buffer"></param>
    /// <param name="normalization"></param>
    internal class NormalizationConverter<T>(IReadOnlyBuffer<T> buffer, INormalize normalization) : ReadOnlyConverterBase<T, T>(buffer)
        where T : unmanaged, INumber<T>
    {
        readonly T _divide = T.CreateSaturating(normalization.Divide), _subtract = T.CreateSaturating(normalization.Subtract);
        readonly bool _divideByZero = Math.Abs(normalization.Divide) <= Math<double>.AlmostZero;

        public NormalizationType Type { get; } = normalization.NormalizationType;

        protected override T Convert(in T from) => (from - _subtract) / (_divideByZero ? T.One : _divide);
    }
}
