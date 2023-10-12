using System;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using BrightData.Buffer.ReadOnly;
using FluentAssertions;
using CommunityToolkit.HighPerformance;
using Xunit;

namespace BrightData.UnitTests
{
    public class BufferTests2
    {
        //const int Size = 1024;
        //const int UintBufferSize = Size * sizeof(uint);
        //static readonly uint[] TestData = Size.AsRange().Select(i => i).ToArray();

        //static ReadOnlyFileBasedBuffer GetIntegersFileBuffer()
        //{
        //    var buffer = MemoryMappedFile.CreateNew(null, UintBufferSize);
        //    using var stream = buffer.CreateViewStream(0, UintBufferSize);
        //    stream.Write(MemoryMarshal.Cast<uint, byte>(TestData.AsSpan()));
        //    return new ReadOnlyFileBasedBuffer(buffer);
        //}

        //static ReadOnlyMemory<uint> GetIntegersMemory() => TestData;

        //[Fact]
        //public void FileBasedIterableBuffer()
        //{
        //    using var buffer = GetIntegersFileBuffer();
        //    using var iterator = buffer.GetIterator<uint>(0, UintBufferSize);
        //    var fromBuffer = iterator.Enumerate().ToArray();
        //    fromBuffer.Should().BeEquivalentTo(TestData);
        //}

        //[Fact]
        //public void FileBasedIterableReferencesBuffer()
        //{
        //    using var buffer = GetIntegersFileBuffer();
        //    using var iterator = buffer.GetIterator<uint>(0, UintBufferSize);
        //    var fromBuffer = new uint[Size];
        //    uint index = 0;

        //    foreach(ref readonly var item in iterator)
        //        fromBuffer[index++] = item;
        //    fromBuffer.Should().BeEquivalentTo(TestData);
        //}

        //[Fact]
        //public void MemoryBasedIterableBuffer()
        //{
        //    using var buffer = new ReadOnlyMemoryBasedBuffer(new ReadOnlyMemory<uint>(TestData).Cast<uint, byte>());
        //    using var iterator = buffer.GetIterator<uint>(0, UintBufferSize);
        //    var fromBuffer = iterator.Enumerate().ToArray();
        //    fromBuffer.Should().BeEquivalentTo(TestData);
        //}

        //[Fact]
        //public void MemoryBasedIterableReferencesBuffer()
        //{
        //    using var buffer = new ReadOnlyMemoryBasedBuffer(new ReadOnlyMemory<uint>(TestData).Cast<uint, byte>());
        //    using var iterator = buffer.GetIterator<uint>(0, UintBufferSize);
        //    var fromBuffer = new uint[Size];
        //    uint index = 0;

        //    foreach(ref readonly var item in iterator)
        //        fromBuffer[index++] = item;
        //    fromBuffer.Should().BeEquivalentTo(TestData);
        //}

        //[Fact]
        //public void FileBasedRandomAccess()
        //{
        //    using var buffer = GetIntegersFileBuffer();
        //    using var block = buffer.GetBlock<uint>(0, UintBufferSize);

        //    for (var i = 0; i < Size; i++) {
        //        block.Get(i, out var val);
        //        val.Should().Be(TestData[i]);
        //    }
        //}

        //[Fact]
        //public void FileBaseRandomAccessRange()
        //{
        //    using var buffer = GetIntegersFileBuffer();
        //    using var block = buffer.GetBlock<uint>(0, UintBufferSize);

        //    var fromBuffer = block.GetSpan(0, Size).ToArray();
        //    fromBuffer.Should().BeEquivalentTo(TestData);
        //}

        //[Fact]
        //public void MemoryBasedRandomAccess()
        //{
        //    using var buffer = new ReadOnlyMemoryBasedBuffer(new ReadOnlyMemory<uint>(TestData).Cast<uint, byte>());
        //    using var block = buffer.GetBlock<uint>(0, UintBufferSize);

        //    for (var i = 0; i < Size; i++) {
        //        block.Get(i, out var val);
        //        val.Should().Be(TestData[i]);
        //    }
        //}

        //[Fact]
        //public void MemoryBaseRandomAccessRange()
        //{
        //    using var buffer = new ReadOnlyMemoryBasedBuffer(new ReadOnlyMemory<uint>(TestData).Cast<uint, byte>());
        //    using var block = buffer.GetBlock<uint>(0, UintBufferSize);

        //    var fromBuffer = block.GetSpan(0, Size).ToArray();
        //    fromBuffer.Should().BeEquivalentTo(TestData);
        //}
    }
}
