using BrightData.Buffer.Composite;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.Buffer.ReadOnly
{
    internal class ReadOnlyManagedCompositeBuffer<T> : ReadOnlyCompositeBufferBase<T> where T : IHaveDataAsReadOnlyByteSpan
    {
        readonly CreateFromReadOnlyByteSpan<T> _createItem;

        public ReadOnlyManagedCompositeBuffer(CreateFromReadOnlyByteSpan<T> createItem, Stream stream) : base(stream)
        {
            _createItem = createItem;
        }

        protected override ReadOnlyMemory<T> Get(ReadOnlyMemory<byte> byteData)
        {
            var span = byteData.Span;
            var count = BinaryPrimitives.ReadUInt32LittleEndian(span);
            span = span[ManagedCompositeBuffer<T>.HeaderSize..];
            var ret = new T[count];
            var index = 0;

            for(var i = 0; i < count; i++) {
                var blockSize = BinaryPrimitives.ReadUInt32LittleEndian(span);
                span = span[4..];
                var item = _createItem(span[..(int)blockSize]);
                span = span[(int)blockSize..];
                ret[index++] = item;
            }

            return ret;
        }
    }
}
