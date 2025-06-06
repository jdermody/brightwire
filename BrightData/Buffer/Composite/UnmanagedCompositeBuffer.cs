﻿using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance;

namespace BrightData.Buffer.Composite
{
    /// <summary>
    /// A composite buffer for unmanaged types
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class UnmanagedCompositeBuffer<T>(
        IProvideByteBlocks? tempStreams = null,
        int blockSize = Consts.DefaultInitialBlockSize,
        int maxBlockSize = Consts.DefaultMaxBlockSize,
        uint? maxInMemoryBlocks = null,
        uint? maxDistinctItems = null)
        : CompositeBufferBase<T, UnmanagedCompositeBuffer<T>.Block>(x => new(x), x => new(x), tempStreams, blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems)
        where T : unmanaged
    {
        public class Block(Memory<T> Data) : IMutableBufferBlock<T>
        {
            public Block(ReadOnlyMemory<T> data) : this(Unsafe.As<ReadOnlyMemory<T>, Memory<T>>(ref data))
            {
                Size = (uint)data.Length;
            }

            public uint Size { get; private set; }
            public ref T GetNext() => ref Data.Span[(int)Size++];
            public bool HasFreeCapacity => Size < Data.Length;
            public uint AvailableCapacity => (uint)Data.Length - Size;
            public ReadOnlySpan<T> WrittenSpan => Data.Span[..(int)Size];
            public ReadOnlyMemory<T> WrittenMemory => Data[..(int)Size];

            public async Task<uint> WriteTo(IByteBlockSource file)
            {
                var bytes = WrittenMemory.Cast<T, byte>();
                await file.WriteAsync(bytes, file.Size);
                return (uint)bytes.Length;
            }

            public void Write(ReadOnlySpan<T> data)
            {
                data.CopyTo(Data.Span.Slice((int)Size, (int)AvailableCapacity));
                Size += (uint)data.Length;
            }
        }

        readonly int _sizeOfT = Unsafe.SizeOf<T>();

        public override async Task<ReadOnlyMemory<T>> GetTypedBlock(uint blockIndex)
        {
            if (blockIndex >= BlockCount)
                throw new ArgumentOutOfRangeException(nameof(blockIndex), $"Must be less than {BlockCount}");
            uint currentIndex = 0;
            var blocksInFile = (uint)(_fileBlockSizes?.Count ?? 0);

            // read from the in memory blocks
            if (_inMemoryBlocks is not null)
            {
                if (blockIndex < blocksInFile + (uint)_inMemoryBlocks.Count)
                {
                    foreach (var block in _inMemoryBlocks)
                    {
                        if (currentIndex++ == blockIndex)
                            return block.WrittenMemory;
                    }
                }
                else
                    currentIndex = blocksInFile + (uint)_inMemoryBlocks.Count;
            }

            // read from the file
            if (_currentDataBlock != null && _fileBlockSizes != null) {
                var fileBlockIndex = 0;
                if (blockIndex < blocksInFile)
                {
                    uint fileLength = _currentDataBlock.Size, offset = 0;
                    while (offset < fileLength) {
                        var blockSize = _fileBlockSizes[fileBlockIndex++];
                        if (currentIndex++ == blockIndex)
                            return (await GetBlockFromFile(_currentDataBlock, offset, blockSize)).Item2;
                        offset += (uint)(blockSize * _sizeOfT);
                    }
                }
                else
                    currentIndex = blocksInFile;
            }

            // then from the current block
            if (_currBlock is not null && currentIndex == blockIndex)
                return _currBlock.WrittenMemory;
            throw new InvalidOperationException("Unexpected");
        }

        public override void Append(ReadOnlySpan<T> inputBlock)
        {
            while (inputBlock.Length > 0)
            {
                var block = EnsureCurrentBlock().GetAwaiter().GetResult();
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

        protected override Task<uint> SkipFileBlock(IByteBlockSource file, uint offset, uint numItemsInBlock)
        {
            return Task.FromResult(numItemsInBlock * (uint)_sizeOfT);
        }

        protected override async Task<(uint, ReadOnlyMemory<T>)> GetBlockFromFile(IByteBlockSource file, uint offset, uint numItemsInBlock)
        {
            var ret = new Memory<T>(new T[numItemsInBlock]);
            var buffer = ret.Cast<T, byte>();
            uint readCount = 0;
            do
            {
                readCount += await file.ReadAsync(buffer[(int)readCount..], offset + readCount);
            } while (readCount < numItemsInBlock * _sizeOfT);
            return (numItemsInBlock * (uint)_sizeOfT, ret);
        }
    }
}
