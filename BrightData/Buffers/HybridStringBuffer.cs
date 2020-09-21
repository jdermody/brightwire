using System.Collections.Generic;
using System.IO;
using System.Text;
using BrightData.Helper;

namespace BrightData.Buffers
{
    public class HybridStringBuffer : HybridBufferBase<string>
    {
        public HybridStringBuffer(IBrightDataContext context, TempStreamManager tempStreams, uint bufferSize = 32768) 
            : base(context, tempStreams, bufferSize)
        {
        }

        public override void Write(IReadOnlyCollection<string> items, BinaryWriter writer)
        {
            foreach (var item in items)
                writer.Write(item);
        }

        protected override IEnumerable<string> _Read(Stream stream)
        {
            using var reader = new BinaryReader(stream, Encoding.UTF8, true);
            while (stream.Position < stream.Length)
                yield return reader.ReadString();
        }
    }
}
