using System;
using System.Buffers;
using System.Threading.Tasks;

namespace BrightData.Buffer.ByteBlockReaders
{
    internal class MemoryByteBlockReader : IByteBlockReader
    {
        readonly IMemoryOwner<byte>? _owner = null;
        readonly ReadOnlyMemory<byte> _memory;
        readonly IDisposable? _disposable;

        public MemoryByteBlockReader(IMemoryOwner<byte> owner)
        {
            _owner = owner;
            _memory = owner.Memory;
        }

        public MemoryByteBlockReader(ReadOnlyMemory<byte> memory, IDisposable? disposable = null)
        {
            _memory = memory;
            _disposable = disposable;
        }

        public void Dispose()
        {
            _owner?.Dispose();
            _disposable?.Dispose();
        }

        public Task<ReadOnlyMemory<byte>> GetBlock(uint offset, uint numBytes)
        {
            if (numBytes == 0 || _memory.IsEmpty)
                return Task.FromResult(ReadOnlyMemory<byte>.Empty);
            return Task.FromResult(_memory.Slice((int)offset, (int)numBytes));
        }

        public Task Update(uint byteOffset, ReadOnlyMemory<byte> data)
        {
            // nop
            return Task.CompletedTask;
        }

        public uint Size => (uint)_memory.Length;
    }
}
