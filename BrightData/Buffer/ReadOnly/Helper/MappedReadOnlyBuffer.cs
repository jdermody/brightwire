using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BrightData.Types;

namespace BrightData.Buffer.ReadOnly.Helper
{
    /// <summary>
    /// Adapts a buffer with a block mapper function
    /// </summary>
    /// <typeparam name="IT"></typeparam>
    /// <typeparam name="T"></typeparam>
    /// <param name="index"></param>
    /// <param name="mapper"></param>
    internal class MappedReadOnlyBuffer<IT, T>(IReadOnlyBufferWithMetaData<IT> index, BlockMapper<IT, T> mapper)
        : TypedBufferBase<T>, IReadOnlyBufferWithMetaData<T>
        where IT : notnull
        where T : notnull
    {
        public uint BlockSize => index.BlockSize;
        public uint BlockCount => index.BlockCount;
        public Type DataType => typeof(T);
        public uint Size => index.Size;

        public async Task ForEachBlock(BlockCallback<T> callback, INotifyOperationProgress? notify, string? msg, CancellationToken ct = default)
        {
            await index.ForEachBlock(x =>
            {
                var mapped = mapper(x);
                callback(mapped.Span);
            }, notify, msg, ct);
        }

        public override async Task<ReadOnlyMemory<T>> GetTypedBlock(uint blockIndex)
        {
            if (blockIndex >= BlockCount)
                return ReadOnlyMemory<T>.Empty;
            var indices = await index.GetTypedBlock(blockIndex);
            var ret = mapper(indices.Span);
            return ret;
        }

        public override async IAsyncEnumerable<T> EnumerateAllTyped()
        {
            for (uint i = 0; i < BlockCount; i++)
            {
                var block = await GetTypedBlock(i);
                for (var j = 0; j < block.Length; j++)
                    yield return block.Span[j];
            }
        }

        public MetaData MetaData => index.MetaData;
    }
}
