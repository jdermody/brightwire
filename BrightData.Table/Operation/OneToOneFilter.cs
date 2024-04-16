using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.Table.Operation
{
    internal class OneToOneFilter<T> : IOperation
        where T: notnull
    {
        readonly IReadOnlyBuffer<T> _from;
        readonly IAppendToBuffer<T> _to;
        readonly Filter _filter;

        public delegate void Filter(ReadOnlySpan<T> from, IAppendToBuffer<T> to);

        public OneToOneFilter(IReadOnlyBuffer<T> from, IAppendToBuffer<T> to, Filter filter)
        {
            _from = from;
            _to = to;
            _filter = filter;
        }

        public Task Process(INotifyUser? notify = null, string? msg = null, CancellationToken ct = default)
        {
            return _from.ForEachBlock(x => _filter(x, _to), notify, msg, ct);
        }
    }
}
