using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.Operation.Conversion
{
    internal abstract class ConversionBase<FT, T> : OneToOneMutation<FT, T>.IWriteMutatedBlocks, IOperation
        where FT : notnull
        where T : notnull
    {
        protected Action? _onComplete;
        readonly IOperation _operation;

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
            to.Add(span);
        }

        protected abstract T Convert(FT from);

        public Task Process(INotifyUser? notify = null, string? msg = null, CancellationToken ct = default)
        {
            var ret = _operation.Process(notify, msg, ct);
            if (_onComplete != null)
                return ret.ContinueWith(_ => _onComplete(), ct);
            return ret;
        }
    }
}
