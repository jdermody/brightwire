using BrightData.Buffer.Composite;
using System;
using System.Buffers.Binary;
using System.IO;
using BrightData.Buffer.MutableBlocks;

namespace BrightData.Buffer.ReadOnly
{
    /// <summary>
    /// Read only composite buffer for managed objects
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="createItem"></param>
    /// <param name="stream"></param>
    internal class ReadOnlyManagedCompositeBuffer<T>(CreateFromReadOnlyByteSpan<T> createItem, Stream stream) : ReadOnlyCompositeBufferBase<T>(stream)
        where T : IHaveDataAsReadOnlyByteSpan
    {
        protected override ReadOnlyMemory<T> Get(ReadOnlyMemory<byte> byteData)
        {
            var span = byteData.Span;
            var count = BinaryPrimitives.ReadUInt32LittleEndian(span);
            span = span[MutableManagedBufferBlock<T>.HeaderSize..];
            var ret = new T[count];
            var index = 0;

            for(var i = 0; i < count; i++) {
                var blockSize = BinaryPrimitives.ReadUInt32LittleEndian(span);
                span = span[4..];
                var item = createItem(span[..(int)blockSize]);
                span = span[(int)blockSize..];
                ret[index++] = item;
            }

            return ret;
        }
    }
}
