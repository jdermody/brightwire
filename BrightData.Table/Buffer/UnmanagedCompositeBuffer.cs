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
    internal class UnmanagedCompositeBuffer<T> : ICompositeBuffer<T> where T: unmanaged 
    {
        protected record Block(T[] Data)
        {
            public uint Size { get; private set; }
            public ref T GetNext() => ref Data[Size++];
            public bool HasFreeCapacity => Size < Data.Length;
            public uint AvailableCapacity => (uint)Data.Length - Size;
            public ReadOnlySpan<T> WrittenSpan => new(Data, 0, (int)Size);

            public void Write(ReadOnlySpan<T> data)
            {
                data.CopyTo(Data.AsSpan((int)Size, (int)AvailableCapacity));
                Size += (uint)data.Length;
            }
        }

        readonly int         _blockSize, _sizeOfT;
        readonly uint?       _maxInMemoryBlocks, _maxDistinctItems;
        IProvideTempStreams? _tempStreams;
        SafeFileHandle?      _file;
        List<Block>?         _inMemoryBlocks;
        Block?               _currBlock;
        HashSet<T>?          _distinct;

        public UnmanagedCompositeBuffer(
            IProvideTempStreams? tempStreams = null, 
            int blockSize = Consts.DefaultBlockSize, 
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        ){
            _tempStreams = tempStreams;
            _blockSize = blockSize;
            _maxInMemoryBlocks = maxInMemoryBlocks;
            _sizeOfT = Unsafe.SizeOf<T>();
            if ((_maxDistinctItems = maxDistinctItems) > 0) {
                _distinct = new HashSet<T>((int)maxDistinctItems!.Value / 32);
            }
        }

        public Guid Id { get; } = Guid.NewGuid();
        public string? Name { get; set; }
        public uint Size { get; private set; }
        public uint? DistinctItems => (uint?)_distinct?.Count;

        public async Task ForEachBlock(BlockCallback<T> callback)
        {
            // read from the file
            if (_file != null) {
                long fileLength = RandomAccess.GetLength(_file), offset = 0;
                using var buffer = MemoryOwner<byte>.Allocate(_blockSize * _sizeOfT);
                while (offset < fileLength) {
                    var readSize = await RandomAccess.ReadAsync(_file, buffer.Memory, offset);
                    callback(buffer.Span[..readSize].Cast<byte, T>());
                    offset += readSize;
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

        public void Add(ReadOnlySpan<T> inputBlock)
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

        public void Add(in T item)
        {
            EnsureCurrentBlock().GetNext() = item;
            if (_distinct?.Add(item) == true && _distinct.Count >= _maxDistinctItems)
                _distinct = null;
            ++Size;
        }

        Block EnsureCurrentBlock()
        {
            if (_currBlock?.HasFreeCapacity != true) {
                if (_currBlock is not null) {
                    if (_maxInMemoryBlocks.HasValue && (_inMemoryBlocks?.Count ?? 0) >= _maxInMemoryBlocks.Value) {
                        _file ??= (_tempStreams ??= new TempFileProvider()).Get(Id);
                        var bytes = _currBlock.WrittenSpan.Cast<T, byte>();
                        RandomAccess.Write(_file, bytes, RandomAccess.GetLength(_file));
                    }else
                        (_inMemoryBlocks ??= new()).Add(_currBlock);
                }
                _currBlock = new(new T[_blockSize]);
            }
            return _currBlock!;
        }

        public override string ToString() => $"Composite buffer ({typeof(T).Name})|{Name ?? Id.ToString("n")}|size:{Size:N0}";
    }
}
