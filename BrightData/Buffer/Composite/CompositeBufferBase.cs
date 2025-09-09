using System;
using System.Collections.Generic;
using System.IO;
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
        where BT : IMutableBufferBlock<T>
    {
        protected delegate BT NewBlockFactory(Memory<T> block);
        protected delegate BT ExistingBlockFactory(ReadOnlyMemory<T> block);

        int                           _blockSize;
        readonly int                  _maxBlockSize;
        protected readonly uint?      _maxInMemoryBlocks, _maxDistinctItems;
        readonly NewBlockFactory      _newBlockFactory;
        readonly ExistingBlockFactory _existingBlockFactory;
        protected List<uint>?         _fileBlockSizes = null;
        IProvideByteBlocks?           _dataBlockProvider;
        protected IByteBlockSource?   _currentDataBlock;
        protected List<BT>?           _inMemoryBlocks;
        protected BT?                 _currBlock;
        protected HashSet<T>?         _distinct;

        protected CompositeBufferBase(
            NewBlockFactory newBlockFactory,
            ExistingBlockFactory existingBlockFactory,
            IProvideByteBlocks? dataBlockProvider = null,
            int blockSize = Consts.DefaultInitialBlockSize,
            int maxBlockSize = Consts.DefaultMaxBlockSize,
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        )
        {
            if (maxBlockSize < blockSize)
                throw new ArgumentException($"Expected max block size to be greater or equal to block size: block-size:{blockSize}, max-block-size:{maxBlockSize}");

            _newBlockFactory      = newBlockFactory;
            _existingBlockFactory = existingBlockFactory;
            _dataBlockProvider    = dataBlockProvider;
            _blockSize            = blockSize;
            _maxInMemoryBlocks    = maxInMemoryBlocks;
            _maxBlockSize         = maxBlockSize;

            if ((_maxDistinctItems = maxDistinctItems) > 0)
                _distinct = new HashSet<T>((int)maxDistinctItems!.Value / 32);
        }

        public Guid Id { get; } = Guid.NewGuid();
        public MetaData MetaData { get; set; } = new();
        public uint Size { get; protected set; }
        public uint BlockCount { get; private set; }
        public uint? DistinctItems => (uint?)_distinct?.Count;
        public Type DataType => typeof(T);

        public uint[] BlockSizes
        {
            get
            {
                var ret = new uint[BlockCount];
                var index = 0;
                if (_inMemoryBlocks is not null) {
                    foreach (var block in _inMemoryBlocks) {
                        ret[index++] = block.Size;
                    }
                }

                if (_fileBlockSizes is not null) {
                    foreach (var blockSize in _fileBlockSizes)
                        ret[index++] = blockSize;
                }


                if (_currBlock is not null)
                    ret[index] = _currBlock.Size;
                return ret;
            }
        }

        public async Task ForEachBlock(BlockCallback<T> callback, CancellationToken ct = default)
        {
            // read from in memory blocks
            if (_inMemoryBlocks is not null) {
                foreach (var block in _inMemoryBlocks) {
                    if (ct.IsCancellationRequested)
                        break;
                    callback(block.WrittenSpan);
                }
            }

            // read from the file
            if (_currentDataBlock != null && _fileBlockSizes != null) {
                var fileBlockIndex = 0;
                uint fileLength = _currentDataBlock.Size, byteOffset = 0;
                while (byteOffset < fileLength && !ct.IsCancellationRequested) {
                    var (size, block) = await GetBlockFromFile(_currentDataBlock, byteOffset, _fileBlockSizes[fileBlockIndex++]);
                    callback(block.Span);
                    byteOffset += size;
                }
            }

            // then from the current block
            if (_currBlock is not null && !ct.IsCancellationRequested)
                callback(_currBlock.WrittenSpan);
        }

        public override async IAsyncEnumerable<T> EnumerateAllTyped()
        {
            // read from in memory blocks
            if (_inMemoryBlocks is not null) {
                foreach (var block in _inMemoryBlocks) {
                    var data = block.WrittenMemory;
                    for (var i = 0; i < data.Length; i++) {
                        yield return data.Span[i];
                    }
                }
            }

            // read from the file
            if (_currentDataBlock != null && _fileBlockSizes != null) {
                var fileBlockIndex = 0;
                uint fileLength = _currentDataBlock.Size, byteOffset = 0;
                while (byteOffset < fileLength) {
                    var (size, block) = await GetBlockFromFile(_currentDataBlock, byteOffset, _fileBlockSizes[fileBlockIndex++]);
                    for (var i = 0; i < block.Length; i++) {
                        yield return block.Span[i];
                    }
                    byteOffset += size;
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
            if (blockIndex >= BlockCount)
                throw new ArgumentOutOfRangeException(nameof(blockIndex), $"Block index {blockIndex} exceeds total block count {BlockCount}");
            
            uint currentIndex = 0;
            var inMemoryBlocks = (uint)(_inMemoryBlocks?.Count ?? 0);
            var inFileBlocks = (uint)(_fileBlockSizes?.Count ?? 0);

            // check in memory blocks
            if (_inMemoryBlocks is not null) {
                if (blockIndex < inMemoryBlocks) {
                    var inMemoryBlock = _inMemoryBlocks[(int)blockIndex];
                    return inMemoryBlock.WrittenMemory;
                }
                currentIndex = inMemoryBlocks;
            }

            // check file blocks
            if (_currentDataBlock != null && _fileBlockSizes != null) {
                var fileBlockIndex = 0;
                if (blockIndex < inMemoryBlocks + inFileBlocks) {
                    uint fileLength = _currentDataBlock.Size, byteOffset = 0;
                    while (byteOffset < fileLength) {
                        if (currentIndex++ == blockIndex)
                            return (await GetBlockFromFile(_currentDataBlock, byteOffset, _fileBlockSizes[fileBlockIndex])).Block;
                        byteOffset += await SkipFileBlock(_currentDataBlock, byteOffset, _fileBlockSizes[fileBlockIndex++]);
                    }
                }
            }

            // check current block
            return _currBlock is not null 
                ? _currBlock.WrittenMemory 
                : throw new InvalidOperationException("Unexpected - failed to find block")
            ;
        }

        public virtual void Append(in T item)
        {
            if (ConstraintValidator?.Allow(item) == false)
                return;

            var block = EnsureCurrentBlock().GetAwaiter().GetResult();
            block.GetNext() = item;
            if (_distinct?.Add(item) == true && _distinct.Count > _maxDistinctItems)
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

        protected async Task<BT> EnsureCurrentBlock()
        {
            if (_currBlock?.HasFreeCapacity != true) {
                if (_currBlock is not null) {
                    if (_maxInMemoryBlocks.HasValue && (_inMemoryBlocks?.Count ?? 0) >= _maxInMemoryBlocks.Value) {
                        _currentDataBlock ??= (_dataBlockProvider ??= new TempFileProvider()).Get(Id);
                        (_fileBlockSizes ??= []).Add(_currBlock.Size);
                        await _currBlock.WriteTo(_currentDataBlock);
                    }
                    else
                        (_inMemoryBlocks ??= []).Add(_currBlock);
                }
                _currBlock = _newBlockFactory(new T[_blockSize]);
                ++BlockCount;

                // increase the size of the next block
                var nextBlockSize = _blockSize * 2;
                if (nextBlockSize > _maxBlockSize)
                    nextBlockSize = _maxBlockSize;
                _blockSize = nextBlockSize;
            }
            return _currBlock!;
        }

        public void AppendObject(object obj) => Append((T)obj);

        public async Task WriteTo(Stream stream)
        {
            // write the header
            await using var writer = new BinaryWriter(stream, Encoding.UTF8, true);
            writer.Write(Size);
            writer.Write(BlockCount);
            writer.Flush();

            // create space for the block positions
            var headerPosition = stream.Position;
            var blockPositions = new (long Offset, uint ByteSize, uint Count)[BlockCount];
            stream.Seek((sizeof(long) + sizeof(uint) + sizeof(uint)) * BlockCount, SeekOrigin.Current);
            stream.SetLength(stream.Position);

            var index = 0;
            var dataBlock = stream.AsDataBlock();
            var pos = (uint)stream.Position;

            // write from in memory blocks
            if (_inMemoryBlocks is not null) {
                foreach (var block in _inMemoryBlocks) {
                    var blockSize = await block.WriteTo(dataBlock);
                    blockPositions[index++] = (pos, blockSize, block.Size);
                    pos += blockSize;
                }
            }

            // write from the file
            if (_currentDataBlock != null && _fileBlockSizes != null) {
                var fileBlockIndex = 0;
                uint fileLength = _currentDataBlock.Size, byteOffset = 0;
                while (byteOffset < fileLength) {
                    var (size, blockData) = await GetBlockFromFile(_currentDataBlock, byteOffset, _fileBlockSizes[fileBlockIndex++]);
                    byteOffset += size;

                    var block = _existingBlockFactory(blockData);
                    var writeSize = await block.WriteTo(dataBlock);
                    blockPositions[index++] = (pos, writeSize, block.Size);
                    pos += writeSize;
                }
            }

            // write current block
            if (_currBlock is not null) {
                var blockSize = await _currBlock.WriteTo(dataBlock);
                blockPositions[index] = (pos, blockSize, _currBlock.Size);
            }

            // write the block header
            stream.Seek(headerPosition, SeekOrigin.Begin);
            foreach (var (startPos, byteSize, count) in blockPositions) {
                writer.Write(startPos);
                writer.Write(byteSize);
                writer.Write(count);
            }

            // move the stream position to the end position
            stream.Seek(0, SeekOrigin.End);
        }

        protected abstract Task<uint> SkipFileBlock(IByteBlockSource file, uint byteOffset, uint numItemsInBlock);
        protected abstract Task<(uint BlockSizeBytes, ReadOnlyMemory<T> Block)> GetBlockFromFile(IByteBlockSource file, uint byteOffset, uint numItemsInBlock);
        public override string ToString() => $"Composite buffer ({typeof(T).Name})|{MetaData.GetName(Id.ToString("n"))}|count={Size:N0}";
    }
}
