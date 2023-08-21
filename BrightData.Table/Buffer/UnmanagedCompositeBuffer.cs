using System.Runtime.CompilerServices;
using BrightData.Table.Helper;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using Microsoft.Win32.SafeHandles;

namespace BrightData.Table.Buffer
{
    /// <summary>
    /// Buffer that writes to disk after exhausting its in memory limit - not thread safe
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class UnmanagedCompositeBuffer<T> : CompositeBufferBase<T, UnmanagedCompositeBuffer<T>.Block> where T: unmanaged 
    {
        internal record Block(T[] Data) : ICompositeBufferBlock<T>
        {
            public uint Size { get; private set; }
            public ref T GetNext() => ref Data[Size++];
            public bool HasFreeCapacity => Size < Data.Length;
            public uint AvailableCapacity => (uint)Data.Length - Size;
            public ReadOnlySpan<T> WrittenSpan => new(Data, 0, (int)Size);
            public ReadOnlyMemory<T> WrittenMemory => new(Data, 0, (int)Size);

            public void WriteTo(SafeFileHandle file)
            {
                var bytes = WrittenSpan.Cast<T, byte>();
                RandomAccess.Write(file, bytes, RandomAccess.GetLength(file));
            }

            public void Write(ReadOnlySpan<T> data)
            {
                data.CopyTo(Data.AsSpan((int)Size, (int)AvailableCapacity));
                Size += (uint)data.Length;
            }
        }
        readonly int         _sizeOfT;

        public UnmanagedCompositeBuffer(
            IProvideTempStreams? tempStreams = null, 
            int blockSize = Consts.DefaultBlockSize, 
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        ) : base(x => new(x), tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems) 
        {
            _sizeOfT = Unsafe.SizeOf<T>();
        }

        public override async Task ForEachBlock(BlockCallback<T> callback)
        {
            // read from the file
            if (_file != null) {
                long fileLength = RandomAccess.GetLength(_file), offset = 0;
                using var buffer = MemoryOwner<byte>.Allocate(_blockSize * _sizeOfT);
                while (offset < fileLength) {
                    var readCount = 0;
                    do {
                        readCount += await RandomAccess.ReadAsync(_file, buffer.Memory[readCount..], offset + readCount);
                    }while(readCount < _blockSize * _sizeOfT);
                    callback(buffer.Span.Cast<byte, T>());
                    offset += _blockSize * _sizeOfT;
                }
            }

            // then from the in memory blocks
            if (_inMemoryBlocks is not null) {
                foreach (var block in _inMemoryBlocks)
                    callback(block.WrittenSpan);
            }

            // then from the current block
            if (_currBlock is not null)
                callback(_currBlock.WrittenSpan);
        }

        public override async Task<ReadOnlyMemory<T>> GetBlock(uint blockIndex)
        {
            if(blockIndex >= BlockCount)
                throw new ArgumentOutOfRangeException(nameof(blockIndex), $"Must be less than {BlockCount}");
            uint currentIndex = 0;

            // read from the file
            if (_file != null) {
                if (blockIndex < _blocksInFile) {
                    long fileLength = RandomAccess.GetLength(_file), offset = 0;
                    while (offset < fileLength) {
                        if (currentIndex++ == blockIndex) {
                            var ret = new Memory<T>(new T[_blockSize]);
                            var buffer = ret.Cast<T, byte>();
                            var readCount = 0;
                            do {
                                readCount += await RandomAccess.ReadAsync(_file, buffer[readCount..], offset + readCount);
                            } while (readCount < _blockSize * _sizeOfT);

                            return ret;
                        }

                        offset += _blockSize * _sizeOfT;
                    }
                }else
                    currentIndex = _blocksInFile;
            }

            // then from the in memory blocks
            if (_inMemoryBlocks is not null) {
                if (blockIndex < _blocksInFile + (uint)_inMemoryBlocks.Count) {
                    foreach (var block in _inMemoryBlocks) {
                        if (currentIndex++ == blockIndex)
                            return block.WrittenMemory;
                    }
                }else
                    currentIndex = _blocksInFile + (uint)_inMemoryBlocks.Count;
            }

            // then from the current block
            if (_currBlock is not null && currentIndex == blockIndex)
                return _currBlock.WrittenMemory;
            throw new Exception("Unexpected");
        }

        public override void Add(ReadOnlySpan<T> inputBlock)
        {
            while (inputBlock.Length > 0) {
                var block = EnsureCurrentBlock();
                var countToWrite = Math.Min(inputBlock.Length, (int)block.AvailableCapacity);
                var itemsToWrite = inputBlock[..countToWrite];
                block.Write(itemsToWrite);
                if (_distinct != null) {
                    foreach (var item in itemsToWrite) {
                        if (_distinct?.Add(item) == true && _distinct.Count >= _maxDistinctItems) {
                            _distinct = null;
                            break;
                        }
                    }
                }
                inputBlock = inputBlock[countToWrite..];
                Size += (uint)countToWrite;
            }
        }

        protected override uint SkipFileBlock(SafeFileHandle file, long offset)
        {
            throw new NotImplementedException();
        }

        protected override ReadOnlyMemory<T> GetBlockFromFile(SafeFileHandle file, long offset)
        {
            throw new NotImplementedException();
        }

        protected override uint GetBlockFromFile(SafeFileHandle file, long offset, BlockCallback<T> callback)
        {
            throw new NotImplementedException();
        }
    }
}
