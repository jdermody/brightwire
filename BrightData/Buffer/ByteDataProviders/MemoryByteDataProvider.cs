using System;

namespace BrightData.Buffer.ByteDataProviders;

/// <summary>
/// Byte data provider based on read only memory
/// </summary>
/// <param name="data"></param>
internal class MemoryByteDataProvider(ReadOnlyMemory<byte> data) : IByteDataProvider
{
    readonly ReadOnlyMemory<byte> _data = data;

    public void Dispose()
    {
        // nop
    }

    public ByteDataBlock GetDataSpan(uint offset, uint size)
    {
        return new(_data.Span.Slice((int)offset, (int)size), null);
    }
}
