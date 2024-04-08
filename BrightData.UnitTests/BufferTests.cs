using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BrightData.Types;
using BrightData.UnitTests.Helper;
using FluentAssertions;
using Xunit;

namespace BrightData.UnitTests
{
    public class BufferTests : UnitTestBase
    {
        public class TempData(Guid id, MemoryStream data) : IByteBlockSource
        {
            public void Dispose()
            {
                GC.SuppressFinalize(this);
                data.Dispose();
            }

            public uint Size => (uint)data.Length;
            public Guid Id { get; } = id;

            public void Write(ReadOnlySpan<byte> data1, uint offset)
            {
                data.Seek(offset, SeekOrigin.Begin);
                data.Write(data1);
            }

            public ValueTask WriteAsync(ReadOnlyMemory<byte> data1, uint offset)
            {
                data.Seek(offset, SeekOrigin.Begin);
                return data.WriteAsync(data1);
            }

            public uint Read(Span<byte> data1, uint offset)
            {
                data.Seek(offset, SeekOrigin.Begin);
                return (uint)data.Read(data1);
            }

            public async Task<uint> ReadAsync(Memory<byte> data1, uint offset)
            {
                data.Seek(offset, SeekOrigin.Begin);
                var ret = await data.ReadAsync(data1);
                return (uint)ret;
            }
        }
        public class InMemoryStreamProvider : IProvideDataBlocks
        {
            readonly Dictionary<Guid, TempData> _data = [];

            public void Dispose()
            {
                GC.SuppressFinalize(this);
                Clear();
            }

            public IByteBlockSource Get(Guid id)
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

        record TestClass : IHaveDataAsReadOnlyByteSpan
        {
            readonly byte[] _data;

            public TestClass(ReadOnlySpan<byte> data)
            {
                _data = data.ToArray();
            }

            public ReadOnlySpan<byte> DataAsBytes => _data;
            public override string ToString()
            {
                return String.Join(", ", _data);
            }
        }

        struct TestStruct
        {
            public int Property { get; set; }
        }

        [Fact]
        public void VectorBuffer()
        {
            var data = new[] {
                new TestClass(new byte[] { 1, 2, 3 }),
                new TestClass(new byte[] { 4, 5, 6 }),
                new TestClass(new byte[] { 7, 8, 9 })
            };
            var vectorBuffer = _streamProvider.CreateCompositeBuffer<TestClass>(x => new(x), 2, 0);
            vectorBuffer.Append(data);
            var index = 0;
            vectorBuffer.ForEachBlock(x => {
                foreach (var item in x) {
                    item.Should().BeEquivalentTo(data[index++]);
                }
            });
        }

        [Fact]
        public async Task StringBuffer()
        {
            var data = new[] {
                "this is a test",
                "this is another test",
                "this is a final test",
                "this is a test",
            };
            var stringBuffer = _streamProvider.CreateCompositeBuffer(2, 0, 128);
            stringBuffer.Append(data);
            var index = 0;
            for (uint i = 0, len = (uint)stringBuffer.BlockSizes.Length; i < len; i++) {
                var block = await stringBuffer.GetTypedBlock(i);
                foreach (var item in block.ToArray())
                    item.Should().Be(data[index++]);
            }

            index = 0;
            await foreach (var item in stringBuffer)
                item.Should().Be(data[index++]);
         
            var (table, encoded) = stringBuffer.Encode();
            encoded.Size.Should().Be(4);
            table.Length.Should().Be(3);
        }

        [Fact]
        public async Task IntBuffer()
        {
            var intBuffer = _streamProvider.CreateCompositeBuffer<int>(2, 0);
            intBuffer.Append(1);
            intBuffer.Append(new ReadOnlySpan<int>([2, 3]));
            var index = 0;
            await intBuffer.ForEachBlock(block => {
                foreach (var num in block)
                    num.Should().Be(++index);
            });
            var test = await intBuffer.ToNumeric(_streamProvider, 256);
            test.DataType.Should().Be(typeof(sbyte));

            index = 0;
            await foreach (var item in intBuffer)
                item.Should().Be(++index);
            index = 0;
            for (uint i = 0, len = (uint)intBuffer.BlockSizes.Length; i < len; i++) {
                var block = await intBuffer.GetTypedBlock(i);
                foreach(var item in block.ToArray())
                    item.Should().Be(++index);
            }
            var table = await _context.CreateTableInMemory(null, intBuffer);
            var column = await table.GetColumn<int>(0).AsReadOnlySequence();
            index = 0;
            foreach (var block in column) {
                foreach (var item in block.ToArray()) {
                    item.Should().Be(++index);
                }
            }
        }

        /// <summary>
        /// Buffer size configurations to test
        /// </summary>
        public static readonly (int numItems, int bufferSize, int inMemoryReadSize, int numDistinct)[] Configurations = [
            (32768, 1024, 256, 4),
            (32768, 32768, 1024, 1024),
            (32768, 128, 32768, 32768)
        ];

        [Fact]
        public Task IntBuffer2()
        {
            return StructTests(i => (int)i);
        }

        [Fact]
        public Task FloatBuffer2()
        {
            return StructTests(i => (float)i);
        }

        [Fact]
        public Task IndexListBuffer2()
        {
            return ObjectTests(i => IndexList.Create(i), x => new(x));
        }

        [Fact]
        public Task WeightedIndexListBuffer2()
        {
            return ObjectTests(i => WeightedIndexList.Create((i, 1f)), x => new(x));
        }

        [Fact]
        public async Task StringBuffer2()
        {
            foreach (var (numItems, bufferSize, inMemoryReadSize, numDistinct) in Configurations)
                await StringBufferReadWriteTest((uint)numItems, bufferSize, (uint)inMemoryReadSize, (ushort)numDistinct, i => i.ToString());
        }

        async Task ObjectTests<T>(Func<uint, T> indexTranslator, CreateFromReadOnlyByteSpan<T> createItem) where T : IHaveDataAsReadOnlyByteSpan
        {
            foreach (var (numItems, bufferSize, inMemoryReadSize, numDistinct) in Configurations)
                await ObjectBufferReadWriteTest((uint)numItems, bufferSize, (uint)inMemoryReadSize, (ushort)numDistinct, indexTranslator, createItem);
        }

        async Task StructTests<T>(Func<uint, T> indexTranslator) where T : unmanaged
        {
            foreach (var (numItems, bufferSize, inMemoryReadSize, numDistinct) in Configurations)
                await StructBufferReadWriteTest((uint)numItems, bufferSize, (uint)inMemoryReadSize, (ushort)numDistinct, indexTranslator);
        }

        async Task StringBufferReadWriteTest(uint numItems, int bufferSize, uint inMemorySize, ushort numDistinct, Func<uint, string> indexTranslator)
        {
            var buffer = _streamProvider.CreateCompositeBuffer(bufferSize, inMemorySize, numDistinct);
            for (uint i = 0; i < numItems; i++)
                buffer.Append(indexTranslator(i));

            using var stream = new MemoryStream();
            buffer.WriteTo(stream).Wait();
            stream.Seek(0, SeekOrigin.Begin);

            uint index = 0;
            var bufferReader = stream.GetReadOnlyStringCompositeBuffer();
            await foreach (var item in bufferReader)
                item.Should().Be(indexTranslator(index++));
        }

        async Task ObjectBufferReadWriteTest<T>(uint numItems, int bufferSize, uint inMemorySize, ushort numDistinct, Func<uint, T> indexTranslator, CreateFromReadOnlyByteSpan<T> createItem) 
            where T : IHaveDataAsReadOnlyByteSpan
        {
            var buffer = _streamProvider.CreateCompositeBuffer(createItem, bufferSize, inMemorySize, numDistinct);
            for (uint i = 0; i < numItems; i++)
                buffer.Append(indexTranslator(i));

            using var stream = new MemoryStream();
            buffer.WriteTo(stream).Wait();
            stream.Seek(0, SeekOrigin.Begin);

            uint index = 0;
            var bufferReader = stream.GetReadOnlyCompositeBuffer(createItem);
            await foreach (var item in bufferReader) {
                var comparison = indexTranslator(index++);
                item.Should().BeEquivalentTo(comparison);
            }
        }

        async Task StructBufferReadWriteTest<T>(uint numItems, int bufferSize, uint inMemorySize, ushort numDistinct, Func<uint, T> indexTranslator) where T : unmanaged
        {
            var buffer = _streamProvider.CreateCompositeBuffer<T>(bufferSize, inMemorySize, numDistinct);
            for (uint i = 0; i < numItems; i++)
                buffer.Append(indexTranslator(i));

            using var stream = new MemoryStream();
            buffer.WriteTo(stream).Wait();
            stream.Seek(0, SeekOrigin.Begin);

            uint index = 0;
            var bufferReader = stream.GetReadOnlyCompositeBuffer<T>();
            await foreach (var item in bufferReader)
                item.Should().Be(indexTranslator(index++));
        }
    }
}
