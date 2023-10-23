using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BrightData.Buffer.ReadOnly.Helper
{
    public delegate ReadOnlyMemory<T> BlockMapper<FT, T>(ReadOnlySpan<FT> span);
    internal class MappedReadOnlyBuffer<IT, T> : IReadOnlyBufferWithMetaData<T>
        where IT : notnull
        where T : notnull
    {
        readonly IReadOnlyBufferWithMetaData<IT> _index;
        readonly BlockMapper<IT, T> _mapper;
        ReadOnlyMemory<T>? _lastBlock;
        uint _lastBlockIndex;

        public MappedReadOnlyBuffer(IReadOnlyBufferWithMetaData<IT> index, BlockMapper<IT, T> mapper)
        {
            _index = index;
            _mapper = mapper;
        }

        public uint BlockSize => _index.BlockSize;
        public uint BlockCount => _index.BlockCount;
        public Type DataType => typeof(T);
        public uint Size => _index.Size;

        public async Task ForEachBlock(BlockCallback<T> callback, INotifyUser? notify, string? msg, CancellationToken ct = default)
        {
            await _index.ForEachBlock(x =>
            {
                var mapped = _mapper(x);
                callback(mapped.Span);
            }, notify, msg, ct);
        }

        public async Task<ReadOnlyMemory<T>> GetTypedBlock(uint blockIndex)
        {
            if (blockIndex >= BlockCount)
                return ReadOnlyMemory<T>.Empty;
            if (_lastBlockIndex == blockIndex && _lastBlock.HasValue)
                return _lastBlock.Value;

            _lastBlockIndex = blockIndex;
            var indices = await _index.GetTypedBlock(blockIndex);
            var ret = _mapper(indices.Span);
            _lastBlock = ret;
            return ret;
        }

        public async Task<ReadOnlyMemory<object>> GetBlock(uint blockIndex)
        {
            var block = await GetTypedBlock(blockIndex);
            return block.AsObjects();
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

        public MetaData MetaData => _index.MetaData;
    }
}
