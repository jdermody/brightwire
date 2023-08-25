using System.Runtime.CompilerServices;
using CommunityToolkit.HighPerformance;

namespace BrightData.Table.Buffer.ReadOnly
{
    internal class BlockReaderReadOnlyBuffer<T> : IReadOnlyBufferWithMetaData<T> where T : unmanaged
    {
        readonly IByteBlockReader _reader;
        readonly uint _offset, _size, _sizeOfT;
        ReadOnlyMemory<T>? _lastBlock;
        uint _lastBlockIndex;

        public BlockReaderReadOnlyBuffer(MetaData metadata, IByteBlockReader reader, uint offset, uint byteSize, uint blockSize = Consts.DefaultBlockSize)
        {
            MetaData = metadata;
            _reader = reader;
            _offset = offset;
            _size = byteSize;
            _sizeOfT = (uint)Unsafe.SizeOf<T>();
            BlockSize = Math.Min(blockSize, byteSize / (uint)Unsafe.SizeOf<T>());
            var denominator = _sizeOfT * BlockSize;
            BlockCount = byteSize / denominator + (byteSize % denominator == 0 ? 0u : 1u);
        }

        public uint BlockSize { get; }
        public uint BlockCount { get; }
        public Type DataType => typeof(T);

        public async Task ForEachBlock(BlockCallback<T> callback)
        {
            for (uint i = 0; i < BlockCount; i++)
                callback((await GetBlock(i)).Span);
        }

        public async Task<ReadOnlyMemory<T>> GetBlock(uint blockIndex)
        {
            if (blockIndex >= BlockCount)
                return ReadOnlyMemory<T>.Empty;
            if (_lastBlockIndex == blockIndex && _lastBlock.HasValue)
                return _lastBlock.Value;

            _lastBlockIndex = blockIndex;
            var start = _offset + blockIndex * BlockSize * _sizeOfT;
            var end = Math.Min(start + BlockSize * _sizeOfT, _size);
            var byteBlock = await _reader.GetBlock(start, end);
            var ret = byteBlock.Cast<byte, T>();
            _lastBlock = ret;
            return ret;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator() => EnumerateAll().GetAsyncEnumerator();

        public async IAsyncEnumerable<T> EnumerateAll()
        {
            for (uint i = 0; i < BlockCount; i++) {
                var block = await GetBlock(i);
                for (var j = 0; j < block.Length; j++)
                    yield return block.Span[j];
            }
        }

        public MetaData MetaData { get; }
    }
}
