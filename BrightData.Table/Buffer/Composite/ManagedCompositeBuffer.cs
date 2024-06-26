﻿using BrightData.Table.Helper;
using CommunityToolkit.HighPerformance.Buffers;
using Microsoft.Win32.SafeHandles;
using System.Buffers;
using System.Buffers.Binary;

namespace BrightData.Table.Buffer.Composite
{
    internal class ManagedCompositeBuffer<T> : CompositeBufferBase<T, ManagedCompositeBuffer<T>.Block> where T : IHaveDataAsReadOnlyByteSpan
    {
        const int HeaderSize = 4;
        internal record Block(T[] Data) : ICompositeBufferBlock<T>
        {
            public uint Size { get; private set; }
            public ref T GetNext() => ref Data[Size++];
            public bool HasFreeCapacity => Size < Data.Length;
            public ReadOnlySpan<T> WrittenSpan => new(Data, 0, (int)Size);
            public ReadOnlyMemory<T> WrittenMemory => new(Data, 0, (int)Size);

            public void WriteTo(ITempData file)
            {
                var offset = file.Size;
                using var writer = new ArrayPoolBufferWriter<byte>();
                var memoryOwner = (IMemoryOwner<byte>)writer;
                writer.Advance(HeaderSize);
                var size = 0;
                for (uint i = 0; i < Size; i++)
                {
                    var itemData = Data[i].DataAsBytes;
                    var span = writer.GetSpan(itemData.Length + 4);
                    BinaryPrimitives.WriteUInt32LittleEndian(span, (uint)itemData.Length);
                    itemData.CopyTo(span[4..]);
                    writer.Advance(itemData.Length + 4);
                    size += itemData.Length + 4;
                }
                BinaryPrimitives.WriteUInt32LittleEndian(memoryOwner.Memory.Span, (uint)size);
                file.Write(writer.WrittenSpan, offset);
            }
        }

        readonly CreateFromReadOnlyByteSpan<T> _createItem;

        public ManagedCompositeBuffer(
            CreateFromReadOnlyByteSpan<T> createItem,
            IProvideTempData? tempStreams = null,
            int blockSize = Consts.DefaultBlockSize,
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        ) : base(x => new(x), tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems)
        {
            _createItem = createItem;
        }

        protected override async Task<uint> SkipFileBlock(ITempData file, uint offset)
        {
            var lengthBytes = new byte[HeaderSize];
            await file.ReadAsync(lengthBytes, offset);
            var blockSize = BinaryPrimitives.ReadUInt32LittleEndian(lengthBytes);
            return blockSize + HeaderSize;
        }

        protected override async Task<(uint, ReadOnlyMemory<T>)> GetBlockFromFile(ITempData file, uint offset)
        {
            var (blockSize, buffer) = await ReadBuffer(file, offset);
            try
            {
                var buffer2 = new Memory<T>(new T[_blockSize]);
                Copy(buffer.Span, buffer2.Span);
                return (blockSize + HeaderSize, buffer2);
            }
            finally
            {
                buffer.Dispose();
            }
        }

        protected override async Task<uint> GetBlockFromFile(ITempData file, uint offset, BlockCallback<T> callback)
        {
            var (blockSize, buffer) = await ReadBuffer(file, offset);
            try
            {
                using var buffer2 = MemoryOwner<T>.Allocate(_blockSize);
                Copy(buffer.Span, buffer2.Span);
                callback(buffer2.Span);
                return blockSize + HeaderSize;
            }
            finally
            {
                buffer.Dispose();
            }
        }

        void Copy(ReadOnlySpan<byte> inputSpan, Span<T> outputSpan)
        {
            for (var i = 0; i < _blockSize; i++)
            {
                var itemSize = BinaryPrimitives.ReadUInt32LittleEndian(inputSpan);
                var itemData = inputSpan[HeaderSize..(int)(itemSize + 4)];
                outputSpan[i] = _createItem(itemData);
                inputSpan = inputSpan[(int)(itemSize + HeaderSize)..];
            }
        }

        static async Task<(uint BlockSize, MemoryOwner<byte> Buffer)> ReadBuffer(ITempData file, uint offset)
        {
            var lengthBytes = new byte[HeaderSize];
            await file.ReadAsync(lengthBytes, offset);
            var blockSize = BinaryPrimitives.ReadUInt32LittleEndian(lengthBytes);
            offset += HeaderSize;
            var buffer = MemoryOwner<byte>.Allocate((int)blockSize);
            uint readCount = 0;
            do
            {
                readCount += await file.ReadAsync(buffer.Memory[(int)readCount..], offset + readCount);
            } while (readCount < blockSize);

            return (blockSize, buffer);
        }
    }
}
