using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BrightData.Buffer.Composite;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.Buffer.ReadOnly
{
    internal class ReadOnlyStringCompositeBuffer : ReadOnlyCompositeBufferBase<string>
    {
        readonly StringPool _stringPool;

        public ReadOnlyStringCompositeBuffer(Stream stream) : base(stream)
        {
            _stringPool = new();
        }

        protected override ReadOnlyMemory<string> Get(ReadOnlyMemory<byte> byteData)
        {
            var span = byteData.Span;
            var numStrings = BinaryPrimitives.ReadUInt32LittleEndian(span[4..]);
            var ret = new string[numStrings];
            var index = 0;
            StringCompositeBuffer.Decode(span[StringCompositeBuffer.HeaderSize..], charSpan => ret[index++] = _stringPool.GetOrAdd(charSpan));
            return ret;
        }
    }
}
