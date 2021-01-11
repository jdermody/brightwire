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
            _StructTests(i => (int) i);
        }

        [Fact]
        public void FloatBuffer()
        {
            _StructTests(i => (float)i);
        }

        [Fact]
        public void IndexListBuffer()
        {
            using var context = new BrightDataContext();
            // ReSharper disable once AccessToDisposedClosure
            _ObjectTests(context, i => IndexList.Create(context, i));
        }

        [Fact]
        public void WeightedIndexListBuffer()
        {
            using var context = new BrightDataContext();
            // ReSharper disable once AccessToDisposedClosure
            _ObjectTests(context, i => WeightedIndexList.Create(context, (i, 1f)));
        }

        void _ObjectTests<T>(IBrightDataContext context, Func<uint, T> indexTranslator) where T : ISerializable
        {
            _ObjectBufferReadWriteTest(context, 32768, 1024, 256, 4, indexTranslator);
            _ObjectBufferReadWriteTest(context, 32768, 32768, 1024, 1024, indexTranslator);
            _ObjectBufferReadWriteTest(context, 32768, 128, 32768, 32768, indexTranslator);
        }

        void _StructTests<T>(Func<uint, T> indexTranslator) where T: struct
        {
            using var context = new BrightDataContext();
            _StructBufferReadWriteTest(context, 32768, 1024, 256, 4, indexTranslator);
            _StructBufferReadWriteTest(context, 32768, 32768, 1024, 1024, indexTranslator);
            _StructBufferReadWriteTest(context, 32768, 128, 32768, 32768, indexTranslator);
        }

        void _ObjectBufferReadWriteTest<T>(IBrightDataContext context, uint numItems, uint bufferSize, uint inMemoryReadSize, ushort numDistinct, Func<uint, T> indexTranslator) where T : ISerializable
        {
            var buffer = _streamProvider.CreateHybridObjectBuffer<T>(context, bufferSize, numDistinct);
            for (uint i = 0; i < numItems; i++)
                buffer.Add(indexTranslator(i));

            using var stream = new MemoryStream();
            buffer.CopyTo(stream);
            stream.Seek(0, SeekOrigin.Begin);

            uint index = 0;
            var reader = context.GetReader<T>(stream, inMemoryReadSize);
            foreach (var item in reader.EnumerateTyped())
                item.Should().Be(indexTranslator(index++));
        }

        void _StructBufferReadWriteTest<T>(IBrightDataContext context, uint numItems, uint bufferSize, uint inMemoryReadSize, ushort numDistinct, Func<uint, T> indexTranslator) where T : struct
        {
            var buffer = _streamProvider.CreateHybridStructBuffer<T>(bufferSize, numDistinct);
            for (uint i = 0; i < numItems; i++)
                buffer.Add(indexTranslator(i));

            using var stream = new MemoryStream();
            buffer.CopyTo(stream);
            stream.Seek(0, SeekOrigin.Begin);

            uint index = 0;
            var reader = context.GetReader<T>(stream, inMemoryReadSize);
            foreach (var item in reader.EnumerateTyped())
                item.Should().Be(indexTranslator(index++));
        }
    }
}
