using BrightData.Table.Helper;
using CommunityToolkit.HighPerformance.Buffers;
using Microsoft.Win32.SafeHandles;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.Table.Buffer
{
    internal interface ICompositeBufferBlock<T>
    {
        uint Size { get; }
        void WriteTo(SafeFileHandle file);
        bool HasFreeCapacity { get; }
        ReadOnlySpan<T> WrittenSpan { get; }
        ReadOnlyMemory<T> WrittenMemory { get; }
        ref T GetNext();
    }
    internal abstract class CompositeBufferBase<T, BT> : ICompositeBuffer<T> 
        where T: notnull 
        where BT: ICompositeBufferBlock<T>
    {
        protected readonly int    _blockSize;
        protected readonly uint?  _maxInMemoryBlocks, _maxDistinctItems;
        readonly Func<T[], BT>    _blockFactory;
        IProvideTempStreams?      _tempStreams;
        protected SafeFileHandle? _file;
        protected List<BT>?       _inMemoryBlocks;
        protected BT?             _currBlock;
        protected HashSet<T>?     _distinct;
        protected uint            _blocksInFile = 0;

        protected CompositeBufferBase(
            Func<T[], BT> blockFactory,
            IProvideTempStreams? tempStreams = null,
            int blockSize = Consts.DefaultBlockSize,
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        )
        {
            _blockFactory          = blockFactory;
            _tempStreams           = tempStreams;
            _blockSize             = blockSize;
            _maxInMemoryBlocks     = maxInMemoryBlocks;
            if ((_maxDistinctItems = maxDistinctItems) > 0) {
                _distinct          = new HashSet<T>((int)maxDistinctItems!.Value / 32);
            }
        }

        public Guid Id { get; } = Guid.NewGuid();
        public MetaData MetaData { get; set; } = new();
        public uint Size { get; protected set; }
        public uint BlockCount { get; private set; }
        public uint? DistinctItems => (uint?)_distinct?.Count;
        public Type DataType => typeof(string);

        public virtual Task ForEachBlock(BlockCallback<T> callback)
        {
            // read from the file
            if (_file != null) {
                long fileLength = RandomAccess.GetLength(_file), offset = 0;
                while (offset < fileLength) {
                    offset += GetBlockFromFile(_file, offset, callback);
                }
            }

            // then from in memory blocks
            if (_inMemoryBlocks is not null) {
                foreach (var block in _inMemoryBlocks)
                    callback(block.WrittenSpan);
            }

            // then from the current block
            if (_currBlock is not null)
                callback(_currBlock.WrittenSpan);
            return Task.CompletedTask;
        }

        public virtual Task<ReadOnlyMemory<T>> GetBlock(uint blockIndex)
        {
            uint currentIndex = 0;

            // read from the file
            if (_file != null) {
                if (blockIndex < _blocksInFile) {
                    long fileLength = RandomAccess.GetLength(_file), offset = 0;
                    while (offset < fileLength) {
                        if (currentIndex++ == blockIndex)
                            return Task.FromResult(GetBlockFromFile(_file, offset));
                        offset += SkipFileBlock(_file, offset);
                    }
                }
                else
                    currentIndex = _blocksInFile;
            }

            // then from in memory blocks
            if (_inMemoryBlocks is not null) {
                if (blockIndex < _blocksInFile + (uint)_inMemoryBlocks.Count) {
                    foreach (var block in _inMemoryBlocks) {
                        if (currentIndex++ == blockIndex) {
                            return Task.FromResult(block.WrittenMemory);
                        }
                    }
                }else
                    currentIndex = _blocksInFile + (uint)_inMemoryBlocks.Count;
            }

            // then from the current block
            if (_currBlock is not null && currentIndex == blockIndex)
                return Task.FromResult(_currBlock.WrittenMemory);
            throw new Exception("Unexpected - failed to find block");
        }

        public virtual void Add(in T item)
        {
            EnsureCurrentBlock().GetNext() = item;
            if (_distinct?.Add(item) == true && _distinct.Count >= _maxDistinctItems)
                _distinct = null;
            ++Size;
        }

        public virtual void Add(ReadOnlySpan<T> inputBlock)
        {
            foreach(var item in inputBlock)
                Add(item);
        }

        protected BT EnsureCurrentBlock()
        {
            if (_currBlock?.HasFreeCapacity != true) {
                if (_currBlock is not null) {
                    if (_maxInMemoryBlocks.HasValue && (_inMemoryBlocks?.Count ?? 0) >= _maxInMemoryBlocks.Value) {
                        _file ??= (_tempStreams ??= new TempFileProvider()).Get(Id);
                        _currBlock.WriteTo(_file);
                        ++_blocksInFile;
                    }else
                        (_inMemoryBlocks ??= new()).Add(_currBlock);
                }
                _currBlock = _blockFactory(new T[_blockSize]);
                ++BlockCount;
            }
            return _currBlock!;
        }

        protected abstract uint SkipFileBlock(SafeFileHandle file, long offset);
        protected abstract ReadOnlyMemory<T> GetBlockFromFile(SafeFileHandle file, long offset);
        protected abstract uint GetBlockFromFile(SafeFileHandle file, long offset, BlockCallback<T> callback);
        public override string ToString() => $"Composite buffer ({typeof(T).Name})|{MetaData.GetName(Id.ToString("n"))}|count={Size:N0}";
    }
}
