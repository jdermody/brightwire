using System;
using System.Buffers.Binary;
using System.IO;
using BrightData.Buffer.Composite;
using BrightData.Buffer.MutableBlocks;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.Buffer.ReadOnly
{
    /// <summary>
    /// Read only composite buffer for strings
    /// </summary>
    /// <param name="stream"></param>
    internal class ReadOnlyStringCompositeBuffer(Stream stream) : ReadOnlyCompositeBufferBase<string>(stream)
    {
        readonly StringPool _stringPool = new();

        protected override ReadOnlyMemory<string> Get(ReadOnlyMemory<byte> byteData)
        {
            var span = byteData.Span;
            var numStrings = BinaryPrimitives.ReadUInt32LittleEndian(span[4..]);
            var ret = new string[numStrings];
            var index = 0;
            MutableStringBufferBlock.Decode(span[MutableStringBufferBlock.HeaderSize..], charSpan => ret[index++] = _stringPool.GetOrAdd(charSpan));
            return ret;
        }
    }
}
