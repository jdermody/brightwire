using BrightData.Table.Helper;
using Microsoft.Win32.SafeHandles;

namespace BrightData.Table.Buffer.Composite
{
    internal interface ICompositeBufferBlock<T>
    {
        uint Size { get; }
        void WriteTo(ITempData file);
        bool HasFreeCapacity { get; }
        ReadOnlySpan<T> WrittenSpan { get; }
        ReadOnlyMemory<T> WrittenMemory { get; }
        ref T GetNext();
    }
    internal abstract class CompositeBufferBase<T, BT> : ICompositeBuffer<T>
        where T : notnull
        where BT : ICompositeBufferBlock<T>
    {
        protected readonly int   _blockSize;
        protected readonly uint? _maxInMemoryBlocks, _maxDistinctItems;
        readonly Func<T[], BT>   _blockFactory;
        IProvideTempData?        _tempStreams;
        protected ITempData?     _tempData;
        protected List<BT>?      _inMemoryBlocks;
        protected BT?            _currBlock;
        protected HashSet<T>?    _distinct;
        protected uint           _blocksInFile;

        protected CompositeBufferBase(
            Func<T[], BT> blockFactory,
            IProvideTempData? tempStreams = null,
            int blockSize = Consts.DefaultBlockSize,
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        )
        {
            _blockFactory      = blockFactory;
            _tempStreams       = tempStreams;
            _blockSize         = blockSize;
            _maxInMemoryBlocks = maxInMemoryBlocks;

            if ((_maxDistinctItems = maxDistinctItems) > 0)
            {
                _distinct = new HashSet<T>((int)maxDistinctItems!.Value / 32);
            }
        }

        public Guid Id { get; } = Guid.NewGuid();
        public MetaData MetaData { get; set; } = new();
        public uint Size { get; protected set; }
        public uint BlockCount { get; private set; }
        public uint BlockSize => (uint)_blockSize;
        public uint? DistinctItems => (uint?)_distinct?.Count;
        public Type DataType => typeof(T);

        public async Task ForEachBlock(BlockCallback<T> callback, INotifyUser? notify = null, string? message = null, CancellationToken ct = default)
        {
            var guid = Guid.NewGuid();
            notify?.OnStartOperation(guid, message);
            var count = 0;

            // read from in memory blocks
            if (_inMemoryBlocks is not null)
            {
                foreach (var block in _inMemoryBlocks) {
                    if (ct.IsCancellationRequested)
                        break;
                    callback(block.WrittenSpan);
                    notify?.OnOperationProgress(guid, (float)++count / BlockCount);
                }
            }

            // read from the file
            if (_tempData != null)
            {
                uint fileLength = _tempData.Size, offset = 0;
                while (offset < fileLength && !ct.IsCancellationRequested)
                {
                    offset += await GetBlockFromFile(_tempData, offset, callback);
                    notify?.OnOperationProgress(guid, (float)++count / BlockCount);
                }
            }

            // then from the current block
            if (_currBlock is not null && !ct.IsCancellationRequested) {
                callback(_currBlock.WrittenSpan);
                notify?.OnOperationProgress(guid, (float)++count / BlockCount);
            }
            notify?.OnCompleteOperation(guid, ct.IsCancellationRequested);
        }

        public async IAsyncEnumerable<T> EnumerateAllTyped()
        {
            // read from in memory blocks
            if (_inMemoryBlocks is not null)
            {
                foreach (var block in _inMemoryBlocks)
                {
                    var data = block.WrittenMemory;
                    for (var i = 0; i < data.Length; i++) {
                        yield return data.Span[i];
                    }
                }
            }

            // read from the file
            if (_tempData != null)
            {
                uint fileLength = _tempData.Size, offset = 0;
                while (offset < fileLength)
                {
                    var (size, block) = await GetBlockFromFile(_tempData, offset);
                    for (var i = 0; i < block.Length; i++) {
                        yield return block.Span[i];
                    }
                    offset += size;
                }
            }

            if (_currBlock is not null) {
                for (var i = 0; i < _currBlock.WrittenMemory.Length; i++) {
                    yield return _currBlock.WrittenMemory.Span[i];
                }
            }
        }

        public async IAsyncEnumerable<object> EnumerateAll()
        {
            await foreach(var item in EnumerateAllTyped())
                yield return item;
        }

        public virtual async Task<ReadOnlyMemory<T>> GetBlock(uint blockIndex)
        {
            uint currentIndex = 0;

            // read from in memory blocks
            if (_inMemoryBlocks is not null)
            {
                if (blockIndex < _blocksInFile + (uint)_inMemoryBlocks.Count)
                {
                    foreach (var block in _inMemoryBlocks)
                    {
                        if (currentIndex++ == blockIndex)
                        {
                            return block.WrittenMemory;
                        }
                    }
                }
                else
                    currentIndex = _blocksInFile + (uint)_inMemoryBlocks.Count;
            }

            // read from the file
            if (_tempData != null)
            {
                if (blockIndex < _blocksInFile)
                {
                    uint fileLength = _tempData.Size, offset = 0;
                    while (offset < fileLength)
                    {
                        if (currentIndex++ == blockIndex)
                            return (await GetBlockFromFile(_tempData, offset)).Block;
                        offset += await SkipFileBlock(_tempData, offset);
                    }
                }
                else
                    currentIndex = _blocksInFile;
            }

            // then from the current block
            if (_currBlock is not null && currentIndex == blockIndex)
                return _currBlock.WrittenMemory;
            throw new Exception("Unexpected - failed to find block");
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken ct = default) => EnumerateAllTyped().GetAsyncEnumerator(ct);

        public virtual void Add(in T item)
        {
            EnsureCurrentBlock().GetNext() = item;
            if (_distinct?.Add(item) == true && _distinct.Count >= _maxDistinctItems)
                _distinct = null;
            ++Size;
        }

        public virtual void Add(ReadOnlySpan<T> inputBlock)
        {
            foreach (var item in inputBlock)
                Add(item);
        }

        public IReadOnlySet<T>? DistinctSet => _distinct;

        protected BT EnsureCurrentBlock()
        {
            if (_currBlock?.HasFreeCapacity != true)
            {
                if (_currBlock is not null)
                {
                    if (_maxInMemoryBlocks.HasValue && (_inMemoryBlocks?.Count ?? 0) >= _maxInMemoryBlocks.Value)
                    {
                        _tempData ??= (_tempStreams ??= new TempFileProvider()).Get(Id);
                        _currBlock.WriteTo(_tempData);
                        ++_blocksInFile;
                    }
                    else
                        (_inMemoryBlocks ??= new()).Add(_currBlock);
                }
                _currBlock = _blockFactory(new T[_blockSize]);
                ++BlockCount;
            }
            return _currBlock!;
        }

        protected abstract Task<uint> SkipFileBlock(ITempData file, uint offset);
        protected abstract Task<(uint Offset, ReadOnlyMemory<T> Block)> GetBlockFromFile(ITempData file, uint offset);
        protected abstract Task<uint> GetBlockFromFile(ITempData file, uint offset, BlockCallback<T> callback);
        public override string ToString() => $"Composite buffer ({typeof(T).Name})|{MetaData.GetName(Id.ToString("n"))}|count={Size:N0}";
    }
}
