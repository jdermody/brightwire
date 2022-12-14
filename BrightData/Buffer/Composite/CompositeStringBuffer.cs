using System;
using System.IO;
using System.Text;

namespace BrightData.Buffer.Composite
{
    internal class CompositeStringBuffer : CompositeBufferBase<string>
    {
        public CompositeStringBuffer(IProvideTempStreams tempStream, uint maxCount, ushort maxDistinct)
            : base(tempStream, maxCount, maxDistinct)
        {
        }

        protected override void WriteTo(ReadOnlySpan<string> ptr, Stream stream)
        {
            using var writer = new BinaryWriter(stream, Encoding.UTF8, true);
            foreach (var item in ptr)
                writer.Write(item);
        }

        protected override uint ReadTo(Stream stream, uint count, string[] buffer)
        {
            uint ret = 0;
            var reader = new BinaryReader(stream, Encoding.UTF8);
            for (; ret < count && stream.Position < stream.Length; ret++)
                buffer[ret] = reader.ReadString();
            return ret;
        }
    }
}
