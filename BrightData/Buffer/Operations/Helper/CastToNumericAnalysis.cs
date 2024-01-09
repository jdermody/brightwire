using System;
using System.Threading;
using System.Threading.Tasks;
using BrightData.Converter;
using BrightData.Helper;

namespace BrightData.Buffer.Operations.Helper
{
    /// <summary>
    /// Casts to double to perform numerical analysis
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class CastToNumericAnalysis<T> : ICastToNumericAnalysis, IAcceptBlock<T> where T : notnull
    {
        readonly BufferScan<T> _scan;
        readonly ICanConvert<T, double> _converter;

        public CastToNumericAnalysis(IReadOnlyBuffer<T> buffer)
        {
            _converter = StaticConverters.GetConverterToDouble<T>();
            _scan = new(buffer, this, null);
        }

        public bool IsInteger { get; private set; } = true;
        public uint NanCount { get; private set; } = 0;
        public uint InfinityCount { get; private set; } = 0;
        public double MinValue { get; private set; } = double.MaxValue;
        public double MaxValue { get; private set; } = double.MinValue;

        public void Add(ReadOnlySpan<T> block)
        {
            foreach (var item in block)
            {
                var val = _converter.Convert(item);
                if (double.IsInfinity(val))
                    ++InfinityCount;
                else if (double.IsNaN(val))
                    ++NanCount;
                else
                {
                    if (val < MinValue)
                        MinValue = val;
                    if (val > MaxValue)
                        MaxValue = val;
                    if (IsInteger && Math.Abs(val % 1) > FloatMath.AlmostZero)
                        IsInteger = false;
                }
            }
        }

        public Task Process(INotifyUser? notify = null, string? msg = null, CancellationToken ct = default) => _scan.Process(notify, msg, ct);
    }
}
