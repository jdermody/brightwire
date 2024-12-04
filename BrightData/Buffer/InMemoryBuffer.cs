using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace BrightData.Buffer
{
    internal class InMemoryBuffer<T>
        : TypedBufferBase<T>, IAppendableBuffer<T>
        where T : notnull
    {
        protected class Block(Memory<T> Data) : IMutableBufferBlock<T>
        {
            public Block(ReadOnlyMemory<T> data) : this(Unsafe.As<ReadOnlyMemory<T>, Memory<T>>(ref data))
            {
                Size = (uint)data.Length;
            }

            public uint Size { get; private set; }
            public Task<uint> WriteTo(IByteBlockSource file)
            {
                throw new NotImplementedException();
            }

            public bool HasFreeCapacity => Size < Data.Length;
            public ReadOnlySpan<T> WrittenSpan => Data.Span[..(int)Size];
            public ReadOnlyMemory<T> WrittenMemory => Data[..(int)Size];
            public ref T GetNext() => ref Data.Span[(int)Size++];
        }
        uint _blockSize;
        readonly uint _maxBlockSize;
        protected List<Block>? _inMemoryBlocks;
        protected Block? _currBlock;

        public InMemoryBuffer(uint blockSize = Consts.DefaultInitialBlockSize, uint maxBlockSize = Consts.DefaultMaxBlockSize)
        {
            if (maxBlockSize < blockSize)
                throw new ArgumentException($"Expected max block size to be greater or equal to block size: block-size:{blockSize}, max-block-size:{maxBlockSize}");

            _blockSize = blockSize;
            _maxBlockSize = maxBlockSize;
        }

        public Task ForEachBlock(BlockCallback<T> callback, CancellationToken ct = default)
        {
            // read from in memory blocks
            if (_inMemoryBlocks is not null)
            {
                foreach (var block in _inMemoryBlocks)
                {
                    if (ct.IsCancellationRequested)
                        break;
                    callback(block.WrittenSpan);
                }
            }

            // then from the current block
            if (_currBlock is not null && !ct.IsCancellationRequested)
                callback(_currBlock.WrittenSpan);
            return Task.CompletedTask;
        }

        public override Task<ReadOnlyMemory<T>> GetTypedBlock(uint blockIndex)
        {
            uint currentIndex = 0;

            // read from in memory blocks
            if (_inMemoryBlocks is not null)
            {
                if (blockIndex < (uint)_inMemoryBlocks.Count)
                {
                    foreach (var block in _inMemoryBlocks)
                    {
                        if (currentIndex++ == blockIndex)
                        {
                            return Task.FromResult(block.WrittenMemory);
                        }
                    }
                }
                else
                    currentIndex = (uint)_inMemoryBlocks.Count;
            }

            // then from the current block
            if (_currBlock is not null && currentIndex == blockIndex)
                return Task.FromResult(_currBlock.WrittenMemory);
            throw new Exception("Unexpected - failed to find block");
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async IAsyncEnumerable<T> EnumerateAllTyped()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            // read from in memory blocks
            if (_inMemoryBlocks is not null)
            {
                foreach (var block in _inMemoryBlocks)
                {
                    var data = block.WrittenMemory;
                    for (var i = 0; i < data.Length; i++)
                    {
                        yield return data.Span[i];
                    }
                }
            }

            // then from current block
            if (_currBlock is not null)
            {
                for (var i = 0; i < _currBlock.WrittenMemory.Length; i++)
                {
                    yield return _currBlock.WrittenMemory.Span[i];
                }
            }
        }

        public uint Size { get; protected set; } = 0;
        public uint BlockCount { get; private set; }

        public uint[] BlockSizes
        {
            get
            {
                var ret = new uint[BlockCount];
                var index = 0;
                if (_inMemoryBlocks is not null)
                {
                    foreach (var block in _inMemoryBlocks)
                        ret[index++] = block.Size;
                }
                if (_currBlock is not null)
                    ret[index] = _currBlock.Size;
                return ret;
            }
        }

        public Type DataType => typeof(T);
        public void Append(ReadOnlySpan<T> block)
        {
            foreach (var item in block)
                Append(item);
        }

        public void Append(in T item)
        {
            var block = EnsureCurrentBlock();
            block.GetNext() = item;
            ++Size;
        }

        public void AppendObject(object obj)
        {
            Append((T)obj);
        }

        protected Block EnsureCurrentBlock()
        {
            if (_currBlock?.HasFreeCapacity != true)
            {
                if (_currBlock is not null)
                {
                    (_inMemoryBlocks ??= []).Add(_currBlock);
                }
                _currBlock = new(new T[_blockSize]);
                ++BlockCount;

                // increase the size of the next block
                var nextBlockSize = _blockSize * 2;
                if (nextBlockSize > _maxBlockSize)
                    nextBlockSize = _maxBlockSize;
                _blockSize = nextBlockSize;
            }
            return _currBlock;
        }
    }
}
