using System;
using System.IO;
using System.Text;
using BrightData.Serialisation;

namespace BrightData.Buffer.Composite
{
    internal class CompositeObjectBuffer<T> : CompositeBufferBase<T>
        where T : IAmSerializable
    {
        readonly BrightDataContext _context;

        public CompositeObjectBuffer(BrightDataContext context, IProvideTempStreams tempStream, uint maxCount) : base(tempStream, maxCount, null)
        {
            _context = context;
        }

        protected override void WriteTo(ReadOnlySpan<T> ptr, Stream stream)
        {
            using var writer = new BinaryWriter(stream, Encoding.UTF8, true);
            foreach (var item in ptr)
                item.WriteTo(writer);
        }

        protected override uint ReadTo(Stream stream, uint count, T[] buffer)
        {
            var reader = new BinaryReader(stream, Encoding.UTF8);
            uint ret = 0;
            for (; ret < count && stream.Position < stream.Length; ret++) {
                var obj = _context.Create<T>(reader);
                buffer[ret] = obj;
            }

            return ret;
        }
    }
}
