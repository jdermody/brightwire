using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using BrightData.Helper;

namespace BrightData.Buffers
{
    public class ObjectHybridBuffer<T> : HybridBufferBase<T>
        where T : ISerializable
    {
        private readonly IBrightDataContext _context;

        public ObjectHybridBuffer(IBrightDataContext context, TempStreamManager tempStream, uint maxCount) : base(tempStream, maxCount, null)
        {
            _context = context;
        }

        protected override void _WriteTo(ReadOnlySpan<T> ptr, Stream stream)
        {
            using var writer = new BinaryWriter(stream, Encoding.UTF8, true);
            foreach(var item in ptr)
                item.WriteTo(writer);
        }

        protected override uint _ReadTo(Stream stream, uint count, T[] buffer)
        {
            var type = typeof(T);
            var reader = new BinaryReader(stream, Encoding.UTF8);
            uint ret = 0;
            for (; ret < count && stream.Position < stream.Length; ret++) {
                var obj = (T)FormatterServices.GetUninitializedObject(type);
                obj.Initialize(_context, reader);
                buffer[ret] = obj;
            }

            return ret;
        }
    }
}
