using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BrightData.Buffer.Operations
{
    /// <summary>
    /// Copies from many buffers to many destinations
    /// </summary>
    internal class ManyToManyCopy : IOperation
    {
        readonly uint                           _size, _blockSize;
        readonly IReadOnlyList<IReadOnlyBuffer> _from;
        readonly IReadOnlyList<IAppendToBuffer> _to;

        public ManyToManyCopy(IReadOnlyList<IReadOnlyBuffer> from, IReadOnlyList<IAppendToBuffer> to)
        {
            _from = from;
            var size = _from[0].Size;
            if (_from.Any(x => x.Size != size))
                throw new ArgumentException("Expected all input buffers to have the same size", nameof(from));
            _size = (uint)_from.Count;
            _blockSize = (uint)_from.Average((IReadOnlyBuffer x) => x.BlockSizes.Average(y => y));
            _to = to;
        }

        public uint CopiedCount { get; private set; }

        public async Task Execute(INotifyOperationProgress? notify = null, string? msg = null, CancellationToken ct = default)
        {
            var id = Guid.NewGuid();
            notify?.OnStartOperation(id, msg);
            // ReSharper disable once NotDisposedResourceIsReturned
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

                if (isValid) {
                    for (var i = 0; i < _size; i++) {
                        _to[i].AppendObject(enumerators[i].Current);
                        ++CopiedCount;
                    }

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
