using System;
using System.Buffers;
using System.Threading.Tasks;

namespace BrightData.Buffer.ByteBlockReaders
{
    /// <summary>
    /// Reads from memory
    /// </summary>
    /// <param name="memory"></param>
    /// <param name="disposable"></param>
    internal class MemoryByteBlockReader(ReadOnlyMemory<byte> memory, IDisposable? disposable = null)
        : IByteBlockReader
    {
        readonly IMemoryOwner<byte>? _owner = null;

        public MemoryByteBlockReader(IMemoryOwner<byte> owner) : this(owner.Memory)
        {
            _owner = owner;
        }

        public void Dispose()
        {
            _owner?.Dispose();
            disposable?.Dispose();
        }

        public Task<ReadOnlyMemory<byte>> GetBlock(uint offset, uint numBytes)
        {
            if (numBytes == 0 || memory.IsEmpty)
                return Task.FromResult(ReadOnlyMemory<byte>.Empty);
            return Task.FromResult(memory.Slice((int)offset, (int)numBytes));
        }

        public Task Update(uint byteOffset, ReadOnlyMemory<byte> data)
        {
            // nop
            return Task.CompletedTask;
        }

        public uint Size => (uint)memory.Length;
    }
}
