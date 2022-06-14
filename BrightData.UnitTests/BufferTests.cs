using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;
using BrightData.Buffer;
using FluentAssertions;

namespace BrightData.UnitTests
{
    public class BufferTests
    {
        public class InMemoryStreamProvider : IProvideTempStreams
        {
            readonly Dictionary<string, MemoryStream> _streams = new();

            public void Dispose()
            {
                foreach(var item in _streams)
                    item.Value.Dispose();
                _streams.Clear();
            }

            public string GetNewTempPath()
            {
                throw new NotImplementedException();
            }

            public Stream Get(string uniqueId)
            {
                if(!_streams.TryGetValue(uniqueId, out var ret))
                    _streams.Add(uniqueId, ret = new MemoryStream());
                return ret;
            }

            public bool HasStream(string uniqueId) => _streams.ContainsKey(uniqueId);
        }
        readonly InMemoryStreamProvider _streamProvider = new();

        /// <summary>
        /// Buffer size configurations to test
        /// </summary>
        public static readonly (int numItems, int bufferSize, int inMemoryReadSize, int numDistinct)[] Configurations = new []
        {
            (32768, 1024, 256, 4),
            (32768, 32768, 1024, 1024),
            (32768, 128, 32768, 32768)
        };

        [Fact]
        public void IntBuffer()
        {
            StructTests(i => (int) i);
        }

        [Fact]
        public void FloatBuffer()
        {
            StructTests(i => (float)i);
        }

        [Fact]
        public void IndexListBuffer()
        {
            using var context = new BrightDataContext();
            // ReSharper disable once AccessToDisposedClosure
            ObjectTests(context, i => context.CreateIndexList(i));
        }

        [Fact]
        public void WeightedIndexListBuffer()
        {
            using var context = new BrightDataContext();
            // ReSharper disable once AccessToDisposedClosure
            ObjectTests(context, i => context.CreateWeightedIndexList((i, 1f)));
        }

        [Fact]
        public void StringBuffer()
        {
            using var context = new BrightDataContext();
            foreach(var (numItems, bufferSize, inMemoryReadSize, numDistinct) in Configurations)
                StringBufferReadWriteTest(context, (uint)numItems, (uint)bufferSize, (uint)inMemoryReadSize, (ushort)numDistinct, i => i.ToString());
        }

        void ObjectTests<T>(IBrightDataContext context, Func<uint, T> indexTranslator) where T : ISerializable
        {
            foreach (var (numItems, bufferSize, inMemoryReadSize, numDistinct) in Configurations)
                ObjectBufferReadWriteTest(context, (uint)numItems, (uint)bufferSize, (uint)inMemoryReadSize, (ushort)numDistinct, indexTranslator);
        }

        void StructTests<T>(Func<uint, T> indexTranslator) where T: struct
        {
            using var context = new BrightDataContext();
            foreach (var (numItems, bufferSize, inMemoryReadSize, numDistinct) in Configurations)
                StructBufferReadWriteTest(context, (uint)numItems, (uint)bufferSize, (uint)inMemoryReadSize, (ushort)numDistinct, indexTranslator);
        }

        void StringBufferReadWriteTest(IBrightDataContext context, uint numItems, uint bufferSize, uint inMemoryReadSize, ushort numDistinct, Func<uint, string> indexTranslator)
        {
            var buffer = _streamProvider.CreateHybridStringBuffer(bufferSize, numDistinct);
            for (uint i = 0; i < numItems; i++)
                buffer.Add(indexTranslator(i));

            using var stream = new MemoryStream();
            buffer.CopyTo(stream);
            stream.Seek(0, SeekOrigin.Begin);
            using var reader = new BinaryReader(stream, Encoding.UTF8, true);

            uint index = 0;
            var bufferReader = context.GetBufferReader<string>(reader, inMemoryReadSize);
            foreach (var item in bufferReader.EnumerateTyped())
                item.Should().Be(indexTranslator(index++));
        }

        void ObjectBufferReadWriteTest<T>(IBrightDataContext context, uint numItems, uint bufferSize, uint inMemoryReadSize, ushort _, Func<uint, T> indexTranslator) where T : ISerializable
        {
            var buffer = _streamProvider.CreateHybridObjectBuffer<T>(context, bufferSize);
            for (uint i = 0; i < numItems; i++)
                buffer.Add(indexTranslator(i));

            using var stream = new MemoryStream();
            buffer.CopyTo(stream);
            stream.Seek(0, SeekOrigin.Begin);
            using var reader = new BinaryReader(stream, Encoding.UTF8, true);

            uint index = 0;
            var bufferReader = context.GetBufferReader<T>(reader, inMemoryReadSize);
            foreach (var item in bufferReader.EnumerateTyped())
                item.Should().Be(indexTranslator(index++));
        }

        void StructBufferReadWriteTest<T>(IBrightDataContext context, uint numItems, uint bufferSize, uint inMemoryReadSize, ushort numDistinct, Func<uint, T> indexTranslator) where T : struct
        {
            var buffer = _streamProvider.CreateHybridStructBuffer<T>(bufferSize, numDistinct);
            for (uint i = 0; i < numItems; i++)
                buffer.Add(indexTranslator(i));

            using var stream = new MemoryStream();
            buffer.CopyTo(stream);
            stream.Seek(0, SeekOrigin.Begin);
            using var reader = new BinaryReader(stream, Encoding.UTF8, true);

            uint index = 0;
            var bufferReader = context.GetBufferReader<T>(reader, inMemoryReadSize);
            foreach (var item in bufferReader.EnumerateTyped())
                item.Should().Be(indexTranslator(index++));
        }
    }
}
