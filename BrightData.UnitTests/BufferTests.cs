using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;
using BrightData.Buffers;
using FluentAssertions;

namespace BrightData.UnitTests
{
    public class BufferTests
    {
        public class InMemoryStreamProvider : IProvideTempStreams
        {
            readonly Dictionary<string, MemoryStream> _streams = new Dictionary<string, MemoryStream>();

            public void Dispose()
            {
                foreach(var item in _streams)
                    item.Value.Dispose();
                _streams.Clear();
            }

            public Stream Get(string uniqueId)
            {
                if(!_streams.TryGetValue(uniqueId, out var ret))
                    _streams.Add(uniqueId, ret = new MemoryStream());
                return ret;
            }

            public bool HasStream(string uniqueId) => _streams.ContainsKey(uniqueId);
        }
        readonly InMemoryStreamProvider _streamProvider = new InMemoryStreamProvider();

        [Fact]
        public void IntBuffer()
        {
            _StructBufferReadWriteTest(32768, 1024, 256, 4, i => (int) i);
            _StructBufferReadWriteTest(32768, 32768, 1024, 1024, i => (int)i);
            _StructBufferReadWriteTest(32768, 128, 32768, 32768, i => (int)i);
        }

        void _StructBufferReadWriteTest<T>(uint numItems, uint bufferSize, uint inMemoryReadSize, ushort numDistinct, Func<uint, T> indexTranslator) where T : struct
        {
            var buffer = _streamProvider.CreateHybridStructBuffer<T>(bufferSize, numDistinct);
            for (uint i = 0; i < numItems; i++)
                buffer.Add(indexTranslator(i));

            using var stream = new MemoryStream();
            using var context = new BrightDataContext();
            buffer.CopyTo(stream);
            stream.Seek(0, SeekOrigin.Begin);

            uint index = 0;
            var reader = context.GetReader<T>(stream, inMemoryReadSize);
            foreach (var item in reader.EnumerateTyped())
            {
                item.Should().Be(indexTranslator(index++));
            }
        }
    }
}
