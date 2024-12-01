using CommunityToolkit.HighPerformance.Buffers;
using System;
using System.Numerics;

namespace BrightData.Helper
{
    /// <summary>
    /// Defines a span aggregation operation that will apply to each element within the span
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="existingVal">Previous value</param>
    /// <param name="val">New value</param>
    /// <param name="total">Number of previous aggregation operations (including this one)</param>
    /// <returns></returns>
    public delegate T SpanAggregationOperation<T>(T existingVal, T val, T total) where T: unmanaged, INumber<T>;

    /// <summary>
    /// A span aggregator
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public ref struct SpanAggregator<T> where T: unmanaged, INumber<T>
    {
        readonly SpanOwner<T> _delta;
        readonly SpanAggregationOperation<T> _operation;
        T _denominator = T.Zero;

        SpanAggregator(uint size, SpanAggregationOperation<T> operation)
        {
            _delta = SpanOwner<T>.Allocate((int)size);
            _delta.Span.Clear();
            _operation = operation;
        }

        /// <summary>
        /// Creates a span aggregator that calculates the average of each value (online algorithm)
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static SpanAggregator<T> GetOnlineAverage(uint size) => new(size, OnlineAverage);

        /// <summary>
        /// Disposes the span aggregation
        /// </summary>
        public readonly void Dispose() => _delta.Dispose();

        /// <summary>
        /// Adds a new operation
        /// </summary>
        /// <param name="span"></param>
        public void Add(ReadOnlySpan<T> span)
        {
            ++_denominator;
            var existing = _delta.Span;
            for (var i = 0; i < _delta.Length; i++)
                existing[i] = _operation(existing[i], span[i], _denominator);
        }

        /// <summary>
        /// Adds a new operation
        /// </summary>
        /// <param name="span"></param>
        /// <param name="coefficient"></param>
        public void Add(ReadOnlySpan<T> span, T coefficient)
        {
            _denominator += coefficient;
            var existing = _delta.Span;
            for (var i = 0; i < _delta.Length; i++)
                existing[i] = _operation(existing[i], span[i] * coefficient, _denominator);
        }

        /// <summary>
        /// The aggregation result
        /// </summary>
        public readonly ReadOnlySpan<T> Span => _delta.Span;

        /// <summary>
        /// Online average aggregation
        /// </summary>
        static readonly SpanAggregationOperation<T> OnlineAverage = (existing, value, total) => {
            var delta = value - existing;
            return existing + delta / T.CreateSaturating(total);
        };
    }
}
