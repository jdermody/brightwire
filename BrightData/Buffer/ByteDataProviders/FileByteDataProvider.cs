using DotNext.IO.MemoryMappedFiles;
using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.Buffer.ByteDataProviders;

/// <summary>
/// Byte data provider based on memory mapped file
/// </summary>
internal class FileByteDataProvider : IByteDataProvider
{
    readonly MemoryMappedFile _file;

    public FileByteDataProvider(string filePath)
    {
        _file = MemoryMappedFile.CreateFromFile(filePath);
    }

    public ByteDataBlock GetDataSpan(uint offset, uint size)
    {
        var accessor = _file.CreateDirectAccessor(offset, size, MemoryMappedFileAccess.Read);
        return new(accessor.Bytes, accessor);
    }

    public void Dispose()
    {
        _file.Dispose();
    }
}
