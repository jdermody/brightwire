using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using BrightData.Helper;

namespace BrightData.Buffers
{
    public class HybridObjectBuffer<T> : HybridBufferBase<T>
        where T: ICanWriteToBinaryWriter, ICanInitializeFromBinaryReader
    {
        public HybridObjectBuffer(IBrightDataContext context, uint index, TempStreamManager tempStreams, int bufferSize = 32768) 
            : base(context, index, tempStreams, bufferSize)
        {
        }

        protected override void _Write(ConcurrentBag<T> items, Stream stream)
        {
            using var writer = new BinaryWriter(stream, Encoding.UTF8, true);
            foreach (var item in items)
                item.WriteTo(writer);
        }

        protected override IEnumerable<T> _Read(Stream stream)
        {
            using var reader = new BinaryReader(stream, Encoding.UTF8, true);
            while (stream.Position < stream.Length) {
                var obj = (T)FormatterServices.GetUninitializedObject(typeof(T));
                obj.Initialize(_context, reader);
                yield return obj;
            }
        }
    }
}
