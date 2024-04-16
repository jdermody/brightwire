using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace BrightData.Table.Operation
{
    internal class ManyToManyFilter<T> : IOperation
        where T: notnull
    {
        readonly uint _size, _blockSize;
        readonly IReadOnlyBuffer<T>[] _from;
        readonly IAppendToBuffer<T>[] _to;
        readonly Filter _filter;
        public delegate void Filter(T[] from, IAppendToBuffer<T>[] to);

        public ManyToManyFilter(IEnumerable<IReadOnlyBuffer<T>> from, IEnumerable<IAppendToBuffer<T>> to, Filter filter)
        {
            _from = from.ToArray();
            _size = _from.First().Size;
            if (_from.Any(x => x.Size != _size))
                throw new ArgumentException("Expected all input buffers to have the same size", nameof(from));
            _blockSize = (uint)_from.Average(x => x.BlockSize);
            _to = to.ToArray();
            _filter = filter;                                                                                                                                           
        }

        public async Task Process(INotifyUser? notify = null, string? msg = null, CancellationToken ct = default)
        {
            var id = Guid.NewGuid();
            notify?.OnStartOperation(id, msg);
            var enumerators = _from.Select(x => x.GetAsyncEnumerator(ct)).ToArray();
            var currentTasks = new ValueTask<bool>[_size];
            var curr = new T[_size];
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
                    curr[i] = enumerators[i].Current;
                _filter(curr, _to);
                if(++index % _blockSize == 0)
                    notify?.OnOperationProgress(id, (float)index / _size);
            }

            notify?.OnCompleteOperation(id, ct.IsCancellationRequested);
        }
    }
}
