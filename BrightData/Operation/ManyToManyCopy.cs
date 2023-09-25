using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BrightData.Operation
{
    internal class ManyToManyCopy : IOperation
    {
        readonly uint _size, _blockSize;
        readonly IReadOnlyList<IReadOnlyBuffer> _from;
        readonly IReadOnlyList<IAppendToBuffer> _to;

        public ManyToManyCopy(IReadOnlyList<IReadOnlyBuffer> from, IReadOnlyList<IAppendToBuffer> to)
        {
            _from = from;
            _size = _from.First().Size;
            if (_from.Any(x => x.Size != _size))
                throw new ArgumentException("Expected all input buffers to have the same size", nameof(from));
            _blockSize = (uint)_from.Average(x => x.BlockSize);
            _to = to;
        }

        public async Task Process(INotifyUser? notify = null, string? msg = null, CancellationToken ct = default)
        {
            var id = Guid.NewGuid();
            notify?.OnStartOperation(id, msg);
            var enumerators = _from.Select(x => x.EnumerateAll().GetAsyncEnumerator(ct)).ToArray();
            var currentTasks = new ValueTask<bool>[_size];
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
                for (var i = 0; i < _size; i++)
                    _to[i].AddObject(enumerators[i].Current);

                if(++index % _blockSize == 0)
                    notify?.OnOperationProgress(id, (float)index / _size);
            }

            notify?.OnCompleteOperation(id, ct.IsCancellationRequested);
        }
    }
}
