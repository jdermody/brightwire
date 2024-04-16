using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.Buffer.Operations.Conversion
{
    /// <summary>
    /// Base class for buffer conversions
    /// </summary>
    /// <typeparam name="FT"></typeparam>
    /// <typeparam name="T"></typeparam>
    internal abstract class ConversionBase<FT, T> : OneToOneMutation<FT, T>.IWriteMutatedBlocks, IOperation
        where FT : notnull
        where T : notnull
    {
        protected Action?                _onComplete;
        readonly OneToOneMutation<FT, T> _operation;

        protected ConversionBase(IReadOnlyBuffer<FT> input, IAppendToBuffer<T> output)
        {
            _operation = new OneToOneMutation<FT, T>(input, output, this);
        }

        public void Write(ReadOnlySpan<FT> from, IAppendToBuffer<T> to)
        {
            var size = from.Length;
            using var output = SpanOwner<T>.Allocate(size);
            var span = output.Span;
            for (var i = 0; i < size; i++)
                span[i] = Convert(from[i]);
            to.Append(span);
        }

        protected abstract T Convert(FT from);

        public Task Execute(INotifyOperationProgress? notify = null, string? msg = null, CancellationToken ct = default)
        {
            var ret = _operation.Execute(notify, msg, ct);
            if (_onComplete != null)
                return ret.ContinueWith(_ => _onComplete(), ct);
            return ret;
        }
    }
}
