using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using BrightData.UnitTests.Helper;
using FluentAssertions;
using Xunit;

namespace BrightData.UnitTests
{
    public class BufferTests : UnitTestBase
    {
        public class TempData : IDataBlock
        {
            readonly MemoryStream _data;

            public TempData(Guid id, MemoryStream data)
            {
                Id = id;
                _data = data;
            }
            
            public void Dispose()
            {
                _data.Dispose();
            }

            public uint Size => (uint)_data.Length;
            public Guid Id { get; }
            public void Write(ReadOnlySpan<byte> data, uint offset)
            {
                _data.Seek(offset, SeekOrigin.Begin);
                _data.Write(data);
            }

            public ValueTask WriteAsync(ReadOnlyMemory<byte> data, uint offset)
            {
                _data.Seek(offset, SeekOrigin.Begin);
                return _data.WriteAsync(data);
            }

            public uint Read(Span<byte> data, uint offset)
            {
                _data.Seek(offset, SeekOrigin.Begin);
                return (uint)_data.Read(data);
            }

            public async Task<uint> ReadAsync(Memory<byte> data, uint offset)
            {
                _data.Seek(offset, SeekOrigin.Begin);
                var ret = await _data.ReadAsync(data);
                return (uint)ret;
            }
        }
        public class InMemoryStreamProvider : IProvideDataBlocks
        {
            readonly Dictionary<Guid, TempData> _data = new();

            public void Dispose()
            {
                GC.SuppressFinalize(this);
                Clear();
            }

            public IDataBlock Get(Guid id)
            {
                if (!_data.TryGetValue(id, out var ret))
                    _data.Add(id, ret = new(id, new MemoryStream()));
                return ret;
            }

            public void Clear()
            {
                foreach (var item in _data)
                    item.Value.Dispose();
                _data.Clear();
            }
        }
        readonly InMemoryStreamProvider _streamProvider = new();

        /// <summary>
        /// Buffer size configurations to test
        /// </summary>
        public static readonly (int numItems, int bufferSize, int inMemoryReadSize, int numDistinct)[] Configurations = new[]
        {
            (32768, 1024, 256, 4),
            (32768, 32768, 1024, 1024),
            (32768, 128, 32768, 32768)
        };

        [Fact]
        public void IntBuffer()
        {
            StructTests(i => (int)i);
        }

        [Fact]
        public void FloatBuffer()
        {
            StructTests(i => (float)i);
        }

        [Fact]
        public void IndexListBuffer()
        {
            ObjectTests(i => IndexList.Create(i), x => new(x));
        }

        [Fact]
        public void WeightedIndexListBuffer()
        {
            ObjectTests(i => WeightedIndexList.Create((i, 1f)), x => new(x));
        }

        [Fact]
        public void StringBuffer()
        {
            foreach (var (numItems, bufferSize, inMemoryReadSize, numDistinct) in Configurations)
                StringBufferReadWriteTest((uint)numItems, bufferSize, (uint)inMemoryReadSize, (ushort)numDistinct, i => i.ToString());
        }

        void ObjectTests<T>(Func<uint, T> indexTranslator, CreateFromReadOnlyByteSpan<T> createItem) where T : IHaveDataAsReadOnlyByteSpan
        {
            foreach (var (numItems, bufferSize, inMemoryReadSize, numDistinct) in Configurations)
                ObjectBufferReadWriteTest((uint)numItems, bufferSize, (uint)inMemoryReadSize, (ushort)numDistinct, indexTranslator, createItem);
        }

        void StructTests<T>(Func<uint, T> indexTranslator) where T : unmanaged
        {
            foreach (var (numItems, bufferSize, inMemoryReadSize, numDistinct) in Configurations)
                StructBufferReadWriteTest((uint)numItems, bufferSize, (uint)inMemoryReadSize, (ushort)numDistinct, indexTranslator);
        }

        void StringBufferReadWriteTest(uint numItems, int bufferSize, uint inMemorySize, ushort numDistinct, Func<uint, string> indexTranslator)
        {
            var buffer = _streamProvider.CreateCompositeBuffer(bufferSize, inMemorySize, numDistinct);
            for (uint i = 0; i < numItems; i++)
                buffer.Add(indexTranslator(i));

            using var stream = new MemoryStream();
            buffer.WriteTo(stream).Wait();
            stream.Seek(0, SeekOrigin.Begin);

            uint index = 0;
            var bufferReader = stream.GetReadOnlyStringCompositeBuffer();
            foreach (var item in bufferReader)
                item.Should().Be(indexTranslator(index++));
        }

        void ObjectBufferReadWriteTest<T>(uint numItems, int bufferSize, uint inMemoryReadSize, ushort _, Func<uint, T> indexTranslator, CreateFromReadOnlyByteSpan<T> createItem) 
            where T : IHaveDataAsReadOnlyByteSpan
        {
            var buffer = _streamProvider.CreateCompositeBuffer(createItem, bufferSize);
            for (uint i = 0; i < numItems; i++)
                buffer.Add(indexTranslator(i));

            using var stream = new MemoryStream();
            buffer.WriteTo(stream).Wait();
            stream.Seek(0, SeekOrigin.Begin);

            uint index = 0;
            var bufferReader = stream.GetReadOnlyCompositeBuffer(createItem);
            foreach (var item in bufferReader) {
                var comparison = indexTranslator(index++);
                item.Should().BeEquivalentTo(comparison, options => options.ComparingByMembers<T>());
            }
        }

        void StructBufferReadWriteTest<T>(uint numItems, int bufferSize, uint inMemoryReadSize, ushort numDistinct, Func<uint, T> indexTranslator) where T : unmanaged
        {
            var buffer = _streamProvider.CreateCompositeBuffer<T>(bufferSize, numDistinct);
            for (uint i = 0; i < numItems; i++)
                buffer.Add(indexTranslator(i));

            using var stream = new MemoryStream();
            buffer.WriteTo(stream).Wait();
            stream.Seek(0, SeekOrigin.Begin);

            uint index = 0;
            var bufferReader = stream.GetReadOnlyCompositeBuffer<T>();
            foreach (var item in bufferReader)
                item.Should().Be(indexTranslator(index++));
        }
    }
}
