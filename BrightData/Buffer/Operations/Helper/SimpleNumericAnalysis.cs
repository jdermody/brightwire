using System;
using System.Threading;
using System.Threading.Tasks;
using BrightData.Helper;

namespace BrightData.Buffer.Operations.Helper
{
    /// <summary>
    /// Casts to double to perform numerical analysis
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class SimpleNumericAnalysis<T> : ISimpleNumericAnalysis, IAppendBlocks<T> where T : notnull
    {
        readonly BufferCopyOperation<T> _scan;
        readonly ICanConvert<T, double> _converter;

        public SimpleNumericAnalysis(IReadOnlyBuffer<T> buffer)
        {
            _converter = (ICanConvert<T, double>)GenericTypeMapping.ConvertToDouble(typeof(T));
            _scan = new(buffer, this, null);
        }

        public bool IsInteger { get; private set; } = true;
        public uint NanCount { get; private set; } = 0;
        public uint InfinityCount { get; private set; } = 0;
        public double MinValue { get; private set; } = double.MaxValue;
        public double MaxValue { get; private set; } = double.MinValue;

        public void Append(ReadOnlySpan<T> block)
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
                    if (IsInteger && Math.Abs(val % 1) > Math<double>.AlmostZero)
                        IsInteger = false;
                }
            }
        }

        public Task Execute(INotifyOperationProgress? notify = null, string? msg = null, CancellationToken ct = default) => _scan.Execute(notify, msg, ct);
    }
}
