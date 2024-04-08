using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BrightData.Buffer.Operations
{
    internal class ManyToOneMutation<FT, TT> : IOperation
        where FT : notnull
        where TT : notnull
    {
        readonly uint                  _size, _blockSize;
        readonly IReadOnlyBuffer<FT>[] _from;
        readonly Func<FT[], TT>        _mutator;
        readonly IAppendToBuffer<TT>   _to;

        public ManyToOneMutation(IEnumerable<IReadOnlyBuffer<FT>> from, IAppendToBuffer<TT> to, Func<FT[], TT> mutator)
        {
            _from = from.ToArray();
            _size = _from.First().Size;
            if (_from.Any(x => x.Size != _size))
                throw new ArgumentException("Expected all input buffers to have the same size", nameof(from));
            _blockSize = (uint)_from.Average(x => x.BlockSizes.Average(y => y));
            _mutator = mutator;
            _to = to;
        }

        public async Task Execute(INotifyOperationProgress? notify = null, string? msg = null, CancellationToken ct = default)
        {
            var id = Guid.NewGuid();
            notify?.OnStartOperation(id, msg);
            // ReSharper disable once NotDisposedResourceIsReturned
            var enumerators = _from.Select(x => x.GetAsyncEnumerator(ct)).ToArray();
            var currentTasks = new ValueTask<bool>[_size];
            var curr = new FT[_size];
            uint index = 0;
            var isValid = true;

            while (!ct.IsCancellationRequested && isValid) {
                for (var i = 0; i < _size; i++)
                    currentTasks[i] = enumerators[i].MoveNextAsync();
                for (var i = 0; i < _size; i++) {
                    if (await currentTasks[i] != true) {
                        isValid = false;
                        break;
                    }
                }

                if (isValid) {
                    for (var i = 0; i < _size; i++)
                        curr[i] = enumerators[i].Current;
                    _to.Append(_mutator(curr));
                    if (++index % _blockSize == 0)
                        notify?.OnOperationProgress(id, (float)index / _size);
                }
            }

            foreach (var item in enumerators)
                await item.DisposeAsync();

            notify?.OnCompleteOperation(id, ct.IsCancellationRequested);
        }
    }
}
