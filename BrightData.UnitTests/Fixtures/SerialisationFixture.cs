﻿using System;
using System.IO;
using System.Text;
using BrightData.Helper;

namespace BrightData.UnitTests.Fixtures
{
    public class SerialisationFixture : IDisposable
    {
        public BrightDataContext Context { get; } = new(null, 0);
        public MemoryStream Stream { get; } = new();
        public BinaryWriter Writer { get; }
        public DataEncoder Encoder { get; }

        public SerialisationFixture()
        {
            Writer = new BinaryWriter(Stream);
            Encoder = Context.GetDataEncoder();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Context.Dispose();
            Stream.Dispose();
        }

        public BinaryReader ReadFromStart()
        {
            Stream.Seek(0, SeekOrigin.Begin);
            return new BinaryReader(Stream, Encoding.UTF8);
        }
    }
}
