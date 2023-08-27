using System.Runtime.CompilerServices;
using BrightData.Table.Helper;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using Microsoft.Win32.SafeHandles;

namespace BrightData.Table.Buffer.Composite
{
    /// <summary>
    /// Buffer that writes to disk after exhausting its in memory limit - not thread safe
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class UnmanagedCompositeBuffer<T> : CompositeBufferBase<T, UnmanagedCompositeBuffer<T>.Block> where T : unmanaged
    {
        internal record Block(T[] Data) : ICompositeBufferBlock<T>
        {
            public uint Size { get; private set; }
            public ref T GetNext() => ref Data[Size++];
            public bool HasFreeCapacity => Size < Data.Length;
            public uint AvailableCapacity => (uint)Data.Length - Size;
            public ReadOnlySpan<T> WrittenSpan => new(Data, 0, (int)Size);
            public ReadOnlyMemory<T> WrittenMemory => new(Data, 0, (int)Size);

            public void WriteTo(ITempData file)
            {
                var bytes = WrittenSpan.Cast<T, byte>();
                file.Write(bytes, file.Size);
            }

            public void Write(ReadOnlySpan<T> data)
            {
                data.CopyTo(Data.AsSpan((int)Size, (int)AvailableCapacity));
                Size += (uint)data.Length;
            }
        }
        readonly int _sizeOfT;

        public UnmanagedCompositeBuffer(
            IProvideTempData? tempStreams = null,
            int blockSize = Consts.DefaultBlockSize,
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        ) : base(x => new(x), tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems)
        {
            _sizeOfT = Unsafe.SizeOf<T>();
        }

        //public override async Task ForEachBlock(BlockCallback<T> callback, INotifyUser? notify = null, string? message = null, CancellationToken ct = default)
        //{
        //    // read from the in memory blocks
        //    if (_inMemoryBlocks is not null)
        //    {
        //        foreach (var block in _inMemoryBlocks) {
        //            if (ct.IsCancellationRequested)
        //                break;
        //            callback(block.WrittenSpan);
        //        }
        //    }

        //    // read from the file
        //    if (_tempData != null && !ct.IsCancellationRequested)
        //    {
        //        uint fileLength = _tempData.Size, offset = 0;
        //        using var buffer = MemoryOwner<byte>.Allocate(_blockSize * _sizeOfT);
        //        while (offset < fileLength && !ct.IsCancellationRequested)
        //        {
        //            uint readCount = 0;
        //            do
        //            {
        //                readCount += await _tempData.ReadAsync(buffer.Memory[(int)readCount..], offset + readCount);
        //            } while (readCount < _blockSize * _sizeOfT);
        //            callback(buffer.Span.Cast<byte, T>());
        //            offset += (uint)(_blockSize * _sizeOfT);
        //        }
        //    }

        //    // then from the current block
        //    if (_currBlock is not null && !ct.IsCancellationRequested)
        //        callback(_currBlock.WrittenSpan);
        //}

        public override async Task<ReadOnlyMemory<T>> GetBlock(uint blockIndex)
        {
            if (blockIndex >= BlockCount)
                throw new ArgumentOutOfRangeException(nameof(blockIndex), $"Must be less than {BlockCount}");
            uint currentIndex = 0;

            // read from the in memory blocks
            if (_inMemoryBlocks is not null)
            {
                if (blockIndex < _blocksInFile + (uint)_inMemoryBlocks.Count)
                {
                    foreach (var block in _inMemoryBlocks)
                    {
                        if (currentIndex++ == blockIndex)
                            return block.WrittenMemory;
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
                            return (await GetBlockFromFile(_tempData, offset)).Item2;
                        offset += (uint)(_blockSize * _sizeOfT);
                    }
                }
                else
                    currentIndex = _blocksInFile;
            }

            // then from the current block
            if (_currBlock is not null && currentIndex == blockIndex)
                return _currBlock.WrittenMemory;
            throw new Exception("Unexpected");
        }

        public override void Add(ReadOnlySpan<T> inputBlock)
        {
            while (inputBlock.Length > 0)
            {
                var block = EnsureCurrentBlock();
                var countToWrite = Math.Min(inputBlock.Length, (int)block.AvailableCapacity);
                var itemsToWrite = inputBlock[..countToWrite];
                block.Write(itemsToWrite);
                if (_distinct != null)
                {
                    foreach (var item in itemsToWrite)
                    {
                        if (_distinct?.Add(item) == true && _distinct.Count >= _maxDistinctItems)
                        {
                            _distinct = null;
                            break;
                        }
                    }
                }
                inputBlock = inputBlock[countToWrite..];
                Size += (uint)countToWrite;
            }
        }

        protected override Task<uint> SkipFileBlock(ITempData file, uint offset)
        {
            return Task.FromResult((uint)_blockSize * (uint)_sizeOfT);
        }

        protected override async Task<(uint, ReadOnlyMemory<T>)> GetBlockFromFile(ITempData file, uint offset)
        {
            var ret = new Memory<T>(new T[_blockSize]);
            var buffer = ret.Cast<T, byte>();
            uint readCount = 0;
            do
            {
                readCount += await file.ReadAsync(buffer[(int)readCount..], offset + readCount);
            } while (readCount < _blockSize * _sizeOfT);
            return ((uint)_blockSize * (uint)_sizeOfT, ret);
        }

        protected override async Task<uint> GetBlockFromFile(ITempData file, uint offset, BlockCallback<T> callback)
        {
            using var buffer = MemoryOwner<byte>.Allocate(_blockSize * _sizeOfT);
            uint readCount = 0;
            do
            {
                readCount += await file.ReadAsync(buffer.Memory[(int)readCount..], offset + readCount);
            } while (readCount < _blockSize * _sizeOfT);
            callback(buffer.Span.Cast<byte, T>());
            return (uint)_blockSize * (uint)_sizeOfT;
        }
    }
}
