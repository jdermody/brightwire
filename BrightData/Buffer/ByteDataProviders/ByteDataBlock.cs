using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.Buffer.ByteDataProviders;

/// <summary>
/// A block of byte data
/// </summary>
public ref struct ByteDataBlock(ReadOnlySpan<byte> data, IDisposable? disposable) : IHaveByteData
{
    readonly ReadOnlySpan<byte> _data = data;

    /// <inheritdoc/>
    public ReadOnlySpan<byte> ByteData => _data;

    /// <inheritdoc/>
    public void Dispose()
    {
        disposable?.Dispose();
    }
}
