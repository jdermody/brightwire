using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace BrightData.Operation
{
    internal class IndexedCopyOperation<T> : IOperation
        where T: notnull
    {
        readonly IReadOnlyBuffer<T> _from;
        readonly IAppendToBuffer<T> _to;
        readonly IEnumerable<uint> _indices;

        public IndexedCopyOperation(IReadOnlyBuffer<T> from, IAppendToBuffer<T> to, IEnumerable<uint> indices)
        {
            _from = from;
            _to = to;
            _indices = indices;
        }

        public async Task Process(INotifyUser? notify = null, string? msg = null, CancellationToken ct = default)
        {
            var blockSize = _from.BlockSize;
            var blocks = _indices.Select(x => (Index: x, BlockIndex: x / _from.BlockSize))
                .GroupBy(x => x.BlockIndex)
                .OrderBy(x => x.Key)
            ;
            foreach (var block in blocks) {
                var blockMemory = await _from.GetTypedBlock(block.Key);
                var baseOffset = block.Key * blockSize;
                CopyIndices(blockMemory, block.Select(x => (int)(x.Index - baseOffset)));
            }
            return;

            void CopyIndices(ReadOnlyMemory<T> from, IEnumerable<int> indices)
            {
                var span = from.Span;
                foreach(var item in indices)
                    _to.Add(span[item]);
            }
        }
    }
}
