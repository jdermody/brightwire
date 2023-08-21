using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.Table.ByteReaders
{
    internal class MemoryByteReader : IByteReader
    {
        readonly MemoryOwner<byte>? _owner = null;
        readonly ReadOnlyMemory<byte> _memory;

        public MemoryByteReader(MemoryOwner<byte> owner)
        {
            _owner = owner;
            _memory = owner.Memory;
        }

        public MemoryByteReader(ReadOnlyMemory<byte> memory)
        {
            _memory = memory;
        }

        public void Dispose()
        {
            _owner?.Dispose();
        }

        public Task<ReadOnlyMemory<byte>> GetBlock(uint offset, uint numBytes)
        {
            return Task.FromResult(_memory.Slice((int)offset, (int)numBytes));
        }
    }
}
