using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BrightData.Buffer
{
    abstract class TypedBufferBase<T>
        where T : notnull
    {
        public abstract Task<ReadOnlyMemory<T>> GetTypedBlock(uint blockIndex);
        public abstract IAsyncEnumerable<T> EnumerateAllTyped();

        public async Task<Array> GetBlock(uint blockIndex)
        {
            var block = await GetTypedBlock(blockIndex);
            if (MemoryMarshal.TryGetArray(block, out var segment) && segment is { Offset: 0, Array: not null })
                return segment.Array;
            return block.ToArray();
        }
        public virtual async IAsyncEnumerable<object> EnumerateAll()
        {
            await foreach(var item in EnumerateAllTyped())
                yield return item;
        }
        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken ct = default) => EnumerateAllTyped().GetAsyncEnumerator(ct);
    }
}
