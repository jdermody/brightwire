using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BrightData.Types;

namespace BrightData.Buffer.ReadOnly.Helper
{
    internal delegate ReadOnlyMemory<T> BlockMapper<FT, T>(ReadOnlySpan<FT> span);

    /// <summary>
    /// Adapts a buffer with a block mapper function
    /// </summary>
    /// <typeparam name="IT"></typeparam>
    /// <typeparam name="T"></typeparam>
    /// <param name="index"></param>
    /// <param name="mapper"></param>
    internal class MappedReadOnlyBuffer<IT, T>(IReadOnlyBufferWithMetaData<IT> index, BlockMapper<IT, T> mapper)
        : IReadOnlyBufferWithMetaData<T>
        where IT : notnull
        where T : notnull
    {
        //ReadOnlyMemory<T>? _lastBlock;
        //uint               _lastBlockIndex;

        public uint BlockSize => index.BlockSize;
        public uint BlockCount => index.BlockCount;
        public Type DataType => typeof(T);
        public uint Size => index.Size;

        public async Task ForEachBlock(BlockCallback<T> callback, INotifyUser? notify, string? msg, CancellationToken ct = default)
        {
            await index.ForEachBlock(x =>
            {
                var mapped = mapper(x);
                callback(mapped.Span);
            }, notify, msg, ct);
        }

        public async Task<ReadOnlyMemory<T>> GetTypedBlock(uint blockIndex)
        {
            if (blockIndex >= BlockCount)
                return ReadOnlyMemory<T>.Empty;
            //if (_lastBlockIndex == blockIndex && _lastBlock.HasValue)
            //    return _lastBlock.Value;

            //_lastBlockIndex = blockIndex;
            var indices = await index.GetTypedBlock(blockIndex);
            var ret = mapper(indices.Span);
            //_lastBlock = ret;
            return ret;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken ct = default) => EnumerateAllTyped().GetAsyncEnumerator(ct);

        public async IAsyncEnumerable<T> EnumerateAllTyped()
        {
            for (uint i = 0; i < BlockCount; i++)
            {
                var block = await GetTypedBlock(i);
                for (var j = 0; j < block.Length; j++)
                    yield return block.Span[j];
            }
        }

        public async IAsyncEnumerable<object> EnumerateAll()
        {
            await foreach (var item in EnumerateAllTyped())
                yield return item;
        }

        public MetaData MetaData => index.MetaData;
    }
}
