using BrightData.Buffer.MutableBlocks;
using BrightData.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BrightData.Buffer
{
    internal class InMemoryBuffer<T>(uint blockSize = Consts.DefaultBlockSize)
        : TypedBufferBase<T>, IAppendableBuffer<T>
        where T : notnull
    {
        protected List<MutableInMemoryBufferBlock<T>>? _inMemoryBlocks;
        protected MutableInMemoryBufferBlock<T>? _currBlock;

        public Task ForEachBlock(BlockCallback<T> callback, INotifyOperationProgress? notify = null, string? message = null, CancellationToken ct = default)
        {
            var guid = Guid.NewGuid();
            notify?.OnStartOperation(guid, message);
            var count = 0;

            // read from in memory blocks
            if (_inMemoryBlocks is not null)
            {
                foreach (var block in _inMemoryBlocks)
                {
                    if (ct.IsCancellationRequested)
                        break;
                    callback(block.WrittenSpan);
                    notify?.OnOperationProgress(guid, (float)++count / BlockCount);
                }
            }

            // then from the current block
            if (_currBlock is not null && !ct.IsCancellationRequested)
            {
                callback(_currBlock.WrittenSpan);
                notify?.OnOperationProgress(guid, (float)++count / BlockCount);
            }
            notify?.OnCompleteOperation(guid, ct.IsCancellationRequested);
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

        public override async IAsyncEnumerable<T> EnumerateAllTyped()
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
                    {
                        ret[index++] = blockSize;
                    }
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

        protected MutableInMemoryBufferBlock<T> EnsureCurrentBlock()
        {
            if (_currBlock?.HasFreeCapacity != true)
            {
                if (_currBlock is not null)
                {
                    (_inMemoryBlocks ??= []).Add(_currBlock);
                }
                _currBlock = new(new T[blockSize], false);
                ++BlockCount;
            }
            return _currBlock;
        }
    }
}
