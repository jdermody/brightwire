﻿using System.Runtime.CompilerServices;
using CommunityToolkit.HighPerformance;

namespace BrightData.Table.Buffer.ReadOnly
{
    internal class BlockReaderReadOnlyBuffer<T> : IReadOnlyBufferWithMetaData<T> where T : unmanaged
    {
        readonly IByteBlockReader _reader;
        readonly uint _offset, _byteSize, _sizeOfT;
        ReadOnlyMemory<T>? _lastBlock;
        uint _lastBlockIndex;

        public BlockReaderReadOnlyBuffer(MetaData metadata, IByteBlockReader reader, uint offset, uint byteSize, uint blockSize = Consts.DefaultBlockSize)
        {
            MetaData = metadata;
            _reader = reader;
            _offset = offset;
            _byteSize = byteSize;
            _sizeOfT = (uint)Unsafe.SizeOf<T>();
            BlockSize = Math.Min(blockSize, byteSize / (uint)Unsafe.SizeOf<T>());
            var denominator = _sizeOfT * BlockSize;
            BlockCount = byteSize / denominator + (byteSize % denominator == 0 ? 0u : 1u);
        }

        public uint BlockSize { get; }
        public uint BlockCount { get; }
        public Type DataType => typeof(T);
        public uint Size => _byteSize / _sizeOfT;

        public async Task ForEachBlock(BlockCallback<T> callback, INotifyUser? notify = null, string? msg = null, CancellationToken ct = default)
        {
            var guid = Guid.NewGuid();
            notify?.OnStartOperation(guid, msg);
            for (uint i = 0; i < BlockCount && !ct.IsCancellationRequested; i++) {
                callback((await GetTypedBlock(i)).Span);
                notify?.OnOperationProgress(guid, i / (float)BlockCount);
            }
            notify?.OnCompleteOperation(guid, ct.IsCancellationRequested);
        }

        public async Task<ReadOnlyMemory<T>> GetTypedBlock(uint blockIndex)
        {
            if (blockIndex >= BlockCount)
                return ReadOnlyMemory<T>.Empty;
            if (_lastBlockIndex == blockIndex && _lastBlock.HasValue)
                return _lastBlock.Value;

            _lastBlockIndex = blockIndex;
            var start = _offset + blockIndex * BlockSize * _sizeOfT;
            var end = Math.Min(start + BlockSize * _sizeOfT, _byteSize);
            var byteBlock = await _reader.GetBlock(start, end);
            var ret = byteBlock.Cast<byte, T>();
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
            for (uint i = 0; i < BlockCount; i++) {
                var block = await GetTypedBlock(i);
                for (var j = 0; j < block.Length; j++)
                    yield return block.Span[j];
            }
        }

        public async IAsyncEnumerable<object> EnumerateAll()
        {
            await foreach(var item in EnumerateAllTyped())
                yield return item;
        }

        public MetaData MetaData { get; }
    }
}
