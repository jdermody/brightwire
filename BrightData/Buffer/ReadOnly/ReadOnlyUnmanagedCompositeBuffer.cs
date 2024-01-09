using System;
using System.IO;
using CommunityToolkit.HighPerformance;

namespace BrightData.Buffer.ReadOnly
{
    /// <summary>
    /// Read only composite buffer for unmanaged objects
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="stream"></param>
    internal class ReadOnlyUnmanagedCompositeBuffer<T>(Stream stream) : ReadOnlyCompositeBufferBase<T>(stream)
        where T : unmanaged
    {
        protected override ReadOnlyMemory<T> Get(ReadOnlyMemory<byte> byteData)
        {
            return byteData.Cast<byte, T>();
        }
    }
}
