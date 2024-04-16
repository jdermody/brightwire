﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BrightData.Buffer.Operations
{
    /// <summary>
    /// Copies specified indices from a buffer to a destination
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="indices"></param>
    internal class IndexedCopyOperation<T>(IReadOnlyBuffer<T> from, IAppendToBuffer<T> to, IEnumerable<uint> indices)
        : IOperation
        where T : notnull
    {
        public async Task Execute(INotifyOperationProgress? notify = null, string? msg = null, CancellationToken ct = default)
        {
            var blocks = from.GetIndices(indices)
                .GroupBy(x => x.BlockIndex)
                .OrderBy(x => x.Key)
            ;
            foreach (var block in blocks) {
                var blockMemory = await from.GetTypedBlock(block.Key);
                CopyIndices(blockMemory, block.Select(x => (int)x.RelativeBlockIndex));
            }
            return;

            void CopyIndices(ReadOnlyMemory<T> fromMemory, IEnumerable<int> copyIndices)
            {
                var span = fromMemory.Span;
                foreach(var item in copyIndices)
                    to.Append(span[item]);
            }
        }
    }
}
