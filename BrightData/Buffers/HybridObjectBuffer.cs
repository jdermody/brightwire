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
        public HybridObjectBuffer(IBrightDataContext context, TempStreamManager tempStreams, uint bufferSize = 32768) 
            : base(context, tempStreams, bufferSize)
        {
        }

        public override void Write(IReadOnlyCollection<T> items, BinaryWriter writer)
        {
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
