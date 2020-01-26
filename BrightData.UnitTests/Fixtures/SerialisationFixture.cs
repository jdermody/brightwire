using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BrightData.Helper;

namespace BrightData.UnitTests.Fixtures
{
    public class SerialisationFixture : IDisposable
    {
        public BrightDataContext Context { get; } = new BrightDataContext(0);
        public MemoryStream Stream { get; } = new MemoryStream();
        public BinaryWriter Writer { get; }
        public DataEncoder Encoder { get; }

        public SerialisationFixture()
        {
            Writer = new BinaryWriter(Stream);
            Encoder = new DataEncoder(Context);
        }

        public void Dispose()
        {
            Context.Dispose();
            Stream.Dispose();
        }

        public BinaryReader ReadFromStart()
        {
            Stream.Seek(0, SeekOrigin.Begin);
            return new BinaryReader(Stream);
        }
    }
}
