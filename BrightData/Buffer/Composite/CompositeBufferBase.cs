using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BrightData.Helper;
using BrightData.Types;

namespace BrightData.Buffer.Composite
{
    /// <summary>
    /// Base class for composite buffers
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="BT"></typeparam>
    internal abstract class CompositeBufferBase<T, BT> : TypedBufferBase<T>, ICompositeBuffer<T>
        where T : notnull
        where BT : ICompositeBufferBlock<T>
    {
        protected readonly int        _blockSize;
        protected readonly uint?      _maxInMemoryBlocks, _maxDistinctItems;
        readonly Func<T[], bool , BT> _blockFactory;
        IProvideDataBlocks?           _dataBlockProvider;
        protected IByteBlockSource?   _currentDataBlock;
        protected List<BT>?           _inMemoryBlocks;
        protected BT?                 _currBlock;
        protected HashSet<T>?         _distinct;
        protected uint                _blocksInFile;

        protected CompositeBufferBase(
            Func<T[], bool, BT> blockFactory,
            IProvideDataBlocks? dataBlockProvider = null,
            int blockSize = Consts.DefaultBlockSize,
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        )
        {
            _blockFactory      = blockFactory;
            _dataBlockProvider = dataBlockProvider;
            _blockSize         = blockSize;
            _maxInMemoryBlocks = maxInMemoryBlocks;

            if ((_maxDistinctItems = maxDistinctItems) > 0)
                _distinct = new HashSet<T>((int)maxDistinctItems!.Value / 32);
        }

        public Guid Id { get; } = Guid.NewGuid();
        public MetaData MetaData { get; set; } = new();
        public uint Size { get; protected set; }
        public uint BlockCount { get; private set; }
        public uint BlockSize => (uint)_blockSize;
        public uint? DistinctItems => (uint?)_distinct?.Count;
        public Type DataType => typeof(T);

        public async Task ForEachBlock(BlockCallback<T> callback, INotifyOperationProgress? notify = null, string? message = null, CancellationToken ct = default)
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
            if (_currentDataBlock != null)
            {
                uint fileLength = _currentDataBlock.Size, offset = 0;
                while (offset < fileLength && !ct.IsCancellationRequested)
                {
                    offset += await GetBlockFromFile(_currentDataBlock, offset, callback);
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

        public override async IAsyncEnumerable<T> EnumerateAllTyped()
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
            if (_currentDataBlock != null)
            {
                uint fileLength = _currentDataBlock.Size, offset = 0;
                while (offset < fileLength)
                {
                    var (size, block) = await GetBlockFromFile(_currentDataBlock, offset);
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

        public override async Task<ReadOnlyMemory<T>> GetTypedBlock(uint blockIndex)
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
            if (_currentDataBlock != null)
            {
                if (blockIndex < _blocksInFile)
                {
                    uint fileLength = _currentDataBlock.Size, offset = 0;
                    while (offset < fileLength)
                    {
                        if (currentIndex++ == blockIndex)
                            return (await GetBlockFromFile(_currentDataBlock, offset)).Block;
                        offset += await SkipFileBlock(_currentDataBlock, offset);
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

        public virtual void Append(in T item)
        {
            if (ConstraintValidator?.Allow(item) == false)
                return;

            EnsureCurrentBlock().GetNext() = item;
            if (_distinct?.Add(item) == true && _distinct.Count >= _maxDistinctItems)
                _distinct = null;
            ++Size;
        }

        public virtual void Append(ReadOnlySpan<T> inputBlock)
        {
            foreach (var item in inputBlock)
                Append(item);
        }

        public IReadOnlySet<T>? DistinctSet => _distinct;
        public IConstraintValidator<T>? ConstraintValidator { get; set; }

        protected BT EnsureCurrentBlock()
        {
            if (_currBlock?.HasFreeCapacity != true)
            {
                if (_currBlock is not null)
                {
                    if (_maxInMemoryBlocks.HasValue && (_inMemoryBlocks?.Count ?? 0) >= _maxInMemoryBlocks.Value)
                    {
                        _currentDataBlock ??= (_dataBlockProvider ??= new TempFileProvider()).Get(Id);
                        _currBlock.WriteTo(_currentDataBlock);
                        ++_blocksInFile;
                    }
                    else
                        (_inMemoryBlocks ??= []).Add(_currBlock);
                }
                _currBlock = _blockFactory(new T[_blockSize], false);
                ++BlockCount;
            }
            return _currBlock!;
        }

        public void AppendObject(object obj) => Append((T)obj);

        public async Task WriteTo(Stream stream)
        {
            // write the header
            await using var writer = new BinaryWriter(stream, Encoding.UTF8, true);
            writer.Write(Size);
            writer.Write(BlockSize);
            writer.Write(BlockCount);
            writer.Flush();

            var headerPosition = stream.Position;
            var blockPositions = new (long Offset, uint Size)[BlockCount];
            stream.Seek((sizeof(long) + sizeof(uint)) * BlockCount, SeekOrigin.Current);
            stream.SetLength(stream.Position);
            var index = 0;
            var dataBlock = stream.AsDataBlock();
            var pos = (uint)stream.Position;

            // write from in memory blocks
            if (_inMemoryBlocks is not null)
            {
                foreach (var block in _inMemoryBlocks) {
                    var blockSize = await block.WriteTo(dataBlock);
                    blockPositions[index++] = (pos, blockSize);
                    pos += blockSize;
                }
            }

            // write from the file
            if (_currentDataBlock != null)
            {
                uint fileLength = _currentDataBlock.Size, offset = 0;
                while (offset < fileLength)
                {
                    var (size, blockData) = await GetBlockFromFile(_currentDataBlock, offset);
                    var block = _blockFactory(blockData.ToArray(), true);
                    var blockSize = await block.WriteTo(dataBlock);
                    offset += size;
                    blockPositions[index++] = (pos, blockSize);
                    pos += blockSize;
                }
            }

            // write current block
            if (_currBlock is not null) {
                var blockSize = await _currBlock.WriteTo(dataBlock);
                blockPositions[index] = (pos, blockSize);
            }

            stream.Seek(headerPosition, SeekOrigin.Begin);
            foreach (var (startPos, size) in blockPositions) {
                writer.Write(startPos);
                writer.Write(size);
            }

            // move the stream position to the end position
            stream.Seek(0, SeekOrigin.End);
        }

        protected abstract Task<uint> SkipFileBlock(IByteBlockSource file, uint offset);
        protected abstract Task<(uint Offset, ReadOnlyMemory<T> Block)> GetBlockFromFile(IByteBlockSource file, uint offset);
        protected abstract Task<uint> GetBlockFromFile(IByteBlockSource file, uint offset, BlockCallback<T> callback);
        public override string ToString() => $"Composite buffer ({typeof(T).Name})|{MetaData.GetName(Id.ToString("n"))}|count={Size:N0}";
    }
}
