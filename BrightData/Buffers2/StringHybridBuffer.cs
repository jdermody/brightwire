using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BrightData.Buffers2
{
    public class StringHybridBuffer : HybridBufferBase<string>
    {
        public StringHybridBuffer(IProvideTempStreams tempStream, uint maxCount, uint maxDistinct = 1024) 
            : base(tempStream, maxCount, maxDistinct)
        {
        }

        protected override void _WriteTo(ReadOnlySpan<string> ptr, Stream stream)
        {
            using var writer = new BinaryWriter(stream, Encoding.UTF8, true);
            foreach(var item in ptr)
                writer.Write(item);
        }

        protected override uint _ReadTo(Stream stream, uint count, string[] buffer)
        {
            uint ret = 0;
            var reader = new BinaryReader(stream);
            for (; ret < count && stream.Position < stream.Length; ret++)
                buffer[ret] = reader.ReadString();
            return ret;
        }
    }
}
