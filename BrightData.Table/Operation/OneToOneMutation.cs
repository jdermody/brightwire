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
        readonly IWriteMutatedBlocks _mutator;

        internal interface IWriteMutatedBlocks
        {
            void Write(ReadOnlySpan<FT> from, IAppendToBuffer<T> to);
        }

        public OneToOneMutation(IReadOnlyBuffer<FT> from, IAppendToBuffer<T> to, IWriteMutatedBlocks mutator)
        {
            _from = from;
            _to = to;
            _mutator = mutator;
        }

        public Task Process(INotifyUser? notify = null, string? msg = null, CancellationToken ct = default)
        {
            return _from.ForEachBlock(x => _mutator.Write(x, _to), notify, msg, ct);
        }
    }
}
