using BrightData.Table.Helper;
using Microsoft.Win32.SafeHandles;
using System.Buffers.Binary;
using System.Text;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.Table.Buffer
{
    public class StringCompositeBuffer : ICompositeBuffer<string>
    {
        record Block(string[] Data)
        {
            public uint Size { get; private set; }
            public ref string GetNext() => ref Data[Size++];
            public bool HasFreeCapacity => Size < Data.Length;
            public ReadOnlySpan<string> WrittenSpan => new(Data, 0, (int)Size);

            public void WriteTo(SafeFileHandle file)
            {
                var offset = RandomAccess.GetLength(file);
                var startOffset = offset += 8;
                for (uint i = 0; i < Size; i++) {
                    Encode(Data[i], bytes => {
                        RandomAccess.Write(file, bytes, offset);
                        offset += bytes.Length;
                    });
                }
                var blockSize = (uint)(offset - startOffset);
                Span<byte> lengthBytes = stackalloc byte[8];
                BinaryPrimitives.WriteUInt32LittleEndian(lengthBytes, blockSize);
                BinaryPrimitives.WriteUInt32LittleEndian(lengthBytes[4..], Size);
                RandomAccess.Write(file, lengthBytes, startOffset-8);
            }
        }

        readonly int         _blockSize;
        readonly uint?       _maxInMemoryBlocks, _maxDistinctItems;
        IProvideTempStreams? _tempStreams;
        SafeFileHandle?      _file;
        List<Block>?         _inMemoryBlocks;
        Block?               _currBlock;
        HashSet<string>?     _distinct;

        public StringCompositeBuffer(
            IProvideTempStreams? tempStreams = null,
            int blockSize = Consts.DefaultBlockSize,
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        )
        {
            _tempStreams          = tempStreams;
            _blockSize            = blockSize;
            _maxInMemoryBlocks    = maxInMemoryBlocks;
            if ((_maxDistinctItems = maxDistinctItems) > 0) {
                _distinct = new HashSet<string>((int)maxDistinctItems!.Value / 32);
            }
        }

        public Guid Id { get; } = Guid.NewGuid();
        public string? Name { get; set; }
        public uint Size { get; private set; }
        public uint? DistinctItems => (uint?)_distinct?.Count;

        public Task ForEachBlock(BlockCallback<string> callback)
        {
            // read from the file
            if (_file != null) {
                Span<byte> lengthBytes = stackalloc byte[8];
                long fileLength = RandomAccess.GetLength(_file), offset = 0;
                while (offset < fileLength) {
                    RandomAccess.Read(_file, lengthBytes, offset);
                    offset += 8;
                    var blockSize = BinaryPrimitives.ReadUInt32LittleEndian(lengthBytes[..4]);
                    var numStrings = BinaryPrimitives.ReadUInt32LittleEndian(lengthBytes[4..]);
                    using var block = SpanOwner<byte>.Allocate((int)blockSize);
                    var readCount = 0;
                    do {
                        readCount += RandomAccess.Read(_file, block.Span[readCount..], offset + readCount);
                    }while(readCount < blockSize);

                    var index = 0;
                    using var buffer = MemoryOwner<string>.Allocate((int)numStrings);
                    Decode(block.Span, chars => {
                        // ReSharper disable once AccessToDisposedClosure
                        buffer.Span[index++] = new string(chars);
                    });
                    callback(buffer.Span);
                    offset += block.Length;
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

        public void Add(in string item)
        {
            if(item.Length > ushort.MaxValue/3)
                throw new ArgumentOutOfRangeException(nameof(item), $"Length cannot exceed {ushort.MaxValue/3}");

            if (_currBlock?.HasFreeCapacity != true) {
                if (_currBlock is not null) {
                    if (_maxInMemoryBlocks.HasValue && (_inMemoryBlocks?.Count ?? 0) >= _maxInMemoryBlocks.Value) {
                        _file ??= (_tempStreams ??= new TempFileProvider()).Get(Id);
                        _currBlock.WriteTo(_file);
                    }else
                        (_inMemoryBlocks ??= new()).Add(_currBlock);
                }
                _currBlock = new(new string[_blockSize]);
            }
            _currBlock.GetNext() = item;
            if (_distinct?.Add(item) == true && _distinct.Count >= _maxDistinctItems)
                _distinct = null;
            ++Size;
        }

        public void Add(ReadOnlySpan<string> inputBlock)
        {
            foreach(var item in inputBlock)
                Add(item);
        }

        public static void Encode(string str, BlockCallback<byte> callback)
        {
            if (str.Length <= 124 / 3) {
                Span<byte> buffer = stackalloc byte[128];
                var actualByteCount = Encoding.UTF8.GetBytes(str, buffer[4..]);
                BinaryPrimitives.WriteUInt16LittleEndian(buffer, (ushort)str.Length);
                BinaryPrimitives.WriteUInt16LittleEndian(buffer[2..], (ushort)actualByteCount);
                callback(buffer[..(actualByteCount + 4)]);
            }
            else {
                using var buffer = SpanOwner<byte>.Allocate(str.Length * 3 + 2);
                var actualByteCount = Encoding.UTF8.GetBytes(str, buffer.Span[4..]);
                BinaryPrimitives.WriteUInt16LittleEndian(buffer.Span, (ushort)str.Length);
                BinaryPrimitives.WriteUInt16LittleEndian(buffer.Span[2..], (ushort)actualByteCount);
                callback(buffer.Span[..(actualByteCount + 4)]);
            }
        }

        public static void Decode(ReadOnlySpan<byte> data, BlockCallback<char> callback)
        {
            Span<char> localBuffer = stackalloc char[128];
            do {
                var charSize = BinaryPrimitives.ReadUInt16LittleEndian(data);
                var byteSize = BinaryPrimitives.ReadUInt16LittleEndian(data[2..]);
                data = data[4..];
                if (charSize <= 128) {
                    Encoding.UTF8.GetChars(data[..byteSize], localBuffer[..charSize]);
                    callback(localBuffer[..charSize]);
                }
                else {
                    using var buffer = SpanOwner<char>.Allocate(charSize);
                    Encoding.UTF8.GetChars(data[..byteSize], buffer.Span);
                    callback(buffer.Span);
                }
                data = data[byteSize..];
            } while (data.Length > 0);
        }

        public override string ToString() => $"String composite buffer|{Name ?? Id.ToString("n")}|count={Size:N0}";
    }
}
