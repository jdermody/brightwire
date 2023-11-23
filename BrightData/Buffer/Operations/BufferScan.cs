using System;
using System.Threading;
using System.Threading.Tasks;

namespace BrightData.Operations
{
    internal class BufferScan<T> : IOperation
        where T: notnull
    {
        readonly IReadOnlyBuffer<T> _from;
        readonly IAcceptBlock<T> _to;
        readonly Action? _onComplete;

        public BufferScan(IReadOnlyBuffer<T> from, IAcceptBlock<T> to, Action? onComplete)
        {
            _from = from;
            _to = to;
            _onComplete = onComplete;
        }

        public Task Process(INotifyUser? notify = null, string? msg = null, CancellationToken ct = default)
        {
            var ret = _from.ForEachBlock(_to.Add, notify, msg, ct);
            return _onComplete is not null 
                ? ret.ContinueWith(_ => _onComplete(), ct) 
                : ret;
        }
    }
}
