using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.Table.Operation
{
    internal class OneToOneMutation<FT, T> : IOperation
        where FT : notnull
        where T: notnull
    {
        readonly IReadOnlyBuffer<FT> _from;
        readonly IAppendToBuffer<T> _to;
        readonly Mutator _mutator;

        public delegate void Mutator(ReadOnlySpan<FT> from, IAppendToBuffer<T> to);

        public OneToOneMutation(IReadOnlyBuffer<FT> from, IAppendToBuffer<T> to, Mutator mutator)
        {
            _from = from;
            _to = to;
            _mutator = mutator;
        }

        public Task Process(INotifyUser? notify = null, string? msg = null, CancellationToken ct = default)
        {
            return _from.ForEachBlock(x => _mutator(x, _to), notify, msg, ct);
        }
    }
}
