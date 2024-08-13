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
        public uint[] BlockSizes => index.BlockSizes;
        public uint BlockCount { get; } = (uint)index.BlockSizes.Length;
        public Type DataType => typeof(T);
        public uint Size => index.Size;

        public async Task ForEachBlock(BlockCallback<T> callback, CancellationToken ct = default)
        {
            for (uint i = 0; i < BlockCount && !ct.IsCancellationRequested; i++)
            {
                var block = await GetTypedBlock(i);
                callback(block.Span);
            }
        }

        public override async Task<ReadOnlyMemory<T>> GetTypedBlock(uint blockIndex)
        {
            if (blockIndex >= BlockCount)
                return ReadOnlyMemory<T>.Empty;
            var indices = await index.GetTypedBlock(blockIndex);
            var ret = await mapper(indices);
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
